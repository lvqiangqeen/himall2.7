using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public partial class SellerQuery:QueryBase
    {
        public IEnumerable<long> Ids { get; set; }
        public string ShopName { get; set; }

        public long? ShopId { get; set; }
        public int? RegionId { get; set; }

        public int? NextRegionId { get; set; }
    }
}
