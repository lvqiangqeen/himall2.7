using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 商品统计
    /// </summary>
    public class ProductCategoryStatisticModel
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public Nullable<long> OrderCounts { get; set; }
        public long ShopId { get; set; }
        /// <summary>
        /// 金额份额
        /// </summary>
        decimal amountRate = 0.00M;
        public decimal AmountRate
        {
            get
            {
                return amountRate;
            }
            set
            {
                amountRate = value;
            }
        }
        /// <summary>
        /// 销售量份额
        /// </summary>
        decimal countRate = 0.00M;
        public decimal CountRate { get { return countRate; } set { countRate = value; } }

    }
}
