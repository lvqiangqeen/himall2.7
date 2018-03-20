using Enyim.Caching;
using Himall.Core;
using System;
using System.Collections.Generic;

namespace Himall.Strategy
{
    public class Memecached : ICache
    {
        const int DEFAULT_TMEOUT = 600;//默认超时时间（单位秒）
        private int timeout = DEFAULT_TMEOUT;
        private MemcachedClient client = MemcachedClientService.Instance.Client;

        public int TimeOut
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value > 0 ? value : DEFAULT_TMEOUT;
            }
        }

        public object Get(string key)
        {
            return client.Get(key);
        }

        public T Get<T>(string key)
        {
            return (T)client.Get(key);
        }

        public void Remove(string key)
        {
            client.Remove(key);
        }

        public void Clear()
        {
            client.FlushAll();
        }

        public void Insert(string key, object data)
        {
            client.Store(Enyim.Caching.Memcached.StoreMode.Add, key, data);
        }

        public void Insert(string key, object data, int cacheTime)
        {
            client.Store(Enyim.Caching.Memcached.StoreMode.Set, key, data, DateTime.Now.AddSeconds(cacheTime));
        }


        public void Insert(string key, object data, DateTime cacheTime)
        {
            client.Store(Enyim.Caching.Memcached.StoreMode.Set, key, data, cacheTime);
        }

        public void Send(string key, object data)
        {
            throw new NotImplementedException();
        }

        public void Recieve<T>(string key, Cache.DoSub dosub)
        {
            throw new NotImplementedException();
        }

        public void RegisterSubscribe<T>(string key, Cache.DoSub dosub)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterSubscrib(string key)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string key)
        {
            if (client.Get(key) != null)
                return true;
            else
                return false;
        }

        public void Insert<T>(string key, T data)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string key, T data, int cacheTime)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string key, T data, DateTime cacheTime)
        {
            throw new NotImplementedException();
        }

        public bool SetNX(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool SetNX(string key, string value, int cacheTime)
        {
            throw new NotImplementedException();
        }

        public ICacheLocker GetCacheLocker(string key)
        {
            throw new NotImplementedException();
        }
    }
}
