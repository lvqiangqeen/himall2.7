using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class ShopServiceMarkModel
    {
        /// <summary>
        /// 包装评分 = 包装+物流
        /// </summary>
        public decimal PackMark { get; set; }

        /// <summary>
        /// 服务评分
        /// </summary>
        public decimal ServiceMark { get; set; }

        public decimal ComprehensiveMark { get; set; }
    }
}