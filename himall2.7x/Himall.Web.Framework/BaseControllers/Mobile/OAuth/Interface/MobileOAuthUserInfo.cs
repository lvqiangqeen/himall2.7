
namespace Himall.Web.Framework
{
    /// <summary>
    /// 移动端信任登录用户信息
    /// </summary>
    class MobileOAuthUserInfo
    {
        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 多个应用共享ID
        /// </summary>
        public string UnionId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Headimgurl { get; set; }

        /// <summary>
        /// 登录授权平台
        /// </summary>
        public string LoginProvider { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }
    }
}
