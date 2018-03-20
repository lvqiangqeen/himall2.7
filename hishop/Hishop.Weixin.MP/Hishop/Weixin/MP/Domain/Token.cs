namespace Hishop.Weixin.MP.Domain
{
    using System;
    using System.Runtime.CompilerServices;

    public class Token
    {
        public string access_token { get; set; }

        public int expires_in { get; set; }
    }
}

