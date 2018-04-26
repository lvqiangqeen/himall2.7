using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class taocan_tcitemService : ServiceBase
    {
        public taocan_tcitem Gettaocan_tcitem(long id)
        {
            return Context.taocan_tcitem.FindById(id);
        }
    }
}
