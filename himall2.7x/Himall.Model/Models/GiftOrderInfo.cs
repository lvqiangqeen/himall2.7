using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Himall.Core;

namespace Himall.Model
{
    public partial class GiftOrderInfo
    {
        public string UserName { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public enum GiftOrderStatus
        {
            /// <summary>
            /// 待付款
            /// </summary>
            //[Description("待付款")]
            //WaitPay = 1,
            /// <summary>
            /// 待发货
            /// </summary>
            [Description("待发货")]
            WaitDelivery = 2,
            /// <summary>
            /// 待收货
            /// </summary>
            [Description("待收货")]
            WaitReceiving = 3,
            /// <summary>
            /// 已关闭
            /// </summary>
            //[Description("已关闭")]
            //Close = 4,
            /// <summary>
            /// 已完成
            /// </summary>
            [Description("已完成")]
            Finish = 5
        }
        /// <summary>
        /// 显示订单状态
        /// </summary>
        [NotMapped]
        public string ShowOrderStatus
        {
            get
            {
                return this.OrderStatus.ToDescription();
            }
        }
        /// <summary>
        /// 物流公司名称显示
        /// </summary>
        [NotMapped]
        public string ShowExpressCompanyName
        {
            get
            {
                string result = this.ExpressCompanyName;
                if (result == "-1")
                {
                    result = "其他";
                }
                return result;
            }
        }
    }
}
