using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Text;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{

    public class TopicController : BaseAdminController
    {
        ITopicService _iTopicService;
        public TopicController(ITopicService iTopicService)
        {
            _iTopicService = iTopicService;
        }
        // GET: Admin/Topic
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int page, int rows)
        {
            var topics = _iTopicService.GetTopics(page, rows);
            var list = new
            {
                rows = topics.Models.Select(item => new
                {
                    id = item.Id,
                    name = item.Name,
                    url = "http://" + HttpContext.Request.Url.Authority + "/topic/detail/" + item.Id,
                    IsRecommend = item.IsRecommend
                }),
                total = topics.Total
            };
            return Json(list);
        }

        public ActionResult Add(long? id)
        {
            if (id.HasValue)
            {
                TopicInfo topicInfo = _iTopicService.GetTopicInfo(id.Value);
                Models.TopicModel topicModel = new Models.TopicModel()
                {
                    BackgroundImage = topicInfo.BackgroundImage,
                    Id = topicInfo.Id,
                    Name = topicInfo.Name,
                    TopImage = topicInfo.TopImage,
                    TopicModuleInfo = topicInfo.TopicModuleInfo,
                    IsRecommend = topicInfo.IsRecommend,
                    SelfDefineText = topicInfo.SelfDefineText
                };
                return View(topicModel);
            }
            else
                return View(new Models.TopicModel());
        }
        [UnAuthorize]
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Add(string topicJson)
        {
            var s = new Newtonsoft.Json.JsonSerializerSettings();
            s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            Model.TopicInfo topic = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.TopicInfo>(topicJson, s);
            if (topic.TopicModuleInfo.Count == 0)
                return Json(new { success = false, msg = "至少需要录入1个模块！" });

            if (topic.Id > 0)
                _iTopicService.UpdateTopic(topic);
            else
                _iTopicService.AddTopic(topic);

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