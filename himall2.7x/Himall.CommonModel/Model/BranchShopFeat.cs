using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class BranchShopFeat
    {
        public long ShopId { get; set; }
        public long BranchShopId { set; get; }
        public string BranchShopName { set; get; }
        public decimal SaleAmount { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// 门店业绩
    /// </summary>
    public class BranchShopDayFeat
    {
        public long BranchShopId { set; get; }
        /// <summary>
        /// 销售额
        /// </summary>
        public decimal SaleAmount { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Day { get; set; }
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
    }
}
