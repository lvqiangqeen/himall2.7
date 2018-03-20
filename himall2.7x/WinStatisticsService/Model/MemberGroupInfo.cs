using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatisticsService.Model
{
    /// <summary>
    /// 会员非组统计数据表
    /// </summary>
    public partial class MemberGroupInfo : BaseModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public Enum.MemberStatisticsType StatisticsType { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }

   
    }
}
