using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using Himall.SmallProgAPI.Model;

namespace Himall.SmallProgAPI.Helper
{
    public class ShopServiceMark
    {
        public static ShopServiceMarkModel GetShopComprehensiveMark(long shopId)
        {
            var result = new ShopServiceMarkModel();
            var orderComment = ServiceHelper.Create<ITradeCommentService>().GetOrderComments(new OrderCommentQuery
            {
                ShopId = shopId,
                PageNo = 1,
                PageSize = 100000
            });


            result.PackMark = orderComment.Models.Count() == 0 ? 0 :
                Math.Round(orderComment.Models.ToList().Average(o => ((decimal)o.PackMark + o.DeliveryMark) / 2), 2);
            result.ServiceMark = orderComment.Models.Count() == 0 ? 0 :
                Math.Round(orderComment.Models.ToList().Average(o => (decimal)o.ServiceMark), 2);
            result.ComprehensiveMark = Math.Round((result.PackMark + result.ServiceMark) / 2, 2);
            return result;
        }
    }
}
