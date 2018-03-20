using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
	public class CustomerService
	{
		public long Id { get; set; }
		public long ShopId { get; set; }
		public CustomerServiceInfo.ServiceTool Tool { get; set; }
		public CustomerServiceInfo.ServiceType Type { get; set; }
		public string Name { get; set; }
		public string AccountCode { get; set; }
		public Model.CustomerServiceInfo.ServiceTerminalType TerminalType { get; set; }
		public Model.CustomerServiceInfo.ServiceStatusType ServerStatus { get; set; }
	}
}
