using System;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.CommonLibraries
{
    /// <summary>
    /// Factory class to create a periodic Task 
    /// </summary>
    public static class PeriodicTaskFactory
    {
        /// <summary>
        /// Starts the periodic task.
        /// </summary>
        public static Task Start(Action action, int intervalInMilliseconds, CancellationToken cancelToken)
        {
            Action mainAction = () =>
            {
                MainPeriodicTaskAction(intervalInMilliseconds, cancelToken,  action);
            };

            return Task.Factory.StartNew(mainAction, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        /// <summary>
        /// Mains the periodic task action.
        /// </summary>
        private static void MainPeriodicTaskAction(int intervalInMilliseconds, CancellationToken cancelToken, Action wrapperAction)
        {
            // using a ManualResetEventSlim as it is more efficient in small intervals.
            // In the case where longer intervals are used, it will automatically use 
            // a standard WaitHandle....
            // see http://msdn.microsoft.com/en-us/library/vstudio/5hbefs30(v=vs.100).aspx
            using (ManualResetEventSlim periodResetEvent = new ManualResetEventSlim(false))
            {
                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    Task subTask = Task.Factory.StartNew(wrapperAction, cancelToken);

                    try
                    {
                        periodResetEvent.Wait(intervalInMilliseconds, cancelToken);
                    }
                    finally
                    {
                        periodResetEvent.Reset();
                    }
                }
            }
        }
    }
}

