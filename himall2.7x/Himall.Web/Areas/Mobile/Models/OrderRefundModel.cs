using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class OrderRefundModel
    {
        public long MaxRGDNumber { get; set; }

        public decimal MaxRefundAmount { get; set; }

        public Model.OrderItemInfo Item { get; set; }

        public string UserName { get; set; }

        public string Phone { get; set; }

        public Model.OrderInfo OrderInfo { get; set; }

        public long? OrderItemId { get; set; }

        public string RefundWay { get; set; }

        public int BackOut { get; set; }
    }
}