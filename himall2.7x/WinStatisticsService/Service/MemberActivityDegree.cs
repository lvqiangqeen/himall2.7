using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinServiceBase;
using WinStatisticsService.Model;
using MongoDB;
using MongoDB.Driver;
using System.Configuration;

namespace WinStatisticsService.Service
{
    /// <summary>
    /// 会员每天订单数统计
    /// </summary>
    public class MemberActivityDegree : ISyncData
    {
        private DateTime lastTime = DateTime.Now;
        private DateTime yesterdayEnd = DateTime.Now;
        private DateTime threeMonthsTime = DateTime.Now;
        private DateTime sixMonthsTime = DateTime.Now;
        private int oneMonths;
        private int threeMonths;
        private int sixMonths;
        private DateTime oneMonthsTime = DateTime.Now;

        public MemberActivityDegree()
        {
            DateTime date = DateTime.Now;

            oneMonths = 1;
            threeMonths = 3;
            sixMonths = 6;

            lastTime = DateTime.Parse(ConfigurationManager.AppSettings["OperationTime"]);
            yesterdayEnd = date;
            threeMonthsTime = date.AddMonths(-threeMonths);
            sixMonthsTime = date.AddMonths(-sixMonths);
            oneMonthsTime = date.AddMonths(-oneMonths);
        }


