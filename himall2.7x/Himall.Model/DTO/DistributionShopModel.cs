using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class DistributionShopModel
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long GradeId { get; set; }
        public string ShopName { get; set; }
        public string SubDomains { get; set; }
        public string Theme { get; set; }
        public bool IsSelf { get; set; }
        public ShopInfo.ShopAuditStatus ShopStatus { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string CompanyName { get; set; }
        public int CompanyRegionId { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public long VShopId { get; set; }
        public int? SaleSum { get; set; }
        public int? ProductCount { get; set; }
        [NotMapped]
        public List<DistributionProductModel> ProductList { get; set; }
    }

    public class DistributionProductModel
    {
        public long Id { get; set; }
        public Nullable<long> ProductId { get; set; }
        public decimal rate { get; set; }
        public Nullable<int> SaleNum { get; set; }
        public Nullable<int> AgentNum { get; set; }
        public Nullable<int> ForwardNum { get; set; }
        public ProductBrokerageInfo.ProductBrokerageStatus Status { get; set; }
        public Nullable<int> Sort { get; set; }
        public decimal saleAmount { get; set; }
        public decimal BrokerageAmount { get; set; }
        public decimal BrokerageTotal { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public decimal MinSalePrice { get; set; }
        [NotMapped]
        /// <summary>
        /// 佣金
        /// </summary>
        public decimal Commission
        {
            get
            {
                decimal result = 0;
                decimal DistributorRate = (decimal)rate;
                if (DistributorRate > 0)
                {
                    result = (MinSalePrice * DistributorRate / 100);
                    int _tmp = (int)(result * 100);
                    //保留两位小数，但不四舍五入
                    result = (decimal)(((decimal)_tmp) / (decimal)100);
                }
                return result;
            }
        }
    }
}
