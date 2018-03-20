using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    public class TopShopModel
    {
        public string Success { get; set; }
        public bool IsFavorite { get; set; }
        public string ShopName { get; set; }
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string VShopId { get; set; }

        public string ShopId { get; set; }
        public string ShopLogo { get; set; }
        public IEnumerable<HomeProduct> Products { get; set; }

        public string Url { get; set; }
    }
}
