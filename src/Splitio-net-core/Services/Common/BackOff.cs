using System;

namespace Splitio.Services.Common
{
    public class BackOff : IBackOff
    {
        private readonly int _backOffBase;
        private int _attempt;

        public BackOff(int backOffBase, int attempt = 0)
        {
            _backOffBase = backOffBase;
            _attempt = attempt;
        }

        public int GetAttempt()
        {
            return _attempt;
        }

        public double GetInterval()
        {
            var interval = 0d;

            if (_attempt > 0)
            {
                interval = _backOffBase * Math.Pow(2, _attempt);
            }

            _attempt++;

            return interval;
        }

        public void Reset()
        {
            _attempt = 0;
        }
    }
}
