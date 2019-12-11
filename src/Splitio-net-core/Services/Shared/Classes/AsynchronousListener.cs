using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Splitio.Services.Shared.Classes
{
    public class AsynchronousListener<T> : IAsynchronousListener<T>
    {
        private readonly ISplitLogger _logger;
        private readonly IList<IListener<T>> _workers;

        public AsynchronousListener(ISplitLogger logger)
        {
            _logger = logger;
            _workers = new List<IListener<T>>();
        }

        public void AddListener(IListener<T> worker)
        {
            _workers.Add(worker);
        }

        public void Notify(T item)
        {
            try
            {
                //This task avoids waiting to fire and forget 
                //all worker's tasks in the main thread
                var listenerTask = new Task(() =>
                {
                    foreach (var worker in _workers)
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
                                _logger.Info(worker.GetType() + " took " + stopwatch.ElapsedMilliseconds + " milliseconds");
                            });

                            logTask.Start();
                        }
                        catch (Exception e)
                        {
                            _logger.Error("Exception performing Log with worker. ", e);
                        }
                    }
                });

                listenerTask.Start();
            }
            catch (Exception e)
            {
                _logger.Error("Exception creating Log task. ", e);
            }
        }
    }
}

