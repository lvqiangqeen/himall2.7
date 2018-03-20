using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Himall.CommonModel
{
    public enum TopicAlign : int
    {
        /// <summary>
        /// 积分
        /// </summary>
        [Description("左对齐")]
        Left = 0,

        /// <summary>
        /// 红包
        /// </summary>
        [Description("居中")]
        Center = 1,

        /// <summary>
        /// 优惠卷
        /// </summary>
        [Description("右对齐")]
        Right = 2,
    }
}
