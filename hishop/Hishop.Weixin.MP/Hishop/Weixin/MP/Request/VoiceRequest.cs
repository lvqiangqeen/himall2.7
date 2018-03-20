namespace Hishop.Weixin.MP.Request
{
    using Hishop.Weixin.MP;
    using System;
    using System.Runtime.CompilerServices;

    public class VoiceRequest : AbstractRequest
    {
        public string Format { get; set; }

        public int MediaId { get; set; }
    }
}

