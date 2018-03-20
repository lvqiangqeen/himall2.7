using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderSubmitOrderModel
    {
        public string skuIds { get; set; }
        public string counts { get; set; }
        public long recieveAddressId { get; set; }
        public string couponIds { get; set; }
        public int integral { get; set; }
        public bool isCashOnDelivery { get; set; }
        public int invoiceType { get; set; }
        public string invoiceTitle { get; set; }
        public string invoiceContext { get; set; }
        public string formId { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        public string orderRemarks { get; set; }
        /// <summary>
        /// 用户APP选择门店自提时用到
        /// </summary>
        public string  jsonOrderShops { get; set; }
    }
    /// <summary>
    /// 小程序订单提交Model
    /// </summary>
    public class SmallProgSubmitOrderModel
    {
        public string openId { get; set; }
        public WXSmallProgFromPageType fromPage { get; set; }
        public long shippingId { get; set; }
        /// <summary>
        /// 优惠券编码
        /// </summary>
        public string couponCode { get; set; }
        /// <summary>
        /// 限时购编号
        /// </summary>
        public long countDownId { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public long buyAmount { get; set; }

        public string productSku { get; set; }
        /// <summary>
        /// 购物车IDS
        /// </summary>
        public string cartItemIds { get; set; }

        public string remark { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public long deductionPoints { get; set; }
        /// <summary>
        /// 小程序formId（发送消息）
        /// </summary>
        public string formId { get; set; }

        public string jsonOrderShops { get; set; }
    }
}
