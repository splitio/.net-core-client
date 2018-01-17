using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Splitio.Services.Events.Classes
{
    public class AsynchronousEventListener : IEventListener
    {
        protected static readonly ILog Logger = LogManager.GetLogger("AsynchronousEventListener");
        private List<IEventListener> workers = new List<IEventListener>();

        public void AddListener(IEventListener worker)
        {
            workers.Add(worker);
        }

        public void Log(Event item)
        {
            try
            {
                //This task avoids waiting to fire and forget 
                //all worker's tasks in the main thread
                var listenerTask = new Task(() =>
                {
                    foreach (IEventListener worker in workers)
                    {
                        try
                        {
                            //This task makes worker.Log() run independently 
                            //and avoid one worker to block another.
                            var logTask = new Task(() =>
                                                {
                                                    var stopwatch = Stopwatch.StartNew();
                                                    worker.Log(item);
                                                    stopwatch.Stop();
                                                    Logger.Info(worker.GetType() + " took " + stopwatch.ElapsedMilliseconds + " milliseconds");
                                                });
                            logTask.Start();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Exception performing Log with worker. ", e);
                        }
                    }
                });
                listenerTask.Start();
            }
            catch (Exception e)
            {
                Logger.Error("Exception creating Log task. ", e);
            }
        }
    }
}

