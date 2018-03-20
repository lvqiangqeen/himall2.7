using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Himall.SmallProgAPI.Model
{
    /// <summary>
    /// 微信小程序返回的用户Model
    /// </summary>
    public class WxAppletUserInfo
    {
        public string openId
        {
            get;set;
        }
        public string nickName
        {
            get; set;
        }
        public string gender
        {
            get; set;
        }
        public string city
        {
            get; set;
        }
        public string province
        {
            get; set;
        }
        public string country
        {
            get; set;
        }
        public string avatarUrl
        {
            get; set;
        }
        public string unionId
        {
            get; set;
        }
        public WaterMark watermark
        {
            get; set;
        }
        
    }
    public class WaterMark
    {
        public string appid { get; set; }
        public string timestamp { get; set; }
    }
}
