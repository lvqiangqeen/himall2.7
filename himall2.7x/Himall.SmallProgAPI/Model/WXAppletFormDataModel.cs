using Himall.Core.Plugins.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class WXAppletFormDataModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 事件ID
        /// </summary>
        public MessageTypeEnum EventId { get; set; }
        /// <summary>
        /// 事件值
        /// </summary>
        public string EventValue { get; set; }
        /// <summary>
        /// 事件的表单ID
        /// </summary>
        public string FormId { get; set; }
        /// <summary>
        /// 事件时间
        /// </summary>
        public DateTime EventTime { get; set; }
        /// <summary>
        /// FormId过期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }
    }
}
