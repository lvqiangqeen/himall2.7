using Himall.Core;
using System;
using System.Collections.Generic;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Configuration;

namespace Himall.Strategy
{
    public class Redis :ICache
    {
        int DEFAULT_TMEOUT = 600;//默认超时时间（单位秒）
        string address;
        JsonSerializerSettings jsonConfig = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
        ConnectionMultiplexer connectionMultiplexer;
        IDatabase database;
        ISubscriber sub;
        Dictionary<string,ISubscriber> subs = new Dictionary<string,ISubscriber>();//多个订阅

        class CacheObject<T>
        {
            public int ExpireTime { get; set; }
            public bool ForceOutofDate { get; set; }
            public T Value { get; set; }
        }

        public Redis()
        {
            this.address = ConfigurationManager.AppSettings["RedisServer"];

            if (this.address == null || string.IsNullOrWhiteSpace(this.address.ToString()))
                throw new ApplicationException("配置文件中未找到RedisServer的有效配置");
            connectionMultiplexer = ConnectionMultiplexer.Connect(address);
            database = connectionMultiplexer.GetDatabase();
            sub = connectionMultiplexer.GetSubscriber();
        }

        /// <summary>
        /// 连接超时设置
        /// </summary>
        public int TimeOut
        {
            get
            {
                return DEFAULT_TMEOUT;
            }
            set
            {
                DEFAULT_TMEOUT = value;
            }
        }

        public object Get(string key)
        {
            return Get<object>(key);
        }

        public T Get<T>(string key)
        {
  
            DateTime begin = DateTime.Now;
            var cacheValue = database.StringGet(key);
            DateTime endCache = DateTime.Now;
            var value = default(T);
            if (!cacheValue.IsNull)
            {
                var cacheObject = JsonConvert.DeserializeObject<CacheObject<T>>(cacheValue, jsonConfig);
                if (!cacheObject.ForceOutofDate)
                    database.KeyExpire(key, new TimeSpan(0, 0, cacheObject.ExpireTime));
                value = cacheObject.Value;
            }
            DateTime endJson = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis取数据时间:" + endCache.Subtract(begin).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(endCache).TotalMilliseconds + "毫秒,总耗时:"+endJson.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
            return value;
   
        }

        public void Insert(string key, object data)
        {
            var currentTime = DateTime.Now;
            var timeSpan = currentTime.AddSeconds(TimeOut) - currentTime;
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData(data, TimeOut, false);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData);
            DateTime endCache = DateTime.Now;
#if DEBUG
            Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds + "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }

        public void Insert(string key, object data, int cacheTime)
        {
            var currentTime = DateTime.Now;
            var timeSpan = TimeSpan.FromSeconds(cacheTime);
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData(data, TimeOut, true);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData, timeSpan);
            DateTime endCache = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds+ "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }

        public void Insert(string key, object data, DateTime cacheTime)
        {
            var currentTime = DateTime.Now;
            var timeSpan = cacheTime - DateTime.Now;
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData(data, TimeOut, true);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData, timeSpan);
            DateTime endCache = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds + "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }

        public void Insert<T>(string key, T data)
        {
            var currentTime = DateTime.Now;
            var timeSpan = currentTime.AddSeconds(TimeOut) - currentTime;
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData<T>(data, TimeOut, false);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData);
            DateTime endCache = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds + "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }

        public void Insert<T>(string key, T data, int cacheTime)
        {
            var currentTime = DateTime.Now;
            var timeSpan = TimeSpan.FromSeconds(cacheTime);
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData<T>(data, TimeOut, true);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData, timeSpan);
            DateTime endCache = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds + "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }

