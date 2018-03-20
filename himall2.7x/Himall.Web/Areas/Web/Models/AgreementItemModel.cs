using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class AgreementItemModel
    {
        public int Id { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal SalePrice { get; set; }

        public int Quantity { get; set; }

        public string SkuId { get; set; }

        public string Annex { get; set; }

        public string Unit { get; set; }

        public string SKU { get; set; }

        public decimal? CostPrice { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Version { get; set; }

        public string ThumbnailsUrl { get; set; }

    }
}