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
    public  class FightGroupApplication
    {
        private static IFightGroupService _iFightGroupService= ObjectContainer.Current.Resolve<IFightGroupService>();
        private static IMarketService _iMarketService= ObjectContainer.Current.Resolve<IMarketService>();
        private static IShopService _iShopService= ObjectContainer.Current.Resolve<IShopService>();
        private static IProductService _iProductService= ObjectContainer.Current.Resolve<IProductService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService= ObjectContainer.Current.Resolve<ILimitTimeBuyService>();

        /// <summary>
        /// 当前营销类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.FightGroup;

        #region 系统
        /// <summary>
        /// 拼团营销活动费用设置
        /// </summary>
        /// <returns></returns>
        public static decimal GetMarketServicePrice()
        {
            var marketser = _iMarketService.GetServiceSetting(CurMarketType);
            if (marketser == null)
            {
                marketser = new MarketSettingInfo() { TypeId = CurMarketType, Price = 0 };
                _iMarketService.AddOrUpdateServiceSetting(marketser);
            }
            return marketser.Price;
        }
        /// <summary>
        /// 设置拼团营销活动费用设置
        /// </summary>
        /// <param name="price"></param>
        public static void SetMarketServicePrice(decimal price)
        {
            MarketSettingInfo marketser = new MarketSettingInfo() { TypeId = CurMarketType, Price = price };
            _iMarketService.AddOrUpdateServiceSetting(marketser);
        }
        /// <summary>
        /// 是否已开启拼团营销
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenMarketService()
        {
            bool result = false;
            var marketser = _iMarketService.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price > 0)
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团营销服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static MarketServiceModel GetMarketService(long shopId)
        {
            MarketServiceModel result = null;
            var market = _iMarketService.GetMarketService(shopId, CurMarketType);
            var marketser = _iMarketService.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price > 0)
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
        /// 是否可以使用拼团服务
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
        /// 购买拼团服务
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
            _iMarketService.OrderMarketService(month, shopId, CurMarketType);
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

            PageModel<MarketServiceRecordInfo> marketEntities = _iMarketService.GetBoughtShopList(queryModel);
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

            return result;
        }
        #endregion

        #region 拼团活动
        /// <summary>
        /// 新增拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void AddActive(FightGroupActiveModel data)
        {
            Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            _iFightGroupService.AddActive(model);
        }

        /// <summary>
        /// 商品是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool ProductCanJoinActive(long productId)
        {
            bool result = false;
            result = _iFightGroupService.ProductCanJoinActive(productId);
            return result;
        }
        /// <summary>
        /// 修改拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void UpdateActive(FightGroupActiveModel data)
        {
            #region 更新活动
            Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            _iFightGroupService.UpdateActive(model);
            #endregion
        }
        /// <summary>
        /// 下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="manageRemark">下架原因</param>
        /// <param name="manageId">管理员编号</param>
        public static void CancelActive(long id, string manageRemark, long manageId)
        {
            _iFightGroupService.CancelActive(id, manageRemark, manageId);
        }
        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取商品的评价数量</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true)
        {
            var data = _iFightGroupService.GetActive(id, needGetProductCommentNumber, isLoadItems);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            FightGroupActiveModel result = Mapper.Map<FightGroupActiveInfo, FightGroupActiveModel>(data);
            if (result != null)
            {
                //商品图片地址修正
                result.ProductDefaultImage = HimallIO.GetProductSizeImage(data.ProductImgPath, 1, (int)ProductInfo.ImageSize.Size_350);
                if (!string.IsNullOrWhiteSpace(result.IconUrl))
                {
                    result.IconUrl = Himall.Core.HimallIO.GetImagePath(result.IconUrl);
                }
                if (result.ActiveItems != null)
                {
                    foreach (var item in result.ActiveItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ShowPic))
                        {
                            item.ShowPic = HimallIO.GetImagePath(item.ShowPic);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 使用商品编号获取正在进行的拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActiveByProductId(long productId)
        {
            long actid = _iFightGroupService.GetActiveIdByProductId(productId);
            FightGroupActiveModel result=null;
            if(actid>0)
            {
                result = GetActive(actid,false,false);
            }
            return result;
        }
        /// <summary>
        /// 获取拼团项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<FightGroupActiveItemModel> GetActiveItems(long id)
        {
            List<FightGroupActiveItemModel> result = new List<FightGroupActiveItemModel>();
            var data = _iFightGroupService.GetActiveItems(id);
            Mapper.CreateMap<List<FightGroupActiveItemInfo>, List<FightGroupActiveItemModel>>();
            result = Mapper.Map<List<FightGroupActiveItemModel>>(data);
            return result;
        }
        /// <summary>
        /// 获取拼团项用于新增活动
        /// </summary>
        /// <param name="productId">商品编号</param>
        /// <returns></returns>
        public static FightGroupGetSkuListModel GetNewActiveItems(long productId)
        {
            FightGroupGetSkuListModel result = new FightGroupGetSkuListModel();
            var pro = _iProductService.GetProduct(productId);
            result.ProductImg = HimallIO.GetProductSizeImage(pro.RelativePath, 1, 150);
            result.skulist = new List<FightGroupActiveItemModel>();
            foreach (var item in pro.SKUInfo)
            {
                FightGroupActiveItemModel _data = new FightGroupActiveItemModel();
                _data.ProductId = productId;
                _data.SkuId = item.Id;
                _data.SkuName = item.Color + " " + item.Size + " " + item.Version;
                _data.ProductPrice = item.SalePrice;
                _data.ProductStock = item.Stock;
                _data.ActivePrice = _data.ProductPrice;
                _data.ActiveStock = 0;  //活动库存置空
                result.skulist.Add(_data);
            }
            return result;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="Statuses">状态集</param>
        /// <param name="ProductName">商品名</param>
        /// <param name="ShopName">店铺名</param>
        /// <param name="ShopId">店铺编号</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupActiveListModel> GetActives(
            List<FightGroupActiveStatus> Statuses = null,
            string ProductName = "",
            string ShopName = "",
            long? ShopId = null,
            int PageNo = 1,
            int PageSize = 10
            )
        {
            QueryPageModel<FightGroupActiveListModel> result = null;
            var data = _iFightGroupService.GetActives(Statuses, null, null, ProductName, ShopName, ShopId, PageNo, PageSize);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveListModel>();
            Mapper.CreateMap<QueryPageModel<FightGroupActiveInfo>, QueryPageModel<FightGroupActiveListModel>>();
            result = Mapper.Map<QueryPageModel<FightGroupActiveListModel>>(data);
            if (result.Total > 0)
            {
                //数据映射
                foreach (var item in result.Models)
                {
                    if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    {
                        item.IconUrl = Himall.Core.HimallIO.GetImagePath(item.IconUrl);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团组团详情
        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="userId">团长用户编号</param>
        /// <returns>组团编号</returns>
        public static long OpenGroup(long activeId, long userId)
        {
            return _iFightGroupService.AddGroup(activeId, userId).Id;
        }
        /// <summary>
        /// 是否可以参团
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CanJoinGroup(long activeId, long groupId, long userId)
        {
            return _iFightGroupService.CanJoinGroup(activeId, groupId, userId);
        }
        /// <summary>
        /// 获取拼团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        /// <returns></returns>
        public static FightGroupsModel GetGroup(long activeId, long groupId)
        {
            var data = _iFightGroupService.GetGroup(activeId, groupId);
            Mapper.CreateMap<FightGroupsInfo, FightGroupsModel>();
            //子数据映射
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            FightGroupsModel result = Mapper.Map<FightGroupsModel>(data);

            if (result != null)
            {
                result.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(result.HeadUserIcon);
                if (result.GroupOrders != null)
                {
                    foreach (var subitem in result.GroupOrders)
                    {
                        subitem.Photo = Himall.Core.HimallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团详情列表
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="Statuses">状态集</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsListModel> GetGroups(
            long activeId,
            List<FightGroupBuildStatus> Statuses,
            DateTime? StartTime,
            DateTime? EndTime,
            int PageNo,
            int PageSize
            )
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsListModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetGroups(activeId, Statuses, StartTime, EndTime, PageNo, PageSize);
            QueryPageModel<FightGroupsListModel> result = new QueryPageModel<FightGroupsListModel>();
            result.Total = data.Total;
            result.Models = new List<FightGroupsListModel>();
            if (data.Total > 0)
            {
                foreach (var item in data.Models)
                {
                    item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                    var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                        {
                            _tmp.OrderIds.Add(ord.OrderId.Value);
                        }
                    }
                    result.Models.Add(_tmp);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取参与的拼团
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="Statuses">参与状态</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsModel> GetJoinGroups(
            long userId
            , List<FightGroupOrderJoinStatus> Statuses = null
            , int PageNo = 1
            , int PageSize = 10
            )
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var map = Mapper.CreateMap<QueryPageModel<FightGroupsInfo>, QueryPageModel<FightGroupsModel>>();
            var data = _iFightGroupService.GetJoinGroups(userId, Statuses, PageNo, PageSize);
            QueryPageModel<FightGroupsModel> result = Mapper.Map<QueryPageModel<FightGroupsModel>>(data);

            foreach (var item in result.Models)
            {
                item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                if (item.GroupOrders != null)
                {
                    foreach (var subitem in item.GroupOrders)
                    {
                        subitem.Photo = Himall.Core.HimallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团订单
        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetMarketSaleCountForUserId(long activeId, long userId)
        {
            return _iFightGroupService.GetMarketSaleCountForUserId(activeId, userId);
        }
        /// <summary>
        /// 拼团订单
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="orderId">订单编号</param>
        /// <param name="userId">用户编号</param>
        /// <param name="groupId">拼团编号 0表示开新团</param>
        public static FightGroupOrderModel AddOrder(long actionId, long orderId, long userId, long groupId = 0)
        {
            FightGroupOrderInfo data = _iFightGroupService.AddOrder(actionId, orderId, userId, groupId);
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);
            if (result != null)
            {
                result.Photo = Himall.Core.HimallIO.GetImagePath(result.Photo);
            }
            return result;
        }
        /// <summary>
        /// 设定加入拼团状态
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">状态</param>
        public static FightGroupOrderJoinStatus SetOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderJoinStatus result = _iFightGroupService.SetOrderStatus(orderId, status);
            return result;
        }
        /// <summary>
        /// 获取拼团订单
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public static FightGroupOrderModel GetOrder(long orderId)
        {
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetOrder(orderId);
            FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);
            if (result != null)
            {
                result.Photo = Himall.Core.HimallIO.GetImagePath(result.Photo);
            }
            return result;
        }

        /// <summary>
        /// 获取参团中的订单数
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        public static int CountJoiningOrder(long userId)
        {
            return _iFightGroupService.CountJoiningOrder(userId);
        }

        #endregion
    }
}
