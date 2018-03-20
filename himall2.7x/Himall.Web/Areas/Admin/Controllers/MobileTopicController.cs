using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;

using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class MobileTopicController : BaseAdminController
    {
        private ITopicService _iTopicService;

        public MobileTopicController(ITopicService iTopicService)
        {
            _iTopicService = iTopicService;
        }
        // GET: Admin/MobileTopic
        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
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
                ShopId = CurrentManager.ShopId,
                Sort = "id"
            };

            var topics = _iTopicService.GetTopics(query);
            var list = new
            {
                rows = topics.Models.ToArray().Select(item => new
                {
                    id = item.Id,
                    name = item.Name,
                    imgUrl = item.FrontCoverImage,
                    url = "http://" + HttpContext.Request.Url.Authority + "/m-wap/topic/detail/" + item.Id,
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
                topicInfo = _iTopicService.GetTopicInfo(id);
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

        [HttpPost]
        public JsonResult Add(string topicJson)
        {
            var s = new Newtonsoft.Json.JsonSerializerSettings();
            s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            Model.TopicInfo topic = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.TopicInfo>(topicJson, s);
            var oriTopic = _iTopicService.GetTopicInfo(topic.Id);
            topic.PlatForm = PlatformType.Mobile;
            topic.BackgroundImage = oriTopic == null ? string.Empty : oriTopic.BackgroundImage;
            topic.FrontCoverImage = oriTopic == null ? string.Empty : oriTopic.FrontCoverImage;

            if (topic.Id == 0)
            {
                _iTopicService.AddTopic(topic);
            }
            else
            {
                _iTopicService.UpdateTopic(topic);
            }

            return Json(new { success = true });
        }


        [UnAuthorize]
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