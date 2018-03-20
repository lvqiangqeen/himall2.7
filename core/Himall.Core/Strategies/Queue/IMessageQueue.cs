using Himall.Core.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 消息队列
    /// </summary>
    public interface IMessageQueue : IStrategy
    {
        /// <summary>
        /// 列队地址
        /// </summary>
        string Address
        {
            set;
        }
        /// <summary>
        /// 向消息队列发送消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <param name="timeDelay">消息延时(毫秒)</param>
        void SendMessage(string queueName, object message, long timeDelay = 0L, string filterProperty = null);
        /// <summary>
        /// 向消息队列发送订阅主题并接收第一个返回对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        T PublishTopicWithFirstReply<T>(string queueName, object message, string filterProperty = null);
        /// <summary>
        /// 向消息队列发送消息并接收返回对象
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns>消息被消费后的返回结果</returns>
        T SendMessageWithReply<T>(string queueName, object message, string filterProperty = null);
        /// <summary>
        /// 发布队列订阅主题
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message"></param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <param name="timeDelay">消息延时(毫秒)</param>
        void PublishTopic(string queueName, object message, long timeDelay = 0L, string filterProperty = null);
        /// <summary>
        /// 监听对列消息
        /// </summary>
        /// <param name="queueName">对列名称</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        IMessageQueueEvent<T> ListenQueue<T>(string queueName, string filterProperty = null);
        /// <summary>
        /// 监听订阅消息
        /// </summary>
        /// <param name="queueName">对列名称</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        IMessageQueueEvent<T> ListenTopic<T>(string queueName, string filterProperty = null);
    }

}
