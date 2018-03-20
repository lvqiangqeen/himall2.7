using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum AdminCatalogType
    {
        /// <summary>
        /// 默认导航,显示在导航
        /// </summary>
        [Description("默认导航")]
        Default = 0,

        /// <summary>
        /// 内部导航，不做导航显示
        /// </summary>
        [Description("内部导航")]
        Internal = 1
    }
}
