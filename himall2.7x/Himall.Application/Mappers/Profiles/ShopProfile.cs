using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
	public class ShopProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<Model.ShopInfo, DTO.Shop>();
			CreateMap<DTO.Shop, Model.ShopInfo>();

			CreateMap<Model.ShopBranchInfo, DTO.ShopBranch>();
			CreateMap<DTO.ShopBranch, Model.ShopBranchInfo>();

			CreateMap<Model.ShopBranchSkusInfo, DTO.ShopBranchSku>();
			CreateMap<DTO.ShopBranchSku, Model.ShopBranchSkusInfo>();

			CreateMap<Model.VShopInfo, DTO.VShop>();
			CreateMap<DTO.VShop, Model.VShopInfo>();

			CreateMap<Model.CustomerServiceInfo, DTO.CustomerService>();
			CreateMap<DTO.CustomerService, Model.CustomerServiceInfo>();

			CreateMap<Model.ShopAccountItemInfo, Himall.DTO.ShopAccountItem>().ForMember(p => p.ShopAccountType, options => options.MapFrom(p => p.TradeType));
			CreateMap<Himall.DTO.ShopAccountItem, Model.ShopAccountItemInfo>().ForMember(p => p.TradeType, options => options.MapFrom(p => p.ShopAccountType));

			CreateMap<Model.ShopAccountInfo, DTO.ShopAccount>();
			CreateMap<DTO.ShopAccount, Model.ShopAccountInfo>();

			CreateMap<Model.ShopBranchManagersInfo, DTO.ShopBranchManager>();
			CreateMap<DTO.ShopBranchManager, Model.ShopBranchManagersInfo>();
		}
	}
}
