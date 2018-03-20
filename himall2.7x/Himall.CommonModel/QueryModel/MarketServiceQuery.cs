using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class MarketServiceQuery:BaseQuery
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long? ShopId { set; get; }
        /// <summary>
        /// 营销服务类型
        /// </summary>
        public int? MarketType { set; get; }

        /// <summary>
        /// 购买时间区间开始
        /// </summary>
        public DateTime? BuyTimeStart{ set; get; }

        /// <summary>
        /// 购买时间区间结束
        /// </summary>
        public DateTime? BuyTimeEnd { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

    }
}
