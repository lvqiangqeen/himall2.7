using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺提现明细
    /// </summary>
    public class ShopWithDrawItem
    {
        /// <summary>
        /// 退款ID
        /// </summary>
        public long Id { set; get; }
        /// <summary>
        /// 处理时间
        /// </summary>
        public string DealTime { set;get;}

        /// <summary>
        /// 申请时间
        /// </summary>
        public string ApplyTime { set; get; }

        /// <summary>
        /// 提现商家用户ID
        /// </summary>
        public long SellerId { set; get; }

        /// <summary>
        /// 提现商家用户名
        /// </summary>
        public string SellerName { set; get; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public string CashAmount { set; get; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public string CashType { set;get;}

        /// <summary>
        /// 提现帐户
        /// </summary>
        public string Account { set;get;}

        /// <summary>
        /// 提现人
        /// </summary>
        public string AccountName { set; get; }

        /// <summary>
        /// 提现店铺Id
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 提现店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 状态
        /// </summary>
        public int WithStatus { get; set; }

        /// <summary>
        /// 提现状态
        /// </summary>
        public string Status { set; get; }

        /// <summary>
        /// 提现流水号（微信等第三方提现返回）
        /// </summary>
        public long AccountNo { set; get; }

        /// <summary>
        /// 提现商家备注
        /// </summary>
        public string ShopRemark { set; get; }

        /// <summary>
        /// 提现平台备注
        /// </summary>
        public string PlatRemark { set; get; }
    }
}
