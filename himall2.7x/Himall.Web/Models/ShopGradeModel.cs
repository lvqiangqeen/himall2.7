using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.Core;

namespace Himall.Web.Models
{
    public class ShopGradeModel
    {
        public long Id { get; set; }

        [Display(Name = "套餐名称")]
        [Required(ErrorMessage = "* 套餐名称为必填项")]
        [MaxLength(20,ErrorMessage="套餐名称最多20个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_\u4e00-\u9fa5]+$", ErrorMessage = "套餐名称必须是中文、英文、字母和下划线")]
        public string Name { get; set; }

        [Display(Name = "可发布商品数")]
        [RegularExpression(@"^\+?[1-9][0-9]*$",ErrorMessage = "只能是大于零的整数")]
        [Required(ErrorMessage = "* 可发布商品数为必填项")]
        public int ProductLimit { get; set; }

        [Display(Name = "可用空间(M)")]
        [RegularExpression(@"^\+?[1-9][0-9]*$", ErrorMessage = "只能是大于零的整数")]
        [Required(ErrorMessage = "* 可用空间为必填项")]
        
        public int ImageLimit { get; set; }

        [Display(Name = "年费")]
        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "只能是大于零的数字")]
        [Required(ErrorMessage = "* 年费为必填项")]
        [DataType( System.ComponentModel.DataAnnotations.DataType.Currency)]
        
        public decimal ChargeStandard { get; set; }


        public ShopGradeModel()
        { }
        public ShopGradeModel(ShopGradeInfo m)
            : this()
        {
            this.Id = m.Id;
            this.Name = m.Name;
            this.ImageLimit = m.ImageLimit;
            this.ProductLimit = m.ProductLimit;
            this.ChargeStandard = m.ChargeStandard;
        }

        public static implicit operator ShopGradeInfo(ShopGradeModel m)
        {
            return new ShopGradeInfo
            {
                Id = m.Id,
                ProductLimit = m.ProductLimit,
                ChargeStandard = m.ChargeStandard,
                ImageLimit = m.ImageLimit,
                Name = m.Name,
                Remark = ""
            };
        }
    }
}