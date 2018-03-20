using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Himall.Core;
using Himall.Model;
using System.Collections.Generic;
using Himall.Web.App_Code.Common;
using System.Text;

namespace Himall.Web.Areas.Web.Controllers
{
    public class PayController : BaseWebController
    {
        private IOrderService _iOrderService;
        private ICashDepositsService _iCashDepositsService;
        private IMemberCapitalService _iMemberCapitalService;
        private IRefundService _iRefundService;
        private IShopService _iShopService;
        private IFightGroupService _iFightGroupService;

        public PayController(IOrderService iOrderService, ICashDepositsService iCashDepositsService, IMemberCapitalService iMemberCapitalService
            , IRefundService iRefundService, IShopService iShopService, IFightGroupService iFightGroupService)
        {
            _iOrderService = iOrderService;
            _iCashDepositsService = iCashDepositsService;
            _iMemberCapitalService = iMemberCapitalService;
            _iRefundService = iRefundService;
            _iShopService = iShopService;
            _iFightGroupService = iFightGroupService;
        }
        /// <summary>
        /// 支付跳转页
        /// <para>完成支付前参数整理工作，并跳转到支付页面</para>
        /// </summary>
        /// <param name="pmtid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult Index(string pmtid, string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
            }
            if (string.IsNullOrWhiteSpace(pmtid))
            {
                return RedirectToAction("Pay", "Order", new { orderIds = ids });
            }
            var orderIdArr = ids.Split(',').Select(item => long.Parse(item));
            //获取待支付的所有订单
            var orders = _iOrderService.GetOrders(orderIdArr).Where(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay && item.UserId == CurrentUser.Id).ToList();

            if (orders == null || orders.Count == 0) //订单状态不正确
            {
                return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
            }

            decimal total = orders.Sum(a => a.OrderTotalAmount);
            if (total == 0)
            {
                return RedirectToAction("Pay", "Order", new { orderIds = ids });
            }

