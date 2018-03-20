using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    /// <summary>
    /// 积分商城首页页面模型
    /// </summary>
    public class IntegralMallPageModel
    {
        /// <summary>
        /// 优惠券列表
        /// <para>第一页数据</para>
        /// </summary>
        public List<CouponInfo> CouponList { get; set; }
        /// <summary>
        /// 优惠券每页显示量
        /// </summary>
        public int CouponPageSize { get; set; }
        /// <summary>
        /// 优惠券总数
        /// </summary>
        public int CouponTotal { get; set; }
        /// <summary>
        /// 优惠券总页数
        /// </summary>
        public int CouponMaxPage { get; set; }
        /// <summary>
        /// 礼品列表
        /// <para>第一页数据</para>
        /// </summary>
        public List<GiftModel> GiftList { get; set; }
        /// <summary>
        /// 礼品每页显示量
        /// </summary>
        public int GiftPageSize { get; set; }
        /// <summary>
        /// 礼品总数
        /// </summary>
        public int GiftTotal { get; set; }
        /// <summary>
        /// 礼品总页数
        /// </summary>
        public int GiftMaxPage { get; set; }
        /// <summary>
        /// 当前用户可用积分
        /// </summary>
        public int MemberAvailableIntegrals { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        public string MemberGradeName { get; set; }
    }
}