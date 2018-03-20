using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopCategoryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long DisplaySequence { get; set; }
        public long ParentCategoryId { get; set; }
        public long ShopId { get; set; }
        public bool IsShow { get; set; }
        public long Depth { get; set; }
        public ShopCategoryModel()
        {

        }

        public ShopCategoryModel(ShopCategoryInfo info)
            : this()
        {
            this.Id = info.Id;
            this.DisplaySequence = info.DisplaySequence;
            this.IsShow = info.IsShow;
            this.Name = info.Name;
            this.ParentCategoryId = info.ParentCategoryId;
            this.ShopId = info.ShopId;
            this.Depth = info.ParentCategoryId != 0 ? 2 : 1;
        }

        public static implicit operator ShopCategoryInfo(ShopCategoryModel m)
        {
            return new ShopCategoryInfo
            {
                Id = m.Id,
                ParentCategoryId = m.ParentCategoryId,
                IsShow = m.IsShow,
                ShopId = m.ShopId,
                DisplaySequence = m.DisplaySequence,
                Name = m.Name
            };
        }
    }
}