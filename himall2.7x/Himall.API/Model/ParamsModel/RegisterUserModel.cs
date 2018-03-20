using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 注册用户
    /// </summary>
    public class RegisterUserModel
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string code { get; set; }
        public string sex { get; set; }
        public string headimgurl { get; set; }
        public string unionid { get;set; }
        public string oauthNickName { get; set; }
        public string oauthType { get; set; }
        public string oauthOpenId { get; set; }
    }
}
