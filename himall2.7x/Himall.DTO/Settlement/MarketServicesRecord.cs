using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 营销服务费用购买记录
    /// </summary>
    public class MarketServicesRecord
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Price { set; get; }

        /// <summary>
        /// 购买周期
        /// </summary>
        public string BuyingCycle { set; get; }

        /// <summary>
        /// 营销服务类型
        /// </summary>
        public string MarketType { set; get; }

        /// <summary>
        /// 购买日期
        /// </summary>
        public string BuyTime { set; get; }

    }
}
