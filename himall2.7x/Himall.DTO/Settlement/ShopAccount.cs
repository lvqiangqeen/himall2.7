using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺财务汇总
    /// </summary>
    public class ShopAccount
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
        /// 帐户余额
        /// </summary>
        public decimal Balance { set; get; }
        /// <summary>
        /// 待结算金额
        /// </summary>
        public decimal PendingSettlement { set; get; }

        /// <summary>
        /// 已结算金额
        /// </summary>
        public decimal Settled { set; get; }
    }
}
