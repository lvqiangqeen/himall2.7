
namespace Himall.Core.Plugins.Payment
{
    /// <summary>
    /// 链接类型
    /// </summary>
    public enum UrlType
    {
        /// <summary>
        /// 普通页面链接
        /// </summary>
        Page,

        /// <summary>
        /// 二维码链接
        /// </summary>
        QRCode,
        
        /// <summary>
        /// 需解析成表单提交的链接
        /// </summary>
        FormPost
    }
}
