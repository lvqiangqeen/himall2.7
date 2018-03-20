using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.IServices.QueryModel
{
    public partial class ProductQuery : QueryBase
    {
        public long? CategoryId { get; set; }

        public string BrandNameKeyword { get; set; }

        public ProductInfo.ProductAuditStatus[] AuditStatus { get; set; }

        public ProductInfo.ProductSaleStatus? SaleStatus { get; set; }

        public string KeyWords { get; set; }

        public IEnumerable<long> Ids { get; set; }

        /// <summary>
        /// 排除的商品ID
        /// </summary>
        public IEnumerable<long> ExceptIds { set; get; }

        public string ShopName { get; set; }

        public long? ShopId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public long? ShopCategoryId { get; set; }

        public bool IsLimitTimeBuy { get; set; }

        public string ProductCode { get; set; }
        public int OrderKey { get; set; }
        public bool OrderType { get; set; }
        /// <summary>
        /// 超出警戒库存
        /// </summary>
        public bool OverSafeStock { get; set; }
        /// <summary>
        /// 不包含草稿箱
        /// </summary>
        public bool NotIncludedInDraft { get; set; }
        /// <summary>
        /// 要过滤的门店ID
        /// </summary>
        public long? ShopBranchId { get; set; }
    }

    public class ProductSearch
    {
        public string Keyword { get; set; }
        public string Ex_Keyword { get; set; }
        public long BrandId { get; set; }
        public List<string> AttrIds { get; set; }
        public long CategoryId { get; set; }
        public int OrderKey { get; set; }
        public bool OrderType { get; set; }
        public long shopId { get; set; }
        /// <summary>
        /// 要过滤的门店ID
        /// </summary>
        public long? shopBranchId { get; set; }
        public decimal startPrice { get; set; }
        public decimal EndPrice { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public long? ShopCategoryId { get; set; }
    }
}
