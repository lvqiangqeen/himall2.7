using Himall.Core.Strategies;
using System;
using System.Collections.Generic;
using Himall.Core;

namespace Himall.Core
{
    public interface ICache : IStrategy
    {
        /// <summary>
        /// 缓存过期时间
        /// </summary>
        int TimeOut { set; get; }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        object Get(string key);
        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);

        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        //void Clear();

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        void Insert(string key, object data);

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        void Insert<T>(string key, T data);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        void Insert(string key, object data, int cacheTime);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        void Insert<T>(string key, T data, int cacheTime);


        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        void Insert(string key, object data, DateTime cacheTime);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间</param>
        void Insert<T>(string key, T data, DateTime cacheTime);

        /// <summary>
        /// 互斥写
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetNX(string key, string value);

        /// <summary>
        /// 互斥写
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        /// <returns></returns>
        bool SetNX(string key, string value, int cacheTime);

        /// <summary>
        /// 缓存队列发送信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void Send(string key, object data);

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        bool Exists(string key);


        /// <summary>
        /// 注册订阅方法 
        /// </summary>
        void RegisterSubscribe<T>(string key, Cache.DoSub dosub);

        /// <summary>
        /// 注销订阅方法
        /// </summary>
        void UnRegisterSubscrib(string key);


        /// <summary>
        /// 获取缓存锁（当使用分布式缓存时，具有分布式锁特性）
        /// 使用时，请务必释放返回的Locker对象，否则会造成对应
        /// 缓存持续阻塞
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        ICacheLocker GetCacheLocker(string key);
    }
}
