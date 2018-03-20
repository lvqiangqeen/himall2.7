
namespace Himall.Web.Areas.Web
{
    /// <summary>
    /// 登录异常
    /// </summary>
    class LoginException : Himall.Core.HimallException
    {
        /// <summary>
        /// 错误类型
        /// </summary>
        public enum ErrorTypes
        {
            UsernameError,
            PasswordError,
            CheckCodeError
        }

        ErrorTypes _errrorType ;

        /// <summary>
        /// 错误类型
        /// </summary>
        public ErrorTypes ErrorType { get { return _errrorType; } }


        public LoginException(string msg, ErrorTypes errorType)
            : base(msg)
        {
            _errrorType = errorType;

        }

    }
}