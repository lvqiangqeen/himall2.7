using Himall.Web.Framework;
using System.Web.Mvc;
using Himall.IServices;
using System.Linq;

namespace Himall.Web.Areas.Web.Controllers
{
    public class TopicController : BaseController
    {
        private ITopicService _iTopicService;
        public TopicController(ITopicService iTopicService)
        {
            _iTopicService = iTopicService;
        }
        // GET: Web/Topic
        public ActionResult Detail(string id = "")
        {
            long tid = 0;
            if (!long.TryParse(id, out tid))
            {
                 //404页面
            }

            var topicObj = _iTopicService.GetTopicInfo(tid);

            if (null == topicObj)
            {
                //404页面
            }
            return View(topicObj);
        }

        public ActionResult List()
        {
            IServices.QueryModel.TopicQuery topicQuery = new IServices.QueryModel.TopicQuery()
            {
                IsRecommend = true,
                PlatformType = Core.PlatformType.PC,
                PageNo = 1,
                PageSize = 5
            };

            var pagemodel = _iTopicService.GetTopics(topicQuery);
            ViewBag.TopicInfo = pagemodel.Models.ToList();
            return View();
        }
        [HttpPost]
        public JsonResult List(int page,int pageSize)
        {
            IServices.QueryModel.TopicQuery topicQuery = new IServices.QueryModel.TopicQuery()
            {
                IsRecommend = true,
                PlatformType = Core.PlatformType.PC,
                PageNo = page,
                PageSize = 5
            };

            var pagemodel = _iTopicService.GetTopics(topicQuery);
            var model = pagemodel.Models.ToList().Select(item => new {id=item.Id, name = item.Name, topimage = item.TopImage });
            return Json(new { success = true, data = model });
        }
    }
}