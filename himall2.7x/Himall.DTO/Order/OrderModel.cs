using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.CommonModel;
using Himall.CommonModel.Enum;

namespace Himall.DTO
{

    public class OrderListModel
    {
        public long Id { get; set; }
        public OrderInfo.OrderOperateStatus OrderStatus { get; set; }
        public int? RefundStats { get; set; }
        public long? OrderRefundId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string CloseReason { get; set; }
        public long ShopId { get; set; }
        public long VShopId { get; set; }
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
        public OrderInfo.OrderTypes? OrderType { get; set; }
        public Himall.Core.PlatformType Platform { get; set; }
        public decimal DiscountAmount { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public decimal IntegralDiscount { get; set; }
        public string InvoiceContext { get; set; }
        /// <summary>
        /// 满减优惠
        /// </summary>
        public decimal FullDiscount { get; set; }

        public decimal OrderTotalAmount { get; set; }
        public int CommentCount { get; set; }
        public IEnumerable<OrderItemListModel> OrderItemList { get; set; }

        public ShopBonusGrantInfo ReceiveBonus { get; set; }

        public OrderInfo.PaymentTypes PaymentType { get; set; }

        public CommentStatus CommentStatus { get; set; }

        public bool HasAppendComment { set; get; }
        /// <summary>
        /// 拼团状态
        /// <para>拼团订单独有</para>
        /// </summary>
        public FightGroupOrderJoinStatus FightGroupJoinStatus { get; set; }
        /// <summary>
        /// 是否可退款
        /// <para>只做简单逻辑判断</para>
        /// </summary>
        public bool OrderCanRefund { get; set; }
        /// <summary>
        /// 拼团是否可以退款
        /// </summary>
        public bool FightGroupCanRefund { get; set; }
        /// <summary>
        /// 可通金额
        /// </summary>
        public Nullable<decimal> EnabledRefundAmount { get; set; }
        /// <summary>
        /// 是否已过售后期
        /// </summary>
        public bool IsRefundTimeOut { get; set; }
        /// <summary>
        /// 提货码
        /// </summary>
        public string PickupCode { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        public long ShopBranchId { get; set; }
        /// <summary>
        /// 门店联系人
        /// </summary>
        public string ShopBranchContactUser { get; set; }
        /// <summary>
        /// 门店电话
        /// </summary>
        public string ShopBranchContactPhone { get; set; }
        /// <summary>
        /// 门店地址
        /// </summary>
        public string ShopBranchAddress { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public DeliveryType DeliveryType { get; set; }
    }

    public class OrderDetailView
    {

        public OrderInfo Order { get; set; }
        public OrderDetail Detail { get; set; }

        public ShopBonusGrantInfo Bonus { get; set; }

        public string ShareHref { get; set; }

        public bool IsRefundTimeOut { get; set; }
        /// <summary>
        /// 拼团状态
        /// <para>拼团订单独有</para>
        /// </summary>
        public FightGroupOrderJoinStatus FightGroupJoinStatus { get; set; }
        /// <summary>
        /// 拼团是否可退款
        /// </summary>
        public bool FightGroupCanRefund { get; set; }

        public string Log { get; set; }
    }

    public class OrderDetail
    {
        public long ShopId { get; set; }
        public long VShopId { get; set; }

        public string ShopName { get; set; }
        public int? RefundStats { get; set; }
        public long? OrderRefundId { get; set; }

        public IEnumerable<OrderItem> OrderItems { get; set; }
    }


    public class OrderItem
    {
		public long Id { get; set; }
		public long OrderId { get; set; }
		public long ShopId { get; set; }
		public long ProductId { get; set; }
		public string SkuId { get; set; }
		public string SKU { get; set; }
		public long Quantity { get; set; }
		public long ReturnQuantity { get; set; }
		public decimal CostPrice { get; set; }
		public decimal SalePrice { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal RealTotalPrice { get; set; }
		public decimal RefundPrice { get; set; }
		public string ProductName { get; set; }
		public string Color { get; set; }
		public string Size { get; set; }
		public string Version { get; set; }
		public string ThumbnailsUrl { get; set; }
		public decimal CommisRate { get; set; }
		public Nullable<decimal> EnabledRefundAmount { get; set; }
		public bool IsLimitBuy { get; set; }
		public Nullable<decimal> DistributionRate { get; set; }
		public Nullable<decimal> EnabledRefundIntegral { get; set; }
		public decimal CouponDiscount { get; set; }

		public string ProductCode { get; set; }

		public long FreightId { set; get; }

		//总共的运费
		public decimal Freight { set; get; }

		public decimal Price { get; set; }

		public long Count { get; set; }

		public string ProductImage { get; set; }
		public string Unit { get; set; }

		public bool IsCanRefund { get; set; }

		public int? RefundStats { get; set; }
        public string ShowRefundStats { get; set; }
		public long? OrderRefundId { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
    }


}