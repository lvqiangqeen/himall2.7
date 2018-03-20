using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Himall.Core.Plugins.Payment;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Himall.Web.Framework;


namespace Himall.SmallProgAPI
{
    public class VirtualOrderController : BaseApiController
    {

        public object GetPrepaidParameter(string openId, decimal payAmount, long shopId)
        {
            CheckUserLogin();
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.WeiXinSmallProg));
            string webRoot = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.Port == 80 ? "" : (":" + HttpContext.Current.Request.Url.Port.ToString()));
            string notifyUrl = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/NotifyVirtualOrder/";
            string orderId = GenerateOrderNumber().ToString();
            var models = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                try
                {
                    url = item.Biz.GetRequestUrl("", notifyUrl + item.PluginInfo.PluginId.Replace(".", "-"), orderId, payAmount, "商家:" + shopId + "快捷收款", CurrentUserOpenId);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("获取支付方式错误：", ex);
                }
                //适配小程序接口，从支付插件里解析出相应参数
                //字符串格式：prepayId:234320480,partnerid:32423489,nonceStr=dslkfjsld
                #region 适配小程序接口，从支付插件里解析出相应参数
                var prepayId = string.Empty;
                var nonceStr = string.Empty;
                var timeStamp = string.Empty;
                var sign = string.Empty;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var paras = url.Split(',');
                    foreach (var str in paras)
                    {
                        var keyValuePair = str.Split(':');
                        if (keyValuePair.Length == 2)
                        {
                            switch (keyValuePair[0])
                            {
                                case "prepayId":
                                    prepayId = keyValuePair[1];
                                    break;
                                case "nonceStr":
                                    nonceStr = keyValuePair[1];
                                    break;
                                case "timeStamp":
                                    timeStamp = keyValuePair[1];
                                    break;
                                case "sign":
                                    sign = keyValuePair[1];
                                    break;
                            }
                        }
                    }
                }
                #endregion 
                return new
                {
                    prepayId = prepayId,
                    nonceStr = nonceStr,
                    timeStamp = timeStamp,
                    sign = sign
                };
            });
            var model = models.FirstOrDefault();
            if (!string.IsNullOrEmpty(model.prepayId))
            {
                IVirtualOrderService virtualOrderService = ServiceHelper.Create<IVirtualOrderService>();
                VirtualOrderInfo entity = new VirtualOrderInfo
                {
                    MoneyFlag = 1,
                    PayAmount = payAmount,
                    PayTime = DateTime.Now,
                    UserId=CurrentUserId,
                    UserName=CurrentUser.Nick,
                    PayNum=orderId,
                    ShopId=shopId
                };
                virtualOrderService.CreateVirtualOrder(entity);
            }
            return Json(new { Status = "OK", Data = model });
        }


        private static object obj = new object();
        private long GenerateOrderNumber()
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 5; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse(DateTime.Now.ToString("yyyyMMddfff") + orderId);
            }
        }

    }
}
