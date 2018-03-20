using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 店铺结算历史查询
    /// </summary>
    public class ShopSettledHistoryQuery : SettledHistoryQuery
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }  
    }

    /// <summary>
    /// 结算历史查询
    /// </summary>
    public class SettledHistoryQuery : BaseQuery
    {
        /// <summary>
        /// 查询在这个时间之后的历史记录
        /// </summary>
        public DateTime MinSettleTime { set; get; }
    }
}
