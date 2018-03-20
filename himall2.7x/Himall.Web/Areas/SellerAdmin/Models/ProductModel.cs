
using Himall.Model;
using System.Collections.Generic;
namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ProductModel
    {
        public long Id { get; set; }


        public decimal Price { get; set; }

        public string Image { get; set; }

        public string QrCode { get; set; }
        public string Name { get; set; }

        public long Stock { get; set; }

        public string PublishTime { get; set; }

        public string BrandName { get; set; }
        public long BrandId
        {
            get;
            set;
        }
        public long CategoryId
        {
            get;
            set;
        }


        public string CategoryName { get; set; }

        public string Url { get; set; }

        public int AuditState { get; set; }

        public int SaleState { get; set; }

        public string AuditReason { get; set; }

        public string ProductCode { get; set; }
        public bool IsLimitTimeBuy { get; set; }
        /// <summary>
        /// 是否超出安全库存
        /// </summary>
        public bool IsOverSafeStock { get; set; }
        public long Uid { get; set; }

        public long SaleCount { get; set; }

        public string Unit { get; set; }

        public ICollection<SKUInfo> SKUInfo { get; set; }

		public string RelationProducts { get; set; }

        /// <summary>
        /// 最大购买数
        /// </summary>
        public int MaxBuyCount { get; set; }
	}
}