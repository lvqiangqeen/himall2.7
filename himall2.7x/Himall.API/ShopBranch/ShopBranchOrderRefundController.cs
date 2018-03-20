using System.Collections.Generic;
using System.Linq;
using Himall.API.Model;
using Himall.Application;
using Himall.CommonModel.Enum;
using Himall.Core;
using Himall.DTO;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.IServices;

namespace Himall.API
{
    public class ShopBranchOrderRefundController : BaseShopBranchApiController
    {
        public List<OrderRefundApiModel> GetRefund(int? refundMode, long? orderId = null, int pageNo = 1, /*页码*/ int pageSize = 10/*每页显示数据量*/)
        {
            CheckUserLogin();

            var queryModel = new RefundQuery()
            {
                ShopId = CurrentShopBranch.ShopId,
                ShopBranchId = this.CurrentUser.ShopBranchId,
                PageSize = pageSize,
                PageNo = pageNo,
                ShowRefundType = refundMode,
                OrderId = orderId
            };

            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);

            return FullModel(refunds.Models);
        }

        public object PostReply(OrderRefundReplyModel reply)
        {
            if (reply == null)
                return ErrorResult("参数不能为空");

            CheckUserLogin();

            switch (reply.AuditStatus)
            {
                case OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                    if (string.IsNullOrWhiteSpace(reply.SellerRemark))
                    {
                        throw new HimallException("请填写拒绝理由");
                    }
                    break;
            }

            var refund = Application.RefundApplication.GetOrderRefund(reply.RefundId);

            if (refund == null || refund.ShopId != CurrentShopBranch.ShopId)
                return ErrorResult("无效的售后申请编号");

            var order = Application.OrderApplication.GetOrder(refund.OrderId);
            if (order == null || order.ShopBranchId != this.CurrentUser.ShopBranchId)
                return ErrorResult("无效的售后申请编号");

            if (order.DeliveryType != CommonModel.Enum.DeliveryType.SelfTake
                && reply.AuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving
                )
            {
                //如果不是自提订单，则状态还是为待买家寄货，不能直接到待商家收货
                reply.AuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery;
            }

            if (refund.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving && reply.AuditStatus!= OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                RefundApplication.SellerConfirmRefundGood(reply.RefundId, this.CurrentUser.UserName);
            else
                RefundApplication.SellerDealRefund(reply.RefundId, reply.AuditStatus, reply.SellerRemark, this.CurrentUser.UserName);

            return SuccessResult("操作成功");
        }

        public object GetRefundLogs(int refundId)
        {
            CheckUserLogin();

            var refund = RefundApplication.GetOrderRefund(refundId);

            if (refund == null || refund.ShopId != CurrentShopBranch.ShopId)
                return ErrorResult("无效的售后申请编号");

            var order = Application.OrderApplication.GetOrder(refund.OrderId);
            if (order == null || order.ShopBranchId != this.CurrentUser.ShopBranchId)
                return ErrorResult("无效的售后申请编号");

            var refundLogs = RefundApplication.GetRefundLogs(refundId);

            var logs = new List<object>();

            var roleMap = new Dictionary<OrderRefundStep, int>();//操作步骤 由谁完成的
            roleMap.Add(OrderRefundStep.Confirmed, 2);
            roleMap.Add(OrderRefundStep.UnAudit, 1);
            roleMap.Add(OrderRefundStep.UnConfirm, 1);
            roleMap.Add(OrderRefundStep.WaitAudit, 0);
            roleMap.Add(OrderRefundStep.WaitDelivery, 1);
            roleMap.Add(OrderRefundStep.WaitReceiving, 1);

            foreach (var log in refundLogs)
            {
                logs.Add(new
                {
                    Role = roleMap[log.Step],//操作者角色，0：买家，1：门店，2：平台
                    Step = log.Step,
                    log.OperateDate,
                    log.Remark
                });
            }

            var model = new OrderRefundApiModel();
            refund.Map(model);
            model.CertPics = new string[3];
            model.CertPics[0] = HimallIO.GetRomoteImagePath(model.CertPic1);
            model.CertPics[1] = HimallIO.GetRomoteImagePath(model.CertPic2);
            model.CertPics[2] = HimallIO.GetRomoteImagePath(model.CertPic3);

            return new
            {
                Refund = model,
                Logs = logs
            };
        }

        public OrderRefundApiModel GetRefundInfo(int refundId)
        {
            CheckUserLogin();

            var refund = Application.RefundApplication.GetOrderRefund(refundId, shopId: CurrentShopBranch.ShopId);

            return FullModel(new List<OrderRefund>() { refund })[0];
        }

        private List<OrderRefundApiModel> FullModel(List<OrderRefund> refunds)
        {
            var orders = Application.OrderApplication.GetOrders(refunds.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItemsByOrderItemId(refunds.Select(p => p.OrderItemId));
            var members = Application.MemberApplication.GetMembers(orders.Select(p => p.UserId));

            var result = refunds.Select(item =>
            {
                var orderItem = orderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId);
                var order = orders.FirstOrDefault(o => o.Id == item.OrderId);
                var member = members.FirstOrDefault(m => m.Id == order.UserId);
                orderItem.ThumbnailsUrl = HimallIO.GetRomoteProductSizeImage(orderItem.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_100);
                ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(orderItem.ProductId);
                orderItem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                orderItem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                orderItem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                var model = new OrderRefundApiModel();
                item.Map(model);
                model.Status = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? (int)item.ManagerConfirmStatus : (int)item.SellerAuditStatus;
                //model.StatusDescription = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? item.ManagerConfirmStatus.ToDescription() : ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription();
                model.StatusDescription = item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited ? item.ManagerConfirmStatus.ToDescription() : (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0 ?
                                   ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription());
                model.UserName = member.RealName;
                model.UserCellPhone = member.CellPhone;
                model.OrderItem = orderItem;
                model.CertPics = new string[3];
                model.CertPics[0] = HimallIO.GetRomoteImagePath(model.CertPic1);
                model.CertPics[1] = HimallIO.GetRomoteImagePath(model.CertPic2);
                model.CertPics[2] = HimallIO.GetRomoteImagePath(model.CertPic3);
                string shopBranchName = "总店";
                if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                    if (shopBranchInfo != null)
                    {
                        shopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                model.ShopName = shopBranchName;

                return model;
            });

            return result.ToList();
        }
    }
}
