namespace Hishop.Weixin.MP.Response
{
    using Hishop.Weixin.MP;
    using System;
    using System.Runtime.CompilerServices;

    public class TextResponse : AbstractResponse
    {
        public string Content { get; set; }

        public override ResponseMsgType MsgType
        {
            get
            {
                return ResponseMsgType.Text;
            }
        }
    }
}

