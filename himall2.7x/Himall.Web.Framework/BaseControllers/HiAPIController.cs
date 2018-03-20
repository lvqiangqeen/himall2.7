using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Routing;

namespace Himall.Web.Framework
{
	[ApiExceptionFilter]
	public abstract class HiAPIController<TUser> : ApiController
	{
		#region 字段
		private TUser _user;
		#endregion

		#region 属性
		/// <summary>
		/// 当前登录的用户
		/// </summary>
		public TUser CurrentUser
		{
			get
			{
				if (object.Equals(_user, default(TUser)))
					_user = GetUser();

				return _user;
			}
		}

		/// <summary>
		/// 当前用户Id
		/// </summary>
		public long CurrentUserId
		{
			get
			{
				string userkey = "";
				userkey = WebHelper.GetQueryString("userkey");
				if (string.IsNullOrWhiteSpace(userkey))
				{
					userkey = WebHelper.GetFormString("userkey");
				}
				return DecryptUserKey(userkey);
			}
		}
		#endregion

		#region 虚拟方法
		/// <summary>
		/// 解析userKey
		/// </summary>
		/// <param name="userKey"></param>
		/// <returns></returns>
		protected virtual long DecryptUserKey(string userKey)
		{
			return UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_USER);
		}
		#endregion

		#region 抽像方法
		/// <summary>
		/// 根据用户id获取用户信息
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		protected abstract TUser GetUser();
		#endregion
	}
}
