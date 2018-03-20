using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Himall.Web.Areas.Admin.Models
{
    public class HomeFloorDetail
    {
        public HomeFloorDetail()
        {
            Brands = new Collection<Brand>();
            TextLinks = new Collection<TextLink>();
            ProductLinks = new Collection<TextLink>();
            ProductModules = new Collection<ProductModule>();
        }

        public long StyleLevel
        {
            get;
            set;
        }

        public long Id { get; set; }

        public string DefaultTabName
        {
            get;
            set;
        }

        public IEnumerable<Category> Categories { get; set; }

        public CategoryImage CategoryImg { get; set; }

        public IEnumerable<Brand> Brands { get; set; }

        public IEnumerable<Tab> Tabs
        { 
            get; 
            set;
        }
        public IEnumerable<TextLink> TextLinks { get; set; }
        public IEnumerable<TextLink> ProductLinks { get; set; }
        public IEnumerable<ProductModule> ProductModules { get; set; }

        public string Name { get; set; }
        public string SubName { get; set; }

        public class Category
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public int Depth { get; set; }

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

            public int Count
            {
                get;
                set;
            }

            public string Ids
            {
                get;
                set;
            }

            public IEnumerable<ProductDetail> Detail
            {
                get; 
                set;
            }
        }


        public class ProductDetail
        {
            public long Id
            {
                get;
                set;
            }

            public long ProductId
            {
                get;
                set;
            }
        }

        public class CategoryImage
        {
            public long Id { get; set; }

            public string ImageUrl { get; set; }

            public string Url { get; set; }
        }


        public class Brand
        {
            public long Id { get; set; }

            public string Name { get; set; }
        }


        public class TextLink
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string Url { get; set; }

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

        }

    }
}