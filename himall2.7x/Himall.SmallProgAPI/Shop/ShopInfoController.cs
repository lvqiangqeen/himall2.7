using Himall.Application;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using Himall.CommonModel;
using System.IO;
using System.Web.Http;
using Himall.Core.Plugins.Message;
using Himall.DTO;

namespace Himall.SmallProgAPI
{
    public class ShopInfoController : BaseApiController
    {
        public object GetInfo(string userKey)
        {
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            var shopinfo = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);

            var todayAmount = ServiceProvider.Instance<IBillingService>.Create.GetShopTodaySettledAmountByAccountId(manager.ShopId, 0);
            var totalAmount = ServiceProvider.Instance<IBillingService>.Create.GetShopTotalSettledAmountByAccountId(manager.ShopId, 0);

            return Json(new { success = true, ShopName = shopinfo.ShopName, Logo = shopinfo.Logo, TodayAmount = todayAmount, TotalAmount = totalAmount, TipCount = 9 });
        }

        public object GetShopInfo(string userKey)
        {
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            if (manager == null)
            {
                return Json(new { success = false, ShopInfo = "[]" });
            }
            else
            {
                var model = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
                if (model == null)
                {
                    return Json(new { success = false, ShopInfo = "[]" });
                }
                else
                {
                    var info = new
                    {
                        Logo = model.Logo,
                        ShopName = model.ShopName,
                        Industry = model.Industry,
                        CompanyPhone = model.CompanyPhone,
                        Lat = model.Lat,
                        Lng = model.Lng,
                        CompanyAddress = model.CompanyAddress,
                        OpeningTime = model.OpeningTime,
                        BusinessLicenceNumberPhoto = model.BusinessLicenceNumberPhoto,
                        BranchImage = model.BranchImage,
                        ShopDescription = model.ShopDescription,
                        WelcomeTitle = model.ShopDescription,
                        ContactsName = model.ContactsName,
                        ContactsPhone = model.ContactsPhone,
                        ContactsPosition = model.ContactsPosition
                    };
                    return Json(new { success = true, ShopInfo = info });
                }
            }
        }

        /// <summary>
        /// 我的收益
        /// </summary>
        /// <param name="userKey">用户key</param>
        /// <returns></returns>
        public object GetProfit(string userKey)
        {
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            decimal Account = 2;
            decimal MonthProfit = 2;
            decimal LastMonthProfit = 2;
            var Today = new
            {
                GroupProfit = 2,
                QuickProfit = 2
            };
            var Yesterday = new
            {
                GroupProfit = 2,
                QuickProfit = 2
            };

