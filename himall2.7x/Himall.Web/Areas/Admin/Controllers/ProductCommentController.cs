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
using Himall.Model;
using Himall.Core;


namespace Himall.Web.Areas.Admin.Controllers
{
    public class ProductCommentController : BaseAdminController
    {
        private ICommentService _iCommentService;
        private IOrderService _iOrderService;
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

        [UnAuthorize]
        public JsonResult List(int page, int rows, string productName, int shopid = 0, bool? isReply = null, int Rank = -1, bool hasAppend = false)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                productName = productName.Trim();
            }
            var orderItemService = _iOrderService;
            var iTypeService = _iTypeService;
            var query = new CommentQuery() { PageNo = page, PageSize = rows, HasAppend=hasAppend, ProductName=productName, Rank=Rank, ShopID = shopid, IsReply = isReply };
            var result = _iCommentService.GetComments(query);
            IEnumerable<ProductCommentModel> comments = result.Models.Select(item => new ProductCommentModel()
            {
                CommentContent = item.ReviewContent,
                CommentDate = item.ReviewDate,
                ReplyContent = item.ReplyContent,
                CommentMark = item.ReviewMark,
                ReplyDate = item.ReplyDate,
                AppendContent = item.AppendContent,
                AppendDate = item.AppendDate,
                ReplyAppendDate=item.ReplyAppendDate,
                Id = item.Id,
                ProductName = item.ProductInfo.ProductName,
                ProductId = item.ProductId,
                ImagePath = item.Himall_OrderItems.ThumbnailsUrl,
                UserName = item.UserName,
                OderItemId = item.SubOrderId,
                Color = "",
                Version = "",
                Size = "",
                IsHidden=item.IsHidden.Value
            }).ToList();
            //TODO LRL 2015/08/06 从评价信息添加商品的规格信息
            foreach (var item in comments)
            {
                item.ImagePath = Core.HimallIO.GetProductSizeImage(item.ImagePath, 1, 100);
                if (item.OderItemId.HasValue)
                {
                    var obj = orderItemService.GetOrderItem(item.OderItemId.Value);
                    if (obj != null)
                    {
                        item.Color = obj.Color;
                        item.Size = obj.Size;
                        item.Version = obj.Version;
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
        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            //zjt modify
            //删除评论后卖家不能再次评论，为减少数据库字段增加，只将评论置空
            ////_iCommentService.SetCommentEmpty(id);
            ////Edit:DZY[150709]  改为删除商品评价
            _iCommentService.HiddenComment(id);
            return Json(new Result() { success = true, msg = "清除成功！" });
        }
        [UnAuthorize]
        [HttpPost]
        public JsonResult Detail(long id)
        {
            var model = _iCommentService.GetComment(id);
            return Json(new { ConsulationContent = model.ReviewContent, ReplyContent = model.ReplyContent });
        }

        public ActionResult GetComment(long Id)
        {
            var model = _iCommentService.GetComment(Id);
            foreach (var item in model.Himall_ProductCommentsImages)
            {
                item.CommentImage = Himall.Core.HimallIO.GetImagePath(item.CommentImage);
            }
            return View(model);
        }
    }
}
