using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class OrderRefundListModel
    {
        public long Vshopid { get; set; }
        public Model.OrderRefundInfo RefundInfo { get; set; }

        public string ShopName { get; set; }


    }
}