            foreach (var item in orders)
            {
                if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    if (!_iFightGroupService.OrderCanPay(item.Id))
                    {
                        throw new HimallException("有拼团订单为不可付款状态");
                    }
                }
            }

            //获取所有订单中的商品名称
            var productInfos = GetProductNameDescriptionFromOrders(orders);
            string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
            //获取同步返回地址
            string returnUrl = webRoot + "/Pay/Return/{0}";
            //获取异步通知地址
            string payNotify = webRoot + "/Pay/Notify/{0}";
            var payment = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(d => d.PluginInfo.PluginId == pmtid);
            if (payment == null)
            {
                throw new HimallException("错误的支付方式");
            }

            #region 支付流水获取
            var orderPayModel = orders.Select(item => new OrderPayInfo
            {
                PayId = 0,
                OrderId = item.Id
            });
            //保存支付订单
            long payid = _iOrderService.SaveOrderPayInfo(orderPayModel, PlatformType.PC);
            #endregion

            //组织返回Model
            Himall.Web.Models.PayJumpPageModel model = new Himall.Web.Models.PayJumpPageModel();
            model.PaymentId = pmtid;
            model.OrderIds = ids;
            model.TotalPrice = total;
            model.UrlType = payment.Biz.RequestUrlType; ;
            model.PayId = payid;
            try
            {
                model.RequestUrl = payment.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(payment.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(payment.PluginInfo.PluginId)), payid.ToString(), total, productInfos);
            }
            catch (Exception ex)
            {
                Core.Log.Error("支付页面加载支付插件出错", ex);
                model.IsErro = true;
                model.ErroMsg = ex.Message + ",请检查平台支付配置";
            }
            if (string.IsNullOrWhiteSpace(model.RequestUrl))
            {
                model.IsErro = true;
                model.ErroMsg = "获取支付地址为空,请检查平台支付配置";
            }
            switch (model.UrlType)
            {
                case UrlType.Page:
                    return Redirect(model.RequestUrl);
                    break;
                case UrlType.QRCode:
                    return Redirect("/Pay/QRPay/?id=" + pmtid + "&url=" + model.RequestUrl + "&orderIds=" + ids);
                    break;
            }
            //form提交在页面组织参数并自动提交
            return View(model);
        }

        /// <summary>
        /// 获取订单内商品名称
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private string GetProductNameDescriptionFromOrders(IEnumerable<OrderInfo> orders)
        {
            List<string> productNames = new List<string>();
            foreach (var order in orders.ToList())
                productNames.AddRange(order.OrderItemInfo.Select(t => t.ProductName));
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }


        [ActionName("Notify")]
        [ValidateInput(false)]
        public ContentResult PayNotify_Post(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(HttpContext.Request);
                var payTime = payInfo.TradeTime;
                var orderid = payInfo.OrderIds.FirstOrDefault();
                var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();
                try
                {
                    _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);
                    response = payment.Biz.ConfirmPayResult();
                    //写入支付状态缓存
                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true, 15);//标记为已支付
                    //限时购销量
                    PaymentHelper.IncreaseSaleCount(orderIds);
                    //红包
                    PaymentHelper.GenerateBonus(orderIds, Request.Url.Host.ToString());
                }
                catch (Exception e)
                {
                    string logErr = id + " " + orderid.ToString();
                    if (payInfo.TradeTime.HasValue)
                    {
                        logErr += " TradeTime:" + payInfo.TradeTime.Value.ToString();
                    }
                    logErr += " TradNo:" + payInfo.TradNo;
                    Log.Error(logErr, e);
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Error("PayNotify_Post", ex);
            }
            return Content(response);
        }
        #region 退款异步通知
        [ActionName("RefundNotify")]
        [ValidateInput(false)]
        public ContentResult RefundNotify_Post(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;

            Log.Info("[异步退款]开始：" + id);
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessRefundNotify(HttpContext.Request);

                if (payInfo != null)
                {
                    string refund_batch_no = payInfo.TradNo;
                    DateTime? refund_time = payInfo.TradeTime;

                    try
                    {
                        _iRefundService.NotifyRefund(refund_batch_no);
                        response = payment.Biz.ConfirmPayResult();    
                    }
                    catch (Exception e)
                    {
                        string logErr = id + " 退款异常";
                        if (payInfo.TradeTime.HasValue)
                        {
                            logErr += " RefundTime:" + refund_time.ToString();
                        }
                        logErr += " RefundBatchNo:" + refund_batch_no;
                        Log.Error(logErr, e);
                    }
                }
                else
                {
                    Log.Info("[异步退款]失败：" + id + " - 插件实例失败");
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Info("[异步退款]失败：" + id);
                Log.Error("RefundNotify_Post", ex);
            }
            return Content(response);
        }
        #endregion

        #region 缴纳保证金回调处理
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
                Log.Error("CashPayNotify_Post", ex);
            }
            return Content(response);
        }
        #endregion 保证金

        //TODO:[LLY]增加预付款充值回调
        #region 预付款充值回调
        [ActionName("CapitalChargeNotify")]
        [ValidateInput(false)]
        public ContentResult PayNotify_Charge(string id)
        {
            string result = string.Empty;
            try
            {
                id = DecodePaymentId(id);
                var payPlugin = PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                if (payPlugin != null)
                {
                    var paymentInfo = payPlugin.Biz.ProcessNotify(Request);
                    var service = _iMemberCapitalService;
                    var chargeInfo = service.GetChargeDetail(paymentInfo.OrderIds.FirstOrDefault());
                    if (chargeInfo != null && chargeInfo.ChargeStatus != ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
                    {
                        chargeInfo.ChargeWay = payPlugin.PluginInfo.DisplayName;
                        chargeInfo.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                        chargeInfo.ChargeTime = paymentInfo.TradeTime.HasValue ? paymentInfo.TradeTime.Value : DateTime.Now;
                        service.UpdateChargeDetail(chargeInfo);
                        result = payPlugin.Biz.ConfirmPayResult();
                        string payStateKey = CacheKeyCollection.PaymentState(chargeInfo.Id.ToString());//获取支付状态缓存键
                        Cache.Insert(payStateKey, true, 15);//标记为已支付
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("预付款充值回调出错：" + ex.Message);
            }
            return Content(result);
        }

        [ActionName("CapitalChargeReturn")]
        [ValidateInput(false)]
        public ActionResult PayReturn_Charge(string id)
        {
            string result = string.Empty;
            try
            {
                id = DecodePaymentId(id);
                var payPlugin = PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                if (payPlugin != null)
                {
                    var paymentInfo = payPlugin.Biz.ProcessReturn(Request);
                    var service = _iMemberCapitalService;
                    var chargeInfo = service.GetChargeDetail(paymentInfo.OrderIds.FirstOrDefault());
                    if (chargeInfo != null && chargeInfo.ChargeStatus != ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
                    {
                        chargeInfo.ChargeWay = payPlugin.PluginInfo.DisplayName;
                        chargeInfo.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                        chargeInfo.ChargeTime = paymentInfo.TradeTime.HasValue ? paymentInfo.TradeTime.Value : DateTime.Now;
                        service.UpdateChargeDetail(chargeInfo);
                        result = payPlugin.Biz.ConfirmPayResult();
                        string payStateKey = CacheKeyCollection.PaymentState(chargeInfo.Id.ToString());//获取支付状态缓存键
                        Cache.Insert(payStateKey, true, 15);//标记为已支付
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("预付款充值回调出错：" + ex.Message);
            }
            return View();
        }
        #endregion

        #region 店铺续费支付回调处理

        [ActionName("ReNewPayNotify")]
        [ValidateInput(false)]
        public ContentResult ReNewPayNotify_Post(string id, string str)
        {
            decimal balance = decimal.Parse(str.Split('-')[0]);
            string userName = str.Split('-')[1];
            long shopId = long.Parse(str.Split('-')[2]);
            int type = int.Parse(str.Split('-')[3]);
            int value = int.Parse(str.Split('-')[4]);
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            string response = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessNotify(HttpContext.Request);
                ShopRenewRecord model = new ShopRenewRecord();
                model.TradeNo = payInfo.TradNo;              
                bool result = Cache.Get(CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds))) == null ? false : true;
                if (!result)
                {
                    //添加店铺续费记录
                    model.ShopId = shopId;
                    model.OperateDate = DateTime.Now;
                    model.Operator = userName;
                    model.Amount = balance;
                    //续费操作
                    if (type == 1)
                    {
                        model.OperateType = ShopRenewRecord.EnumOperateType.ReNew;
                        var shopInfo = _iShopService.GetShop(shopId);
                        DateTime beginTime = shopInfo.EndDate.Value;
                        if (beginTime < DateTime.Now)
                            beginTime = DateTime.Now;
                        string strNewEndTime = beginTime.AddYears(value).ToString("yyyy-MM-dd");
                        model.OperateContent = "续费 " + value + " 年至 " + strNewEndTime;
                        _iShopService.AddShopRenewRecord(model);
                        //店铺续费
                        _iShopService.ShopReNew(shopId, value);
                    }
                    //升级操作
                    else
                    {
                        model.ShopId = shopId;
                        model.OperateType = ShopRenewRecord.EnumOperateType.Upgrade;
                        var shopInfo = _iShopService.GetShop(shopId);
                        var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                        var newshopGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)value).FirstOrDefault();
                        model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                        _iShopService.AddShopRenewRecord(model);
                        //店铺升级
                        _iShopService.ShopUpGrade(shopId, (long)value);
                    }
                    response = payment.Biz.ConfirmPayResult();
                    string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                    Cache.Insert(payStateKey, true);//标记为已支付
                    if (Cache.Exists(CacheKeyCollection.CACHE_SHOP(shopId, false)))
                        Cache.Remove(CacheKeyCollection.CACHE_SHOP(shopId, false));
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Log.Error("ReNewPayNotify_Post", ex);
            }
            return Content(response);
        }

        #endregion

        public ActionResult QRPay(string url, string id)
        {
            ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo


            var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
            ViewBag.Title = payment.PluginInfo.DisplayName + "支付";
            ViewBag.Name = payment.PluginInfo.DisplayName;

            //生成二维码
            var qrCode = Core.Helper.QRCodeHelper.Create(url);
            string fileName = DateTime.Now.ToString("yyMMddHHmmssffffff") + ".jpg";
            var qrCodeImagePath = "/temp/" + fileName;
            qrCode.Save(Server.MapPath("~/temp/") + fileName);
            ViewBag.QRCode = qrCodeImagePath;
            ViewBag.HelpImage = "/Plugins/Payment/" + payment.PluginInfo.ClassFullName.Split(',')[1] + "/" + payment.Biz.HelpImage;

            ViewBag.Step = 2;//支付第二步
            return View();
        }

        public ActionResult ReturnSuccess(string id)
        {
            ViewBag.OrderIds = Request.QueryString[id];
            ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
            return View("Return");
        }


        public ActionResult Return(string id)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;
            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(HttpContext.Request);
                var payTime = payInfo.TradeTime;

                var orderid = payInfo.OrderIds.FirstOrDefault();
                var orderIds = _iOrderService.GetOrderPay(orderid).Select(item => item.OrderId).ToList();

                ViewBag.OrderIds = string.Join(",", orderIds);
                _iOrderService.PaySucceed(orderIds, id, payInfo.TradeTime.Value, payInfo.TradNo, payId: orderid);

                //写入支付状态缓存
                string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                Cache.Insert(payStateKey, true, 15);//标记为已支付

                //红包
                PaymentHelper.GenerateBonus(orderIds, Request.Url.Host.ToString());

                //Dictionary<long , ShopBonusInfo> bonusGrantIds = new Dictionary<long , ShopBonusInfo>();
                //string url = "http://" + Request.Url.Host.ToString() + "/m-weixin/shopbonus/index/";
                //var bonusService = ServiceHelper.Create<IShopBonusService>();
                //var buyOrders = _iOrderService.GetOrders( orderIds.AsEnumerable() );
                //foreach( var o in buyOrders )
                //{
                //    var shopBonus = bonusService.GetByShopId( o.ShopId );
                //    if( shopBonus != null && shopBonus.GrantPrice <= o.OrderTotalAmount )
                //    {
                //        long grantid = bonusService.GenerateBonusDetail( shopBonus , o.UserId , o.Id , url );
                //        bonusGrantIds.Add( grantid , shopBonus );
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.Error("pay Return:" + ex.Message);
                errorMsg = ex.Message;
                Log.Error(errorMsg);
            }
            ViewBag.Error = errorMsg;
            ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
            return View();
        }


        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

    }


    public class PayStateController : BaseAsyncController
    {
        public void CheckAsync(string orderIds)
        {
            AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为15s
            Task.Factory.StartNew(() =>
               {
                   string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                   object payStateObj = null;
                   int time = 0;
                   while (true)
                   {
                       //检查是否已经建立缓存
                       payStateObj = Cache.Get(payStateKey);
                       if (payStateObj == null)
                       {//没有进入缓存
                           var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
                           //检查对应订单是否已经支付

                           using (var service = ServiceHelper.Create<IOrderService>())
                           {
                               var payDone = !service.GetOrders(orderIdArr).Any(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitPay);
                               Cache.Insert(payStateKey, payDone, 15);//标记支付状态
                           }
                       }

                       //检查缓存的值，如果已支付则直接结束，若未替并小于15秒则阻塞200ms后重复检查，否则直接结束
                       var payState = (bool)Cache.Get(payStateKey);
                       if (payState)
                       {//如果已成功支付，则调用成功
                           AsyncManager.Parameters["done"] = true;
                           break;
                       }
                       if (time > maxWaitingTime)
                       {//大于15秒
                           AsyncManager.Parameters["done"] = false;
                           break;
                       }
                       else
                       {
                           time += interval;
                           System.Threading.Thread.Sleep(interval);
                       }
                   }
                   AsyncManager.OutstandingOperations.Decrement();
               });
        }


        public JsonResult CheckCompleted(bool done)
        {
            return Json(new { success = done }, JsonRequestBehavior.AllowGet);
        }

        public void CheckChargeAsync(string orderIds)
        {
            AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为15s
            Task.Factory.StartNew(() =>
            {
                string payStateKey = CacheKeyCollection.PaymentState(orderIds);//获取支付状态缓存键
                object payStateObj = null;
                int time = 0;
                while (true)
                {
                    //检查是否已经建立缓存
                    payStateObj = Cache.Get(payStateKey);
                    if (payStateObj == null)
                    {//没有进入缓存
                        //检查对应订单是否已经支付

                        using (var service = ServiceHelper.Create<IMemberCapitalService>())
                        {
                            var model = service.GetChargeDetail(long.Parse(orderIds));
                            var payDone = model != null && model.ChargeStatus == ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                            Cache.Insert(payStateKey, payDone, 15);//标记支付状态
                        }
                    }

                    //检查缓存的值，如果已支付则直接结束，若未替并小于15秒则阻塞200ms后重复检查，否则直接结束
                    var payState = (bool)Cache.Get(payStateKey);
                    if (payState)
                    {//如果已成功支付，则调用成功
                        AsyncManager.Parameters["done"] = true;
                        break;
                    }
                    if (time > maxWaitingTime)
                    {//大于15秒
                        AsyncManager.Parameters["done"] = false;
                        break;
                    }
                    else
                    {
                        time += interval;
                        System.Threading.Thread.Sleep(interval);
                    }
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public JsonResult CheckChargeCompleted(bool done)
        {
            return Json(new { success = done }, JsonRequestBehavior.AllowGet);
        }
    }
}