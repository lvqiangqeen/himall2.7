using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Models
{
	public class SiteViewModels
	{
		public class PayViewModel
		{
			public string Controller { get; set; }

			public string Action { get; set; }

			public System.Web.Routing.RouteValueDictionary RouteValueDictionary { get; set; }
		}
	}
}