namespace Hishop.Weixin.MP.Domain
{
    using System;
    using System.Runtime.CompilerServices;

    public class Video : IMedia, IThumbMedia
    {
        public int MediaId { get; set; }

        public int ThumbMediaId { get; set; }
    }
}

