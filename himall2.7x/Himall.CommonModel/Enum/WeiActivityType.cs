using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Himall.CommonModel
{
    /// <summary>
    /// 微信活动类型
    /// </summary>
    public enum WeiActivityType:int
    {
        /// <summary>
        /// 刮刮卡
        /// </summary>
        [Description("刮刮卡")]
        ScratchCard=0,

        /// <summary>
        /// 大转盘
        /// </summary>
        [Description("大转盘")]
        Roulette=1

    }
}
