using System.Collections.Generic;
using System.Linq;

namespace Himall.Model
{
	[System.Obsolete("请使用Himall.CommonModel.QueryPageModel<T>")]
    public class ObsoletePageModel<T>
    {
        /// <summary>
        /// 模型集合
        /// </summary>
        public IQueryable<T> Models { get; set; }

        /// <summary>
        /// 符合条件的总数
        /// </summary>
        public int Total { get; set; }
    }

	[System.Obsolete("请使用Himall.CommonModel.QueryPageModel<T,TData>")]
    public class ObsoletePageModel<T,TData>
    {
        /// <summary>
        /// 模型集合
		/// </summary>
		public IQueryable<T> Models { get; set; }

        /// <summary>
        /// 符合条件的总数
        /// </summary>
        public int Total { get; set; }

        public TData TotalData { get; set; }
    }
}
