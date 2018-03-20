using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
	public class ProductProfile:Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<Model.ProductInfo, DTO.Product.Product>();
			CreateMap<DTO.Product.Product, Model.ProductInfo>();

			CreateMap<Model.ProductAttributeInfo, DTO.ProductAttribute>();
			CreateMap<DTO.ProductAttribute, Model.ProductAttributeInfo>();

			CreateMap<Model.ProductDescriptionInfo, DTO.ProductDescription>();
			CreateMap<DTO.ProductDescription, Model.ProductDescriptionInfo>();

			CreateMap<Model.SKUInfo, DTO.SKU>();
			CreateMap<DTO.SKU, Model.SKUInfo>();

			CreateMap<Model.SpecificationValueInfo, DTO.SpecificationValue>();
			CreateMap<DTO.SpecificationValue, Model.SpecificationValueInfo>();

			CreateMap<Model.ProductTypeInfo, DTO.ProductType>();
			CreateMap<DTO.ProductType, Model.ProductTypeInfo>();

			CreateMap<Model.BrandInfo, DTO.Brand>();
			CreateMap<DTO.Brand, Model.BrandInfo>();

			CreateMap<Model.ShopBrandApplysInfo, DTO.ShopBrandApply>();
			CreateMap<DTO.ShopBrandApply, Model.ShopBrandApplysInfo>();

			CreateMap<Model.ProductCommentsImagesInfo, DTO.ProductCommentImage>();
			CreateMap<DTO.ProductCommentImage, Model.ProductCommentsImagesInfo>();

			CreateMap<Model.ProductCommentInfo, DTO.ProductComment>().ForMember(p => p.Images, config => config.MapFrom(p => p.Himall_ProductCommentsImages));
			CreateMap<DTO.ProductComment, Model.ProductCommentInfo>().ForMember(p => p.Himall_ProductCommentsImages, config => config.MapFrom(p => p.Images));

			CreateMap<Model.ProductRelationProductInfo, DTO.ProductRelationProduct>();
			CreateMap<DTO.ProductRelationProduct, Model.ProductRelationProductInfo>();

			CreateMap<Model.SellerSpecificationValueInfo, DTO.SellerSpecificationValue>();
			CreateMap<DTO.SellerSpecificationValue, Model.SellerSpecificationValueInfo>();

			CreateMap<Model.ProductDescriptionInfo, DTO.ProductDescription>();
			CreateMap<DTO.ProductDescription, Model.ProductDescriptionInfo>();

			CreateMap<Model.ProductDescriptionTemplateInfo, DTO.ProductDescriptionTemplate>();
			CreateMap<DTO.ProductDescriptionTemplate, Model.ProductDescriptionTemplateInfo>();

			CreateMap<Model.ShopCategoryInfo, DTO.ShopCategory>();
			CreateMap<DTO.ShopCategory, Model.ShopCategoryInfo>();

			CreateMap<Model.ProductShopCategoryInfo, DTO.ProductShopCategory>();
			CreateMap<DTO.ProductShopCategory, Model.ProductShopCategoryInfo>();
		}
	}
}
