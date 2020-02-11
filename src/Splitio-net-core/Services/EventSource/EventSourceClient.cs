using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource
{
    public class EventSourceClient : IEventSourceClient
    {
        private readonly Uri _uri;
        private readonly int _readTimeout;

        private readonly object _statusLock = new object();
        private Status _status;

        private HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;

        public EventSourceClient(Uri uri,
            int readTimeout = 300000)
        {
            _uri = uri;
            _readTimeout = readTimeout;

            Task.Factory.StartNew(() => ConnectAsync());
        }

        public event EventHandler<EventReceivedEventArgs> EventReceived;
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;

        #region Public Methods
        public Status Status()
        {
            lock (_statusLock)
            {
                return _status;
            }
        }

        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _httpClient.CancelPendingRequests();
            _httpClient.Dispose();

            UpdateStatus(EventSource.Status.Disconnected);
        }
        #endregion

        #region Private Methods
        private async Task ConnectAsync()
        {
            try
            {
                _httpClient = new HttpClient();
                _cancellationTokenSource = new CancellationTokenSource();

                UpdateStatus(EventSource.Status.Connecting);

                var request = new HttpRequestMessage(HttpMethod.Get, _uri);

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        stream.ReadTimeout = _readTimeout;
                        UpdateStatus(EventSource.Status.Connected);
                        await ReadStreamAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                DispatchError(ex.Message);
                UpdateStatus(EventSource.Status.Disconnected);
            }
        }

        private async Task ReadStreamAsync(Stream stream)
        {
            var encoder = new UTF8Encoding();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (stream.CanRead)
                {
                    var buffer = new byte[2048];

                    int len = await stream.ReadAsync(buffer, 0, 2048, _cancellationTokenSource.Token);

                    if (len > 0 && Status() == EventSource.Status.Connected)
                    {
                        var text = encoder.GetString(buffer, 0, len);

                        if (text.Contains("event"))
                        {
                            try
                            {
                                var ssevent = JsonConvert.DeserializeObject<Event>(text);

                                if (ssevent.Data == null || string.IsNullOrEmpty(ssevent.Type))
                                    throw new Exception("Invalid format.");

                                DispatchEvent(ssevent);
                            }
                            catch (Exception ex)
                            {
                                DispatchError(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void DispatchEvent(Event ssEvent)
        {
            OnEvent(new EventReceivedEventArgs(ssEvent));
        }

        private void DispatchError(string message)
        {
            OnError(new ErrorReceivedEventArgs(message));
        }

        private void OnEvent(EventReceivedEventArgs e)
        {
            EventReceived?.Invoke(this, e);
        }

        private void OnError(ErrorReceivedEventArgs e)
        {
            ErrorReceived?.Invoke(this, e);
        }

        private void UpdateStatus(Status status)
        {
            lock (_statusLock)
            {
                _status = status;
            }
        }
        #endregion
    }
}
