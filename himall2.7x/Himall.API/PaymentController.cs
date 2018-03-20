using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.Core;
using Himall.Core.Plugins.Payment;
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
using System.Web.Mvc;

namespace Himall.API
{
    public class PaymentController : BaseApiController
    {
        public object GetPaymentList(string orderIds,string typeid)
        {
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.Android));
            string webRoot =HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.Port == 80 ? "" : (":" + HttpContext.Current.Request.Url.Port.ToString()));
            string urlPre = webRoot + "/m-" +Core.PlatformType.Android + "/Payment/";

            //获取待支付的所有订单
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            IEnumerable<OrderInfo> orders = orderService.GetOrders(orderIds.Split(',').Select(t => long.Parse(t))).ToList();
            orders = orders.Where(r => r.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);
            var totalAmount = orders.Sum(t => t.OrderTotalAmount);

            //获取所有订单中的商品名称
            string productInfos = GetProductNameDescriptionFromOrders(orders);
            

            string[] strIds = orderIds.Split(',');
            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + Core.PlatformType.Android + "/Member/PaymentToOrders?ids=" + orderIds;
            var orderPayModel = strIds.Select(item => new OrderPayInfo
            {
                PayId = 0,
                OrderId = long.Parse(item)
            });
            //保存支付订单
            var payid = orderService.SaveOrderPayInfo(orderPayModel, Core.PlatformType.Android);
            var ids = payid.ToString();

            var model = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                try
                {
                    url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-"), ids, totalAmount, productInfos);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("获取支付方式错误：", ex);
                }
                return new
                {
                    id = item.PluginInfo.PluginId,
                    name = item.PluginInfo.DisplayName,
                    logo = item.Biz.Logo,
                    url = url
                };
            });
            model = model.Where(item => !string.IsNullOrWhiteSpace(item.url) && item.id.Contains(typeid));
            return Json(model);
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
        /// 预付款支付
        /// </summary>
        /// <param name="pmtidpmtid"></param>
        /// <param name="ids"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        public object GetPayByCapital(string ids, string payPwd)
        {
            CheckUserLogin();
            var curUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority;
            OrderApplication.PayByCapital(CurrentUser.Id, ids, payPwd, curUrl);
            return Json(new { success = true, msg = "支付成功" });
        }
        public object GetPayByCapitalIsOk(string ids)
        {
            CheckUserLogin();
            var result = OrderApplication.PayByCapitalIsOk(CurrentUser.Id, ids);
            return Json(new { success = result });
        }

        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        public object GetPayPwd()
        {
            CheckUserLogin();
            bool result = false;
            result = OrderApplication.GetPayPwd(CurrentUser.Id);
            return Json(new { success = result });
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public object PostSetPayPwd(LoginModPwdModel model)
        {
            CheckUserLogin();
            MemberCapitalApplication.SetPayPwd(CurrentUser.Id, model.Password);
            return SuccessResult("设置成功");
        }

    }
}
