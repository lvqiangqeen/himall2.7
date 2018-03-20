using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.IServices.QueryModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CashDepositController : BaseSellerController
    {
        private ICashDepositsService _iCashDepositsService;
        private ISiteSettingService _iSiteSettingService;
        public CashDepositController(ICashDepositsService iCashDepositsService, ISiteSettingService iSiteSettingService)
        {
            this._iCashDepositsService = iCashDepositsService;
            _iSiteSettingService = iSiteSettingService;
        }

        public ActionResult Management()
        {
            var model = _iCashDepositsService.GetCashDepositByShopId(CurrentSellerManager.ShopId);
            ViewBag.NeedPayCashDeposit = _iCashDepositsService.GetNeedPayCashDepositByShopId(CurrentSellerManager.ShopId);
            if (ViewBag.NeedPayCashDeposit==-1)
            {
                return View("UnSet");
            }
            return View(model);
        }

        public JsonResult CashDepositDetail(long cashDepositId, int pageNo = 1, int pageSize = 10)
        {
            CashDepositDetailQuery query = new CashDepositDetailQuery()
            {
                CashDepositId = cashDepositId,
                PageNo = pageNo,
                PageSize = pageSize
            };
            ObsoletePageModel<CashDepositDetailInfo> cashDepositDetails = _iCashDepositsService.GetCashDepositDetails(query);
            var cashDepositDetailModel = cashDepositDetails.Models.ToArray().Select(item => new
                {
                    Id = item.Id,
                    Date = item.AddDate.ToString("yyyy-MM-dd HH:mm"),
                    Balance = item.Balance,
                    Operator = item.Operator,
                    Description = item.Description
                });
            return Json(new { rows = cashDepositDetailModel, total = cashDepositDetails.Total });

        }
        public JsonResult PaymentList(decimal balance)
        {
            var needPayCashDeposit = _iCashDepositsService.GetNeedPayCashDepositByShopId(CurrentSellerManager.ShopId);
            if (balance < needPayCashDeposit)
            {
                throw new HimallException("缴纳保证金必须大于应缴保证金");
            }
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/CashDeposit/Return/{0}?balance={1}";

            //获取异步通知地址
            string payNotify = webRoot + "/pay/CashNotify/{0}?str={1}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            //不重复数字
            string ids = DateTime.Now.ToString("yyyyMMddmmss") + CurrentSellerManager.ShopId.ToString();

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId), balance), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId), balance + "-" + CurrentSellerManager.UserName + "-" + CurrentSellerManager.ShopId), ids, balance, "保证金充值");
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
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Himall.Plugin.Payment.WeiXinPay" && item.Id != "Himall.Plugin.Payment.WeiXinPay_Native");//只选择正常加载的插件
            return Json(models);
        }

        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

        [ActionName("CashNotify")]
        [ValidateInput(false)]
        public ContentResult CashPayNotify_Post(string id, string str)
        {
            decimal balance = decimal.Parse(str.Split('-')[0]);
            string userName = str.Split('-')[1];
            long shopId = long.Parse(str.Split('-')[2]);
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
                bool result = Cache.Get(CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds))) == null ? false : true;
                if (!result)
                {


                    var accountService = _iCashDepositsService;
                    CashDepositDetailInfo model = new CashDepositDetailInfo();

                    model.AddDate = DateTime.Now;
                    model.Balance = balance;
                    model.Description = "充值";
                    model.Operator = userName;

                    List<CashDepositDetailInfo> list = new List<CashDepositDetailInfo>();
                    list.Add(model);
                    if (accountService.GetCashDepositByShopId(shopId) == null)
                    {
                        CashDepositInfo cashDeposit = new CashDepositInfo()
                        {
                            CurrentBalance = balance,
                            Date = DateTime.Now,
                            ShopId = shopId,
                            TotalBalance = balance,
                            EnableLabels = true,
                            Himall_CashDepositDetail = list
                        };
                        accountService.AddCashDeposit(cashDeposit);
                    }

                    else
                    {
                        model.CashDepositId = accountService.GetCashDepositByShopId(shopId).Id;

                        _iCashDepositsService.AddCashDepositDetails(model);
                    }


                    response = payment.Biz.ConfirmPayResult();

                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return Content(response);
        }

        public ActionResult Return(string id, decimal balance)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;

            try
            {

                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
                var accountService = _iCashDepositsService;
                CashDepositDetailInfo model = new CashDepositDetailInfo();
                bool result = Cache.Get(CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds))) == null ? false : true;
                if (!result)
                {
                    model.AddDate = DateTime.Now;
                    model.Balance = balance;
                    model.Description = "充值";
                    model.Operator = CurrentSellerManager.UserName;

                    List<CashDepositDetailInfo> list = new List<CashDepositDetailInfo>();
                    list.Add(model);
                    if (accountService.GetCashDepositByShopId(CurrentSellerManager.ShopId) == null)
                    {
                        CashDepositInfo cashDeposit = new CashDepositInfo()
                        {
                            CurrentBalance = balance,
                            Date = DateTime.Now,
                            ShopId = CurrentSellerManager.ShopId,
                            TotalBalance = balance,
                            EnableLabels = true,
                            Himall_CashDepositDetail = list
                        };
                        accountService.AddCashDeposit(cashDeposit);
                    }

                    else
                    {
                        model.CashDepositId = accountService.GetCashDepositByShopId(CurrentSellerManager.ShopId).Id;

                        _iCashDepositsService.AddCashDepositDetails(model);
                    }

                    //写入支付状态缓存
                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true);//标记为已支付
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            ViewBag.Error = errorMsg;
            ViewBag.Logo =  _iSiteSettingService.GetSiteSettings().Logo;//获取Logo
            return View();
        }
    }
}