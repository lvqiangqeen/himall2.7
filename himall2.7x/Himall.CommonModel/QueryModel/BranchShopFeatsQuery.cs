using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 门店搜索
    /// </summary>
    public partial class BranchShopFeatsQuery : BaseQuery
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }
    }
}
