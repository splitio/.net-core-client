using Common.Logging;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Concurrent;

namespace Splitio.Services.Shared.Classes
{
    public class FactoryInstantiationsService : IFactoryInstantiationsService
    {
        private static FactoryInstantiationsService _instance;
        private static object _instanceLock = new object();
        private static object _lock = new object();

        private ILog _log;
        private ConcurrentDictionary<string, int> _factoryInstantiations;

        public static IFactoryInstantiationsService Instance(ILog log)
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new FactoryInstantiationsService(log);
                    }
                }
            }

            return _instance;
        }

        private FactoryInstantiationsService(ILog log)
        {
            _log = log;
            _factoryInstantiations = new ConcurrentDictionary<string, int>();
        }

        public void Decrease(string apiKey)
        {
            lock (_lock)
            {
                if (_factoryInstantiations.TryGetValue(apiKey, out int quantity))
                {
                    if (quantity == 1)
                    {
                        _factoryInstantiations.TryRemove(apiKey, out int value);

                        return;
                    }

                    var newQuantity = quantity - 1;

                    _factoryInstantiations.TryUpdate(apiKey, newQuantity, quantity);
                }
            }
        }

        public void Increase(string apiKey)
        {
            lock (_lock)
            {
                var exists = _factoryInstantiations.TryGetValue(apiKey, out int quantity);

                if (exists)
                {
                    if (quantity >= 1)
                    {
                        _log.Warn($"factory instantiation: You already have {quantity} factories with this API Key. We recommend keeping only one instance of the factory at all times(Singleton pattern) and reusing it throughout your application.");
                    }

                    var newQuantity = quantity + 1;

                    _factoryInstantiations.TryUpdate(apiKey, newQuantity, quantity);

                    return;
                }

                if (_factoryInstantiations.Count > 0)
                {
                    _log.Warn("factory instantiation: You already have an instance of the Split factory. Make sure you definitely want this additional instance. We recommend keeping only one instance of the factory at all times(Singleton pattern) and reusing it throughout your application.");
                }

                _factoryInstantiations.TryAdd(apiKey, 1);
            }
        }

        //This method is only for test
        public ConcurrentDictionary<string, int> GetInstantiations()
        {
            return _factoryInstantiations;
        }
    }
}
