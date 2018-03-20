using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Himall.Application.Mappers.Profiles
{
	public class CommonProfile:Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<Model.CategoryInfo, DTO.Category>();
			CreateMap<DTO.Category, Model.CategoryInfo>();

			CreateMap<Model.ManagerInfo, DTO.Manager>();
			CreateMap<DTO.Manager, Model.ManagerInfo>();
		}
	}
}
