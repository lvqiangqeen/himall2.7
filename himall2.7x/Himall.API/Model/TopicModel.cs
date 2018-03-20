using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    public class TopicModel
    {
        public long Id { get; set; }
        public string TopImage { get; set; }
        public string Name { get; set; }

        public List<TopicModuleModel> TopicModule { get; set; }

    }
    public class TopicModuleModel
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<TopicModuleProductModel> TopicModelProduct { get; set; }

        public int Total { get; set; }

    }
    public class TopicModuleProductModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal MarketPrice { get; set; }
    }
}
