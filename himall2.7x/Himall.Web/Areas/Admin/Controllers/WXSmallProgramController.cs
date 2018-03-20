using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class WXSmallProgramController : BaseAdminController
    {
        // GET: Admin/WXSmallProgram
        private IWXSmallProgramService _iWXSmallProgramService;
        ISiteSettingService _iSiteSettingService;
        private IWXMsgTemplateService _iWXMsgTemplateService;

        public WXSmallProgramController(
            IWXSmallProgramService iWXSmallProgramService, ISiteSettingService iSiteSettingService, IWXMsgTemplateService iWXMsgTemplateService)
        {
            _iWXSmallProgramService = iWXSmallProgramService;
            _iSiteSettingService = iSiteSettingService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }
        public ActionResult HomePageSetting()
        {
            VTemplateEditModel model = new Models.VTemplateEditModel();
            model.ClientType = VTemplateClientTypes.WXSmallProgram;
            model.Name = "smallprog";
            //门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return View(model);
        }

        public ActionResult ProductSetting()
        {
            return View();
        }

        /// <summary>
        /// 设置小程序商品
        /// </summary>
        /// <param name="productIds">商品ID，用','号隔开</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddWXSmallProducts(string productIds)
        {
            WXSmallProgramApplication.SetWXSmallProducts(productIds);
            return Json(new { success = true });
        }

        /// <summary>
        /// 查询已绑定的商品信息
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetWXSmallProducts(int page, int rows, string keyWords, long? categoryId = null)
        {
            Himall.Model.ObsoletePageModel<Himall.Model.ProductInfo> datasql = _iWXSmallProgramService.GetWXSmallProducts(page, rows);

            IEnumerable<ProductModel> products = datasql.Models.ToArray().Select(item => new ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandName,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice,
                state= item.ShowProductState,
                productCode = item.ProductCode
            });
            DataGridModel<ProductModel> dataGrid = new DataGridModel<ProductModel>() { rows = products, total = datasql.Total };
            return Json(dataGrid);
        }

        /// <summary>
        /// 删除对应商品
        /// </summary>
        /// <param name="Id">设置ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteWXSmallProductById(long Id)
        {
            _iWXSmallProgramService.DeleteWXSmallProductById(Id);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteList(string ids)
        {
            var strArr = ids.Split(',');
            List<long> listid = new List<long>();
            foreach (var arr in strArr)
            {
                listid.Add(Convert.ToInt64(arr));
            }
            _iWXSmallProgramService.DeleteWXSmallProductByIds(listid.ToArray());
            return Json(new Result() { success = true, msg = "批量删除成功！" });
        }
        #region 微信模版
        public ActionResult EditWXMessage()
        {
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item =>
            {
                dynamic model = new ExpandoObject();
                model.name = item.PluginInfo.DisplayName;
                model.pluginId = item.PluginInfo.PluginId;
                model.enable = item.PluginInfo.Enable;
                model.status = item.Biz.GetAllStatus();
                return model;
            }
                );

            ViewBag.messagePlugins = data;

            List<WeiXinMsgTemplateInfo> wxtempllist = new List<WeiXinMsgTemplateInfo>();
            wxtempllist = _iWXMsgTemplateService.GetWeiXinMsgTemplateListByApplet();
            return View(wxtempllist);
        }

        #endregion

        [HttpPost]
        [UnAuthorize]
        [ValidateInput(false)]
        public JsonResult Save(string values)
        {
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(values);
            _iWXMsgTemplateService.UpdateWXsmallMessage(items);
            return Json(new { success = true });
        }
    }
}