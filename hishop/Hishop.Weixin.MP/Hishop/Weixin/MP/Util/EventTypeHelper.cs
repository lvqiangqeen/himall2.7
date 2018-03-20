namespace Hishop.Weixin.MP.Util
{
    using Hishop.Weixin.MP;
    using System;
    using System.Xml.Linq;

    public static class EventTypeHelper
    {
        public static RequestEventType GetEventType(string str)
        {
            return (RequestEventType) Enum.Parse(typeof(RequestEventType), str, true);
        }

        public static RequestEventType GetEventType(XDocument doc)
        {
            return GetEventType(doc.Root.Element("Event").Value);
        }
    }
}

