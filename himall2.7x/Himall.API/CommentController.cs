using Himall.IServices;
using Himall.Model;
using Himall.Model.DTO;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Himall.API.Model;
using Himall.API.Model.ParamsModel;

namespace Himall.API
{
    public class CommentController : BaseApiController
    {
        public object GetComment(long orderId)
        {
            CheckUserLogin();
            var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            if (order != null && order.OrderCommentInfo.Count == 0)
            {
                var model = ServiceProvider.Instance<ICommentService>.Create.GetProductEvaluationByOrderId(orderId, CurrentUser.Id).Select(item => new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //Image = "http://" + Url.Request.RequestUri.Host + item.ThumbnailsUrl
                    //Image = Core.HimallIO.GetRomoteImagePath(item.ThumbnailsUrl)
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220) //商城App评论时获取商品图片
                });

                var orderEvaluation = ServiceProvider.Instance<ITradeCommentService>.Create.GetOrderCommentInfo(orderId, CurrentUser.Id);
                return new { Success = true, Product = model, orderItemIds = order.OrderItemInfo.Select(item => item.Id) };
            }
            else
                return new { Success = false, ErrorMsg = "该订单不存在或者已评论过" };
        }
        //订单评论
        public object PostAddComment(CommentAddCommentModel value)
        {
            CheckUserLogin();
            try
            {
                string Jsonstr = value.Jsonstr;
                bool result = false;
                var orderComment = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderCommentModel>(Jsonstr);
                if (orderComment != null)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        AddOrderComment(orderComment);//添加订单评价
                        AddProductsComment(orderComment.OrderId, orderComment.ProductComments);//添加商品评论
                        scope.Complete();
                    }
                    result = true;
                }
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        void AddOrderComment(OrderCommentModel comment)
        {
            ServiceProvider.Instance<ITradeCommentService>.Create.AddOrderComment(new OrderCommentInfo()
            {
                OrderId = comment.OrderId,
                DeliveryMark = comment.DeliveryMark,
                ServiceMark = comment.ServiceMark,
                PackMark = comment.PackMark,
                UserId = CurrentUser.Id,

            });
        }

        void AddProductsComment(long orderId, IEnumerable<ProductCommentModel> productComments)
        {
            var commentService = ServiceProvider.Instance<ICommentService>.Create;
            foreach (var productComment in productComments)
            {
                ProductCommentInfo model = new ProductCommentInfo();
                model.ReviewDate = DateTime.Now;
                model.ReviewContent = productComment.Content;
                model.UserId = CurrentUser.Id;
                model.UserName = CurrentUser.UserName;
                model.Email = CurrentUser.Email;
                model.SubOrderId = productComment.OrderItemId;
                model.ReviewMark = productComment.Mark;
                model.ProductId = productComment.ProductId;
                if (productComment.Images != null && productComment.Images.Length > 0)
                {
                    foreach (var img in productComment.Images)
                    {
                        var p = new ProductCommentsImagesInfo();
                        p.CommentType = 0;//0代表默认的表示评论的图片
                        p.CommentImage = MoveImages(img, CurrentUser.Id);
                        model.Himall_ProductCommentsImages.Add(p);
                    }
                }
                #region APP中 微信图片可以去除掉 
                //else if (productComment.WXmediaId != null && productComment.WXmediaId.Length > 0)
                //{
                //    foreach (var img in productComment.WXmediaId)
                //    {
                //        var p = new ProductCommentsImagesInfo();
                //        p.CommentType = 0;//0代表默认的表示评论的图片
                //        p.CommentImage = DownloadWxImage(img);
                //        if (!string.IsNullOrEmpty(p.CommentImage))
                //        {
                //            model.Himall_ProductCommentsImages.Add(p);
                //        }
                //    }
                //}
                #endregion

                commentService.AddComment(model);
            }
        }

        ///// <summary>
        ///// 下载微信图片
        ///// </summary>
        ///// <param name="link">下载地址</param>
        ///// <param name="filePath">保存相对路径</param>
        ///// <param name="fileName">保存地址</param>
        ///// <returns></returns>
        //public string DownloadWxImage(string mediaId)
        //{
        //    var token = AccessTokenContainer.TryGetToken(CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);
        //    var address = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
        //    Random ra = new Random();
        //    var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(10) + ".jpg";
        //    var ImageDir = Core.Helper.IOHelper.GetMapPath("/Storage/Plat/Comment");
        //    // string dirPath = Globals.PhysicalPath(Path.Combine(Globals.ApplicationPath, filePath));
        //    if (!System.IO.Directory.Exists(ImageDir))
        //        System.IO.Directory.CreateDirectory(ImageDir);
        //    WebClient wc = new WebClient();
        //    try
        //    {
        //        string fullPath = Path.Combine(ImageDir, fileName);
        //        wc.DownloadFile(address, fullPath);
        //        return "/Storage/Plat/Comment/" + fileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("下载图片发生异常" + ex.Message);
        //        return string.Empty;
        //    }
        //}

        private string MoveImages(string image, long userId)
        {
            string OriUrl = Core.Helper.IOHelper.GetMapPath(image);
            //  var ext = new System.IO.FileInfo(OriUrl).Extension;
            var oldname = new System.IO.FileInfo(OriUrl).Name;
            string ImageDir = string.Empty;

            //转移图片
            ImageDir = Core.Helper.IOHelper.GetMapPath("/Storage/Plat/Comment");
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + oldname;
            if (!System.IO.Directory.Exists(ImageDir))
                System.IO.Directory.CreateDirectory(ImageDir);//创建图片目录

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                Core.Helper.IOHelper.CopyFile(OriUrl, ImageDir, false, fileName);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                return image;
            }
        }

        public object GetAppendComment(long orderid)
        {
            CheckUserLogin();
            var model = ServiceProvider.Instance<ICommentService>.Create.GetProductEvaluationByOrderIdNew(orderid, CurrentUser.Id);

            if (model.FirstOrDefault().AppendTime.HasValue)
                return new { Success = false, ErrorMsg = "追加评论时，获取数据异常" };
            else
            {
                var listResult = model.Select(item => new
                {
                    Id = item.Id,
                    CommentId = item.CommentId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //ThumbnailsUrl = item.ThumbnailsUrl,
                    ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220), //商城App追加评论时获取商品图片
                    BuyTime = item.BuyTime,
                    EvaluationStatus = item.EvaluationStatus,
                    EvaluationContent = item.EvaluationContent,
                    AppendContent = item.AppendContent,
                    AppendTime = item.AppendTime,
                    EvaluationTime = item.EvaluationTime,
                    ReplyTime = item.ReplyTime,
                    ReplyContent = item.ReplyContent,
                    ReplyAppendTime = item.ReplyAppendTime,
                    ReplyAppendContent = item.ReplyAppendContent,
                    EvaluationRank = item.EvaluationRank,
                    OrderId = item.OrderId,
                    CommentImages = item.CommentImages.Select(r => new
                    {
                        CommentImage = r.CommentImage,
                        CommentId = r.CommentId,
                        CommentType = r.CommentType
                    }).ToList(),
                    Color = item.Color,
                    Size = item.Size,
                    Version = item.Version
                }).ToList();
                return new { Success = true, List = listResult };
            }
        }

        public object PostAppendComment(CommentAppendCommentModel value)
        {
            CheckUserLogin();
            string productCommentsJSON = value.productCommentsJSON;
            var commentService = ServiceProvider.Instance<ICommentService>.Create;
            var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);

            foreach (var m in productComments)
            {
                m.UserId = CurrentUser.Id;
                ;
            }
            commentService.AppendComment(productComments);
            return Json(new { success = true });
        }
    }
}
