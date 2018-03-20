
namespace Himall.Core.Plugins
{

    /// <summary>
    /// 插件类型
    /// </summary>
    public enum PluginType
    {
        /// <summary>
        /// 开放登录插件
        /// </summary>
        OauthPlugin,

        /// <summary>
        /// 支付插件
        /// </summary>
        PayPlugin,

        /// <summary>
        /// 快递插件
        /// </summary>
        Express,

        /// <summary>
        /// 消息插件
        /// </summary>
        Message,

        /// <summary>
        /// 手机短信插件
        /// </summary>
        SMS=5,

        /// <summary>
        /// 邮件插件
        /// </summary>
        Email=6
    }
}
