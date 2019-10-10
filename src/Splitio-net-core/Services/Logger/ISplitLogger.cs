using System;

namespace Splitio.Services.Logger
{
    public interface ISplitLogger
    {
        void Debug(string message, Exception exception);
        void Debug(string message);
        void Error(string message, Exception exception);
        void Error(string message);
        void Info(string message, Exception exception);
        void Info(string message);
        void Trace(string message, Exception exception);
        void Trace(string message);
        void Warn(string message, Exception exception);
        void Warn(string message);

        bool IsDebugEnabled();
    }
}
