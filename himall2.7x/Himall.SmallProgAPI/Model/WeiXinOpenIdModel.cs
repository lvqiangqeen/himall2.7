using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class WeiXinOpenIdModel
    {
        public long expires_in { get; set; }

        public string session_key { get; set; }

        public string openid { get; set; }
        

    }
}
