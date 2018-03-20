using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Core;
using System.Dynamic;
using Himall.Core.Plugins.Message;
using Himall.Model;
using Himall.Web.Models;
using Himall.Web.Areas.Web.Models;
using System.Threading.Tasks;

namespace Himall.Web.Areas.Web.Controllers
{
    public class FindPassWordController : BaseWebController
    {
        private IMemberService _iMemberService;

        private IMessageService _iMessageService;
        public FindPassWordController(IMessageService iMessageService, IMemberService iMemberService)
        {
            _iMessageService = iMessageService;
            _iMemberService = iMemberService;
        }
        // GET: Web/FindPassWord
        public ActionResult Index()
        {
            return View();
        }

        //找回密码第二步
        public ActionResult Step2(string key)
        {
            var s = Core.Cache.Get<UserMemberInfo>(key);
            if (s == null)
            {
                return RedirectToAction("Error", "FindPassWord");
            }
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(s.Id, item.PluginInfo.PluginId, Himall.Model.MemberContactsInfo.UserTypes.General))
            });
            ViewBag.BindContactInfo = data;
            ViewBag.Key = key;
            return View(s);
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Step3(string key)
        {
            var s = Core.Cache.Get<UserMemberInfo>(key + "3");
            if (s == null)
            {
                return RedirectToAction("Error", "FindPassWord");
            }
            ViewBag.Key = key;
            return View();
        }

        public ActionResult ChangePassWord(string passWord, string key)
        {
            var member = Core.Cache.Get<UserMemberInfo>(key + "3");
            if (member == null)
            {
                return Json(new { success = false, flag = -1, msg = "验证超时" });
            }
            var userId = member.Id;
            _iMemberService.ChangePassword(userId, passWord);
            MessageUserInfo info = new MessageUserInfo();
            info.SiteName = CurrentSiteSetting.SiteName;
            info.UserName = member.UserName;
            Task.Factory.StartNew(() => _iMessageService.SendMessageOnFindPassWord(userId, info));
            return Json(new { success = true, flag = 1, msg = "成功找回密码" });
        }

        public ActionResult Step4()
        {
            return View();
        }

        public ActionResult GetCheckCode()
        {
            string code;
            var image = Core.Helper.ImageHelper.GenerateCheckCode(out code);
            Session["FindPassWordcheckCode"] = code;
            return File(image.ToArray(), "image/png");
        }
        ///短信或者邮件验证码对比
        [HttpPost]
        public ActionResult CheckPluginCode(string pluginId, string code, string key)
        {
            var member = Core.Cache.Get<UserMemberInfo>(key);
            var cache = CacheKeyCollection.MemberFindPasswordCheck(member.UserName, pluginId);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberFindPasswordCheck(member.UserName, pluginId));
                Core.Cache.Insert(key + "3", member, DateTime.Now.AddMinutes(15));
                return Json(new { success = true, msg = "验证正确", key = key });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }
        void VaildCode(string checkCode)
        {
            if (string.IsNullOrWhiteSpace(checkCode))
            {
                throw new HimallException("验证码不能为空");
            }
            else
            {
                string systemCheckCode = Session["FindPassWordcheckCode"] as string;
                if (string.IsNullOrEmpty(systemCheckCode))
                {
                    throw new HimallException("验证码超时，请刷新");
                }
                if (systemCheckCode.ToLower() != checkCode.ToLower())
                {
                    throw new HimallException("验证码不正确");
                }
            }
            Session["FindPassWordcheckCode"] = Guid.NewGuid().ToString();
        }


        //发送短信邮件验证码
        [HttpPost]
        public ActionResult SendCode(string pluginId, string key)
        {
            var s = Core.Cache.Get<UserMemberInfo>(key);
            if (s == null)
                return Json(new { success = false, flag = -1, msg = "验证已超时！" });
            string destination = _iMessageService.GetDestination(s.Id, pluginId, MemberContactsInfo.UserTypes.General);
            var timeout = CacheKeyCollection.MemberPluginFindPassWordTime(s.UserName, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new { success = false, flag = 0, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberFindPasswordCheck(s.UserName, pluginId), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = s.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginFindPassWordTime(s.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new { success = true, flag = 1, msg = "发送成功" });
        }

        ///第一步，检查用户邮箱手机是否存在对应的用户
        [HttpPost]
        public ActionResult CheckUser(string userName, string checkCode)
        {
            var key = Guid.NewGuid().ToString().Replace("-", "");
            VaildCode(checkCode);
            var memberinfo = _iMemberService.GetMemberByContactInfo(userName);
            UserMemberInfo member = null;
            if (memberinfo != null)
            {
                member = new UserMemberInfo()
                {
                    Id = memberinfo.Id,
                    Address = memberinfo.Address,
                    CellPhone = memberinfo.CellPhone,
                    CreateDate = memberinfo.CreateDate,
                    Disabled = memberinfo.Disabled,
                    Email = memberinfo.Email,
                    Expenditure = memberinfo.Expenditure,
                    InviteUserId = memberinfo.InviteUserId,
                    LastLoginDate = memberinfo.LastLoginDate,
                    Nick = memberinfo.Nick,
                    OrderNumber = memberinfo.OrderNumber,
                    ParentSellerId = memberinfo.ParentSellerId,
                    Password = memberinfo.Password,
                    PasswordSalt = memberinfo.PasswordSalt,
                    PayPwd = memberinfo.PayPwd,
                    PayPwdSalt = memberinfo.PayPwdSalt,
                    Photo = memberinfo.Photo,
                    Points = memberinfo.Points,
                    UserName = memberinfo.UserName,
                    TopRegionId = memberinfo.TopRegionId,
                    ShareUserId = memberinfo.ShareUserId,
                    Sex = memberinfo.Sex,
                    Remark = memberinfo.Remark,
                    RegionId = memberinfo.RegionId,
                    RealName = memberinfo.RealName,
                    QQ = memberinfo.QQ
                };
                member.MemberOpenIdInfo = memberinfo.MemberOpenIdInfo.Select(m => new MemberOpenIdInfo
                {
                    AppIdType = m.AppIdType,
                    Id = m.Id,
                    OpenId = m.OpenId,
                    MemberInfo = null,
                    ServiceProvider = m.ServiceProvider,
                    UnionId = m.UnionId,
                    UnionOpenId = m.UnionOpenId,
                    UserId = m.UserId
                }).ToList();
                member.ShippingAddressInfo = memberinfo.ShippingAddressInfo.Select(s => new ShippingAddressInfo
                {
                    Address = s.Address,
                    Id = s.Id,
                    IsDefault = s.IsDefault,
                    IsQuick = s.IsQuick,
                    Phone = s.Phone,
                    RegionFullName = s.RegionFullName,
                    RegionId = s.RegionId,
                    RegionIdPath = s.RegionIdPath,
                    ShipTo = s.ShipTo,
                    UserId = s.UserId
                }).ToList();
                member.Himall_FavoriteShops = memberinfo.Himall_FavoriteShops.Select(h => new Himall.Model.FavoriteShopInfo
                {
                    Date = h.Date,
                    Id = h.Id,
                    ShopId = h.ShopId,
                    Tags = h.Tags,
                    UserId = h.UserId
                }).ToList();
                member.Himall_BrowsingHistory = memberinfo.Himall_BrowsingHistory.Select(h => new Himall.Model.BrowsingHistoryInfo
                {
                    BrowseTime = h.BrowseTime,
                    Id = h.Id,
                    MemberId = h.MemberId,
                    ProductId = h.ProductId
                }).ToList();
                member.Himall_ProductComments = memberinfo.Himall_ProductComments.Select(h => new Himall.Model.ProductCommentInfo
                {
                    AppendContent = h.AppendContent,
                    AppendDate = h.AppendDate,
                    Email = h.Email,
                    Id = h.Id,
                    IsHidden = h.IsHidden,
                    ProductId = h.ProductId,
                    ReplyAppendContent = h.ReplyAppendContent,
                    ReplyAppendDate = h.ReplyAppendDate,
                    ReplyContent = h.ReplyContent,
                    ReplyDate = h.ReplyDate,
                    ReviewContent = h.ReviewContent,
                    ReviewDate = h.ReviewDate,
                    ReviewMark = h.ReviewMark,
                    ShopId = h.ShopId,
                    ShopName = h.ShopName,
                    SubOrderId = h.SubOrderId,
                    UserId = h.UserId,
                    UserName = h.UserName
                }).ToList();
                member.Himall_MemberIntegral = memberinfo.Himall_MemberIntegral.Select(h => new MemberIntegral
                {
                    AvailableIntegrals = h.AvailableIntegrals,
                    HistoryIntegrals = h.HistoryIntegrals,
                    Id = h.Id,
                    MemberId = h.MemberId,
                    UserName = h.UserName
                }).ToList();
                member.Himall_MemberIntegralRecord = memberinfo.Himall_MemberIntegralRecord.Select(h => new MemberIntegralRecord
                {
                    Id = h.Id,
                    Integral = h.Integral,
                    MemberId = h.MemberId,
                    RecordDate = h.RecordDate,
                    ReMark = h.ReMark,
                    TypeId = h.TypeId,
                    UserName = h.UserName
                }).ToList();
                member.Himall_AgentProducts = memberinfo.Himall_AgentProducts.Select(h => new Himall.Model.AgentProductsInfo
                {
                    AddTime = h.AddTime,
                    Id = h.Id,
                    ProductId = h.ProductId,
                    ShopId = h.ShopId,
                    UserId = h.UserId
                }).ToList();
                member.Himall_Promoter = memberinfo.Himall_Promoter.Select(h => new Himall.Model.PromoterInfo()
                {
                    ApplyTime = h.ApplyTime,
                    Id = h.Id,
                    PassTime = h.PassTime,
                    Remark = h.Remark,
                    ShopName = h.ShopName,
                    Status = h.Status,
                    UserId = h.UserId
                }).ToList();
            }
            if (member == null)
            {
                return Json(new { success = false, tag = "username", msg = "您输入的账户名不存在或者没有绑定邮箱和手机，请核对后重新输入" });
            }
            else
            {
                Core.Cache.Insert<UserMemberInfo>(key, member, DateTime.Now.AddMinutes(15));
                return Json(new { success = true, key = key });
            }
        }
    }
}