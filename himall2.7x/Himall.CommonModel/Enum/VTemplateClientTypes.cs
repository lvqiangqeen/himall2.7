using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 可视模板类型
    /// </summary>
    public enum VTemplateClientTypes
    {
        /// <summary>
        /// 移动端首页
        /// </summary>
        WapIndex = 1,
        /// <summary>
        /// 商家移动端首页
        /// </summary>
        SellerWapIndex = 2,
        /// <summary>
        /// 移动端专题
        /// </summary>
        WapSpecial = 11,
        /// <summary>
        /// 商家移动端专题
        /// </summary>
        SellerWapSpecial = 12,
        /// <summary>
        /// 微信小程序首页
        /// </summary>
        WXSmallProgram=13,
    }
}
