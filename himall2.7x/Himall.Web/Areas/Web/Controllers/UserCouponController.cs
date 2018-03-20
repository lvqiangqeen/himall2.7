using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.Web.Models;


namespace Himall.Web.Areas.Web.Controllers
{
    public class UserCouponController : BaseMemberController
    {
        private IShopBonusService _iShopBonusService;
        private ICouponService _iCouponService;
        public UserCouponController(ICouponService iCouponService, IShopBonusService iShopBonusService)
        {
            _iCouponService = iCouponService;
            _iShopBonusService = iShopBonusService;
        }
        public ActionResult Index(int?status,int pageSize = 10, int pageNo = 1)
        {
            if(!status.HasValue)
            {
                status = 0;
            }

            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId= CurrentUser.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = status;
            var model = _iCouponService.GetCouponRecordList(query);

            var shopBonus = _iShopBonusService.GetDetailByQuery( query );


            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total + shopBonus.Total
            };
            ViewBag.pageInfo = info;
            ViewBag.Bonus = shopBonus.Models.ToList();
            ViewBag.State = query.Status;
            #endregion
            return View(model.Models.ToList());
        }
    }
}