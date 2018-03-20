using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Helper;

namespace Himall.API
{
	public class ShopCenterController : BaseShopApiController
	{
		protected override object ChangePasswordByOldPassword(string oldPassword, string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				return ErrorResult("密码不能为空");

           if (string.IsNullOrWhiteSpace(oldPassword))
                return ErrorResult("旧密码输入错误");
            CheckUserLogin();

			var user = CurrentUser;

			var pwd = SecureHelper.MD5(SecureHelper.MD5(oldPassword) + user.PasswordSalt);

			if (pwd == user.Password)
			{
				Application.ManagerApplication.ChangeSellerManagerPassword(user.Id, user.ShopId, password, user.RoleId);
                if (CurrentUser.RoleId == 0)
                {
                    Application.MemberApplication.ChangePassword(user.UserName, password);//修改商家对应的用户密码
                }

				return SuccessResult("密码修改成功");
			}

			return ErrorResult("旧密码输入不正确");
		}
	}
}
