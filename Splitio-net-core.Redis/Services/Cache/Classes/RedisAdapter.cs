﻿using Splitio.Redis.Services.Cache.Interfaces;
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

        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly IServer _server;

        public RedisAdapter(string host, 
            string port, 
            string password = "", 
            int databaseNumber = 0,
            int connectTimeout = 0, 
            int connectRetry = 0, 
            int syncTimeout = 0)
        {
            try
            {
                var config = GetConfig(host, port, password, connectTimeout, connectRetry, syncTimeout);

                _redis = ConnectionMultiplexer.Connect(config);
                _database = _redis.GetDatabase(databaseNumber);
                _server = _redis.GetServer($"{host}:{port}");
            }
            catch (Exception e)
            {
                _log.Error(string.Format("Exception caught building Redis Adapter '{0}:{1}': ", host, port), e);
            }
        }

        public bool IsConnected()
        {
            return _server?.IsConnected ?? false;
        }

        private static string GetConfig(string host, string port, string password, int connectTimeout, int connectRetry, int syncTimeout)
        {
            var config = string.Format("{0}:{1}, password = {2}, allowAdmin = true", host, port, password);

            if (connectTimeout > 0)
            {
                config += ", connectTimeout = " + connectTimeout;
            }

            if (connectRetry > 0)
            {
                config += ", connectRetry = " + connectRetry;
            }

            if (syncTimeout > 0)
            {
                config += ", syncTimeout = " + syncTimeout;
            }

            config += ", keepAlive = " + 1;

            return config;
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
    }
}
