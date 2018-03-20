using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;
using Himall.Model.DTO;

namespace Himall.Application
{
	public class CommentApplication
	{
		#region 字段
		private static ICommentService _commentService;
		#endregion

		#region 构造函数
		static CommentApplication()
		{
			_commentService = Himall.Core.ObjectContainer.Current.Resolve<ICommentService>();
		}
		#endregion

		#region 方法

		/// <summary>
		/// 获取用户订单中商品的评价列表
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId)
		{
			return _commentService.GetProductEvaluationByOrderId(orderId, userId);
		}

		public static List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId)
		{
			return _commentService.GetProductEvaluationByOrderIdNew(orderId, userId);
		}
        /// <summary>
        /// 根据评论ID取评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ProductCommentInfo GetComment(long id)
        {
            return _commentService.GetComment(id);
        }
        public static List<ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids)
        {
            return _commentService.GetCommentsByIds(ids).ToList();
        }
		public static void Add(DTO.ProductComment comment)
		{
			var info = comment.Map<ProductCommentInfo>();
			_commentService.AddComment(info);
			comment.Id = info.Id;
		}

		public static void Add(IEnumerable<DTO.ProductComment> comments)
		{
			var list = comments.ToList().Map<List<ProductCommentInfo>>();
			_commentService.AddComment(list);
		}

		/// <summary>
		/// 追加评论
		/// </summary>
		public static void Append(List<AppendCommentModel> models)
		{
			_commentService.AppendComment(models);
		}
		#endregion
	}
}
