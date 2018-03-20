using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum ShopBusinessType
    {
        /// <summary>
        /// 企业入驻
        /// </summary>
        [Description("企业入驻")]
        Enterprise = 0,

        /// <summary>
        /// 个人入驻
        /// </summary>
        [Description("个人入驻")]
        Personal = 1,
    }
}
