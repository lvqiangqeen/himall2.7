using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices.QueryModel
{
    public class LabelQuery : QueryBase
    {
        public string LabelName { get; set; }
        /// <summary>
        /// 标签ID列表
        /// </summary>
        public IEnumerable<long> LabelIds { get; set; }

    }
}
