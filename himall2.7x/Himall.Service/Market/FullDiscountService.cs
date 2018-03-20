using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Himall.Core;
using Himall.CommonModel;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Core.Plugins.Message;

namespace Himall.Service
{
    public class FullDiscountService : ServiceBase, IFullDiscountService
    {
        /// <summary>
        /// 当前活动类型
        /// </summary>
        private MarketActiveType CurrentActiveType = MarketActiveType.FullDiscount;

        #region 满减活动操作
        /// <summary>
        /// 新增满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public void AddActive(ActiveInfo data, IEnumerable<FullDiscountRulesInfo> rules, IEnumerable<ActiveProductInfo> products)
        {
            data.ActiveType = CurrentActiveType;
            Context.ActiveInfo.Add(data);
            Context.SaveChanges();
            foreach (var item in rules)
            {
                item.ActiveId = data.Id;
            }
            foreach (var item in products)
            {
                item.ActiveId = data.Id;
            }
            Context.FullDiscountRulesInfo.AddRange(rules);
            Context.ActiveProductInfo.AddRange(products);
            Context.SaveChanges();
        }
        /// <summary>
        /// 更新满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public void UpdateActive(ActiveInfo data, IEnumerable<FullDiscountRulesInfo> rules, IEnumerable<ActiveProductInfo> products)
        {
            UpdateData(data);
            Context.FullDiscountRulesInfo.Remove(d => d.ActiveId == data.Id);
            foreach (var item in rules)
            {
                item.ActiveId = data.Id;
            }
            Context.FullDiscountRulesInfo.AddRange(rules);
            Context.ActiveProductInfo.Remove(d => d.ActiveId == data.Id);
            foreach (var item in products)
            {
                item.ActiveId = data.Id;
            }
            Context.ActiveProductInfo.AddRange(products);
            Context.SaveChanges();
        }
        /// <summary>
        /// 删除满减活动
        /// </summary>
        /// <param name="id"></param>
        public void DeleteActive(long id)
        {
            Context.Database.ExecuteSqlCommand("delete from Himall_ActiveProducts where ActiveId=" + id);
            Context.Database.ExecuteSqlCommand("delete from Himall_FullDiscountRules where ActiveId=" + id);
            Context.Database.ExecuteSqlCommand("delete from Himall_Active where ActiveType=" + CurrentActiveType.GetHashCode() + " and Id=" + id);
        }
        #endregion

