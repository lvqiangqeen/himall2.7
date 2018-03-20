namespace Hishop.Weixin.MP
{
    using System;
    using System.Runtime.CompilerServices;

    public class AbstractRequest
    {
        public DateTime CreateTime { get; set; }

        public string FromUserName { get; set; }

        public long MsgId { get; set; }

        public RequestMsgType MsgType { get; set; }

        public string ToUserName { get; set; }
    }
}

