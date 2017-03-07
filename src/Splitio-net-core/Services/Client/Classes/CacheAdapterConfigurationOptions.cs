namespace Splitio.Services.Client.Classes
{
    public class CacheAdapterConfigurationOptions
    {
        public AdapterType Type { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Password { get; set; }
        public int? Database { get; set; }
        public int? ConnectTimeout { get; set; }
        public int? ConnectRetry { get; set; }
        public int? SyncTimeout { get; set; }
        public string UserPrefix { get; set; }
    }
}