        public void Insert<T>(string key, T data, DateTime cacheTime)
        {
            var currentTime = DateTime.Now;
            var timeSpan = cacheTime - DateTime.Now;
            DateTime begin = DateTime.Now;
            var jsonData = GetJsonData<T>(data, TimeOut, true);
            DateTime endJson = DateTime.Now;
            database.StringSet(key, jsonData, timeSpan);
            DateTime endCache = DateTime.Now;
#if DEBUG
                Core.Log.Debug("redis插入数据时间:" + endCache.Subtract(endJson).TotalMilliseconds + "毫秒,转JSON时间:" + endJson.Subtract(begin).TotalMilliseconds + "毫秒,总耗时:"+endCache.Subtract(begin).TotalMilliseconds+"毫秒");
#endif
        }


        string GetJsonData(object data, int cacheTime, bool forceOutOfDate)
        {
            var cacheObject = new CacheObject<object>() { Value = data, ExpireTime = cacheTime, ForceOutofDate = forceOutOfDate };
             return JsonConvert.SerializeObject(cacheObject, jsonConfig);//序列化对象
        }

        string GetJsonData<T>(T data, int cacheTime, bool forceOutOfDate)
        {
            var cacheObject = new CacheObject<T>() { Value = data, ExpireTime = cacheTime, ForceOutofDate = forceOutOfDate };
            return JsonConvert.SerializeObject(cacheObject, jsonConfig);//序列化对象
        }

        public void Remove(string key)
        {
            database.KeyDelete(key, CommandFlags.HighPriority);
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public bool Exists (string key)
        {
            return database.KeyExists(key);
        }

        /// <summary>
        /// 缓存队列发送信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Send(string key,object data)
        {

            var currentTime = DateTime.Now;
            var timeSpan = currentTime.AddSeconds(TimeOut) - currentTime;
            string jsonData = GetJsonData(data, TimeOut, false);
            sub.Publish(key, jsonData);
        }

        /// <summary>
        /// 注册订阅方法 
        /// </summary>
        public void RegisterSubscribe<T>(string key, Cache.DoSub dosub)
        {
            ISubscriber isub = connectionMultiplexer.GetSubscriber();
            isub.Subscribe(key, (channel, message) => {
                var cacheObject = Recieve<T>((string)message);
                dosub(cacheObject);
            });
            foreach (var mkey in subs.Keys)
                if (mkey == key)
                    return;
            subs.Add(key,isub);
        }

        /// <summary>
        /// 注销订阅方法
        /// </summary>
        public void UnRegisterSubscrib(string key)
        {
            sub.Unsubscribe(key);
            subs.Remove(key);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private T Recieve<T>(string cachevalue)
        {
            var value = default(T);
            if (!string.IsNullOrEmpty(cachevalue))
            {
                var cacheObject = JsonConvert.DeserializeObject<CacheObject<T>>(cachevalue, jsonConfig);
                value = cacheObject.Value;
            }
            return value;
        }


        #region 缓存锁

        static Random Randmon = new Random();

        /// <summary>
        /// 获取缓存锁（当使用分布式缓存时，具有分布式锁特性）
        /// 使用时，请务必释放返回的Locker对象，否则会造成对应
        /// 缓存持续阻塞
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public ICacheLocker GetCacheLocker(string key)
        {
            key = string.Format("RedisLocker_{0}", key);
            var times = 1;
            while (!SetNX(key, "1", 10))
            {
                System.Threading.Thread.Sleep(Randmon.Next(50));//delay
                if (times++ > 100)//5秒
                    throw new ApplicationException(string.Format("获取锁{0}超时", key));
            }
            return new RedisCacheLocker(key);
        }

        class RedisCacheLocker : ICacheLocker
        {
            string lockerKey;

            static Redis redis = new Redis();

            public RedisCacheLocker(string key)
            {
                lockerKey = key;
            }

            public void Dispose()
            {
                redis.Remove(lockerKey);
            }
        }


        RedisKey GetRedisKey(RedisKey key)
        {
            return key;
        }

        public bool SetNX(string key, string value)
        {
            return database.StringSet(key, value, when: When.NotExists);
        }

        public bool SetNX(string key, string value, int cacheTime)
        {
            return database.StringSet(key, value, when: When.NotExists, expiry: new TimeSpan(0, 0, cacheTime));
        }


        #endregion

    }
}
