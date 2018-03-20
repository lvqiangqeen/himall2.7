using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class  QueryPageModel<T>
    {
        /// <summary>
        /// 返回的实体
        /// </summary>
        public List<T> Models{set;get;}

        /// <summary>
        /// 返回的记录总数
        /// </summary>
        public int Total { set; get; }
    }

	public class QueryPageModel<T, TData>
	{
		/// <summary>
		/// 返回的实体
		/// </summary>
		public List<T> Models { set; get; }

		/// <summary>
		/// 返回的记录总数
		/// </summary>
		public int Total { set; get; }

		public TData TotalData { get; set; }
	}
}
