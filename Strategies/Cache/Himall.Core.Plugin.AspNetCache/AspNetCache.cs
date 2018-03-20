using Himall.Core;
using System;
using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;

namespace Himall.Strategy
{
    public class AspNetCache:ICache
    {
        private System.Web.Caching.Cache cache;
        static object cacheLocker = new object();

        public AspNetCache()
        {
            cache = HttpRuntime.Cache;
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public object Get(string key)
        {
            return cache.Get(key);
        }

        public T Get<T>(string key)
        {
            T result =  (T)cache.Get(key);
            return result;
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Insert(string key, object value)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value);
            }
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        public void Insert(string key, object value, int cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value, null, DateTime.Now.AddSeconds(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
        }


        public void Insert(string key, object value, DateTime cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value, null, cacheTime, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
        }

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        public void Clear()
        {
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            while (cacheEnum.MoveNext())
                cache.Remove(cacheEnum.Key.ToString());
        }

        public void Send(string key, object data)
        {
            return;
        }

        public void Recieve<T>(string key, Core.Cache.DoSub dosub)
        {
            return;
        }

        public void RegisterSubscribe<T>(string key, Core.Cache.DoSub dosub)
        {
            return ;
        }

        public void UnRegisterSubscrib(string key)
        {
            return;
        }

        public bool Exists(string key)
        {
            if (cache.Get(key) != null)
                return true;
            else
                return false;
        }

        public void Insert<T>(string key, T value)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value);
            }
        }

        public void Insert<T>(string key, T value, int cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value, null, DateTime.Now.AddSeconds(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
        }

        public void Insert<T>(string key, T value, DateTime cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Insert(key, value, null, cacheTime, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
            }
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

        const int DEFAULT_TMEOUT = 600;//默认超时时间（单位秒）

        private int _timeout = DEFAULT_TMEOUT;

        /// <summary>
        /// 缓存过期时间
        /// </summary>
        /// <value></value>
        public int TimeOut
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value > 0 ? value : DEFAULT_TMEOUT;
            }
        }




    }
}
