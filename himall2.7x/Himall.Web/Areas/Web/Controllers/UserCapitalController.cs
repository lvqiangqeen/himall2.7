using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Models;
using Himall.Core;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Payment;
using System.Threading.Tasks;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class UserCapitalController : BaseMemberController
    {
        ISiteSettingService _iSiteSettingService;
        IMemberCapitalService _iMemberCapitalService;
        IMemberService _iMemberService;
        public UserCapitalController(IMemberCapitalService iMemberCapitalService, ISiteSettingService iSiteSettingService, IMemberService iMemberService)
        {
            _iSiteSettingService = iSiteSettingService;
            _iMemberCapitalService = iMemberCapitalService;
            _iMemberService = iMemberService;
        }

        // GET: Web/UserCapital
        public ActionResult Index()
        {
            var capitalService = _iMemberCapitalService;
            var model = capitalService.GetCapitalInfo(CurrentUser.Id);
            return View(model);
        }

        public JsonResult List(CapitalDetailInfo.CapitalDetailType capitalType, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;

            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                capitalType = capitalType,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
              {
                  Id = e.Id,
                  Amount = e.Amount,
                  CapitalID = e.CapitalID,
                  CreateTime = e.CreateTime.Value.ToString(),
                  SourceData = e.SourceData,
                  SourceType = e.SourceType,
                  Remark = e.SourceType.ToDescription() + ",单号：" + e.Id,
                  PayWay = e.Remark
              }).ToList();

            var models = new DataGridModel<CapitalDetailModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public JsonResult ApplyWithDrawList(int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var query = new ApplyWithDrawQuery
            {
                memberId = CurrentUser.Id,
                PageSize = rows,
                PageNo = page,
                Sort = "ApplyTime"
            };
            var pageMode = capitalService.GetApplyWithDraw(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                string applyStatus = string.Empty;
                if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail
                    || e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm
                    )
                {
                    applyStatus = "提现中";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
                {
                    applyStatus = "提现失败";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess)
                {
                    applyStatus = "提现申请成功";
                }
                return new ApplyWithDrawModel
                 {
                     Id = e.Id,
                     ApplyAmount = e.ApplyAmount,
                     ApplyStatus = e.ApplyStatus,
                     ApplyStatusDesc = applyStatus,
                     ApplyTime = e.ApplyTime.ToString()
                 };
            });
            var models = new DataGridModel<ApplyWithDrawModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public JsonResult ChargeList(int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var query = new ChargeQuery
            {
                memberId = CurrentUser.Id,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetChargeLists(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                return new ChargeDetailModel
                {
                    Id = e.Id.ToString(),
                    ChargeAmount = e.ChargeAmount,
                    ChargeStatus = e.ChargeStatus,
                    ChargeStatusDesc = e.ChargeStatus.ToDescription(),
                    ChargeTime = e.ChargeTime.ToString(),
                    CreateTime = e.CreateTime.ToString(),
                    ChargeWay = e.ChargeWay,
                    MemId = e.MemId
                };
            });
            var models = new DataGridModel<ChargeDetailModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public ActionResult CapitalCharge()
        {
            var capitalService = _iMemberCapitalService;
            var model = capitalService.GetCapitalInfo(CurrentUser.Id);
            return View(model);
        }

       
        public ActionResult ApplyWithDraw()
        {
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret))
                throw new HimallException("未配置公众号参数");

            var token = AccessTokenContainer.TryGetToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);

            SceneModel scene = new SceneModel(QR_SCENE_Type.WithDraw)
            {
                Object = CurrentUser.Id.ToString()
            };
            SceneHelper helper = new SceneHelper();
            var sceneid = helper.SetModel(scene);
            var ticket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create(token, 300, sceneid);
            ViewBag.ticket = ticket.ticket;
            ViewBag.Sceneid = sceneid;
            var capitalService = _iMemberCapitalService;
            var model = capitalService.GetCapitalInfo(CurrentUser.Id);
            if (model != null)
            {
                ViewBag.ApplyWithMoney = model.Balance.Value;
            }
            else
            {
                ViewBag.ApplyWithMoney = 0;
            }
            var member = _iMemberService.GetMember(CurrentUser.Id);//CurrentUser对象有缓存，取不到最新数据
            ViewBag.IsSetPwd = string.IsNullOrWhiteSpace(member.PayPwd) ? false : true;
            ViewBag.WithDrawMinimum = siteSetting.WithDrawMinimum;
            ViewBag.WithDrawMaximum = siteSetting.WithDrawMaximum;
            return View();
        }
        public JsonResult ApplyWithDrawSubmit(string openid, string nickname, decimal amount, string pwd)
        {
            var success = Application.MemberApplication.VerificationPayPwd(CurrentUser.Id, pwd);
            if (!success)
            {
                throw new HimallException("支付密码不对，请重新输入！");
            }
            var capitalInfo = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
            if (amount > capitalInfo.Balance)
            {
                throw new HimallException("提现金额不能超出可用金额！");
            }
            if (amount <= 0)
            {
                throw new HimallException("提现金额不能小于等于0！");
            }
            ApplyWithDrawInfo model = new ApplyWithDrawInfo()
            {
                ApplyAmount = amount,
                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = openid,
                NickName = nickname
            };
            _iMemberCapitalService.AddWithDrawApply(model);
            return Json(new { success = true });
        }

		public JsonResult SavePayPwd(string oldPwd, string pwd)
		{
			var hasPayPwd = MemberApplication.HasPayPwd(CurrentUser.Id);

			if (hasPayPwd && string.IsNullOrEmpty(oldPwd))
				return Json(new { success = false, msg = "请输入旧支付密码" });

			if (string.IsNullOrWhiteSpace(pwd))
				return Json(new { success = false, msg = "请输入新支付密码" });

			if (hasPayPwd)
			{
				var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, oldPwd);
				if (!success)
					return Json(new { success = false, msg = "旧支付密码错误" });
			}

			_iMemberCapitalService.SetPayPwd(CurrentUser.Id, pwd);

			return Json(new { success = true, msg = "设置成功" });
		}

        public ActionResult SetPayPwd()
        {
			var hasPayPwd = MemberApplication.HasPayPwd(CurrentUser.Id);
			var viewModel = new UserCapitalViewModels.SetPayPwdModel()
			{
				HasPawPwd = hasPayPwd
			};
			return View(viewModel);
        }

        public JsonResult PaymentList(decimal balance)
        {
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));

            //获取同步返回地址
            string returnUrl = webRoot + "/pay/CapitalChargeReturn/{0}";

            //获取异步通知地址
            string payNotify = webRoot + "/pay/CapitalChargeNotify/{0}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            //不重复数字
            string ids = _iMemberCapitalService.CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount).ToString();

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId) + "-" + balance.ToString() + "-" + CurrentUser.Id.ToString()), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId) + "-" + balance.ToString() + "-" + CurrentUser.Id.ToString()), ids, balance, "预付款充值");
                }
                catch (Exception ex)
                {
                    Core.Log.Error("支付页面加载支付插件出错", ex);
                }
                return new PaymentModel()
                {
                    Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                    RequestUrl = requestUrl,
                    UrlType = item.Biz.RequestUrlType,
                    Id = item.PluginInfo.PluginId
                };
            });
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl));//只选择正常加载的插件
            return Json(models);
        }

        public JsonResult ChargeSubmit(decimal amount)
        {
            //var ids = _iMemberCapitalService.CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount);
            ChargeDetailInfo detail = new ChargeDetailInfo()
            {
                ChargeAmount = amount,
                ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay,
                CreateTime = DateTime.Now,
                MemId = CurrentUser.Id
            };
            long id = _iMemberCapitalService.AddChargeApply(detail);
            return Json(new { success = true, msg = id.ToString() });
        }
        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }
    }

    public class ScanStateController : BaseAsyncController
    {
        public void GetStateAsync(string sceneid)
        {
            AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为10s
            Task.Factory.StartNew(() =>
            {
                int time = 0;
                while (true)
                {
                    var key = CacheKeyCollection.SceneReturn(sceneid);
                    var obj = Core.Cache.Get<ApplyWithDrawInfo>(key);
                    if (obj != null)
                    {
                        AsyncManager.Parameters["state"] = true;
                        AsyncManager.Parameters["model"] = obj;
                        break;
                    }
                    else
                    {
                        if (time >= maxWaitingTime)
                        {
                            AsyncManager.Parameters["state"] = false;
                            AsyncManager.Parameters["model"] = obj;
                            break;
                        }
                        else
                        {
                            time += interval;
                            System.Threading.Thread.Sleep(interval);
                        }
                    }
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }
        public JsonResult GetStateCompleted(bool state, ApplyWithDrawInfo model)
        {
            return Json(new { success = state, data = model }, JsonRequestBehavior.AllowGet);
        }
    }

}