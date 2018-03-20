using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Himall.Web.Areas.Admin.Models.Product
{
    public class CategoryModel
    {

        public long Id { get; set; }

        [MaxLength(12, ErrorMessage = "分类名称不能多于12个字符")]
        [Required(ErrorMessage = "分类名称必填,且不能多于12个字符")]
        [RegularExpression("^[^<>'/\\~\"]+$", ErrorMessage="分类名称不能包含 <>'/\\~\"")]
        public string Name { get; set; }
        public string Icon { get; set; }
        public long DisplaySequence { get; set; }

        //[RegularExpression(@"[^0]", ErrorMessage = "上级分类必填")]
        //[Required(ErrorMessage = "上级分类必填")]
        public long ParentCategoryId { get; set; }

        [RegularExpression(@"^\d{1,3}(\.\d{1,2})?$", ErrorMessage = "分佣比例只能是大于等于0的的数字，可保留两位小数")]
        [Range(0.00, 100, ErrorMessage = "佣金比不可以超出商品价值")]
        [Required(ErrorMessage = "分类佣金比例必填")]
        public decimal CommisRate { get; set; }

        public int Depth { get; set; }
        public string Path { get; set; }
        public string RewriteName { get; set; }
        public bool HasChildren { get; set; }

        [RegularExpression(@"^[0-9]*[1-9][0-9]*$", ErrorMessage = "类型必填")]
        [Required(ErrorMessage = "类型必填")]
        public long TypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public CategoryModel()
        { }
        public CategoryModel(CategoryInfo m)
            : this()
        {   
            this.Id = m.Id;
            this.Depth = m.Depth;
            this.DisplaySequence = m.DisplaySequence;
            this.HasChildren = m.HasChildren;
            this.Icon = m.Icon;
            this.Description = m.Meta_Description;
            this.Keywords = m.Meta_Keywords;
            this.Title = m.Meta_Title;
            this.Name = m.Name;
            this.ParentCategoryId = m.ParentCategoryId;
            this.RewriteName = m.RewriteName;
            this.Path = m.Path;
            this.TypeId = m.TypeId;
            this.CommisRate = m.CommisRate;
        }

        public static implicit operator CategoryInfo(CategoryModel m)
        {
            return new CategoryInfo
            {
                Id = m.Id,
                ParentCategoryId = m.ParentCategoryId,
                Depth = m.Depth,
                DisplaySequence = m.DisplaySequence,
                HasChildren = m.HasChildren,
                Icon = m.Icon,
                Meta_Description = m.Description,
                Meta_Keywords = m.Keywords,
                Meta_Title = m.Title,
                Name = m.Name,
                Path = m.Path,
                RewriteName = m.RewriteName,
                TypeId = m.TypeId,
                CommisRate = m.CommisRate


            };
        }


    }

    public class CategoryDropListModel
    {
        public long Id { get; set; }
        public long ParentCategoryId { get; set; }
        public string Name { get; set; }
        public int Depth { get; set; }
    }


    public class CategoryTreeModel
    {
        public long Id { get; set; }
        public long ParentCategoryId { get; set; }
        public string Name { get; set; }
        public int Depth { get; set; }

        public IEnumerable<CategoryTreeModel> Children { get; set; }
    }

}