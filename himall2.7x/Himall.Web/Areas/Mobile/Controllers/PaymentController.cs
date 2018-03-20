using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.App_Code.Common;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class PaymentController : BaseMobileTemplatesController


    {
        IOrderService _iOrderService;
        IMemberService _iMemberService;
        IMemberCapitalService _iMemberCapitalService;
        IFightGroupService _iFightGroupService;
        public PaymentController(IOrderService iOrderService, IMemberService iMemberService
            , IMemberCapitalService iMemberCapitalService, IFightGroupService iFightGroupService
            )
        {
            _iOrderService = iOrderService;
            _iMemberService = iMemberService;
            _iMemberCapitalService = iMemberCapitalService;
            _iFightGroupService = iFightGroupService;

        }
        /// <summary>
        /// 预付款支付
        /// </summary>
        /// <param name="pmtidpmtid"></param>
        /// <param name="ids"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        public JsonResult PayByCapital(string ids, string payPwd)
        {
            OrderApplication.PayByCapital(UserId, ids, payPwd, Request.Url.Host.ToString());
            return Json(new { success = true, msg = "支付成功" });
        }
        public JsonResult PayByCapitalIsOk(string ids)
        {
            var result = OrderApplication.PayByCapitalIsOk(UserId, ids);
            return Json(new { success = result });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pmtid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult Pay(string pmtid, string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                Log.Error("payment/pay 参数异常：IDS为空");
                return Json(new { success = false, msg = "参数异常" }); ;
            }
            if (string.IsNullOrWhiteSpace(pmtid))
            {
                Log.Error("payment/pay 参数异常：pmtid为空");
                return Json(new { success = false, msg = "参数异常" }); ;
            }
            #if DEBUG
            Log.Debug("Pay ids:" + ids);
            #endif
            var orderIdArr = ids.Split(',').Select(item => long.Parse(item));
            //获取待支付的所有订单
            var orders = _iOrderService.GetOrders(orderIdArr).Where(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay && item.UserId == CurrentUser.Id).ToList();

            if (orders == null || orders.Count == 0) //订单状态不正确
            {
                Log.Error("payment/pay 未找到可支付的订单");
                return Json(new { success = false, msg = "未找到可支付的订单" }); ;
            }

            decimal total = orders.Sum(a => a.OrderTotalAmount);
            if (total == 0)
            {
                Log.Error("payment/pay 支付金额不能为0");
                return Json(new { success = false, msg = "支付金额不能为0" }); ;
            }

            foreach (var item in orders)
            {
                if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    if (!_iFightGroupService.OrderCanPay(item.Id))
                    {
                        Log.Error("payment/pay 有拼团订单为不可付款状态");
                        return Json(new { success = false, msg = "有拼团订单为不可付款状态" }); 
                        //throw new HimallException("有拼团订单为不可付款状态");
                    }
                }
            }

            //获取所有订单中的商品名称
            var productInfos = GetProductNameDescriptionFromOrders(orders);
            string webRoot = Request.Url.Scheme + "://" + Request.Url.Authority;
            string urlPre = webRoot + "/m-" + PlatformType + "/Payment/";
            //获取同步返回地址
            string returnUrl = webRoot + "/Pay/Return/{0}";
            //获取异步通知地址
            string payNotify = webRoot + "/Pay/Notify/{0}";
            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + PlatformType + "/Member/PaymentToOrders?ids=" + ids;
            if (pmtid.ToLower().Contains("weixin"))
            {//微信里跳转到分享页面
                //支付成功后晒单地址(source=pay用来区分从哪个页面跳转过去的)
                returnPre = webRoot + "/m-" + PlatformType + "/Order/OrderShare?source=pay&orderids=" + ids;
            }
            var payment = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(d => d.PluginInfo.PluginId == pmtid);
            if (payment == null)
            {
                throw new HimallException("错误的支付方式");
            }
            string openId = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER_OpenID);
            if (!string.IsNullOrWhiteSpace(openId))
            {
                openId = Core.Helper.SecureHelper.AESDecrypt(openId, "Mobile");
            }
            else
            {
                var openUserInfo = _iMemberService.GetMember(CurrentUser.Id).MemberOpenIdInfo.FirstOrDefault(item => item.AppIdType == MemberOpenIdInfo.AppIdTypeEnum.Payment);
                if (openUserInfo != null)
                    openId = openUserInfo.OpenId;
            }

            #region 支付流水获取
            var orderPayModel = orders.Select(item => new OrderPayInfo
            {
                PayId = 0,
                OrderId = item.Id
            });
            //保存支付订单
            long payid = _iOrderService.SaveOrderPayInfo(orderPayModel, PlatformType);
            #endregion
            #if DEBUG
            Log.Debug("Pay payid:" + payid);
            #endif
            //组织返回Model
            Himall.Web.Models.PayJumpPageModel model = new Himall.Web.Models.PayJumpPageModel();
            model.PaymentId = pmtid;
            model.OrderIds = ids;
            model.TotalPrice = total;
            model.UrlType = payment.Biz.RequestUrlType; ;
            model.PayId = payid;
            try
            { 
                #if DEBUG
                Core.Log.Info("其他详情 :  returnPre = " + returnPre + " notifyPre = " + notifyPre + payment.PluginInfo.PluginId.Replace(".", "-") + " ids = " + ids + " totalAmount=" + total + " productInfos=" + productInfos + " openId=" + openId);
                #endif
                model.RequestUrl = payment.Biz.GetRequestUrl(returnPre, notifyPre + payment.PluginInfo.PluginId.Replace(".", "-"), payid.ToString(), total, productInfos, openId);
				Log.Debug(string.Format("openId:{0} Url:{1}", openId, model.RequestUrl));
            }
            catch (Exception ex)
            {
                Core.Log.Error("支付页面加载支付插件出错：", ex);
                throw new HimallException("错误的支付方式");
            }
            #if DEBUG
            Core.Log.Info("支付方式详情 :  id = " + payment.PluginInfo.PluginId + " name = " + payment.PluginInfo.DisplayName + " url = " + model.RequestUrl);
            #endif    
            if (string.IsNullOrWhiteSpace(model.RequestUrl))
            {
                throw new HimallException("错误的支付方式,获取支付地址为空");
            }
            switch (model.UrlType)
            {
                case UrlType.Page:
                    return Json(new { success = true, msg = "", jumpUrl = model.RequestUrl }); 
                    //return Redirect(model.RequestUrl);
                    break;
                case UrlType.QRCode:
                    return Json(new { success = true, msg = "", jumpUrl = "/Pay/QRPay/?id=" + pmtid + "&url=" + model.RequestUrl + "&orderIds=" + ids }); 
                    //return Redirect("/Pay/QRPay/?id=" + pmtid + "&url=" + model.RequestUrl + "&orderIds=" + ids);
                    break;
            }
            return Json(new { success = false, msg = "调用支付方式异常"}); 
        }
        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        // GET: Mobile/Payment
        public JsonResult Get(string orderIds)
        {
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType));
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
            string urlPre = webRoot + "/m-" + PlatformType + "/Payment/";

            //获取待支付的所有订单
            var orderService = _iOrderService;
            IEnumerable<OrderInfo> orders = orderService.GetOrders(orderIds.Split(',').Select(t => long.Parse(t))).ToList();
            IEnumerable<OrderInfo> waitPayOrders = orders.Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);
            var totalAmount = waitPayOrders.Sum(t => t.OrderTotalAmount);

            /* 移到 Payment/pay实现 lly
            //获取所有订单中的商品名称
            string productInfos = GetProductNameDescriptionFromOrders(orders);
            string openId = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER_OpenID);
            if (!string.IsNullOrWhiteSpace(openId))
            {
                openId = Core.Helper.SecureHelper.AESDecrypt(openId, "Mobile");
            }
            else
            {
                var openUserInfo = _iMemberService.GetMember(CurrentUser.Id).MemberOpenIdInfo.FirstOrDefault(item => item.AppIdType == MemberOpenIdInfo.AppIdTypeEnum.Payment);
                if (openUserInfo != null)
                    openId = openUserInfo.OpenId;
            }
            string[] strIds = orderIds.Split(',');
            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + PlatformType + "/Member/PaymentToOrders?ids=" + orderIds;

            var orderPayModel = waitPayOrders.Select(p => new OrderPayInfo
            {
                PayId = 0,
                OrderId = p.Id
            });

            //保存支付订单
            var payid = orderService.SaveOrderPayInfo(orderPayModel, PlatformType);
            var ids = payid.ToString();
             * */
            var model = mobilePayments.ToArray().Select(item =>
               {
                   string url = string.Empty;
                   return new
                   {
                       id = item.PluginInfo.PluginId,
                       name = item.PluginInfo.DisplayName,
                       logo = item.Biz.Logo,
                       url = url
                   };
               });
            foreach (var item in model)
            {
                Core.Log.Debug(item.id + "   " + item.name);
            }
            return Json(new { data = model, totalAmount = totalAmount });
        }

        [ValidateInput(false)]
        public ContentResult Notify(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(this.HttpContext.Request);
                if (payInfo != null)
                {
                    var orderid = payInfo.OrderIds.FirstOrDefault();
                    var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();
                    var payTime = payInfo.TradeTime;
                    _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);
                    //写入支付状态缓存
                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true, 15);//标记为已支付
                    PaymentHelper.IncreaseSaleCount(orderIds);
                    PaymentHelper.GenerateBonus(orderIds, Request.Url.Host.ToString());
                    response = payment.Biz.ConfirmPayResult();

                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Core.Log.Error("移动端支付异步通知返回出错，支持方式：" + id, ex);
            }
            return Content(response);
        }

        [ValidateInput(false)]
        public ContentResult NotifyVirtualOrder(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                //获取传统context
                var payInfo = payment.Biz.ProcessNotify(this.HttpContext.Request);
                if (payInfo != null)
                {
                    var orderid = payInfo.OrderIds.FirstOrDefault();
                    IVirtualOrderService virtualOrderService = ServiceHelper.Create<IVirtualOrderService>();
                    //更新付款状态
                    virtualOrderService.UpdateMoneyFlagByPayNum(orderid.ToString());
                    //更新商家账户的待结算金额和余额
                    virtualOrderService.UpdateShopAccountByPayNum(orderid.ToString());
                    //更新平台账户的待结算金额和余额
                    virtualOrderService.UpdatePlatAccountByPayNum(orderid.ToString());
                    //更新平台待结算金额
                    response = payment.Biz.ConfirmPayResult();                
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Core.Log.Error("移动端支付异步通知返回出错，支持方式：" + id, ex);
            }
            return Content(response);
        }
        public ActionResult Return(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
                if (payInfo != null)
                {
                    var payTime = payInfo.TradeTime;

                    var orderid = payInfo.OrderIds.FirstOrDefault();
                    var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();

                    ViewBag.OrderIds = string.Join(",", orderIds);
                    _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);

                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true, 15);//标记为已支付

                    var order = _iOrderService.GetOrder(orderid);
                    if(order!=null)
                    {
                        if(order.OrderType== OrderInfo.OrderTypes.FightGroup)
                        {
                            var gpord = _iFightGroupService.GetOrder(orderid);
                            if(gpord!=null)
                            {
                                return Redirect(string.Format("/m-{0}/FightGroup/GroupOrderOk?orderid={1}", PlatformType.ToString(),orderid));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Core.Log.Error("移动端同步返回出错，支持方式：" + id, ex);
            }
            ViewBag.Error = errorMsg;

            return View();
        }



        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }


        string GetProductNameDescriptionFromOrders(IEnumerable<OrderInfo> orders)
        {
            List<string> productNames = new List<string>();
            foreach (var order in orders)
                productNames.AddRange(order.OrderItemInfo.Select(t => t.ProductName));
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }
        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        public JsonResult GetPayPwd()
        {
            bool result = false;
            result = OrderApplication.GetPayPwd(UserId);
            return Json(new { success = result });
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public JsonResult SetPayPwd(string pwd)
        {
            _iMemberCapitalService.SetPayPwd(CurrentUser.Id, pwd);
            return Json(new { success = true, msg = "设置成功" });
        }
        [ActionName("CapitalChargeNotify")]
        [ValidateInput(false)]
        public ContentResult PayNotify_Charge(string id)
        {
            var plugin = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id.Replace("-", "."));
            var payInfo = plugin.Biz.ProcessNotify(this.HttpContext.Request);
            if (payInfo != null)
            {

                var chargeApplyId = payInfo.OrderIds.FirstOrDefault();
                MemberCapitalApplication.ChargeSuccess(chargeApplyId);
                var response = plugin.Biz.ConfirmPayResult();
                return Content(response);
            }
            return Content(string.Empty);
        }
    }
}