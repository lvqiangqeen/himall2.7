using Himall.IServices;
using Himall.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinOrderService
{
    public class OrderComment : ISyncData
    {
        public void SyncData()
        {
            AutoComment();
        }


        private void AutoComment()
        {
            try
            {
                var service2 = Instance<ICommentService>.Create;
                service2.AutoComment();//自动评论
               // Himall.Core.Log.Error("AutoComment运行成功");
            }
            catch (Exception ex)
            {
                Himall.Core.Log.Error("订单自动评价错误:"+ex);
            }
        }
    }
}
