using System;

namespace Splitio.Services.EventSource
{
    public interface ISSEHandler
    {
        bool Start(string token, string channels);
        void Stop();
        void StartWorkers();
        void StopWorkers();

        event EventHandler<SSEActionsEventArgs> ActionEvent;
    }
}
