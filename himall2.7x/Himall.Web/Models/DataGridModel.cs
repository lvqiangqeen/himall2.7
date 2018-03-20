using System.Collections.Generic;

namespace Himall.Web.Models
{
    /// <summary>
    /// 表格模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataGridModel<T>
    {
        /// <summary>
        /// 数据
        /// </summary>
        public IEnumerable<T> rows { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int total { get; set; }
    }
}