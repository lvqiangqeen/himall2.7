using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
	 public class TaoCanMenu
	{
		public int ID { get; set; }
		public string Title { get; set; }
		public int ParentID { get; set; }
		public string LinkUrl { get; set; }
	}
}
