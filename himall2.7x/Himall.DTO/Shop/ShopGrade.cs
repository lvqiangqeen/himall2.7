using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺等级列表
    /// </summary>
    public class ShopGrade
    {
        long _id;
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 等级名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 最大上传商品数量
        /// </summary>
        public int ProductLimit { get; set; }
        /// <summary>
        /// 最大图片可使用空间数量
        /// </summary>
        public int ImageLimit { get; set; }
        /// <summary>
        /// 最大模板数量
        /// </summary>
        public int TemplateLimit { get; set; }

        public decimal ChargeStandard { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
