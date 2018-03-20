using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ProductConsultationController : BaseAdminController
    {
        private IConsultationService _iConsultationService;
        public ProductConsultationController(IConsultationService iConsultationService)
        {
           _iConsultationService = iConsultationService;
        }
        // GET: Admin/ProductConsultation
        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult List(int page, int rows, string Keywords, int shopid = 0, bool? isReply = null)
        {
            var query = new ConsultationQuery { PageNo = page, PageSize = rows, KeyWords = Keywords, ShopID = shopid, IsReply = isReply };
            var result = _iConsultationService.GetConsultations(query);
            IEnumerable<ProductConsultationModel> consultation = result.Models.ToArray().Select(item => new ProductConsultationModel()
            {
                Id = item.Id,
                ConsultationContent = HTMLEncode(item.ConsultationContent),
                Date = item.ConsultationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ProductName = item.ProductInfo.ProductName,
                ProductId = item.ProductId,
                UserName = item.UserName,
                ReplyContent = HTMLEncode(item.ReplyContent),
                ImagePath=item.ProductInfo.GetImage(ImageSize.Size_50),
                ReplyDate = item.ReplyDate,
            });
            DataGridModel<ProductConsultationModel> model = new DataGridModel<ProductConsultationModel>() { rows = consultation, total = result.Total };
            return Json(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Detail(long id)
        {
            var model = _iConsultationService.GetConsultation(id);
            return Json(new { ConsulationContent = model.ConsultationContent, ReplyContent = model.ReplyContent });
        }
        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iConsultationService.DeleteConsultation(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }

        public static string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }
    }
}