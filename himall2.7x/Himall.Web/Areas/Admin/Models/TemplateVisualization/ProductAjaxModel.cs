using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class ProductAjaxModel
    {
        public int status { get; set; }
        public List<ProductContent> list { get; set; }
        public string page { get; set; }
        public bool showPrice { get; set; }
        public bool showIco { get; set; }
        public bool showName { get; set; }
        public bool showWarp { get; set; }
        public string warpId { get; set; }
    }
    public class ProductContent
    {
        public long product_id { get; set; }
        public long item_id { get; set; }
        public string title { get; set; }
        public string create_time { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        /// <summary>
        /// SalePrice
        /// </summary>
        public string price { get; set; }
        /// <summary>
        /// MarketPrice
        /// </summary>
        public string original_price { get; set; }
        public string is_compress { get; set; }
        public bool is_limitbuy { get; set; }
        /// <summary>
        /// 销量
        /// </summary>
        public long SaleCounts { get; set; }
    }
}