using Splitio.Services.Common;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource
{
    public class EventSourceClient : IEventSourceClient
    {
        private const string KeepAliveResponse = ":keepalive\n\n";
        private const int ReadTimeoutMs = 70000;
        private const int ConnectTimeoutMs = 30000;
        private const int BufferSize = 10000;

        private readonly ISplitLogger _log;
        private readonly INotificationParser _notificationParser;
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly CountdownEvent _disconnectSignal;

        private bool _connected;

        private ISplitioHttpClient _splitHttpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _streamReadcancellationTokenSource;        
        private string _url;

        public EventSourceClient(ISplitLogger log = null,
            INotificationParser notificationParser = null,
            IWrapperAdapter wrapperAdapter = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(EventSourceClient));
            _notificationParser = notificationParser ?? new NotificationParser();
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();

            _disconnectSignal = new CountdownEvent(1);
        }

        public event EventHandler<EventReceivedEventArgs> EventReceived;
        public event EventHandler<SSEActionsEventArgs> ActionEvent;

        #region Public Methods
        public bool ConnectAsync(string url)
        {
            if (IsConnected())
            {
                _log.Debug("Event source Client already connected.");
                return false;
            }

            _url = url;
            _disconnectSignal.Reset();
            var signal = new CountdownEvent(1);

            Task.Factory.StartNew(() => ConnectAsync(signal));

            try
            {
                if (!signal.Wait(ConnectTimeoutMs))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex.Message);
                return false;
            }

            return IsConnected();
        }

        public bool IsConnected()
        {
            return _connected;
        }

        public void Disconnect(SSEClientActions action = SSEClientActions.DISCONNECT)
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _streamReadcancellationTokenSource.Cancel();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _splitHttpClient.Dispose();

            DispatchActionEvent(action);

            _disconnectSignal.Wait(ReadTimeoutMs);
            _log.Debug($"Disconnected from {_url}");
        }
        #endregion

        #region Private Methods
        private async Task ConnectAsync(CountdownEvent signal)
        {
            var action = SSEClientActions.DISCONNECT;

            try
            {
                _splitHttpClient = new SplitioHttpClient(new Dictionary<string, string> { { "Accept", "text/event-stream" } });
                _cancellationTokenSource = new CancellationTokenSource();

                using (var response = await _splitHttpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                {
                    _log.Debug($"Response from {_url}: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                _log.Info($"Connected to {_url}");

                                _connected = true;
                                DispatchActionEvent(SSEClientActions.CONNECTED);
                                signal.Signal();
                                await ReadStreamAsync(stream);
                            }
                        }
                        catch (ReadStreamException ex)
                        {
                            _log.Debug(ex.Message);
                            action = ex.Action;
                        }
                        catch (Exception ex)
                        {
                            _log.Debug($"Error reading stream: {ex.Message}");
                            action = SSEClientActions.RETRYABLE_ERROR;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug($"Error connecting to {_url}: {ex.Message}");
            }
            finally
            {
                _disconnectSignal.Signal();
                Disconnect(action);
                _connected = false;

                _log.Debug("Finished Event Source client ConnectAsync.");
            }            
        }

        private async Task ReadStreamAsync(Stream stream)
        {
            var encoder = new UTF8Encoding();
            _streamReadcancellationTokenSource = new CancellationTokenSource();

            _log.Debug($"Reading stream ....");

            try
            {
                while (!_streamReadcancellationTokenSource.IsCancellationRequested && IsConnected())
                {
                    if (stream.CanRead && IsConnected())
                    {
                        var buffer = new byte[BufferSize];

                        var timeoutTask = _wrapperAdapter.TaskDelay(ReadTimeoutMs);
                        var streamReadTask = stream.ReadAsync(buffer, 0, BufferSize, _streamReadcancellationTokenSource.Token);
                        // Creates a task that will complete when any of the supplied tasks have completed.
                        // Returns: A task that represents the completion of one of the supplied tasks. The return task's Result is the task that completed.
                        var finishedTask = await _wrapperAdapter.WhenAny(streamReadTask, timeoutTask);

                        if (finishedTask == timeoutTask)
                        {
                            throw new ReadStreamException(SSEClientActions.RETRYABLE_ERROR, $"Streaming read time out after {ReadTimeoutMs} seconds.");
                        }
                        
                        int len = streamReadTask.Result;

                        if (len == 0)
                        {
                            throw new ReadStreamException(SSEClientActions.RETRYABLE_ERROR, "Streaming end of the file.");
                        }

                        var notificationString = encoder.GetString(buffer, 0, len);
                        _log.Debug($"Read stream encoder buffer: {notificationString}");

                        if (notificationString != KeepAliveResponse && IsConnected())
                        {
                            var lines = notificationString.Split(new[] { "\n\n" }, StringSplitOptions.None);

                            foreach (var line in lines)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    var eventData = _notificationParser.Parse(line);

                                    if (eventData != null)
                                    {
                                        if (eventData.Type == NotificationType.ERROR)
                                        {
                                            var notificationError = (NotificationError)eventData;

                                            ProcessErrorNotification(notificationError);
                                        }
                                        else
                                        {
                                            DispatchEvent(eventData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (ReadStreamException ex)
            {
                _log.Debug($"ReadStreamException: {ex.Message}");
                throw ex;
            }
            catch (Exception ex)
            {
                _log.Debug($"Stream Token canceled. {ex.Message}");
            }
            finally
            {
                _log.Debug($"Stop read stream");
            }
        }

        private void ProcessErrorNotification(NotificationError notificationError)
        {
            _log.Debug($"Notification error: {notificationError.Message}. Status Server: {notificationError.StatusCode}.");

            if (notificationError.Code >= 40140 && notificationError.Code <= 40149)
            {
                throw new ReadStreamException(SSEClientActions.RETRYABLE_ERROR, $"Ably Notification code: {notificationError.Code}");
            }

            if (notificationError.Code >= 40000 && notificationError.Code <= 49999)
            {
                throw new ReadStreamException(SSEClientActions.NONRETRYABLE_ERROR, $"Ably Notification code: {notificationError.Code}");
            }
        }

        private void DispatchEvent(IncomingNotification incomingNotification)
        {
            _log.Debug($"DispatchEvent: {incomingNotification}");
            EventReceived?.Invoke(this, new EventReceivedEventArgs(incomingNotification));
        }

        private void DispatchActionEvent(SSEClientActions action)
        {
            ActionEvent?.Invoke(this, new SSEActionsEventArgs(action));
        }
        #endregion
    }
}
