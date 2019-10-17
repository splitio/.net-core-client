#if NET40 || NET45
using Common.Logging;
using System; 

namespace Splitio.Services.Logger
{
    public class CommonLogging : ISplitLogger
    {
        private ILog _logger;

        public CommonLogging(Type type)
        {
            _logger = LogManager.GetLogger(type);
        }

        public CommonLogging(string type)
        {
            _logger = LogManager.GetLogger(type);
        }

        public void Debug(string message, Exception exception)
        {
            _logger.Debug(message, exception);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Info(string message, Exception exception)
        {
            _logger.Info(message, exception);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }
        
        public void Trace(string message, Exception exception)
        {
            _logger.Trace(message, exception);
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Warn(string message, Exception exception)
        {
            _logger.Warn(message, exception);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public bool IsDebugEnabled => _logger.IsDebugEnabled;
    }
}
#endif