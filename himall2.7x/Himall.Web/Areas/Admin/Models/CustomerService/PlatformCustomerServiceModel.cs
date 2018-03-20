using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Himall.DTO;

namespace Himall.Web.Areas.Admin.Models
{
    public class PlatformCustomerServiceModel:CustomerService
    {
		static PlatformCustomerServiceModel()
		{
			AutoMapper.Mapper.CreateMap<CustomerService, PlatformCustomerServiceModel>();
			AutoMapper.Mapper.CreateMap<PlatformCustomerServiceModel, CustomerService>();
		}

		/// <summary>
		/// 准备创建时的id
		/// </summary>
		public Guid CreateId { get; set; }
    }
}