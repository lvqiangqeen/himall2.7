using Himall.IServices;
using Himall.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinOrderService
{
    public class FreightGroup : ISyncData
    {
        public void SyncData()
        {
            AutoFreightGroup();
        }

        private void AutoFreightGroup()
        {
            //团购自动关闭

            try
            {
                var service = Instance<IFightGroupService>.Create;
                service.AutoCloseGroup();
             //   Himall.Core.Log.Error("AutoFreightGroup运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("FightGroupJob : AutoCloseGroup 有异常", ex);
            }
        }

    }
}
