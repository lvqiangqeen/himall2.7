
using Himall.Core;
using Himall.Model;
namespace Himall.IServices.QueryModel
{
    public partial class TopicQuery : QueryBase
    {

        public string Name { get; set; }

        public string Tags { get; set; }

        public long ShopId { get; set; }
        public PlatformType PlatformType { get; set; }
        //是否推荐
        public  bool? IsRecommend { get; set; }
        public TopicQuery()
        {
            PlatformType = PlatformType.PC;
        }


    }
}
