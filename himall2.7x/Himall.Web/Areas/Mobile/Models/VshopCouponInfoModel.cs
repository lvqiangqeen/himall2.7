using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile.Models
{
    public class VshopCouponInfoModel
    {
        public long CouponId { get; set; }
        public long CouponRecordId { get; set; }
        public long ShopId { get; set; }
        public long VShopid { get; set; }
        public bool IsFavoriteShop { get; set; }
        public CouponInfo.CouponReceiveStatus CouponStatus { get; set; }
        public bool IsShowSyncWeiXin { get; set; }
        public string WeiXinReceiveUrl { get; set; }
        public string FollowUrl { get; set; }
        public CouponInfo CouponData { get; set; }
        public int? AcceptId { get; set; }

        #region 微信JSSDK参数
        public WXSyncJSInfoByCard WXJSInfo { get; set; }
        public WXJSCardModel WXJSCardInfo { get; set; }
        #endregion
    }
}