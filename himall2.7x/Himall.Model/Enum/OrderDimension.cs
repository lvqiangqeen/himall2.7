using System.ComponentModel;

namespace Himall.Model
{
    public enum OrderDimension : int
    {
        /// <summary>
        /// 下单客户数
        /// </summary>
        [Description("下单客户数")]
        OrderMemberCount = 1,

        /// <summary>
        /// 下单量
        /// </summary>
        [Description("下单量")]
        OrderCount,

        /// <summary>
        /// 下单金额
        /// </summary>
        [Description("下单金额")]
        OrderMoney
    }
}
