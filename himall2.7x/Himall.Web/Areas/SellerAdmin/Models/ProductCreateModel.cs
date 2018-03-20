using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using FluentValidation;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Validators;
using Himall.DTO.Product;
using Himall.DTO;

namespace Himall.Web.Areas.SellerAdmin.Models
{
	[FluentValidation.Attributes.Validator(typeof(ProductCreateValidator))]
	public class ProductCreateModel : DTO.Product.Product
	{
		#region 属性
		/// <summary>
		/// 此属性只为前端验证用
		/// </summary>
		public long Stock
		{
			get;
			set;
		}

		public string[] Pics
		{
			get;
			set;
		}

		public SKUEx[] SKUExs
		{
			get;
			set;
		}

		public long[] GoodsCategory
		{
			get;
			set;
		}

		public long? SafeStock { get; set; }

		/// <summary>
		/// 商家修改的规格
		/// </summary>
		public SpecificationValue[] UpdateSpecs
		{
			get;
			set;
		}

		public AttrSelectData[] SelectAttributes { get; set; }

		public List<FreightTemplate> FreightTemplates
		{
			get;
			set;
		}

		public List<ShopCategory> ShopCategorys
		{
			get;
			set;
		}

		public List<ProductDescriptionTemplate> DescriptionTemplates
		{
			get;
			set;
		}

		public List<CategoryGroup> CategoryGroups
		{
			get;
			set;
		}
		#endregion

		#region 方法
		public SellerSpecificationValue[] GetSellerSpecification(long shopId, long productTypeId)
		{
			//保存商家自定义规格
			if (this.UpdateSpecs != null && this.UpdateSpecs.Length > 0)
			{
				return this.UpdateSpecs.Select(item => new SellerSpecificationValue()
				{
					ShopId = shopId,
					ValueId = item.Id,
					Specification = item.Specification,
					TypeId = productTypeId,
					Value = item.Value
				}).ToArray();
			}

			return null;
		}

		public ProductAttribute[] GetProductAttribute(long productId)
		{
			var attributes = new List<ProductAttribute>();

			if (this.SelectAttributes != null && this.SelectAttributes.Length > 0)
			{
				foreach (var attr in this.SelectAttributes)
				{
					if (attr == null || string.IsNullOrWhiteSpace(attr.ValueId))
						continue;

					foreach (var item in attr.ValueId.Split(','))
					{
						if (string.IsNullOrWhiteSpace(item))
							continue;

						attributes.Add(new ProductAttribute
						{
							AttributeId = attr.AttributeId,
							ProductId = productId,
							ValueId = long.Parse(item)
						});
					}
				}
			}

			return attributes.ToArray();
		}
		#endregion
	}

	public class AttrSelectData
	{
		public long AttributeId { get; set; }

		public string ValueId { get; set; }
	}

	//属性必须为SpecificationType枚举项名称加Id，如要修改Product.cshtml页面也需做相应修改
	[FluentValidation.Attributes.Validator(typeof(SKUExValidator))]
	public class SKUEx : DTO.SKU
	{
		#region 字段
		private long _cid;
		private long _sid;
		private long _vid;
		#endregion

		#region 属性
		public long ColorId
		{
			get
			{
				if (_cid == 0 && !string.IsNullOrWhiteSpace(this.Id))
					_cid = SplitId(this.Id)[SpecificationType.Color];

				return _cid;
			}
			set
			{
				_cid = value;
			}
		}

		public long SizeId
		{
			get
			{
				if (_sid == 0 && !string.IsNullOrWhiteSpace(this.Id))
					_sid = SplitId(this.Id)[SpecificationType.Size];

				return _sid;
			}
			set
			{
				_sid = value;
			}
		}

		public long VersionId
		{
			get
			{
				if (_vid == 0 && !string.IsNullOrWhiteSpace(this.Id))
					_vid = SplitId(this.Id)[SpecificationType.Version];

				return _vid;
			}
			set
			{
				_vid = value;
			}
		}
		#endregion

