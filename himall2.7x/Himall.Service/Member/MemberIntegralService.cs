using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class MemberIntegralService : ServiceBase, IMemberIntegralService
    {
        public void AddMemberIntegral(MemberIntegralRecord model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            if (null == model) { throw new NullReferenceException("添加会员积分记录时，会员积分Model为空."); }
            if (0 == model.MemberId) { throw new NullReferenceException("添加会员积分记录时，会员Id为空."); }
            if (!Context.UserMemberInfo.Any(a => a.Id == model.MemberId && a.UserName == model.UserName))
            {
                throw new Himall.Core.HimallException("不存在此会员");
            }
            if (null != conversionMemberIntegralEntity)
            {
                model.Integral = conversionMemberIntegralEntity.ConversionIntegral();
            }
            if (model.Integral == 0)
            {
                return;
            }
            var userIntegral = Context.MemberIntegral.FirstOrDefault(a => a.MemberId == model.MemberId);
            if (userIntegral == null)
            {
                userIntegral = new MemberIntegral();
                userIntegral.MemberId = model.MemberId;
                userIntegral.UserName = model.UserName;
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    throw new Himall.Core.HimallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                Context.MemberIntegral.Add(userIntegral);
            }
            else
            {
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    if (userIntegral.AvailableIntegrals < Math.Abs(model.Integral))
                        throw new Himall.Core.HimallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
            }
            Context.MemberIntegralRecord.Add(model);
            Context.SaveChanges();
        }

        public void AddMemberIntegralByRecordAction(MemberIntegralRecord model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            if (null == model) { throw new NullReferenceException("添加会员积分记录时，会员积分Model为空."); }
            if (0 == model.MemberId) { throw new NullReferenceException("添加会员积分记录时，会员Id为空."); }
            if (!Context.UserMemberInfo.Any(a => a.Id == model.MemberId && a.UserName == model.UserName))
            {
                throw new Himall.Core.HimallException("不存在此会员");
            }
            if (null != conversionMemberIntegralEntity)
            {
                var conversionIntegral = conversionMemberIntegralEntity.ConversionIntegral();
                if (model.Himall_MemberIntegralRecordAction != null && model.Himall_MemberIntegralRecordAction.Count > 0)
                {//多个明细记录时，每个记录都需计算
                    model.Integral = conversionIntegral * model.Himall_MemberIntegralRecordAction.Count;
                }
                else
                {
                    model.Integral = conversionIntegral;
                }
            }
            if (model.Integral == 0)
            {
                return;
            }
            var userIntegral = Context.MemberIntegral.FirstOrDefault(a => a.MemberId == model.MemberId);
            if (userIntegral == null)
            {
                userIntegral = new MemberIntegral();
                userIntegral.MemberId = model.MemberId;
                userIntegral.UserName = model.UserName;
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    throw new Himall.Core.HimallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
                Context.MemberIntegral.Add(userIntegral);
            }
            else
            {
                if (model.Integral > 0)
                {
                    userIntegral.HistoryIntegrals += model.Integral;
                }
                else
                {
                    if (userIntegral.AvailableIntegrals < Math.Abs(model.Integral))
                        throw new Himall.Core.HimallException("用户积分不足以扣减该积分！");
                }
                userIntegral.AvailableIntegrals += model.Integral;
            }
            Context.MemberIntegralRecord.Add(model);
            Context.SaveChanges();
        }

        public ObsoletePageModel<MemberIntegral> GetMemberIntegralList(IServices.QueryModel.IntegralQuery query)
        {
            int total = 0;
            IQueryable<MemberIntegral> list = Context.MemberIntegral.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                list = list.Where(item => item.UserName.Equals(query.UserName));
            }
            if (query.StartDate.HasValue)
            {
                
                list = list.Where(item => item.Himall_Members.CreateDate>=query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                var end = query.EndDate.Value.Date.AddDays(1);
                list = list.Where(item =>  item.Himall_Members.CreateDate<end);
            }
            list = list.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(item => item.HistoryIntegrals));
            ObsoletePageModel<MemberIntegral> pageModel = new ObsoletePageModel<MemberIntegral>() { Models = list, Total = total };
            return pageModel;
        }

        public ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordList(IServices.QueryModel.IntegralRecordQuery query)
        {
            int total = 0;
            IQueryable<MemberIntegralRecord> list = Context.MemberIntegralRecord.AsQueryable();
            if (query.UserId.HasValue)
            {
                list = list.Where(item => item.MemberId == query.UserId.Value);
            }
            if (query.StartDate.HasValue)
            {
                list = list.Where(item => query.StartDate <= item.RecordDate);
            }
            if (query.IntegralType.HasValue)
            {
                list = list.Where(item => item.TypeId == query.IntegralType.Value);
            }
            if (query.EndDate.HasValue)
            {
                list = list.Where(item => query.EndDate >= item.RecordDate);
            }
            list = list.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<MemberIntegralRecord> pageModel = new ObsoletePageModel<MemberIntegralRecord>() { Models = list, Total = total };
            return pageModel;
        }

        public ObsoletePageModel<MemberIntegralRecord> GetIntegralRecordListForWeb(IServices.QueryModel.IntegralRecordQuery query)
        {
            int total = 0;
            IQueryable<MemberIntegralRecord> list = Context.MemberIntegralRecord.AsQueryable();
            if (query.UserId.HasValue)
            {
                list = list.Where(item => item.MemberId == query.UserId.Value);
            }
            if (query.StartDate.HasValue)
            {
                list = list.Where(item => query.StartDate <= item.RecordDate);
            }
            if (query.IntegralType.HasValue)
            {
                if ((int)query.IntegralType.Value == 0)
                {
                    list = list.Where(item => true);
                }
                else if ((int)query.IntegralType.Value == 1)
                {
                    list = list.Where(item => item.Integral > 0);
                }
                else if ((int)query.IntegralType.Value == 2)
                {
                    list = list.Where(item => item.Integral <= 0);
                }
            }
            if (query.EndDate.HasValue)
            {
                list = list.Where(item => query.EndDate >= item.RecordDate);
            }
            list = list.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<MemberIntegralRecord> pageModel = new ObsoletePageModel<MemberIntegralRecord>() { Models = list, Total = total };
            return pageModel;
        }

        public void SetIntegralRule(IEnumerable<MemberIntegralRule> info)
        {
            var list = Context.MemberIntegralRule.ToList();
            foreach (var s in info)
            {
                var t = list.FirstOrDefault(a => a.TypeId == s.TypeId);
                if (t != null)
                {
                    t.Integral = s.Integral;
                }
                else
                {
                    Context.MemberIntegralRule.Add(s);
                }
            }
            Context.SaveChanges();
        }

        public void SetIntegralChangeRule(MemberIntegralExchangeRules info)
        {
            var model = Context.MemberIntegralExchangeRules.FirstOrDefault();
            if (model != null)
            {
                model.IntegralPerMoney = info.IntegralPerMoney;
                model.MoneyPerIntegral = info.MoneyPerIntegral;
            }
            else
            {
                model = new MemberIntegralExchangeRules();
                model.IntegralPerMoney = info.IntegralPerMoney;
                model.MoneyPerIntegral = info.MoneyPerIntegral;
                Context.MemberIntegralExchangeRules.Add(model);
            }
            Context.SaveChanges();
        }

        public MemberIntegralExchangeRules GetIntegralChangeRule()
        {
            return Context.MemberIntegralExchangeRules.FirstOrDefault();
        }

        public IEnumerable<MemberIntegralRule> GetIntegralRule()
        {
            return Context.MemberIntegralRule.ToList();
        }

        public bool HasLoginIntegralRecord(long userId)
        {
            var Date = DateTime.Now.Date;
            var Date2 = Date.AddDays(1);
            return Context.MemberIntegralRecord.Any(a => a.MemberId == userId && a.RecordDate.Value >= Date && a.RecordDate.Value < Date2 && a.TypeId == MemberIntegral.IntegralType.Login);
        }

        public UserIntegralGroupModel GetUserHistroyIntegralGroup(long userId)
        {
            UserIntegralGroupModel model = new UserIntegralGroupModel();
            model.BindWxIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.BindWX).Sum(a => (int?)a.Integral).GetValueOrDefault(); ;
            model.CommentIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.Comment).Sum(a => (int?)a.Integral).GetValueOrDefault();
            model.ConsumptionIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.Consumption).Sum(a => (int?)a.Integral).GetValueOrDefault();
            model.LoginIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.Login).Sum(a => (int?)a.Integral).GetValueOrDefault();
            model.RegIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.Reg).Sum(a => (int?)a.Integral).GetValueOrDefault();
            model.SignIn = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.SignIn).Sum(a => (int?)a.Integral).GetValueOrDefault();
            model.InviteIntegral = Context.MemberIntegralRecord.Where(a => a.MemberId == userId && a.TypeId == Himall.Model.MemberIntegral.IntegralType.InvitationMemberRegiste).Sum(a => (int?)a.Integral).GetValueOrDefault();
            return model;
        }

        public MemberIntegral GetMemberIntegral(long userId)
        {
            var model = Context.MemberIntegral.FirstOrDefault(a => a.MemberId == userId);
            if (model == null)
            {
                model = new MemberIntegral();
            }
            return model;
        }

        public List<MemberIntegral> GetMemberIntegrals(IEnumerable<long> userIds)
        {
            var model = Context.MemberIntegral.Where(a => userIds.Contains(a.MemberId.Value)).ToList();
            return model;
        }

        public  List<MemberIntegralRecordAction> GetIntegralRecordAction(IEnumerable<long> virtualItemIds, MemberIntegral.VirtualItemType type)
        {
            return Context.MemberIntegralRecordAction.Where(e => virtualItemIds.Contains(e.VirtualItemId) && e.VirtualItemTypeId == type).ToList();
        }

        
    }
}