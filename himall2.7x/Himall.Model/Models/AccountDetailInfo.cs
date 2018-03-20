using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class AccountDetailInfo
    {
        /// <summary>
        /// 订单状态
        /// </summary>
        public enum EnumOrderType
        {
            /// <summary>
            /// 退订单
            /// </summary>
            [Description("退单列表")]
            ReturnOrder = 0,

            /// <summary>
            /// 已完成
            /// </summary>
            [Description("订单列表")]
            FinishedOrder
        }

       
    }
}
