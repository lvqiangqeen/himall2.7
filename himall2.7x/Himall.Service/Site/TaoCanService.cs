using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Model;

namespace Himall.Service
{
	public class TaoCanService : ServiceBase
	{
		public List<TaoCanMenu> GetTaoCanMenu() {
			List<TaoCanMenu>list= Context.Database.SqlQuery<TaoCanMenu>("select id ID, title Title, parent_id ParentID, link_url LinkUrl from himall_tcmenu where delete_flag=0").ToList();
			return list;
		}
	}
}
