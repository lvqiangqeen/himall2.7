using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.DTO;
using Himall.CommonModel;
using Himall.IServices.QueryModel;

namespace Himall.Application
{
    /// <summary>
    /// 拼团逻辑
    /// </summary>
    public class FullDiscountApplication
    {
        /// <summary>
        /// 当前营销类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.FullDiscount;

        private static IFullDiscountService _iFullDiscountService = ObjectContainer.Current.Resolve<IFullDiscountService>();
        private static IProductService _iProductService = ObjectContainer.Current.Resolve<IProductService>();

        #region 满减活动查询
        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FullDiscountActive GetActive(long id)
        {
            FullDiscountActive result = null;
            var _adata = _iFullDiscountService.GetActive(id);
            result = Mapper.Map<ActiveInfo, FullDiscountActive>(_adata);
            if (result != null)
            {
                var _rlist = _iFullDiscountService.GetActiveRules(id);
                result.Rules = Mapper.Map<List<FullDiscountRulesInfo>, List<FullDiscountRules>>(_rlist);
                var _plist = _iFullDiscountService.GetActiveProducts(id);
                result.Products = Mapper.Map<List<ActiveProductInfo>, List<FullDiscountActiveProduct>>(_plist);

                if (result.IsAllProduct)
                {
                    result.ProductCount = -1;
                }
                else
                {
                    result.ProductCount = result.Products.Count;
                }
            }

            return result;
        }


        /// <summary>
        /// 获取某个店铺的一批商品正在进行的满额减活动
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<FullDiscountActive> GetOngoingActiveByProductIds(IEnumerable<long> productIds, long shopId)
        {
            var onGoingActives = _iFullDiscountService.GetOngoingActiveByProductIds(productIds, shopId);
            var result = Mapper.Map<List<ActiveInfo>, List<FullDiscountActive>>(onGoingActives);
            return result;
        }


        /// <summary>
        /// 获取某个实体正在参加的满额折
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static FullDiscountActive GetOngoingActiveByProductId(long proId, long shopId)
        {
            FullDiscountActive result = null;
            var active = _iFullDiscountService.GetOngoingActiveByProductId(proId, shopId);
            if (active == null)
                return result;
            result = Mapper.Map<ActiveInfo, FullDiscountActive>(active);
            var _rlist = _iFullDiscountService.GetActiveRules(result.Id);
            result.Rules = Mapper.Map<List<FullDiscountRulesInfo>, List<FullDiscountRules>>(_rlist);
            return result;
        }


        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullDiscountActiveList> GetActives(FullDiscountActiveQuery query)
        {
            QueryPageModel<FullDiscountActiveList> result = new QueryPageModel<FullDiscountActiveList>();
            var datalist = _iFullDiscountService.GetActives(query);
            result.Total = datalist.Total;
            var actids = datalist.Models.Select(d => d.Id).ToList();
            var fdparg = _iFullDiscountService.GetActivesProductCountAggregate(actids);
            result.Models = new List<FullDiscountActiveList>();
            foreach (var item in datalist.Models)
            {
                FullDiscountActiveList _data = Mapper.Map<ActiveInfo, FullDiscountActiveList>(item);
                if (_data.IsAllProduct)
                {
                    _data.ProductCount = -1;
                }
                else
                {
                    var _parg = fdparg.FirstOrDefault(d => d.ActiveId == _data.Id);
                    if (_parg != null)
                    {
                        _data.ProductCount = _parg.ProductCount;
                    }
                }
                result.Models.Add(_data);
            }
            return result;
        }
        /// <summary>
        /// 商品是否可以参加满减活动
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <returns></returns>
        public static bool ProductCanJoinActive(long productId, long activeId)
        {
            return _iFullDiscountService.ProductCanJoinActive(productId, activeId);
        }
        /// <summary>
        /// 过滤活动商品编号
        /// <para>返回可以能加商动的商品</para>
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns></returns>
        public static List<long> FilterActiveProductId(IEnumerable<long> productIds, long activeId, long shopId)
        {
            return _iFullDiscountService.FilterActiveProductId(productIds, activeId,shopId);
        }
        #endregion

