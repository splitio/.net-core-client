using Microsoft.CSharp.RuntimeBinder;
using Splitio.Services.Client.Interfaces;
using System;
using System.Globalization;
using System.Reflection;

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
                        client = new LocalhostClient(options.LocalhostFilePath, Common.Logging.LogManager.GetLogger(typeof(SplitClient)));
                    }
                    else
                    {
                        client = new SelfRefreshingClient(apiKey, options, Common.Logging.LogManager.GetLogger(typeof(SplitClient)));
                    }
                    break;
                case Mode.Consumer:
                    if (options.CacheAdapterConfig != null && options.CacheAdapterConfig.Type == AdapterType.Redis)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(options.CacheAdapterConfig.Host) || String.IsNullOrEmpty(options.CacheAdapterConfig.Port))
                            {
                                throw new Exception("Redis Host and Port should be set to initialize Split SDK in Redis Mode.");
                            }
                            var redisAssembly = Assembly.Load(new AssemblyName("Splitio-net-core.Redis"));
                            var redisType = redisAssembly.GetType("Splitio.Redis.Services.Client.Classes.RedisClient");
                            client = (ISplitClient)Activator.CreateInstance(redisType, new Object[] { options, Common.Logging.LogManager.GetLogger(typeof(SplitClient)) });

                        }
                        catch (Exception e)
                        {
                            throw new Exception("Splitio.Redis package should be added as reference, to build split client in Redis Consumer mode.", e);
                        }
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
