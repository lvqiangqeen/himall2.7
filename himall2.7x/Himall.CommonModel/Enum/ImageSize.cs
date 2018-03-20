using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 图片大小
    /// </summary>
    public enum ImageSize : int
    {
        /// <summary>
        /// 50×50
        /// </summary>
        [Description("50×50")]
        Size_50 = 50,

        /// <summary>
        /// 100×100
        /// </summary>
        [Description("100×100")]
        Size_100 = 100,

        /// <summary>
        /// 150×150
        /// </summary>
        [Description("150×150")]
        Size_150 = 150,

        /// <summary>
        /// 220×220
        /// </summary>
        [Description("220×220")]
        Size_220 = 220,

        /// <summary>
        /// 350×350
        /// </summary>
        [Description("350×350")]
        Size_350 = 350,
        /// <summary>
        /// 400×400
        /// </summary>
        [Description("400×400")]
        Size_400 = 400,

        /// <summary>
        /// 500×500
        /// </summary>
        [Description("500×500")]
        Size_500 = 500

    }
}
