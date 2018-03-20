using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace Himall.Core.Helper
{
    /// <summary>
    /// ZJT 定时任务帮助类
    /// </summary>
    public class QuartzHelper
    {
        // Fields
        private static object oLock = new object();
        private static Dictionary<string , QuartzKey> quartzCache = new Dictionary<string , QuartzKey>();
        private const string QuartzHelperScheulerName = "QuartzHelperScheulerName";
        private static IScheduler sched = null;
        private static ISchedulerFactory sf = null;

        // Methods
        static QuartzHelper()
        {
            NameValueCollection props = new NameValueCollection();
            props.Add( "quartz.scheduler.instanceName" , "QuartzHelperScheulerName" );
            sf = new StdSchedulerFactory( props );
            sched = sf.GetScheduler();
        }

        public static void Close()
        {
            GetScheduler().Shutdown( true );
        }

        public static void Close( object jobKey )
        {
            if( jobKey is JobKey )
            {
                GetScheduler().DeleteJob( jobKey as JobKey );
            }
        }

        public static QuartzKey ExecuteAtDate( Action<Dictionary<string , object>> action , DateTime date , Dictionary<string , object> dataMap = null , string jobName = null )
        {
            return Start( action , delegate( TriggerBuilder p )
            {
                p.WithCronSchedule( BuilderCronExpression( date ) );
            } , dataMap , jobName );
        }

        public static QuartzKey ExecuteAtTime( Action<Dictionary<string , object>> action , string cronExpression , Dictionary<string , object> dataMap = null , string jobName = null )
        {
            return Start( action , delegate( TriggerBuilder p )
            {
                p.WithCronSchedule( cronExpression );
            } , dataMap , jobName );
        }

        public static void ExecuteInterval( Action<Dictionary<string , object>> action , TimeSpan interval , Dictionary<string , object> dataMap = null )
        {
            Action<TriggerBuilder> action2 = null;
            lock( oLock )
            {
                if( action2 == null )
                {
                    action2 = p => p.WithSimpleSchedule( p1 => p1.WithInterval( interval ) );
                }
                Start( action , action2 , dataMap , null );
            }
        }

        public static IScheduler GetScheduler()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            return factory.GetScheduler( "QuartzHelperScheulerName" );
        }

        public static bool IsStart()
        {
            return ( ( GetScheduler() != null ) && GetScheduler().IsStarted );
        }

        public static string BuilderCronExpression( DateTime date )
        {
            string cron = string.Empty;
            cron = string.Format( "{0} {1} {2} {3} {4} ?" , date.Second , date.Minute , date.Hour , date.Day , date.Month );
            return cron;
        }

        private static QuartzKey Start( Action<Dictionary<string , object>> action , Action<TriggerBuilder> action2 , Dictionary<string , object> dataMap , string jobName )
        {
            QuartzKey key = new QuartzKey();
            if( jobName != null )
            {
                if( quartzCache.ContainsKey( jobName ) )
                {
                    key = quartzCache[ jobName ];
                }
                else
                {
                    quartzCache.Add( jobName , key );
                }
            }
            jobName = jobName ?? Guid.NewGuid().ToString( "D" );
            string group = "group_" + jobName;
            string name = "trigger_" + jobName;
            IJobDetail jobDetail = JobBuilder.Create( typeof( QuartzJob ) ).WithIdentity( jobName , group ).Build();
            TriggerBuilder builder = TriggerBuilder.Create().WithIdentity( name , group );
            action2( builder );
            ITrigger trigger = builder.Build();
            if( quartzCache.ContainsKey( jobName ) )
            {
                quartzCache[ jobName ].JobKey = jobDetail.Key;
                quartzCache[ jobName ].TriggerKey = trigger.Key;
                quartzCache[ jobName ].Logs.Add( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " 调度任务已经启动。" );
            }
            jobDetail.JobDataMap.Add( "dataMap" , dataMap );
            jobDetail.JobDataMap.Add( "action" , action );
            jobDetail.JobDataMap.Add( "jobName" , jobName );
            jobDetail.JobDataMap.Add( "quartzCache" , quartzCache );
            sched.ScheduleJob( jobDetail , new Quartz.Collection.HashSet<ITrigger> { trigger } , true );
            sched.Start();
            return key;
        }
    }

    public class QuartzJob : IJob
    {
        // Methods
        public void Execute( IJobExecutionContext context )
        {
            Dictionary<string , object> dictionary = context.JobDetail.JobDataMap[ "dataMap" ] as Dictionary<string , object>;
            string key = context.JobDetail.JobDataMap[ "jobName" ] as string;
            Dictionary<string , QuartzKey> dictionary2 = context.JobDetail.JobDataMap[ "quartzCache" ] as Dictionary<string , QuartzKey>;
            try
            {
                ( context.JobDetail.JobDataMap[ "action" ] as Action<Dictionary<string , object>> )( dictionary );
                if( dictionary2.ContainsKey( key ) )
                {
                    dictionary2[ key ].Logs.Add( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " 任务执行成功。" );
                }
            }
            catch( Exception exception )
            {
                if( dictionary2.ContainsKey( key ) )
                {
                    dictionary2[ key ].Logs.Add( exception.Message );
                }
            }
        }
    }



    public class QuartzKey
    {
        // Methods
        public QuartzKey()
        {
            this.Logs = new List<string>();
        }

        // Properties
        public JobKey JobKey { get; set; }

        public List<string> Logs { get; set; }

        public TriggerKey TriggerKey { get; set; }
    }
}
