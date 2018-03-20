using System.ComponentModel;

namespace Himall.Model
{
    public partial class MemberContactsInfo
    {

        public enum UserTypes
        {
            /// <summary>
            /// 普通用户
            /// </summary>
            [Description("普通用户")]
            General,

            /// <summary>
            /// 店铺用户
            /// </summary>
            [Description("店铺用户")]
            ShopManager,
        }

    }
}
