using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models.Member
{
	public class MemberViewModels
	{
		public class MemberDetail : DTO.Members
		{
			static MemberDetail()
			{
				AutoMapper.Mapper.CreateMap<DTO.Members, MemberDetail>();
			}

			/// <summary>
			/// 是否是销售员
			/// </summary>
			public bool IsPromoter { get; set; }
		}
	}
}