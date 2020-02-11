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
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly object _statusLock = new object();
        private Status _status;

        public EventSourceClient(Uri uri)
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => Connect(uri));
        }

        public event EventHandler<EventReceivedEventArgs> EventReceived;
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;

        public Status Status()
        {
            return _status;
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _httpClient.Dispose();
            UpdateStatus(EventSource.Status.Disconnected);
        }

        private async Task Connect(Uri uri)
        {
            try
            {
                UpdateStatus(EventSource.Status.Connecting);

                var request = new HttpRequestMessage(HttpMethod.Get, uri);

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        UpdateStatus(EventSource.Status.Connected);
                        await ReadStream(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                DispatchError(ex.Message);
                UpdateStatus(EventSource.Status.Disconnected);
            }
        }

        private async Task ReadStream(Stream stream)
        {
            var encoder = new UTF8Encoding();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (stream.CanRead)
                {
                    var buffer = new byte[2048];

                    int len = await stream.ReadAsync(buffer, 0, 2048, _cancellationTokenSource.Token);

                    if (len > 0)
                    {
                        var text = encoder.GetString(buffer, 0, len);

                        if (text.Contains("event"))
                        {
                            var ssevent = JsonConvert.DeserializeObject<Event>(text);

                            DispatchEvent(ssevent);
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
    }
}
