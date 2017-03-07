namespace Splitio.Services.Metrics.Classes
{
    public class Counter
    {
        private int count = 0;
        private long sum = 0;

        public Counter():base()
        { }

        public Counter(long delta)
        {
            count++;
            sum = delta;
        }

        public int GetCount()
        {
            return count;
        }

        public long GetDelta()
        {
            return sum;
        }

        public void AddDelta(long delta)
        {
            count++;
            sum += delta;
        }
    }
}
