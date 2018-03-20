using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 消息到达委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="args">消息参数</param>
    public delegate void MessageRecievedHandle<T>(object sender, MQArgs<T> args);

    
    /// <summary>
    /// 消息队列参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MQArgs<T> : EventArgs
    {
        public MQArgs()
        {
            this.Data = default(T);
        }
        public MQArgs(T t)
        {
            this.Data = t;
        }
       /// <summary>
       /// 消息内容
       /// </summary>
        public T Data { get; private set; }
      
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnData { get; set; }
    }
}
