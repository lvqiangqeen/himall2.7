using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.Core;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using System.IO;
using Himall.DTO;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ThemeController : BaseAdminController
    {

        public ActionResult Index()
        {
            return View(ThemeApplication.getTheme());
        }


        /// <summary>
        /// 修改主题设置
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="typeId">0、默认；1、自定义主题</param>
        /// <param name="MainColor">主色</param>
        /// <param name="SecondaryColor">商城辅色</param>
        /// <param name="WritingColor">字体颜色</param>
        /// <param name="FrameColor">边框颜色</param>
        /// <param name="ClassifiedsColor">边框栏颜色</param>
        /// <returns></returns>
        public JsonResult updateTheme(long id, int typeId, string MainColor = "", string SecondaryColor = "", string WritingColor = "", string FrameColor = "", string ClassifiedsColor = "")
        {
            Theme mVTheme = new Theme()
            {
                ThemeId = id,
                TypeId = (Himall.CommonModel.ThemeType)typeId,
                MainColor = MainColor,
                SecondaryColor = SecondaryColor,
                WritingColor = WritingColor,
                FrameColor = FrameColor,
                ClassifiedsColor = ClassifiedsColor
            };

            ThemeApplication.SetTheme(mVTheme);

            return Json(new
            {
                status = 1
            });
        }
    }
}