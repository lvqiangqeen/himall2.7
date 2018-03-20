using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
	public class FullDiscountProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<Model.ActiveProductInfo, DTO.FullDiscountActiveProduct>();
			CreateMap<DTO.FullDiscountActiveProduct, Model.ActiveProductInfo>();

            CreateMap<Model.FullDiscountRulesInfo, DTO.FullDiscountRules>();
            CreateMap<DTO.FullDiscountRules, Model.FullDiscountRulesInfo>();

            CreateMap<Model.ActiveInfo, DTO.FullDiscountActive>();
            CreateMap<DTO.FullDiscountActive, Model.ActiveInfo>();
            CreateMap<Model.ActiveInfo, DTO.FullDiscountActiveBase>();
            CreateMap<DTO.FullDiscountActiveBase, Model.ActiveInfo>();
            CreateMap<Model.ActiveInfo, DTO.FullDiscountActiveList>();
            CreateMap<DTO.FullDiscountActiveList, Model.ActiveInfo>();

            //CreateMap<Model.ShopAccountItemInfo, Himall.DTO.ShopAccountItem>().ForMember(p => p.ShopAccountType, options => options.MapFrom(p => p.TradeType));
        }
	}
}
