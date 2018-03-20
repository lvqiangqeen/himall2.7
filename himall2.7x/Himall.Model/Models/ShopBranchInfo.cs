using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 门店分部类
    /// </summary>
    public partial class ShopBranchInfo
    {
        /// <summary>
        /// 门店距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 门店是否可用
        /// </summary>
        public bool Enabled { get; set; }
    }
}
