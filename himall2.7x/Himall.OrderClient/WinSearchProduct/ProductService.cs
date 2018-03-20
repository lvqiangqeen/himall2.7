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

namespace WinSearchProduct
{
    public partial class ProductService : ServiceBase
    {
        System.Timers.Timer orderTimer;  //计时器

        IProductIndex productIndex; //接口列表 

        /// 索引创建间隔时间
        /// </summary>
        int hours = 0;
        public ProductService()
        {
            var str = ConfigurationManager.AppSettings["Interval"];
            productIndex = new SearchIndex();
            hours = int.Parse(str);
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
             EmptyProductIndex();
             CreateProductIndex();
            orderTimer = new System.Timers.Timer();
            orderTimer.Interval = hours * 1000 * 60 * 60;  //设置计时器事件间隔执行时间
            orderTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            orderTimer.Enabled = true;

        }

        protected override void OnStop()
        {
            this.orderTimer.Enabled = false;
        }
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CreateProductIndex();
        }

        private void CreateProductIndex()
        {
            productIndex.CreateIndex();
        }

        private void EmptyProductIndex()
        {
            productIndex.EmptyIndex();
        }
    }
}
