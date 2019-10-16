using Splitio.Services.Client.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Reflection;

namespace Splitio.Services.Client.Classes
{
    public class SplitFactory
    {
        private readonly IApiKeyValidator _apiKeyValidator;
        private readonly IFactoryInstantiationsService _factoryInstantiationsService;
        private readonly string _apiKey;

        private ISplitClient _client;
        private ISplitManager _manager;
        private ConfigurationOptions _options;

        public SplitFactory(string apiKey, 
            ConfigurationOptions options = null)
        {
            _apiKey = apiKey;
            _options = options;

            _apiKeyValidator = new ApiKeyValidator();
            _factoryInstantiationsService = FactoryInstantiationsService.Instance();
        }

        public ISplitClient Client()
        {
            if (_client == null)
            {
                BuildSplitClient();
            }

            return _client;
        }

        private void BuildSplitClient()
        {
            _options = _options ?? new ConfigurationOptions();

            _apiKeyValidator.Validate(_apiKey);

            switch (_options.Mode)
            {
                case Mode.Standalone:
                    if (string.IsNullOrEmpty(_apiKey))
                    {
                        throw new Exception("API Key should be set to initialize Split SDK.");
                    }
                    if (_apiKey == "localhost")
                    {
                        _client = new LocalhostClient(_options.LocalhostFilePath);
                    }
                    else
                    {
                        _client = new SelfRefreshingClient(_apiKey, _options);
                    }
                    break;
                case Mode.Consumer:
                    if (_options.CacheAdapterConfig != null && _options.CacheAdapterConfig.Type == AdapterType.Redis)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(_options.CacheAdapterConfig.Host) || string.IsNullOrEmpty(_options.CacheAdapterConfig.Port))
                            {
                                throw new Exception("Redis Host and Port should be set to initialize Split SDK in Redis Mode.");
                            }

                            var redisAssembly = Assembly.Load(new AssemblyName("Splitio-net-core.Redis"));
                            var redisType = redisAssembly.GetType("Splitio.Redis.Services.Client.Classes.RedisClient");

                            _client = (ISplitClient)Activator.CreateInstance(redisType, new object[] { _options, _apiKey, null });

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

            _factoryInstantiationsService.Increase(_apiKey);
        }

        public ISplitManager Manager()
        {
            if (_client == null)
            {
                BuildSplitClient();
            }
           
            _manager = _client.GetSplitManager();

            return _manager;
        }
    }
}
