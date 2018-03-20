using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum MatchType
    {
        /// <summary>
        /// 完全匹配
        /// </summary>
        [Description("完全匹配")]
        Full = 0,

        /// <summary>
        /// 模糊匹配
        /// </summary>
        [Description("模糊匹配")]
        Like = 1,
    }
}
