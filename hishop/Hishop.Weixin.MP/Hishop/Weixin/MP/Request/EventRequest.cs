namespace Hishop.Weixin.MP.Request
{
    using Hishop.Weixin.MP;
    using System;
    using System.Runtime.CompilerServices;

    public abstract class EventRequest : AbstractRequest
    {
        protected EventRequest()
        {
        }

        public virtual RequestEventType Event { get; set; }
    }
}

