using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class ShippingAddressOperaAddressPModel
    {
        public string openId { get; set; }
        public long shippingId { get; set; }
        public int regionId { get; set; }
        public string address { get; set; }
        public string cellphone { get; set; }
        public string shipTo { get; set; }
        public bool isDefault { get; set; }
        public string city { get; set; }
        public string county { get; set; }
    }
}
