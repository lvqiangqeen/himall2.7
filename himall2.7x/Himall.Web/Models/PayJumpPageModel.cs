using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Core;

namespace Himall.Web.Models
{
    /// <summary>
    /// 支付页返回模板
    /// </summary>
    public class PayJumpPageModel
    {
        public string PaymentId { get; set; }
        public string OrderIds { get; set; }
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// 支付流水号
        /// </summary>
        public long PayId { get; set; }
        public Himall.Core.Plugins.Payment.UrlType UrlType { get; set; }
        public string RequestUrl { get; set; }

        /// <summary>
        /// 是否出错
        /// </summary>
        public bool IsErro { get; set; }

        public string ErroMsg { set;get;}
    }
}