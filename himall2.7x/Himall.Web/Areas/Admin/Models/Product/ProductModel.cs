
namespace Himall.Web.Areas.Admin.Models.Product
{
    public class ProductModel
    {
        public long id { get; set; }
        public string name { get; set; }
        public string categoryName { get; set; }
        public string brandName { get; set; }
        public decimal price { get; set; }
        public string state { get; set; }
        public string imgUrl { get; set; }
        public int auditStatus { get; set; }
        public string url { get; set; }
        public string auditReason { get; set; }

        public string shopName { get; set; }
        /// <summary>
        /// 销量
        /// </summary>
        public long saleCounts { get; set; }
        public int saleStatus { get; set; }

        public string productCode { get; set; }
        public string AddedDate { get; set; }
	}
}