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
using System.Threading.Tasks;
using System.Transactions;

namespace Himall.Service
{
    /// <summary>
    /// 签到服务
    /// </summary>
    public class MemberSignInService : ServiceBase, IMemberSignInService
    {
        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns>已持续签到天数,0表示签到不成功</returns>
        public int SignIn(long memberId)
        {
            int result = 0;
            SiteSignInConfigInfo config = GetConfig();
            if (CanSignInByToday(memberId))
            {
                MemberSignInInfo signinfo = GetSignInInfo(memberId);
                DateTime _oldlastsigntime = signinfo.LastSignTime.Value;
                signinfo.LastSignTime = DateTime.Now;
                signinfo.SignDaySum += 1;
                //处理积分
                int needAddIntegral = config.DayIntegral;  //需要增加的各分
                bool isReward = false;

                //连续登录
                if (_oldlastsigntime.Date == (DateTime.Now.AddDays(-1).Date))
                {
                    signinfo.DurationDaySum += 1;

                    if (config.DurationCycle > 0)
                    {
                        signinfo.DurationDay += 1;
                        if (signinfo.DurationDay > config.DurationCycle)
                        {
                            signinfo.DurationDay = 1;
                        }
                        //处理额外积分
                        if (signinfo.DurationDay == config.DurationCycle)
                        {
                            needAddIntegral += config.DurationReward;
                            isReward = true;
                        }
                    }
                    else
                    {
                        signinfo.DurationDay = 1;
                    }
                }
                else
                {
                    signinfo.DurationDay = 1;
                    signinfo.DurationDaySum = 1;
                }
                //积分到户
                AddIntegralToUser(memberId, needAddIntegral, isReward);
                result = signinfo.DurationDay;
                Context.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// 积分抵扣
        /// </summary>
        /// <param name="member"></param>
        /// <param name="Id"></param>
        /// <param name="integral"></param>
        private void AddIntegralToUser(long memberId, int integral, bool isReward)
        {
            if (integral == 0)
            {
                return;
            }
            var member = Context.UserMemberInfo.FirstOrDefault(d => d.Id == memberId);
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            var remark = "签到奖励";
            if (isReward)
            {
                remark += "(含额外奖励)";
            }
            record.TypeId = MemberIntegral.IntegralType.SignIn;
            record.ReMark = remark;
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.SignIn, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }
        /// <summary>
        /// 今天是否可以签到
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public bool CanSignInByToday(long memberId)
        {
            bool result = false;
            SiteSignInConfigInfo config = GetConfig();
            if (config.IsEnable)
            {
                result = true;
                MemberSignInInfo signinfo = GetSignInInfo(memberId);
                if (signinfo.LastSignTime.HasValue)
                {
                    if (signinfo.LastSignTime.Value.Date == DateTime.Now.Date)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取签到信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public MemberSignInInfo GetSignInInfo(long memberId)
        {
            MemberSignInInfo result = null;
            result = Context.MemberSignInInfo.FirstOrDefault(d => d.UserId == memberId);
            if (result == null)
            {
                result = new MemberSignInInfo();
                result.UserId = memberId;
                result.LastSignTime = DateTime.Now.AddYears(-1);
                result.DurationDay = 0;
                result.DurationDaySum = 0;
                result.SignDaySum = 0;
                Context.MemberSignInInfo.Add(result);
                Context.SaveChanges();
            }
            return result;
        }
        /// <summary>
        /// 取得签到配置
        /// </summary>
        /// <returns></returns>
        public SiteSignInConfigInfo GetConfig()
        {
            SiteSignInConfigInfo result = null;
            result = Context.SiteSignInConfigInfo.FirstOrDefault();
            if (result == null)
            {
                result = new SiteSignInConfigInfo();
                result.IsEnable = false;
                result.DayIntegral = 0;
                result.DurationCycle = 0;
                result.DurationReward = 0;
                Context.SiteSignInConfigInfo.Add(result);
                Context.SaveChanges();
            }
            if (result.DayIntegral < 1)
            {
                result.IsEnable = false;
            }
            return result;
        }
        /// <summary>
        /// 保存签到配置
        /// </summary>
        /// <param name="config"></param>
        public void SaveConfig(SiteSignInConfigInfo config)
        {
            SiteSignInConfigInfo data = GetConfig();
            data.IsEnable = config.IsEnable;
            data.DayIntegral = config.DayIntegral;
            data.DurationCycle = config.DurationCycle;
            data.DurationReward = config.DurationReward;
            Context.SaveChanges();
        }
    }
}


