using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ISplitLogger _log;
        private readonly ISplitsWorker _splitsWorker;
        private readonly IWorker<SegmentQueueDto> _segmentsWorker;

        public NotificationProcessor(ISplitsWorker splitsWorker,
            IWorker<SegmentQueueDto> segmentsWorker,
            ISplitLogger log = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(EventSourceClient));
            _splitsWorker = splitsWorker;
            _segmentsWorker = segmentsWorker;
        }

        public void Proccess(IncomingNotification notification)
        {
            try
            {
                switch (notification.Type)
                {
                    case NotificationType.SPLIT_UPDATE:
                        var scn = (SplitChangeNotifiaction)notification;
                        _splitsWorker.AddToQueue(scn.ChangeNumber);
                        break;
                    case NotificationType.SPLIT_KILL:
                        var skn = (SplitKillNotification)notification;
                        _splitsWorker.KillSplit(skn.ChangeNumber, skn.SplitName, skn.DefaultTreatment);
                        _splitsWorker.AddToQueue(skn.ChangeNumber);
                        break;
                    case NotificationType.SEGMENT_UPDATE:
                        var sc = (SegmentChangeNotification)notification;
                        _segmentsWorker.AddToQueue(new SegmentQueueDto(sc.ChangeNumber, sc.SegmentName));
                        break;
                    default:
                        _log.Debug($"Incorrect Event type: {notification}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Processor: {ex.Message}");
            }
        }
    }
}
