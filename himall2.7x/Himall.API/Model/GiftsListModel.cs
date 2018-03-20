using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model
{
    /// <summary>
    /// 获取礼品列表模型
    /// </summary>
    public class GiftsListModel
    {
        /// <summary>
        /// 礼品列表
        /// <para>一页数据</para>
        /// </summary>
        public List<GiftModel> DataList { get; set; }
        /// <summary>
        /// 礼品每页显示量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 礼品总数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 礼品总页数
        /// </summary>
        public int MaxPage { get; set; }
    }
}
