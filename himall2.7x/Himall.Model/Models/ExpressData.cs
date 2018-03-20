using System;
using System.Collections.Generic;

namespace Himall.Model
{
    /// <summary>
    /// 快递实时数据
    /// </summary>
    public class ExpressData
    {
        public ExpressData()
        {
            ExpressDataItems = new ExpressDataItem[0];
        }

        /// <summary>
        /// 是否查询数据成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 快递查询返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 快递物流详细信息（仅Success为True时有效）
        /// </summary>
        public IEnumerable<ExpressDataItem> ExpressDataItems { get; set; }

    }

    /// <summary>
    /// 快递实时数据
    /// </summary>
    public class ExpressDataItem
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }



}
