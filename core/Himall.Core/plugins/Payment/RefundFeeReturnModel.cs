using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Core.Plugins.Payment
{
    /// <summary>
    /// 退款执行方式
    /// </summary>
    public enum RefundRunMode
    {
        /// <summary>
        /// 同步完成
        /// <para>方法体内执行完成，直接返回退款结果</para>
        /// </summary>
        Sync=1,
        /// <summary>
        /// 异步完成
        /// <para>方法体只负责请求参数生成，由前台完成请求。支付平台会异步返回请求结果。</para>
        /// </summary>
        Async=2
    }
    public enum RefundState
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success=1,
        /// <summary>
        /// 失败
        /// </summary>
        Failure=-1
    }
    /// <summary>
    /// 退款操作结果模型
    /// </summary>
    public class RefundFeeReturnModel
    {
        /// <summary>
        /// 退款执行方式
        /// </summary>
        public RefundRunMode RefundMode { get; set; }
        /// <summary>
        /// 退款状态
        /// </summary>
        public RefundState RefundResult { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// 退款编号
        /// </summary>
        public long RefundId { get; set; }

        /// <summary>
        /// 退款流水号
        /// </summary>
        public string RefundNo { get; set; }

        /// <summary>
        /// 退款时间
        /// </summary>
        public DateTime RefundTime { get; set; }

        /// <summary>
        /// 主业务处理完成后响应内容
        /// 即当主程序相关订单状态完成后，需要响应请求的内容
        /// </summary>
        public string ResponseContentWhenFinished { get; set; }
        /// <summary>
        /// 退款请求方式
        /// <para>异步方式时使用</para>
        /// </summary>
        public UrlType RequestMode { get; set; }
    }
}
