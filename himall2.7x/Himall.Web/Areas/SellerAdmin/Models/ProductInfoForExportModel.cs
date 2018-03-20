using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ProductInfoForExportModel
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }

        //public long TypeId { get; set; }
        public string BrandName { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public ProductInfo.ProductSaleStatus SaleStatus { get; set; }
        public System.DateTime AddedDate { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public ProductInfo.ProductAuditStatus AuditStatus { get; set; }

        public string AuditReason { get; set; }
        //public Nullable<decimal> Weight { get; set; }
        //public Nullable<decimal> Volume { get; set; }
        public int? Quantity { get; set; }
        public string MeasureUnit { get; set; }
        //public int EditStatus { get; set; }

        public ICollection<SKUInfo> SKUInfo { get; set; }
    }
}