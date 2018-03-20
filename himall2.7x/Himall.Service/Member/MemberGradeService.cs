using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Transactions;

namespace Himall.Service
{
    public class MemberGradeService : ServiceBase, IMemberGradeService
    {
        public void AddMemberGrade(MemberGrade model)
        {
            if (model.Integral < 0)
                throw new HimallException("积分不能为负数");
            if (Context.MemberGrade.Any(a => a.Integral == model.Integral))
            {
                throw new HimallException("等级之间的积分不能相同");
            }
            if (Context.MemberGrade.Any(a => a.GradeName.ToLower()==model.GradeName.ToLower()))
            {
                throw new HimallException("已存在相同名称的等级");
            }
            Context.MemberGrade.Add(model);
            Context.SaveChanges();
        }

        public void UpdateMemberGrade(MemberGrade model)
        {
            if (Context.MemberGrade.Any(a => a.Integral == model.Integral && a.Id != model.Id))
            {
                throw new HimallException("等级之间的积分不能相同");
            }
            if (Context.MemberGrade.Any(a =>a.Id != model.Id&&a.GradeName.ToLower() == model.GradeName.ToLower()))
            {
                throw new HimallException("已存在相同名称的等级");
            }
            Context.Set<MemberGrade>().Attach(model);
            Context.Entry<MemberGrade>(model).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void DeleteMemberGrade(long id)
        {
            if(Context.GiftInfo.Any(d=>d.NeedGrade==id))
            {
                throw new HimallException("有礼品兑换与等级关联，不可删除！");
            }
            Context.MemberGrade.Remove(a => a.Id == id);
            Context.SaveChanges();
        }


        public MemberGrade GetMemberGrade(long id)
        {
            var model = Context.MemberGrade.Find(id);
            return model;
        }


        public IEnumerable<MemberGrade> GetMemberGradeList()
        {
            List<MemberGrade> result= Context.MemberGrade.ToList();
            //补充积分礼品信息
            var gifts = Context.GiftInfo.GroupBy(d => d.NeedGrade).Select(d => new { cnt = d.Count(), grid = d.Key }).ToList();
            foreach (var item in result)
            {
                if (gifts.Any(d => d.grid == item.Id && d.cnt > 0))
                {
                    item.IsNoDelete = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 通过用户编号获取用户等级编号
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public long GetMemberGradeByUserId(long userId)
        {
            var HistoryIntegrals = 0;
            var uiobj = Context.MemberIntegral.FirstOrDefault(d=>d.MemberId== userId);
            if (uiobj != null)
            {
                HistoryIntegrals = uiobj.HistoryIntegrals;
            }
            long memberGrade = 0;
            //var mgobj = Context.MemberGrade.OrderByDescending(d => d.Integral).FirstOrDefault(d => d.Integral >= HistoryIntegrals);zyken
            var mgobj =Context.MemberGrade.Where(a => a.Integral <= HistoryIntegrals).OrderByDescending(a => a.Integral).FirstOrDefault();
            if (mgobj!=null)
            {
                memberGrade = mgobj.Id;
            }

            return memberGrade;
        } 
        /// <summary>
        /// 前一个等级是否大于后一个等级
        /// </summary>
        /// <param name="oneId"></param>
        /// <param name="twoId"></param>
        /// <returns></returns>
        public bool IsOneGreaterOrEqualTwo(long oneId,long twoId)
        {
            bool result = false;
            if(oneId==twoId)
            {
                return true;
            }
            if (twoId > 0)
            {
                if(oneId==0)
                {
                    return false;
                }
                var one = GetMemberGrade(oneId);
                var two = GetMemberGrade(twoId);
                if(one==null)
                {
                    return false;
                }
                if (two != null)
                {
                    if (one.Integral > two.Integral)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
