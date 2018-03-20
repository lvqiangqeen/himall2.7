using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;

namespace Himall.Web.Areas.Web.Controllers
{
    public class UserIntegralController : BaseMemberController
    {
        private IGiftsOrderService _iGiftsOrderService;
        private IMemberIntegralService _iMemberIntegralService;
        public UserIntegralController(IGiftsOrderService iGiftsOrderService, IMemberIntegralService iMemberIntegralService)
        {
            _iGiftsOrderService = iGiftsOrderService;
            _iMemberIntegralService = iMemberIntegralService;
        }

        // GET: Web/UserIntegral
        public ActionResult Index(int? type, int pageSize = 10, int pageNo = 1)
        {
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            ViewBag.IntegralPerMoney = model == null ? 0 : model.IntegralPerMoney;
            var memberIntegral = _iMemberIntegralService.GetMemberIntegral(CurrentUser.Id);
            ViewBag.Integral = memberIntegral == null ? 0 : memberIntegral.AvailableIntegrals;
            Himall.Model.MemberIntegral.IntegralType? integralType = null;
            if (type.HasValue)
            {
                integralType = (Himall.Model.MemberIntegral.IntegralType)type.Value;
            }
            var query = new IntegralRecordQuery() { IntegralType = integralType, UserId = CurrentUser.Id, PageNo = pageNo, PageSize = pageSize };
            var result = _iMemberIntegralService.GetIntegralRecordListForWeb(query);
            var list = result.Models.ToList().Select(item => new MemberIntegralRecord
            {
                Id = item.Id,
                UserName = item.UserName,
                RecordDate = item.RecordDate,
                Integral = item.Integral,
                TypeId = item.TypeId,
                ReMark = GetRemarkFromIntegralType(item.TypeId, item.Himall_MemberIntegralRecordAction, item.ReMark)
            });
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = result.Total
            };
            ViewBag.pageInfo = info;
            return View(list);
        }

        public ActionResult IntegralRule()
        {
            var model = _iMemberIntegralService.GetUserHistroyIntegralGroup(CurrentUser.Id);
            return View(model);
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
                //case MemberIntegral.IntegralType.Comment:
                //    remark = "商品评价（商品ID：" + recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                //case MemberIntegral.IntegralType.ProportionRebate:
                //    remark = "使用订单号(" +recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                default:
                    return remark;
            }
            return remark;
        }

        #region 礼品订单
        public ActionResult OrderList(string skey,GiftOrderInfo.GiftOrderStatus? status, int page=1)
        {
            int rows = 12;
            GiftsOrderQuery query = new GiftsOrderQuery();
            query.skey = skey;
            if (status != null)
            {
                if ((int)status != 0)
                {
                    query.status = status;
                }
            }
            query.UserId = CurrentUser.Id;
            query.PageSize = rows;
            query.PageNo = page;
            var orderdata = _iGiftsOrderService.GetOrders(query);
            PagingInfo info = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = rows,
                TotalItems = orderdata.Total
            };
            List<GiftOrderInfo> orderlist = orderdata.Models.ToList();
            ViewBag.pageInfo = info;
            _iGiftsOrderService.OrderAddUserInfo(orderlist);
            var result = orderlist.ToList();
            foreach(var item in result)
            {
                item.Address = ClearHtmlString(item.Address);
                item.CloseReason = ClearHtmlString(item.CloseReason);
                item.UserRemark = ClearHtmlString(item.UserRemark);
            }
            return View(result);
        }
        /// <summary>
        /// 确认到货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmOrder(long id)
        {
            Result result = new Result();
            _iGiftsOrderService.ConfirmOrder(id, CurrentUser.Id);
            result.success = true;
            result.status = 1;
            result.msg = "订单完成";
            return Json(result);
        }
        #endregion

        //TODO：YZY 应该提交
        /// <summary>
        /// 清理引号类字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ClearHtmlString(string str)
        {
            string result = str;
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("'", "&#39;");
                result = result.Replace("\"", "&#34;");
                result = result.Replace(">", "&gt;");
                result = result.Replace("<", "&lt;");
            }
            return result;
        }
    }
}