		#region 方法
		public string CreateId(long? pid)
		{
			return CreateId(pid, this.ColorId, this.SizeId, this.VersionId);
		}

		public static string CreateId(long? pid, long colorId, long sizeId, long versionId)
		{
			//解析的顺序必需和创建的顺序一致
			return string.Format("{0}_{1}_{2}_{3}", pid.HasValue ? pid.Value.ToString() : "{0}", colorId, sizeId, versionId);
		}

		public static Dictionary<SpecificationType, long> SplitId(string id)
		{
			var result = new Dictionary<SpecificationType, long>();

			if (!string.IsNullOrWhiteSpace(id))
			{
				var temp = id.Split('_');
				//解析的顺序必需和创建的顺序一致
				result.Add(SpecificationType.Color, long.Parse(temp[1]));
				result.Add(SpecificationType.Size, long.Parse(temp[2]));
				result.Add(SpecificationType.Version, long.Parse(temp[3]));
			}

			return result;
		}
		#endregion
	}

	public class ProductCreateValidator : AbstractValidator<ProductCreateModel>
	{
		public ProductCreateValidator()
		{
			RuleFor(p => p.Stock).NotNull().WithMessage("请输入库存").GreaterThanOrEqualTo(0).WithMessage("请输入正确的库存");
			RuleFor(p => p.MinSalePrice).NotNull().WithMessage("请输入吊牌价").GreaterThanOrEqualTo((decimal)0.01).WithMessage("请输入正确的金额");
			RuleFor(p => p.CategoryId).NotNull().WithMessage("请选择平台分类");
			RuleFor(p => p.ProductName).NotNull().WithMessage("请输入商品名称").Length(1, 100).WithMessage("商品名称不能超过100个字符");
			RuleFor(p => p.ProductCode).NotNull().WithMessage("请输入商品货号").Length(1, 100).WithMessage("商品货号不能超过100个字符");
			RuleFor(p => p.ShortDescription).Length(0, 4000).WithMessage("广告词过长");
			RuleFor(p => p.MarketPrice).NotNull().WithMessage("请输入默认供货价").GreaterThanOrEqualTo((decimal)0.01).WithMessage("请输入大于0.01的金额");
			RuleFor(p => p.MeasureUnit).NotNull().WithMessage("请输入计量单位").Length(1, 20).WithMessage("计量单位不能超过20个字符");
			RuleFor(p => p.FreightTemplateId).NotNull().WithMessage("请选择运费模版").GreaterThan(0).WithMessage("请选择运费模版");
			RuleFor(p => p.Pics).SetValidator(new PicsValidator("请至少上传一张商品图片"));
			RuleFor(p => p.Description).NotNull().WithMessage("请输入用于PC端展示的商品描述");
			RuleFor(p => p.Description.Description).NotNull().WithMessage("请输入用于PC端展示的商品描述");
			RuleFor(p => p.Description.MobileDescription).NotNull().WithMessage("请输入用于手机端展示的商品描述");
            RuleFor(p => p.Weight).NotNull().WithMessage("请输入商品重量");
            RuleFor(p => p.Volume).NotNull().WithMessage("请输入商品体积");
        }
	}

	public class PicsValidator : PropertyValidator
	{
		public PicsValidator(string errorMessage)
			: base(errorMessage)
		{
		}

		protected override bool IsValid(PropertyValidatorContext context)
		{
			var pics = context.PropertyValue as string[];
			return pics != null && pics.Any(p => !string.IsNullOrWhiteSpace(p));
		}
	}

	public class SKUExValidator : AbstractValidator<SKUEx>
	{
		public SKUExValidator()
		{
			RuleFor(p => p.CostPrice).GreaterThanOrEqualTo(0).WithMessage("请输入正确的金额");
			RuleFor(p => p.SalePrice).GreaterThanOrEqualTo(0).WithMessage("请输入正确的金额");
			RuleFor(p => p.Stock).GreaterThanOrEqualTo(0).WithMessage("请输入正确库存");
		}
	}
}