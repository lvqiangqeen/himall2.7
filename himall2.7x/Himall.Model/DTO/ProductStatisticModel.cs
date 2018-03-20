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
    public class ProductStatisticModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public Nullable<long> OrderCounts { get; set; }
        public long ShopId { get; set; }
        public long VisitUserCounts { get; set; }
        public long PayUserCounts { get; set; }
        /// <summary>
        /// 单品转化率
        /// </summary>
        public decimal SinglePercentConversion
        {
            get
            {
                decimal conversion = 0.00M;
                if (this.VisitUserCounts == 0)
                {//没有浏览人数，有付款人数
                    if (this.PayUserCounts > 0) conversion = 100.00M;
                }
                else
                {
                    conversion = Math.Round((decimal)this.PayUserCounts / this.VisitUserCounts * 100, 2);
                }
                return conversion;
            }
        }

        /// <summary>
        /// 导出时显示百分号
        /// </summary>
        public string SinglePercentConversionString
        {
            get
            {
                decimal conversion = SinglePercentConversion;
                string strConversion = conversion + "%";
                return strConversion;
            }
        }
    }
}
