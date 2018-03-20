using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class MemberIntegralController : BaseAdminController
    {

        private IMemberService _iMemberService;
        private IMemberIntegralService _iMemberIntegralService;
        private IMemberGradeService _iMemberGradeService;
        private IMemberIntegralConversionFactoryService _iMemberIntegralConversionFactoryService;
        public MemberIntegralController(IMemberService iMemberService, 
            IMemberIntegralService iMemberIntegralService, 
            IMemberGradeService iMemberGradeService,
            IMemberIntegralConversionFactoryService iMemberIntegralConversionFactoryService)
        {
            _iMemberService = iMemberService;
            _iMemberIntegralService = iMemberIntegralService;
            _iMemberGradeService = iMemberGradeService;
            _iMemberIntegralConversionFactoryService = iMemberIntegralConversionFactoryService;
        }

        // GET: Admin/MemberIntegral
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Save(string Operation, int Integral, string userName, int? userId, string reMark)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new Core.HimallException("该用户不存在");
            }
            var memeber = _iMemberService.GetMemberByName(userName);
            if (memeber == null)
            {
                throw new Core.HimallException("该用户不存在");
            }
            if (Integral <= 0||Integral>100000)
            {
                throw new Core.HimallException("积分必须为大于0且小于十万的整数");
            }
            MemberIntegralRecord info = new MemberIntegralRecord();
            info.UserName = userName;
            info.MemberId = memeber.Id;
            info.RecordDate = DateTime.Now;
            info.TypeId = MemberIntegral.IntegralType.SystemOper;
            info.ReMark = reMark;
            if (Operation == "sub")
            {
                Integral = -Integral;
            }
            var memberIntegral = _iMemberIntegralConversionFactoryService.Create(MemberIntegral.IntegralType.SystemOper, Integral);

            _iMemberIntegralService.AddMemberIntegral(info, memberIntegral);
            return Json(new Result() { success = true, msg = "操作成功" });
        }

        [Description("分页获取会员积分JSON数据")]
        public JsonResult List(int page, string userName, DateTime? startDate, DateTime? endDate, int rows)
        {
            var memberGrade = _iMemberGradeService.GetMemberGradeList();
            var result = _iMemberIntegralService.GetMemberIntegralList(new IntegralQuery() { UserName = userName, StartDate = startDate, EndDate = endDate, PageNo = page, PageSize = rows });
            var list = result.Models.ToList().Select(item => new
            {
                Id = item.Id,
                UserName = item.UserName,
                UserId = item.MemberId,
                AvailableIntegrals = item.AvailableIntegrals,
                MemberGrade = GetMemberGrade(memberGrade, item.HistoryIntegrals),
                HistoryIntegrals = item.HistoryIntegrals,
                RegDate = item.Himall_Members.CreateDate.ToString("yyyy-MM-dd")
            });

            var model = new { rows = list, total = result.Total };
            return Json(model);
        }

        private string GetMemberGrade(IEnumerable<MemberGrade> memberGrade, int historyIntegrals)
        {
            var grade = memberGrade.Where(a => a.Integral <= historyIntegrals).OrderByDescending(a => a.Integral).FirstOrDefault();
            if (grade == null)
            {
                return "Vip0";
            }
            return grade.GradeName;
        }



        [HttpPost]
        public JsonResult GetMembers(bool? status, string keyWords)
        {
            var after = _iMemberService.GetMembers(status, keyWords);
            var values = after.Select(item => new { key = item.Id, value = item.UserName });
            return Json(values);
        }

        public ActionResult Detail(int id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        public JsonResult GetMemberIntegralDetail(int page, int? userId, Himall.Model.MemberIntegral.IntegralType? type, DateTime? startDate, DateTime? endDate, int rows)
        {

            var query = new IntegralRecordQuery() { StartDate = startDate, EndDate = endDate, IntegralType = type, UserId = userId, PageNo = page, PageSize = rows };
            var result = _iMemberIntegralService.GetIntegralRecordList(query);
            var list = result.Models.ToList().Select(item => new
            {
                Id = item.Id,
                UserName = item.UserName,
                RecordDate = item.RecordDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                Integral = item.Integral,
                Type = item.TypeId.ToDescription(),
                Remark = GetRemarkFromIntegralType(item.TypeId, item.Himall_MemberIntegralRecordAction, item.ReMark)
            });

            var model = new { rows = list, total = result.Total };
            return Json(model);

        }

        private string GetRemarkFromIntegralType(Himall.Model.MemberIntegral.IntegralType type, ICollection<MemberIntegralRecordAction> recordAction, string remark = "")
        {
            if (recordAction == null || recordAction.Count == 0)
                return remark;
            switch (type)
            {
                //case MemberIntegral.IntegralType.InvitationMemberRegiste:
                //    remark = "邀请用户(用户ID：" + recordAction.FirstOrDefault().VirtualItemId+")";
                //    break;
                case MemberIntegral.IntegralType.Consumption:
                    var orderIds = "";
                    foreach (var item in recordAction)
                    {
                        orderIds += item.VirtualItemId + ",";
                    }
                    remark = "使用订单号(" + orderIds.TrimEnd(',') + ")";
                    break;
                case MemberIntegral.IntegralType.Comment:
                    remark = "商品评价（商品ID：" + recordAction.FirstOrDefault().VirtualItemId + ")";
                    break;
                //case MemberIntegral.IntegralType.ProportionRebate:
                //    remark = "使用订单号(" +recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                default:
                    return remark;
            }
            return remark;
        }
    }
}