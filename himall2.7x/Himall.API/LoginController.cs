using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.Application;

namespace Himall.API
{
    public class LoginController : BaseApiController
    {
		#region 静态字段
		private static string _encryptKey = Guid.NewGuid().ToString("N");
		#endregion

		#region 方法
		public object GetUser(string userName = "", string password = "", string oauthType = "", string oauthOpenId="", string unionid = "", string headimgurl = "", string oauthNickName = "", int? sex = null, string city = "", string province = "")
		{          
			dynamic d = new System.Dynamic.ExpandoObject();
			//信任登录
			if (!string.IsNullOrEmpty(oauthType) && (!string.IsNullOrEmpty(unionid)||!string.IsNullOrEmpty(oauthOpenId)) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
			{
				Log.Debug(string.Format("oauthType={0} openId={1} unionid={2} userName={3}", oauthType, oauthOpenId, unionid, userName));
				
				var member = Application.MemberApplication.GetMemberByUnionIdAndProvider(oauthType, unionid);
				if (member == null)
					member = Application.MemberApplication.GetMemberByOpenId(oauthType, oauthOpenId);

                if (member != null)
                {
                    //信任登录并且已绑定
                    d.Success = "true";
                    d.UserId = member.Id.ToString();
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    d.UserKey = memberId;
                    var prom = DistributionApplication.GetPromoterByUserId(member.Id);
                    if (prom != null && prom.Status == PromoterInfo.PromoterStatus.Audited)
                    {
                        d.IsPromoter = true;
                    }
                    else
                    {
                        d.IsPromoter = false;
                    }
                }
                else
                {
                    //信任登录未绑定
                    d.ErrorCode = "104";
                    d.ErrorMsg = "未绑定商城帐号";
                }
			}
			//普通登录
			if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password) && string.IsNullOrEmpty(oauthType) && string.IsNullOrEmpty(unionid)&&string.IsNullOrEmpty(oauthOpenId))
			{
                UserMemberInfo member = null;
                try{
                    member = ServiceProvider.Instance<IMemberService>.Create.Login(userName, password);
                }
                catch (Exception ex)
                {
                    d.Success = "false";
                    d.ErrorCode = "104";
                    d.ErrorMsg = ex.Message;
                    return d;
                }
				if (member == null)
				{
					d.Success = "false";
					d.ErrorCode = "103";
					d.ErrorMsg = "用户名或密码错误";
				}
				else
				{
					d.Success = "true";
					d.UserId = member.Id.ToString();
					string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
					d.UserKey = memberId;
                    var prom = DistributionApplication.GetPromoterByUserId(member.Id);
                    if (prom != null && prom.Status == PromoterInfo.PromoterStatus.Audited)
                    {
                        d.IsPromoter = true;
                    }
                    else
                    {
                        d.IsPromoter = false;
                    }
                }
			}
			//绑定
			if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(oauthType) && (!string.IsNullOrEmpty(unionid) || !string.IsNullOrEmpty(oauthOpenId)))
			{
				var service = ServiceProvider.Instance<IMemberService>.Create;
				var member = service.Login(userName, password);
				if (member == null)
				{
					d.Success = "false";
					d.ErrorCode = "103";
					d.ErrorMsg = "用户名或密码错误";
				}
				else
				{
					string wxsex = null;
					if (null != sex)
						wxsex = sex.Value.ToString();

					province = System.Web.HttpUtility.UrlDecode(province);
					city = System.Web.HttpUtility.UrlDecode(city);

					service.BindMember(member.Id, oauthType, oauthOpenId, wxsex, headimgurl, unionid, null, city, province);
					string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
					d.Success = "true";
					d.UserId = member.Id;
					d.UserKey = memberId;
                    var prom = DistributionApplication.GetPromoterByUserId(member.Id);
                    if (prom != null && prom.Status != PromoterInfo.PromoterStatus.Audited)
                    {
                        d.IsPromoter = true;
                    }
                    else
                    {
                        d.IsPromoter = false;
                    }
                }
			}

			return d;
		}

		#region 重写方法
		protected override bool CheckContact(string contact, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (!string.IsNullOrWhiteSpace(contact))
			{
				var userMenberInfo = Application.MemberApplication.GetMemberByContactInfo(contact);
				if (userMenberInfo != null)
					Cache.Insert(_encryptKey + contact, string.Format("{0}:{1:yyyyMMddHHmmss}", userMenberInfo.Id, userMenberInfo.CreateDate), DateTime.Now.AddHours(1));
				return userMenberInfo != null;
			}

			return false;
		}

		protected override string CreateCertificate(string contact)
		{
			var identity = Cache.Get<string>(_encryptKey + contact);
			identity = SecureHelper.AESEncrypt(identity, _encryptKey);
			return identity;
		}

		protected override object ChangePassowrdByCertificate(string certificate, string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				return ErrorResult("密码不能为空");

			certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
			long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

			if (userId == 0)
				throw new HimallException("数据异常");

			Application.MemberApplication.ChangePassword(userId, password);

			return SuccessResult("密码修改成功");
		}
		#endregion
		#endregion
    }
}
