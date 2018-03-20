using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    /// <summary>
    /// 门店账户
    /// </summary>
    public class ShopAccountModel
    {
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 上次结算金额
        /// </summary>
        public decimal LastSettlement { get; set; }

        /// <summary>
        /// 帐户余额
        /// </summary>
        public decimal Balance { set; get; }

        /// <summary>
        /// 已结算金额
        /// </summary>
        public decimal Settlement { get; set; }

        /// <summary>
        /// 待结算金额
        /// </summary>
        public decimal PeriodSettlement { get; set; }


        /// <summary>
        /// 上一次结算信息
        /// </summary>
        public Himall.DTO.SmipleAccount LastSettlementModel { set; get; }
        /// <summary>
        /// 是否显示提现
        /// </summary>
        public bool IsShowWithDraw { get; set; }
    }
}
