using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Core;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ProductConcernController:BaseMemberController
    {
        private IProductService _iProductService;
        public ProductConcernController(IProductService iProductService)
        {
            _iProductService = iProductService;
        }
        // GET: Web/ProductConcern
        public ActionResult Index(int pageSize=10,int pageNo=1)
        {
            var model = _iProductService.GetUserConcernProducts(CurrentUser.Id,pageNo,pageSize);
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            return View(model.Models.ToList());
        }
        public JsonResult CancelConcernProducts(string ids)
        {
            var strArr = ids.Split(',');
            List<long> listid = new List<long>();
            foreach (var arr in strArr)
            {
                listid.Add(Convert.ToInt64(arr));
            }
            _iProductService.CancelConcernProducts(listid, CurrentUser.Id);
            return Json(new Result() { success = true, msg = "取消成功！" });
        }
    }
}


