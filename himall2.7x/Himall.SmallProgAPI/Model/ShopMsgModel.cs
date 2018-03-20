using System;
using System.Collections.Generic;

namespace Himall.SmallProgAPI.Model
{
    public class ShopMsgModel
    {
        public long ShopId { get; set; }

        public string ShopName { get; set; }

        public string MsgTitle { get; set; }
        public string Msg { get; set; }
        public string ShopLogo { get; set; }
        public string ProductImage { get; set; }
        public string MsgTime { get; set; }
    }
}