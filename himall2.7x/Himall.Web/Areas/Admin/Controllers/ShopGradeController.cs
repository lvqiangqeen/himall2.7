using Himall.IServices;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopGradeController : BaseAdminController
    {
        private IShopService _iShopService;
        public ShopGradeController(IShopService iShopService)
        {
            _iShopService = iShopService;
        }
        // GET: Admin/ShopGrade
        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        public JsonResult List()
        {
            var shopG = _iShopService.GetShopGrades();
            IEnumerable<ShopGradeModel> shopGs = shopG.ToArray().Select(item => new ShopGradeModel()
            {
                Id = item.Id,
                ChargeStandard = item.ChargeStandard,
                ImageLimit = item.ImageLimit,
                ProductLimit = item.ProductLimit,
                Name = item.Name,

            });

            DataGridModel<ShopGradeModel> dataGrid = new DataGridModel<ShopGradeModel>() { rows = shopGs, total = shopG.Count() };
            return Json(dataGrid);
        }

        [UnAuthorize]
        [HttpPost]
        public ActionResult Edit(ShopGradeModel shopG)
        {
            if (ModelState.IsValid)
            {
                _iShopService.UpdateShopGrade(shopG);
                return RedirectToAction("Management");
            }
            return View(shopG);
        }

        [HttpGet]

        public ActionResult Edit(long id)
        {
            return View(new ShopGradeModel(_iShopService.GetShopGrade(id)));
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [UnAuthorize]
        public ActionResult Add(ShopGradeModel shopG)
        {
           if (ModelState.IsValid)
            {
                _iShopService.AddShopGrade(shopG);
                return RedirectToAction("Management");
            }
            return View(shopG);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteShopGrade(long id)
        {
            string msg = "";
            _iShopService.DeleteShopGrade(id,out msg);
            if (string.IsNullOrWhiteSpace(msg))
            {
                return Json(new { Successful = true });
            }
            else
            {
                return Json(new { Successful = false,msg=msg });
            }
        }
    }
}