using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class LimitTimeBuyModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
        public string ProductId { get; set; }
        public string Status { get; set; }
        public string ShopName { get; set; }
        public string Price { get; set; }
        public string RecentMonthPrice { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SaleCount { get; set; }
    }
}