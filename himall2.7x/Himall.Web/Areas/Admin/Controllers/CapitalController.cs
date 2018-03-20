using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Payment;
using Himall.Core.Plugins;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CapitalController : BaseAdminController
    {
        IMemberCapitalService _iMemberCapitalService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        IOperationLogService _iOperationLogService;
        public CapitalController(IMemberCapitalService iMemberCapitalService, 
            IMemberService iMemberService, 
            ISiteSettingService iSiteSettingService,
            IOperationLogService iOperationLogService)
        {
            _iMemberCapitalService = iMemberCapitalService;
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iOperationLogService = iOperationLogService;
        }
        // GET: Admin/Capital
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetMemberCapitals(string user, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var memberService = _iMemberService;
            long? membid = null;
            if (!string.IsNullOrWhiteSpace(user))
            {
                var memberInfo = memberService.GetMemberByName(user) ?? new UserMemberInfo() { Id = 0 };
                membid = memberInfo.Id;
            }

            var query = new CapitalQuery
            {
                PageNo = page,
                PageSize = rows,
                Sort = "Balance",
                memberId = membid
            };
            var pagemodel = capitalService.GetCapitals(query);
            var model = pagemodel.Models.ToList().Select(e =>
            {
                var member = memberService.GetMember(e.MemId);
                return new CapitalModel
                {
                    Balance = e.Balance.Value,
                    ChargeAmount = e.ChargeAmount.HasValue ? e.ChargeAmount.Value : 0.00M,
                    FreezeAmount = e.FreezeAmount.HasValue ? e.FreezeAmount.Value : 0.00M,
                    Id = e.Id,
                    UserId = e.MemId,
                    UserCode = member.UserName,
                    UserName = string.IsNullOrWhiteSpace(member.RealName) ? member.UserName : member.RealName
                };
            });
            var models = new DataGridModel<CapitalModel>
            {
                rows = model,
                total = pagemodel.Total
            };
            return Json(models);
        }
        public ActionResult Detail(long id)
        {
            ViewBag.UserId = id;
            return View();
        }
        public ActionResult WithDraw()
        {
            return View();
        }
        public JsonResult List(CapitalDetailInfo.CapitalDetailType capitalType, long userid, string startTime, string endTime, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;

            var query = new CapitalDetailQuery
            {
                memberId = userid,
                capitalType = capitalType,
                PageSize = rows,
                PageNo = page
            };
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                query.startTime = DateTime.Parse(startTime);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                query.endTime = DateTime.Parse(endTime).AddDays(1).AddSeconds(-1);
            }
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.Value.ToString(),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = e.SourceType.ToDescription() + ",单号：" + e.Id,
                PayWay = e.Remark
            }).ToList();

            var models = new DataGridModel<CapitalDetailModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public JsonResult ApplyWithDrawListByUser(long userid, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var query = new ApplyWithDrawQuery
            {
                memberId = userid,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetApplyWithDraw(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                string applyStatus = string.Empty;
                if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail
                    || e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm
                    )
                {
                    applyStatus = "提现中";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
                {
                    applyStatus = "提现失败";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess)
                {
                    applyStatus = "提现成功";
                }
                return new ApplyWithDrawModel
                 {
                     Id = e.Id,
                     ApplyAmount = e.ApplyAmount,
                     ApplyStatus = e.ApplyStatus,
                     ApplyStatusDesc = applyStatus,
                     ApplyTime = e.ApplyTime.ToString()
                 };
            });
            var models = new DataGridModel<ApplyWithDrawModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public JsonResult ApplyWithDrawList(ApplyWithDrawInfo.ApplyWithDrawStatus capitalType, string withdrawno, string user, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var memberService = _iMemberService;
            long? membid = null;
            if (!string.IsNullOrWhiteSpace(user))
            {
                var memberInfo = memberService.GetMemberByName(user) ?? new UserMemberInfo() { Id = 0 };
                membid = memberInfo.Id;
            }
            var query = new ApplyWithDrawQuery
            {
                memberId = membid,
                PageSize = rows,
                PageNo = page,
                withDrawStatus = capitalType
            };
            if (!string.IsNullOrWhiteSpace(withdrawno))
            {
                query.withDrawNo = long.Parse(withdrawno);
            }
            var pageMode = capitalService.GetApplyWithDraw(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                string applyStatus = e.ApplyStatus.ToDescription();
                var mem = memberService.GetMember(e.MemId);
                return new ApplyWithDrawModel
                {
                    Id = e.Id,
                    ApplyAmount = e.ApplyAmount,
                    ApplyStatus = e.ApplyStatus,
                    ApplyStatusDesc = applyStatus,
                    ApplyTime = e.ApplyTime.ToString(),
                    NickName = e.NickName,
                    MemberName = mem.UserName,
                    ConfirmTime = e.ConfirmTime.ToString(),
                    MemId = e.MemId,
                    OpUser = e.OpUser,
                    PayNo = e.PayNo,
                    PayTime = e.PayTime.ToString(),
                    Remark = e.Remark
                };
            });
            var models = new DataGridModel<ApplyWithDrawModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmApply(long id, ApplyWithDrawInfo.ApplyWithDrawStatus comfirmStatus, string remark)
        {
            var service = _iMemberCapitalService;
            ApplyWithDrawQuery query = new ApplyWithDrawQuery
            {
                withDrawNo = id,
                PageNo = 1,
                PageSize = 1
            };
            var status = comfirmStatus;
            var model = service.GetApplyWithDraw(query).Models.FirstOrDefault();
            if (status == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
            {
                service.RefuseApplyWithDraw(id, status, CurrentManager.UserName, remark);
                //操作日志
                _iOperationLogService.AddPlatformOperationLog(
          new LogInfo
          {
              Date = DateTime.Now,
              Description = string.Format("会员提现审核拒绝，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
              status, remark),
              IPAddress = Request.UserHostAddress,
              PageUrl = "/Admin/Capital/WithDraw",
              UserName = CurrentManager.UserName,
              ShopId = 0

          });
                return Json(new { success = true, msg = "审核成功！" });
            }
            else
            {
                var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(e => e.PluginInfo.PluginId.ToLower().Contains("weixin")).FirstOrDefault();
                if (plugins != null)
                {
                    try
                    {
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = model.ApplyAmount,
                            check_name = false,
                            openid = model.OpenId,
                            out_trade_no = model.Id.ToString(),
                            desc = "提现"
                        };
                        PaymentInfo result = plugins.Biz.EnterprisePay(para);
                        ApplyWithDrawInfo info = new ApplyWithDrawInfo
                        {
                            PayNo = result.TradNo,
                            ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess,
                            Remark = plugins.PluginInfo.Description,
                            PayTime = result.TradeTime.HasValue ? result.TradeTime.Value : DateTime.Now,
                            ConfirmTime = DateTime.Now,
                            OpUser = CurrentManager.UserName,
                            ApplyAmount = model.ApplyAmount,
                            Id = model.Id
                        };
                        //Log.Debug("提现:" + info.PayNo);
                        service.ConfirmApplyWithDraw(info);

                        //操作日志
                        _iOperationLogService.AddPlatformOperationLog(
                          new LogInfo
                          {
                              Date = DateTime.Now,
                              Description = string.Format("会员提现审核成功，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                              status, remark),
                              IPAddress = Request.UserHostAddress,
                              PageUrl = "/Admin/Capital/WithDraw",
                              UserName = CurrentManager.UserName,
                              ShopId = 0

                          });
                    }
                    catch(PluginException pex)
                    {//插件异常，直接返回错误信息
                        Log.Error("调用企业付款接口异常：" + pex.Message);
                        //操作日志
                        _iOperationLogService.AddPlatformOperationLog(
                          new LogInfo
                          {
                              Date = DateTime.Now,
                              Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                              status, remark),
                              IPAddress = Request.UserHostAddress,
                              PageUrl = "/Admin/Capital/WithDraw",
                              UserName = CurrentManager.UserName,
                              ShopId = 0

                          });
                        return Json(new { success = false, msg =pex.Message});
                    }
                    catch (Exception ex)
                    {
                        Log.Error("提现审核异常：" + ex.Message);
                        ApplyWithDrawInfo info = new ApplyWithDrawInfo
                        {
                            ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail,
                            Remark = plugins.PluginInfo.Description,
                            ConfirmTime = DateTime.Now,
                            OpUser = CurrentManager.UserName,
                            ApplyAmount = model.ApplyAmount,
                            Id = model.Id
                        };
                        service.ConfirmApplyWithDraw(info);

                        //操作日志
                        _iOperationLogService.AddPlatformOperationLog(
                          new LogInfo
                          {
                              Date = DateTime.Now,
                              Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                              status, remark),
                              IPAddress = Request.UserHostAddress,
                              PageUrl = "/Admin/Capital/WithDraw",
                              UserName = CurrentManager.UserName,
                              ShopId = 0

                          });

                        return Json(new { success = false, msg = "付款接口异常" });
                    }
                }
                else
                {
                    return Json(new { success = false, msg = "未找到支付插件" });
                }
            }

            return Json(new { success = true, msg = "付款成功" });
        }

        public JsonResult Pay(long id)
        {

            return Json(new { success = true, msg = "付款成功" });
        }

        public ActionResult Setting()
        {
            var siteSetting = CurrentSiteSetting;
            return View(siteSetting);
        }

        public JsonResult SaveWithDrawSetting(string minimum,string maximum)
        {
            int min=0,max=0;
            if (int.TryParse(minimum, out min) && int.TryParse(maximum, out max))
            {
                if (min > 0 && min < max && max <= 20000)
                {
                    _iSiteSettingService.SaveSetting("WithDrawMaximum", maximum);
                    _iSiteSettingService.SaveSetting("WithDrawMinimum", minimum);
                    return Json(new Result() { success = true, msg = "保存成功" });
                }
                return Json(new Result() { success = false, msg = "金额范围只能是(1-20000)" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "只能输入数字" });
            }
        }
    }
}