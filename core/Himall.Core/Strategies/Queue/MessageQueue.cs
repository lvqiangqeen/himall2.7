using Himall.Core;
using System;
using System.Configuration;
using System.Web.Configuration;
namespace Himall.Core
{
    /// <summary>
    /// 消息队列
    /// </summary>
    public class MessageQueue
    {
        private static object cacheLocker;
        private static IMessageQueue imessageQueue;
        private static bool IsDebug;
        static MessageQueue()
        {
            MessageQueue.cacheLocker = new object();
            MessageQueue.imessageQueue = null;
            MessageQueue.IsDebug = false;
            MessageQueue.Init();
            MessageQueue.Load();
        }
        private static void Init()
        {
            CompilationSection compilationSection = ConfigurationSettings.GetConfig("system.web/compilation") as CompilationSection;
            if (compilationSection != null)
            {
                MessageQueue.IsDebug = compilationSection.Debug;
            }
        }
        /// <summary>
        /// 加载队列策略
        /// </summary>
        private static void Load()
        {
            try
            {
                imessageQueue = ObjectContainer.Current.Resolve<IMessageQueue>();

            }
            catch (Exception ex)
            {
                throw new  QueueRegisterException("注册队列服务异常", ex);
            }

            MessageQueue.imessageQueue.Address = ConfigurationManager.AppSettings["MQServer"];
        }
        private static string ProcessQueueName(string queueName)
        {
            if (MessageQueue.IsDebug)
            {
                queueName = string.Format("{0}_{1}", Himall.Core.Helper.NetworkHelper.GetAddressIP(), queueName);
            }
            return queueName;
        }
        /// <summary>
        /// 向消息队列发送消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message"></param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <param name="timeDelay">消息延时(毫秒)</param>
        public static void SendMessage(string queueName, object message, long timeDelay = 0L, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            imessageQueue.SendMessage(queueName, message, timeDelay, filterProperty);
        }
        /// <summary>
        /// 向消息队列发送消息并接收返回对象
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns>消息被消费后的返回结果</returns>
        public static T SendMessageWithReply<T>(string queueName, object message, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            return imessageQueue.SendMessageWithReply<T>(queueName, message, filterProperty);
        }
        /// <summary>
        /// 向消息队列发送订阅主题并接收第一个返回对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        public static T PublishTopicWithFirstReply<T>(string queueName, object message, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            return MessageQueue.imessageQueue.PublishTopicWithFirstReply<T>(queueName, message, filterProperty);
        }
        /// <summary>
        /// 发布队列订阅主题
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message"></param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <param name="timeDelay">消息延时(毫秒)</param>
        public static void PublishTopic(string queueName, object message, long timeDelay = 0L, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            MessageQueue.imessageQueue.PublishTopic(queueName, message, timeDelay, filterProperty);
        }
        /// <summary>
        /// 监听队列消息
        /// </summary>
        /// <param name="queueName">对列名称</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        public static IMessageQueueEvent<T> ListenQueue<T>(string queueName, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            return MessageQueue.imessageQueue.ListenQueue<T>(queueName, filterProperty);
        }
        /// <summary>
        /// 监听队列消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="filterProperty">过滤属性，用于筛选消息</param>
        /// <returns></returns>
        public static IMessageQueueEvent<T> ListenTopic<T>(string queueName, string filterProperty = null)
        {
            queueName = ProcessQueueName(queueName);
            return MessageQueue.imessageQueue.ListenTopic<T>(queueName, filterProperty);
        }
    }
}
