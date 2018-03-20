using System;
using System.Collections.Generic;

namespace Himall.Core
{
    /// <summary>
    /// Himall缓存
    /// </summary>
    public static class Cache
    {
        private static object cacheLocker = new object();//缓存锁对象
        private static ICache cache = null;//缓存接口

        /// <summary>
        /// 接收数据处理方法定义
        /// </summary>
        /// <param name="d">数据</param>
        public delegate void DoSub(object d);

        static Cache()
        {
            Load();
        }

        /// <summary>
        /// 加载缓存策略
        /// </summary>
        /// <exception cref="CacheRegisterException"></exception>
        private static void Load()
        {
            //通过autofac获取缓存的实现
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ICache>();
            //builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
            //IContainer container = null;
            try
            {
                //container = builder.Build();
                cache = ObjectContainer.Current.Resolve<ICache>();
            }
            catch (Exception ex)
            {
                throw new CacheRegisterException("注册缓存服务异常", ex);
            }
        }

        public static ICache GetCache()
        {
            return cache;
        }


        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public static int TimeOut
        {
            get
            {
                return cache.TimeOut;
            }
            set
            {
                lock (cacheLocker)
                {
                    cache.TimeOut = value;
                }
            }
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public static object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            return cache.Get(key);
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public static T Get<T>(string key)
        {
            return cache.Get<T>(key);
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        public static void Insert(string key, object data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;
            //lock (cacheLocker)
            {
                cache.Insert(key, data);
            }
        }
        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        public static void Insert<T>(string key, T data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;
            //lock (cacheLocker)
            {
                cache.Insert<T>(key, data);
            }
        }
        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        public static void Insert(string key, object data, int cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                //lock (cacheLocker)
                {
                    cache.Insert(key, data, cacheTime);
                }
            }
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        public static void Insert<T>(string key, T data, int cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                //lock (cacheLocker)
                {
                    cache.Insert<T>(key, data, cacheTime);
                }
            }
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        public static void Insert(string key, object data, DateTime cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                //lock (cacheLocker)
                {
                    cache.Insert(key, data, cacheTime);
                }
            }
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        public static void Insert<T>(string key, T data, DateTime cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                //lock (cacheLocker)
                {
                    cache.Insert<T>(key, data, cacheTime);
                }
            }
        }

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        public static void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            lock (cacheLocker)
            {
                cache.Remove(key);
            }
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public static bool Exists(string key)
        {
            return cache.Exists(key);
        }

        /// <summary>
        /// 缓存消息队列发送消息
        /// </summary>
        public static void Send(string key ,object data)
        {
            cache.Send(key, data);
        }
        /// <summary>
        /// 注册订阅方法 
        /// </summary>
        public static void RegisterSubscribe<T>(string key, DoSub dosub)
        {
            cache.RegisterSubscribe<T>(key, dosub);
        }

        /// <summary>
        /// 注销订阅方法
        /// </summary>
        public static void UnRegisterSubscrib(string key)
        {
            cache.UnRegisterSubscrib(key);
        }

        /// <summary>
        /// 获取缓存锁（当使用分布式缓存时，具有分布式锁特性）
        /// 使用时，请务必释放返回的Locker对象，否则会造成对应
        /// 缓存持续阻塞
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public static ICacheLocker GetCacheLocker(string key)
        {
            return cache.GetCacheLocker(key);
        }


        /// <summary>
        /// 互斥写
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetNX(string key, string value)
        {
            return cache.SetNX(key, value);
        }

        /// <summary>
        /// 互斥写
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        /// <returns></returns>
        public static bool SetNX(string key, string value, int cacheTime)
        {
            return cache.SetNX(key, value, cacheTime);
        }

    }
}
