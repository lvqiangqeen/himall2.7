namespace Hishop.Weixin.MP.Response
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Domain;
    using System;
    using System.Runtime.CompilerServices;

    public class ImageResponse : AbstractResponse
    {
        public Hishop.Weixin.MP.Domain.Image Image { get; set; }

        public override ResponseMsgType MsgType
        {
            get
            {
                return ResponseMsgType.Image;
            }
        }
    }
}

