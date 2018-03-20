using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class ProductBrowsedHistoryModel
    {
        public long ProductId { set; get; }
 
        public decimal ProductPrice { set; get; }

        public string ProductName { set; get; }

        public string ImagePath { set; get; }

        public DateTime BrowseTime { set; get; }

        public long UserId { set; get; }

        public long ShopId { get; set; }
    }
}