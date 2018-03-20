using System.Collections.Generic;

namespace Himall.Web.Areas.Mobile
{

    public class OrderDetail
    {
        public long ShopId { get; set; }
        public long VShopId { get; set; }

        public string ShopName { get; set; }
        public int? RefundStats { get; set; }
        public long? OrderRefundId { get; set; }

        public IEnumerable<OrderItem> OrderItems { get; set; }
    }


    public class OrderItem
    {
        public long Id { get; set; }
        public long ProductId { get; set; }

        public decimal Price { get; set; }

        public long Count { get; set; }

        public string ProductName { get; set; }

        public string ProductImage { get; set; }
        public string Unit { get; set; }

        public bool IsCanRefund { get; set; }


        public object Color { get; set; }

        public object Size { get; set; }

        public object Version { get; set; }
        public int? RefundStats { get; set; }
        public long? OrderRefundId { get; set; }
    }


}