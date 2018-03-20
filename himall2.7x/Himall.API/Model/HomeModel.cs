using Himall.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    public class HomeModel
    {
    }
    public class APPHome
    {
        public string Success { get; set; }

        public string TotalProduct { get; set; }
        public IEnumerable<HomeSlides> Slide { get; set; }

        public IEnumerable<HomeSlides> Icon { get; set; }

        public IEnumerable<HomeImage> Topic { get; set; }

        public IEnumerable<HomeProduct> Product { get; set; }

        public IEnumerable<CustomerService> CustomerServices { get; set; }
    }

    public class HomeSlides
    {
        public string ImageUrl { get; set; }

        public string Url { get; set; }

        public string Desc { set; get; }
    }

    public class HomeImage
    {
        public string ImageUrl { get; set; }

        public string Url { get; set; }
    }

    public class HomeProduct
    {
        public string Id { get; set; }


        public string Name { get; set; }


        public string MarketPrice { get; set; }


        public string SalePrice { get; set; }

        public string ImageUrl { get; set; }

        public string CommentsCount { get; set; }
        public string Discount { get; set; }

        public string Url { get; set; }

    }
}
