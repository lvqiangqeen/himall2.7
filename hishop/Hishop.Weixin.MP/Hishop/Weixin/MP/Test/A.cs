namespace Hishop.Weixin.MP.Test
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Handler;
    using System;

    public class A : RequestHandler
    {
        public A(string xml) : base(xml)
        {
        }

        public override AbstractResponse DefaultResponse(AbstractRequest requestMessage)
        {
            return null;
        }
    }
}

