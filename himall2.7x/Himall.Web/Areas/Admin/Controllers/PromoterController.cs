using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Core;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class PromoterController : BaseAdminController
    {
        IDistributionService _iDistributionService;
        IMemberService _iMemberService;
        IRegionService _iRegionService;
        public PromoterController(IDistributionService iDistributionService, IMemberService iMemberService, IRegionService iRegionService)
        {

            _iDistributionService = iDistributionService;
            _iMemberService = iMemberService;
            _iRegionService = iRegionService;
        }


        public ActionResult Management()
        {
            var model = _iDistributionService.GetPromoterStatistics();
            return View(model);
        }

        [HttpPost]
        public ActionResult GetPromoterInfo(long id)
        {
            var model = _iDistributionService.GetPromoter(id);
            if(model==null)
            return Json(new Result() { success = false, msg = "找不到该销售员！" });
            else
            {
                var t = new
                {
                    Id = model.UserId,
                    RegDate = model.Himall_Members.CreateDate.ToString("yyyy-MM-dd"),
                    UserName=model.Himall_Members.UserName,
                    RealName=model.Himall_Members.RealName,
                    CellPhone=model.Himall_Members.CellPhone,
                    Address=_iRegionService.GetFullName(model.Himall_Members.RegionId)+" "+model.Himall_Members.Address,
                    ShopName=model.ShopName
                };
                return Json(new { success=true,data=t});
            }
        }

        [HttpPost]
        public ActionResult Agree(long Id)
        {
            _iDistributionService.AduitPromoter(Id);
            return Json(new Result() { success = true, msg = "审核成功" });
        }

        [HttpPost]
        public ActionResult DisAgree(long Id)
        {
            _iDistributionService.RefusePromoter(Id);
            return Json(new Result() { success = true, msg = "申请已拒绝" });
        }

        public ActionResult Disable(long Id)
        {
            _iDistributionService.DisablePromoter(Id);
            return Json(new Result() { success = true, msg = "已清退" });
        }

        [HttpPost]
        public JsonResult List(int page, int rows, string userName, int status = -1)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                userName = userName.Trim();
            }
            PromoterQuery query = new PromoterQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.UserName = userName;
            if (status != -1)
            {
                query.Status = (Himall.Model.PromoterInfo.PromoterStatus)status;
            }
            var model = _iDistributionService.GetPromoterList(query);
            var list = model.Models.ToList().Select(a => new
            {
                Id = a.Id,
                UserId = a.UserId,
                ShopName = a.ShopName,
                UserName=a.Himall_Members.UserName,
                RealName = a.Himall_Members.RealName == null ? "" : a.Himall_Members.RealName,
                ApplyTime = a.ApplyTime.Value.ToString("yyyy-MM-dd"),
                PassTime = a.PassTime.HasValue ? a.PassTime.Value.ToString("yyyy-MM-dd") : (a.Status == Himall.Model.PromoterInfo.PromoterStatus.Audited? a.ApplyTime.Value.ToString("yyyy-MM-dd") : ""),
                Status = a.Status.ToDescription(),
                CellPhone = a.Himall_Members.CellPhone==null?"":a.Himall_Members.CellPhone,
                Email = a.Himall_Members.Email == null ? "" : a.Himall_Members.Email
            });
            var dataGrid = new { rows = list, total = model.Total };
            return Json(dataGrid);
        }
        public ActionResult SaveSetting(Model.RecruitSettingInfo model)
        {
            _iDistributionService.UpdateRecruitmentSetting(model);
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        [HttpPost]
        public JsonResult GetMembers(string keyWords)
        {
            var after = _iMemberService.GetMembers(false, keyWords).Where(a=>a.Himall_Promoter.Count()>0);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }


        // GET: Admin/Distribution
        public ActionResult Setting()
        {
            var m = _iDistributionService.GetRecruitmentSetting();
            return View(m);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SavePlanning(Model.RecruitPlanInfo model)
        {
            if (!string.IsNullOrWhiteSpace(model.Title) && !string.IsNullOrWhiteSpace(model.Content))
            {
                _iDistributionService.UpdateRecruitmentPlan(model);
                return Json(new Result() { success = true, msg = "保存成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "推广标题和内容不能为空！" });
            }
        }

        public ActionResult Planning()
        {
            var m = _iDistributionService.GetRecruitmentPlan();
            string host = Request.Url.Host;
            host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
            ViewBag.Url = String.Format("http://{0}/m-wap/home/RecruitPlan", host);
            var map = Core.Helper.QRCodeHelper.Create(ViewBag.Url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            ViewBag.QRCode = strUrl;
            return View(m);
        }
    }
}
