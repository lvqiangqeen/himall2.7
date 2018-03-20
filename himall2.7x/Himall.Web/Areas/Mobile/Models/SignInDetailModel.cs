using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile.Models
{
    public class SignInDetailModel
    {
        /// <summary>
        /// 是否为当前请求签到
        /// </summary>
        public bool isCurSign { get; set; }
        /// <summary>
        /// 当前签到天数
        /// </summary>
        public int CurSignDurationDay { get; set; }
        /// <summary>
        /// 总计签到
        /// </summary>
        public long CurSignDaySum { get; set; }
        /// <summary>
        /// 用户可用积分
        /// </summary>
        public int MemberAvailableIntegrals { get; set; }
        /// <summary>
        /// 当前用户信息
        /// </summary>
        public UserMemberInfo UserInfo { get; set; }
        public SiteSignInConfigInfo SignConfig { get; set; }
    }
}