using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using Himall.Core;
using Himall.Model;

namespace Himall.Model
{
    /// <summary>
    /// 分销业绩模型
    /// </summary>
    public class DistributionFeatModel
    {
        /// <summary>
        /// 流水编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public long? OrderId { get; set; }
        /// <summary>
        /// 订单项编号
        /// </summary>
        public long? OrderItemId { get; set; }
        /// <summary>
        /// 所属店铺
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 购买人
        /// </summary>
        public long BuyUserId { get; set; }
        public long ProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 货品编号
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 货品
        /// </summary>
        public string SkuInfo { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductImage { get; set; }
        /// <summary>
        /// 订单可得佣金
        /// </summary>
        public decimal Brokerage { get; set; }
        /// <summary>
        /// 订单项金额
        /// </summary>
        public decimal OrderItemPrice { get; set; }
        /// <summary>
        /// 结算状态
        /// </summary>
        public BrokerageIncomeInfo.BrokerageStatus SettleState { get; set; }
        /// <summary>
        /// 结算状态(显示)
        /// </summary>
        public string ShowSettleState
        {
            get
            {
                return SettleState.ToDescription();
            }
        }
        /// <summary>
        /// 是否已结算
        /// </summary>
        public bool IsSettled
        {
            get
            {
                return SettleState == BrokerageIncomeInfo.BrokerageStatus.Settled;
            }
        }
        /// <summary>
        /// 流水产生时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 最后维权期
        /// </summary>
        public DateTime? LastRightsTime { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShippingDate { get; set; }
        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime? SettleTime { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SalesName { get; set; }
        /// <summary>
        /// 销售员编号
        /// </summary>
        public long SalesUserId { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderInfo.OrderOperateStatus OrderState { get; set; }
        /// <summary>
        /// 订单状态(显示)
        /// </summary>
        public string ShowOrderState
        {
            get
            {
                return OrderState.ToDescription();
            }
        }
        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// 是否有退款
        /// </summary>
        public bool IsHaveRefund
        {
            get
            {
                return RefundPrice > 0;
            }
        }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal? RefundPrice { get; set; }
        /// <summary>
        /// 退还佣金
        /// </summary>
        public decimal? RefundBrokerage { get; set; }
        /// <summary>
        /// 退款时间
        /// </summary>
        public DateTime? RefundTime { get; set; }
        /// <summary>
        /// 计算后可获得佣金
        /// </summary>
        public decimal CanBrokerage
        {
            get
            {
                decimal result = Brokerage;
                if (RefundBrokerage.HasValue)
                {
                    result = result - RefundBrokerage.Value;
                }
                return result;
            }
        }
    }
}
