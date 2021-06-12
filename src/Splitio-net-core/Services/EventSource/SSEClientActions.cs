namespace Splitio.Services.EventSource
{
    public enum SSEClientActions
    {
        CONNECTED,
        DISCONNECT,
        RETRYABLE_ERROR,
        NONRETRYABLE_ERROR,
        SUBSYSTEM_DOWN,
        SUBSYSTEM_READY,
        SUBSYSTEM_OFF
    }
}
