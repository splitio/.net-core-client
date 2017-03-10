using log4net;
using log4net.Repository.Hierarchy;
using Splitio.Services.Client.Interfaces;
using System;

namespace Splitio.Services.Client.Classes
{
    public class SplitFactory
    {
        private ISplitClient client;
        private ISplitManager manager;
        private string apiKey;
        private ConfigurationOptions options;

        public SplitFactory(string apiKey, ConfigurationOptions options = null)
        {
            this.apiKey = apiKey;
            this.options = options;

            try
            {
                var respository = LogManager.GetRepository("splitio");
            }
            catch
            {
                LogManager.CreateRepository("splitio", typeof(Hierarchy));
            }
        }

        public ISplitClient Client()
        {
            if (client == null)
            {
                BuildSplitClient();
            }
            return client;
        }

        private void BuildSplitClient()
        {
            if (options == null)
            {
                options = new ConfigurationOptions();
            }

            switch(options.Mode)
            {
                case Mode.Standalone:
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        throw new Exception("API Key should be set to initialize Split SDK.");
                    }
                    if (apiKey == "localhost")
                    {
                        client = new LocalhostClient(options.LocalhostFilePath);
                    }
                    else
                    {
                        client = new SelfRefreshingClient(apiKey, options);
                    }
                    break;
                case Mode.Consumer:
                    if (options.CacheAdapterConfig != null && options.CacheAdapterConfig.Type == AdapterType.Redis)
                    {
                        if (string.IsNullOrEmpty(options.CacheAdapterConfig.Host) || string.IsNullOrEmpty(options.CacheAdapterConfig.Port) || string.IsNullOrEmpty(options.CacheAdapterConfig.Password))
                        {
                            throw new Exception("Redis Host, Port and Password should be set to initialize Split SDK in Redis Mode.");
                        }
                        client = new RedisClient(options);
                    }
                    else
                    {
                        throw new Exception("Redis config should be set to build split client in Consumer mode.");
                    }
                    break;
                case Mode.Producer:
                    throw new Exception("Unsupported mode.");
                default:
                    throw new Exception("Mode should be set to build split client.");
            }            
        }

        public ISplitManager Manager()
        {
            if (client == null)
            {
                BuildSplitClient();
            }
           
            manager = client.GetSplitManager();

            return manager;
        }
    }
}
