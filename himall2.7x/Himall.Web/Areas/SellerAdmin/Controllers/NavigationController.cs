using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class NavigationController :BaseSellerController
    {
        private INavigationService _iNavigationService;
        public NavigationController(INavigationService iNavigationService)
        {
            _iNavigationService = iNavigationService;
        }
        
        [Description("获取导航列表数据")]
        public ActionResult Management()
        {
            var result = _iNavigationService.GetSellerNavigations(CurrentSellerManager.ShopId).ToList();
            return View(result);
        }

        [Description("删除导航")]
        [ShopOperationLog(Message = "删除导航")]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iNavigationService.DeleteSellerformNavigation(CurrentSellerManager.ShopId, id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }
         [ShopOperationLog(Message = "新增导航")]
        [Description("新增导航")]
        public JsonResult Add(BannerInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Name) && !string.IsNullOrWhiteSpace(info.Url))
            {
                info.ShopId = CurrentSellerManager.ShopId;
                _iNavigationService.AddSellerNavigation(info);
                return Json(new Result() { success = true, msg = "添加导航成功！" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "导航名称和跳转地址不能为空！" });
            }
        }

        [ShopOperationLog(Message = "编辑导航")]
        public JsonResult Edit(BannerInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Name) && !string.IsNullOrWhiteSpace(info.Url))
            {
                info.ShopId = CurrentSellerManager.ShopId;
                _iNavigationService.UpdateSellerNavigation(info);
                return Json(new Result() { success = true, msg = "编辑导航成功！" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "导航名称和跳转地址不能为空！" });
            }
        }

        [HttpPost]
        public JsonResult OpenOrClose(long Id, bool status)
        {
            try
            {
                _iNavigationService.OpenOrClose(Id, status);
                return Json(new Result() { success = true, msg = "开关导航成功！" });
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = "开关导航失败！" + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SwapDisplaySequence(long id, long id2)
        {
            _iNavigationService.SwapSellerDisplaySequence(CurrentSellerManager.ShopId, id, id2);
            return Json(new Result() { success = true, msg = "排序成功！" });
        }
    }
}