namespace Hishop.Weixin.MP
{
    using System;

    public class WeixinException : Exception
    {
        public WeixinException()
        {
        }

        public WeixinException(string message) : base(message)
        {
        }

        public WeixinException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

