using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;

namespace Himall.ViewModel
{
    public class OrderModel
    {
        public long OrderId { set; get; }

        public string OrderStatus { get; set; }

        public string OrderDate { get; set; }
        
        public long ShopId { set; get; }

        public string ShopName { set; get; }

        public long UserId { set; get; }

        public string UserName { set; get; }

        public decimal TotalPrice { get; set; }

        public string PaymentTypeName { get; set; }

        public int PlatForm { get; set; }

        public string PlatformText { get; set; }

        public string IconSrc { get; set; }

        public int? RefundStats { get; set; }

        public string PaymentTypeGateway { get; set; }

        public int OrderState { get; set; }

        public DateTime? PayDate { get; set; }

        public string PaymentTypeStr { get; set; }

        public OrderInfo.PaymentTypes PaymentType { get; set; }
    }
}