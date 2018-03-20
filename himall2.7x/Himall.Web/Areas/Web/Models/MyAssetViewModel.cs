using Himall.IServices;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Himall.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    public class MyAssetViewModel
    {
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool isLogin { get; set; }
        /// <summary>
        /// 我关注的商品
        /// </summary>
        public List<FavoriteInfo> MyConcernsProducts { get; set; }
        /// <summary>
        /// 我的积分
        /// </summary>
        public int MyMemberIntegral { get; set; }
        /// <summary>
        /// 我的优惠券
        /// </summary>
        public List<UserCouponInfo> MyCoupons { get; set; }
        /// <summary>
        /// 我的随机红包
        /// </summary>
        public List<ShopBonusReceiveInfo> MyShopBonus { get; set; }
        /// <summary>
        /// 优惠券总数
        /// </summary>
        public int MyCouponCount { get; set; }
        /// <summary>
        /// 浏览商品记录
        /// </summary>
        public List<ProductBrowsedHistoryModel> MyBrowsingProducts { get; set; }
    }
}