using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using StackExchange.Redis;
using System;
using System.Linq;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisAdapter : IRedisAdapter
    {
        private static readonly ISplitLogger _log = WrapperAdapter.GetLogger(typeof(RedisAdapter));
               
        private readonly string _host;
        private readonly string _port;
        private readonly string _password = "";
        private readonly int _databaseNumber = 0;
        private readonly int _connectTimeout = 0;
        private readonly int _connectRetry = 0;
        private readonly int _syncTimeout = 0;

        private IConnectionMultiplexer _redis;
        private IDatabase _database;
        private IServer _server;

        public RedisAdapter(string host,
            string port,
            string password = "",
            int databaseNumber = 0,
            int connectTimeout = 0,
            int connectRetry = 0,
            int syncTimeout = 0)
        {
            _host = host;
            _port = port;
            _password = password;
            _databaseNumber = databaseNumber;
            _connectTimeout = connectTimeout;
            _connectRetry = connectRetry;
            _syncTimeout = syncTimeout;
        }

        public void Connect()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(GetConfig());
                _database = _redis.GetDatabase(_databaseNumber);
                _server = _redis.GetServer($"{_host}:{_port}");
            }
            catch (Exception e)
            {
                _log.Error(string.Format("Exception caught building Redis Adapter '{0}:{1}': ", _host, _port), e);
            }
        }

        public bool IsConnected()
        {
            return _server?.IsConnected ?? false;
        }
        
        public bool Set(string key, string value)
        {
            try
            {
                return _database.StringSet(key, value);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Set", e);
                return false;
            }
        }

        public string Get(string key)
        {
            try
            {
                return _database.StringGet(key);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Get", e);
                return string.Empty;
            }
        }

        public RedisValue[] MGet(RedisKey[] keys)
        {
            try
            {
                return _database.StringGet(keys);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Get", e);
                return new RedisValue[0];
            }
        }

        public RedisKey[] Keys(string pattern)
        {
            try
            {
                var keys = _server.Keys(pattern: pattern);
                return keys.ToArray();
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Keys", e);
                return new RedisKey[0];
            }
        }

        public bool Del(string key)
        {
            try
            {
                return _database.KeyDelete(key);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Del", e);
                return false;
            }
        }

        public long Del(RedisKey[] keys)
        {
            try
            {
                return _database.KeyDelete(keys);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Del", e);
                return 0;
            }
        }

        public bool SAdd(string key, RedisValue value)
        {
            try
            {
                return _database.SetAdd(key, value);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter SAdd", e);
                return false;
            }
        }

        public long SAdd(string key, RedisValue[] values)
        {
            try
            {
                return _database.SetAdd(key, values);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter SAdd", e);
                return 0;
            }
        }

        public long SRem(string key, RedisValue[] values)
        {
            try
            {
                return _database.SetRemove(key, values);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter SRem", e);
                return 0;
            }
        }

        public bool SIsMember(string key, string value)
        {
            try
            {
                return _database.SetContains(key, value);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter SIsMember", e);
                return false;
            }
        }

        public RedisValue[] SMembers(string key)
        {
            try
            {
                return _database.SetMembers(key);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter SMembers", e);
                return new RedisValue[0];
            }
        }

        public long IcrBy(string key, long value)
        {
            try
            {
                return _database.StringIncrement(key, value);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter IcrBy", e);
                return 0;
            }
        }

        public long ListRightPush(string key, RedisValue value)
        {
            try
            {
                return _database.ListRightPush(key, value);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter ListRightPush", e);
                return 0;
            }
        }

        public void Flush()
        {
            try
            {
                _server.FlushDatabase();
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter Flush", e);
            }
        }

        public bool KeyExpire(string key, TimeSpan expiry)
        {
            try
            {
                return _database.KeyExpire(key, expiry);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter KeyExpire", e);
                return false;
            }
        }

        public long ListRightPush(string key, RedisValue[] values)
        {
            try
            {
                return _database.ListRightPush(key, values);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter ListRightPush", e);
                return 0;
            }
        }

        public RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1)
        {
            try
            {
                return _database.ListRange(key, start, stop);
            }
            catch (Exception e)
            {
                _log.Error("Exception calling Redis Adapter ListRange", e);
                return new RedisValue[0];
            }
        }

        private string GetConfig()
        {
            var config = string.Format("{0}:{1}, password = {2}, allowAdmin = true", _host, _port, _password);

            if (_connectTimeout > 0)
            {
                config += ", connectTimeout = " + _connectTimeout;
            }

            if (_connectRetry > 0)
            {
                config += ", connectRetry = " + _connectRetry;
            }

            if (_syncTimeout > 0)
            {
                config += ", syncTimeout = " + _syncTimeout;
            }

            config += ", keepAlive = " + 1;

            return config;
        }
    }
}
