using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class FullDiscountActiveModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 所属店铺
        /// </summary>
        public long ShopId { get; set; }
        [Required(ErrorMessage = "请填写活动名称")]
        [Display(Name = "活动名称")]
        [StringLength(20,ErrorMessage ="活动名称不可大于20字符")]
        [RegularExpression("^(\\w|[\\u4e00-\\u9fa5]){1,20}$", ErrorMessage = "活动名称为1-20长度的非特殊字符")]
        public string ActiveName { get; set; }
        [Required(ErrorMessage = "请填写活动开始时间")]
        [DataType(DataType.Date, ErrorMessage = "错误的开始时间")]
        [Display(Name = "开始时间")]
        public System.DateTime StartTime { get; set; }
        [Required(ErrorMessage = "请填写活动结束时间")]
        [DataType(DataType.Date, ErrorMessage = "错误的结束时间")]
        [Display(Name = "结束时间")]
        public System.DateTime EndTime { get; set; }
        /// <summary>
        /// 是否全部商品
        /// </summary>
        public bool IsAllProduct { get; set; }
        /// <summary>
        /// 是否全部门店
        /// </summary>
        public bool IsAllStore { get; set; }
        /// <summary>
        /// 规则
        /// </summary>
        [Required(ErrorMessage = "请填写活动规则")]
        [Display(Name = "活动规则")]
        public string RuleJSON { get; set; }
        /// <summary>
        /// 商品集
        /// </summary>
        public string ProductIds { get; set; }

        public DateTime EndServerTime { get; set; }

        /// <summary>
        /// 验证有效性
        /// </summary>
        public void CheckValidation()
        {
            if (EndTime <= StartTime)
            {
                throw new HimallException("错误的活动时间");
            }
            if (!IsAllProduct && string.IsNullOrWhiteSpace(ProductIds))
            {
                throw new HimallException("请选择参与活动的商品");
            }
            if (string.IsNullOrWhiteSpace(RuleJSON))
            {
                throw new HimallException("请填写优惠规则");
            }
        }
    }
}