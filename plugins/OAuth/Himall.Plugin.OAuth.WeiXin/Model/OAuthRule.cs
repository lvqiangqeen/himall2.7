using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Plugin.OAuth.WeiXin.Model
{
    public class OAuthRule
    {
        public string GetCodeUrl { get; set; }
        public string GetTokenUrl { get; set; }
        public string GetUserInfoUrl { get; set; }
    }
}