        #region 满减活动查询
        /// <summary>
        /// 商品是否可以参加满减活动
        /// <para>不判断商品的销售状态</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <returns></returns>
        public bool ProductCanJoinActive(long productId, long activeId)
        {
            var actsql = Context.ActiveInfo.Where(a => a.ActiveType == CurrentActiveType && a.EndTime > DateTime.Now);
            if (activeId > 0)
            {
                actsql = actsql.Where(d => d.Id != activeId);
            }
            var activeIds = actsql.Select(a => a.Id);
            var actProducts = Context.ActiveProductInfo.Where(a => (a.ProductId == productId || a.ProductId == -1) && activeIds.Contains(a.ActiveId));
            return actProducts.Any();
        }
        /// <summary>
        /// 过滤活动商品编号
        /// <para>返回可以参加活动的商品</para>
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns></returns>
        public List<long> FilterActiveProductId(IEnumerable<long> productIds, long activeId,long shopId)
        {
            var actsql = Context.ActiveInfo.Where(a => a.ActiveType == CurrentActiveType && a.EndTime > DateTime.Now && a.ShopId== shopId);

            if (activeId > 0)
            {
                actsql = actsql.Where(d => d.Id != activeId);
            }
            var activeIds = actsql.Select(a => a.Id);
            var actProductIds = Context.ActiveProductInfo.Where(a => activeIds.Contains(a.ActiveId)).Select(d => d.ProductId).ToList();

            List<long> result = new List<long>();
            if (!actProductIds.Any(d => d == -1))
            {
                //过滤活动中的商品
                //actProductIds = Context.ProductInfo.Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                //&& d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                // && d.IsDeleted == false
                //&& actProductIds.Contains(d.Id)).Select(d => d.Id).ToList();
                //过滤非销售中的商品
                var okproductIds = Context.ProductInfo.Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                  && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                  && d.IsDeleted == false
                  && productIds.Contains(d.Id)).Select(d => d.Id).ToList();
                result = productIds.Where(d => !actProductIds.Contains(d)).ToList();
                result = productIds.Where(d => okproductIds.Contains(d)).ToList();
            }
            return result;
        }
        /// <summary>
        /// 根据商品ID取正在参与且进行中的活动信息
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public ActiveInfo GetOngoingActiveByProductId(long proId, long shopId)
        {
            var goingActives = Context.ActiveInfo.Where(a => a.ActiveType == CurrentActiveType && a.ShopId == shopId && a.StartTime <= DateTime.Now && a.EndTime > DateTime.Now).ToList();
            var activeIds = goingActives.Select(a => a.Id);
            var productActiveId = Context.ActiveProductInfo.Where(a => (a.ProductId == proId || a.ProductId == -1) && activeIds.Contains(a.ActiveId)).Select(a => a.ActiveId).FirstOrDefault();
            var result = goingActives.Where(a => a.Id == productActiveId).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 获取某个店铺正在进行的满额减活动列表
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public List<ActiveInfo> GetOngoingActiveByShopId(long shopId)
        {
            var goingActives = Context.ActiveInfo.Where(a => a.ActiveType == CurrentActiveType && a.ShopId == shopId && a.StartTime <= DateTime.Now && a.EndTime > DateTime.Now).ToList();
            //  var activeIds = goingActives.Select(a => a.Id);
            return goingActives;
        }
        /// <summary>
        /// 根据正在进行的活动ID和商品ID获取满额减活动分组商品
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeIds"></param>
        /// <returns></returns>
        private List<IGrouping<long, ActiveProductInfo>> GetActiveProductGroup(IEnumerable<long> productIds, IEnumerable<long> activeIds)
        {
            var productActives = Context.ActiveProductInfo.Where(a => (productIds.Contains(a.ProductId) || a.ProductId == -1) && activeIds.Contains(a.ActiveId)).GroupBy(a => a.ActiveId).ToList();

            return productActives;
        }

        /// <summary>
        /// 获取某个店铺的一批商品正在进行的满额减活动
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<ActiveInfo> GetOngoingActiveByProductIds(IEnumerable<long> productIds, long shopId)
        {
            var actives = GetOngoingActiveByShopId(shopId);
            if (actives == null || actives.Count == 0)
                return new List<ActiveInfo>();
            var activeIds = actives.Select(a => a.Id);
            var activeGroup = GetActiveProductGroup(productIds, activeIds);
            if (activeGroup == null || activeGroup.Count == 0)
                return new List<ActiveInfo>();
            var ids = activeGroup.Select(a => a.Key);
            var onGoingActives = actives.Where(a => ids.Contains(a.Id)).ToList();
            foreach (var a in onGoingActives)
            {
                var p = activeGroup.Where(x => x.Key == a.Id).FirstOrDefault().Select(y => y).ToList();
                a.Products = p;
                a.Rules = GetActiveRules(a.Id);
            }
            return onGoingActives;
        }



        /// <summary>
        /// 获取满减活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActiveInfo GetActive(long id)
        {
            var result = Context.ActiveInfo.FirstOrDefault(d => d.Id == id);
            return result;
        }
        /// <summary>
        /// 获取满减优惠阶梯
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<FullDiscountRulesInfo> GetActiveRules(long activeId)
        {
            var result = Context.FullDiscountRulesInfo.Where(d => d.ActiveId == activeId).ToList();
            return result;
        }
        /// <summary>
        /// 获取满减商品
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<ActiveProductInfo> GetActiveProducts(long activeId)
        {
            var result = Context.ActiveProductInfo.Where(d => d.ActiveId == activeId).ToList();
            return result;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ActiveInfo> GetActives(FullDiscountActiveQuery query)
        {
            QueryPageModel<ActiveInfo> result = new QueryPageModel<ActiveInfo>();
            var curtime = DateTime.Now;
            int total = 0;

            if (query.Status != null)  //有状态不过UNION all
            {
                #region 普通排序走ef
                var _where = Context.ActiveInfo.GetDefaultPredicate(true);
                _where = _where.And(d => d.ActiveType == CurrentActiveType);
                if (query.StartTime.HasValue)
                {
                    var startTime = query.StartTime.Value.Date;
                    _where = _where.And(d => d.StartTime >= startTime);
                }
                if (query.EndTime.HasValue)
                {
                    DateTime EndTime = query.EndTime.Value.AddDays(1).Date;
                    _where = _where.And(d => d.StartTime <= EndTime);
                }
                if (query.ShopId.HasValue)
                {
                    _where = _where.And(d => d.ShopId == query.ShopId.Value);
                }
                if (!string.IsNullOrWhiteSpace(query.ActiveName))
                {
                    _where = _where.And(d => d.ActiveName.Contains(query.ActiveName));
                }
                switch (query.Status)
                {
                    case FullDiscountStatus.Ending:
                        _where = _where.And(d => d.EndTime < curtime);
                        break;
                    case FullDiscountStatus.Ongoing:
                        _where = _where.And(d => d.StartTime <= curtime && d.EndTime >= curtime);
                        break;
                    case FullDiscountStatus.WillStart:
                        _where = _where.And(d => d.StartTime > curtime && d.EndTime > d.StartTime);
                        break;
                }
                var sql = Context.ActiveInfo.Where(_where);
                var order = sql.GetOrderBy(d => d.OrderByDescending(o => o.StartTime));
                List<ActiveInfo> datalist = sql.GetPage(out total, query.PageNo, query.PageSize, order).ToList();
                result.Models = datalist;
                result.Total = total;
                #endregion
            }
            else
            {
                string sqlbasestring = @"select * from (
                        SELECT *,1 as ordernum from Himall_Active where EndTime<NOW() {where}
                        UNION ALL
                        SELECT *,2 as ordernum from Himall_Active where StartTime>NOW() {where}
                        UNION ALL
                        SELECT *,3 as ordernum from Himall_Active where StartTime<=NOW() and EndTime>=now() {where}
                        ) as t";
                string wherestring = " and ActiveType=" + CurrentActiveType.GetHashCode().ToString() + " ";
                if (query.ShopId.HasValue)
                {
                    wherestring += " and ShopId=" + query.ShopId.ToString() + " ";
                }
                if (!string.IsNullOrWhiteSpace(query.ActiveName))
                {
                    wherestring += " and ActiveName like '%" + query.ActiveName + "%' ";
                }
                if (query.StartTime.HasValue)
                {
                    wherestring += " and StartTime >= '" + query.StartTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                if (query.EndTime.HasValue)
                {
                    DateTime EndTime = query.EndTime.Value.AddDays(1).Date;
                    wherestring += " and StartTime <= '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                sqlbasestring = sqlbasestring.Replace("{where}", wherestring);
                string countsqlstring = @"select count(1) from (" + sqlbasestring + ") as t";
                int pstart = (query.PageNo - 1) * query.PageSize;
                string sqlstring = sqlbasestring + " ORDER BY ordernum DESC, StartTime desc limit " + pstart + "," + query.PageSize;
                total = Context.Database.SqlQuery<int>(countsqlstring).FirstOrDefault();
                result.Total = total;
                List<ActiveInfo> datas = Context.Database.SqlQuery<ActiveInfo>(sqlstring).ToList();
                result.Models = datas;
            }
            return result;
        }
        /// <summary>
        /// 获取活动商品数量聚合
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<FullDiscountProductCountAggregate> GetActivesProductCountAggregate(IEnumerable<long> activeId)
        {
            var sql = (from d in Context.ActiveProductInfo
                       join p in Context.ProductInfo on d.ProductId equals p.Id
                       where activeId.Contains(d.ActiveId) && p.IsDeleted == false
                       group d by d.ActiveId into g
                       select new FullDiscountProductCountAggregate
                       {
                           ActiveId = g.Key,
                           ProductCount = g.Count(),
                       });
            List<FullDiscountProductCountAggregate> result = sql.ToList();
            return result;
        }
        #endregion

        #region 其他功能
        /// <summary>
        /// 获取可以参与满减活动的商品集
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productName"></param>
        /// <param name="productCode"></param>
        /// <param name="selectedProductIds"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public QueryPageModel<ProductInfo> GetCanJoinProducts(long shopId
            , string productName = null, string productCode = null
            , IEnumerable<long> selectedProductIds = null, int activeId = 0
            , int page = 1, int pagesize = 10)
        {
            QueryPageModel<ProductInfo> result = new QueryPageModel<ProductInfo>();
            //获取活动
            DateTime curtime = DateTime.Now;
            var actsql = Context.ActiveInfo.Where(d => d.EndTime >= curtime && d.ShopId== shopId);
            if (activeId > 0)
            {
                actsql = actsql.Where(d => d.Id != activeId);
            }
            var actids = actsql.Select(d => d.Id);
            var actproids = Context.ActiveProductInfo.Where(d => actids.Contains(d.ActiveId)).Select(d => d.ProductId);
            if (!actproids.Any(d => d == -1))
            {
                var prosql = Context.ProductInfo.Where(d => d.ShopId == shopId
                && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && d.IsDeleted == false
                );
                prosql = prosql.Where(d => !actproids.Contains(d.Id));
                if (!string.IsNullOrWhiteSpace(productName))
                {
                    prosql = prosql.Where(d => d.ProductName.Contains(productName));
                }
                if (!string.IsNullOrWhiteSpace(productCode))
                {
                    prosql = prosql.Where(d => d.ProductCode.Contains(productCode));
                }
                if (selectedProductIds != null && selectedProductIds.Count() > 0)
                {
                    prosql = prosql.Where(d => !selectedProductIds.Contains(d.Id));
                }
                int total = 0;
                var datas = prosql.GetPage(out total, page, pagesize);
                result.Total = total;
                result.Models = datas.ToList();
            }
            return result;
        }
        /// <summary>
        /// 是否可以操作(添加/修改)活动
        /// </summary>
        /// <param name="active"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        public bool CanOperationActive(ActiveInfo active, IEnumerable<ActiveProductInfo> products)
        {
            bool result = false;
            DateTime now = DateTime.Now;
            var actsql = Context.ActiveInfo.Where(d => d.ActiveType == CurrentActiveType && d.EndTime > now && d.ShopId== active.ShopId);
            if (active.Id > 0)
            {
                actsql = actsql.Where(d => d.Id != active.Id);
            }
            if (active.IsAllProduct && actsql.Count() > 0)
            {
                return false;
            }
            if (!actsql.Any(d => d.IsAllProduct))
            {
                var actids = actsql.Select(d => d.Id);
                var proids = Context.ActiveProductInfo.Where(d => actids.Contains(d.ActiveId)).Select(d => d.ProductId).ToList();
                result = true;
                foreach (var item in products)
                {
                    if (proids.Contains(item.ProductId))
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取不在销售中的商品
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public List<long> GetNoSaleProductId(IEnumerable<long> productIds)
        {
            List<long> result = new List<long>();
            var saleproids = Context.ProductInfo.Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && productIds.Contains(d.Id)).Select(d => d.Id).ToList();
            result = productIds.Where(d => !saleproids.Contains(d)).ToList();
            return result;

        }
        #endregion

        /// <summary>
        /// 提交保存
        /// </summary>
        public void Commit()
        {
            Context.SaveChanges();
        }

    }
}
