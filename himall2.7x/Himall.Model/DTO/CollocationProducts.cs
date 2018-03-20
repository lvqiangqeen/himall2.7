using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class CollocationProducts
    {
        public long ProductId { get; set; }

        public long ColloPid { set; get; }
        public string ProductName { set; get; }

        public bool IsMain { get; set; }

        public long Stock { set; get; }

        public decimal MinSalePrice { set; get; }
        public decimal MaxSalePrice { set; get; }

        public decimal MinCollPrice { set; get; }

        public decimal MaxCollPrice { set; get; }

        public string Image { set; get; }
        public int DisplaySequence { get; set; }

        public bool IsShowSku { get; set; }
    }
}
