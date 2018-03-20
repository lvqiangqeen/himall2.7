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
using Himall.Web.Areas.Web.Models;
using Himall.Web.Models;
using Himall.IServices.QueryModel;
using System.Threading.Tasks;
using Himall.Core.Plugins;

namespace Himall.Web.Areas.Web.Controllers
{
    //TODO:YZY好多Service
    public class UserCenterController : BaseMemberController
    {
        private IMemberService _iMemberService;
        private IProductService _iProductService;
        private IMessageService _iMessageService;
        private ICommentService _iCommentService;
        private IMemberCapitalService _iMemberCapitalService;
        private IOrderService _iOrderService;
        //private IMemberInviteService _iMemberInviteService;
        //private IMemberIntegralService _iMemberIntegralService;
        private ICartService _iCartService;
        public UserCenterController(
            IMemberService iMemberService,
            IProductService iProductService,
            IMessageService iMessageService,
            ICommentService iCommentService,
            IMemberCapitalService iMemberCapitalService,
            IOrderService iOrderService,
            //IMemberInviteService iMemberInviteService,
            //IMemberIntegralService iMemberIntegralService,
            ICartService iCartService
            )
        {
            _iMemberService = iMemberService;
            _iProductService = iProductService;
            _iMessageService = iMessageService;
            _iCommentService = iCommentService;
            _iMemberCapitalService = iMemberCapitalService;
            _iOrderService = iOrderService;
            //_iMemberInviteService = iMemberInviteService;
            //_iMemberIntegralService = iMemberIntegralService;
            _iCartService = iCartService;

        }

        public ActionResult Index()
        {//TODO:个人中心改成单页后，将index框架页改成与home页一致
            return RedirectToAction("home");
        }

