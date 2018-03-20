using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 入驻缴费记录
    /// </summary>
    public class SettledPayment
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 商家
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 缴费日期
        /// </summary>
        public string OperateDate { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperateType { get; set; }
        /// <summary>
        /// 缴纳金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 操作备注
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operate { get; set; }
    }
}
