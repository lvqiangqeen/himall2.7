using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class MobileTopicController : BaseSellerController
    {
        private ITopicService _iTopicService;
        public MobileTopicController(ITopicService iTopicService)
        {
            _iTopicService = iTopicService;
        }
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        public JsonResult List(int page, int rows, string titleKeyword, string tagsKeyword)
        {
            TopicQuery query = new TopicQuery()
            {
                IsAsc = false,
                PageSize = rows,
                PageNo = page,
                Name = titleKeyword,
                Tags = tagsKeyword,
                PlatformType = PlatformType.Mobile,
                ShopId = CurrentSellerManager.ShopId,
                Sort = "id"
            };

            var topics = _iTopicService.GetTopics(query);
            var list = new
            {
                rows = topics.Models.ToArray().Select(item => new
                {
                    id = item.Id,
                    url = "http://" + HttpContext.Request.Url.Authority + "/m-wap/topic/detail/" + item.Id,
                    name = item.Name,
                    imgUrl = item.FrontCoverImage,
                    tags = string.IsNullOrWhiteSpace(item.Tags) ? "" : item.Tags.Replace(",", " ")
                }),
                total = topics.Total
            };
            return Json(list);
        }

        public ActionResult Save(long id = 0)
        {
            TopicInfo topicInfo;

            if (id > 0)
            {
                topicInfo = _iTopicService.GetTopicInfo(id);
                if (topicInfo.ShopId != CurrentSellerManager.ShopId)
                {
                    throw new HimallException("不存在该专题或者删除！" + id);
                }
            }
            else
                topicInfo = new TopicInfo();

            Models.TopicModel topicModel = new Models.TopicModel()
            {
                Id = topicInfo.Id,
                Name = topicInfo.Name,
                TopImage = topicInfo.TopImage,
                TopicModuleInfo = topicInfo.TopicModuleInfo,
                Tags = topicInfo.Tags,
            };

            return View(topicModel);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Add(string topicJson)
        {
            var s = new Newtonsoft.Json.JsonSerializerSettings();
            s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            Model.TopicInfo topic = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.TopicInfo>(topicJson, s);
            if(string.IsNullOrWhiteSpace(topic.Name))
            {
                return Json(new { success = false,msg="专题名不可为空" });
            }
            topic.Name = topic.Name.Trim();
            if (string.IsNullOrWhiteSpace(topic.Tags))
            {
                return Json(new { success = false, msg = "标签不可为空" });
            }
            foreach(var item in topic.TopicModuleInfo)
            {
                if(string.IsNullOrWhiteSpace(item.Name))
                {
                    return Json(new { success = false, msg = "错误的模块名" });
                }
            }
            topic.Tags = topic.Tags.Trim();
            var oriTopic = _iTopicService.GetTopicInfo(topic.Id);
            topic.PlatForm = PlatformType.Mobile;
            topic.ShopId = CurrentSellerManager.ShopId;
            topic.BackgroundImage = oriTopic == null ? string.Empty : oriTopic.BackgroundImage;
            topic.FrontCoverImage = oriTopic == null ? string.Empty : oriTopic.FrontCoverImage;


            if (topic.Id > 0)
                _iTopicService.UpdateTopic(topic);
            else
                _iTopicService.AddTopic(topic);

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            Result result = new Result();
            _iTopicService.DeleteTopic(id);
            result.success = true;
            return Json(result);
        }
    }
}