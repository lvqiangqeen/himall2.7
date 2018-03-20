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

namespace WinOrderCommentsService
{
    public partial class OrderCommentService : ServiceBase
    {
        Timer times;
        public OrderCommentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            times = new Timer();
            times.Interval = 1000;
            times.Elapsed += Times_Elapsed;
            times.Enabled = true;
        }

        private void Times_Elapsed(object sender, ElapsedEventArgs e)
        {
            var syncData = new ISyncData[] {
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
