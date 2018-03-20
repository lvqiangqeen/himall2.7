
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace Himall.Model
{
    public partial class MenuInfo
    {


        /// <summary>
        /// 链接类型
        /// </summary>
        public enum UrlTypes
        {
            [Description("")]
            /// <summary>
            /// 不链接
            /// </summary>
            Nothing=0,

            [Description("首页")]
            /// <summary>
            /// 商城首页
            /// </summary>
            ShopHome=1,

            [Description("微店")]
            /// <summary>
            /// 微店
            /// </summary>
            WeiStore=2,

            [Description("分类")]
            /// <summary>
            /// 商城分类
            /// </summary>
            ShopCategory=3,

            [Description("个人中心")]
            /// <summary>
            /// 个人中心
            /// </summary>
            MemberCenter=4,

            [Description("购物车")]
            /// <summary>
            /// 购物车
            /// </summary>
            ShopCart=5,
            /// <summary>   
            /// 链接
            /// </summary>
            [Description("")]
            Linkage=6
        }



    }
}
