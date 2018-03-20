using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 虚拟订单实体
    /// </summary>
    public partial class VirtualOrderInfo
    {
        /// <summary>
        /// 虚拟订单Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 会员Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime{ get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount { get; set; }
        /// <summary>
        /// 支付编号
        /// </summary>
        public string PayNum { get; set; }
        /// <summary>
        /// 商家Id
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 收款标识
        /// </summary>
        public int MoneyFlag { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }
    }
}
