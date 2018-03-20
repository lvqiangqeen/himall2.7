using System;
using System.Collections.Generic;

namespace Himall.Model
{

    public partial class MemberGrade
    {
        /// <summary>
        /// 是否可以删除
        /// <para>拥有关联礼品时不可删除</para>
        /// </summary>
        public bool IsNoDelete { get; set; }
    }
}
