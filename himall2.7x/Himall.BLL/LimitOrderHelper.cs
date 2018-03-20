using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using Himall.Model;
using Himall.IServices;
using System.Configuration;

namespace Himall.BLL
{
    /// <summary>
    /// 限时购订单处理类
    /// </summary>
    public class LimitOrderHelper
    {
        /// <summary>
        /// 库存锁
        /// </summary>
        private static Dictionary<string, object> _locker;
        /// <summary>
        /// 数据库锁
        /// </summary>
        public static object _databaselocker = new object();
        /// <summary>
        /// 队列锁
        /// </summary>
        public static object _quenelocker = new object();
        /// <summary>
        /// 限时购销售计数器
        /// </summary>
        private const string LIMIT_COUNT_TAG = "limitcount:";
        /// <summary>
        /// 限时购缓存订单订阅处理标识
        /// </summary>
        private const string LIMIT_LIST_TAG = "limitsub:";
        /// <summary>
        /// 限时购总库存
        /// </summary>
        private const string LIMIT_TOTAL_TAG = "limittotal:";
        /// <summary>
        /// 订单队列
        /// </summary>
        private const string LIMIT_QUENE_TAG = "limitquene:all";

        private static IOrderService iorder = Himall.ServiceProvider.Instance<IOrderService>.Create;
        private static ILimitTimeBuyService _iLimitTimeBuy = Himall.ServiceProvider.Instance<ILimitTimeBuyService>.Create;
        /// <summary>
        /// 限时购库存缓存过期时间=限时购商品过期时间+CACHE_OVER_DAY
        /// </summary>
        private const int CACHE_OVER_DAY = 1;//单位天

        /// <summary>
        /// 线程休息时间
        /// </summary>
        private const int TREADSLEEP = 1000 * 30;

