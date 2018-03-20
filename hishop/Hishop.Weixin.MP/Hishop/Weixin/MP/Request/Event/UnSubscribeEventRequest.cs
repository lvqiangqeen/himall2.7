namespace Hishop.Weixin.MP.Request.Event
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Request;
    using System;

    public class UnSubscribeEventRequest : EventRequest
    {
        public override RequestEventType Event
        {
            get
            {
                return RequestEventType.UnSubscribe;
            }
            set
            {
            }
        }
    }
}

