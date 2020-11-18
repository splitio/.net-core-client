using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SegmentTaskWorker
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(SegmentTaskWorker));

        private readonly int _numberOfParallelTasks;
        private readonly ISegmentTaskQueue _segmentTaskQueue;
        private int _counter;

        //Worker is always one task, so when it is signaled, after the
        //task stops its wait, this variable is auto-reseted
        private AutoResetEvent waitForExecution = new AutoResetEvent(false);

        public SegmentTaskWorker(int numberOfParallelTasks,
            ISegmentTaskQueue segmentTaskQueue)
        {
            _numberOfParallelTasks = numberOfParallelTasks;
            _counter = 0;
            _segmentTaskQueue = segmentTaskQueue;
        }

        private void IncrementCounter()
        {
            Interlocked.Increment(ref _counter);
        }

        private void DecrementCounter()
        {
            Interlocked.Decrement(ref _counter);
            waitForExecution.Set();
        }

        public void ExecuteTasks(CancellationToken token)
        {
            while (true)
            {
                if (_counter < _numberOfParallelTasks)
                {
                    try
                    {
                        //Wait indefinitely until a segment is queued
                        if (_segmentTaskQueue.GetQueue().TryTake(out SelfRefreshingSegment segment, -1))
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(string.Format("Segment dequeued: {0}", segment.Name));
                            }

                            IncrementCounter();
                            Task task = new Task(async () => await segment.FetchSegment(), token);
                            task.ContinueWith((x) => { DecrementCounter(); });
                            task.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex.Message);
                    }
                }
                else
                {
                    waitForExecution.WaitOne();
                }
            }
        }
    }
}
