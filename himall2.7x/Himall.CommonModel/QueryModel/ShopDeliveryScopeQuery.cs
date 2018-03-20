using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 门店配送范围查询参数
    /// </summary>
    public class ShopDeliveryScopeQuery: BaseQuery
    {
        /// <summary>
        /// 门店表ID
        /// </summary>
        public long ShopBranchId { get; set; }

        public List<int> RegionIdList { get; set; }

        public int RegionId { get; set; }
    }
}