        // GET: Web/UserCenter
        public ActionResult Home()
        {
            UserCenterHomeModel viewModel = new UserCenterHomeModel();
            var model = _iMemberService.GetUserCenterModel(CurrentUser.Id);
            viewModel.userCenterModel = model;
            viewModel.UserName = CurrentUser.Nick == "" ? CurrentUser.UserName : CurrentUser.Nick;
            viewModel.Logo = CurrentUser.Photo;
            var items = _iCartService.GetCart(CurrentUser.Id).Items.OrderByDescending(a => a.AddTime).Select(p => p.ProductId).Take(3).ToArray();
            viewModel.ShoppingCartItems = _iProductService.GetProductByIds(items).ToArray();
            var UnEvaluatProducts = _iCommentService.GetUnEvaluatProducts(CurrentUser.Id).ToArray();
            viewModel.UnEvaluatProductsNum = UnEvaluatProducts.Count();
            viewModel.Top3UnEvaluatProducts = UnEvaluatProducts.Take(3).ToArray();
            viewModel.Top3RecommendProducts = _iProductService.GetPlatHotSaleProductByNearShop(8, CurrentUser.Id).ToArray();
            viewModel.BrowsingProducts = BrowseHistrory.GetBrowsingProducts(4, CurrentUser == null ? 0 : CurrentUser.Id);

            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Himall.Model.MemberContactsInfo.UserTypes.General))
            });
            viewModel.BindContactInfo = data;
            var orders = _iOrderService.GetOrders<OrderInfo>(new OrderQuery
            {
                PageNo = 1,
                PageSize = int.MaxValue,
                UserId = CurrentUser.Id
            });
            viewModel.OrderCount = orders.Total;
            //交易订单 待收货
            viewModel.OrderWaitReceiving = orders.Models.Where(c => c.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving).Count();
            //交易订单 待付款
            viewModel.OrderWaitPay = orders.Models.Where(c => c.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).Count();
            //交易订单 待评价
            var productEvaluations = _iCommentService.GetProductEvaluation(new CommentQuery
            {
                UserID = CurrentUser.Id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Sort = "PComment"
            });
            var orderEvaluations = productEvaluations.Models.Where(item => item.EvaluationStatus == false).Select(item => item.OrderId).Distinct();
            viewModel.OrderEvaluationStatus = orderEvaluations.Count();
            //TODO:[LLY]增加我的资产
            var capitalInfo = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
            var balance = 0M;
            if (capitalInfo != null && capitalInfo.Balance.HasValue)
            {
                balance = capitalInfo.Balance.Value;
            }
            viewModel.Balance = balance;
            //TODO:[YZG]增加账户安全等级
            MemberAccountSafety memberAccountSafety = new MemberAccountSafety();
            memberAccountSafety.AccountSafetyLevel = 1;
            if (CurrentUser.PayPwd != null)
            {
                memberAccountSafety.PayPassword = true;
                memberAccountSafety.AccountSafetyLevel += 1;
            }
            var ImessageService = _iMessageService;
            foreach (var messagePlugin in data)
            {
                if (messagePlugin.PluginId.IndexOf("SMS") > 0)
                {
                    if (messagePlugin.IsBind)
                    {
                        memberAccountSafety.BindPhone = true;
                        memberAccountSafety.AccountSafetyLevel += 1;
                    }
                }
                else
                {
                    if (messagePlugin.IsBind)
                    {
                        memberAccountSafety.BindEmail = true;
                        memberAccountSafety.AccountSafetyLevel += 1;
                    }
                }
            }
            viewModel.memberAccountSafety = memberAccountSafety;
            return View(viewModel);
        }

        public ActionResult Bind(string pluginId)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            return View();
        }

        [HttpPost]
        public ActionResult SendCode(string pluginId, string destination)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Get(timeout) != null)
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId+destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public ActionResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            var member = CurrentUser;
            var mark = "";
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, MemberContactsInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                    mark = "邮箱";
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                    mark = "手机";
                }

                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = CurrentUser.Id, UserType = MemberContactsInfo.UserTypes.General });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);

				MemberIntegralRecord info = new MemberIntegralRecord();
				//_iMemberIntegralConversionFactoryService = ServiceHelper.Create<im;
				//_iMemberIntegralService = ServiceHelper.Create<IMemberIntegralService>();
				//_iMemberInviteService = ServiceHelper.Create<IMemberInviteService>();
				info.UserName = member.UserName;
				info.MemberId = member.Id;
				info.RecordDate = DateTime.Now;
				info.TypeId = MemberIntegral.IntegralType.Reg;
				info.ReMark = "绑定" + mark;
				var memberIntegral = ServiceHelper.Create<IMemberIntegralConversionFactoryService>().Create(MemberIntegral.IntegralType.Reg);
				ServiceHelper.Create<IMemberIntegralService>().AddMemberIntegral(info, memberIntegral);

				if (member.InviteUserId.HasValue)
				{
					var inviteMember = _iMemberService.GetMember(member.InviteUserId.Value);
					if (inviteMember != null)
						ServiceHelper.Create<IMemberInviteService>().AddInviteIntegel(member, inviteMember);
				}

                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        public ActionResult Finished()
        {
            return View();
        }

        public ActionResult AccountSafety()
        {
            MemberAccountSafety model = new MemberAccountSafety();
            model.AccountSafetyLevel = 1;
            if (CurrentUser.PayPwd != null)
            {
                model.PayPassword = true;
                model.AccountSafetyLevel += 1;
            }
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var ImessageService = _iMessageService;
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(ImessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Himall.Model.MemberContactsInfo.UserTypes.General))
            });
            foreach (var messagePlugin in data)
            {
                if (messagePlugin.PluginId.IndexOf("SMS") > 0)
                {
                    if (messagePlugin.IsBind)
                    {
                        model.BindPhone = true;
                        model.AccountSafetyLevel += 1;
                    }
                }
                else
                {
                    if (messagePlugin.IsBind)
                    {
                        model.BindEmail = true;
                        model.AccountSafetyLevel += 1;
                    }
                }
            }
            return View(model);
        }
    }
}
