using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderRefundAfterSalePreCheckModel : BaseResultModel
    {
        public OrderRefundAfterSalePreCheckModel(bool status) : base(status)
        {
        }
        /// <summary>
        /// 原路退回
        /// </summary>
        public bool CanBackReturn { get; set; }
        /// <summary>
        /// 退到预存款
        /// </summary>
        public bool CanToBalance { get; set; }
        /// <summary>
        /// 原路退回
        /// </summary>
        public bool CanReturnOnStore { get; set; }
        /// <summary>
        /// 最大可退金额
        /// </summary>
        public string MaxRefundAmount { get; set; }
        /// <summary>
        /// 最大可退数量
        /// </summary>
        public long MaxRefundQuantity { get; set; }
        /// <summary>
        /// 单件金额
        /// </summary>
        public string oneReundAmount { get; set; }
        
        public List<RefundReasonInfo> RefundReasons { get; set; }

    }


}
