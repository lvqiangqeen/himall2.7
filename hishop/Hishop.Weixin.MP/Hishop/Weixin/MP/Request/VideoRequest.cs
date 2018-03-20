namespace Hishop.Weixin.MP.Request
{
    using Hishop.Weixin.MP;
    using System;
    using System.Runtime.CompilerServices;

    public class VideoRequest : AbstractRequest
    {
        public int MediaId { get; set; }

        public int ThumbMediaId { get; set; }
    }
}

