using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class WeixinPassiveReply
    {
        public WeixinPassiveReply( string to , string form , DateTime time , string content )
        {
            ToUserName = to;
            FromUserName = form;
            CreateTime = time;
            Content = content;
        }

        public string ToUserName { get; private set; }

        public string FromUserName { get; private set; }

        public DateTime CreateTime { get; private set; }

        public string MsgType
        {
            get
            {
                return "text";
            }
        }

        public string Content { get; private set; }
    }
}