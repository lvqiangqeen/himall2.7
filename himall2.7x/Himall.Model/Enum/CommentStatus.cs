using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
  public enum CommentStatus
    {
        /// <summary>
        /// 已评论
        /// </summary>
        [Description("初评")]
         First = 0,
        /// <summary>
        /// 增值税发票
        /// </summary>
        [Description("追加评论")]
        Append = 1,

        /// <summary>
        /// 普通发票
        /// </summary>
        [Description("已评")]
        Finshed = 2
    }
}
