using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    public class LoginModPwdModel
    {
		/// <summary>
		/// 验证码验证成功后的凭证(和OldPassword必须一个有值)
		/// </summary>
		public string Certificate { get; set; }
		/// <summary>
		/// 旧密码(和Identity必须一个有值)
		/// </summary>
		public string OldPassword { get; set; }
		/// <summary>
		/// 新密码
		/// </summary>
        public string Password { get; set; }
    }
}
