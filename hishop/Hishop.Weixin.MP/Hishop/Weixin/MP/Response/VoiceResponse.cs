namespace Hishop.Weixin.MP.Response
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Domain;
    using System;
    using System.Runtime.CompilerServices;

    public class VoiceResponse : AbstractResponse
    {
        public override ResponseMsgType MsgType
        {
            get
            {
                return ResponseMsgType.Voice;
            }
        }

        public Hishop.Weixin.MP.Domain.Voice Voice { get; set; }
    }
}