        public void SyncData()
        {
            try
            {
                SetMemberActivity();
                UpdateMemberActivityDegreeByTiem();
            }
            catch (Exception ex)
            {
                WinServiceBase.Log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 更新会员订单统计表
        /// </summary>
        private void SetMemberActivity()
        {
            //每次获取10000条数据，分批执行
            var PAGE_SIZE = 10000;

            for (int i = 1; i <= int.MaxValue; i++)
            {
                var orders = GetYesterdayOrders(i, PAGE_SIZE);

  
                //重新计算活跃会员
                UpdateMemberActivityDegreeByOrder(orders);

                //如果此页没有数据终止循环
                if (orders.Count == 0)
                    break;
            }

            //修改维护时间
            UpdateAppSettings("OperationTime", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
        }

        /// <summary>
        /// 更新会员订单统计表(弃用的解决方法)
        /// </summary>
        /// <param name="orders"></param>
        private void UpdateMemberOrderStatistics(List<OrderUser> orders)
        {
            var days = DateTime.Now.DayOfYear % 180;
            //插入记录
            var coll = MongodbClient.Collection<MemberOrderStatisticsInfo>();
            List<MemberOrderStatisticsInfo> memberOrderStatisticss = new List<MemberOrderStatisticsInfo>();
            foreach (var item in orders)
            {
                //删除会员已有数据
                coll.DeleteMany(p => p.UserId == item.UserId && p.Days == days);

                //重新添加
                var memberOrderStatistics = new MemberOrderStatisticsInfo()
                {
                    OrderNumber = item.OrderNumber,
                    UserId = item.UserId
                };
                memberOrderStatistics.Days = days;
                memberOrderStatisticss.Add(memberOrderStatistics);
            }
            coll.InsertMany(memberOrderStatisticss);
        }

        #region 会员根据每天订单写入活跃用户数据
        /// <summary>
        /// 根据新订单更新活跃会员状态
        /// </summary>
        private void UpdateMemberActivityDegreeByOrder(List<OrderUser> orders)
        {
            int ONE_ORDERNUMBER = 1;
            //三个月活跃会员需要订单数
            int THREE_ORDERNUMBER = 2;
            //六个月活跃会员需要订单数
            int SIX_ORDERNUMBER = 3;
           

            foreach (var item in orders)
            {
                MemberActivityDegreeInfo memberActivityDegree = new MemberActivityDegreeInfo()
                {
                    OneMonth = false,
                    OneMonthEffectiveTime = DateTime.Now.AddHours(-12).AddMonths(oneMonths),
                    UserId = item.UserId
                };

                var oneMonthsOrders = GetOneMonthsOrders(item.UserId, ONE_ORDERNUMBER);
                if(oneMonthsOrders!=null&&oneMonthsOrders.Count==1)
                {
                    memberActivityDegree.OneMonth = true;
                    memberActivityDegree.ThreeMonthEffectiveTime = oneMonthsOrders.FirstOrDefault().PayDate.Value.AddMonths(oneMonths);
                }

                var threeMonthsOrders = GetThreeMonthsOrders(item.UserId, SIX_ORDERNUMBER);
                //三个月活跃会员激活
                if (threeMonthsOrders.Count >= THREE_ORDERNUMBER)
                {
                    memberActivityDegree.ThreeMonth = true;
                    memberActivityDegree.ThreeMonthEffectiveTime = threeMonthsOrders[THREE_ORDERNUMBER - 1].PayDate.Value.AddMonths(threeMonths);
                }
                else
                {
                    memberActivityDegree.ThreeMonth = false;
                }

                //六个月活跃会员
                if (threeMonthsOrders.Count == SIX_ORDERNUMBER)
                {
                    memberActivityDegree.SixMonth = true;
                    memberActivityDegree.SixMonthEffectiveTime = threeMonthsOrders[SIX_ORDERNUMBER - 1].PayDate.Value.AddMonths(sixMonths);
                }
                else
                {
                    var sixMonthsOrders = GetSixMonthsOrders(item.UserId, SIX_ORDERNUMBER);
                    if (sixMonthsOrders.Count == SIX_ORDERNUMBER)
                    {
                        memberActivityDegree.SixMonth = true;
                        memberActivityDegree.SixMonthEffectiveTime = sixMonthsOrders[SIX_ORDERNUMBER - 1].PayDate.Value.AddMonths(sixMonths);
                    }
                    else
                    {
                        memberActivityDegree.SixMonth = false;
                    }
                }
                DealWithMemberActivityDegree(memberActivityDegree);
            }
        }

        /// <summary>
        /// 维护会员活跃状态
        /// </summary>
        /// <param name="model"></param>
        private void DealWithMemberActivityDegree(MemberActivityDegreeInfo model)
        {
            var memberActivityDegree = GetMemberActivityDegreeByUserId(model.UserId);
            if (memberActivityDegree == null)
            {
                AddMemberActivityDegree(model);
            }
            else
            {
                UpdateMemberActivityDegree(model);
            }
        }

        /// <summary>
        /// 更新数据库活跃用户状态
        /// </summary>
        /// <param name="model"></param>
        private void UpdateMemberActivityDegree(MemberActivityDegreeInfo model)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "UPDATE Himall_MemberActivityDegree SET OneMonth=@OneMonth,ThreeMonth=@ThreeMonth,SixMonth=@SixMonth,OneMonthEffectiveTime=@OneMonthEffectiveTime,ThreeMonthEffectiveTime=@ThreeMonthEffectiveTime,SixMonthEffectiveTime=@SixMonthEffectiveTime where UserId=@UserId";
                conn.Execute(query, new { OneMonth = model.OneMonth, ThreeMonth = model.ThreeMonth, SixMonth = model.SixMonth, OneMonthEffectiveTime = model.OneMonthEffectiveTime, ThreeMonthEffectiveTime = model.ThreeMonthEffectiveTime, SixMonthEffectiveTime = model.SixMonthEffectiveTime, UserId = model.UserId });
            }
        }

        /// <summary>
        /// 获取会员活跃状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private MemberActivityDegreeInfo GetMemberActivityDegreeByUserId(long id)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT * FROM Himall_MemberActivityDegree where UserId=@UserId";
                return conn.Query<MemberActivityDegreeInfo>(query, new { UserId = id }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 新增数据库活跃用户状态
        /// </summary>
        /// <param name="model"></param>
        private void AddMemberActivityDegree(MemberActivityDegreeInfo model)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "INSERT INTO `Himall_MemberActivityDegree` (`UserId`, `OneMonth`, `ThreeMonth`, `SixMonth`, `OneMonthEffectiveTime`, `ThreeMonthEffectiveTime`, `SixMonthEffectiveTime`) VALUES ( @UserId, @OneMonth, @ThreeMonth, @SixMonth, @OneMonthEffectiveTime, @ThreeMonthEffectiveTime, @SixMonthEffectiveTime)";
                conn.Execute(query, new { OneMonth = model.OneMonth, ThreeMonth = model.ThreeMonth, SixMonth = model.SixMonth, OneMonthEffectiveTime = model.OneMonthEffectiveTime, ThreeMonthEffectiveTime = model.ThreeMonthEffectiveTime, SixMonthEffectiveTime = model.SixMonthEffectiveTime, UserId = model.UserId });
            }
        }
        #endregion

        #region 定时更新活跃用户状态
        /// <summary>
        /// 定时修改活跃会员状态过期
        /// </summary>
        private void UpdateMemberActivityDegreeByTiem()
        {
            SetOneMonthMemberActivityDegree();
            SetThreeMonthMemberActivityDegree();
            SetSixMonthMemberActivityDegree();
        }

