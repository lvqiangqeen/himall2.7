using System;
using System.Collections.Generic;

namespace Himall.Core.Plugins.Payment
{
    /// <summary>
    /// 支付事件
    /// </summary>
    public class PaymentInfo
    {

        /// <summary>
        /// 支付订单号
        /// </summary>
        public IEnumerable<long> OrderIds { get; set; }

        /// <summary>
        /// 支付流水号
        /// </summary>
        public string TradNo { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime? TradeTime { get; set; }

        /// <summary>
        /// 主业务处理完成后响应内容
        /// 即当主程序相关订单状态完成后，需要响应请求的内容
        /// </summary>
        public string ResponseContentWhenFinished { get; set; }

    }
}
