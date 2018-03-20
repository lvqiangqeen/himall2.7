using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using Himall.CommonModel;
using EntityFramework.Extensions;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class MemberService : ServiceBase, IMemberService
    {
        #region 方法
        public string GetLogo()
        {
            return Context.SiteSettingsInfo.Where(p => p.Key == "MemberLogo").FirstOrDefault().MemberLogo;
        }

        public UserMemberInfo GetMemberByName(string userName)
        {
            UserMemberInfo result = Context.UserMemberInfo.FirstOrDefault(d=>d.UserName== userName);
            return result;
        }

        /// <summary>
        /// 新方法修改用户信息
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMemberInfo(UserMemberInfo model)
        {
            base.UpdateData<UserMemberInfo>(model);
            Context.SaveChanges();
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public void UpdateMember(UserMemberInfo model)
        {
            var m = Context.UserMemberInfo.FindById(model.Id);
            m.Nick = model.Nick;
            m.RealName = model.RealName;
            m.Email = model.Email;
            m.QQ = model.QQ;
            m.CellPhone = model.CellPhone;
            Context.SaveChanges();
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public void DeleteMember(long id)
        {
            Context.UserMemberInfo.Remove(id);
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(id);
            Context.SaveChanges();
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }

        public QueryPageModel<UserMemberInfo> GetMembers(MemberQuery query)
        {
            int total = 0;
            IQueryable<UserMemberInfo> users = Context.UserMemberInfo.AsQueryable();
            if (query.IsHaveEmail.HasValue)
            {
                if (query.IsHaveEmail.Value)
                {
                    users = users.Where(u => u.Email != null && u.Email != "");
                }
                //else
                //{
                //    users = users.Where(u => u.Email == "");
                //}
            }
            if (!string.IsNullOrEmpty(query.Mobile))
            {
                users = users.Where(d => d.CellPhone.Equals(query.Mobile));
            }
            if (!string.IsNullOrWhiteSpace(query.keyWords))
            {
                users = users.Where(d => d.UserName.Equals(query.keyWords));
            }
            if (!string.IsNullOrWhiteSpace(query.weChatNick))
            {
                users = users.Where(d => d.Nick.Contains(query.weChatNick));
            }
            if (query.Status.HasValue && query.Status.Value)
            {
                users = users.Where(d => d.Disabled);
            }
            else if (query.Status.HasValue && !query.Status.Value)
            {
                users = users.Where(d => !d.Disabled);
            }
            if (query.LabelId != null && query.LabelId.Length > 0)
            {
                users = (from l in Context.MemberLabelInfo
                         join u in users on l.MemId equals u.Id
                         where query.LabelId.Contains(l.LabelId)
                         select u);
            }


            if (query.LoginTimeStart.HasValue)
            {
                users = users.Where(e => e.LastLoginDate >= query.LoginTimeStart.Value);
            }
            if (query.LoginTimeEnd.HasValue)
            {
                var end = query.LoginTimeEnd.Value.Date.AddDays(1);
                users = users.Where(e => e.LastLoginDate < end);
            }
            if (query.RegistTimeStart.HasValue)
            {
                users = users.Where(e => e.CreateDate >= query.RegistTimeStart.Value);
            }
            if (query.RegistTimeEnd.HasValue)
            {
                var end = query.RegistTimeEnd.Value.Date.AddDays(1);
                users = users.Where(e => e.CreateDate < end);
            }
            if (query.IsSeller.HasValue)
            {
                var managers = Context.ManagerInfo.Join(Context.ShopInfo, item => item.ShopId, shop => shop.Id, (item, shop) => new { item.UserName, shop.ShopStatus }).Where(r => r.ShopStatus == ShopInfo.ShopAuditStatus.Open).Select(e => e.UserName);
                if (query.IsSeller.Value)
                {
                    users = users.Where(e => managers.Contains(e.UserName));
                }
                else
                {
                    users = users.Where(e => !managers.Contains(e.UserName));
                }
            }
            if (query.IsFocusWeiXin.HasValue)
            {
                var openids = (from m in Context.MemberOpenIdInfo
                               join u in users on m.UserId equals u.Id
                               select new { openid = m.OpenId, userid = u.Id });
                var focusUsers = (from ids in Context.OpenIdsInfo
                                  join o in openids on ids.OpenId equals o.openid
                                  where ids.IsSubscribe
                                  select o.userid);
                if (query.IsFocusWeiXin.Value)
                {
                    users = users.Where(e => focusUsers.Contains(e.Id));
                }
                else
                {
                    users = users.Where(e => !focusUsers.Contains(e.Id));
                }
            }
            if (query.MinIntegral.HasValue) //积分范围
            {
                //users = (from u in users
                //         join i in context.MemberIntegral on u.Id equals i.MemberId
                //         where i.HistoryIntegrals >= query.MinIntegral && i.HistoryIntegrals <= query.MaxIntegral

                //         //from gci in gc.DefaultIfEmpty(
                //         //       new MemberIntegral { HistoryIntegrals = 0}     //设置为空时的默认值
                //         //       )
                //         select u);

                users = users.Join(Context.MemberIntegral.Where(i => i.HistoryIntegrals >= query.MinIntegral),
                    a => a.Id, b => b.MemberId, (a, b) => a);
            }
            if (query.MaxIntegral.HasValue) //积分范围
            {
                //users = (from u in users
                //         join i in context.MemberIntegral on u.Id equals i.MemberId
                //         where i.HistoryIntegrals >= query.MinIntegral && i.HistoryIntegrals <= query.MaxIntegral

                //         //from gci in gc.DefaultIfEmpty(
                //         //       new MemberIntegral { HistoryIntegrals = 0}     //设置为空时的默认值
                //         //       )
                //         select u);

                users = users.Join(Context.MemberIntegral.Where(i => i.HistoryIntegrals <= query.MaxIntegral),
                    a => a.Id, b => b.MemberId, (a, b) => a);
            }
            #region 会员分组搜索

            if (query.MemberStatisticsType.HasValue)
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                switch (query.MemberStatisticsType.Value)
                {
                    case MemberStatisticsType.ActiveOne:
                        var oneIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == true).Select(p => p.UserId);
                        users = users.Where(p => oneIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.ActiveThree:
                        var threeIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == true).Select(p => p.UserId);
                        users = users.Where(p => threeIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.ActiveSix:
                        var sixIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == false && p.SixMonth == true).Select(p => p.UserId);
                        users = users.Where(p => sixIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.SleepingThree:
                        startDate = DateTime.Now.AddMonths(-6);
                        endDate = DateTime.Now.AddMonths(-3);
                        users = users.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingSix:
                        startDate = DateTime.Now.AddMonths(-9);
                        endDate = DateTime.Now.AddMonths(-6);
                        users = users.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingNine:
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now.AddMonths(-9);
                        users = users.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwelve:
                        startDate = DateTime.Now.AddMonths(-24);
                        endDate = DateTime.Now.AddMonths(-12);
                        users = users.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwentyFour:
                        endDate = DateTime.Now.AddMonths(-24);
                        users = users.Where(p => p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.BirthdayToday:
                        users = users.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day == DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayToMonth:
                        users = users.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day != DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayNextMonth:
                        startDate = DateTime.Now.AddMonths(1);
                        users = users.Where(p => p.BirthDay.Value.Month == startDate.Month);
                        break;
                    case MemberStatisticsType.RegisteredMember:
                        users = users.Where(p => p.OrderNumber == 0);
                        break;
                }
            }

            #endregion
            users = users.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderBy(o => o.Id));
            QueryPageModel<UserMemberInfo> pageModel = new QueryPageModel<UserMemberInfo>() { Models = users.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<UserMemberInfo> GetMembers(IEnumerable<long> userIds)
        {
            List<UserMemberInfo> result = null;
            if (userIds.Count() > 0)
            {
                string ids = string.Join(",", userIds.ToArray());
                string sql = "SELECT * FROM Himall_Members WHERE Id in ("+ids+")";
                using (var conn = new MySqlConnection(Connection.ConnectionString))
                {
                    result = conn.Query<UserMemberInfo>(sql, new { Ids = ids }).ToList();
                }
            }
            return result;
        }

        public UserMemberInfo GetMember(long id)
        {
            UserMemberInfo result = Context.UserMemberInfo.FirstOrDefault(d => d.Id == id);
            if (result != null)
            {
                int memberIntergral = GetHistoryIntegral(result.Id);
                result.MemberDiscount = GetMemberDiscount(memberIntergral);
            }
            return result;
        }

        /// <summary>
        /// 根据用户id和类型获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public MemberOpenIdInfo GetMemberOpenIdInfoByuserId(long userId, MemberOpenIdInfo.AppIdTypeEnum appIdType)
        {
            return this.Context.MemberOpenIdInfo.FirstOrDefault(p => p.UserId == userId && p.AppIdType == appIdType);
        }


        public void LockMember(long id)
        {
            var model = Context.UserMemberInfo.FindById(id);
            model.Disabled = true;
            Context.SaveChanges();
            string CACHE_USER_KEY = CacheKeyCollection.Member(id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public void UnLockMember(long id)
        {
            var model = Context.UserMemberInfo.FindById(id);
            model.Disabled = false;
            Context.SaveChanges();
            string CACHE_USER_KEY = CacheKeyCollection.Member(id);
            Core.Cache.Remove(CACHE_USER_KEY);
        }

        public IQueryable<UserMemberInfo> GetMembers(bool? status, string keyWords)
        {
            IQueryable<UserMemberInfo> members = Context.UserMemberInfo.FindBy(item => item.ParentSellerId == 0 &&
                       (!status.HasValue || item.Disabled == status.Value) &&
                         (keyWords == null || keyWords == "" || item.UserName.Contains(keyWords)));
            return members;
        }


        public void ChangePassword(long id, string password)
        {
            if (password.Length < 6)
                throw new HimallException("密码长度至少6位字符！");
            var model = Context.UserMemberInfo.FindById(id);

            ChangePassword(model, password);
        }

        /// <summary>
        /// 根据用户名修改密码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public void ChangePassword(string name, string password)
        {
            var user = this.Context.UserMemberInfo.FirstOrDefault(p => p.UserName == name);
            if (user == null)
                throw new HimallException("未找到指定用户");

            ChangePassword(user, password);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public void ChangePayPassword(long id, string password)
        {
            if (password.Length < 6)
                throw new HimallException("密码长度至少6位字符！");

            var model = Context.UserMemberInfo.FirstOrDefault(p => p.Id == id);
            if (model != null)
            {
                model.PayPwd = GetPasswrodWithTwiceEncode(password, model.PayPwdSalt);
                Context.SaveChanges();
            }
        }


        public void BatchDeleteMember(long[] ids)
        {
            Context.UserMemberInfo.Remove(ids);
            Context.SaveChanges();

            foreach (var id in ids)
            {
                string CACHE_USER_KEY = CacheKeyCollection.Member(id);
                Core.Cache.Remove(CACHE_USER_KEY);
            }
        }


        public void BatchLock(long[] ids)
        {
            var models = Context.UserMemberInfo.Where(item => ids.Contains(item.Id));
            foreach (var model in models)
            {
                model.Disabled = true;
            }
            Context.SaveChanges();
            foreach (var id in ids)
            {
                string CACHE_USER_KEY = CacheKeyCollection.Member(id);
                Core.Cache.Remove(CACHE_USER_KEY);
            }
        }

        public UserMemberInfo GetUserByCache(long userId)
        {
            UserMemberInfo memberinfo = null;
            UserMemberInfo member = null;
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(userId);
            if (Cache.Exists(CACHE_MEMBER_KEY))
            {
                memberinfo = Core.Cache.Get<UserMemberInfo>(CACHE_MEMBER_KEY);
            }
            if (memberinfo == null)
            {
                memberinfo = GetMember(userId);
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
                        BirthDay = memberinfo.BirthDay,
                        QQ = memberinfo.QQ,
                        NetAmount = memberinfo.NetAmount,
                        photo = memberinfo.photo,
                        LastConsumptionTime = memberinfo.LastConsumptionTime,
                        TotalAmount = memberinfo.TotalAmount,
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

                    //memberinfo.Himall_BonusReceive = memberinfo.Himall_BonusReceive.Select(t => new BonusReceiveInfo()
                    //{
                    //    Id = t.Id,
                    //    BonusId = t.BonusId,
                    //    IsShare = t.IsShare,
                    //    IsTransformedDeposit = t.IsTransformedDeposit,
                    //    OpenId = t.OpenId,
                    //    Price = t.Price,
                    //    ReceiveTime = t.ReceiveTime,
                    //    UserId = t.UserId
                    //}).ToList();

                    int memberIntergral = GetHistoryIntegral(member.Id);
                    member.MemberDiscount = GetMemberDiscount(memberIntergral);

                }
                if (memberinfo != null)
                    Cache.Insert<UserMemberInfo>(CACHE_MEMBER_KEY, member, DateTime.Now.AddMinutes(15));
            }
            return memberinfo;
        }

        private string GetMemberGrade(int historyIntegrals)
        {
            var grade = Context.MemberGrade.OrderByDescending(a => a.Integral).FirstOrDefault(a => a.Integral <= historyIntegrals);
            if (grade == null)
            {
                return "Vip0";
            }
            return grade.GradeName;
        }
        public int GetHistoryIntegral(long userId)
        {
            var historyIntegral = Context.MemberIntegral.Where(a => a.MemberId == userId).Select(a => a.HistoryIntegrals).FirstOrDefault();
            return historyIntegral;
        }
        private decimal GetMemberDiscount(int historyIntegrals)
        {
            var grade = Context.MemberGrade.OrderByDescending(a => a.Integral).FirstOrDefault(a => a.Integral <= historyIntegrals);
            if (grade != null)
                return grade.Discount / 10;
            return 1;
        }
        public UserCenterModel GetUserCenterModel(long id)
        {
            var orders = Context.OrderInfo.Where(a => a.UserId == id).ToList();
            UserCenterModel model = new UserCenterModel();
            //  model.Expenditure = context.UserMemberInfo.FindById(id).Expenditure;
            var historyIntegral = Context.MemberIntegral.Where(a => a.MemberId == id).Select(a => a.HistoryIntegrals).FirstOrDefault();
            model.GradeName = GetMemberGrade(historyIntegral);
            model.Intergral = Context.MemberIntegral.Where(a => a.MemberId == id).Select(a => a.AvailableIntegrals).FirstOrDefault();
            model.UserCoupon = Context.CouponRecordInfo.Count(a => a.UserId == id && a.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && a.Himall_Coupon.EndTime > DateTime.Now);
            model.UserCoupon += Context.ShopBonusReceiveInfo.Count(p => p.UserId == id && p.State == ShopBonusReceiveInfo.ReceiveState.NotUse && p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd > DateTime.Now);
            model.RefundCount = Context.OrderRefundInfo.Count(a => a.UserId == id && (a.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit && a.ManagerConfirmStatus != OrderRefundInfo.OrderRefundConfirmStatus.Confirmed));
            model.WaitPayOrders = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).Count();
            model.WaitReceivingOrders = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving).Count();
            var waitdelordnum = orders.Count(item => item.OrderStatus == Model.OrderInfo.OrderOperateStatus.WaitDelivery);//获取待发货订单数
            var fgwaitdelordnum = ServiceProvider.Instance<IOrderService>.Create.GetFightGroupOrderByUser(id);
            model.WaitDeliveryOrders = waitdelordnum - fgwaitdelordnum;
            model.WaitEvaluationOrders = orders.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.Finish && a.OrderCommentInfo.Count == 0).Count();
            model.FollowProductCount = Context.FavoriteInfo.Count(a => a.UserId == id);
            if (model.FollowProductCount > 0)
            {
                model.FollwProducts = Context.FavoriteInfo.Where(a => a.UserId == id).OrderByDescending(a => a.Id).ToArray()
                    .Select(a =>
                        new FollowProduct()
                        {
                            ProductId = a.ProductId,
                            ProductName = a.ProductInfo.ProductName,
                            Price = a.ProductInfo.MinSalePrice,
                            ImagePath = a.ProductInfo.ImagePath
                        }).Take(4).ToList();
            }
            var shopinfos = Context.FavoriteShopInfo.Include("Himall_Shops").Where(a => a.UserId == id).OrderByDescending(a => a.Id).ToArray();
            model.FollowShopsCount = shopinfos.Length;
            if (shopinfos.Length > 0)
            {
                var shops = shopinfos.Select(a => new FollowShop { ShopName = a.Himall_Shops.ShopName, Logo = a.Himall_Shops.Himall_VShop.FirstOrDefault() != null?a.Himall_Shops.Himall_VShop.FirstOrDefault().Logo:a.Himall_Shops.Logo, ShopID = a.ShopId }).Take(4).ToList();
                model.FollowShops = shops;
            }
            model.Orders = ServiceProvider.Instance<IOrderService>.Create.GetTopOrders(3, id);
            //购物车
            model.FollowShopCartsCount = Context.ShoppingCartItemInfo.Count(a => a.UserId == id);
            if (model.FollowShopCartsCount > 0)
            {
                var followShopCarts = (from p in Context.ShoppingCartItemInfo
                                       join o in Context.ProductInfo on p.ProductId equals o.Id
                                       join x in Context.ShopInfo on o.ShopId equals x.Id
                                       where p.UserId == id && o.IsDeleted == false
                                       select o
                                         ).Distinct().Take(4).ToList();

                model.FollowShopCarts =
                    (from o in followShopCarts
                     select new FollowShopCart
                     {
                         ImagePath = o.ImagePath,
                         ProductName = o.ProductName,
                         ProductId = o.Id
                     }).ToList();
            }
            return model;
        }

        public UserMemberInfo Register(string username, string password, string mobile = "", string email = "", long introducer = 0)
        {
            //检查输入合法性
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(username))
                throw new HimallException("用户名 " + username + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("密码不能为空");

            if (!string.IsNullOrEmpty(mobile) && Core.Helper.ValidateHelper.IsMobile(mobile))
            {
                if (CheckMobileExist(mobile))
                {
                    throw new HimallException("手机号已经被其它会员注册");
                }
            }
            if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email))
            {
                if (CheckEmailExist(email))
                {
                    throw new HimallException("邮箱已经被其它会员注册");
                }
            }
            password = password.Trim();
            UserMemberInfo member;
            var salt = Guid.NewGuid().ToString("N").Substring(12);
            password = GetPasswrodWithTwiceEncode(password, salt);
            using (TransactionScope scope = new TransactionScope())
            {
                //填充会员信息
                member = new UserMemberInfo()
                {
                    UserName = username,
                    PasswordSalt = salt,
                    CreateDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Nick = username,
                    RealName = username,
                    CellPhone = mobile,
                    Email = email
                };
                if (introducer != 0)
                    member.InviteUserId = introducer;
                //密码加密
                member.Password = password;
                member = Context.UserMemberInfo.Add(member);
                Context.SaveChanges();
                if (!string.IsNullOrEmpty(mobile) && Core.Helper.ValidateHelper.IsMobile(mobile)) //绑定手机号
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = mobile, ServiceProvider = "Himall.Plugin.Message.SMS", UserId = member.Id, UserType = MemberContactsInfo.UserTypes.General });
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = username;
                    info.MemberId = member.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = MemberIntegral.IntegralType.Reg;
                    info.ReMark = "绑定手机";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                    var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                    if (introducer != 0)
                    {
                        var inviteMember = GetMember(introducer);
                        if (inviteMember != null)
                        {
                            inviteService.AddInviteIntegel(member, inviteMember);
                        }
                    }
                }
                //TODO:DZY[160301]起码有三处,优化时记得合并
                if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email)) //绑定邮箱
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = email, ServiceProvider = "Himall.Plugin.Message.Email", UserId = member.Id, UserType = MemberContactsInfo.UserTypes.General });
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = username;
                    info.MemberId = member.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = MemberIntegral.IntegralType.Reg;
                    info.ReMark = "绑定邮箱";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                    var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                    if (introducer != 0)
                    {
                        var inviteMember = GetMember(introducer);
                        if (inviteMember != null)
                        {
                            inviteService.AddInviteIntegel(member, inviteMember);
                        }
                    }
                }
                scope.Complete();
            }
            return member;
        }

        public UserMemberInfo Register(string username, string password, string serviceProvider, string openId, string sex = null, string headImage = null, long introducer = 0, string nickname = null, string unionid = null, string city = null, string province = null)
        {
            if (string.IsNullOrWhiteSpace(serviceProvider))
                throw new ArgumentNullException("信任登录提供商不能为空");
            if (string.IsNullOrWhiteSpace(openId))
                throw new ArgumentNullException("openId不能为空");

            //检查OpenId是否被使用
            //CheckOpenIdHasBeenUsed(serviceProvider, openId);

            //检查输入合法性
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(username))
                throw new HimallException("用户名 " + username + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("密码不能为空");
            password = password.Trim();
            int? wxsex = null;
            if (!string.IsNullOrEmpty(sex))
                wxsex = int.Parse(sex);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var region = iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province);
                if (region != null)
                    topRegionID = region.Id;
                if (!string.IsNullOrEmpty(city))
                {
                    region = iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City);
                    if (region != null)
                    {
                        regionID = region.Id;
                    }
                }
            }

            //填充会员信息
            UserMemberInfo memeber = new UserMemberInfo()
            {
                UserName = username,
                PasswordSalt = Guid.NewGuid().ToString("N").Substring(12),
                CreateDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                Nick = string.IsNullOrWhiteSpace(nickname) ? username : nickname,
                Sex = wxsex.HasValue ? (Himall.CommonModel.SexType?)wxsex.Value : null,
                TopRegionId = topRegionID,
                RegionId = regionID
            };
            if (Core.Helper.ValidateHelper.IsMobile(username))
            {
                memeber.CellPhone = username;
            }
            if (introducer != 0)
                memeber.InviteUserId = introducer;
            using (TransactionScope scope = new TransactionScope())
            {
                //密码加密
                memeber.Password = GetPasswrodWithTwiceEncode(password, memeber.PasswordSalt);
                memeber = Context.UserMemberInfo.Add(memeber);
                Context.SaveChanges();

                //更新绑定
                MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = memeber.Id,
                    OpenId = openId,
                    ServiceProvider = serviceProvider,
                    UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid
                };
                ChangeOpenIdBindMember(memberOpenIdInfo);
                Context.SaveChanges();

                if (!string.IsNullOrWhiteSpace(headImage))
                    memeber.Photo = TransferHeadImage(headImage, memeber.Id);
                Context.SaveChanges();

                if (!string.IsNullOrEmpty(username) && Core.Helper.ValidateHelper.IsMobile(username)) //绑定手机号
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = username, ServiceProvider = "Himall.Plugin.Message.SMS", UserId = memeber.Id, UserType = MemberContactsInfo.UserTypes.General });
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = MemberIntegral.IntegralType.Reg;
                    info.ReMark = "绑定手机";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                    var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                    if (introducer != 0)
                    {
                        var inviteMember = GetMember(introducer);
                        if (inviteMember != null)
                        {
                            inviteService.AddInviteIntegel(memeber, inviteMember);
                        }
                    }
                }
                scope.Complete();
            }
            return memeber;
        }

        public UserMemberInfo Register(OAuthUserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LoginProvider))
                throw new ArgumentNullException("信任登录提供商不能为空");
            if (string.IsNullOrWhiteSpace(model.OpenId))
                throw new ArgumentNullException("openId不能为空");

            //检查OpenId是否被使用
            //CheckOpenIdHasBeenUsed(model.LoginProvider, model.OpenId);

            //检查输入合法性
            if (string.IsNullOrWhiteSpace(model.UserName))
                throw new ArgumentNullException("用户名不能为空");
            if (CheckMemberExist(model.UserName))
                throw new HimallException("用户名 " + model.UserName + " 已经被其它会员注册");

            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentNullException("密码不能为空");
            var password = model.Password.Trim();

            var sex = 0;
            if (int.TryParse(model.Sex, out sex))
            {

            }

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(model.Province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                var regionProvince = iRegionService.GetRegionByName(model.Province.Trim(), Region.RegionLevel.Province);
                if (regionProvince != null)
                {
                    topRegionID = (int)regionProvince.Id;
                }
                if (!string.IsNullOrEmpty(model.City))
                {
                    var regionCity = iRegionService.GetRegionByName(model.City.Trim(), Region.RegionLevel.City);
                    if (regionCity != null)
                    {
                        regionID = (int)regionCity.Id;
                    }
                }
            }


            //填充会员信息
            UserMemberInfo memeber = new UserMemberInfo()
            {
                UserName = model.UserName,
                PasswordSalt = Guid.NewGuid().ToString("N").Substring(12),
                CreateDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                Email = model.Email,
                Nick = string.IsNullOrWhiteSpace(model.NickName) ? model.UserName : model.NickName,
                Sex = (Himall.CommonModel.SexType)sex,
                TopRegionId = topRegionID,
                RegionId = regionID
            };

            if (model.introducer.HasValue && model.introducer.Value != 0)
                memeber.InviteUserId = model.introducer.Value;
            if (Core.Helper.ValidateHelper.IsMobile(model.UserName))
            {
                memeber.CellPhone = model.UserName;
            }
            using (TransactionScope scope = new TransactionScope())
            {
                //密码加密
                memeber.Password = GetPasswrodWithTwiceEncode(password, memeber.PasswordSalt);
                memeber = Context.UserMemberInfo.Add(memeber);
                Context.SaveChanges();
                //更新绑定
                MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = memeber.Id,
                    OpenId = model.OpenId,
                    ServiceProvider = model.LoginProvider,
                    UnionId = string.IsNullOrWhiteSpace(model.UnionId) ? string.Empty : model.UnionId
                };
                ChangeOpenIdBindMember(memberOpenIdInfo);
                Context.SaveChanges();

                if (!string.IsNullOrWhiteSpace(model.Headimgurl))
                    memeber.Photo = TransferHeadImage(model.Headimgurl, memeber.Id);
                Context.SaveChanges();
                var username = model.UserName;
                if (!string.IsNullOrEmpty(username) && Core.Helper.ValidateHelper.IsMobile(username)) //绑定手机号
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = username, ServiceProvider = "Himall.Plugin.Message.SMS", UserId = memeber.Id, UserType = MemberContactsInfo.UserTypes.General });
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = MemberIntegral.IntegralType.Reg;
                    info.ReMark = "绑定手机";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }
                var email = model.Email;
                if (!string.IsNullOrEmpty(email) && Core.Helper.ValidateHelper.IsEmail(email)) //绑定邮箱
                {
                    var service = ServiceProvider.Instance<IMessageService>.Create;
                    service.UpdateMemberContacts(new Model.MemberContactsInfo() { Contact = email, ServiceProvider = "Himall.Plugin.Message.Email", UserId = memeber.Id, UserType = MemberContactsInfo.UserTypes.General });
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = username;
                    info.MemberId = memeber.Id;
                    info.RecordDate = DateTime.Now;
                    info.TypeId = MemberIntegral.IntegralType.Reg;
                    info.ReMark = "绑定邮箱";
                    var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Reg);
                    ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
                }

                var inviteService = ServiceProvider.Instance<IMemberInviteService>.Create;
                if (model.introducer.HasValue && model.introducer.Value != 0)
                {
                    var inviteMember = GetMember(model.introducer.Value);
                    if (inviteMember != null)
                    {
                        inviteService.AddInviteIntegel(memeber, inviteMember);
                    }
                }
                scope.Complete();
            }
            return memeber;
        }
        public bool CheckMemberExist(string username)
        {
            bool result = Context.UserMemberInfo.Any(item => item.UserName == username);
            return result;
        }

        public bool CheckMobileExist(string mobile)
        {
            bool result = Context.UserMemberInfo.Any(item => item.CellPhone == mobile);
            return result;
        }
        public bool CheckEmailExist(string email)
        {
            bool result = Context.UserMemberInfo.Any(item => item.Email == email);
            return result;
        }

        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }

        public UserMemberInfo Login(string username, string password)
        {
            UserMemberInfo memberInfo = null;

            var IsEmail = Core.Helper.ValidateHelper.IsEmail(username);
            var IsPhone = Core.Helper.ValidateHelper.IsPhone(username);
            if (IsEmail)
            {
                var contact = Context.MemberContactsInfo.Where(a => a.ServiceProvider == "Himall.Plugin.Message.Email" && a.Contact == username && a.UserType == MemberContactsInfo.UserTypes.General).FirstOrDefault();
                if (contact == null)
                    return null;
                else
                    memberInfo = Context.UserMemberInfo.Where(a => a.Id == contact.UserId).FirstOrDefault();
            }
            else if (IsPhone)
            {
                var contact = Context.MemberContactsInfo.Where(a => a.ServiceProvider == "Himall.Plugin.Message.SMS" && a.Contact == username && a.UserType == MemberContactsInfo.UserTypes.General).FirstOrDefault();
                if (contact == null)
                    return null;
                else
                    memberInfo = Context.UserMemberInfo.Where(a => a.Id == contact.UserId).FirstOrDefault();
            }
            else
            {
                memberInfo = GetMemberByName(username);
            }

            if (memberInfo != null)
            {
                string encryptedWithSaltPassword = GetPasswrodWithTwiceEncode(password, memberInfo.PasswordSalt);
                if (encryptedWithSaltPassword.ToLower() != memberInfo.Password)//比较密码是否一致
                    memberInfo = null;//不一致，则置空，表示未找到指定的会员
                else
                {
                    if (memberInfo.Disabled)
                        throw new HimallException("账号已被冻结");

                    //一致，则更新最后登录时间

                    memberInfo.LastLoginDate = DateTime.Now;
                    Context.SaveChanges();
                    Task.Factory.StartNew(() => { AddIntegel(memberInfo); }); //给用户加积分//执行登录后初始化相关操作
                    string CACHE_MEMBER_KEY = CacheKeyCollection.Member(memberInfo.Id);
                    Core.Cache.Remove(CACHE_MEMBER_KEY);
                }
            }
            return memberInfo;
        }

        /// <summary>
        /// 修改最后登录时间
        /// </summary>
        /// <param name="id"></param>
        public void UpdateLastLoginDate(long id)
        {
            var memberInfo = this.Context.UserMemberInfo.FirstOrDefault(p => p.Id == id);
            if (memberInfo != null)
            {
                memberInfo.LastLoginDate = DateTime.Now;
                this.Context.SaveChanges();
            }
        }

        private void AddIntegel(UserMemberInfo member)
        {
            if (!ServiceProvider.Instance<IMemberIntegralService>.Create.HasLoginIntegralRecord(member.Id)) //当天没有登录过，加积分
            {
                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.ReMark = "每天登录";
                info.TypeId = MemberIntegral.IntegralType.Login;
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Login);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
            }
        }


        public UserMemberInfo GetMemberByOpenId(string serviceProvider, string openId)
        {
            UserMemberInfo memberInfo = null;
            var memberOpenInfo = Context.MemberOpenIdInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.OpenId == openId);
            if (memberOpenInfo != null)
            {
                memberInfo = Context.UserMemberInfo.FindById(memberOpenInfo.UserId);
                if (memberInfo != null)
                {
                    int memberIntergral = GetHistoryIntegral(memberInfo.Id);
                    memberInfo.MemberDiscount = GetMemberDiscount(memberIntergral);
                }
            }
            return memberInfo;
        }


        public UserMemberInfo GetMemberByContactInfo(string contact)
        {
            UserMemberInfo memberInfo = null;
            var memberContactInfo = Context.MemberContactsInfo.FirstOrDefault(item => item.Contact == contact && item.UserType == MemberContactsInfo.UserTypes.General);
            if (memberContactInfo != null)
                memberInfo = Context.UserMemberInfo.FindById(memberContactInfo.UserId);
            else
                memberInfo = Context.UserMemberInfo.Where(a => a.UserName == contact && Context.MemberContactsInfo.Any(item => item.UserId == a.Id)).FirstOrDefault();
            return memberInfo;
        }

        public void CheckContactInfoHasBeenUsed(string serviceProvider, string contact, Himall.Model.MemberContactsInfo.UserTypes userType = Himall.Model.MemberContactsInfo.UserTypes.General)
        {
            var memberOpenIdInfo = Context.MemberContactsInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.Contact == contact && item.UserType == userType);
            if (memberOpenIdInfo != null)
                throw new HimallException(string.Format("{0}已经被其它用户绑定", contact));
        }
        /// <summary>
        /// 获取一个新的用户名
        /// </summary>
        /// <returns></returns>
        private string GetNewUserName()
        {
            string result = "";
            while (true)
            {
                result = "wx";
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                result += seeds[rnd.Next(0, seedlen)];
                if (!Context.UserMemberInfo.Any(d => d.UserName == result))
                {
                    break;
                }
            }
            return result;
        }

        public UserMemberInfo QuickRegister(string username, string realName, string nickName, string serviceProvider, string openId, string unionid, string sex = null, string headImage = null, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionopenid = null, string city = null, string province = null)
        {
            if (string.IsNullOrEmpty(unionid) && string.IsNullOrEmpty(openId))
                throw new ArgumentNullException("unionid and openId");

            UserMemberInfo userMember = null;
            if (!string.IsNullOrWhiteSpace(unionid))
            {
                userMember = GetMemberByUnionId(unionid);
            }
            if (userMember == null)
                userMember = GetMemberByOpenId(serviceProvider, openId);

            if (userMember == null)
            {
                username = GetNewUserName();   //重新生成用户名

                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentNullException("用户名不能为空");
                if (string.IsNullOrWhiteSpace(serviceProvider))
                    throw new ArgumentNullException("服务提供商不能为空");

                //检查OpenId是否被使用
                //CheckOpenIdHasBeenUsed(serviceProvider, openId);

                if (string.IsNullOrWhiteSpace(nickName))
                    nickName = username;
                var salt = "o" + Guid.NewGuid().ToString("N").Substring(12);  //o开头表示一键注册用户
                string password = GetPasswrodWithTwiceEncode("", salt);
                int? wxsex = null;
                if (!string.IsNullOrEmpty(sex))
                    wxsex = int.Parse(sex);


                int topRegionID = 0;
                int regionID = 0;
                //省份信息绑定
                if (!string.IsNullOrEmpty(province))
                {
                    var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                    topRegionID = iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province).Id;
                    if (!string.IsNullOrEmpty(city))
                    {
                        regionID = iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City).Id;
                    }
                }

                //填充会员信息
                userMember = new UserMemberInfo()
                {
                    UserName = username,
                    PasswordSalt = salt,
                    CreateDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Nick = nickName,
                    RealName = realName,
                    Sex = wxsex.HasValue ? ((Himall.CommonModel.SexType?)wxsex.Value) : null,
                    TopRegionId = topRegionID,
                    RegionId = regionID
                };

                //密码加密
                userMember.Password = password;
                userMember = Context.UserMemberInfo.Add(userMember);
                Context.SaveChanges();

                if (!string.IsNullOrWhiteSpace(headImage))
                    userMember.Photo = TransferHeadImage(headImage, userMember.Id);
                Context.SaveChanges();
            }            
            else
            {
                //Log.Debug("QuickRegister:" + headImage);
                //如果头像发生改变
                if (!string.IsNullOrWhiteSpace(headImage))
                {
                    userMember.Photo = TransferHeadImage(headImage, userMember.Id);
                    Context.SaveChanges();
                }
            }

            if (!this.Context.MemberOpenIdInfo.Any(p => p.UserId == userMember.Id && p.OpenId == openId))//微信和app的OpenId不同
            {
                var memberOpenIdInfo = new MemberOpenIdInfo()
                {
                    UserId = userMember.Id,
                    OpenId = openId,
                    ServiceProvider = serviceProvider,
                    AppIdType = appidtype,
                    UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid,
                    UnionOpenId = string.IsNullOrWhiteSpace(unionopenid) ? string.Empty : unionopenid
                };
                Context.MemberOpenIdInfo.Add(memberOpenIdInfo);
                Context.SaveChanges();
            }
            return userMember;
        }


        void CheckOpenIdHasBeenUsed(string serviceProvider, string openId, long userId = 0)
        {
            var memberOpenIdInfo = Context.MemberOpenIdInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.OpenId == openId);
            //if (userId != 0)
            //{
            //    var exesit = context.MemberOpenIdInfo.FirstOrDefault(a => a.UserId == userId && a.ServiceProvider == serviceProvider);
            //    if (exesit != null)
            //        throw new HimallException(string.Format("该用户已经绑定其它帐号,请用绑定的帐号登录"));
            //}
            if (memberOpenIdInfo != null)
                throw new HimallException(string.Format("OpenId:{0}已经被使用", openId));
        }

        public void BindMember(long userId, string serviceProvider, string openId, string sex = null, string headImage = null, string unionid = null, string unionopenid = null, string city = null, string province = null)
        {
            //检查是否已经存在同一服务商相同的openId
            //CheckOpenIdHasBeenUsed(serviceProvider, openId, userId);
            int? wxsex = null;
            if (!string.IsNullOrEmpty(sex))
                wxsex = int.Parse(sex);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                topRegionID = (int)iRegionService.GetRegionByName(province.Trim(), Region.RegionLevel.Province).Id;
                if (!string.IsNullOrEmpty(city))
                {
                    regionID = (int)iRegionService.GetRegionByName(city.Trim(), Region.RegionLevel.City).Id;
                }
            }

            var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == userId);
            if (!string.IsNullOrWhiteSpace(headImage))
            {
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                    member.Photo = TransferHeadImage(headImage, userId);
            }

            if (wxsex != null)
                member.Sex = (Himall.CommonModel.SexType)wxsex.Value;

            member.RegionId = regionID;
            member.TopRegionId = topRegionID;

            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = userId,
                OpenId = openId,
                ServiceProvider = serviceProvider,
                UnionId = unionid == null ? string.Empty : unionid,
                UnionOpenId = string.IsNullOrWhiteSpace(unionopenid) ? string.Empty : unionopenid
            };
            ChangeOpenIdBindMember(memberOpenIdInfo);
            Context.SaveChanges();
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            //注：因绑定OpenId的代码入口不同，所有会有多处调用此方法
            Himall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(userId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }


        string TransferHeadImage(string image, long memberId)
        {
            string localName = string.Empty;
            if (!string.IsNullOrWhiteSpace(image))
            {
                if ((image.StartsWith("http://") || image.StartsWith("https://")) && image.IndexOf("/Storage") < 0)//网络图片
                {
                    var webClient = new WebClient();
                    string shortName = image.Substring(image.LastIndexOf('/'));
                    string ext = string.Empty;//获取文件扩展名
                    if (shortName.LastIndexOf('.') <= 0)//如果扩展名不包含 '.'，那么说明该文件名不包含扩展名，因此扩展名赋空
                        ext = ".jpg";
                    else
                        ext = shortName.Substring(shortName.LastIndexOf('.'));//否则取扩展名

                    string localTempName = "/temp/" + DateTime.Now.ToString("yyMMddHHmmssff") + ext;
                    try
                    {
                        var bytes = webClient.DownloadData(image);
                        Stream stream = new MemoryStream(bytes);
                        Core.HimallIO.CreateFile(localTempName, stream, FileCreateType.Create);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex.Message);
                        //网络下载异常
                        localName = null;
                    }
                    image = localTempName;
                }
                string directory = string.Format("/Storage/Member/{0}", memberId);
                localName = directory + "/headImage.jpg";
                if (image.Contains("/temp/"))
                {
                    image = image.Substring(image.LastIndexOf("/temp"));
                    //转移图片
                    try { 
                        Core.HimallIO.CopyFile(image, localName, true);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("复制图片异常（TransferHeadImage）：" + image + " " + ex.Message);
                    }
                }
            }
            return localName;
        }

        public void BindMember(long userId, string serviceProvider, string openId, MemberOpenIdInfo.AppIdTypeEnum AppidType, string sex = null, string headImage = null, string unionid = null)
        {
            //检查是否已经存在同一服务商相同的openId
            //CheckOpenIdHasBeenUsed(serviceProvider, openId, userId);
            Log.Info("进入BindMember");
            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = userId,
                OpenId = openId,
                ServiceProvider = serviceProvider,
                AppIdType = AppidType,
                UnionId = string.IsNullOrWhiteSpace(unionid) ? string.Empty : unionid
            };

            var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == userId);
            if (!string.IsNullOrWhiteSpace(headImage))
            {
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                    member.Photo = TransferHeadImage(headImage, userId);
                if (!string.IsNullOrEmpty(sex))
                    member.Sex = (Himall.CommonModel.SexType)int.Parse(sex);
            }
            Core.Log.Error("headImage " + headImage);
            Core.Log.Error("member.Photo " + member.Photo);
            Core.Log.Error("TransferHeadImage(headImage, userId)  " + TransferHeadImage(headImage, userId));
            //Context.MemberOpenIdInfo.Add(memberOpenIdInfo);
            ChangeOpenIdBindMember(memberOpenIdInfo);
            Context.SaveChanges();
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            Himall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            if (serviceProvider.ToLower() == "Himall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(member);
                //Task.Factory.StartNew(()=>AddBindInergral(member));
            }
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(userId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        public bool VerificationPayPwd(long memid, string payPwd)
        {
            payPwd = payPwd.Trim();
            var data = Context.UserMemberInfo.Where(e => e.Id == memid).Select(p => new { p.PayPwdSalt, p.PayPwd }).FirstOrDefault();
            if (data != null)
            {
                var pwdmd5 = Himall.Core.Helper.SecureHelper.MD5(Himall.Core.Helper.SecureHelper.MD5(payPwd) + data.PayPwdSalt);
                if (pwdmd5 == data.PayPwd)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否有支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasPayPwd(long id)
        {
            var payPwd = Context.UserMemberInfo.Where(p => p.Id == id).Select(p => p.PayPwd).FirstOrDefault();
            return string.IsNullOrEmpty(payPwd) == false;
        }

        public void BindMember(OAuthUserModel model)
        {
            //检查是否已经存在同一服务商相同的openId
            //CheckOpenIdHasBeenUsed(model.LoginProvider, model.OpenId, model.UserId);

            int topRegionID = 0;
            int regionID = 0;
            //省份信息绑定
            if (!string.IsNullOrEmpty(model.Province))
            {
                var iRegionService = ServiceProvider.Instance<IRegionService>.Create;
                topRegionID = (int)iRegionService.GetRegionByName(model.Province.Trim(), Region.RegionLevel.Province).Id;
                if (!string.IsNullOrEmpty(model.City))
                {
                    regionID = (int)iRegionService.GetRegionByName(model.City.Trim()).Id;
                }
            }
            var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == model.UserId);
            if (!string.IsNullOrWhiteSpace(model.Headimgurl))
            {
                Log.Error("BindMember(OAuthUserModel model.Headimgurl )"+model.Headimgurl);
                Log.Error("member.Photol )" + member.Photo);
                //if (string.IsNullOrWhiteSpace(member.Photo) && Core.HimallIO.ExistFile(member.Photo))//优先使用原头像
                if (string.IsNullOrWhiteSpace(member.Photo))//优先使用原头像
                        member.Photo = TransferHeadImage(model.Headimgurl, model.UserId);
            }
            if (!string.IsNullOrWhiteSpace(model.NickName))
            {
                member.Nick = model.NickName;
            }
            if (!string.IsNullOrWhiteSpace(model.Sex))
            {
                int sex = 0;
                if (int.TryParse(model.Sex, out sex))
                {
                    member.Sex = (Himall.CommonModel.SexType)sex;
                }
            }

            member.TopRegionId = topRegionID;
            member.RegionId = regionID;

            //更新绑定
            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
            {
                UserId = model.UserId,
                OpenId = model.OpenId,
                ServiceProvider = model.LoginProvider,
                AppIdType = model.AppIdType,
                UnionId = string.IsNullOrWhiteSpace(model.UnionId) ? string.Empty : model.UnionId
            };
            ChangeOpenIdBindMember(memberOpenIdInfo);
            Context.SaveChanges();
            //TODO:ZJT  在绑定用户与OpenId的时候，检查此OpenId是否存在红包，存在则添加到用户预存款里
            Himall.ServiceProvider.Instance<IBonusService>.Create.DepositToRegister(member.Id);
            if (model.LoginProvider == "Himall.Plugin.OAuth.WeiXin".ToLower())
            {
                AddBindInergral(member);
                //Task.Factory.StartNew(()=>AddBindInergral(member));
            }
            string CACHE_MEMBER_KEY = CacheKeyCollection.Member(model.UserId);
            Core.Cache.Remove(CACHE_MEMBER_KEY);
        }

        private void ChangeOpenIdBindMember(MemberOpenIdInfo model)
        {
            //更新绑定
            MemberOpenIdInfo memberOpenIdInfo = Context.MemberOpenIdInfo.FirstOrDefault(d => d.OpenId == model.OpenId && d.ServiceProvider == model.ServiceProvider);
            //清理绑定
            //Context.MemberOpenIdInfo.Remove(d => d.UserId == model.UserId && d.OpenId == model.OpenId);
            if (memberOpenIdInfo != null)
            {
                Context.MemberOpenIdInfo.Remove(memberOpenIdInfo);
            }
            Context.MemberOpenIdInfo.Add(model);
        }

        private void AddBindInergral(UserMemberInfo member)
        {
            bool x = Context.MemberIntegralRecord.Any(a => a.MemberId == member.Id && a.TypeId == MemberIntegral.IntegralType.BindWX);
            if (x)
                return;
            try
            {
                //绑定微信积分
                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.TypeId = MemberIntegral.IntegralType.BindWX;
                info.ReMark = "绑定微信";
                var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.BindWX);
                ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(info, memberIntegral);
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex);
            }
        }


        public void DeleteMemberOpenId(long userid, string openid)
        {
            var memberopenid = Context.MemberOpenIdInfo.Where(e => e.UserId == userid).Where(item => string.IsNullOrEmpty(openid) || (!string.IsNullOrEmpty(openid) && openid == item.OpenId));
            Context.MemberOpenIdInfo.RemoveRange(memberopenid);
            Context.SaveChanges();
        }


        public UserMemberInfo GetMemberByUnionId(string serviceProvider, string UnionId)
        {
            UserMemberInfo memberInfo = null;
            var memberOpenInfo = Context.MemberOpenIdInfo.FirstOrDefault(item => item.ServiceProvider == serviceProvider && item.UnionId == UnionId);
            if (memberOpenInfo != null)
                memberInfo = Context.UserMemberInfo.FindById(memberOpenInfo.UserId);

            return memberInfo;
        }
        public UserMemberInfo GetMemberByUnionId(string UnionId)
        {
            UserMemberInfo memberInfo = null;
            if (!string.IsNullOrWhiteSpace(UnionId))
            {
                var memberOpenInfo = Context.MemberOpenIdInfo.FirstOrDefault(item => item.UnionId == UnionId);
                if (memberOpenInfo != null)
                    memberInfo = Context.UserMemberInfo.FindById(memberOpenInfo.UserId);
            }
            return memberInfo;
        }

        public IQueryable<MemberLabelInfo> GetMembersByLabel(long labelid)
        {
            return Context.MemberLabelInfo.Where(item => item.LabelId == labelid);
        }

        #region 分销用户关系
        /// <summary>
        /// 更改用户的推销员，并建立店铺分佣关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shareUserId">销售员用户编号</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns>0表示无须维护或数据错误</returns>
        public long UpdateShareUserId(long id, long shareUserId, long shopId)
        {
            long result = 0;
            if (shopId > 0 && shareUserId > 0 && shareUserId != id)
            {
                DistributionUserLinkInfo _tmpdata;
                bool isNeedAdd = true;
                if (id > 0)
                {
                    _tmpdata = Context.DistributionUserLinkInfo.FirstOrDefault(d => d.BuyUserId == id && d.ShopId == shopId);
                    if (_tmpdata != null)
                    {
                        isNeedAdd = false;
                        if (_tmpdata.PartnerId != shareUserId)
                        {
                            _tmpdata.PartnerId = shareUserId;
                            Context.SaveChanges();
                        }
                    }
                }
                if (isNeedAdd)
                {
                    _tmpdata = new DistributionUserLinkInfo();
                    _tmpdata.PartnerId = shareUserId;
                    _tmpdata.ShopId = shopId;
                    _tmpdata.BuyUserId = id;
                    _tmpdata.LinkTime = DateTime.Now;
                    Context.DistributionUserLinkInfo.Add(_tmpdata);
                    Context.SaveChanges();
                    if (id == 0)
                    {
                        result = _tmpdata.Id;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 更新用户关系
        /// </summary>
        /// <param name="id">关系信息编号列表</param>
        /// <param name="userId">买家编号</param>
        public void UpdateDistributionUserLink(IEnumerable<long> ids, long userId)
        {
            if (ids.Count() > 0 && userId > 0)
            {
                List<DistributionUserLinkInfo> links = Context.DistributionUserLinkInfo.Where(d => ids.Contains(d.Id)).ToList();
                foreach (var item in links)
                {
                    item.BuyUserId = userId;
                }
                Context.SaveChanges();
            }
            //清理过期信息
            ClearExpiredLinkData();
        }
        /// <summary>
        /// 清理过期信息
        /// <para>超过一天的垃圾链接信息</para>
        /// </summary>
        public void ClearExpiredLinkData()
        {
            Context.Database.ExecuteSqlCommand("delete from himall_distributionuserlink where TIMESTAMPDIFF(HOUR,LinkTime,NOW())>24 and BuyUserId=0");
        }
        #endregion

        public IEnumerable<MemberLabelInfo> GetMemberLabels(long memid)
        {
            return Context.MemberLabelInfo.Where(e => e.MemId == memid).ToList();
        }

        public void SetMemberLabel(long userid, IEnumerable<long> labelids)
        {
            var memLabels = Context.MemberLabelInfo.Where(e => e.MemId == userid);
            Context.MemberLabelInfo.RemoveRange(memLabels);
            if (labelids.Count() > 0)
            {
                var labelInfo = labelids.Select(e => new MemberLabelInfo { LabelId = e, MemId = userid });
                Context.MemberLabelInfo.AddRange(labelInfo);
            }
            Context.SaveChanges();
        }
        public void SetMembersLabel(long[] userid, IEnumerable<long> labelids)
        {
            var memLabels = Context.MemberLabelInfo.Where(e => userid.Contains(e.MemId) && labelids.Contains(e.LabelId)).ToList();
            var member = (from u in userid
                          from l in labelids
                          select new { uid = u, lid = l });
            var label = from m in member
                        where !memLabels.Any(l => m.lid == l.LabelId && m.uid == l.MemId)
                        select new MemberLabelInfo { LabelId = m.lid, MemId = m.uid };
            Context.MemberLabelInfo.AddRange(label);
            Context.SaveChanges();
        }

        public IEnumerable<int> GetAllTopRegion()
        {
            return Context.UserMemberInfo.Select(e => e.TopRegionId).Distinct();
        }

        /// <summary>
        /// 通过会员等级ID获取会员消费范围
        /// </summary>
        /// <param name="gradeId"></param>
        /// <returns></returns>
        public GradeIntegralRange GetMemberGradeRange(long gradeId)
        {
            GradeIntegralRange range = new GradeIntegralRange();
            var min = Context.MemberGrade.Where(a => a.Id == gradeId).Select(a => a.Integral).FirstOrDefault();
            var max = min;
            var maxIntegral = Context.MemberGrade.Where(a => a.Integral > min).OrderBy(a => a.Integral).FirstOrDefault();
            if (maxIntegral != null)
            {
                max = maxIntegral.Integral;
            }
            range.MinIntegral = min;
            range.MaxIntegral = max;
            return range;
        }


        /// <summary>
        /// 会员购买力列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<UserMemberInfo> GetPurchasingPowerMember(MemberPowerQuery query)
        {
            var result = Context.UserMemberInfo.Where(p => p.Disabled == false);

            #region 最近消费
            if (query.RecentlySpentTime.HasValue)
            {
                var date = DateTime.Now;
                switch (query.RecentlySpentTime.Value)
                {
                    case RecentlySpentTime.OneWeek:
                        date = DateTime.Now.AddDays(-7);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.TwoWeek:
                        date = DateTime.Now.AddDays(-14);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.OneMonthWithin:
                        date = DateTime.Now.AddMonths(-1);
                        result = result.Where(p => p.LastConsumptionTime > date);
                        break;
                    case RecentlySpentTime.OneMonth:
                        date = DateTime.Now.AddMonths(-1);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.TwoMonth:
                        date = DateTime.Now.AddMonths(-2);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.ThreeMonth:
                        date = DateTime.Now.AddMonths(-3);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                    case RecentlySpentTime.SixMonth:
                        date = DateTime.Now.AddMonths(-6);
                        result = result.Where(p => p.LastConsumptionTime < date);
                        break;
                }
            }
            if (query.StartTime.HasValue)
            {
                var date = query.StartTime.Value;
                result = result.Where(p => p.LastConsumptionTime > date);
            }
            if (query.EndTime.HasValue)
            {
                var date = query.EndTime.Value.AddDays(1);
                result = result.Where(p => p.LastConsumptionTime < date);
            }
            #endregion

            #region 购买次数

            if (query.Purchases.HasValue)
            {
                switch (query.Purchases.Value)
                {
                    case Purchases.ZeroTimes:
                        result = result.Where(p => p.OrderNumber == 0);
                        break;
                    case Purchases.OneTimes:
                        result = result.Where(p => p.OrderNumber >= 1);
                        break;
                    case Purchases.TwoTimes:
                        result = result.Where(p => p.OrderNumber >= 2);
                        break;
                    case Purchases.ThreeTimes:
                        result = result.Where(p => p.OrderNumber >= 3);
                        break;
                    case Purchases.FourTimes:
                        result = result.Where(p => p.OrderNumber >= 4);
                        break;
                }
            }

            if (query.StartPurchases.HasValue)
            {
                result = result.Where(p => p.OrderNumber >= query.StartPurchases.Value);
            }

            if (query.EndPurchases.HasValue)
            {
                result = result.Where(p => p.OrderNumber <= query.EndPurchases.Value);
            }

            #endregion

            #region 类目

            if (query.CategoryId.HasValue)
            {
                var userIds = Context.MemberBuyCategoryInfo.Where(p => p.CategoryId == query.CategoryId.Value).Select(p => p.UserId);
                result = result.Where(p => userIds.Contains(p.Id));
            }

            #endregion

            #region 消费金额
            if (query.AmountOfConsumption.HasValue)
            {
                switch (query.AmountOfConsumption.Value)
                {
                    case AmountOfConsumption.AmountOne:
                        result = result.Where(p => p.NetAmount >= 0 && p.NetAmount < 500);
                        break;
                    case AmountOfConsumption.AmountTwo:
                        result = result.Where(p => p.NetAmount >= 500 && p.NetAmount < 1000);
                        break;
                    case AmountOfConsumption.AmountThree:
                        result = result.Where(p => p.NetAmount >= 1000 && p.NetAmount < 3000);
                        break;
                    case AmountOfConsumption.AmountFour:
                        result = result.Where(p => p.NetAmount >= 3000);
                        break;
                        //case AmountOfConsumption.AmountFive:
                        //    result = result.Where(p => p.NetAmount >= 200 && p.NetAmount < 300);
                        //    break;
                }
            }

            if (query.StartAmountOfConsumption.HasValue)
            {
                result = result.Where(p => p.NetAmount >= query.StartAmountOfConsumption.Value);
            }

            if (query.EndAmountOfConsumption.HasValue)
            {
                result = result.Where(p => p.NetAmount <= query.EndAmountOfConsumption.Value);
            }
            #endregion

            #region 会员标签
            if (query.LabelId.HasValue)
            {
                var userIds = Context.MemberLabelInfo.Where(p => p.LabelId == query.LabelId.Value).Select(p => p.MemId);
                result = result.Where(p => userIds.Contains(p.Id));
            }

            if (query.LabelIds != null && query.LabelIds.Count() > 0)
            {
                var userIds = Context.MemberLabelInfo.Where(p => query.LabelIds.Contains(p.LabelId)).Select(p => p.MemId);
                result = result.Where(p => userIds.Contains(p.Id));
            }
            #endregion

            #region 会员分组搜索

            if (query.MemberStatisticsType.HasValue)
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                switch (query.MemberStatisticsType.Value)
                {
                    case MemberStatisticsType.ActiveOne:
                        var oneIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == true).Select(p => p.UserId);
                        result = result.Where(p => oneIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.ActiveThree:
                        var threeIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == true).Select(p => p.UserId);
                        result = result.Where(p => threeIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.ActiveSix:
                        var sixIds = Context.MemberActivityDegreeInfo.Where(p => p.OneMonth == false && p.ThreeMonth == false && p.SixMonth == true).Select(p => p.UserId);
                        result = result.Where(p => sixIds.Contains(p.Id));
                        break;
                    case MemberStatisticsType.SleepingThree:
                        startDate = DateTime.Now.AddMonths(-6);
                        endDate = DateTime.Now.AddMonths(-3);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingSix:
                        startDate = DateTime.Now.AddMonths(-9);
                        endDate = DateTime.Now.AddMonths(-6);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingNine:
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now.AddMonths(-9);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwelve:
                        startDate = DateTime.Now.AddMonths(-24);
                        endDate = DateTime.Now.AddMonths(-12);
                        result = result.Where(p => p.LastConsumptionTime > startDate && p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.SleepingTwentyFour:
                        endDate = DateTime.Now.AddMonths(-24);
                        result = result.Where(p => p.LastConsumptionTime < endDate);
                        break;
                    case MemberStatisticsType.BirthdayToday:
                        result = result.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day == DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayToMonth:
                        result = result.Where(p => p.BirthDay.Value.Month == DateTime.Now.Month && p.BirthDay.Value.Day != DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayNextMonth:
                        startDate = DateTime.Now.AddMonths(1);
                        result = result.Where(p => p.BirthDay.Value.Month == startDate.Month);
                        break;
                    case MemberStatisticsType.RegisteredMember:
                        result = result.Where(p => p.OrderNumber == 0);
                        break;
                }
            }

            #endregion

            #region 排序
            var orderBy = result.GetOrderBy(d => d.OrderByDescending(o => o.NetAmount));

            if ((!string.IsNullOrWhiteSpace(query.Sort)) && !query.Sort.Equals(""))
            {
                switch (query.Sort.ToLower())
                {
                    case "netamount":
                        if (query.IsAsc)
                            orderBy = result.GetOrderBy(d => d.OrderBy(o => o.NetAmount).ThenByDescending(o => o.Id));
                        else
                            orderBy = result.GetOrderBy(d => d.OrderByDescending(o => o.NetAmount).ThenByDescending(o => o.Id));
                        break;
                    case "ordernumber":
                        if (query.IsAsc)
                            orderBy = result.GetOrderBy(d => d.OrderBy(o => o.OrderNumber).ThenByDescending(o => o.Id));
                        else
                            orderBy = result.GetOrderBy(d => d.OrderByDescending(o => o.OrderNumber).ThenByDescending(o => o.Id));
                        break;
                    case "lastconsumptiontime":
                        if (query.IsAsc)
                            orderBy = result.GetOrderBy(d => d.OrderBy(o => o.LastConsumptionTime).ThenByDescending(o => o.Id));
                        else
                            orderBy = result.GetOrderBy(d => d.OrderByDescending(o => o.LastConsumptionTime).ThenByDescending(o => o.Id));
                        break;
                }
            }

            #endregion

            int total = 0;
            result = result.GetPage(out total, query.PageNo, query.PageSize, orderBy);
            QueryPageModel<UserMemberInfo> pageModel = new QueryPageModel<UserMemberInfo>()
            {
                Models = result.ToList(),
                Total = total
            };

            return pageModel;
        }


        /// <summary>
        /// 获取会员分组数据
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="franchiseeId"></param>
        /// <returns></returns>
        public List<MemberGroupInfo> GetMemberGroup()
        {
            return Context.MemberGroupInfo.ToList();
        }


        /// <summary>
        /// 批量获取会员购买类别
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<MemberBuyCategoryInfo> GetMemberBuyCategoryByUserIds(IEnumerable<long> userIds)
        {
            var sqlData = (from m in Context.MemberBuyCategoryInfo
                           join c in Context.CategoryInfo on m.CategoryId equals c.Id
                           where userIds.Contains(m.UserId)
                           select new { m.CategoryId, m.Id, m.OrdersCount, m.UserId, c.Name }).OrderByDescending(o => o.OrdersCount).ThenByDescending(o => o.Id).ToList();

            var models = sqlData.Select(p => new MemberBuyCategoryInfo
            {
                CategoryId = p.CategoryId,
                UserId = p.UserId,
                CategoryName = p.Name,
                Id = p.Id,
                OrdersCount = p.OrdersCount
            }).ToList();

            return models;
        }


        /// <summary>
        /// 获取平台会员数
        /// </summary>
        /// <returns></returns>
        public int PlatformMemberTotal()
        {
            return Context.UserMemberInfo.Where(p => p.Disabled == false).Count();
        }

        /// <summary>
        /// 批量获取用户OPENID
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOpenIdByUserIds(IEnumerable<long> userIds)
        {
            var memOpenid = (from m in Context.MemberOpenIdInfo
                             join o in Context.OpenIdsInfo on m.OpenId equals o.OpenId
                             join u in Context.UserMemberInfo on m.UserId equals u.Id
                             where o.IsSubscribe
                             select new
                             {
                                 userid = m.UserId,
                                 openid = m.OpenId,
                                 regionid = u.TopRegionId,
                                 sex = u.Sex
                             });

            memOpenid = memOpenid.Where(e => userIds.Contains(e.userid));

            return memOpenid.Select(e => e.openid).Distinct().ToList();
        }

        /// <summary>
        /// 修改会员净消费金额
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="netAmount"></param>
        public void UpdateNetAmount(long userId, decimal netAmount)
        {
            var sql = "UPDATE Himall_Members SET NetAmount=NetAmount+@p1 WHERE Id=@p0";
            this.Context.Database.ExecuteSqlCommand(sql, userId, netAmount);
        }

        /// <summary>
        /// 增加会员下单量
        /// </summary>
        /// <param name="userId"></param>
        public void IncreaseMemberOrderNumber(long userId)
        {
            var sql = "UPDATE Himall_Members SET OrderNumber=OrderNumber+1 WHERE Id=@p0";
            this.Context.Database.ExecuteSqlCommand(sql, userId);
        }

        /// <summary>
        /// 减少会员下单量
        /// </summary>
        /// <param name="userId"></param>
        public void DecreaseMemberOrderNumber(long userId)
        {
            var sql = "UPDATE Himall_Members SET OrderNumber=OrderNumber-1 WHERE Id=@p0";
            this.Context.Database.ExecuteSqlCommand(sql, userId);
        }

        /// <summary>
        /// 修改最后消费时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastConsumptionTime">最后消费时间</param>
        public void UpdateLastConsumptionTime(long userId, DateTime lastConsumptionTime)
        {
            //this.Context.UserMemberInfo.Where(p => p.Id == userId).Update(p => new UserMemberInfo
            //{
            //	LastConsumptionTime = lastConsumptionTime
            //});
            var model = Context.UserMemberInfo.Where(p => p.Id == userId).FirstOrDefault();
            model.LastConsumptionTime = lastConsumptionTime;
            Context.SaveChanges();
        }
        #endregion

        #region 私有方法
        private void ChangePassword(UserMemberInfo model, string password)
        {
            if (model.PasswordSalt.StartsWith("o"))
            {
                model.PasswordSalt = Guid.NewGuid().ToString("N").Substring(12);   //微信一键注册用户初次改密需要重新生成掩值
            }
            model.Password = GetPasswrodWithTwiceEncode(password, model.PasswordSalt);

            var seller = Context.ManagerInfo.FirstOrDefault(a => a.UserName == model.UserName && a.ShopId != 0);
            if (seller != null)
            {
                seller.PasswordSalt = model.PasswordSalt;
                seller.Password = model.Password;
            }
            Context.SaveChanges();
            string CACHE_USER_KEY = CacheKeyCollection.Member(model.Id);
            Core.Cache.Remove(CACHE_USER_KEY);
            string CACHE_Seller_KEY = CacheKeyCollection.Seller(model.Id);
            Core.Cache.Remove(CACHE_Seller_KEY);
        }
        #endregion


        public void UpdateOpenIdBindMember(MemberOpenIdInfo model)
        {
            //更新绑定
            var memberOpenIdInfos = Context.MemberOpenIdInfo.Where(d => d.OpenId==model.OpenId  && d.ServiceProvider == model.ServiceProvider);
            //清理绑定
            //Context.MemberOpenIdInfo.Remove(d => d.UserId == model.UserId && d.OpenId == model.OpenId);
            if (memberOpenIdInfos.Count()>0)
            {
                Context.MemberOpenIdInfo.RemoveRange(memberOpenIdInfos);
            }
            Context.MemberOpenIdInfo.Add(model);
            Context.SaveChanges();
        }

        /// <summary>
        /// 根据用户id和平台获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public MemberOpenIdInfo GetMemberOpenIdInfoByuserIdAndType(long userId, string serviceProvider)
        {
            return this.Context.MemberOpenIdInfo.FirstOrDefault(p => p.UserId == userId && p.ServiceProvider == serviceProvider);
        }



        #region 用户佣金
        /// <summary>
        /// 用户当天获取的佣金总数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public decimal GetTodayCommission(long userId)
        {
            DateTime nowbegin = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
            DateTime nowend = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            return (decimal)Context.MemberCommissionInfo.Where(p => p.MemberId == userId && p.GetTime.Value >= nowbegin && p.GetTime.Value <= nowend).ToList().Sum(p=>p.InCome);
        }

        /// <summary>
        /// 用户总的佣金数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public decimal GetTotalCommission(long userId)
        {
            return (decimal)Context.MemberCommissionInfo.Where(p => p.MemberId == userId).ToList().Sum(p => p.InCome);
        }

        /// <summary>
        /// 获取佣金列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<MemberCommissionInfo> GetCommissionList(long userId)
        {
            return Context.MemberCommissionInfo.Where(p => p.MemberId == userId).ToList();
        }

        /// <summary>
        /// 添加一条佣金记录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int AddCommission(MemberCommissionInfo info)
        {
            Context.MemberCommissionInfo.Add(info);
            return Context.SaveChanges();
        }
        #endregion
    }
}