        /// <summary>
        /// 是否是Redis缓存
        /// </summary>
        /// <returns></returns>
        public bool IsRedisCache()
        {
            if (Cache.GetCache().GetType().FullName =="Himall.Strategy.Redis")
                return true;
            else
                return false;
        }
        /// <summary>
        /// 服务器启动限时购数据缓存初始化
        /// </summary>
        public void InitLimitOrder()
        {
            if (!IsRedisCache())
                return;
            var conf = ConfigurationManager.AppSettings["IsInstalled"];
            if (!((null == conf || bool.Parse(conf))))
                return;
            _locker = new Dictionary<string, object>();
            {
                var data = _iLimitTimeBuy.GetAllStartData();
                foreach (var t in data)//初始化缓存
                {
                    if (!_locker.Keys.Contains(t.SkuId))
                        _locker.Add(t.SkuId, new object());
                    Cache.Insert(LIMIT_TOTAL_TAG + t.SkuId, t.Stock, t.EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
                    Cache.Insert(LIMIT_COUNT_TAG + t.SkuId, 0, t.EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
                    Cache.RegisterSubscribe<OrderIdentity>(LIMIT_LIST_TAG + t.SkuId, DisposeOrder);
                }
                List<OrderIdentity> all = null;
                if (!Cache.Exists(LIMIT_QUENE_TAG))//不存在订单缓存则创建空订单缓存
                {
                    all = new List<OrderIdentity>();
                    Cache.Insert<List<OrderIdentity>>(LIMIT_QUENE_TAG, all);
                }
                else
                {
                    all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
                    if (all != null)
                    {
                        for (int i = 0; i < all.Count(); i++)//处理上次未入库订单
                        {
                            if (all[i].State == OrderState.Untreated)
                            {
                                try
                                { ///将缓存订单入库从缓存中删除，并更新限时购库存
                                    var orders = iorder.CreateOrder(all[i].Order);
                                    var orderIds = orders.Select(item => item.Id).ToArray();
                                    if (orderIds.Count() >= 1)
                                    {
                                        int sellcount = int.Parse(Cache.Get(LIMIT_COUNT_TAG + all[i].Order.SkuIds.ElementAt(0)).ToString());
                                        sellcount++;
                                        Cache.Insert(LIMIT_COUNT_TAG + all[i].Order.SkuIds.ElementAt(0).ToString(), sellcount);
                                    }
                                    all.Remove(all[i]);
                                    i--;
                                }
                                catch//异常订单直接丢弃
                                {
                                    all.Remove(all[i]);
                                    i--;
                                    continue;
                                }
                            }
                            else if (all[i].State == OrderState.Processed || all[i].State == OrderState.Fail || all[i].State == OrderState.Exception)//失败或已处理订单
                            {
                                all.Remove(all[i]);
                                i--;
                            }
                        }
                        Cache.Insert(LIMIT_QUENE_TAG, all);
                    }
                }
            }
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="id">返回缓存订单标识</param>
        /// <param name="order">订单数据实体</param>
        /// <returns>下单结果</returns>
        public SubmitOrderResult SubmitOrder(OrderCreateModel order, out string id)
        {
            id = "";
            if (order == null)
                return SubmitOrderResult.NoData;
            else if (order.SkuIds == null || order.SkuIds.Count() == 0 || string.IsNullOrEmpty(order.SkuIds.ElementAt(0)))
                return SubmitOrderResult.NoSkuId;
            if (!_locker.Keys.Contains(order.SkuIds.ElementAt(0)))
                return SubmitOrderResult.NoLimit;
            string skuid = order.SkuIds.ElementAt(0);
            lock (_locker[skuid])//锁库存
            {
                int sellcount = int.Parse(Cache.Get(LIMIT_COUNT_TAG + skuid).ToString());
                int total = int.Parse(Cache.Get(LIMIT_TOTAL_TAG + skuid).ToString());
                int buy = order.Counts.ElementAt(0);
                if (sellcount + buy > total)//判断是否超卖
                    return SubmitOrderResult.SoldOut;
                sellcount = sellcount + buy;
                Cache.Insert(LIMIT_COUNT_TAG + skuid, sellcount);//更新库存
                OrderIdentity myorder = new OrderIdentity();//给订单加标识
                myorder.Id = Guid.NewGuid().ToString();
                myorder.Order = order;
                myorder.State = OrderState.Untreated;
                myorder.Message = "订单正在处理";
                id = myorder.Id;
                AddOrder(myorder);
                Cache.Send(LIMIT_LIST_TAG + skuid, myorder);//发消息后台处理
                return SubmitOrderResult.Success;
            }
        }


        /// <summary>
        /// 获得订单状态
        /// </summary>
        /// <param name="id">缓存订单标识</param>
        /// <param name="message">返回缓存订单相关消息</param>
        /// <param name="orderids">处理成功时返回实际订单标识</param>
        /// <returns>缓存订单状态</returns>
        public  OrderState GetOrderState(string id, out string message, out long[] orderids)
        {
            message = "";
            orderids = null;
            List<OrderIdentity> all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
            OrderIdentity o = all.FirstOrDefault(t => t.Id == id);
            if (o != null)
            {
                if (o.State == OrderState.Processed)
                {
                    orderids = o.OrderIds;
                    message = o.Message;
                    RemoveOrder(o.Id);
                }
                else if (o.State == OrderState.Fail)
                {
                    message = o.Message;
                    RemoveOrder(o.Id);
                }
                else if (o.State == OrderState.Untreated)
                {
                    message = o.Message;
                }
                return o.State;
            }
            else
            {
                message = "订单失踪,服务器异常";
                return OrderState.Exception;
            }

        }

        /// <summary>
        /// 释放缓存库存
        /// </summary>
        /// <param name="skuid">库存标识</param>
        /// <param name="Quantity">释放库存数量</param>
        public void ReleaseStock(string skuid, long Quantity)
        {
            if (Cache.Exists(LIMIT_COUNT_TAG + skuid))
            {
                if (!_locker.Keys.Contains(skuid))
                {
                    _locker.Add(skuid, new object());
                }
                lock (_locker[skuid])//锁库存
                {
                    long sellcount = long.Parse(Cache.Get(LIMIT_COUNT_TAG + skuid).ToString());
                    if (sellcount == 0)
                    {
                        long stock = long.Parse(Cache.Get(LIMIT_TOTAL_TAG + skuid).ToString());
                        stock = stock + 1;
                        Cache.Insert(LIMIT_TOTAL_TAG + skuid, stock);
                    }
                    else if (sellcount > 0)
                    {
                        sellcount = sellcount - Quantity;
                        Cache.Insert(LIMIT_COUNT_TAG + skuid, sellcount);
                    }
                }
            }
        }


        /// <summary>
        /// 修改缓存限时购库存数据量
        /// </summary>
        /// <param name="skuid">库存标识</param>
        /// <param name="stock">库存数量</param>
        /// <param name="EndDate">该库存活动结束时间</param>
        /// <returns>返回处理结果</returns>
        public ModifyLimiyStockResult ModifyLimitStock(string skuid, int stock, DateTime EndDate)
        {
            if (!IsRedisCache())
                return ModifyLimiyStockResult.Sucess;
            if (!_locker.Keys.Contains(skuid))
            {
                AddLimitStock(skuid, stock, EndDate);
                return ModifyLimiyStockResult.Sucess;
            }
            else
            {
                lock (_locker[skuid])//锁库存
                {

                    int sellcount = int.Parse(Cache.Get(LIMIT_COUNT_TAG + skuid).ToString());
                    int total = int.Parse(Cache.Get(LIMIT_TOTAL_TAG + skuid).ToString());
                    if (sellcount < stock)
                    {
                        Cache.Insert(LIMIT_TOTAL_TAG + skuid, stock, EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
                        Cache.Insert(LIMIT_COUNT_TAG + skuid, sellcount, EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
                        return ModifyLimiyStockResult.Sucess;
                    }
                    else
                        return ModifyLimiyStockResult.OutStock;
                }
            }
        }

        /// <summary>
        /// 新增限时购库存
        /// </summary>
        /// <param name="skuid">库存标识</param>
        /// <param name="stock">库存数量</param>
        /// <param name="EndDate">活动结束时间</param>
        public void AddLimitStock(string skuid, int stock, DateTime EndDate)
        {
            if (!IsRedisCache())
                return;
            if (_locker == null)
                _locker = new Dictionary<string, object>();
            if (!_locker.Keys.Contains(skuid))
                _locker.Add(skuid, new object());
            //if (!Cache.Exists(LIMIT_TOTAL_TAG + skuid))//总库存
            Cache.Insert(LIMIT_TOTAL_TAG + skuid, stock, EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
            //if (!Cache.Exists(LIMIT_COUNT_TAG + skuid))//已售
            Cache.Insert(LIMIT_COUNT_TAG + skuid, 0, EndDate.Add(TimeSpan.FromDays(CACHE_OVER_DAY)));
            //if (!Cache.Exists(LIMIT_LIST_TAG + skuid))//订单处理
            Cache.RegisterSubscribe<OrderIdentity>(LIMIT_LIST_TAG + skuid, DisposeOrder);
        }
        #region 订单队列操作
        static void RemoveOrder(string id)
        {
            lock (_quenelocker)
            {
                List<OrderIdentity> all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
                if (all == null)
                    return;
                OrderIdentity order = all.FirstOrDefault(t => t.Id == id);
                if (order != null)
                {
                    all.Remove(order);
                    Cache.Insert<List<OrderIdentity>>(LIMIT_QUENE_TAG, all);
                }
            }
        }

        static void AddOrder(OrderIdentity order)
        {
            lock (_quenelocker)
            {
                List<OrderIdentity> all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
                if (all == null)
                {
                    all = new List<OrderIdentity>();
                }
                if (order != null)
                {
                    all.Add(order);
                    Cache.Insert<List<OrderIdentity>>(LIMIT_QUENE_TAG, all);
                }
            }
        }

        static void UpdateOrder(OrderIdentity order)
        {
            lock (_quenelocker)
            {
                List<OrderIdentity> all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
                if (all == null)
                    return;
                if (order != null)
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        if (all[i].Id == order.Id)
                        {
                            all[i] = order;
                        }
                    }
                    Cache.Insert<List<OrderIdentity>>(LIMIT_QUENE_TAG, all);
                }
            }
        }
        #endregion

        /// <summary>
        /// 消耗定单
        /// </summary>
        static void DisposeOrder(object o)
        {
            OrderIdentity order = o as OrderIdentity;
            List<OrderIdentity> all = Cache.Get<List<OrderIdentity>>(LIMIT_QUENE_TAG);
            OrderIdentity oi = all.FirstOrDefault(t => t.Id == order.Id);
            if (oi.State == OrderState.Untreated)
            {
                try
                {
                    //数据库创建订单
                    lock (_databaselocker)//锁数据库操作
                    {
                        var orders = iorder.CreateOrder(order.Order);
                        var orderIds = orders.Select(item => item.Id).ToArray();
                        oi.State = OrderState.Processed;
                        oi.Message = "下单成功!";
                        oi.OrderIds = orderIds;
                        UpdateOrder(oi);
                    }
                }
                catch (Exception e)//数据库创建订单失败
                {
                    lock (_locker[order.Order.SkuIds.ElementAt(0)])//锁库存
                    {
                        int sellcount = int.Parse(Cache.Get(LIMIT_COUNT_TAG + order.Order.SkuIds.ElementAt(0)).ToString());
                        sellcount = sellcount - order.Order.Counts.ElementAt(0);
                        Cache.Insert(LIMIT_COUNT_TAG + order.Order.SkuIds.ElementAt(0), sellcount);
                        oi.State = OrderState.Fail;
                        oi.Message = "下单失败:" + e.Message;
                        UpdateOrder(oi);
                    }
                }
            }
        }

        /// <summary>
        /// 订单标识
        /// </summary>
        class OrderIdentity
        {
            /// <summary>
            /// 订单标识
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// 订单数据
            /// </summary>
            public OrderCreateModel Order { get; set; }
            /// <summary>
            /// 订单状态
            /// </summary>
            public OrderState State { get; set; }
            /// <summary>
            /// 订单编号
            /// </summary>
            public long[] OrderIds { get; set; }
            /// <summary>
            /// 订单消息
            /// </summary>
            public String Message { get; set; }
        }

    }

    #region 枚举
    /// <summary>
    /// 下单结果
    /// </summary>
    public enum SubmitOrderResult
    {
        /// <summary>
        /// 没有数据
        /// </summary>
        NoData,
        /// <summary>
        /// 无SKUID
        /// </summary>
        NoSkuId,
        /// <summary>
        /// 卖光
        /// </summary>
        SoldOut,
        /// <summary>
        /// 没有限时购活动
        /// </summary>
        NoLimit,
        /// <summary>
        /// 下单成功
        /// </summary>
        Success
    }

    /// <summary>
    /// 修改限时购结果
    /// </summary>
    public enum ModifyLimiyStockResult
    {
        /// <summary>
        /// 卖出超出库存修改
        /// </summary>
        OutStock,
        /// <summary>
        /// 修改成功
        /// </summary>
        Sucess
    }

    /// <summary>
    /// 订单处理状态
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// 处理成功
        /// </summary>
        Processed,
        /// <summary>
        /// 处理失败
        /// </summary>
        Fail,
        /// <summary>
        /// 未处理
        /// </summary>
        Untreated,
        /// <summary>
        /// 异常
        /// </summary>
        Exception
    }
    #endregion

}