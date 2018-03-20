using Himall.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Himall.Web.Areas.Web.Models
{
    public class WebFloorProduct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Pic { get; set; }
    }
    public class HomeFloorModel
    {
        public HomeFloorModel()
        {
            Brands = new List<WebFloorBrand>();
            TextLinks = new List<WebFloorTextLink>();
            Products = new List<WebFloorProductLinks>();
            Tabs = new List<Tab>();
            ProductModules = new List<ProductModule>();
        }
        public string Name { get; set; }
        public long Index { get; set; }
        public long Id { get; set; }
        public string SubName { set; get; }
        public long StyleLevel
        {
            set;
            get;
        }
        public string DefaultTabName
        {
            get;
            set;
        }

        public List<WebFloorBrand> Brands { get; set; }

        public List<WebFloorTextLink> TextLinks { get; set; }

        public List<WebFloorProductLinks> Products { get; set; }

        /// <summary>
        /// 推荐商品
        /// </summary>
        public List<ProductModule> ProductModules { get; set; }

        public List<WebFloorProductLinks> Scrolls
        {
            get;
            set;
        }
         
        public List<WebFloorProductLinks> RightTops
        {
            get; 
            set;
        }

        public List<WebFloorProductLinks> RightBottons 
        { 
            get;
            set;
        }

        public List<Tab> Tabs
        {
            get;
            set;
        }
        

        public class WebFloorTextLink
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string Url { get; set; }

        }

        public class Tab
        {
            public long Id
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public List<ProductDetail> Detail
            {
                get;
                set;
            }
        }


        public class ProductDetail
        {
            public long ProductId
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public string ImagePath
            {
                get;
                set;
            }

            public decimal Price
            {
                get;
                set;
            }
        }



        public class WebFloorProductLinks
        {
            public long Id { get; set; }

            public string ImageUrl { get; set; }

            public string Url { get; set; }

            public Position Type
            {
                get;
                set;
            }

        }


        public class ProductModule
        {
            /// <summary>
            /// 主键
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// 标签
            /// </summary>
            public int Tab { get; set; }

            /// <summary>
            /// 商品ID
            /// </summary>
            public long ProductId { get; set; }

            /// <summary>
            /// 商品名称
            /// </summary>
            public string productName { get; set; }

            /// <summary>
            /// 商品头像
            /// </summary>
            public string productImg { get; set; }

            /// <summary>
            /// 商品价格
            /// </summary>
            public decimal price { get; set; }

            /// <summary>
            /// 市场价
            /// </summary>
            public decimal MarketPrice { get; set; }
        }
    }
}