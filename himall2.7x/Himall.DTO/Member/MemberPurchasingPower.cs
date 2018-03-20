using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 会员购买力度
    /// </summary>
    public class MemberPurchasingPower
    {
        /// <summary>
        /// 会员ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 登陆用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string GradeName { set; get; }


        /// <summary>
        /// 净消费金额（排除退款后的消费金额）
        /// </summary>
        public decimal NetAmount { set; get; }

        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// 最后消费时间
        /// </summary>
        public DateTime? LastConsumptionTime { get; set; }

        /// <summary>
        /// 最近3次购买类别
        /// </summary>
        public string CategoryNames { get; set; }
        /// <summary>
        /// 会员手机
        /// </summary>
        public string CellPhone { get; set; }
    }
}
