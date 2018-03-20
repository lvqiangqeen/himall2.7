namespace Hishop.Weixin.MP.Util
{
    using Hishop.Weixin.MP;
    using System;
    using System.Xml.Linq;

    public static class MsgTypeHelper
    {
        public static RequestMsgType GetMsgType(string str)
        {
            return (RequestMsgType) Enum.Parse(typeof(RequestMsgType), str, true);
        }

        public static RequestMsgType GetMsgType(XDocument doc)
        {
            return GetMsgType(doc.Root.Element("MsgType").Value);
        }
    }
}

