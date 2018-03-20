using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WinServiceBase;

namespace WinOrderProductStatisticsService
{
    public partial class OrderProductStatisticsService : ServiceBase
    {
        Timer times;
        public OrderProductStatisticsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            times = new System.Timers.Timer();
            times.Interval = 60 * 1000;
            times.Elapsed += Times_Elapsed;
            times.Enabled = true;
        }

        private void Times_Elapsed(object sender, ElapsedEventArgs e)
        {
            string startTime = ConfigurationManager.AppSettings["StartTime"];
            if (DateTime.Now.ToString("HH:mm").Equals(startTime))
            {
                //执行后将下次执行时间设置成23小时后，每天只执行一次
                ((Timer)sender).Interval = 23 * 60 * 60 * 1000;

                var syncData = new ISyncData[] {
                    new StatisticOrderProduct()
                };
                foreach (var item in syncData)
                {
                    item.Execute();
                }
            }
            else
            {
                //60秒执行一次
                ((Timer)sender).Interval = 60 * 1000;
            }
        }

        protected override void OnStop()
        {
            times.Enabled = false;
        }
    }
}
