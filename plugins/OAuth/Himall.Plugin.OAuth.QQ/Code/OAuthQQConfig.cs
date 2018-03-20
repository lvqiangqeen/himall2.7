
namespace Himall.Plugin.OAuth.QQ
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class OAuthQQConfig
    {
        /// <summary>
        /// AppId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// AppKey 
        /// </summary>
        public string AppKey { get; set; }

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
