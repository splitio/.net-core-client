using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;

namespace Splitio.Services.EventSource
{
    public class NotificationPorcessor : INotificationPorcessor
    {
        private readonly ISplitLogger _log;
        private readonly ISplitsWorker _splitsWorker;
        private readonly ISegmentsWorker _segmentsWorker;

        public NotificationPorcessor(ISplitsWorker splitsWorker,
            ISegmentsWorker segmentsWorker,
            ISplitLogger log = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(EventSourceClient));
            _splitsWorker = splitsWorker;
            _segmentsWorker = segmentsWorker;
        }

        public void Proccess(IncomingNotification notification)
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
                    break;
                case NotificationType.SEGMENT_UPDATE:
                    var sc = (SegmentChangeNotification)notification;
                    _segmentsWorker.AddToQueue(sc.ChangeNumber, sc.SegmentName);
                    break;
                case NotificationType.CONTROL:
                    // TODO: check this.
                    break;
                default:
                    _log.Debug($"Incorrect Event type: {notification}");
                    break;
            }
        }
    }
}
