using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 微信场景类型
    /// </summary>
    public enum QR_SCENE_Type : int
    {
        /// <summary>
        /// 提现
        /// </summary>
        [Description("提现")]
        WithDraw = 1,

        /// <summary>
        /// 红包领取场景，用于客户关注平台公众号
        /// </summary>
        [Description("红包")]
        Bonus = 2 ,

        /// <summary>
        /// 限时购开团提醒推送消息
        /// </summary>
        [Description( "限时购开团提醒" )]
        FlashSaleRemind = 3 ,

        /// <summary>
        /// 绑定微信
        /// </summary>
        [Description("绑定微信")]
        Binding = 4 
    }
}
