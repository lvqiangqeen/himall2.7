using Himall.IServices;
using Himall.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Himall.Web.Models;
using System.EnterpriseServices;
using Himall.Web.Framework;
using Himall.Core;
using Newtonsoft.Json;
using Himall.Model;


namespace Himall.Web.Areas.Admin.Controllers
{
    public class PrivilegeController : BaseAdminController
    {
        private IPrivilegesService _iPrivilegesService;
        private IManagerService _iManagerService;
        public PrivilegeController(IPrivilegesService iPrivilegesService, IManagerService iManagerService)
        {
            _iPrivilegesService = iPrivilegesService;
            _iManagerService = iManagerService;
        }

        // GET: Admin/Privileges
        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        [Description("角色列表显示")]
        public JsonResult List()
        {
            var list = _iPrivilegesService.GetPlatformRoles();
            var result = list.Select(item => new { Id = item.Id, Name = item.RoleName });
            var model = new { rows = result };
            return Json(model);
        }

        public ActionResult Edit(long id)
        {
            SetPrivileges();
            var model = _iPrivilegesService.GetPlatformRole(id);
            RoleInfoModel result = new RoleInfoModel() { ID = model.Id, RoleName = model.RoleName };
            var s = new Newtonsoft.Json.JsonSerializerSettings();
            s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ViewBag.RolePrivilegeInfo = Newtonsoft.Json.JsonConvert.SerializeObject(model.RolePrivilegeInfo.Select(item => new { Privilege = item.Privilege }), s);
            return View(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Edit(string roleJson, long id)
        {
            if (ModelState.IsValid)
            {
                var s = new Newtonsoft.Json.JsonSerializerSettings();
                s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                Model.RoleInfo role = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.RoleInfo>(roleJson, s);
                role.Id = id;
                _iPrivilegesService.UpdatePlatformRole(role);
                var users = _iManagerService.GetPlatformManagerByRoleId(id).ToList();
                foreach (var user in users)
                {
                    string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(user.Id);
                    Core.Cache.Remove(CACHE_MANAGER_KEY);
                }
            }
            else
            {
                return Json(new { success = true, msg = "验证失败" });
            }
            return Json(new { success = true });
        }

        private void SetPrivileges()
        {
            ViewBag.Privileges = PrivilegeHelper.AdminPrivileges;
        }

        public ActionResult Add()
        {
            SetPrivileges();
            return View();
        }
        [Description("角色添加")]
        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(string roleJson)
        {
            if (ModelState.IsValid)
            {
                var s = new Newtonsoft.Json.JsonSerializerSettings();
                s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                Model.RoleInfo role = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.RoleInfo>(roleJson, s);
                _iPrivilegesService.AddPlatformRole(role);
            }
            else
            {
                return Json(new { success = true, msg = "验证失败" });
            }
            return Json(new { success = true });
        }

        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            var service = _iPrivilegesService;
            var roles = service.GetPlatformRole(id);
            if (_iManagerService.GetPlatformManagerByRoleId(id).Count() > 0)
            {
                return Json(new Result() { success = false, msg = "该角色下还有管理员，不允许删除！" });
            }
            service.DeletePlatformRole(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }
    }
}