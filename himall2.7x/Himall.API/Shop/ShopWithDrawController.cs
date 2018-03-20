using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.IServices.QueryModel;
using Himall.Application;
using Himall.API.Model.ParamsModel;
using Himall.DTO;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins;

namespace Himall.API
{
    public class ShopWithDrawController : BaseShopApiController
	{
		#region 静态字段
		private static string _encryptKey = Guid.NewGuid().ToString("N");
		#endregion

		#region 常量
		private const decimal MAX_WithDraw_Money = 10000;
		#endregion

        /// <summary>
        /// 获取可提现金额
        /// </summary>
        /// <returns></returns>
        public object GetWithdraw()
        {
            CheckShopManageLogin(); ;

            //获取店铺账户信息
            DTO.ShopAccount shopAccount = BillingApplication.GetShopAccount(CurrentShop.Id);


            //获取站点配置信息
            var siteSetting = SiteSettingApplication.GetSiteSettings();
            //获取加盟商账户余额
            var balance = 0m;
            if (shopAccount != null)
            {
                balance = shopAccount.Balance;
            }

            //判断店铺是否绑定银行卡
            bool IsBindBank = true;
            if (string.IsNullOrWhiteSpace(CurrentShop.BankAccountNumber))
            {
                IsBindBank = false;
            }
            var user = MemberApplication.GetMemberByName(CurrentUser.UserName);

            Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
            return Json(new
            {
                success = true, //状态
                msg = "",
                Balance = balance, //店铺账户余额
                RealMoney = (MAX_WithDraw_Money - balance) <= 0 ? MAX_WithDraw_Money : balance, //实际可提现金额  //实际可提现金额
                BankAccountName = CurrentShop.BankAccountName,//银行开户名
                BankAccountNumber = CurrentShop.BankAccountNumber,//银行账号
                BankName = CurrentShop.BankName, //开户银行名称
                IsBindBank = IsBindBank,   //店铺是否绑定银行卡号 true=已绑定
                Phone = mMemberAccountSafety.Phone, // 手机号码
                BankBranch = CurrentShop.BankName //开户银行支行完整名称
            });
        }


		#region /*提现验证码发送和验证*/
		///// <summary>
		///// 发送验证码(提现验证码)
		///// </summary>
		///// <returns></returns>
		//public object SendWithdrawCode(ShopApplyWithDrawModel model)
		//{
		//	CheckShopManageLogin();
		//	;
		//	//获取站点配置信息
		//	var siteSetting = SiteSettingApplication.GetSiteSettings();
		//	var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
		//	Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
		//	if (!mMemberAccountSafety.BindPhone)
		//	{
		//		return Json(new Result() { success = false, msg = "未绑定手机号" });
		//	}

		//	if (!ShopApplication.SendCode("Himall.Plugin.Message.SMS", mMemberAccountSafety.Phone, user.UserName, siteSetting.SiteName))
		//	{
		//		return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试！" });
		//	}
		//	return Json(new Result() { success = true, msg = "发送成功" });
		//}

		///// <summary>
		///// 检查验证码是否正确（提现验证码）
		///// </summary>
		///// <param name="code"></param>
		///// <returns></returns>
		//public object WithdrawCodeCheck(ShopApplyWithDrawModel model)
		//{
		//	CheckShopManageLogin();
		//	;
		//	var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
		//	Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
		//	int result = ShopApplication.CheckCode("Himall.Plugin.Message.SMS", model.Code, mMemberAccountSafety.Phone, user.UserName);
		//	if (result > 0)
		//		return Json(new Result() { success = true });
		//	else
		//		return Json(new Result() { success = false, msg = "验证码错误" });
		//}
		#endregion


		#region /*绑定银行卡验证码发送和验证*/
		///// <summary>
		///// 发送验证码(绑定银行卡)
		///// </summary>
		///// <returns></returns>
		//public object SendBindBankCode(ShopApplyWithDrawModel model)
		//{
		//	CheckShopManageLogin();
		//	;
		//	//获取站点配置信息
		//	var siteSetting = SiteSettingApplication.GetSiteSettings();

		//	var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
		//	Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
		//	if (!mMemberAccountSafety.BindPhone)
		//	{
		//		return Json(new Result() { success = false, msg = "未绑定手机号" });
		//	}

		//	if (!ShopApplication.BindBankSendCode("Himall.Plugin.Message.SMS", mMemberAccountSafety.Phone, user.UserName, siteSetting.SiteName))
		//	{
		//		return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试！" });
		//	}
		//	return Json(new Result() { success = true, msg = "发送成功" });
		//}