        #region 满减活动操作
        public static void AddActive(FullDiscountActive model)
        {
            ActiveInfo data = Mapper.Map<FullDiscountActive, ActiveInfo>(model);
            List<FullDiscountRulesInfo> rules = Mapper.Map<List<FullDiscountRules>, List<FullDiscountRulesInfo>>(model.Rules);
            List<ActiveProductInfo> products = Mapper.Map<List<FullDiscountActiveProduct>, List<ActiveProductInfo>>(model.Products);

            //判断活动是否可添加
            if (!_iFullDiscountService.CanOperationActive(data, products))
            {
                throw new HimallException("有其他冲突活动存在，不可以完成操作");
            }

            _iFullDiscountService.AddActive(data, rules, products);
            //值回填
            model.Id = data.Id;
            foreach (var item in model.Rules)
            {
                item.ActiveId = model.Id;
            }
            foreach (var item in model.Products)
            {
                item.ActiveId = model.Id;
            }
        }
        /// <summary>
        /// 更新满减活动
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateActive(FullDiscountActive model)
        {
            ActiveInfo data = Mapper.Map<FullDiscountActive, ActiveInfo>(model);
            List<FullDiscountRulesInfo> rules = Mapper.Map<List<FullDiscountRules>, List<FullDiscountRulesInfo>>(model.Rules);
            List<ActiveProductInfo> products = Mapper.Map<List<FullDiscountActiveProduct>, List<ActiveProductInfo>>(model.Products);

            if (data.Id == 0)
            {
                throw new HimallException("错误的活动编号");
            }

            //判断活动是否可添加
            if (!_iFullDiscountService.CanOperationActive(data, products))
            {
                throw new HimallException("有其他冲突活动存在，不可以完成操作");
            }

            _iFullDiscountService.UpdateActive(data, rules, products);
        }
        /// <summary>
        /// 删除活动
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteActive(long id)
        {
            _iFullDiscountService.DeleteActive(id);
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
        public static QueryPageModel<ProductInfo> GetCanJoinProducts(long shopId
            , string productName = null, string productCode = null
            , IEnumerable<long> selectedProductIds = null
            ,int activeId=0
            , int page = 1, int pagesize = 10)
        {
            QueryPageModel<ProductInfo> result = _iFullDiscountService.GetCanJoinProducts(shopId, productName, productCode, selectedProductIds, activeId, page, pagesize);
            return result;
        }
        /// <summary>
        /// 获取不在销售中的商品
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static List<long> GetNoSaleProductId(IEnumerable<long> productIds)
        {
            return _iFullDiscountService.GetNoSaleProductId(productIds);
        }
        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ProductInfo> GetProductByIds(IEnumerable<long> productIds)
        {
            List<ProductInfo> result = _iProductService.GetProductByIds(productIds).ToList();
            return result;
        }
        #endregion

        #region 系统
        /// <summary>
        /// 满减营销活动费用设置
        /// </summary>
        /// <returns></returns>
        public static decimal GetMarketServicePrice()
        {
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser == null)
            {
                marketser = SetMarketServicePrice(0.00m);
            }
            return marketser.Price;
        }
        /// <summary>
        /// 设置满减营销活动费用设置
        /// </summary>
        /// <param name="price"></param>
        public static MarketSettingInfo SetMarketServicePrice(decimal price)
        {
            MarketSettingInfo marketser = new MarketSettingInfo() { TypeId = CurMarketType, Price = price };
            MarketApplication.AddOrUpdateServiceSetting(marketser);
            return marketser;
        }
        /// <summary>
        /// 是否已开启满减营销
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenMarketService()
        {
            bool result = false;
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取满减营销服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static MarketServiceModel GetMarketService(long shopId)
        {
            MarketServiceModel result = null;
            var market = MarketApplication.GetMarketService(shopId, CurMarketType);
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result = new MarketServiceModel();
                    result.ShopId = shopId;
                    result.Price = marketser.Price;
                    result.MarketType = CurMarketType;
                    if (market != null)
                    {
                        result.EndTime = market.ServiceEndTime;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以使用满减服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsCanUseMarketService(long shopId)
        {
            bool result = false;
            if (shopId <= 0)
            {
                throw new HimallException("错误的商家编号");
            }
            var market = GetMarketService(shopId);
            if (market != null)
            {
                if (market.IsBuy)
                {
                    result = !market.IsExpired;
                }
            }
            return result;
        }
        /// <summary>
        /// 购买满减服务
        /// </summary>
        /// <param name="month">数量(月)</param>
        /// <param name="shopId">店铺编号</param>
        public static void BuyMarketService(int month, long shopId)
        {

            if (shopId <= 0)
            {
                throw new HimallException("错误的商家编号");
            }
            if (month <= 0)
            {
                throw new HimallException("错误的购买数量(月)");
            }
            if (month > 120)
            {
                throw new HimallException("购买数量(月)过大");
            }
            MarketApplication.OrderMarketService(month, shopId, CurMarketType);
        }

        /// <summary>
        /// 获取服务购买列表
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<MarketServiceBuyRecordModel> GetMarketServiceBuyList(string shopName, int page = 1, int pagesize = 10)
        {
            QueryPageModel<MarketServiceBuyRecordModel> result = new QueryPageModel<MarketServiceBuyRecordModel>();
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = pagesize,
                PageNo = page,
                ShopName = shopName,
                MarketType = CurMarketType
            };

            ObsoletePageModel<MarketServiceRecordInfo> marketEntities = MarketApplication.GetBoughtShopList(queryModel);
            if (marketEntities.Total > 0)
            {
                result.Models = marketEntities.Models.Select(d => new MarketServiceBuyRecordModel
                {
                    Id = d.Id,
                    EndTime = d.EndTime,
                    MarketServiceId = d.MarketServiceId,
                    StartTime = d.StartTime,
                    SettlementFlag = d.SettlementFlag,
                    ShopName = d.ActiveMarketServiceInfo.ShopName
                }).ToList();
            }
            if (result.Models == null)
            {
                result.Models = new List<MarketServiceBuyRecordModel>();
            }
            result.Total = marketEntities.Total;

            return result;
        }
        #endregion
    }
}
