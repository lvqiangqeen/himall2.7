using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Models
{
    public class WeiXinShareArgsModel
    {
        public WeiXinShareArgsModel()
        {

        }

        public WeiXinShareArgsModel( string t , string n , string s )
        {
            this.Timestamp = t;
            this.NonceStr = n;
            this.Signature = s;
        }

        public WeiXinShareArgsModel( string t , string n , string s  , string a , string ticket)
        {
            this.Timestamp = t;
            this.NonceStr = n;
            this.Signature = s;
            this.AppId = a;
            this.Ticket = ticket;
        }

        public string Timestamp { get; set; }

        public string NonceStr { get; set; }

        public string Signature { get; set; }

        public string AppId { get; set; }

        public string Ticket { get; set; }
    }
}