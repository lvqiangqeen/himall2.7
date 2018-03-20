using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinStatisticsService.Model
{
    public class OrderUser:BaseModel
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 当天订单数
        /// </summary>
        public int OrderNumber { get; set; }
    }
}
