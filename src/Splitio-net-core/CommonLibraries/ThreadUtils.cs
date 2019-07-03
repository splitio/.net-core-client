using System.Threading.Tasks;

namespace Splitio.CommonLibraries
{
    public class ThreadUtils
    {
        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
#if net40
            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.TrySetResult(true);
            };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            
#endif
            return tcs.Task;
        }
    }
}
