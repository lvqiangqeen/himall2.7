using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class ProductItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal MarketPrice { get; set; }

        public decimal SalePrice { get; set; }
        public string ImageUrl { get; set; }
        public int CommentsCount { get; set; }

    }
}
