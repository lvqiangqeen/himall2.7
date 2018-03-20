using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 会员积分范围
    /// </summary>
    public class GradeIntegralRange
    {
        /// <summary>
        /// 会员积分范围开始
        /// </summary>
        public int MinIntegral { set; get; }

        /// <summary>
        /// 会员积分范围结束
        /// </summary>
        public int MaxIntegral { set; get; }
    }

    /// <summary>
    /// 会员净消费金额范围
    /// </summary>
    public class GradeNetAmountRange
    {
        /// <summary>
        /// 会员消费范围开始
        /// </summary>
        public int MinNetAmount { set; get; }

        /// <summary>
        /// 会员消费范围结束
        /// </summary>
        public int MaxNetAmount { set; get; }
    }
}
