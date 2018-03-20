using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.DTO;

namespace Himall.Web.Areas.Mobile.Models
{
    public class FightGroupShowDetailModel
    {
        public FightGroupActiveModel ActiveData { get; set; }
        /// <summary>
        /// 成团时限处显示形式
        /// </summary>
        public int LimitedHourShowType { get; set; }
        /// <summary>
        /// 组团结束时间
        /// </summary>
        public DateTime EndBuildGroupTime { get; set; }
        /// <summary>
        /// 当前用户的最低商品价
        /// </summary>
        public decimal ProductMiniPriceByUser { get; set; }
    }
}