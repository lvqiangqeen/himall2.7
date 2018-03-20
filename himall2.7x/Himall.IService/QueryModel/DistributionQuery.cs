using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public class DistributionQuery : QueryBase
    {

        public string ProductName { set; get; }

        public ProductBrokerageInfo.ProductBrokerageStatus? Status { get; set; }

        public string ShopName { set; get; }
        public long? ShopId { get; set; }

    }
}
