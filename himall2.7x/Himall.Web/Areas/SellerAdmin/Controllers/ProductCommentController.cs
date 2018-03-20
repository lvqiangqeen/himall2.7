using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Models;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using Himall.Application;
using Himall.CommonModel;
using Himall.Model;
using Himall.Core;


namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductCommentController : BaseSellerController
    {
        private IOrderService _iOrderService;
        private ICommentService _iCommentService;
        private ITypeService _iTypeService;
        public ProductCommentController(IOrderService iOrderService, ICommentService iCommentService, ITypeService iTypeService)
        {
            _iOrderService = iOrderService;
            _iCommentService = iCommentService;
            _iTypeService = iTypeService;
        }
        public ActionResult Management()
        {
            return View();
        }

        public JsonResult List(int page, int rows, string productName, bool? isReply = null, int Rank = -1, bool hasAppend = false)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                productName = productName.Trim();
            }
            var orderItemService = _iOrderService;
            var iTypeService = _iTypeService;
            var query = new CommentQuery() { PageNo = page, PageSize = rows, HasAppend = hasAppend, ProductName = productName, Rank = Rank, ShopID = base.CurrentSellerManager.ShopId, IsReply = isReply };
            var result = _iCommentService.GetComments(query);
            IEnumerable<ProductCommentModel> comments = result.Models.Select(item => new ProductCommentModel()
            {
                CommentContent = item.ReviewContent,
                CommentDate = item.ReviewDate,
                ReplyContent = item.ReplyContent,
                AppendContent = item.AppendContent,
                AppendDate = item.AppendDate,
                ReplyAppendDate = item.ReplyAppendDate,
                CommentMark = item.ReviewMark,
                ReplyDate = item.ReplyDate,
                Id = item.Id,
                ProductName = item.ProductInfo.ProductName,
                ProductId = item.ProductId,
                ImagePath = item.Himall_OrderItems.ThumbnailsUrl,
                UserName = item.UserName,
                OderItemId = item.SubOrderId,
                Color = "",
                Version = "",
                Size = "",
                UserId = item.UserId
            }).ToList();
            //TODO LRL 2015/08/06 从评价信息添加商品的规格信息
            foreach (var item in comments)
            {
                item.ImagePath = Core.HimallIO.GetProductSizeImage(item.ImagePath, 1, (int)ImageSize.Size_100);
                if (item.OderItemId.HasValue)
                {
                    var obj = orderItemService.GetOrderItem(item.OderItemId.Value);
                    if (obj != null)
                    {
                        item.Color = obj.Color;
                        item.Size = obj.Size;
                        item.Version = obj.Version;
                        item.OrderId = obj.OrderId;

                        var member = MemberApplication.GetMember(item.UserId);
                        if (member != null)
                        {
                            item.UserName = member.UserName;
                            item.UserPhone = member.CellPhone;
                        }
                    }
                }
                ProductTypeInfo typeInfo = iTypeService.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            DataGridModel<ProductCommentModel> model = new DataGridModel<ProductCommentModel>() { rows = comments, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        public JsonResult Detail(long id)
        {
            var model = _iCommentService.GetComment(id);
            return Json(new { ConsulationContent = model.ReviewContent, ReplyContent = model.ReplyContent });
        }

        [HttpPost]
        public JsonResult ReplyComment(long id, string replycontent, string appendContent)
        {
            var shopid = base.CurrentSellerManager.ShopId;
            _iCommentService.ReplyComment(id, shopid, replycontent, appendContent);
            return Json(new Result() { success = true, msg = "回复成功！" });
        }
        public ActionResult GetComment(long Id)
        {
            var model = _iCommentService.GetComment(Id);
            return View(model);
        }
    }
}
