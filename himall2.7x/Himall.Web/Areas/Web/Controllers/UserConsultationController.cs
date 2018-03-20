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
    public class UserConsultationController : BaseMemberController
    {
        private IConsultationService _iConsultationService;
        public UserConsultationController(IConsultationService iConsultationService)
        {
            _iConsultationService = iConsultationService;
        }

        // GET: Web/Register
        public ActionResult Index(int pageNo = 1, int pageSize = 20)
        {


            ConsultationQuery query = new ConsultationQuery();
            query.UserID = CurrentUser.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = _iConsultationService.GetConsultations(query);
            IEnumerable<ProductConsultationModel> consultation = model.Models.ToList().Select(item => new ProductConsultationModel()
            {
                Id = item.Id,
                ConsultationContent = item.ConsultationContent,
                ConsultationDate = item.ConsultationDate,
                ProductName = item.ProductInfo.ProductName,
                ProductPic = item.ProductInfo.ImagePath,
                ProductId = item.ProductId,
                UserName = item.UserName,
                ReplyContent = item.ReplyContent,
                ReplyDate = item.ReplyDate,
            });
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            return View(consultation);
        }
    }
}