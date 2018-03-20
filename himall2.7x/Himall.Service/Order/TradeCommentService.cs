using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.Core;


namespace Himall.Service
{
    public class TradeCommentService : ServiceBase, ITradeCommentService
    {
        public ObsoletePageModel<OrderCommentInfo> GetOrderComments(OrderCommentQuery query)
        {
            IQueryable<OrderCommentInfo> orderComments = Context.OrderCommentInfo.AsQueryable();

            #region 条件组合
            if (query.OrderId.HasValue){
                orderComments=orderComments.Where(item=>query.OrderId == item.OrderId);
            }
            if(query.StartDate.HasValue){
                orderComments=orderComments.Where(item=>item.CommentDate>=query.StartDate.Value);
            }
            if(query.EndDate.HasValue){

                var end = query.EndDate.Value.Date.AddDays(1);
                orderComments=orderComments.Where(item=>item.CommentDate<end);
            }
            if(query.ShopId.HasValue){
                orderComments=orderComments.Where(item=>query.ShopId == item.ShopId);
            }
            if(query.UserId.HasValue){
                orderComments=orderComments.Where(item=>query.UserId == item.UserId);
            }
            if(!string.IsNullOrWhiteSpace(query.ShopName)){
                orderComments=orderComments.Where(item=>item.ShopName.Contains(query.ShopName));
            }
            if(!string.IsNullOrWhiteSpace(query.UserName)){
                orderComments=orderComments.Where(item=>item.UserName.Contains(query.UserName));
            }
            #endregion
            
            int total;
            orderComments = orderComments.GetPage(out total, query.PageNo, query.PageSize);

            ObsoletePageModel<OrderCommentInfo> pageModel = new ObsoletePageModel<OrderCommentInfo>() { Models = orderComments, Total = total };
            return pageModel;
        }
        
        public void DeleteOrderComment(long Id)
        {
            OrderCommentInfo ociobj = Context.OrderCommentInfo.FindById(Id);
            if (ociobj != null)
            {
                //删除相关信息
                List<long?> orditemid = Context.OrderItemInfo.FindBy(d => d.OrderId == ociobj.OrderId).Select(d => (long?)d.Id).ToList();
                var procoms = Context.ProductCommentInfo.FindBy(d => orditemid.Contains(d.SubOrderId)).ToList();
                Context.ProductCommentInfo.RemoveRange(procoms);
                //删除订单评价
                Context.OrderCommentInfo.Remove(ociobj);
                Context.SaveChanges();
            }
        }

        public void AddOrderComment(OrderCommentInfo info)
        {
            var order = Context.OrderInfo.Where(a => a.Id == info.OrderId && a.UserId == info.UserId).FirstOrDefault();
            if (order == null)
            {
                throw new HimallException("该订单不存在，或者不属于该用户！");
            }
            var orderComment= Context.OrderCommentInfo.Where(a => a.OrderId == info.OrderId && a.UserId == info.UserId);
            if (orderComment.Count() > 0)
                throw new HimallException("您已经评论过该订单！");
            info.ShopId = order.ShopId;
            info.ShopName = order.ShopName;
            info.UserName = order.UserName;
            info.CommentDate = DateTime.Now;
            Context.OrderCommentInfo.Add(info);
            Context.SaveChanges();
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = info.UserName;
            record.ReMark = "订单号:" + info.OrderId;
            record.MemberId = info.UserId;
            record.RecordDate = DateTime.Now;
            record.TypeId = MemberIntegral.IntegralType.Comment;
            MemberIntegralRecordAction action = new MemberIntegralRecordAction();
            action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Comment;
            action.VirtualItemId = info.OrderId;
            record.Himall_MemberIntegralRecordAction.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Comment);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        public OrderCommentInfo GetOrderCommentInfo(long orderId, long userId)
        {
            return Context.OrderCommentInfo.Where(a => a.UserId == userId && a.OrderId == orderId).FirstOrDefault();
        }


        public IQueryable<OrderCommentInfo> GetOrderComments(long userId)
        {
            return Context.OrderCommentInfo.Where(item => item.UserId == userId);
        }
    }
}
