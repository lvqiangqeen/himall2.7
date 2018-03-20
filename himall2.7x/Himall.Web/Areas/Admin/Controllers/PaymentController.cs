using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.Web.Framework;

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Himall.IServices;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class PaymentController : BaseAdminController
    {
        private IPaymentConfigService _iPaymentConfigService;
        public PaymentController( IPaymentConfigService iPaymentConfigService )
        {
            _iPaymentConfigService = iPaymentConfigService;
        }

        // GET: Admin/Payment
        public ActionResult Management()
        {
            var paymentPlugins = PluginsManagement.GetPlugins<IPaymentPlugin>();
            var data = paymentPlugins.Select(item =>
                  {
                    dynamic model = new ExpandoObject();
                    model.name = item.PluginInfo.DisplayName;
                    model.pluginId = item.PluginInfo.PluginId;
                    model.enable = item.PluginInfo.Enable;
                    return model;
                  }
                );
            ViewBag.IsReceivingAddress = _iPaymentConfigService.IsEnable();
            return View(data);
        }


        public ActionResult Edit(string pluginId)
        {
            ViewBag.Id = pluginId;

            var payment = PluginsManagement.GetPlugin<IPaymentPlugin>(pluginId);
            ViewBag.Name = payment.PluginInfo.DisplayName;
            var formData = payment.Biz.GetFormData();

            return View(formData);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Save(string pluginId, string values)
        {
            IPaymentPlugin paymentPlugin = PluginsManagement.GetPlugin<IPaymentPlugin>(pluginId).Biz;
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(values);
            paymentPlugin.SetFormValues(items);


            return Json(new { success = true });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult Enable(string pluginId, bool enable)
        {
            Result result = new Result();
            PluginsManagement.EnablePlugin(pluginId, enable);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult ChangeReceivingAddressState( bool enable )
        {
            Result result = new Result();
            if( enable )
            {
                _iPaymentConfigService.Enable();
            }
            else
            {
                _iPaymentConfigService.Disable();
            }
            result.success = true;
            return Json( result );
        }
    }
}