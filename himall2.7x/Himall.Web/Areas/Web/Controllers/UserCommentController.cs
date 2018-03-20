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
    public class UserCommentController : BaseMemberController
    {
       private ICommentService _iCommentService; 
       private IMemberIntegralService _iMemberIntegralService;
       private IMemberIntegralConversionFactoryService _iMemberIntegralConversionFactoryService;
        public UserCommentController(ICommentService iCommentService, IMemberIntegralService iMemberIntegralService,IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService)
        {
            _iCommentService = iCommentService;
            _iMemberIntegralService = iMemberIntegralService;
            _iMemberIntegralConversionFactoryService = iMemberIntegralConversionFactoryService;
        }
        public ActionResult Index(int pageSize = 10, int pageNo = 1)
        {
            OrderCommentQuery query = new OrderCommentQuery();
            query.UserId = base.CurrentUser.Id;
            query.PageSize = pageSize;
            query.PageNo = pageNo;

            var model=_iCommentService.GetOrderComment(query);
           //// query.Sort = "PComment";
           // var model = _iCommentService.GetProductEvaluation(query);
           // //var newModel = (from p in 
           // //                group p by new { p.BuyTime, p.OrderId, p.EvaluationStatus } into g
           // //                select new ProductEvaluation
           // //         {
           // //             BuyTime = g.Key.BuyTime,
           // //             OrderId = g.Key.OrderId,
           // //             EvaluationStatus = g.Key.EvaluationStatus
           // //         }
           // //           ).ToList();
           // #region 分页控制
           // //PagingInfo info = new PagingInfo
           // //{
           // //    CurrentPage = pageNo,
           // //    ItemsPerPage = pageSize,
           // //    TotalItems = model.Total
           // //};
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            
          //  #endregion
            //return View(model.Models);
            return View(model.Models);
        }

        public JsonResult AddComment(long subOrderId, int star, string content)
        {
            ProductCommentInfo model = new ProductCommentInfo();
            model.ReviewDate = DateTime.Now;
            model.ReviewContent = content;
            model.UserId = CurrentUser.Id;
            model.UserName = CurrentUser.UserName;
            model.Email = CurrentUser.Email;
            model.SubOrderId = subOrderId;
            model.ReviewMark = star;
            _iCommentService.AddComment(model);
            //TODO发表评论获得积分
            MemberIntegralRecord info = new MemberIntegralRecord();
            info.UserName = CurrentUser.UserName;
            info.MemberId = CurrentUser.Id;
            info.RecordDate = DateTime.Now;
            info.TypeId = MemberIntegral.IntegralType.Comment;
            MemberIntegralRecordAction action = new MemberIntegralRecordAction();
            action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Comment;
            action.VirtualItemId = model.ProductId;
            info.Himall_MemberIntegralRecordAction.Add(action);
            var memberIntegral = _iMemberIntegralConversionFactoryService.Create(MemberIntegral.IntegralType.Comment);
            _iMemberIntegralService.AddMemberIntegral(info, memberIntegral);
            return Json(new Result() { success = true, msg = "发表成功" });
        }
    }
}