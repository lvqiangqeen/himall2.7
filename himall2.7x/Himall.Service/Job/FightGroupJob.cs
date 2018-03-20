using Himall.IServices;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Himall.Service.Job
{
    public class FightGroupJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //时间间隔请在小时内
            Himall.Core.Log.Debug("FightGroupJob : checkDate=" + DateTime.Now);
            var service = ServiceProvider.Instance<IFightGroupService>.Create;

            try
            {
                service.AutoOpenGroup();
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Debug("FightGroupJob : AutoCloseGroup 有异常", ex);
            }
        }
    }
}
