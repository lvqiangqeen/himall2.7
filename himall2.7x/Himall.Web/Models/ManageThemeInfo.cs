using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web
{
    /// <summary>
    /// 样式管理实体类
    /// </summary>
    public class ManageThemeInfo
    {
        /// <summary>
        /// 样式的显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 样式名称
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        /// 样式图片
        /// </summary>
        public string ThemeImgUrl { get; set; }

    }
}