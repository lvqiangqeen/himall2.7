using System.Collections.Generic;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopHomeModuleBasicModel
    {

        public long Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<long> ProductIds { get; set; }


        public int DisplaySequence { get; set; }

        public bool IsEnable { get; set; }
    }
}