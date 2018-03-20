using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class HomeBrands
    {
        public HomeBrands()
        {
            listBrands = new List<WebFloorBrand>();
        }

        public ImageAdInfo imageAdinroLeft { get; set; }
        public ImageAdInfo imageAdinroRight { get; set; }

        public List<WebFloorBrand> listBrands { get; set; }
    }
}