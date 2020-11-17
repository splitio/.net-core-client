using Splitio.Services.Common;
using Splitio.Services.Exceptions;
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
        public event EventHandler<FeedbackEventArgs> ConnectedEvent;
        public event EventHandler<FeedbackEventArgs> DisconnectEvent;
        public event EventHandler<EventArgs> ReconnectEvent;

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

        public void Disconnect(bool reconnect = false)
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _streamReadcancellationTokenSource.Cancel();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _splitHttpClient.Dispose();

            DispatchDisconnect();

            _disconnectSignal.Wait(ReadTimeoutMs);
            _log.Debug($"Disconnected from {_url}");

            if (reconnect)
            {
                DispatchReconnect();
                _log.Debug("Reconnecting Event Source Client...");
            }            
        }
        #endregion

        #region Private Methods
        private async Task ConnectAsync(CountdownEvent signal)
        {
            bool reconnect = false;

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
                                DispatchConnected();
                                signal.Signal();
                                await ReadStreamAsync(stream);
                            }
                        }
                        catch (ReadStreamException ex)
                        {
                            _log.Debug(ex.Message);
                            reconnect = ex.ReconnectEventSourveClient;
                        }
                        catch (Exception ex)
                        {
                            _log.Debug($"Error reading stream: {ex.Message}");
                            reconnect = true;
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
                Disconnect(reconnect);
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

                        if (finishedTask == timeoutTask) throw new ReadStreamException(true, $"Streaming read time out after {ReadTimeoutMs} seconds.");

                        int len = streamReadTask.Result;

                        if (len == 0)
                        {
                            throw new ReadStreamException(true, "Streaming end of the file.");
                        }

                        var notificationString = encoder.GetString(buffer, 0, len);
                        _log.Debug($"Read stream encoder buffer: {notificationString}");

                        if (notificationString != KeepAliveResponse && IsConnected())
                        {
                            var lines = notificationString.Split(new[] { "\n\n" }, StringSplitOptions.None);

                            foreach (var line in lines)
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        var eventData = _notificationParser.Parse(line);

                                        if (eventData != null) DispatchEvent(eventData);
                                    }
                                }
                                catch (NotificationErrorException ex)
                                {
                                    _log.Debug($"Notification error: {ex.Message}. Status Server: {ex.Notification.StatusCode}.");

                                    if (ex.Notification.Code >= 40140 && ex.Notification.Code <= 40149)
                                    {
                                        throw new ReadStreamException(true, $"Ably Notification code: {ex.Notification.Code}");
                                    }
                                    else if (ex.Notification.Code >= 40000 && ex.Notification.Code <= 49999)
                                    {
                                        throw new ReadStreamException(false, $"Ably Notification code: {ex.Notification.Code}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _log.Debug($"Error during event parse: {ex.Message}");
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

        private void DispatchEvent(IncomingNotification incomingNotification)
        {
            _log.Debug($"DispatchEvent: {incomingNotification}");
            EventReceived?.Invoke(this, new EventReceivedEventArgs(incomingNotification));
        }

        private void DispatchDisconnect()
        {
            DisconnectEvent?.Invoke(this, new FeedbackEventArgs(isConnected: false));
        }

        private void DispatchConnected()
        {
            ConnectedEvent?.Invoke(this, new FeedbackEventArgs(isConnected: true));
        }

        private void DispatchReconnect()
        {
            ReconnectEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
