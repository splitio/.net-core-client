# if NETSTANDARD
using Microsoft.Extensions.Logging;
using System;

namespace Splitio.Services.Logger
{
    public class MicrosoftExtensionsLogging : ISplitLogger
    {
        private const int DefaultLoggingEvent = 0;

        private static ILoggerFactory _loggerFactory => SplitLoggerFactoryExtensions.GetLoggerFactory() ?? new LoggerFactory();

        private readonly ILogger _logger;
        
        public MicrosoftExtensionsLogging(Type type)
        {
            _logger = _loggerFactory.CreateLogger(type);
        }

        public MicrosoftExtensionsLogging(string type)
        {
            _logger = _loggerFactory.CreateLogger(type);
        }

        public void Debug(string message, Exception exception)
        {
            _logger.LogDebug(DefaultLoggingEvent, exception, message);
        }

        public void Debug(string message)
        {
            _logger.LogDebug(message);
        }

        public void Error(string message, Exception exception)
        {
            _logger.LogError(DefaultLoggingEvent, exception, message);
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }        

        public void Info(string message, Exception exception)
        {
            _logger.LogInformation(DefaultLoggingEvent, exception, message);
        }

        public void Info(string message)
        {
            _logger.LogInformation(message);
        }

        public bool IsDebugEnabled()
        {
            return _logger.IsEnabled(LogLevel.Debug);
        }

        public void Trace(string message, Exception exception)
        {
            _logger.LogTrace(DefaultLoggingEvent, exception, message);
        }

        public void Trace(string message)
        {
            _logger.LogTrace(message);
        }

        public void Warn(string message, Exception exception)
        {
            _logger.LogWarning(DefaultLoggingEvent, exception, message);
        }

        public void Warn(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
#endif