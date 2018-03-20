using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatisticsService.Model
{
    public  class NewOrderInfo : BaseModel
    {
        public long Id { get; set; }
        public int OrderStatus { get; set; }
        public long ShopId { get; set; }
        public long FranchiseeId { get; set; }
        public long SalesId { get; set; }
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public int RegionId { get; set; }
        public decimal Freight { get; set; }
        public int InvoiceType { get; set; }
        public string InvoiceTitle { get; set; }
        public string InvoiceContent { get; set; }
        public decimal Tax { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }
        public decimal ProductTotalAmount { get; set; }
        public int ActiveType { get; set; }
        public int Platform { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal IntegralDiscount { get; set; }
        public int OrderType { get; set; }
        public int DeliveryType { get; set; }
        public int PickingStatus { get; set; }
        public string CloseReason { get; set; }
        public string ShopName { get; set; }
        public string SellerPhone { get; set; }
        public string SellerAddress { get; set; }
        public string SellerRemark { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
        public Nullable<System.DateTime> ShippingDate { get; set; }
        public Nullable<bool> IsPrinted { get; set; }
        public string PaymentTypeName { get; set; }
        public string PaymentTypeGateway { get; set; }
        public Nullable<int> PaymentType { get; set; }
        public string GatewayOrderId { get; set; }
        public string PayRemark { get; set; }
        public Nullable<System.DateTime> PayDate { get; set; }
        public Nullable<decimal> RefundTotalAmount { get; set; }
        public Nullable<decimal> CommisTotalAmount { get; set; }
        public Nullable<decimal> RefundCommisAmount { get; set; }
        public Nullable<long> ShareUserId { get; set; }
        public Nullable<System.DateTime> LastModifyTime { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public decimal ActualAmount { get; set; }
        public Nullable<long> VerifierId { get; set; }
        public string VerifierName { get; set; }
        public Nullable<System.DateTime> VerificationTime { get; set; }
        public string PickupCode { get; set; }
        public decimal TotalAmount { get; set; }
        public Nullable<int> CommentStatus { get; set; }
    }
}
