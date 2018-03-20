using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WinBrokerageService
{
    public partial class BrokerageService : ServiceBase
    {
        /// <summary>
        /// 定时器
        /// </summary>
        Timer times;

        public BrokerageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            times = new System.Timers.Timer();
            //执行时间设置成半小时循环
            times.Interval = 0.5 * 60 * 60 * 1000;
         //   times.Interval =60 * 1000;
            times.Elapsed += Times_Elapsed;
            times.Enabled = true;
        }

        private void Times_Elapsed(object sender, ElapsedEventArgs e)
        {
            var syncData = new ISyncData[] {
                 new BrokerageJob()
                };
            foreach (var item in syncData)
            {
                item.SyncData();
            }
        }

        protected override void OnStop()
        {
            this.times.Enabled = false;
        }
    }
}
