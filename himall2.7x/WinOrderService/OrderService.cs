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

namespace WinOrderService
{
    partial class OrderService : System.ServiceProcess.ServiceBase
    { /// <summary>
      /// 定时器
      /// </summary>
        Timer times;
        public OrderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            times = new System.Timers.Timer();
            //执行时间设置成0.5小时循环
            times.Interval = 30 * 60 * 1000;
        //    times.Interval = 60 * 1000;
            times.Elapsed += new ElapsedEventHandler(Times_Elapsed);
          //  times.Elapsed += Times_Elapsed;
            times.Enabled = true;
        }


        private void Times_Elapsed(object sender, ElapsedEventArgs e)
        {
            var syncData = new ISyncData[] {
                    new OrderJob(),
                    new GiftOrder(),
                    new OrderRefund(),
                    new FreightGroup(),
                    new OrderComment()
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
