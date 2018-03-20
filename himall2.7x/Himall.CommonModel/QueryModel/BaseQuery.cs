using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class BaseQuery
    {
        public BaseQuery()
        {
            PageNo = 1;
            PageSize = 10;
        }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// 每页显示多少纪录
        /// </summary>
        public int PageSize { get; set; }


        [Obsolete("为同步插件数据,禁止直接使用此字段")]
        public int Rows
        {
            get { return PageSize; }
            set { PageSize = value; }
        }
        [Obsolete("为同步插件数据,禁止直接使用此字段")]
        public int Page
        {
            get { return PageNo; }
            set { PageNo = value; }
        }
    }
}
