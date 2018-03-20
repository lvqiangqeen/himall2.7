using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices.QueryModel
{
    public partial class CouponQuery : QueryBase
    {
        public string CouponName { get; set; }
        /// <summary>
        /// Null表示取所有
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 显示平台
        /// </summary>
        public Himall.Core.PlatformType? ShowPlatform { get; set; }
        /// <summary>
        /// 仅显示正常
        /// </summary>
        public bool? IsOnlyShowNormal { get; set; }
        /// <summary>
        /// 是否显示所有
        /// <para>默认仅显示正常优惠券</para>
        /// </summary>
        public bool? IsShowAll { get; set; }
    }
}
