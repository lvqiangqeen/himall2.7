using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 提现方式
    /// </summary>
    public enum WithdrawType
    {
        [Description("微信")]
        WeiChat = 1,

        [Description("银行卡")]
        BankCard = 2
    }
}
