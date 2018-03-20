
namespace Himall.Plugin.OAuth.Weibo.Code
{
    /// <summary>
    /// 微博登录配置
    /// </summary>
    public class OAuthWeiboConfig
    {

        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// AppSecret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 授权地址
        /// </summary>
        public string AuthorizeURL { get; set; }

        /// <summary>
        /// 验证内容
        /// </summary>
        public string ValidateContent { get; set; }
    }
}
