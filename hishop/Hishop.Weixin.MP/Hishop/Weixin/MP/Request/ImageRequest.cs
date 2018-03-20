namespace Hishop.Weixin.MP.Request
{
    using Hishop.Weixin.MP;
    using System;
    using System.Runtime.CompilerServices;

    public class ImageRequest : AbstractRequest
    {
        public int MediaId { get; set; }

        public string PicUrl { get; set; }
    }
}

