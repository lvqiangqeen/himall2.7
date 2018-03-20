using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    public class TopicController : BaseApiController
    {
        public object GetTopicList(int pageNo,int pageSize)
        {
            TopicQuery topicQuery = new TopicQuery();
            topicQuery.ShopId = 0;
            topicQuery.PlatformType = PlatformType.Mobile;
            topicQuery.PageNo = pageNo;
            topicQuery.PageSize = pageSize;
            var pagemodel = ServiceProvider.Instance<ITopicService>.Create.GetTopics(topicQuery);
            var topics =pagemodel.Models.ToList();
            var model = topics.Select(item => new
            {
                Id = item.Id,
                //TopImage = "http://" + Url.Request.RequestUri.Host + item.TopImage,
                TopImage = Core.HimallIO.GetRomoteImagePath(item.TopImage),
                Name = item.Name
            }
                );
            return Json(new { Success = "true", Topic = model, Total = pagemodel.Total });
        }
        public object GetTopicDetail(long id)
        {
            TopicModel model = new TopicModel();
            TopicInfo topic = ServiceProvider.Instance<ITopicService>.Create.GetTopicInfo(id);
            model.Id = topic.Id;
            model.Name = topic.Name;
            //model.TopImage = "http://" + Url.Request.RequestUri.Host + topic.TopImage;
            model.TopImage = Core.HimallIO.GetRomoteImagePath(topic.TopImage);
            model.TopicModule = new List<TopicModuleModel>();
            var topicModule=topic.TopicModuleInfo.ToList();
            foreach (var module in topicModule)
            {
                var topicModuleModel = new TopicModuleModel();

                var productIds = module.ModuleProductInfo
                .Where(item => item.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && item.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                .OrderBy(item => item.Id)
                    //.Skip(pageSize * (pageNo - 1))
                    //.Take(pageSize)
                .Select(item => item.ProductId);
                var products = ServiceProvider.Instance<IProductService>.Create.GetProductByIds(productIds);
                var model1 = products.ToArray().Select(item => new TopicModuleProductModel
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    //Image = "http://" + Url.Request.RequestUri.Host + item.GetImage(Model.ProductInfo.ImageSize.Size_350),
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    Price = item.MinSalePrice,
                    MarketPrice = item.MarketPrice
                });
                topicModuleModel.Id = module.Id;
                topicModuleModel.Name = module.Name;
                topicModuleModel.TopicModelProduct = model1.ToList();
                model.TopicModule.Add(topicModuleModel);
            }
            return Json(new { Success = "true", Topic=model });
        }
        
        public object LoadProducts(long topicId, long moduleId)
        {
            var topic = ServiceProvider.Instance<ITopicService>.Create.GetTopicInfo(topicId);
            var module = topic.TopicModuleInfo.FirstOrDefault(item => item.Id == moduleId);
            var productIds = module.ModuleProductInfo
                .Where(item => item.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && item.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                .OrderBy(item => item.Id)
                //.Skip(pageSize * (page - 1))
                //.Take(pageSize)//暂时不分页，可以考虑以后优化
                .Select(item => item.ProductId);
            var products = ServiceProvider.Instance<IProductService>.Create.GetProductByIds(productIds);
            var model = products.ToArray().Select(item => new TopicModuleProductModel
            {
                Name = item.ProductName,
                Id = item.Id,
                Image = item.GetImage(ImageSize.Size_350),
                Price = item.MinSalePrice,
                MarketPrice = item.MarketPrice
            });
            return Json(new { data= model.ToList() });
        }
    }

}
