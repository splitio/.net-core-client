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
        private const int ReadTimeout = 70;
        private const int BufferSize = 10000;
        private const int ConnectTimeout = 100000;
        private const int DisconnectTimeout = 1000;

        private readonly ISplitLogger _log;
        private readonly INotificationParser _notificationParser;
        private readonly IWrapperAdapter _wrapperAdapter;

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
        }

        public event EventHandler<EventReceivedEventArgs> EventReceived;
        public event EventHandler<FeedbackEventArgs> ConnectedEvent;
        public event EventHandler<FeedbackEventArgs> DisconnectEvent;

        #region Public Methods
        public bool ConnectAsync(string url)
        {
            if (IsConnected())
            {
                _log.Debug("Event source Client already connected.");
                return false;
            }

            _url = url;
            
            var signal = new CountdownEvent(1);
            Task.Factory.StartNew(() => ConnectAsync(signal));

            try
            {
                if (!signal.Wait(ConnectTimeout))
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
            if (!IsConnected() || _cancellationTokenSource.IsCancellationRequested) return;

            _streamReadcancellationTokenSource.Cancel();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _splitHttpClient.Dispose();

            DispatchDisconnect(reconnect);
            _log.Info($"Disconnected from {_url}");
        }
        #endregion

        #region Private Methods
        private async Task ConnectAsync(CountdownEvent signal)
        {
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
                        catch (Exception ex)
                        {
                            _log.Error($"Error reading stream: {ex.Message}");
                            Disconnect(reconnect: true);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error connecting to {_url}: {ex.Message}");
            }
            finally
            {
                _connected = false;
            }

            _log.Debug("Finished Event Source client ConnectAsync.");
            Disconnect();
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

                        var timeoutTask = _wrapperAdapter.TaskDelay(ReadTimeout * 1000);
                        var streamReadTask = stream.ReadAsync(buffer, 0, BufferSize, _streamReadcancellationTokenSource.Token);
                        // Creates a task that will complete when any of the supplied tasks have completed.
                        // Returns: A task that represents the completion of one of the supplied tasks. The return task's Result is the task that completed.
                        var finishedTask = await _wrapperAdapter.WhenAny(streamReadTask, timeoutTask);

                        if (finishedTask == timeoutTask) throw new Exception($"Streaming read time out after {ReadTimeout} seconds.");

                        int len = streamReadTask.Result;

                        if (len == 0) throw new Exception($"Streaming end of the file.");

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
                                        Disconnect(reconnect: true);
                                    }
                                    else if (ex.Notification.Code >= 40000 && ex.Notification.Code <= 49999)
                                    {
                                        Disconnect();
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
            OnEvent(new EventReceivedEventArgs(incomingNotification));
        }

        private void DispatchDisconnect(bool reconnect = false)
        {
            OnDisconnect(new FeedbackEventArgs(isConnected: false, reconnect: reconnect));
        }

        private void DispatchConnected()
        {
            OnConnected(new FeedbackEventArgs(isConnected: true));
        }

        private void OnEvent(EventReceivedEventArgs e)
        {
            EventReceived?.Invoke(this, e);
        }

        private void OnConnected(FeedbackEventArgs e)
        {
            ConnectedEvent?.Invoke(this, e);
        }

        private void OnDisconnect(FeedbackEventArgs e)
        {
            DisconnectEvent?.Invoke(this, e);
        }
        #endregion
    }
}