		///// <summary>
		///// 检查验证码是否正确（绑定银行卡）
		///// </summary>
		///// <param name="code"></param>
		///// <returns></returns>
		//public object BindBankCodeCheck(ShopApplyWithDrawModel model)
		//{
		//	CheckShopManageLogin();
		//	;
		//	var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
		//	Himall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
		//	int result = ShopApplication.BindBankCheckCode("Himall.Plugin.Message.SMS", model.Code, mMemberAccountSafety.Phone, user.UserName);
		//	if (result > 0)
		//		return Json(new Result() { success = true });
		//	else
		//		return Json(new Result() { success = false, msg = "验证码错误" });
		//}
		#endregion

		/// <summary>
		/// 发送验证码之前验证联系方式
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		protected override bool CheckContact(string contact, out string errorMessage)
		{
			CheckShopManageLogin();

			//获取站点配置信息
			var siteSetting = SiteSettingApplication.GetSiteSettings();
			var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
			var mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(user.Id);
			Cache.Insert(_encryptKey + contact, CurrentUser.Id, DateTime.Now.AddHours(1));
			errorMessage = "未绑定手机号";
			return mMemberAccountSafety.BindPhone;
		}

		/// <summary>
		/// 短信验证成功后创建验证成功凭证
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		protected override string CreateCertificate(string contact)
		{
			var identity = Cache.Get(_encryptKey + contact).ToString();
			identity = SecureHelper.AESEncrypt(identity, _encryptKey);
			return identity;
		}

        /// <summary>
        /// 银行数据提交
        /// </summary>
        /// <param name="BankAccountName">开户行名称</param>
        /// <param name="BankAccountNumber">开户行号</param>
        /// <param name="BankName">支行名称</param>
        /// <param name="BankRegionId">开户行所在地</param>
        /// <returns></returns>
		public object SetBankAccount(SetBankAccountPostModel model)
        {
            CheckShopManageLogin();

			if (!CheckCertificate(model.Certificate))
				return ErrorResult("凭证无效");

            Himall.DTO.BankAccount info = new BankAccount()
            {
                ShopId = CurrentShop.Id,
                BankAccountName = model.BankAccountName,
                BankAccountNumber = model.BankAccountNumber,
                BankCode = model.BankCode,
                BankName = model.BankName,
                BankRegionId = model.BankRegionId
            };
            ShopApplication.UpdateBankAccount(info);
            //更新店铺信息后，清除缓存，以便获取到最新数据
            RemoveShopCache();
            return Json(new { success = true, msg = "成功" });
        }


        /// <summary>
        /// 提现申请数据提交
        /// </summary>
        /// <returns></returns>
		public object ApplyWithDrawSubmit(ApplyWithDrawSubmitPostModel model)
        {
            CheckShopManageLogin();

			if (!CheckCertificate(model.Certificate))
				return ErrorResult("凭证无效");

			Himall.DTO.ShopWithDraw info = new ShopWithDraw()
			{
				SellerId = CurrentUser.Id,
				SellerName = CurrentUser.UserName,
				ShopId = CurrentShop.Id,
				WithdrawalAmount = model.Amount,
				WithdrawType = WithdrawType.BankCard
			};

            if(model.Amount<=0)
            {
                return ErrorResult("提现金额不正确");
            }

			bool isbool = BillingApplication.ShopApplyWithDraw(info);
			if (isbool)
				return Json(new { success = true, msg = "成功！" });
			else
				return Json(new { success = false, msg = "余额不足，无法提现！" });
        }

        public object GetCheckContact(string contact, string certificate)
        {
            CheckShopManageLogin();
            if (!CheckCertificate(certificate))
                return ErrorResult("凭证无效");

            PluginInfo pluginInfo;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contact);
			if (isMobile)
				pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();
			else
				pluginInfo = PluginsManagement.GetInstalledPluginInfos(PluginType.Email).First();

            MemberApplication.UpdateMemberContacts(new MemberContacts()
            {
                Contact = contact,
                ServiceProvider = pluginInfo.PluginId,
                UserId = CurrentUser.Id,
                UserType = Himall.Model.MemberContactsInfo.UserTypes.ShopManager
            });

            return SuccessResult();
        }

		#region 私有方法
		private bool CheckCertificate(string certificate)
		{
			var identity = SecureHelper.AESDecrypt(certificate, _encryptKey);
			long managerId;
			return long.TryParse(identity, out managerId) && managerId == CurrentUser.Id;
		}
		#endregion
    }
}
