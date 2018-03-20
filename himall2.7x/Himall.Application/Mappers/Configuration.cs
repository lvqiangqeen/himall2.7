using AutoMapper;
using System.Web;
using System.Collections.Generic;
using System.Collections;

[assembly: PreApplicationStartMethod(typeof(Himall.Application.Configuration), "InitConfiguration")]
namespace Himall.Application
{
    public static class Configuration
    {
        public static void InitConfiguration()
        {
            Mapper.Initialize(cfg =>
            {
				cfg.AddProfile<Himall.Application.Mappers.Profiles.OrderProfile>();
                cfg.AddProfile<Himall.Application.Mappers.Profiles.ShopProfile>();
				cfg.AddProfile<Himall.Application.Mappers.Profiles.MemberProfile>();
				cfg.AddProfile<Himall.Application.Mappers.Profiles.ProductProfile>();
				cfg.AddProfile<Himall.Application.Mappers.Profiles.FreightTemplateProfile>();
				cfg.AddProfile<Himall.Application.Mappers.Profiles.CommonProfile>();
				cfg.AddProfile<Himall.Application.Mappers.Profiles.FullDiscountProfile>();
            });
        }

		public static TDestination Map<TDestination>(this object source)
		{
			return AutoMapper.Mapper.Map<TDestination>(source);
		}

		public static void Map<TSource, TDestination>(this TSource source, TDestination target)
		{
			AutoMapper.Mapper.Map(source, target);
		}

		public static void DynamicMap<TSource, TDestination>(this TSource source, TDestination target)
		{
			AutoMapper.Mapper.DynamicMap(source, target);
		}

		public static TDestination DynamicMap<TDestination>(this object source)
		{
			return AutoMapper.Mapper.DynamicMap<TDestination>(source);
		}
    }
}
