using NLog;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Splitio.Services.Impressions.Classes
{
    public class AsynchronousImpressionListener : IImpressionListener
    {
        protected static readonly Logger Logger = LogManager.GetLogger("AsynchronousImpressionListener");
        private List<IImpressionListener> workers = new List<IImpressionListener>();

        public void AddListener(IImpressionListener worker)
        {
            workers.Add(worker);
        }

        public void Log(KeyImpression impression)
        {
            try
            {
                var enqueueTask = new Task(() =>
                {
                    foreach (IImpressionListener worker in workers)
                    {
                        try
                        {
                            worker.Log(impression);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Exception performing Log with worker. ");
                        }
                    }
                });

                enqueueTask.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception creating Log task. ");
            }
        }
    }
}
