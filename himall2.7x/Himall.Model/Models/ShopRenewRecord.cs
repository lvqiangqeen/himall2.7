using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class ShopRenewRecord
    {
        /// <summary>
        /// 续费类型
        /// </summary>
        public enum EnumOperateType
        {
            /// <summary>
            /// 续费当前套餐
            /// </summary>
            [Description("续费当前套餐")]
            ReNew = 1,

            /// <summary>
            /// 升级套餐
            /// </summary>
            [Description("升级套餐")]
            Upgrade
        }


        /// <summary>
        /// 续费或者升级时支付返回的流水号
        /// </summary>
        [NotMapped]
        public string TradeNo
        {
            set;
            get;
        }

       
    }
}
