using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public partial class ShopQuery : QueryBase
    {
        public ShopQuery()
        {
            MoreStatus = new List<Model.ShopInfo.ShopAuditStatus>();
        }
        public long? ShopGradeId { get; set; }
        public Model.ShopInfo.ShopAuditStatus? Status { get; set; }
        /// <summary>
        /// 多个状态
        /// <para>补充Status</para>
        /// </summary>
        public List<Model.ShopInfo.ShopAuditStatus> MoreStatus { get; set; }
        public string ShopName { get; set; }
        public string ShopAccount { get; set; }
        public long CategoryId
        { 
            get;
            set;
        }
        public long BrandId
        {
            get;
            set;
        }

        /// <summary>
        /// 排序方式，
        /// 0 = 默认
        /// 1 = 销量
        /// </summary>
        public int OrderBy
        {
            get;
            set;
        }
    }
}

