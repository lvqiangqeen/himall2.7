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

namespace WinShopServices
{
    public partial class ShopService : ServiceBase
    { /// <summary>
        /// 定时器
        /// </summary>
        Timer times;

        public ShopService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            times = new System.Timers.Timer();
            //执行时间设置成23小时循环
            times.Interval = 23 * 60 * 60 * 1000;
            times.Elapsed += Times_Elapsed;
            times.Enabled = true;
        }

        private void Times_Elapsed(object sender, ElapsedEventArgs e)
        {
            var syncData = new ISyncData[] {
                 new Shop()
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
