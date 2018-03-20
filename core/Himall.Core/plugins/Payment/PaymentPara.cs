using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Payment
{
    public class PaymentPara
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public long order_id { get; set; }
        /// <summary>
        /// 商户支付订单号
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 交易号
        /// </summary>
        public string pay_trade_no { get; set; }
        /// <summary>
        /// 商户退款单号
        /// </summary>
        public string out_refund_no { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal total_fee { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal refund_fee { get; set; }
        /// <summary>
        /// 返回接收地址
        /// </summary>
        public string return_url { get; set; }
        /// <summary>
        /// 返回异步通知地址
        /// </summary>
        public string notify_url { get; set; }

    }
    public class EnterprisePayPara
    {
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string out_trade_no { get; set; }
        bool checkname = false;
        /// <summary>
        /// 是否检测用户名
        /// </summary>
        public bool check_name
        {
            set
            {
                checkname = value;
            }
            get { return checkname; }
        }
        /// <summary>
        /// 检测的用户名
        /// </summary>
        public string re_user_name { set; get; }
        /// <summary>
        /// 付款金额
        /// </summary>
        public decimal amount { set; get; }
        /// <summary>
        /// 用户openid
        /// </summary>
        public string openid { set; get; }
        /// <summary>
        /// 企业付款描述信息
        /// </summary>
        public string desc { set; get; }
        /// <summary>
        /// 调用接口的机器Ip地址
        /// </summary>
        public string spbill_create_ip { set; get; }
    }
}
