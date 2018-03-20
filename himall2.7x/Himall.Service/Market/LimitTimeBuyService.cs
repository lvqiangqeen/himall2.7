using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Entity;
using Himall.Core;
using System.Data.Entity;
using Quartz;
using Himall.Core.Plugins.Message;
using Himall.Model.Models;
using Himall.CommonModel;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    /// <summary>
    /// 限时购服务实现
    /// </summary>
    public class LimitTimeBuyService : ServiceBase, ILimitTimeBuyService
    {
        #region 平台

        /// <summary>
        /// 设置活动参数
        /// </summary>
        public void UpdateConfig(FlashSaleConfigModel data)
        {
            var model = Context.FlashSaleConfigInfo.FirstOrDefault();
            if (model == null)
            {
                model = new FlashSaleConfigInfo();
                model.IsNormalPurchase = data.IsNormalPurchase;
                model.Preheat = data.Preheat;
                Context.FlashSaleConfigInfo.Add(model);
            }
            else
            {
                model.IsNormalPurchase = data.IsNormalPurchase;
                model.Preheat = data.Preheat;
            }
            Context.SaveChanges();
            string cacheKey = CacheKeyCollection.CACH_FLASHSALECONFIG;
            Cache.Remove(cacheKey);
        }


        /// <summary>
        /// 删除
        /// </summary>
        public void Delete(long id, long shopId)
        {
            var m = Context.FlashSaleInfo.Where(a => a.Id == id && a.ShopId == shopId).FirstOrDefault();
            if (m != null)
            {
                Context.FlashSaleInfo.Remove(m);
                var list = Context.FlashSaleDetailInfo.Where(a => a.FlashSaleId == m.Id).ToList();
                Context.FlashSaleDetailInfo.RemoveRange(list);
                Context.SaveChanges();
            }
        }

        public FlashSaleConfigModel GetConfig()
        {
            string cacheKey = CacheKeyCollection.CACH_FLASHSALECONFIG;
            if (Cache.Exists(cacheKey))
                return Cache.Get<FlashSaleConfigModel>(cacheKey);

            var model = Context.FlashSaleConfigInfo.FirstOrDefault();
            if (model == null)
            {
                model = new FlashSaleConfigInfo();
                model.IsNormalPurchase = true;
                model.Preheat = 24;
                model = Context.FlashSaleConfigInfo.Add(model);
                Context.SaveChanges();
            }
            var result = new FlashSaleConfigModel(model.Preheat, model.IsNormalPurchase);
            Cache.Insert<FlashSaleConfigModel>(cacheKey, result, 600);
            return result;
        }

        /// <summary>
        /// 审核限时购活动
        /// </summary>
        /// <param name="Id">限时购ID</param>
        /// <param name="status">审核状态</param>
        /// <param name="message">备注</param>
        public void AuditItem(long Id, LimitTimeMarketInfo.LimitTimeMarketAuditStatus status, string message)
        {
            var market = Context.LimitTimeMarketInfo.FindById(Id);
            market.AuditStatus = status;
            market.CancelReson = message ?? "";
            market.AuditTime = DateTime.Now;
            Context.SaveChanges();
        }

        /// <summary>
        /// 更新限时购服务设置
        /// </summary>
        public void UpdateServiceSetting(LimitTimeBuySettingModel model)
        {
            var market = Context.MarketSettingInfo.FirstOrDefault(m => m.TypeId == MarketType.LimitTimeBuy);
            if (market != null && market.Id != 0)
            {
                market.Price = model.Price;
            }
            else
            {
                MarketSettingInfo info = new MarketSettingInfo()
                {
                    Price = model.Price,
                    TypeId = MarketType.LimitTimeBuy
                };
                Context.MarketSettingInfo.Add(info);
            }
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取限时购服务设置
        /// </summary>
        /// <returns></returns>
        public LimitTimeBuySettingModel GetServiceSetting()
        {
            int days = 0;
            var result = (from m in Context.MarketSettingInfo
                          where m.TypeId == MarketType.LimitTimeBuy
                          select m).FirstOrDefault();
            if (result == null || result.Id == 0)
                return null;
            ;
            var model = new LimitTimeBuySettingModel() { Price = result.Price, ReviceDays = 0 };
            if (result.Himall_MarketSettingMeta.Count() != 0)
            {
                var marketMeta = result.Himall_MarketSettingMeta.FirstOrDefault(m => m.MetaKey.ToLower().Equals("revicedays"));
                if (marketMeta != null)
                {
                    var ReviceDays = marketMeta.MetaValue;
                    if (int.TryParse(ReviceDays, out days)) { }
                }
                model.ReviceDays = days;
            }
            return model;
        }

        /// <summary>
        /// 更新限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        public void AddServiceCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new HimallException("分类不能为空，添加失败.");
            }
            var market = (from m in Context.MarketSettingInfo
                          where m.TypeId == MarketType.LimitTimeBuy
                          select m).FirstOrDefault();
            if (market != null && market.Id != 0)
            {
                if (market.Himall_MarketSettingMeta.Count() == 0)
                    market.Himall_MarketSettingMeta = new List<MarketSettingMetaInfo>();

                var meta = market.Himall_MarketSettingMeta.
                        FirstOrDefault(m => m.MarketId == market.Id && m.MetaKey.ToLower().Equals("categories"));


                if (meta == null || meta.Id == 0)
                {
                    market.Himall_MarketSettingMeta.Add(
                            new MarketSettingMetaInfo
                            {
                                MetaKey = "Categories",
                                MetaValue = categoryName,
                                MarketId = market.Id
                            }
                        );
                }
                else
                {
                    if (meta.MetaValue.Split(',').Any(c => c.Equals(categoryName)))
                    {
                        throw new HimallException("添加的限时购分类已经存在，添加失败.");
                    }
                    else
                    {
                        meta.MetaValue += string.Format(",{0}", categoryName);
                    }

                }
            }
            Context.SaveChanges();
        }

        /// <summary>
        /// 删除一个限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        public void DeleteServiceCategory(string categoryName)
        {
            //if( string.IsNullOrWhiteSpace( categoryName ) )
            //{
            //    throw new HimallException( "分类不能为空，添加失败." );
            //}
            var market = (from m in Context.MarketSettingInfo
                          where m.TypeId == MarketType.LimitTimeBuy
                          select m).FirstOrDefault();

            if (Context.FlashSaleInfo.Any(m =>
                m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate < DateTime.Now && m.EndDate > DateTime.Now
                || m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate < DateTime.Now
                || m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing && m.EndDate > DateTime.Now))
            {
                throw new HimallException("该分类不能被删除，有未审核或者其他正在进行中的限时购.");
            }
            if (market != null && market.Id != 0)
            {
                var cate = market.Himall_MarketSettingMeta.FirstOrDefault
                    (m => m.MarketId == market.Id && m.MetaKey.ToLower().Equals("categories"));
                if (cate != null && cate.Id != 0)
                {
                    var cateList = cate.MetaValue.Split(',').ToList();
                    if (cateList.Count == 1)
                    {
                        throw new HimallException("分类不能少于一条.");
                    }
                    cateList.Remove(categoryName);
                    cate.MetaValue = string.Join(",", cateList);

                }
            }
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取限时购服务分类
        /// </summary>
        /// <returns></returns>
        public string[] GetServiceCategories()
        {
            var result = new List<string>();
            var market = (from m in Context.MarketSettingInfo
                          where m.TypeId == MarketType.LimitTimeBuy
                          select m).FirstOrDefault();
            if (market != null && market.Id != 0)
            {
                var cate = market.Himall_MarketSettingMeta.FirstOrDefault
                    (m => m.MarketId == market.Id && m.MetaKey.ToLower().Equals("categories"));
                if (cate != null && cate.Id != 0)
                {
                    result = cate.MetaValue.Split(',').ToList();
                }
            }
            return result.ToArray();
        }


        /// <summary>
        /// 获取指定营销类型服务的已购买商家列表
        /// </summary>
        /// <param name="MarketBoughtQuery">营销查询对象</param>
        /// <returns></returns>
        public ObsoletePageModel<ActiveMarketServiceInfo> GetBoughtShopList(MarketBoughtQuery query)
        {
            IQueryable<ActiveMarketServiceInfo> markets = Context.ActiveMarketServiceInfo.AsQueryable();
            if (query.MarketType.HasValue)
            {
                markets = markets.Where(d => d.TypeId == query.MarketType);
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                markets = markets.Where(d => d.ShopName.Contains(query.ShopName));
            }
            int total = 0;
            markets = markets.GetPage(out total, d => d.OrderBy(o => o.MarketServiceRecordInfo.Max(m => m.EndTime)), query.PageNo, query.PageSize);
            ObsoletePageModel<ActiveMarketServiceInfo> pageModel = new ObsoletePageModel<ActiveMarketServiceInfo>() { Models = markets, Total = total };
            return pageModel;
        }

        /// <summary>
        /// 获取参加限时购的所有活动商品列表
        /// </summary>
        /// <param name="query">限时购活动查询对象</param>
        /// <returns></returns>
        public ObsoletePageModel<LimitTimeMarketInfo> GetItemList(LimitTimeQuery query)
        {
            IQueryable<LimitTimeMarketInfo> limit = Context.LimitTimeMarketInfo.AsQueryable();
            if (query.OrderType != 0)
            {
                limit = limit.Where(item => item.EndTime > DateTime.Now);
            }
            if (query.ShopId.HasValue)
            {
                limit = limit.Where(item => query.ShopId == item.ShopId);
            }
            if (!string.IsNullOrWhiteSpace(query.ItemName))
            {
                limit = limit.Where(item => item.ProductName.Contains(query.ItemName));
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                limit = limit.Where(item => item.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                limit = limit.FindBy(d => d.CategoryName == query.CategoryName);
            }
            var Date = DateTime.Now;
            if (query.AuditStatus.HasValue)
            {
                if (query.AuditStatus.Value == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ended)
                {
                    limit = limit.Where(a => a.EndTime < Date && (a.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing || a.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.WaitForAuditing));
                }
                else if (query.AuditStatus.Value == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing)
                {
                    limit = limit.Where(a => a.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing && a.EndTime > Date);
                }
                else if (query.AuditStatus.Value == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.WaitForAuditing)
                {
                    limit = limit.Where(a => a.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.WaitForAuditing && a.EndTime > Date);
                }
                else
                {
                    limit = limit.Where(a => a.AuditStatus == query.AuditStatus.Value);
                }

            }
            int total = 0;
            limit = from l in limit
                    join p in Context.ProductInfo on l.ProductId equals p.Id
                    where p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && ((!query.CheckProductStatus) || (query.CheckProductStatus && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale))
                    select l;

            //日龙添加（判断分页超出）
            if (query.PageSize == 0)
            {
                query.PageSize = 10;
            }
            if (limit.Count() / query.PageSize < query.PageNo - 1)
            {
                query.PageNo = 1;
            }

            //end

            var orderby = limit.GetOrderBy(d => d.OrderByDescending(item => item.Id));
            switch (query.OrderKey)
            {
                case 2:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.SaleCount));
                    break;
                case 3:
                    if (query.OrderType == 2)
                        orderby = limit.GetOrderBy(d => d.OrderBy(item => item.Price));
                    else
                        orderby = limit.GetOrderBy(d => d.OrderByDescending(item => item.Price));
                    break;
                case 4:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.EndTime));
                    break;
                case 5:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.AuditStatus));
                    break;
                default:
                    orderby = limit.GetOrderBy(d => d.OrderByDescending(item => item.Id));
                    break;
            }

            limit = limit.GetPage(out total, orderby, query.PageNo, query.PageSize);
            //
            ObsoletePageModel<LimitTimeMarketInfo> pageModel = new ObsoletePageModel<LimitTimeMarketInfo>() { Models = limit, Total = total };
            return pageModel;
        }

        #endregion

        #region 商家
        private void CheckLimit(LimitTimeMarketInfo model)
        {
            if (Context.LimitTimeMarketInfo.Any(m => m.Id != model.Id && m.ShopId == model.ShopId && m.ProductId == model.ProductId && m.EndTime > DateTime.Now &&
              (m.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing || m.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.WaitForAuditing)))
            {
                throw new HimallException(string.Format("操作失败，限时购活动：{0} 已经存在.", model.ProductName));
            }
            var co = Context.ActiveMarketServiceInfo.FirstOrDefault(a => a.TypeId == MarketType.LimitTimeBuy && a.ShopId == model.ShopId);
            if (co == null)
            {
                throw new HimallException("您没有订购此服务");
            }
            if (co.MarketServiceRecordInfo.Max(item => item.EndTime.Date) < model.EndTime)
            {
                throw new HimallException("结束日期不能超过购买限时购服务的日期");
            }
        }

        public void UpdateLimitTimeItem(LimitTimeMarketInfo model)
        {
            CheckLimit(model);
            var item = Context.LimitTimeMarketInfo.FindById(model.Id);
            item.Title = model.Title;
            item.ProductId = model.ProductId;
            item.ProductName = model.ProductName;
            item.CategoryName = model.CategoryName;
            item.StartTime = model.StartTime;
            item.EndTime = model.EndTime;
            item.Price = model.Price;
            item.Stock = model.Stock;
            item.MaxSaleCount = model.MaxSaleCount;
            Context.SaveChanges();
        }

        /// <summary>
        /// 添加一个限时购活动
        /// </summary>
        /// <param name="model">限时购对象</param>
        public void AddLimitTimeItem(LimitTimeMarketInfo model)
        {
            CheckLimit(model);
            Context.LimitTimeMarketInfo.Add(model);
            Context.SaveChanges();
        }

        /// <summary>
        /// 根据店铺Id获取该店铺购买的限时购营销服务信息
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public ActiveMarketServiceInfo GetMarketService(long shopId)
        {
            if (shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }
            var market = Context.ActiveMarketServiceInfo.FirstOrDefault(m => m.ShopId == shopId && m.TypeId == MarketType.LimitTimeBuy);
            return market;
        }

        /// <summary>
        /// 为指定的店铺开通限时购服务
        /// </summary>
        /// <param name="monthCount">时长（以月为单位）</param>
        /// <param name="shopId">店铺Id</param>
        public void EnableMarketService(int monthCount, long shopId)
        {
            if (shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }
            if (monthCount <= 0)
            {
                throw new HimallException("购买服务时长必须大于零");
            }

            var shop = Context.ShopInfo.FindById(shopId);
            if (shop == null || shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }

            if (Context.ActiveMarketServiceInfo.Any(a => a.ShopId == shopId && a.TypeId == MarketType.LimitTimeBuy))
            {
                var market = Context.ActiveMarketServiceInfo.FirstOrDefault(a => a.ShopId == shopId && a.TypeId == MarketType.LimitTimeBuy);
                AddMarketServiceRecord(market.Id, DateTime.Now.Date, DateTime.Now.Date.AddMonths(monthCount));
            }
            else
            {
                Context.ActiveMarketServiceInfo.Add(new ActiveMarketServiceInfo
                {
                    ShopId = shopId,
                    ShopName = shop.ShopName,
                    TypeId = MarketType.LimitTimeBuy,
                    MarketServiceRecordInfo = new List<MarketServiceRecordInfo> {
                        new MarketServiceRecordInfo{
                        StartTime = DateTime.Now.Date,
                        EndTime = DateTime.Now.Date.AddMonths(monthCount)
                        }
                    }
                });
            }
            Context.SaveChanges();
        }

        private void AddMarketServiceRecord(long msId, DateTime start, DateTime end)
        {
            Context.MarketServiceRecordInfo.Add(new MarketServiceRecordInfo
            {
                MarketServiceId = msId,
                EndTime = end,
                StartTime = start
            });
        }

        #endregion

        #region 前台

        /// <summary>
        /// 获取一个限时购的详细信息
        /// </summary>
        /// <param name="id">限时购活动Id</param>
        public LimitTimeMarketInfo GetLimitTimeMarketItem(long id)
        {
            if (id <= 0)
            {
                throw new HimallException("限时购活动Id不能识别");
            }

            var item = Context.LimitTimeMarketInfo.FindById(id);
            return item;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsLimitTimeMarketItem(long id)
        {
            if (id <= 0)
            {
                return false;
            }

            var item = Context.FlashSaleInfo.Any(p => p.ProductId == id
                && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing
                && p.BeginDate <= DateTime.Now
                && p.EndDate > DateTime.Now);
            return item;
        }

        /// <summary>
        /// 根据商品Id获取一个限时购的详细信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public FlashSaleInfo GetLimitTimeMarketItemByProductId(long pid)
        {
            if (pid <= 0)
            {
                throw new HimallException("商品Id不能识别");
            }
            FlashSaleInfo result = null;
            var now = DateTime.Now;
            string sql = "SELECT * FROM himall_flashsale WHERE ProductId=@ProductId AND Status=@Status AND BeginDate<=@BeginDate AND EndDate > @EndDate";
            using (var conn = new MySqlConnection(Connection.ConnectionString))
            {
                result = conn.QueryFirstOrDefault<FlashSaleInfo>(sql, new { ProductId = pid, Status = FlashSaleInfo.FlashSaleStatus.Ongoing, BeginDate = now, EndDate = now });
            }
            return result;
        }

        #endregion


        #region 新版本限时购

        public ObsoletePageModel<FlashSaleInfo> GetAll(FlashSaleQuery query)
        {
            ExpiredDataChangeStatus();
            IQueryable<FlashSaleInfo> limit = Context.FlashSaleInfo.AsQueryable();

            var config = GetConfig();

            if (query.IsStart == 1)//已开始
            {
                limit = limit.Where(p => p.BeginDate < DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing);

            }
            else if (query.IsStart == 2)  //即将开始 
            {
                var date = DateTime.Now.AddHours(config.Preheat);
                limit = limit.Where(p => p.BeginDate < date && p.BeginDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing);
            }
            else
            {
                if (query.IsPreheat)
                {
                    if (config.Preheat > 0)
                    {
                        var date = DateTime.Now.AddHours(config.Preheat);
                        limit = limit.Where(p => p.BeginDate < date);
                    }
                }
            }

            if (query.OrderType != 0)
            {
                limit = limit.Where(item => item.EndDate > DateTime.Now);
            }
            if (query.ShopId.HasValue)
            {
                limit = limit.Where(item => query.ShopId == item.ShopId);
            }
            if (!string.IsNullOrWhiteSpace(query.ItemName))
            {
                limit = limit.Where(item => item.Himall_Products.ProductName.Contains(query.ItemName));
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                limit = limit.Where(item => item.Himall_Shops.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                limit = limit.FindBy(d => d.CategoryName == query.CategoryName);
            }
            var Date = DateTime.Now;
            if (query.AuditStatus.HasValue)
            {
                var status = query.AuditStatus;
                FlashSaleInfo.FlashSaleStatus s = (FlashSaleInfo.FlashSaleStatus)status;
                limit = limit.Where(p => p.Status == (FlashSaleInfo.FlashSaleStatus)status);
            }
            int total = 0;
            limit = from l in limit
                    join p in Context.ProductInfo on l.ProductId equals p.Id
                    where p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted==false
                    && ((!query.CheckProductStatus) || (query.CheckProductStatus && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale))
                    select l;

            //日龙添加（判断分页超出）
            if (query.PageSize == 0)
            {
                query.PageSize = 10;
            }
            if (limit.Count() / query.PageSize < query.PageNo - 1)
            {
                query.PageNo = 1;
            }

            //end

            var orderby = limit.GetOrderBy(d => d.OrderByDescending(item => item.Id));
            switch (query.OrderKey)
            {
                case 2:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.SaleCount));
                    break;
                //case 3:
                //    if( query.OrderType == 2 )
                //        orderby = limit.GetOrderBy( d => d.OrderBy( item => item.Price ) );
                //    else
                //        orderby = limit.GetOrderBy( d => d.OrderByDescending( item => item.Price ) ); 
                //    break;
                case 4:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.EndDate));
                    break;
                case 5:
                    orderby = limit.GetOrderBy(d => d.OrderBy(item => item.BeginDate));
                    break;
                default:
                    orderby = limit.GetOrderBy(d => d.OrderByDescending(item => item.Id));
                    break;
            }

            limit = limit.GetPage(out total, orderby, query.PageNo, query.PageSize);
            limit = limit.Include(_td => _td.Himall_Products).Include(_td => _td.Himall_Shops);
            ObsoletePageModel<FlashSaleInfo> pageModel = new ObsoletePageModel<FlashSaleInfo>() { Models = limit, Total = total };
            return pageModel;

        }

        /// <summary>
        /// 获取所有正在进行或未开始的限时购活动缓存信息
        /// </summary>
        public List<FlashSaleRedisInfo> GetAllStartData()
        {
            var result = from f in Context.FlashSaleInfo
                         join d in Context.FlashSaleDetailInfo
                         on f.Id equals d.FlashSaleId
                         join s in Context.SKUInfo
                         on d.SkuId equals s.Id
                         where f.EndDate > DateTime.Now
                         select new FlashSaleRedisInfo { SkuId = d.SkuId, FlashSaleId = d.FlashSaleId, Stock = s.Stock, BeginDate = f.BeginDate, EndDate = f.EndDate };
            return result.ToList();
        }
        public ObsoletePageModel<FlashSaleInfo> GetStartData(int index, int size, string cname)
        {
            ExpiredDataChangeStatus();
            IQueryable<FlashSaleInfo> limit = Context.FlashSaleInfo.Where(p => p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing);
            if (!string.IsNullOrEmpty(cname))
            {
                limit = limit.Where(p => p.BeginDate <= DateTime.Now && p.CategoryName == cname);
            }
            int total = 0;
            limit = limit.GetPage(out total, limit.GetOrderBy(p => p.OrderBy(d => d.BeginDate).ThenByDescending(d => d.Id)), index, size);
            ObsoletePageModel<FlashSaleInfo> pageModel = new ObsoletePageModel<FlashSaleInfo>() { Models = limit, Total = total };
            return pageModel;
        }

        ObsoletePageModel<FlashSaleInfo> ILimitTimeBuyService.GetAll(int status, string shopname, string title, int pageIndex, int pageSize)
        {
            ExpiredDataChangeStatus();
            IQueryable<FlashSaleInfo> query = Context.FlashSaleInfo;
            if (!string.IsNullOrEmpty(shopname))
            {
                query = query.Where(p => p.Himall_Shops.ShopName.Contains(shopname));
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (status > 0)
            {
                FlashSaleInfo.FlashSaleStatus s = (FlashSaleInfo.FlashSaleStatus)status;
                //  query = query.Where(p => p.Status == (FlashSaleInfo.FlashSaleStatus)status);
                if (s == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    query = query.Where(p => p.EndDate < DateTime.Now&&p.Status!=FlashSaleInfo.FlashSaleStatus.Cancelled);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                {
                    query = query.Where(p => p.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    query = query.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.AuditFailed)
                {
                    query = query.Where(a => a.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.Cancelled)
                {
                    query = query.Where(a => a.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.NotBegin)
                {
                    query = query.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate > DateTime.Now);
                }
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            int total = 0;
            IQueryable<FlashSaleInfo> datas = query.GetPage(out total, p => p.OrderBy(o => o.Status), pageIndex, pageSize);
            ObsoletePageModel<FlashSaleInfo> pageModel = new ObsoletePageModel<FlashSaleInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;

        }


        public ObsoletePageModel<FlashSaleInfo> GetAll(long shopid, int? status, string productName, DateTime? StartDate, DateTime? EndDate, int pageIndex, int pageSize)
        {
            ExpiredDataChangeStatus();
            IQueryable<FlashSaleInfo> query = Context.FlashSaleInfo.Where(p => p.ShopId == shopid);

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(p => p.Himall_Products.ProductName.Contains(productName));
            }
            if (StartDate.HasValue)
            {
                query = query.Where(a => a.BeginDate >= StartDate);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(a => a.BeginDate >= EndDate);
            }

            if (status > 0)
            {
                FlashSaleInfo.FlashSaleStatus s = (FlashSaleInfo.FlashSaleStatus)status;
                //  query = query.Where(p => p.Status == (FlashSaleInfo.FlashSaleStatus)status);
                if (s == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    query = query.Where(p => p.EndDate < DateTime.Now&&p.Status!=FlashSaleInfo.FlashSaleStatus.Cancelled);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                {
                    query = query.Where(p => p.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    query = query.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.AuditFailed)
                {
                    query = query.Where(a => a.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.Cancelled)
                {
                    query = query.Where(a => a.Status == (FlashSaleInfo.FlashSaleStatus)status);
                }
                else if (s == FlashSaleInfo.FlashSaleStatus.NotBegin)
                {
                    query = query.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate > DateTime.Now);
                }
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            int total = 0;
            IQueryable<FlashSaleInfo> datas = query.GetPage(out total, p => p.OrderByDescending(o => o.BeginDate), pageIndex, pageSize);
            ObsoletePageModel<FlashSaleInfo> pageModel = new ObsoletePageModel<FlashSaleInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }

        private void ExpiredDataChangeStatus()
        {
            var expiredList = Context.FlashSaleInfo.Where(p => p.EndDate <= DateTime.Now && p.Status != FlashSaleInfo.FlashSaleStatus.Cancelled);
            if (expiredList.Count() > 0)
            {
                foreach (var item in expiredList)
                {
                    item.Status = FlashSaleInfo.FlashSaleStatus.Ended;
                }
                Context.SaveChanges();
            }
        }

        public FlashSaleModel GetDetailInfo(long productId)
        {
            var fs = new FlashSaleModel();
            fs.ProductId = productId;
            fs.ProductImg = Context.ProductInfo.FirstOrDefault(p => p.Id == productId).ImagePath;
            List<FlashSaleDetailModel> list = new List<FlashSaleDetailModel>();
            var result = Context.SKUInfo.Where(p => p.ProductId == productId).ToList();

            foreach (var item in result)
            {
                FlashSaleDetailModel mdoel = new FlashSaleDetailModel();
                mdoel.SkuId = item.Id;
                mdoel.Color = item.Color;
                mdoel.Size = item.Size;
                mdoel.Version = item.Version;
                mdoel.Stock = (int)item.Stock;
                mdoel.SalePrice = item.SalePrice;
                mdoel.CostPrice = item.CostPrice;
                list.Add(mdoel);
            }
            fs.Details = list;
            return fs;
        }

        public FlashSaleDetailInfo GetDetail(string skuid)
        {
            //return context.FlashSaleDetailInfo.Include("").FirstOrDefault( p => p.SkuId == skuid);

            var x = Context.FlashSaleDetailInfo.Join(Context.FlashSaleInfo, detail => detail.FlashSaleId, main => main.Id, (detail, main) => new { detail, main })
                .Where(p => p.detail.SkuId == skuid && p.main.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && p.main.BeginDate < DateTime.Now && p.main.EndDate > DateTime.Now)
                .Select(p => p.detail).FirstOrDefault();

            return x;
        }

        public FlashSaleModel GetFlaseSaleByProductId(long pid)
        {
            if (pid <= 0)
            {
                throw new HimallException("商品Id不能识别");
            }

            var model = Context.FlashSaleInfo.FirstOrDefault(m => m.ProductId == pid && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate <= DateTime.Now
                && m.EndDate > DateTime.Now);
            FlashSaleModel result = new FlashSaleModel();
            if (model != null)
            {
                result.Id = model.Id;
                result.Title = model.Title;
                result.ShopId = model.ShopId;
                result.ProductId = model.ProductId;
                result.Status = model.Status;
                result.ProductName = model.Himall_Products.ProductName;
                result.ProductImg = model.Himall_Products.RelativePath;
                result.StatusStr = model.Status.ToDescription();
                result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
                result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
                result.LimitCountOfThePeople = model.LimitCountOfThePeople;
                result.SaleCount = model.SaleCount;
                result.CategoryName = model.CategoryName;
                result.MinPrice = model.MinPrice;
                result.Details = new List<FlashSaleDetailModel>();
                return result;
            }
            else
                return null;
        }

        public FlashSaleModel Get(long id)
        {
            var model = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == id);
            if (model == null)
            {
                throw new HimallException("活动不存在!");
            }

            FlashSaleModel result = new FlashSaleModel();
            result.Id = model.Id;
            result.Title = model.Title;
            result.ShopId = model.ShopId;
            result.ProductId = model.ProductId;
            result.Status = model.Status;
            result.ProductName = model.Himall_Products.ProductName;
            result.ProductImg = model.Himall_Products.RelativePath;
            result.MarketPrice = model.Himall_Products.MarketPrice;
            result.StatusStr = model.Status.ToDescription();
            result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
            result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
            result.LimitCountOfThePeople = model.LimitCountOfThePeople;
            result.SaleCount = model.SaleCount;
            result.CategoryName = model.CategoryName;
            result.MinPrice = model.MinPrice;
            result.Details = new List<FlashSaleDetailModel>();

            var details = Context.FlashSaleDetailInfo.Where(p => p.FlashSaleId == result.Id).ToList();
            var skus = Context.SKUInfo.Where(p => p.ProductId == model.ProductId);

            if (skus != null)
            {
                foreach (var sku in skus)
                {
                    var detail = details.FirstOrDefault(p => p.SkuId == sku.Id);
                    FlashSaleDetailModel d = new FlashSaleDetailModel();
                    d.Id = detail == null ? 0 : detail.Id;
                    d.SkuId = sku.Id;
                    d.Price = detail == null ? sku.SalePrice : (decimal)detail.Price;
                    d.Color = sku.Color;
                    d.Size = sku.Size;
                    d.Version = sku.Version;
                    d.Stock = (int)sku.Stock;
                    d.CostPrice = sku.CostPrice;
                    d.SalePrice = sku.SalePrice;
                    result.Details.Add(d);
                }
            }

            //if( details != null )
            //{
            //    foreach( var detail in details )
            //    {
            //        var sku = context.SKUInfo.FirstOrDefault( p => p.Id == detail.SkuId );
            //        if( sku == null )
            //        {
            //            //如果sku为空，证明限时购的sku记录与商品的不一致
            //            //证明商品在限时购已存在的情况下修改了sku相关信息
            //            //暂时还没做处理
            //            break;
            //        }
            //        FlashSaleDetailModel d = new FlashSaleDetailModel();
            //        d.Id = detail.Id;
            //        d.SkuId = detail.SkuId;
            //        d.Price = ( decimal )detail.Price;
            //        d.Color = sku.Color;
            //        d.Size = sku.Size;
            //        d.Version = sku.Version;
            //        d.Stock = ( int )sku.Stock;
            //        d.CostPrice = sku.CostPrice;
            //        d.SalePrice = sku.SalePrice;
            //        result.Details.Add( d );
            //    }
            //}

            return result;
        }


        private void CheckFlashSale(FlashSaleModel model)
        {
            if (Context.FlashSaleInfo.Exist(p => p.Id != 0 && p.Id != model.Id && p.ShopId == model.ShopId && p.ProductId == model.ProductId && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing ||
              p.Id != 0 && p.Id != model.Id && p.ShopId == model.ShopId && p.ProductId == model.ProductId && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing))
            {
                throw new HimallException("此商品已存在限时购活动");
            }
            var co = Context.ActiveMarketServiceInfo.FirstOrDefault(a => a.TypeId == MarketType.LimitTimeBuy && a.ShopId == model.ShopId);
            if (co == null)
            {
                throw new HimallException("您没有订购此服务");
            }
            var date = Convert.ToDateTime(model.EndDate);
            if (co.MarketServiceRecordInfo.Max(item => item.EndTime.Date) < date)
            {
                //throw new HimallException("结束日期不能超过购买限时购服务的日期");
                throw new HimallException(string.Format(" 活动结束日期不能超过服务购买日期，<br/>您的服务到期时间为{0}", co.MarketServiceRecordInfo.Max(item => item.EndTime.Date).ToString("yyyy-MM-dd HH:mm:ss")));
            }
        }

        public void AddFlashSale(FlashSaleModel model)
        {
            CheckFlashSale(model);

            var product = Context.ProductInfo.FirstOrDefault(p => p.Id == model.ProductId);

            FlashSaleInfo flashSale = new FlashSaleInfo();
            flashSale.Title = model.Title;
            flashSale.ShopId = model.ShopId;
            flashSale.ProductId = model.ProductId;
            flashSale.Status = FlashSaleInfo.FlashSaleStatus.WaitForAuditing;
            flashSale.BeginDate = DateTime.Parse(model.BeginDate);
            flashSale.EndDate = DateTime.Parse(model.EndDate);
            flashSale.CategoryName = model.CategoryName;
            flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
            flashSale.SaleCount = model.SaleCount;
            flashSale.ImagePath = product.RelativePath;
            flashSale.MinPrice = model.Details.Min(p => p.Price);
            flashSale = Context.FlashSaleInfo.Add(flashSale);
            Context.SaveChanges();
            foreach (var detail in model.Details)
            {
                FlashSaleDetailInfo fsd = new FlashSaleDetailInfo();
                fsd.FlashSaleId = flashSale.Id;
                fsd.ProductId = flashSale.ProductId;
                fsd.SkuId = detail.SkuId;
                fsd.Price = detail.Price;
                Context.FlashSaleDetailInfo.Add(fsd);
            }
            Context.SaveChanges();
        }

        public void UpdateFlashSale(FlashSaleModel model)
        {
            CheckFlashSale(model);
            FlashSaleInfo flashSale = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == model.Id);
            if (flashSale == null)
            {
                throw new HimallException("此活动已失踪");
            }


            if (flashSale.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
            {
                if (flashSale.ProductId != model.ProductId)
                {
                    var product = Context.ProductInfo.FirstOrDefault(p => p.Id == model.ProductId);
                    flashSale.ImagePath = product.RelativePath;
                }

                flashSale.Title = model.Title;
                flashSale.ShopId = model.ShopId;
                flashSale.ProductId = model.ProductId;
                flashSale.BeginDate = DateTime.Parse(model.BeginDate);
                flashSale.CategoryName = model.CategoryName;
                flashSale.EndDate = DateTime.Parse(model.EndDate);
                flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
                flashSale.MinPrice = model.Details.Min(p => p.Price);

                Context.FlashSaleDetailInfo.Remove(p => p.ProductId == model.ProductId);
                foreach (var detail in model.Details)
                {
                    FlashSaleDetailInfo fsd = new FlashSaleDetailInfo();
                    fsd.ProductId = flashSale.ProductId;
                    fsd.SkuId = detail.SkuId;
                    fsd.Price = detail.Price;
                    fsd.FlashSaleId = flashSale.Id;
                    Context.FlashSaleDetailInfo.Add(fsd);
                }
                Context.SaveChanges();
            }
            else if (flashSale.Status == FlashSaleInfo.FlashSaleStatus.Ongoing)
            {
                flashSale.Title = model.Title;
                flashSale.CategoryName = model.CategoryName;
                flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
                Context.SaveChanges();
            }

        }

        public void Pass(long id)
        {
            var model = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == id);
            if (model == null || model.EndDate <= DateTime.Now)
            {
                throw new HimallException("活动不存在或过期!");
            }
            model.Status = FlashSaleInfo.FlashSaleStatus.Ongoing;
            Context.SaveChanges();

            //启动定时任务
            CreateFlashSaleJob(model);
        }

        public void Refuse(long id)
        {
            var model = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == id);
            if (model == null)
            {
                throw new HimallException("活动不存在!");
            }

            model.Status = FlashSaleInfo.FlashSaleStatus.AuditFailed;
            Context.SaveChanges();
        }

        public void Cancel(long id)
        {
            var model = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == id);
            if (model == null)
            {
                throw new HimallException("活动不存在!");
            }

            model.Status = FlashSaleInfo.FlashSaleStatus.Cancelled;
            model.EndDate = DateTime.Now;
            Context.SaveChanges();
        }

        public bool IsAdd(long productid)
        {
            var result = Context.FlashSaleInfo.Any(p => (p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) || (
                p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing));

            return !result;
        }
        public bool IsEdit(long productid, long id)
        {
            bool result = true;
            if (id > 0)
            {
                result = Context.FlashSaleInfo.Any(p => ((p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) || (
                    p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)) && p.Id != id);
            }
            else
            {
                result = IsAdd(productid);
            }
            return !result;
        }

        public void IncreaseSaleCount(List<long> orderids)
        {
            //限时购销量
            var orderid = orderids[0];
            long productid = Context.OrderItemInfo.Where(p => p.OrderId == orderid).FirstOrDefault().ProductId;
            var result = Context.FlashSaleInfo.Where(p => p.ProductId == productid && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing).FirstOrDefault();
            if (result != null)
            {
                long saleCount = Context.OrderItemInfo.Where(p => p.OrderId == orderid).FirstOrDefault().Quantity;
                result.SaleCount = result.SaleCount + (int)saleCount;
                Context.SaveChanges();
            }

            //商品销量
            //ServiceProvider.Instance<Himall.IServices.IOrderService>.Create.UpdateProductVistiOrderCount( orderid );
        }

        public FlashSaleModel IsFlashSaleDoesNotStarted(long productid)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTLIMITNOTSTART(productid);
            if (Cache.Exists(cacheKey))
                return Cache.Get<FlashSaleModel>(cacheKey);

            var model = Context.FlashSaleInfo.FirstOrDefault(p => p.ProductId == productid
                && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing
                && p.BeginDate > DateTime.Now);

            if (model != null)
            {
                FlashSaleModel result = new FlashSaleModel();
                result.Id = model.Id;
                result.Title = model.Title;
                result.ShopId = model.ShopId;
                result.ProductId = model.ProductId;
                result.Status = model.Status;
                result.ProductName = model.Himall_Products.ProductName;
                result.ProductImg = model.Himall_Products.ImagePath;
                result.StatusStr = model.Status.ToDescription();
                result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
                result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
                result.LimitCountOfThePeople = model.LimitCountOfThePeople;
                result.SaleCount = model.SaleCount;
                result.CategoryName = model.CategoryName;
                Cache.Insert<FlashSaleModel>(cacheKey, result, model.BeginDate.AddSeconds(-10));//缓存至开始时间前10秒
                return result;
            }
            Cache.Insert<FlashSaleModel>(cacheKey, null, 60);
            return null;
        }

        public List<FlashSalePrice> GetPriceByProducrIds(List<long> ids)
        {
            List<FlashSalePrice> list = new List<FlashSalePrice>();
            string idlist = ids != null && ids.Count > 0 ? string.Join(",", ids) : "";
            string sql = "SELECT ProductId,MinPrice FROM HiMall_FlashSale WHERE {0} Status=@Status AND BeginDate < @BeginDate AND EndDate > @EndDate";
            sql = string.Format(sql, ids != null && ids.Count > 0 ? "ProductId IN (" + idlist + ") AND" : "");
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                list = conn.Query<FlashSalePrice>(sql, new { @Status = FlashSaleInfo.FlashSaleStatus.Ongoing, @BeginDate = DateTime.Now, @EndDate = DateTime.Now }).ToList();
            }
            return list;
        }
        #endregion

        #region 开团提醒定时任务

        //创建定时任务
        private void CreateFlashSaleJob(FlashSaleInfo flashSale)
        {
            if (flashSale.BeginDate > DateTime.Now)  //还未开始  设置开始时间为提示时间
            {
                DateTime date = flashSale.BeginDate;
                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("FlashSaleId", flashSale.Id);
                Himall.Core.Helper.QuartzHelper.ExecuteAtDate(SendRemind, date, args, "FlashSaleRemind-" + flashSale.Id);
#if DEBUG
                Log.Info("限时购 - 添加活动，启动定时任务，时间 = " + date.ToString("yyyy-MM-dd HH:mm"));
#endif
            }
            //否则是已经开始了，所以用户没机会去进行开团提醒操作 ， 此时不需要提示
        }

        public void AddRemind(long flashSaleId, string openId)
        {
#if DEBUG
            Log.Info("限时购 - 添加订阅用户");
#endif
            if (Context.FlashSaleRemindInfo.Any(p => p.FlashSaleId == flashSaleId && p.OpenId == openId))
            {
                return;
            }

            FlashSaleRemindInfo model = new FlashSaleRemindInfo();
            model.FlashSaleId = flashSaleId;
            model.OpenId = openId;
            model.RecordDate = DateTime.Now;
            model = Context.FlashSaleRemindInfo.Add(model);
            Context.SaveChanges();

            var flashSale = Context.FlashSaleInfo.FirstOrDefault(p => p.Id == flashSaleId);

            string openid = openId;
            var msgdata = new WX_MsgTemplateSendDataModel();
            msgdata.first.value = "亲爱的用户，您已经成功设置限时购订阅提醒！";
            msgdata.first.color = "#000000";
            msgdata.keyword1.value = flashSale.Himall_Products.ProductName;
            msgdata.keyword1.color = "#FF0000";
            msgdata.keyword2.value = flashSale.MinPrice.ToString();
            msgdata.keyword2.color = "#FF0000";
            msgdata.keyword3.value = model.RecordDate.ToString("yyyy-MM-dd HH:mm");
            msgdata.keyword3.color = "#000000";
            msgdata.remark.value = "活动开始时将自动提醒您，请关注您的微信消息。";
            msgdata.remark.color = "#000000";
            //处理url
            var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.SubscribeLimitTimeBuy);
            _iwxtser.SendMessageByTemplate(MessageTypeEnum.SubscribeLimitTimeBuy, 0, msgdata, url, openid);
        }

        //开团提醒发送消息给用户
        private void SendRemind(Dictionary<string, object> args)
        {
            long flashSaleId = (long)args["FlashSaleId"];
#if DEBUG
            Log.Info("限时购 - 进入SendRemind");
            Log.Info("FlashSaleId = " + flashSaleId);
#endif
            try
            {
                Entities contextEntity = new Entities();
                string sitename = contextEntity.Database.SqlQuery<string>("select Value from himall_sitesettings where `key` = 'SiteName'").Single<string>();
                Log.Info("SendRemind sitename = " + sitename);
                var flashSale = contextEntity.FlashSaleInfo.Include(p => p.Himall_Products).FirstOrDefault(p => p.Id == flashSaleId);
                Log.Info("SendRemind flashSale = " + flashSale.Title);
                var flashSaleReminds = contextEntity.FlashSaleRemindInfo.Where(p => p.FlashSaleId == flashSaleId);
                Log.Info("SendRemind flashSaleReminds.Count = " + flashSaleReminds.Count());
                if (flashSale == null || flashSale.Status != FlashSaleInfo.FlashSaleStatus.Ongoing || flashSale.EndDate <= DateTime.Now)
                {
                    Log.Info("SendRemind Null");
                    return;
                }
                Log.Info("SendRemind 准备发送消息");
                foreach (var remind in flashSaleReminds)
                {
                    string openid = remind.OpenId;
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您好！您参与的活动已经开始";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = "限时抢购";
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = flashSale.BeginDate.ToString("yyyy-MM-dd HH:mm");
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = sitename;
                    msgdata.keyword3.color = "#000000";
                    msgdata.keyword4.value = flashSale.Himall_Products.ProductName;
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "";
                    msgdata.remark.color = "#000000";
                    Log.Info("SendRemind 数据加载完毕");
                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.LimitTimeBuy);
                    url = url.Replace("{id}", flashSaleId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.LimitTimeBuy, 0, msgdata, url, openid);
                }
            }
            catch (Exception e)
            {
                Exception innerEx = e.InnerException == null ? e : e.InnerException;
                Log.Info("SendRemind发生错误 Message：" + innerEx.Message);
                Log.Info("SendRemind发生错误 StackTrace：" + innerEx.StackTrace);
            }
        }


        #endregion

        public int GetMarketSaleCountForUserId(long pId, long userId)
        {
            int count = 0;
            string sql = "select a.Quantity,b.OrderDate from himall_orderitems a inner join himall_orders b on a.orderid=b.id where b.UserId=@UserId and a.IsLimitBuy=1 and a.ProductId=@ProductId and b.OrderStatus != 4";
            string limitsql = "select * from himall_flashsale where ProductId=@ProductId and Status=2 and EndDate>=@EndDate";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                var list = conn.Query(sql, new { UserId = userId, ProductId = pId }).ToList();
                if (list.Count != 0)
                {
                    var limit = conn.Query(limitsql, new { ProductId = pId, EndDate = DateTime.Now }).FirstOrDefault();
                    if (limit != null)
                    {
                        count = list.Where(a => a.OrderDate >= limit.BeginDate).Sum(r => (int?)r.Quantity).GetValueOrDefault();
                    }
                }
            }
            return count;

            //var list = Context.OrderItemInfo.Include("OrderInfo").Where(a => a.ProductId == pId && a.IsLimitBuy &&
            //    a.OrderInfo.UserId == userId &&
            //    a.OrderInfo.OrderStatus != OrderInfo.OrderOperateStatus.Close).ToList();

            //if (list.Count == 0)
            //{
            //    return 0;
            //}
            //var now = DateTime.Now;
            //var limit = Context.FlashSaleInfo.Where(item => item.ProductId == pId && item.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && item.EndDate >= now).FirstOrDefault();
            //var count = list.Where(a => a.OrderInfo.OrderDate >= limit.BeginDate).Sum(a => (long?)a.Quantity).GetValueOrDefault();

            //var orders = ServiceProvider.Instance<IOrderService>.Create.GetOrders<OrderInfo>(
            //    new OrderQuery
            //    {
            //        UserId = userId,
            //        PageNo = 1,
            //        PageSize = int.MaxValue,
            //    }, null).Models;

            //if (orders.Count() == 0) return 0;
            //var exist = orders.Where(o => o.OrderStatus != OrderInfo.OrderOperateStatus.Close && o.OrderItemInfo.Any(oi => oi.ProductId == pId));

            //var limit = context.LimitTimeMarketInfo.Where(item => item.ProductId == pId && item.AuditStatus == Himall.Model.LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing && item.EndTime >= DateTime.Now).FirstOrDefault();
            //long count = 0;
            //if (limit != null)
            //{
            //    exist = exist.Where(item => item.OrderDate > limit.AuditTime);
            //    count = exist.Count() == 0 ? 0 : exist.Sum(o => o.OrderItemInfo.FirstOrDefault().Quantity);
            //}
            
        }

        /// <summary>
        /// 获取最近5个限时购
        /// </summary>
        /// <returns></returns>
        public List<FlashSaleModel> GetRecentFlashSale()
        {
            var list = Context.FlashSaleInfo.Where(item => item.Status == Himall.Model.FlashSaleInfo.FlashSaleStatus.Ongoing && item.EndDate > DateTime.Now && item.BeginDate < DateTime.Now && item.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited).OrderByDescending(p => p.SaleCount).Take(5).ToList();

            List<FlashSaleModel> model = new List<FlashSaleModel>();
            foreach (FlashSaleInfo p in list)
            {
                FlashSaleModel fsm = new FlashSaleModel();
                fsm.BeginDate = p.BeginDate.ToString("yyyy-MM-dd HH:mm:ss");
                fsm.CategoryName = p.CategoryName;
                fsm.EndDate = p.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
                fsm.Id = p.Id;
                fsm.IsStarted = true;
                fsm.LimitCountOfThePeople = p.LimitCountOfThePeople;
                fsm.MarketPrice = p.Himall_Products.MarketPrice;
                fsm.MinPrice = p.MinPrice;
                fsm.ProductId = p.ProductId;
                fsm.ProductImg = HimallIO.GetProductSizeImage(p.Himall_Products.ImagePath, 1, (int)ImageSize.Size_350);
                fsm.ProductName = p.Himall_Products.ProductName;
                fsm.SaleCount = p.SaleCount;
                fsm.ShopId = p.ShopId;
                fsm.ShopName = p.Himall_Shops.ShopName;
                fsm.Status = p.Status;
                fsm.StatusNum = (int)p.Status;
                fsm.StatusStr = p.Status.ToDescription();
                fsm.Title = p.Title;
                fsm.Quantity = p.Himall_Products.Quantity == null ? p.Himall_Products.SKUInfo.Sum(s => s.Stock) : p.Himall_Products.Quantity.Value;
                model.Add(fsm);

            }

            return model;
        }

    }

}
