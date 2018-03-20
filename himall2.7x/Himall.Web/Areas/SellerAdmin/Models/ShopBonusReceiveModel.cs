using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopBonusReceiveModel
    {
        public long Id { get; set; }

        public string OpenId { get; set; }

        public decimal Price { get; set; }

        public string StateStr { get; set; }

        public string ReceiveTime { get; set; }

        public string UsedTime { get; set; }

        public string UsedOrderId { get; set; }
    }
}