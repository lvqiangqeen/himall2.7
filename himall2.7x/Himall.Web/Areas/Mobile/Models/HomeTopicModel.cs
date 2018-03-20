
using System.Collections.Generic;
namespace Himall.Web.Areas.Mobile.Models
{
    public class HomeTopicModel
    {

        public string ImageUrl { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public long Id { get; set; }

    }
}