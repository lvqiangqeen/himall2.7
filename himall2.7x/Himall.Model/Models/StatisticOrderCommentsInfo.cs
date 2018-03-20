using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class StatisticOrderCommentsInfo
    {
        /// <summary>
        /// 结算状态
        /// </summary>
        public enum EnumCommentKey
        {
            /// <summary>
            /// 宝贝与描述相符 商家得分
            /// </summary>
            [Description("宝贝与描述相符 商家得分")]
            ProductAndDescription = 1,

            /// <summary>
            /// 宝贝与描述相符 同行业平均分
            /// </summary>
            [Description("宝贝与描述相符 同行业平均分")]
            ProductAndDescriptionPeer ,

            /// <summary>
            /// 宝贝与描述相符 同行业商家最高得分
            /// </summary>
            [Description("宝贝与描述相符 同行业商家最高得分")]
            ProductAndDescriptionMax ,

            /// <summary>
            /// 宝贝与描述相符 同行业商家最低得分
            /// </summary>
            [Description("宝贝与描述相符 同行业商家最低得分")]
            ProductAndDescriptionMin,
            /// <summary>
            /// 卖家发货速度 商家得分
            /// </summary>
            [Description("卖家发货速度 商家得分")]
            SellerDeliverySpeed,

            /// <summary>
            /// 卖家发货速度 同行业平均分
            /// </summary>
            [Description("卖家发货速度 同行业平均分")]
            SellerDeliverySpeedPeer,

            /// <summary>
            /// 卖家发货速度 同行业商家最高得分
            /// </summary>
            [Description("卖家发货速度 同行业商家最高得分")]
            SellerDeliverySpeedMax,

            /// <summary>
            /// 卖家发货速度 同行业商家最低得分
            /// </summary>
            [Description("卖家发货速度 同行业商家最低得分")]
            SellerDeliverySpeedMin,

            /// <summary>
            /// 卖家服务态度 商家得分
            /// </summary>
            [Description("卖家服务态度 商家得分")]
            SellerServiceAttitude ,

            /// <summary>
            /// 卖家服务态度 同行业平均分
            /// </summary>
            [Description("卖家服务态度 同行业平均分")]
            SellerServiceAttitudePeer ,

            /// <summary>
            /// 卖家服务态度 同行业商家最高得分
            /// </summary>
            [Description("卖家服务态度 同行业商家最高得分")]
            SellerServiceAttitudeMax ,

            /// <summary>
            /// 卖家服务态度 同行业商家最低得分
            /// </summary>
            [Description("卖家服务态度 同行业商家最低得分")]
            SellerServiceAttitudeMin,



        }

        
    }
}
