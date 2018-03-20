using Himall.Core.Plugins.Express;
using Himall.IServices;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ExpressTemplateController : BaseAdminController
    {
        private IExpressService _iExpressService;
        private ISiteSettingService _iSiteSettingService;

        public ExpressTemplateController(IExpressService iExpressService, ISiteSettingService iSiteSettingService)
        {
            _iExpressService = iExpressService;
            _iSiteSettingService = iSiteSettingService;
        }

        // GET: Admin/ExpressTemplate
        public ActionResult Management()
        {
            var templates = _iExpressService.GetAllExpress();
            return View(templates);
        }

        public ActionResult Setting()
        {
            var siteSetting = CurrentSiteSetting;
            return View(siteSetting);
        }

        public JsonResult SaveExpressSetting(string Kuaidi100Key, int KuaidiType, string KuaidiApp_key, string KuaidiAppSecret)
        {
            var siteSetting = CurrentSiteSetting;
            siteSetting.Kuaidi100Key = Kuaidi100Key;
            siteSetting.KuaidiType = KuaidiType;
            siteSetting.KuaidiApp_key = KuaidiApp_key;
            siteSetting.KuaidiAppSecret = KuaidiAppSecret;
            _iSiteSettingService.SetSiteSettings(siteSetting);
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        public ActionResult Edit(string name)
        {
            var template = _iExpressService.GetExpress(name);
            return View(template);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetConfig(string name)
        {
            var template = _iExpressService.GetExpress(name);
            ExpressTemplateConfig config = new ExpressTemplateConfig()
            {
                width = template.Width,
                height = template.Height,

                data = ExpressPrintElement.AllPrintElements.Select(item => new Element()
                {
                    key = item.Key.ToString(),
                    value = item.Value,
                }).ToArray()
            };
            if (template.Elements != null)
            {
                int i = 0;
                foreach (var element in template.Elements)
                {
                    var item = config.data.FirstOrDefault(t => t.key == element.PrintElementIndex.ToString());
                    item.a = new int[] { element.LeftTopPoint.X, element.LeftTopPoint.Y };
                    item.b = new int[] { element.RightBottomPoint.X, element.RightBottomPoint.Y };
                    item.selected = true;
                    i++;
                }
                config.selectedCount = i;
            }
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Save(string elements, string name)//前台返回的的元素点的X、Y与宽、高的比例
        {
            elements = elements.Replace("\"[", "[").Replace("]\"", "]");
            var expressElements = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ExpressElement>>(elements);

            //获取原模板
            var oriExpressTemplate = _iExpressService.GetExpress(name);

            var newElements = expressElements.Select(item => new ExpressPrintElement()
                {
                    //获取绝对高度与宽度，因为前台放大10000倍，因此这里需要减小10000倍
                    LeftTopPoint = new ExpressPrintElement.Point() { X = item.a[0], Y = item.a[1] },
                    RightBottomPoint = new ExpressPrintElement.Point() { X = item.b[0], Y = item.b[1] },
                    PrintElementIndex = item.name
                });

            _iExpressService.UpdatePrintElement(name, newElements);
            return Json(new { success = true });
        }
    }
}