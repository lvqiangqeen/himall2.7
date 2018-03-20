using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{

    /// <summary>
    /// 主题类别枚举
    /// </summary>
    public enum ThemeType : int
    {
        /// <summary>
        /// 默认主题
        /// </summary>
        [Description("默认主题")]
        Defaults = 0,
        /// <summary>
        /// 自定义主题
        /// </summary>
        [Description("自定义主题")]
        Customize = 1
    }

}
