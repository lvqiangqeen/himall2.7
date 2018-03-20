using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinServiceBase;
using WinStatisticsService.Enum;
using WinStatisticsService.Model;

namespace WinStatisticsService.Service
{
    /// <summary>
    /// 会员分组统计
    /// </summary>
    public class MemberGroup : ISyncData
    {
        public void SyncData()
        {
            try
            {
                //平台
                SetMemberGroup(0);
            }
            catch (Exception ex)
            {
                WinServiceBase.Log.Error(ex.ToString());
            }
        }


        /// <summary>
        /// 更新设置会员分组数据
        /// </summary>
        /// <param name="shopId"></param>

        private void SetMemberGroup(long shopId)
        {
            MemberGroupInfo memberGroup = new MemberGroupInfo()
            {
                ShopId = shopId
            };
            #region 活跃会员
            //一个月活跃会员
            memberGroup.Total = StatisticsActiveMember(true, false, false, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.ActiveOne;
            DealWithMemberGroup(memberGroup);
            //三个月活跃会员
            memberGroup.Total = StatisticsActiveMember(false, true, false, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.ActiveThree;
            DealWithMemberGroup(memberGroup);
            //六个月活跃会员
            memberGroup.Total = StatisticsActiveMember(false, false, true, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.ActiveSix;
            DealWithMemberGroup(memberGroup);
            #endregion

            #region 沉睡会员
            //三个月沉睡会员
            memberGroup.Total = StatisticsSleepingMember(MemberStatisticsType.SleepingThree, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.SleepingThree;
            DealWithMemberGroup(memberGroup);

            //六个月沉睡会员
            memberGroup.Total = StatisticsSleepingMember(MemberStatisticsType.SleepingSix, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.SleepingSix;
            DealWithMemberGroup(memberGroup);

            //九个月沉睡会员
            memberGroup.Total = StatisticsSleepingMember(MemberStatisticsType.SleepingNine, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.SleepingNine;
            DealWithMemberGroup(memberGroup);

            //十二个月沉睡会员
            memberGroup.Total = StatisticsSleepingMember(MemberStatisticsType.SleepingTwelve, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.SleepingTwelve;
            DealWithMemberGroup(memberGroup);

            //二十四个月沉睡会员
            memberGroup.Total = StatisticsSleepingMember(MemberStatisticsType.SleepingTwentyFour, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.SleepingTwentyFour;
            DealWithMemberGroup(memberGroup);
            #endregion

            #region 生日会员
            //今日生日会员
            memberGroup.Total = StatisticsBirthdayMember(MemberStatisticsType.BirthdayToday, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.BirthdayToday;
            DealWithMemberGroup(memberGroup);

            //今月生日会员
            memberGroup.Total = StatisticsBirthdayMember(MemberStatisticsType.BirthdayToMonth, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.BirthdayToMonth;
            DealWithMemberGroup(memberGroup);

            //下月生日会员
            memberGroup.Total = StatisticsBirthdayMember(MemberStatisticsType.BirthdayNextMonth, shopId);
            memberGroup.StatisticsType = MemberStatisticsType.BirthdayNextMonth;
            DealWithMemberGroup(memberGroup);
            #endregion

            #region 注册会员
            memberGroup.Total = StatisticsRegisteredMember(shopId);
            memberGroup.StatisticsType = MemberStatisticsType.RegisteredMember;
            DealWithMemberGroup(memberGroup);
            #endregion
        }

        /// <summary>
        /// 处理会员分组数据
        /// </summary>
        /// <param name="model"></param>
        private void DealWithMemberGroup(MemberGroupInfo model)
        {
            var memberGroup = GetMemberGroup(model);
            if (memberGroup == null)
                AddMemberGroup(model);
            else
            {
                model.Id = memberGroup.Id;
                UpdateMemberGroup(model);
            }
        }

        #region 数据库操作
        /// <summary>
        /// 获取所有门店ID
        /// </summary>
        /// <returns></returns>
        private List<long> GetShopIds()
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT Id FROM Himall_Shops";
                return conn.Query<long>(query, new { }).ToList();
            }
        }


        /// <summary>
        /// 活跃用户统计
        /// </summary>
        /// <param name="OneMonth"></param>
        /// <param name="ThreeMonth"></param>
        /// <param name="SixMonth"></param>
        /// <param name="ShopId"></param>

        /// <returns></returns>
        private int StatisticsActiveMember(bool OneMonth, bool ThreeMonth, bool SixMonth, long ShopId)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT count(b.id) as total FROM Himall_MemberActivityDegree as a join Himall_Members as b on a.userId=b.id where b.Disabled=false";
                if (ShopId > 0)
                {
                    query += " and b.ShopId=" + ShopId;
                }
                if (OneMonth)
                {
                    query += " and OneMonth=true";
                }
                if (ThreeMonth)
                {
                    query += " and OneMonth=false and ThreeMonth=true";
                }
                if (SixMonth)
                {
                    query += " and OneMonth=false and ThreeMonth=false and SixMonth=true";
                }
                return conn.Query<int>(query, new { }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 沉睡会员统计
        /// </summary>
        /// <param name="statisticsType"></param>
        /// <param name="ShopId"></param>

        /// <returns></returns>
        private int StatisticsSleepingMember(MemberStatisticsType statisticsType, long ShopId)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                string query = "SELECT count(id) as total FROM Himall_Members where Disabled=false ";

                if (ShopId > 0)
                {
                    query += " and ShopId=" + ShopId;
                }

                switch (statisticsType)
                {
                    case MemberStatisticsType.SleepingThree:
                        startDate = DateTime.Now.AddMonths(-6);
                        endDate = DateTime.Now.AddMonths(-3);
                        query += string.Format(" and LastConsumptionTime>'{0}' and LastConsumptionTime<'{1}'", startDate, endDate);
                        break;
                    case MemberStatisticsType.SleepingSix:
                        startDate = DateTime.Now.AddMonths(-9);
                        endDate = DateTime.Now.AddMonths(-6);
                        query += string.Format(" and LastConsumptionTime>'{0}' and LastConsumptionTime<'{1}'", startDate, endDate);
                        break;
                    case MemberStatisticsType.SleepingNine:
                        startDate = DateTime.Now.AddMonths(-12);
                        endDate = DateTime.Now.AddMonths(-9);
                        query += string.Format(" and LastConsumptionTime>'{0}' and LastConsumptionTime<'{1}'", startDate, endDate);
                        break;
                    case MemberStatisticsType.SleepingTwelve:
                        startDate = DateTime.Now.AddMonths(-24);
                        endDate = DateTime.Now.AddMonths(-12);
                        query += string.Format(" and LastConsumptionTime>'{0}' and LastConsumptionTime<'{1}'", startDate, endDate);
                        break;
                    case MemberStatisticsType.SleepingTwentyFour:
                        endDate = DateTime.Now.AddMonths(-24);
                        query += string.Format(" and (LastConsumptionTime<'{0}')", endDate);
                        break;
                }
                return conn.Query<int>(query, new { }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 生日会员
        /// </summary>
        /// <param name="statisticsType"></param>
        /// <param name="ShopId"></param>

        /// <returns></returns>
        private int StatisticsBirthdayMember(MemberStatisticsType statisticsType, long ShopId)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                string query = "SELECT count(id) as total FROM Himall_Members where Disabled=false ";

                if (ShopId > 0)
                {
                    query += " and ShopId=" + ShopId;
                }
                switch (statisticsType)
                {
                    case MemberStatisticsType.BirthdayToday:
                        query += string.Format(" and MONTH(BirthDay)='{0}' and DAY(BirthDay)='{1}'", DateTime.Now.Month, DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayToMonth:
                        query += string.Format(" and MONTH(BirthDay)='{0}' and DAY(BirthDay)<>'{1}'", DateTime.Now.Month, DateTime.Now.Day);
                        break;
                    case MemberStatisticsType.BirthdayNextMonth:
                        startDate = DateTime.Now.AddMonths(1);
                        query += string.Format(" and MONTH(BirthDay)='{0}'", startDate.Month);
                        break;
                }
                return conn.Query<int>(query, new { }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 注册会员
        /// </summary>
        /// <param name="ShopId"></param>

        /// <returns></returns>
        private int StatisticsRegisteredMember(long ShopId)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                string query = "SELECT count(id) as total FROM Himall_Members where Disabled=false and OrderNumber=0";

                if (ShopId > 0)
                {
                    query += " and ShopId=" + ShopId;
                }
                return conn.Query<int>(query, new { }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取会员分组数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private MemberGroupInfo GetMemberGroup(MemberGroupInfo model)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT * FROM Himall_MemberGroup where ShopId =@ShopId and StatisticsType=@StatisticsType";
                return conn.Query<MemberGroupInfo>(query, new { ShopId = model.ShopId, StatisticsType = (int)model.StatisticsType }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 更新会员分组数据
        /// </summary>
        /// <param name="model"></param>
        private void UpdateMemberGroup(MemberGroupInfo model)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "UPDATE Himall_MemberGroup set Total=@Total WHERE Id=@Id";
                conn.Execute(query, new { Total = model.Total, Id = model.Id });
            }
        }

        /// <summary>
        /// 新增会员分组数据
        /// </summary>
        /// <param name="model"></param>
        private void AddMemberGroup(MemberGroupInfo model)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "INSERT INTO `Himall_MemberGroup` (`ShopId`, `StatisticsType`, `Total`) VALUES ( @ShopId, @StatisticsType, @Total)";
                conn.Execute(query, new { ShopId = model.ShopId, StatisticsType = (int)model.StatisticsType, Total = model.Total });
            }
        }

        #endregion

    }
}
