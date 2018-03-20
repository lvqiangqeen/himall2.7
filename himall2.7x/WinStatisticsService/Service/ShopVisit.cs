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
    /// 门店
    /// </summary>
    public class ShopVisit : ISyncData
    {
       public void SyncData()
       {
           InitShopVisit();
       }


        private void InitShopVisit()
        {
            //获取所有店铺
            using (var conn = MySqlHelper.OpenConnection())
            {
                StringBuilder strSql = new StringBuilder("insert into himall_shopvistis(ShopId,Date,VistiCounts,OrderUserCount,OrderCount,OrderProductCount,OrderAmount,OrderPayUserCount,OrderPayCount,SaleCounts,SaleAmounts,StatisticFlag) ");
                strSql.AppendFormat("select Id,date(NOW()),0,0,0,0,0,0,0,0,0,0 from himall_shops where id not in(select ShopId from Himall_ShopVistis  where Date>='{0}' and Date<='{1}' group by ShopId)", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
                conn.Execute(strSql.ToString(), new { });
            }
        }

    }
}
