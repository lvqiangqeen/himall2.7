using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 消息队列事件接口
    /// </summary>
    public interface IMessageQueueEvent<T>
    {
        /// <summary>
        /// 消息到达事件
        /// </summary>
        event MessageRecievedHandle<T> OnMessageRecieved;
    }
}
