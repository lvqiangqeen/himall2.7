using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 提现状态
    /// </summary>
    public enum WithdrawStaus
    {
        [Description("待处理")]
        WatingAudit = 1,

        [Description("拒绝提现")]
        Refused = 2,

        [Description("提现完成")]
        Succeed = 3,

        [Description("提现失败")]
        Fail = 4,
    }
}
