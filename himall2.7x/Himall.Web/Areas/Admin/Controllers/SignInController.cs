using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Himall.Core;
using System.Drawing;
using Himall.Core.Helper;
using Himall.IServices.QueryModel;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

namespace Himall.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// 签到
    /// </summary>
    public class SignInController : BaseAdminController
    {
        private IMemberSignInService _iMemberSignInService;
        public SignInController(IMemberSignInService iMemberSignInService)
        {
            _iMemberSignInService = iMemberSignInService;
            #region 数据关系映射
            Mapper.CreateMap<SiteSignInConfigInfo, SiteSignInConfigModel>();
            Mapper.CreateMap<SiteSignInConfigModel, SiteSignInConfigInfo>();
            #endregion
        }
        public ActionResult Setting()
        {
            SiteSignInConfigInfo data = _iMemberSignInService.GetConfig();
            SiteSignInConfigModel model = new SiteSignInConfigModel();
            model = Mapper.Map<SiteSignInConfigInfo, SiteSignInConfigModel>(data);
            return View(model);
        }
        [HttpPost]
        public JsonResult Setting(SiteSignInConfigModel model)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (ModelState.IsValid)
            {
                if (model.DayIntegral == 0)
                {
                    model.IsEnable = false;
                }
                SiteSignInConfigModel postdata = new SiteSignInConfigModel();
                SiteSignInConfigInfo data = _iMemberSignInService.GetConfig();
                postdata = Mapper.Map<SiteSignInConfigInfo, SiteSignInConfigModel>(data);
                UpdateModel(postdata);
                data = Mapper.Map<SiteSignInConfigModel, SiteSignInConfigInfo>(postdata);
                _iMemberSignInService.SaveConfig(data);
                result.success = true;
                result.msg = "配置签到成功";
            }
            else
            {
                result.success = false;
                result.msg = "数据错误";
            }
            return Json(result);
        }
    }
}