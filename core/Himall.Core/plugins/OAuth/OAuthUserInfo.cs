
namespace Himall.Core.Plugins.OAuth
{
    /// <summary>
    /// 信任登录用户信息
    /// </summary>
    public class OAuthUserInfo
    {

        /// <summary>
        /// 从登录方返回的OpenId
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 多应用共享ID(微信)
        /// </summary>
        public string UnionId { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 是否为男性
        /// </summary>
        public bool? IsMale { get; set; }

    }
}
