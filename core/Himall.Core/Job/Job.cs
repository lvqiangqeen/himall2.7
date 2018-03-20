using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Xml;

namespace Himall.Core
{
    /// <summary>
    /// 自动化任务
    /// </summary>
    public static class Job
    {
        static Job()
        {
            XMLSchedulingDataProcessor processor = new XMLSchedulingDataProcessor(new SimpleTypeLoadHelper());
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = schedulerFactory.GetScheduler();
            processor.ProcessFileAndScheduleJobs(Core.Helper.IOHelper.GetMapPath("/quartz_jobs.xml"), scheduler);
            scheduler.Start();  
        }

        /// <summary>
        /// 开启自动化任务
        /// </summary>
        public static void Start()
        {
            //本方法不做任务实质性操作，仅仅为了触发构造函数
        }

    }
}
