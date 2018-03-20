using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Transactions;
using Himall.IServices.QueryModel;

namespace Himall.Service
{
    public partial class GiftsOrderService : ServiceBase, IGiftsOrderService
    {
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public GiftOrderInfo CreateOrder(GiftOrderModel model)
        {
            if (model.CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            if (model.ReceiveAddress == null)
            {
                throw new HimallException("错误的收货人信息");
            }
            GiftOrderInfo result = new GiftOrderInfo()
            {
                Id = GenerateOrderNumber(),
                UserId = model.CurrentUser.Id,
                RegionId = model.ReceiveAddress.RegionId,
                ShipTo = model.ReceiveAddress.ShipTo,
                Address = model.ReceiveAddress.Address,
                RegionFullName = model.ReceiveAddress.RegionFullName,
                CellPhone = model.ReceiveAddress.Phone,
                TopRegionId = int.Parse(model.ReceiveAddress.RegionIdPath.Split(',')[0]),
                UserRemark = model.UserRemark
            };
            using (TransactionScope scope = new TransactionScope())
            {
                //礼品信息处理，库存判断并减库存
                foreach (var item in model.Gifts)
                {
                    if (item.Counts < 1)
                    {
                        throw new HimallException("错误的兑换数量！");
                    }
                    GiftInfo giftdata = Context.GiftInfo.FirstOrDefault(d => d.Id == item.GiftId);
                    if (giftdata != null && giftdata.GetSalesStatus == GiftInfo.GiftSalesStatus.Normal)
                    {
                        if (giftdata.StockQuantity >= item.Counts)
                        {
                            giftdata.StockQuantity = giftdata.StockQuantity - item.Counts;   //先减库存
                            giftdata.RealSales += item.Counts;    //加销量

                            GiftOrderItemInfo gorditem = new GiftOrderItemInfo()
                            {
                                GiftId = giftdata.Id,
                                GiftName = giftdata.GiftName,
                                GiftValue = giftdata.GiftValue,
                                ImagePath = giftdata.ImagePath,
                                OrderId = result.Id,
                                Quantity = item.Counts,
                                SaleIntegral = giftdata.NeedIntegral
                            };
                            result.Himall_GiftOrderItem.Add(gorditem);
                        }
                        else
                        {
                            throw new HimallException("礼品库存不足！");
                        }
                    }
                    else
                    {
                        throw new HimallException("礼品不存在或已失效！");
                    }
                }
                //建立订单
                result.TotalIntegral = result.Himall_GiftOrderItem.Sum(d => d.Quantity * d.SaleIntegral);
                result.OrderStatus = GiftOrderInfo.GiftOrderStatus.WaitDelivery;
                result.OrderDate = DateTime.Now;
                Context.GiftOrderInfo.Add(result);
                Context.SaveChanges();
                //减少积分
                var userdata = Context.UserMemberInfo.FirstOrDefault(d => d.Id == model.CurrentUser.Id);
                DeductionIntegral(userdata, result.Id, (int)result.TotalIntegral);

                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// 积分抵扣
        /// </summary>
        /// <param name="member"></param>
        /// <param name="Id"></param>
        /// <param name="integral"></param>
        private void DeductionIntegral(UserMemberInfo member, long Id, int integral)
        {
            if (integral == 0)
            {
                return;
            }
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            var remark = "礼品订单号:";
            record.TypeId = MemberIntegral.IntegralType.Exchange;
            remark += Id.ToString();
            MemberIntegralRecordAction action = new MemberIntegralRecordAction();
            action.VirtualItemTypeId = MemberIntegral.VirtualItemType.Exchange;
            action.VirtualItemId = Id;
            record.Himall_MemberIntegralRecordAction.Add(action);
            record.ReMark = remark;
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Exchange, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public GiftOrderInfo GetOrder(long orderId)
        {
            GiftOrderInfo result = Context.GiftOrderInfo.FirstOrDefault(d => d.Id == orderId);
            return result;
        }
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public GiftOrderInfo GetOrder(long orderId, long userId)
        {
            GiftOrderInfo result = Context.GiftOrderInfo.FirstOrDefault(d => d.Id == orderId && d.UserId == userId);
            return result;
        }
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ObsoletePageModel<GiftOrderInfo> GetOrders(GiftsOrderQuery query)
        {
            ObsoletePageModel<GiftOrderInfo> result = new ObsoletePageModel<GiftOrderInfo>();

            long sOrderId;
            bool IsNumber = long.TryParse(query.skey, out sOrderId);

            var datasql = Context.GiftOrderInfo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.skey))
            {
                datasql = datasql.Where(d => d.ShipTo.Contains(query.skey) || (d.Id == sOrderId) || d.Himall_GiftOrderItem.Any(di => di.GiftName.Contains(query.skey)));
            }

            if (query.OrderId.HasValue)
            {
                datasql = datasql.Where(d => d.Id == query.OrderId.Value);
            }

            if (query.status.HasValue)
            {
                datasql = datasql.Where(d => d.OrderStatus == query.status.Value);
            }

            if (query.UserId.HasValue)
            {
                datasql = datasql.Where(d => d.UserId == query.UserId.Value);
            }

            var orderby = datasql.GetOrderBy(o => o.OrderByDescending(d => d.Id));
            //排序
            switch (query.Sort)
            {
                default:
                    orderby = datasql.GetOrderBy(o => o.OrderByDescending(d => d.OrderDate).ThenByDescending(d => d.Id));
                    break;
            }

            int total = 0;
            datasql = datasql.GetPage(out total, query.PageNo, query.PageSize, orderby);

            //数据转换
            result.Models = datasql;
            result.Total = total;

            return result;
        }
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<GiftOrderInfo> GetOrders(IEnumerable<long> ids)
        {
            List<GiftOrderInfo> result = Context.GiftOrderInfo.OrderByDescending(d => d.Id).Include(d => d.Himall_GiftOrderItem).Where(d => ids.Contains(d.Id)).ToList();
            return result;
        }
        /// <summary>
        /// 获取订单计数
        /// </summary>
        /// <param name="status"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetOrderCount(GiftOrderInfo.GiftOrderStatus? status, long userId = 0)
        {
            int result = 0;
            var sql = Context.GiftOrderInfo.AsQueryable();
            if (status.HasValue)
            {
                sql = sql.Where(d => d.OrderStatus == status.Value);
            }
            if (userId != 0)
            {
                sql = sql.Where(d => d.UserId == userId);
            }
            try
            {
                result = sql.Count();
            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }
        /// <summary>
        /// 补充用户数据
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public IEnumerable<GiftOrderInfo> OrderAddUserInfo(IEnumerable<GiftOrderInfo> orders)
        {
            if (orders.Count() > 0)
            {
                List<long> uids = orders.Select(d => d.UserId).ToList();
                if (uids.Count > 0)
                {
                    List<UserMemberInfo> userlist = Context.UserMemberInfo.Where(d => uids.Contains(d.Id)).ToList();
                    if (userlist.Count > 0)
                    {
                        foreach (var item in orders)
                        {
                            UserMemberInfo umdata = userlist.FirstOrDefault(d => d.Id == item.UserId);
                            item.UserName = (umdata == null ? "" : umdata.UserName);
                        }
                    }
                }
            }
            return orders;
        }
        /// <summary>
        /// 获取订单项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GiftOrderItemInfo GetOrderItemById(long id)
        {
            GiftOrderItemInfo result = Context.GiftOrderItemInfo.FirstOrDefault(d => d.Id == id);
            return result;
        }
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="closeReason"></param>
        public void CloseOrder(long id, string closeReason)
        {

        }
        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shipCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        public void SendGood(long id, string shipCompanyName, string shipOrderNumber)
        {
            GiftOrderInfo orderdata = GetOrder(id);
            if (string.IsNullOrWhiteSpace(shipCompanyName) || string.IsNullOrWhiteSpace(shipOrderNumber))
            {
                throw new HimallException("请填写快递公司与快递单号");
            }
            if (orderdata == null)
            {
                throw new HimallException("错误的订单编号");
            }
            if (orderdata.OrderStatus != GiftOrderInfo.GiftOrderStatus.WaitDelivery)
            {
                throw new HimallException("订单状态有误，不可重复发货");
            }
            orderdata.ExpressCompanyName = shipCompanyName;
            orderdata.ShipOrderNumber = shipOrderNumber;
            orderdata.ShippingDate = DateTime.Now;
            orderdata.OrderStatus = GiftOrderInfo.GiftOrderStatus.WaitReceiving;
            Context.SaveChanges();
        }
        /// <summary>
        /// 确认订单到货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        public void ConfirmOrder(long id, long userId)
        {
            var order = Context.GiftOrderInfo.FirstOrDefault(a => a.UserId == userId && a.Id == id && a.OrderStatus == GiftOrderInfo.GiftOrderStatus.WaitReceiving);
            if (order == null)
            {
                throw new HimallException("错误的订单编号，或订单状态不对！");
            }
            //Log.Debug("orderid=" + o.Id.ToString());
            order.OrderStatus = GiftOrderInfo.GiftOrderStatus.Finish;
            order.FinishDate = DateTime.Now;
            Context.SaveChanges();
        }
        /// <summary>
        /// 过期自动确认订单到货
        /// </summary>
        public void AutoConfirmOrder()
        {
            try
            {
                //  var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
                //windows服务调用此处不报错
                var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();
                //退换货间隔天数
                int intIntervalDay = siteSetting == null ? 7 : (siteSetting.NoReceivingTimeout == 0 ? 7 : siteSetting.NoReceivingTimeout);
                DateTime waitReceivingDate = DateTime.Now.AddDays(-intIntervalDay);
                var orders = Context.GiftOrderInfo.Where(a => a.ShippingDate < waitReceivingDate && a.OrderStatus == GiftOrderInfo.GiftOrderStatus.WaitReceiving).ToList();
                foreach (var o in orders)
                {
                    //Log.Debug("orderid=" + o.Id.ToString());
                    o.OrderStatus = GiftOrderInfo.GiftOrderStatus.Finish;
                    o.CloseReason = "完成过期未确认收货的礼品订单";
                    o.FinishDate = DateTime.Now;
                }
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("AutoConfirmGiftOrder:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }
        /// <summary>
        /// 已购买数量
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="giftid"></param>
        /// <returns></returns>
        public int GetOwnBuyQuantity(long userid, long giftid)
        {
            int result = 0;
            try
            {
                result = Context.GiftOrderItemInfo.Where(d => d.GiftId == giftid && d.Himall_GiftsOrder.UserId == userid).Sum(d => d.Quantity);
            }
            catch (Exception ex)
            {
                //无订单项时异常处理
                result = 0;
            }
            return result;
        }

        #region 生成订单号
        private static object obj = new object();
        /// <summary>
        ///  生成订单号
        ///  <para>所有礼品订单号以1开头</para>
        /// </summary>
        public long GenerateOrderNumber()
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 4; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse("1" + DateTime.Now.ToString("yyyyMMddfff") + orderId);
            }
        }
        #endregion
    }
}
