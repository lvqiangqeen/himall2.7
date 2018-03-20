using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
   
    /// <summary>
    /// 小程序订单提交页查询参数
    /// </summary>
    public class OrderSubmitPagePara
    {
        public string openId { get; set; }
        public WXSmallProgFromPageType fromPage { get; set; }
        public long shipAddressId { get; set; }
        /// <summary>
        /// 限时购编号
        /// </summary>
        public long countDownId { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public long buyAmount { get; set; }

        public string productSku { get; set; }
        /// <summary>
        /// 购物车IDS
        /// </summary>
        public string cartItemIds { get; set; }

    }
}
