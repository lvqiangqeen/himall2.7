using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
    public class MessageOrderInfo
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderId { set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { set; get; }
        /// <summary>
        /// 购买品类
        /// </summary>
        public long ItemNumber { set; get; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public long Quantity { set; get; }
        /// <summary>
        /// 总价格
        /// </summary>
        public decimal TotalMoney { set; get; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentType { set; get; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public string PaymentStatus { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public string ShippingCompany { set; get; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string ShippingNumber { set; get; }
        /// <summary>
        /// 收货人
        /// </summary>
        public string ShipTo { get; set; }
        /// <summary>
        /// 商城名称
        /// </summary>
        public string SiteName { set; get; }
        /// <summary>
        /// 售后对应OrderItem
        /// <para>0表示为订单整笔退款</para>
        /// </summary>
        public long RefundOrderItemId { get; set; }
        /// <summary>
        /// 售后编号
        /// </summary>
        public long RefundId { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundMoney { set; get; }
        /// <summary>
        /// 退货数量
        /// </summary>
        public long RefundQuantity { set; get; }
        /// <summary>
        /// 售后申请时间
        /// </summary>
        public DateTime RefundTime { get; set; }
        /// <summary>
        /// 售后审核时间
        /// </summary>
        public DateTime RefundAuditTime { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// 订单商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Remark { get; set; }
    }

    public class orderItem
    {


    }
}
