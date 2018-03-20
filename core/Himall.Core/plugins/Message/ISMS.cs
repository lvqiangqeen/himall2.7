
namespace Himall.Core.Plugins.Message
{
    /// <summary>
    /// 短信接口
    /// </summary>
    public interface ISMSPlugin : IMessagePlugin
    {
        /// <summary>
        /// 获取短信剩余数量
        /// </summary>
        /// <returns></returns>
        string GetSMSAmount();

        /// <summary>
        /// 获取购买链接
        /// </summary>
        /// <returns></returns>
        string GetBuyLink();

        /// <summary>
        /// 获取登录链接
        /// </summary>
        /// <returns></returns>
        string GetLoginLink();
    }
}
