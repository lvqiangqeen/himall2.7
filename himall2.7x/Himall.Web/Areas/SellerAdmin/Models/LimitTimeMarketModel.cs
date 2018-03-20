using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class LimitTimeMarketModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// 审核状态码
        /// </summary>
        public int AuditStatusNum { get; set; }
        public string AuditStatus { get; set; }
        public System.DateTime AuditTime { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public decimal RecentMonthPrice { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Stock { get; set; }
        public int SaleCount { get; set; }
        public string CancelReson { get; set; }
        public int MaxSaleCount { get; set; }
        public string CategoryName { get; set; }
        public bool IsStarted { get; set; }
    }
}