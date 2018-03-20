using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class ProductSkuModel
    {
        public long productId { set; get; }

        public string ProductName { set; get; }

        public string ImgUrl { set; get; }
        public List<SKUModel> SKUs { set; get; }
    }

    public class SKUModel
    {
        public  string Id { set; get; }
        public long ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Version { get; set; }
        public string Sku { get; set; }
        public long Stock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public long AutoId { get; set; }
    }
}