        /// <summary>
        ///获取一个月过期的会员，并更新状态
        /// </summary>
        /// <returns></returns>
        private void SetOneMonthMemberActivityDegree()
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "UPDATE Himall_MemberActivityDegree SET OneMonth=FALSE where OneMonthEffectiveTime<@dateTime and OneMonth=true";
                conn.Execute(query, new { dateTime = DateTime.Now });
            }
        }

        /// <summary>
        ///获取三个月过期的会员，并更新状态
        /// </summary>
        /// <returns></returns>
        private void SetThreeMonthMemberActivityDegree()
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "UPDATE Himall_MemberActivityDegree SET ThreeMonth=FALSE where ThreeMonthEffectiveTime<@dateTime and ThreeMonth=true";
                var memberActivityDegree = conn.Execute(query, new { dateTime = DateTime.Now });
            }
        }

        /// <summary>
        ///获取六个月过期的会员，并更新状态
        /// </summary>
        /// <returns></returns>
        private void SetSixMonthMemberActivityDegree()
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "UPDATE Himall_MemberActivityDegree SET SixMonth=FALSE where SixMonthEffectiveTime<@dateTime and SixMonth=true";
                var memberActivityDegree = conn.Execute(query, new { dateTime = DateTime.Now });
            }
        }

        #endregion

        #region 获取订单数据
        /// <summary>
        ///分页获取前一天的订单基础信息
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private List<OrderUser> GetYesterdayOrders(int pageNo, int pageSize)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT UserId,count(Id) as OrderNumber FROM Himall_Orders where PayDate>@startPayDate and PayDate<=@endPayDate GROUP BY UserId order by id limit @startIndex,@pageSize";
                //无参数查询，返回列表，带参数查询和之前的参数赋值法相同。
                var orders = conn.Query<OrderUser>(query, new { startPayDate = lastTime, endPayDate = yesterdayEnd, startIndex = (pageNo - 1) * pageSize, pageSize = pageSize }).ToList();
                return orders;
            }
        }


        /// <summary>
        ///获取3个月内N条数据
        /// </summary>
        /// <param name="userId">会员ID</param>
        /// <param name="top">前多少条数据</param>
        /// <returns></returns>
        private List<NewOrderInfo> GetThreeMonthsOrders(long userId, int top)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT * FROM Himall_Orders where PayDate >@PayDate and UserId=@UserId order by PayDate desc  limit " + top;
                //无参数查询，返回列表，带参数查询和之前的参数赋值法相同。
                var orders = conn.Query<NewOrderInfo>(query, new { PayDate = threeMonthsTime, UserId = userId }).ToList();
                return orders;
            }
        }



        /// <summary>
        ///获取6个月内N条数据
        /// </summary>
        /// <param name="userId">会员ID</param>
        /// <param name="top">前多少条数据</param>
        /// <returns></returns>
        private List<NewOrderInfo> GetSixMonthsOrders(long userId, int top)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT * FROM Himall_Orders where PayDate >@PayDate and UserId=@UserId order by PayDate desc  limit " + top;
                //无参数查询，返回列表，带参数查询和之前的参数赋值法相同。
                var orders = conn.Query<NewOrderInfo>(query, new { PayDate = sixMonthsTime, UserId = userId }).ToList();
                return orders;
            }
        }



        /// <summary>
        ///获取1个月内N条数据
        /// </summary>
        /// <param name="userId">会员ID</param>
        /// <param name="top">前多少条数据</param>
        /// <returns></returns>
        private List<NewOrderInfo> GetOneMonthsOrders(long userId, int top)
        {
            using (var conn = MySqlHelper.OpenConnection())
            {
                string query = "SELECT * FROM Himall_Orders where PayDate >@PayDate and UserId=@UserId order by PayDate desc  limit " + top;
                //无参数查询，返回列表，带参数查询和之前的参数赋值法相同。
                var orders = conn.Query<NewOrderInfo>(query, new { PayDate = oneMonthsTime, UserId = userId }).ToList();
                return orders;
            }
        }


        ///// <summary>
        ///// 获取会员前个月前的订单数据
        ///// </summary>
        ///// <returns></returns>
        //private List<NewOrderInfo> GetBeforeThreeMonthsOrders(long userId, int top)
        //{
        //    var coll = MongodbClient.Collection<NewOrderInfo>();

        //    return coll.Find(p => p.UserId == userId && p.PayDate > sixMonthsTime).SortByDescending(p => p.PayDate).Limit(top).ToList();
        //}
        #endregion

        /// <summary>
        /// 修改配置文件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private void UpdateAppSettings(string key, string value)
        {
            var _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (_config.HasFile)
            {
                KeyValueConfigurationElement _key = _config.AppSettings.Settings[key];
                if (_key == null)
                    _config.AppSettings.Settings.Add(key, value);
                else
                    _config.AppSettings.Settings[key].Value = value;
                _config.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}
