using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;
using Himall.Model;
using Himall.Core;

namespace Himall.DTO
{
    public class Order
    {
        public long Id { get; set; }

        public string OrderId { get { return Id.ToString(); } }

        public OrderInfo.OrderOperateStatus OrderStatus { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string CloseReason { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string SellerPhone { get; set; }
        public string SellerAddress { get; set; }
        public string SellerRemark { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string ExpressCompanyName { get; set; }
        public decimal Freight { get; set; }
        public string ShipOrderNumber { get; set; }
        public Nullable<System.DateTime> ShippingDate { get; set; }
        public bool IsPrinted { get; set; }
        public string PaymentTypeName { get; set; }
        public string PaymentTypeGateway { get; set; }
        public string GatewayOrderId { get; set; }
        public string PayRemark { get; set; }
        public Nullable<System.DateTime> PayDate { get; set; }
        public string InvoiceTitle { get; set; }
        public decimal Tax { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }
        public decimal ProductTotalAmount { get; set; }
        public decimal RefundTotalAmount { get; set; }
        public decimal CommisTotalAmount { get; set; }
        public decimal RefundCommisAmount { get; set; }
        public OrderInfo.ActiveTypes ActiveType { get; set; }
        public Himall.Core.PlatformType Platform { get; set; }
        public decimal DiscountAmount { get; set; }
        /// <summary>
        /// 满减优惠
        /// </summary>
        public decimal FullDiscount { set; get; }
        public InvoiceType InvoiceType { get; set; }
        public decimal IntegralDiscount { get; set; }
        public string InvoiceContext { get; set; }
        public Nullable<Himall.Model.OrderInfo.OrderTypes> OrderType { get; set; }
        public Himall.Model.OrderInfo.PaymentTypes PaymentType { get; set; }
        public Nullable<long> ShareUserId { get; set; }
        public string OrderRemarks { get; set; }
        public Nullable<System.DateTime> LastModifyTime { get; set; }
        public Himall.CommonModel.Enum.DeliveryType DeliveryType { get; set; }
        public Nullable<long> ShopBranchId { get; set; }
        public string PickupCode { get; set; }
        public Nullable<int> SellerRemarkFlag { get; set; }

        /// <summary>
        /// 订单商品总数
        /// </summary>
        public long OrderProductQuantity { get; set; }

        /// <summary>
        /// 订单退货总数
        /// </summary>
        public long OrderReturnQuantity { get; set; }

        /// <summary>
        /// 订单实付金额
        /// 公式： 商品应付+运费+税 - 优惠券金额 - 积分抵扣金额
        /// </summary>
        public decimal OrderTotalAmount { get; set; }

        /// <summary>
        /// 订单金额 （商品应付+运费+税 -优惠券金额） 不包含积分抵扣部分
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 商品实付（商品应付-优惠券的价格）
        /// </summary>
        public decimal ProductTotal { get; set; }


        ///// <summary>
        ///// 订单实付金额(转为数据库冗余字段)
        ///// </summary>
        //public decimal TotalAmount { get { return OrderTotalAmount; } }


        ///// <summary>
        ///// 订单实收金额（订单实付金额-退款）
        ///// </summary>
        //public decimal ActualPayAmount
        //{
        //    get { return OrderTotalAmount - RefundTotalAmount; }
        //}



        /// <summary>
        /// 订单实付金额(转为数据库冗余字段)
        /// </summary>
        public decimal TotalAmount { get; set; }


        /// <summary>
        /// 订单实收金额（订单实付金额-退款）(转为数据库冗余字段)
        /// </summary>
        public decimal ActualPayAmount
        {
            get;
            set;
        }



        /// <summary>
        /// 订单可退金额
        /// </summary>
        public decimal OrderEnabledRefundAmount { get; set; }

        /// <summary>
        /// 订单实际分佣
        /// </summary>
        public decimal CommisAmount { get; set; }

        /// <summary>
        /// 商家结算金额
        /// </summary>
        public decimal ShopAccountAmount { get; set; }

        /// <summary>
        /// 退款状态
        /// </summary>
        public int? RefundStats { get; set; }
        public string ShowRefundStats {get;set;        }

        /// <summary>
        /// 是否包含被删除的商品
        /// </summary>
        public bool HaveDelProduct { get; set; }

        /// <summary>
        /// 是否已过售后期
        /// <para>需要手动补充数据</para>
        /// </summary>
        public bool? IsRefundTimeOut { get; set; }

        /// <summary>
        /// 拼团订单的状态
        /// </summary>
        public FightGroupOrderJoinStatus? FightGroupOrderJoinStatus { get; set; }

        /// <summary>
        /// 拼团是否可退款状态
        /// </summary>
        public bool? FightGroupCanRefund { get; set; }

        /// <summary>
        /// 物流公司名称显示
        /// </summary>
        public string ShowExpressCompanyName { get; set; }


        /// <summary>
        /// 订单
        /// </summary>
        public string CreateTimeStr
        {
            get { return OrderDate.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public string OrderStatusStr { get { return this.OrderStatus.ToDescription(); } }


        public string PaymentTypeStr { get { return this.PaymentType.ToDescription(); } }

        public string PlatformStr { get { return this.Platform.ToDescription(); } }
    }
}
