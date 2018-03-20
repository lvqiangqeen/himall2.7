using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class TopicController : BaseMobileTemplatesController
    {
       private  IProductService _iProductService;
       private ITopicService _iTopicService;
       private ILimitTimeBuyService _iLimitTimeBuyService;
       public TopicController(ITopicService iTopicService, IProductService iProductService, ILimitTimeBuyService iLimitTimeBuyService)
       {
           _iProductService = iProductService;
           _iTopicService = iTopicService;
           _iLimitTimeBuyService = iLimitTimeBuyService;
       }
        // GET: Mobile/Topic

        public ActionResult List(int pageNo = 1, int pageSize = 10)
        {
            TopicQuery topicQuery = new TopicQuery();
            topicQuery.ShopId = 0;
            topicQuery.PlatformType = PlatformType.Mobile;
            topicQuery.PageNo = pageNo;
            topicQuery.PageSize = pageSize;
            var topics = _iTopicService.GetTopics(topicQuery).Models;
            return View(topics);
        }

        [HttpPost]
        public JsonResult TopicList(int pageNo = 1, int pageSize = 10)
        {
            TopicQuery topicQuery = new TopicQuery();
            topicQuery.ShopId = 0;
            topicQuery.PlatformType = PlatformType.Mobile;
            topicQuery.PageNo = pageNo;
            topicQuery.PageSize = pageSize;
            var topics = _iTopicService.GetTopics(topicQuery).Models.ToList();
            var model = topics.Select(item => new
            {
                Id = item.Id,
                TopImage = item.TopImage,
                Name = item.Name
            }
                );
            return Json(model);
        }
        public ActionResult Detail(long id)
        {
            TopicInfo topic = _iTopicService.GetTopicInfo(id);
            string tmppath = VTemplateHelper.GetTemplatePath(id.ToString(), VTemplateClientTypes.WapSpecial);
            tmppath = "~" + tmppath;
            string viewpath = tmppath+ "Skin-HomePage.cshtml";
            ViewBag.Title= "专题-" + topic.Name;
            return View(viewpath);
        }

        [HttpPost]
        public JsonResult LoadProducts(long topicId,long moduleId,int page,int pageSize)
        {
            var topic = _iTopicService.GetTopicInfo(topicId);
            var module = topic.TopicModuleInfo.FirstOrDefault(item => item.Id == moduleId);
            var productIds = module.ModuleProductInfo
                .Where(item => item.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale 
                    && item.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                .OrderBy(item => item.Id)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .Select(item => item.ProductId);
            var products = _iProductService.GetProductByIds(productIds);
            var model = products.ToArray().Select(item =>
                {
                    var flashSaleModel = _iLimitTimeBuyService.GetFlaseSaleByProductId(item.Id);
                    return new
                        {
                            name = item.ProductName,
                            id = item.Id,
                            image = item.GetImage(ImageSize.Size_350),
                            price = flashSaleModel != null ? flashSaleModel.MinPrice : item.MinSalePrice,
                            marketPrice = item.MarketPrice
                        };
                });
            return Json(model);
        }

    }
}