            return Json(new { Status = "OK", Account = Account, MonthProfit = MonthProfit, LastMonthProfit = LastMonthProfit, Today = Today, Yesterday = Yesterday });
        }
        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="userKey">用户key</param>
        /// <returns></returns>
        public object GetWithDrawRecords(string userKey)
        {
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            List<ShopWithDrawInfo> list = new List<ShopWithDrawInfo>();
            ShopWithDrawInfo Models = new ShopWithDrawInfo()
            {
                Id = 1,
                Account = "heavenspring007716",
                AccountName = "杨文俊",
                ApplyTime = DateTime.Now,
                CashAmount = 25,
                CashType = Himall.CommonModel.WithdrawType.WeiChat,
                SellerId = userId,
                SellerName = manager.UserName,
                DealTime = null,
                ShopId = 372,
                ShopRemark = "",
                PlatRemark = "",
                Status = Himall.CommonModel.WithdrawStaus.WatingAudit,
                ShopName = "绝妙发型屋",
                CashNo = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                SerialNo = ""
            };
            ShopWithDrawInfo Models1 = new ShopWithDrawInfo()
            {
                Id = 2,
                Account = "heavenspring007716",
                AccountName = "杨文俊",
                ApplyTime = DateTime.Now,
                CashAmount = 25,
                CashType = Himall.CommonModel.WithdrawType.BankCard,
                SellerId = userId,
                SellerName = manager.UserName,
                DealTime = null,
                ShopId = 372,
                ShopRemark = "",
                PlatRemark = "",
                Status = Himall.CommonModel.WithdrawStaus.Succeed,
                ShopName = "绝妙发型屋",
                CashNo = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                SerialNo = ""
            };
            list.Add(Models);
            list.Add(Models1);
            var data = new { Status = "OK", Data = list };
            return Json(data);
        }
        /// <summary>
        /// 体现明细
        /// </summary>
        /// <param name="userKey">用户key</param>
        /// <param name="id">体现编号</param>
        /// <returns></returns>
        public object GetWithDrawDetailById(string userKey, long id)
        {
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            ShopWithDrawInfo Models = new ShopWithDrawInfo()
            {
                Id = 1,
                Account = "heavenspring007716",
                AccountName = "杨文俊",
                ApplyTime = DateTime.Now,
                CashAmount = 25,
                CashType = Himall.CommonModel.WithdrawType.WeiChat,
                SellerId = userId,
                SellerName = manager.UserName,
                DealTime = null,
                ShopId = 372,
                ShopRemark = "",
                PlatRemark = "",
                Status = Himall.CommonModel.WithdrawStaus.WatingAudit,
                ShopName = "绝妙发型屋",
                CashNo = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                SerialNo = ""
            };
            return Json(new { Status = "OK", Data = Models });
        }
        /// <summary>
        /// 商铺拼团列表
        /// </summary>
        /// <param name="userKey">用户key</param>
        /// <param name="status">拼团状态</param>
        /// <returns></returns>
        public object GetFightGroupInfo(string userKey, int status)
        {
            IFightGroupService _ifightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
            IMemberService _imemberService = ServiceProvider.Instance<IMemberService>.Create;           
            long userId = UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            if (manager != null)
            {
                List<ShopFightGroupModel> list = new List<ShopFightGroupModel>();
                List<FightGroupsInfo> fightGroupList = _ifightGroupService.GetFightGroupsByShopId(manager.ShopId, status);
                if (fightGroupList != null)
                {                  
                    for (int i = 0; i < fightGroupList.Count; i++)
                    {
                        ShopFightGroupModel model = new ShopFightGroupModel();
                        var list1 = fightGroupList[i];
                        model.Id = list1.Id;
                        model.HeadUserId = list1.HeadUserId;
                        model.JoinedNumber = list1.JoinedNumber;
                        model.LimitedNumber = list1.LimitedNumber;
                        model.GroupStatus = list1.GroupStatus;
                        model.ActiveId = list1.ActiveId;
                        var memberInfo = _imemberService.GetMember((long)model.HeadUserId);
                        if (memberInfo != null)
                        {
                            model.Photo = memberInfo.Photo;
                            model.Nick = memberInfo.Nick;
                        }
                        var fightGroupActive = _ifightGroupService.GetFightGroupActiveById((long)model.ActiveId);
                        if (fightGroupActive != null)
                        {
                            model.ProductName = fightGroupActive.ProductName;
                            model.IconUrl = fightGroupActive.IconUrl;
                            model.ReturnMoney = fightGroupActive.ReturnMoney;
                            model.OpenGroupReward = fightGroupActive.OpenGroupReward;
                            model.InvitationReward = fightGroupActive.InvitationReward;
                            model.EndTime = fightGroupActive.EndTime;
                        }
                        var fightGroupActiveItem = _ifightGroupService.GetActiveItemByActiveId((long)model.ActiveId);
                        if (fightGroupActiveItem != null)
                        {
                            model.ActivePrice = fightGroupActiveItem.ActivePrice;
                        }
                        model.TotalPrice = model.JoinedNumber * model.ActivePrice -model.OpenGroupReward- (model.JoinedNumber - 1) * (model.InvitationReward+model.ReturnMoney);
                        model.SalePrice =decimal.Round((decimal)(model.TotalPrice / model.JoinedNumber),2);
                        list.Add(model);
                    }
                    return Json(new { Status="OK",Data=list,Message="数据返回成功"});
                }
                else
                {
                    return Json(new { Status = "OK", Data = "", Message = "没有数据" });
                }
            }
            else
            {
                return Json(new { Status = "NO", Message = "当前用户不存在" });
            }
        }

        /// <summary>
        /// 提现申请数据提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public object ApplyWithDraw(ApplayWithDrawParam applay)
        {
            long userId = UserCookieEncryptHelper.Decrypt(applay.userKey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var manager = ServiceProvider.Instance<IManagerService>.Create.GetSellerManager(userId);
            var shopinfo = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
            Himall.DTO.ShopWithDraw model = new ShopWithDraw()
            {
                SellerId = manager.Id,
                SellerName = manager.UserName,
                ShopId = shopinfo.Id,
                WithdrawalAmount = applay.amount,
                WithdrawType = (Himall.CommonModel.WithdrawType)applay.withdrawType
            };

            bool isbool = BillingApplication.ShopApplyWithDraw(model);

            if (isbool)
                return Json(new { success = true, msg = "成功！" });
            else
                return Json(new { success = false, msg = "余额不足，无法提现！" });
        }
    }

    public class ApplayWithDrawParam
    {
        public string userKey { get; set; }

        public decimal amount { get; set; }

        public int withdrawType { get; set; }

    }
}
