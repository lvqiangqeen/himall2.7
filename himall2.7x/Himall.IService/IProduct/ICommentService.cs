using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface ICommentService : IService
    {
        /// <summary>
        /// 添加一个产品评论
        /// </summary>
        /// <param name="model"></param>
        void AddComment(ProductCommentInfo model);
		void AddComment(IEnumerable<ProductCommentInfo> models);

        /// <summary>
        /// 回复产品评论
        /// </summary>
        /// <param name="id">商品评论的ID</param>
        /// <param name="shopId">店铺的ID防止跨店回复</param>
        /// <param name="replyConent">回复内容</param>
        void ReplyComment(long id, string replyConent, long shopId);

        /// <summary>
        /// 回复评论或者追加评论
        void ReplyComment(long id, long shopId, string replyContent = "", string appendContent = "");

        /// <summary>
        /// 追加评论
        /// </summary>
        void AppendComment(List<AppendCommentModel> model);

        /// <summary>
        /// 自动评论
        /// </summary>
        /// <param name="userid"></param>
        void AutoComment(long? userid=null);

        /// <summary>
        /// 删除产品评论
        /// </summary>
        /// <param name="id">评论ID</param>
        void HiddenComment(long id);

        /// <summary>
        /// 查询评论列表
        /// </summary>
        /// <param name="query">查询条件实体</param>
        /// <returns></returns>
        ObsoletePageModel<ProductCommentInfo> GetComments(CommentQuery query);


        /// <summary>
        /// 订单列表项判断有没有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        bool HasAppendComment(long subOrderId);
        IQueryable<ProductCommentInfo> GetCommentsByProductId(long productId);

        /// <summary>
        /// 获取单个评论内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductCommentInfo GetComment(long id);
        /// <summary>
        /// 根据IDS取多个评论
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids);
        /// <summary>
        /// 获取用户评价列表
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>

        ObsoletePageModel<ProductEvaluation> GetProductEvaluation(CommentQuery query);

        /// <summary>
        /// 获取用户订单评价的列表
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        ObsoletePageModel<UserOrderCommentModel> GetOrderComment(OrderCommentQuery query);


        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId);

        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId);

        void SetCommentEmpty( long id );

        /// <summary>
        /// 获取用户未评论的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topN"></param>
        /// <returns></returns>
         IQueryable<OrderItemInfo> GetUnEvaluatProducts(long userId);
        decimal GetProductMark(long id);
    }
}
