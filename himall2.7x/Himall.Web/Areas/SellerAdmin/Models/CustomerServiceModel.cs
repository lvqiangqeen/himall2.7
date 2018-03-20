using Himall.Model;
using System.ComponentModel.DataAnnotations;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class CustomerServiceModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage="客服名称必须填写")]
        public string Name { get; set; }

        public CustomerServiceInfo.ServiceTool Tool { get; set; }

        public CustomerServiceInfo.ServiceType? Type { get; set; }

        [Required(ErrorMessage = "账号必须填写")]
        public string Account { get; set; }
    }

	public class CustomerServiceManagementViewModel
	{
		public System.Collections.Generic.List<CustomerServiceModel> CustomerServices { get; set; }

		public bool HasMobileService { get; set; }

		public CustomerServiceInfo MobileService { get; set; }
	}
}