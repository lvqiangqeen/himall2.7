using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class ComplaintService : ServiceBase, IComplaintService
    {
        public ObsoletePageModel<OrderComplaintInfo> GetOrderComplaints(ComplaintQuery complaintQuery)
        {
            int total;

            //所有的子账号
            IList<long> childIds = new List<long>();
            
            IQueryable<OrderComplaintInfo> complaints = Context.OrderComplaintInfo.AsQueryable();
            if(complaintQuery.OrderId.HasValue){
                complaints=complaints.Where(item=>complaintQuery.OrderId == item.OrderId);
            }
            if(complaintQuery.StartDate.HasValue){
                complaints=complaints.Where(item=> item.ComplaintDate>=complaintQuery.StartDate.Value);
            }
            if(complaintQuery.EndDate.HasValue){
                var endDay = complaintQuery.EndDate.Value.Date.AddDays(1);
                complaints=complaints.Where(item=>item.ComplaintDate< endDay);
            }
            if(complaintQuery.Status.HasValue){
                complaints=complaints.Where(item=>complaintQuery.Status == item.Status);
            }
            if(complaintQuery.ShopId.HasValue){
                complaints=complaints.Where(item=>complaintQuery.ShopId == item.ShopId);
            }
            if(complaintQuery.UserId.HasValue){
                complaints=complaints.Where(item=>item.UserId == complaintQuery.UserId);
            }
            if(!string.IsNullOrWhiteSpace(complaintQuery.ShopName)){
                complaints=complaints.Where(item=>item.ShopName.Contains(complaintQuery.ShopName));
            }
            if(!string.IsNullOrWhiteSpace(complaintQuery.UserName)){
                complaints=complaints.Where(item=>item.UserName.Contains(complaintQuery.UserName));
            }
            complaints = complaints.GetPage(out total, complaintQuery.PageNo, complaintQuery.PageSize);
            ObsoletePageModel<OrderComplaintInfo> pageModel = new ObsoletePageModel<OrderComplaintInfo>() { Models = complaints, Total = total };
            return pageModel;
        }

        public void DealComplaint(long id)
        {
            OrderComplaintInfo orderComplaint = Context.OrderComplaintInfo.FindById(id);

            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.End;

            Context.SaveChanges();
        }

        public void SellerDealComplaint(long id, string reply)
        {
            OrderComplaintInfo orderComplaint = Context.OrderComplaintInfo.FindById(id);

            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.Dealed;
            orderComplaint.SellerReply = reply;

            Context.SaveChanges();
        }

        public void UserDealComplaint(long id,long userId)
        {
            OrderComplaintInfo orderComplaint = Context.OrderComplaintInfo.FindById(id);
            if(orderComplaint.UserId!=userId)
            {
                throw new HimallException("该投诉不属于此用户！");
            }
            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.End;
            Context.SaveChanges();
        }

        public void UserApplyArbitration(long id, long userId)
        {
            OrderComplaintInfo orderComplaint = Context.OrderComplaintInfo.FindById(id);
            if (orderComplaint.UserId != userId)
            {
                throw new HimallException("该投诉不属于此用户！");
            }
            orderComplaint.Status = OrderComplaintInfo.ComplaintStatus.Dispute;
            Context.SaveChanges();
        }


        public IQueryable<OrderComplaintInfo> GetAllComplaint()
        {
            return Context.OrderComplaintInfo.FindAll();
        }

        //添加一个用户投诉
        public void AddComplaint(OrderComplaintInfo model)
        {
            if(Context.OrderComplaintInfo.Any(a=>a.OrderId==model.OrderId))
            {
                throw new HimallException("你已经投诉过了，请勿重复投诉！");
            }
            if(string.IsNullOrEmpty(model.SellerReply))
            {
                model.SellerReply = string.Empty;
            }
            Context.OrderComplaintInfo.Add(model);
            Context.SaveChanges();
        }
    }
}
