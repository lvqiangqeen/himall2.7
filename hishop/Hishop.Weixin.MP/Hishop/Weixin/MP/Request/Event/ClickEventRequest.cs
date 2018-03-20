namespace Hishop.Weixin.MP.Request.Event
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Request;
    using System;
    using System.Runtime.CompilerServices;

    public class ClickEventRequest : EventRequest
    {
        public override RequestEventType Event
        {
            get
            {
                return RequestEventType.Click;
            }
            set
            {
            }
        }

        public string EventKey { get; set; }
    }
}

