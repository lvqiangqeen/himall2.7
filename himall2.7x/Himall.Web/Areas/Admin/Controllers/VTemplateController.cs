using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Core;
using System.IO;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// 可视化模板管理
    /// </summary>
    public class VTemplateController : BaseAdminController
    {
        private ITemplateSettingsService _iTemplateSettingsService;
        public VTemplateController(ITemplateSettingsService iTemplateSettingsService)
        {
            _iTemplateSettingsService = iTemplateSettingsService;

        }
        /// <summary>
        /// 设置微商城当前模板
        /// </summary>
        /// <param name="tName"></param>
        /// <returns></returns>
        public JsonResult UpdateCurrentTemplate(string tName)
        {

            if (string.IsNullOrWhiteSpace(tName)) return Json(new { success = false, msg = "模板名称不能为空" });
            _iTemplateSettingsService.SetCurrentTemplate(tName, 0);
            return Json(new { success = true, msg = "模板启用成功" });
        }
        /// <summary>
        /// 微商城模板管理
        /// </summary>
        /// <returns></returns>
        public ActionResult VHomepage()
        {
            string crrentTemplateName = "t1";
            var curr = _iTemplateSettingsService.GetCurrentTemplate(0);
            if (null != curr)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }

            var helper = new GalleryHelper();
            var themes = helper.LoadThemes();
            var CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals(crrentTemplateName.ToLower()));
            if (CurTemplateObj == null)
            {
                CurTemplateObj = themes.FirstOrDefault(t => t.ThemeName.Equals("t1"));
            }
            if (CurTemplateObj == null)
            {
                throw new HimallException("错误的模板：" + crrentTemplateName);
            }
            ViewBag.CurrentTemplate = CurTemplateObj;
            ViewBag.CurUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
            return View(themes.Where(t => t.ThemeName != crrentTemplateName.ToLower()).ToList());
        }

        /// <summary>
        /// 模板编辑
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tName"></param>
        /// <returns></returns>
        public ActionResult EditTemplate(int client = 1, string tName = "t1")
        {
            VTemplateEditModel model = new VTemplateEditModel();
            model.Name = tName;
            model.ClientType = (VTemplateClientTypes)client;
            model.IsShowPrvPage = true;
            switch (model.ClientType)
            {
                case VTemplateClientTypes.WapSpecial:
                case VTemplateClientTypes.SellerWapSpecial:
                    model.IsShowTitle = true;
                    model.IsShowTags = true;
                    model.IsShowPrvPage = false;
                    model.IsShowIcon = true;
                    break;
            }
            //门店授权
            ViewBag.IsOpenStore =  SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return View(model);
        }
    }
}