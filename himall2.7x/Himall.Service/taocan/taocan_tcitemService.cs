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

		public List<taocan_tcmenu> GetTaoCanMenus()
		{
			return Context.taocan_tcmenu.Where(tc => tc.delete_flag == 0).ToList();
		}

		public List<taocan_tcitem> GetTaoCanItems(int menuID)
		{
			return Context.taocan_tcitem.Where(tc => tc.menu_id == menuID && tc.delete_flag == 0).ToList();
		}

		public taocao_tcinfo GetTaoCanInfo(int id)
		{
			return Context.taocao_tcinfo.FirstOrDefault(tc => tc.Id == id && tc.delete_flag == 0);
		}
	}
}
