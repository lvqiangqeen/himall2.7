using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 提现查询
    /// </summary>
    public class WithdrawQuery : BaseQuery
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long? ShopId { set; get; }

        /// <summary>
        /// 提现Id
        /// </summary>
        public long? Id { set; get; }

        /// <summary>
        /// 提现店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 申请提现区间开始时间
        /// </summary>
        public DateTime? ApplyStartTime { set; get; }

        /// <summary>
        /// 申请提现区间结束时间
        /// </summary>
        public DateTime? ApplyEndTime { set; get; }

        /// <summary>
        /// 提现审核区间开始时间
        /// </summary>
        public DateTime? AuditedStartTime { set; get; }

        /// <summary>
        /// 提现审核区间结束时间
        /// </summary>
        public DateTime? AuditedEndTime { set; get; }

        /// <summary>
        /// 提现状态
        /// </summary>
        public Himall.CommonModel.WithdrawStaus? Status { get; set; }
    }
}
