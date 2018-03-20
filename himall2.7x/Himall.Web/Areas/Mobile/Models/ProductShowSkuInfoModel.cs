using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;
using Himall.Web.Areas.Web.Models;

namespace Himall.Web.Areas.Mobile.Models
{
    public class ProductShowSkuInfoModel
    {
        public ProductShowSkuInfoModel()
        {
            Color = new CollectionSKU();
            Size = new CollectionSKU();
            Version = new CollectionSKU();
        }

        public string ProductImagePath { get; set; }
        public decimal MinSalePrice { get; set; }
        public string MeasureUnit { get; set; }
		public int MaxBuyCount { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
    }
}