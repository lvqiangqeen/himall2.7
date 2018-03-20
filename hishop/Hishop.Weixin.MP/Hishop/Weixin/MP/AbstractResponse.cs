namespace Hishop.Weixin.MP
{
    using System;
    using System.Runtime.CompilerServices;

    public class AbstractResponse
    {
        public AbstractResponse()
        {
            this.CreateTime = DateTime.Now;
        }

        public DateTime CreateTime { get; set; }

        public string FromUserName { get; set; }

        public bool FuncFlag { get; set; }

        public virtual ResponseMsgType MsgType { get; set; }

        public string ToUserName { get; set; }
    }
}

