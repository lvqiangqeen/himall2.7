using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Himall.Core;
using Himall.Model.DTO;
using System.Net;
using System.IO;
using Senparc.Weixin.MP.CommonAPIs;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class CommentService : ServiceBase, ICommentService
    {
        public void AddComment(Model.ProductCommentInfo model)
        {
            var m = Context.OrderItemInfo.Where(a => a.Id == model.SubOrderId && a.OrderInfo.UserId == model.UserId).FirstOrDefault();
            if (m == null)
            {
                throw new Himall.Core.HimallException("不能对此商品进行评价！");
            }
            model.ShopId = m.ShopId;
            model.ProductId = m.ProductId;
            model.ShopName = Context.ShopInfo.Where(a => a.Id == m.ShopId).Select(a => a.ShopName).FirstOrDefault();
            model.IsHidden = false;
            Context.ProductCommentInfo.Add(model);

            //更新搜索商品评论数
            var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == m.ProductId);
            if(searchProduct != null)
                searchProduct.Comments += 1;
            Context.SaveChanges();
        }

		public void AddComment(IEnumerable<ProductCommentInfo> models)
		{
			foreach (var model in models)
			{
				var m = Context.OrderItemInfo.Where(a => a.Id == model.SubOrderId && a.OrderInfo.UserId == model.UserId).FirstOrDefault();
				if (m == null)
				{
					throw new Himall.Core.HimallException("不能对此商品进行评价！");
				}
				model.ShopId = m.ShopId;
				model.ProductId = m.ProductId;
				model.ShopName = Context.ShopInfo.Where(a => a.Id == m.ShopId).Select(a => a.ShopName).FirstOrDefault();
				model.IsHidden = false;

                //更新搜索商品评论数
                var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == m.ProductId);
                if (searchProduct != null)
                    searchProduct.Comments += 1;

                Context.ProductCommentInfo.Add(model);
			}

			Context.SaveChanges();
		}

        public void AutoComment(long? userid = null)
        {
          //  var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            //windows服务调用此方法不报错
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();

            //自动订单评价天数
            int intIntervalDay = siteSetting == null ? 7 : (siteSetting.OrderCommentTimeout == 0 ? 7 : siteSetting.OrderCommentTimeout);
            DateTime waitCommentDate = DateTime.Now.AddDays(-intIntervalDay);
            List<OrderInfo> info = new List<OrderInfo>();
            if (userid.HasValue)
            {
                info = Context.OrderInfo.Where(a => a.UserId == userid.Value && a.FinishDate < waitCommentDate && a.OrderStatus == OrderInfo.OrderOperateStatus.Finish && a.OrderCommentInfo.Count() == 0).ToList();
            }
            else
            {
                info = Context.OrderInfo.Where(a => a.FinishDate < waitCommentDate && a.OrderStatus == OrderInfo.OrderOperateStatus.Finish && a.OrderCommentInfo.Count() == 0).ToList();
            }
            try
            {
                AutoOrderComment(info);
                AutoProductComment(info);
            }
            catch (Exception ex)
            {
                Log.Error("AutoCommnetOrder:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }

        private void AutoProductComment(List<OrderInfo> orders)
        {
            foreach (var order in orders)
            {
                foreach (var item in order.OrderItemInfo)
                {
                    ProductCommentInfo model = new ProductCommentInfo();
                    model.ReviewDate = DateTime.Now;
                    model.ReviewContent = "好评!";
                    model.UserId = order.UserId;
                    model.UserName = order.UserName;
                    model.Email = "";
                    model.SubOrderId = item.Id;
                    model.ReviewMark = 5;
                    model.ShopId = order.ShopId;
                    model.ProductId = item.ProductId;
                    model.ShopName = order.ShopName;
                    model.IsHidden = false;
                    Context.ProductCommentInfo.Add(model);

                    //更新商品评论数
                    var searchProduct = Context.SearchProductsInfo.FirstOrDefault(r => r.ProductId == item.ProductId);
                    if (searchProduct != null)
                        searchProduct.Comments += 1;
                }
            }
            Context.SaveChanges();
        }

        private void AutoOrderComment(List<OrderInfo> orders)
        {
            foreach (var order in orders)
            {
                OrderCommentInfo info = new OrderCommentInfo();
                info.UserId = order.UserId;
                info.PackMark = 5;
                info.DeliveryMark = 5;
                info.ServiceMark = 5;
                info.OrderId = order.Id;
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.UserName = order.UserName;
                info.CommentDate = DateTime.Now;
                Context.OrderCommentInfo.Add(info);
                Context.SaveChanges();
                //MemberIntegralRecord record = new MemberIntegralRecord();
                //record.UserName = info.UserName;
                //record.ReMark = "订单号:" + info.OrderId;
                //record.MemberId = info.UserId;
                //record.RecordDate = DateTime.Now;
                //record.TypeId = MemberIntegral.IntegralType.Comment;
                //MemberIntegralRecordAction action = new MemberIntegralRecordAction();
                //action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Comment;
                //action.VirtualItemId = info.OrderId;
                //record.Himall_MemberIntegralRecordAction.Add(action);
                //var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Comment);
                //ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
            }
        }

        public void ReplyComment(long id, string replyContent, long shopId)
        {
            var model = Context.ProductCommentInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (shopId == 0 || model == null)
            {
                throw new Himall.Core.HimallException("不存在该商品评论");
            }
            model.ReplyContent = replyContent;
            model.ReplyDate = DateTime.Now;
            Context.SaveChanges();
        }

        public void HiddenComment(long id)
        {
            var model = Context.ProductCommentInfo.FindBy(item => item.Id == id).FirstOrDefault();
            model.IsHidden = true;
            // context.ProductCommentInfo.Remove(id);
            Context.SaveChanges();
        }

        public void SetCommentEmpty(long id)
        {
            var model = Context.ProductCommentInfo.First(p => p.Id == id);
            model.ReviewContent = "";
            model.ReplyContent = "";
            Context.SaveChanges();
        }

        public Model.ObsoletePageModel<Model.ProductCommentInfo> GetComments(IServices.QueryModel.CommentQuery query)
        {
            int total = 0;
            IQueryable<ProductCommentInfo> consultation = Context.ProductCommentInfo.Include(a => a.ProductInfo).AsQueryable();

            var orderby = consultation.GetOrderBy(item=>item.OrderByDescending(a=>a.Id));

            #region 条件组合
            if (query.IsReply.HasValue)
            {
                if (!query.IsReply.Value)
                {
                    consultation = consultation.Where(item => !item.ReplyDate.HasValue || (!item.ReplyAppendDate.HasValue && item.AppendDate.HasValue));
                }
                else
                {
                    consultation = consultation.Where(item => item.ReplyDate.HasValue && item.ReplyAppendDate.HasValue);
                }
            }
            if (query.ShopID > 0)
            {
                consultation = consultation.Where(item => query.ShopID == item.ShopId);
            }
            if (query.ProductID > 0)
            {
                consultation = consultation.Where(item => query.ProductID == item.ProductId);
            }
            if (query.Rank != -1)
            {
                switch (query.Rank)
                {
                    case 0: //好评
                        consultation = consultation.Where(item => item.ReviewMark == 4 || item.ReviewMark == 5);
                        break;
                    case 1: //中评
                        consultation = consultation.Where(item => item.ReviewMark == 3);
                        break;
                    case 2: //差评
                        consultation = consultation.Where(item => item.ReviewMark <= 2);
                        break;
                }
            }
            if (query.HasAppend)
            {
                consultation = consultation.Where(item => item.AppendDate.HasValue);
                orderby = consultation.GetOrderBy(a=>a.OrderByDescending(b=>b.AppendDate));
            }
            if (query.UserID > 0)
            {
                consultation = consultation.Where(item => query.UserID == item.UserId);
            }
            if (!string.IsNullOrWhiteSpace(query.KeyWords))
            {
                consultation = consultation.Where(item => item.ReviewContent.Contains(query.KeyWords));
            }
            if (!string.IsNullOrWhiteSpace(query.ProductName))
            {
                consultation = consultation.Where(item => item.ProductInfo.ProductName.Contains(query.ProductName));
            }
            #endregion

            consultation = consultation.GetPage(out total, query.PageNo, query.PageSize,orderby);
            ObsoletePageModel<ProductCommentInfo> pageModel = new ObsoletePageModel<ProductCommentInfo>() { Models = consultation, Total = total };
            return pageModel;
        }

        public Model.ProductCommentInfo GetComment(long id)
        {
            return Context.ProductCommentInfo.FindById(id);
        }

        public IEnumerable<Model.ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids)
        {
            return Context.ProductCommentInfo.Where(e => ids.Contains(e.Id)).ToList();
        }
        public ObsoletePageModel<UserOrderCommentModel> GetOrderComment(OrderCommentQuery query)
        {
            var model = Context.OrderCommentInfo.Where(a => a.UserId == query.UserId);
            int total = 0;
            model = model.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(item => item.Id));
            var OrderCommentModel = model.Select(a => new UserOrderCommentModel { CommentTime = a.CommentDate, OrderId = a.OrderId }).ToList();
            ObsoletePageModel<UserOrderCommentModel> pageModel = new ObsoletePageModel<UserOrderCommentModel>()
            {
                Models = OrderCommentModel.AsQueryable(),
                Total = total
            };
            return pageModel;
        }


        /// <summary>
        /// 分页获取用户的评价
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ObsoletePageModel<ProductEvaluation> GetProductEvaluation(CommentQuery query)
        {
            int total = 0;
            IQueryable<OrderItemInfo> order = Context.OrderItemInfo.FindBy(a => a.OrderInfo.UserId == query.UserID && a.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish);
            var orderBy = order.GetOrderBy(d => d.OrderByDescending(o => o.Id));
            if ((!string.IsNullOrWhiteSpace(query.Sort)) && query.Sort.Equals("PComment"))
            {
                orderBy = order.GetOrderBy(d => d.OrderBy(a => Context.OrderCommentInfo.Any(b => b.OrderId == a.OrderId && b.UserId == query.UserID)));
            }
            order = order.GetPage(out total, query.PageNo, query.PageSize, orderBy);

            var model = order.Select(a => new ProductEvaluation
            {
                ProductId = a.ProductId,
                ThumbnailsUrl = a.ThumbnailsUrl,
                ProductName = a.ProductName,
                BuyTime = Context.OrderCommentInfo.Any(c => c.OrderId == a.OrderId) ? Context.OrderCommentInfo.Where(c => c.OrderId == a.OrderId).FirstOrDefault().CommentDate : DateTime.MinValue,
                EvaluationStatus = Context.OrderCommentInfo.Any(b => b.OrderId == a.OrderId && b.UserId == query.UserID),
                Id = a.Id,
                OrderId = a.OrderId
            });
            ObsoletePageModel<ProductEvaluation> pageModel = new ObsoletePageModel<ProductEvaluation>()
            {
                Models = model,
                Total = total
            };
            return pageModel;
        }

        public ObsoletePageModel<ProductEvaluation> GetProductEvaluationTwo(CommentQuery query)
        {
            int total = 0;
            IQueryable<OrderItemInfo> order = Context.OrderItemInfo.FindBy(a => a.OrderInfo.UserId == query.UserID && a.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish);
            var orderBy = order.GetOrderBy(d => d.OrderByDescending(o => o.Id));
            if ((!string.IsNullOrWhiteSpace(query.Sort)) && query.Sort.Equals("PComment"))
            {
                orderBy = order.GetOrderBy(d => d.OrderBy(a => Context.OrderCommentInfo.Any(b => b.OrderId == a.OrderId && b.UserId == query.UserID)));
            }
            order = order.GetPage(out total, query.PageNo, query.PageSize, orderBy);

            var model = order.Select(a => new ProductEvaluation
            {
                ProductId = a.ProductId,
                ThumbnailsUrl = a.ThumbnailsUrl,
                ProductName = a.ProductName,
                BuyTime = Context.ProductCommentInfo.Where(c => c.ProductId == a.ProductId && c.SubOrderId == a.Id).FirstOrDefault() != null ? Context.ProductCommentInfo.Where(c => c.ProductId == a.ProductId && c.SubOrderId == a.Id).FirstOrDefault().ReviewDate : DateTime.MinValue,
                EvaluationStatus = Context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == query.UserID && b.SubOrderId == a.Id),
                Id = a.Id,
                OrderId = a.OrderId
            });
            ObsoletePageModel<ProductEvaluation> pageModel = new ObsoletePageModel<ProductEvaluation>()
            {
                Models = model,
                Total = total
            };
            return pageModel;
        }
        /// <summary>
        /// 根据订单ID获取订单商品的评价
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId)
        {
            var order = Context.OrderItemInfo.Where(a => a.OrderId == orderId && a.OrderInfo.UserId == userId && a.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish)
                .OrderBy(a => Context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id));
            var model = order.Select(a => new ProductEvaluation
            {
                ProductId = a.ProductId,
                ThumbnailsUrl = a.ThumbnailsUrl,
                ProductName = a.ProductName,
                BuyTime = a.OrderInfo.OrderDate,
                EvaluationStatus = Context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id),
                Id = a.Id,
                OrderId = a.OrderId,
                Color = a.Color,
                Size = a.Size,
                Version = a.Version,
                SkuId=a.SkuId,
                Price=a.SalePrice
            }).ToList();
            foreach (var pe in model)
            {
                //ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(pe.ProductId);
                ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == pe.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                pe.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                pe.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                pe.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            return model;
        }

        public IQueryable<OrderItemInfo> GetUnEvaluatProducts(long userId)
        {
            var order = Context.OrderItemInfo.Where(a => a.OrderInfo.UserId == userId && a.OrderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish && !Context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id)).OrderByDescending(a => a.Id);
            return order;
        }

        public IQueryable<ProductCommentInfo> GetCommentsByProductId(long productId)
        {
            return Context.ProductCommentInfo.FindBy(c => c.ProductId.Equals(productId)).Where(a => (!a.IsHidden.HasValue || a.IsHidden.Value == false));
        }
        public decimal GetProductMark(long id)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTMARK(id);
            if (Cache.Exists(cacheKey))
                return decimal.Parse(Cache.Get(cacheKey).ToString());
            object result = null;
            string sql = "SELECT AVG(ReviewMark) from himall_productcomments where ProductId = @Id";
            using (var conn = new MySqlConnection(Connection.ConnectionString))
            {
                result = conn.ExecuteScalar(sql, new { Id = id });
            }
            if (result == null) {
                Cache.Insert(cacheKey, 5, 600);
                return 5;
            }
            Cache.Insert(cacheKey, result, 600);
            return decimal.Parse(result.ToString());
        }

        /// <summary>
        /// 有否有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        public bool HasAppendComment(long subOrderId)
        {
            return Context.ProductCommentInfo.Any(a => a.SubOrderId == subOrderId && a.AppendDate.HasValue);
        }


        public List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId)
        {
            var model = (
                from p in Context.ProductCommentInfo
                where p.Himall_OrderItems.OrderId == orderId
                && p.UserId == userId
                select new ProductEvaluation
                {
                    ProductId = p.ProductId,
                    CommentId = p.Id,
                    ThumbnailsUrl = p.Himall_OrderItems.ThumbnailsUrl,
                    ProductName = p.ProductInfo.ProductName,
                    BuyTime = p.Himall_OrderItems.OrderInfo.OrderDate,
                    EvaluationStatus = true,
                    Id = p.SubOrderId.Value,
                    OrderId = p.Himall_OrderItems.OrderId,
                    EvaluationRank = p.ReviewMark - 1,
                    EvaluationContent = p.ReviewContent,
                    EvaluationTime = p.ReviewDate,
                    Color = p.Himall_OrderItems.Color,
                    Size = p.Himall_OrderItems.Size,
                    Version = p.Himall_OrderItems.Version,
                    ReplyContent = p.ReplyContent,
                    ReplyTime = p.ReplyDate.Value,
                    ReplyAppendContent = p.ReplyAppendContent,
                    ReplyAppendTime = p.ReplyAppendDate,
                    AppendContent = p.AppendContent,
                    AppendTime = p.AppendDate,
                    CommentImages = p.Himall_ProductCommentsImages.ToList()
                }
                ).ToList();
            foreach(var pe in model)
            {
                //ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(pe.ProductId);
                ProductTypeInfo typeInfo = (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == pe.ProductId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
                pe.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                pe.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                pe.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            //var model = order.Select(a => new ProductEvaluation
            //{
            //    ProductId = a.ProductId,
            //    ThumbnailsUrl = a.ThumbnailsUrl,
            //    ProductName = a.ProductName,
            //    BuyTime = a.OrderInfo.OrderDate,
            //    EvaluationStatus = context.ProductCommentInfo.Any(b => b.ProductId == a.ProductId && b.UserId == userId && b.SubOrderId == a.Id),
            //    Id = a.Id,
            //    OrderId = a.OrderId
            //});
            return model.ToList();
        }


        public void ReplyComment(long id, long shopId, string replyContent = "", string appendContent = "")
        {
            var model = Context.ProductCommentInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (shopId == 0 || model == null)
            {
                throw new Himall.Core.HimallException("不存在该商品评论");
            }
            if (!string.IsNullOrEmpty(replyContent))
            {
                model.ReplyContent = replyContent;
                model.ReplyDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(appendContent))
            {
                model.ReplyAppendContent = appendContent;
                model.ReplyAppendDate = DateTime.Now;
            }
            Context.SaveChanges();
        }


        private string MoveImages(string image, long userId)
        {
            if(string.IsNullOrWhiteSpace(image))
            {
                return string.Empty;
            }
            string ImageDir = string.Empty;
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + Path.GetFileName(image);

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                return relativeDir + fileName;
            }
        }

        private string DownloadWxImage(string mediaId)
        {
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var token = AccessTokenContainer.TryGetToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
            var address = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            Random ra = new Random();
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(10) + ".jpg";
            var ImageDir ="/Storage/Plat/Comment/";
            WebClient wc = new WebClient();
            try
            {
                string fullPath = Path.Combine(ImageDir, fileName);
               var data= wc.DownloadData(address);
               MemoryStream stream = new MemoryStream(data);
               Core.HimallIO.CreateFile(fullPath,stream, FileCreateType.Create);
               return fullPath;
            }
            catch (Exception ex)
            {
                Log.Error("下载图片发生异常" + ex.Message);
                return string.Empty;
            }
        }



        public void AppendComment(List<AppendCommentModel> list)
        {
            var userid = 0L;
            var userName = "";
            long orderId = 0;
            foreach (var model in list)
            {
                var m = Context.ProductCommentInfo.FindBy(item => item.Id == model.Id && item.UserId == model.UserId).FirstOrDefault();
                if (model.UserId == 0 || m == null)
                {
                    throw new Himall.Core.HimallException("该商品尚未评论，请先评论。");
                }
                if(m.AppendDate.HasValue)
                {
                    throw new Himall.Core.HimallException("您已追加评价过了，不需再重复操作。");
                }
                userid = m.UserId;
                userName = m.UserName;
                orderId = m.Himall_OrderItems.OrderId;
                m.AppendContent = model.AppendContent;
                m.AppendDate = DateTime.Now;
                if (model.Images != null && model.Images.Length > 0)
                {
                    foreach (var img in model.Images)
                    {
                        var p = new ProductCommentsImagesInfo();
                        p.CommentType = 1;//1代表表示追加评论的图片
                        p.CommentImage = MoveImages(img, model.UserId);
                        m.Himall_ProductCommentsImages.Add(p);
                    }
                }
                else if (model.WXmediaId != null && model.WXmediaId.Length > 0)
                {
                    foreach (var img in model.WXmediaId)
                    {
                        var p = new ProductCommentsImagesInfo();
                        p.CommentType = 1;//1表示追加的图片
                        p.CommentImage = DownloadWxImage(img);
                        if (!string.IsNullOrEmpty(p.CommentImage))
                        {
                            m.Himall_ProductCommentsImages.Add(p);
                        }
                    }
                }
            }
            Context.SaveChanges();
            try
            {
                //TODO发表追加评论获得积分
                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = userName;
                info.MemberId = userid;
                info.RecordDate = DateTime.Now;
                info.TypeId = MemberIntegral.IntegralType.Comment;
                info.ReMark = "追加评论,订单号:" + orderId;
                MemberIntegralRecordAction action = new MemberIntegralRecordAction();
                action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Comment;
                action.VirtualItemId = orderId;
                info.Himall_MemberIntegralRecordAction.Add(action);
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Comment);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }
    }
}
