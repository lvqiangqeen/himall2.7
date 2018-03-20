
namespace Himall.Plugin.Message.Email
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class MessageEmailConfig
    {
        /// <summary>
        /// 邮件发送服务器
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// 发送邮箱端口
        /// </summary>
        public string SmtpPort { get; set; }
        /// <summary>
        /// 邮箱用户名
        /// </summary>
        public string EmailName { get; set; }

        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 发送邮箱
        /// </summary>
        public string SendAddress { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
    }
}
