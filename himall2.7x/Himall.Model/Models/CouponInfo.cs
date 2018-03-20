using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace Himall.Model
{
    public partial class CouponInfo
    {
        /// <summary>
        /// 领取方式
        /// </summary>
        public enum CouponReceiveType
        {
            /// <summary>
            /// 店铺首页
            /// </summary>
            [Description("店铺首页")]
            ShopIndex = 0,
            /// <summary>
            /// 积分兑换
            /// </summary>
            [Description("积分兑换")]
            IntegralExchange = 1,
            /// <summary>
            /// 主动发放
            /// </summary>
            [Description("主动发放")]
            DirectHair = 2
        }
        /// <summary>
        /// 领取状态
        /// </summary>
        public enum CouponReceiveStatus
        {
            Success=1,
            /// <summary>
            /// 已过期
            /// </summary>
            HasExpired=2,
            /// <summary>
            /// 已超额
            /// <para>达个人领取上限</para>
            /// </summary>
            HasLimitOver = 3,
            /// <summary>
            /// 已领完
            /// <para>优惠券已领完</para>
            /// </summary>
            HasReceiveOver=4,
            /// <summary>
            /// 积分不足
            /// </summary>
            IntegralLess=5
        }
        public string ShowIntegralCover
        {
            get
            {
                string result = "";
                if (this != null)
                {
                    result =Himall.Core.HimallIO.GetRomoteImagePath(this.IntegralCover);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        if (this.Himall_Shops != null)
                        {
                            result =Himall.Core.HimallIO.GetRomoteImagePath(this.Himall_Shops.Logo);
                        }
                    }
                }
                if(string.IsNullOrWhiteSpace(result))
                {
                    result = "";
                }
                return result;
            }
        }
        /// <summary>
        /// 微信关联信息
        /// </summary>
        [NotMapped]
        public WXCardLogInfo WXCardInfo { get; set; }

        #region 表单传参
        /// <summary>
        /// 是否同步微信
        /// <para>仅限表单使用</para>
        /// </summary>
        [NotMapped]
        public bool FormIsSyncWeiXin { get; set; }
        [NotMapped]
        public string FormWXColor { get; set; }
        [NotMapped]
        public string FormWXCTit { get; set; }
        [NotMapped]
        public string FormWXCSubTit { get; set; }
        /// <summary>
        /// 是否可以发布到微店首页
        /// </summary>
        [NotMapped]
        public bool CanVshopIndex { get; set; }
        #endregion
    }
}
