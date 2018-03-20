using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderSubmitOrderByCartModel
    {
        public string cartItemIds { get; set; }
        public long recieveAddressId { get; set; }
        public string couponIds { get; set; }
        public int integral { get; set; }
        public bool isCashOnDelivery { get; set; }
        public int invoiceType { get; set; }
        public string invoiceTitle { get; set; }
        public string formId { get; set; }
        public string invoiceContext { get; set; }
        /// <summary>
        /// 用户APP选择门店自提时用到
        /// </summary>
        public string jsonOrderShops { get; set; }
    }
}
