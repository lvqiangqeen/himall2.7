using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class RefundReturnSendGoodsPModel
    {
        public string openId { get; set; }
        public long ReturnsId { get; set; }
        public string express { get; set; }
        public string shipOrderNumber { get; set; }
        public string formId { get; set; }
        public string orderId { get; set; }
        public string skuId { get; set; }
    }
}
