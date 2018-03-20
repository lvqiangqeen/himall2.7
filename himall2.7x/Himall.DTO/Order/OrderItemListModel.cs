using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.DTO
{
    public class OrderItemListModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public long VShopId { get; set; }
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
        public CashDepositsObligation CashDepositsObligation { get; set; }
        public int? RefundStats { get; set; }
        public string ShowRefundStats { get; set; }
        public long? ItemRefundId { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
    }
}