
using Himall.Core.Plugins.Payment;
namespace Himall.Web.Areas.Web.Models
{
    public class PaymentModel
    {

        public string RequestUrl { get; set; }

        public string Logo { get; set; }

        public string Id { get; set; }

        public UrlType UrlType { get; set; }
    }
}