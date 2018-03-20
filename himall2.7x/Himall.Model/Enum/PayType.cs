using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{

    /// <summary>
    /// Mobile支付类型
    /// </summary>
    public enum PayType
    {
        /// <summary>
        /// 支付宝支付
        /// </summary>
        [Description("支付宝")]
        AliPay = 0,
        /// <summary>
        /// 微信支付
        /// </summary>
        [Description("微信")]
        WxPay = 1,

        /// <summary>
        /// 预存款支付
        /// </summary>
        [Description("预存款")]
        CapitalPay = 9
    }
}
