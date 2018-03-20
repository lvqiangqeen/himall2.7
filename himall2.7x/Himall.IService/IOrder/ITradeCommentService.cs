using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface ITradeCommentService : IService
    {
        /// <summary>
        /// 查询订单评价
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<OrderCommentInfo> GetOrderComments(OrderCommentQuery query);

        /// <summary>
        /// 删除订单评价
        /// </summary>
        /// <param name="id"></param>
        void DeleteOrderComment(long id);


        void AddOrderComment(OrderCommentInfo info);
        /// <summary>
        /// 根据用户ID和订单ID获取单个订单评价信息
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        OrderCommentInfo GetOrderCommentInfo(long orderId,long userId);

        /// <summary>
        /// 获取订单评论
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IQueryable<OrderCommentInfo> GetOrderComments(long userId);
    }
}
