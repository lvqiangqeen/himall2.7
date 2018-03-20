using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 申请提现实体
    /// </summary>
    public class ShopWithDraw
    {
        /// <summary>
        /// 提现金额
        /// </summary>
        public decimal WithdrawalAmount { set; get; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public WithdrawType WithdrawType { set; get; }

        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 提现操作人ID
        /// </summary>
        public long SellerId { set; get; }

        /// <summary>
        /// 提现操作人用户名
        /// </summary>
        public string SellerName { set; get; }
    }
}
