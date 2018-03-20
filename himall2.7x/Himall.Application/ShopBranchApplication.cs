using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;
using Himall.IServices;
using Himall.Core;
using Himall.Application.Mappers;
using Himall.Model;
using Himall.Core.Helper;
using Himall.CommonModel;
using Himall.DTO.Product;
using Himall.IServices.QueryModel;

namespace Himall.Application
{
    /// <summary>
    /// 门店应用服务
    /// </summary>
    public class ShopBranchApplication
    {
        private static IShopBranchService _shopBranchService = ObjectContainer.Current.Resolve<IShopBranchService>();
        private static IAppMessageService _appMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();

        #region 密码加密处理
        /// <summary>
        /// 二次加盐后的密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GetPasswrodEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }
        /// <summary>
        /// 取密码盐
        /// </summary>
        /// <returns></returns>
        public static string GetSalt()
        {
            return Guid.NewGuid().ToString("N").Substring(12);
        }
        #endregion 密码加密处理

        #region 查询相关
        /// <summary>
        /// 获取门店
        /// </summary>
        /// <returns></returns>
        public static ShopBranch GetShopBranchById(long id)
        {
            var branchInfo = _shopBranchService.GetShopBranchById(id);
            if (branchInfo == null)
                return null;
            var branchManagers = _shopBranchService.GetShopBranchManagers(id);
            var shopBranch = AutoMapper.Mapper.Map<ShopBranchInfo, ShopBranch>(branchInfo);
            //补充地址中文名称
            //shopBranch.AddressFullName = RegionApplication.GetFullName(shopBranch.AddressId, CommonConst.ADDRESS_PATH_SPLIT);
            shopBranch.AddressFullName = RenderAddress(shopBranch.AddressPath,shopBranch.AddressDetail,0);
            if (branchManagers != null && branchManagers.Count() > 0)
            {//补充管理员名称
                shopBranch.UserName = branchManagers.FirstOrDefault().UserName;
            }
            return shopBranch;
        }


        /// <summary>
        /// 根据 IDs批量获取门店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchByIds(IEnumerable<long> ids)
        {
            var branchInfos = _shopBranchService.GetShopBranchByIds(ids);
            var shopBranchs = AutoMapper.Mapper.Map<List<ShopBranch>>(branchInfos);
            return shopBranchs;
        }

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static ShopBranch GetShopBranchByContact(string contact)
        {
            var branchInfo = _shopBranchService.GetShopBranchByContact(contact);
            return branchInfo.Map<DTO.ShopBranch>();
        }

        /// <summary>
        /// 分页查询门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    AddressFullName = RegionApplication.GetFullName(e.AddressId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + e.AddressDetail,
                    AddressId = e.AddressId,
                    ContactPhone = e.ContactPhone,
                    ContactUser = e.ContactUser,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    ShopId = e.ShopId,
                    Status = e.Status
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        public static List<ShopBranch> GetShopBranchByShopId(long shopId)
        {
            var shopBranch = _shopBranchService.GetShopBranchByShopId(shopId).ToList();
            return shopBranch.Map<List<ShopBranch>>();
        }
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchs(IEnumerable<long> ids)
        {
            var shopBranchs = _shopBranchService.GetShopBranchs(ids).Map<List<ShopBranch>>();
            //补充地址详细信息,地址库采用了缓存，循环取
            foreach (var b in shopBranchs)
            {
                b.AddressFullName = RegionApplication.GetFullName(b.AddressId);
                b.RegionIdPath = RegionApplication.GetRegionPath(b.AddressId);
            }
            return shopBranchs;
        }
        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds)
        {
            var list = _shopBranchService.GetSkus(shopId, shopBranchIds);
            return list.Map<List<ShopBranchSkusInfo>>();
        }
        /// <summary>
        /// 根据SKU AUTOID取门店SKU
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuids"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuids)
        {
            var list = _shopBranchService.GetSkusByIds(shopBranchId, skuids);
            return list;
        }
        /// <summary>
        /// 根据商品ID取门店sku信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSkusByProductId(long shopBranchId, long pid)
        {
            var sku = ProductManagerApplication.GetSKU(pid);
            var shopBranchSkus = _shopBranchService.GetSkusByIds(shopBranchId, sku.Select(e => e.Id));
            foreach (var item in sku)
            {
                var branchSku = shopBranchSkus.FirstOrDefault(e => e.SkuId == item.Id);
                if (branchSku != null)
                    item.Stock = branchSku.Stock;
            }
            return sku;
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopBranchManager GetShopBranchManager(long id)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersById(id);
            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            //管理员类型为门店管理员
            manager.UserType = ManagerType.ShopBranchManager;
            return manager;
        }

        /// <summary>
        /// 根据门店id获取门店管理员
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static List<DTO.ShopBranchManager> GetShopBranchManagerByShopBranchId(long shopBranchId)
        {
            return _shopBranchService.GetShopBranchManagers(shopBranchId).Map<List<DTO.ShopBranchManager>>();
        }

        /// <summary>
        /// 门店商品查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProducts(ShopBranchProductQuery query)
        {
            var pageModel = _shopBranchService.SearchProduct(query);
            //TODO:补充门店销售数量
            var orders = OrderApplication.GetOrdersNoPage(new OrderQuery { ShopBranchId = query.shopBranchId });
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Select(e => e.Id));
            var pids = pageModel.Models.Select(e => e.Id);
            var productSaleCounts = orderItems.Where(e => pids.Contains(e.ProductId)).GroupBy(o => o.ProductId).Select(e => new { productId = e.Key, saleCount = e.Sum(p => p.Quantity) });
            foreach (var p in pageModel.Models)
            {
                var productCount = productSaleCounts.FirstOrDefault(e => e.productId == p.Id);
                if (productCount != null)
                    p.SaleCounts = productCount.saleCount;
                else
                    p.SaleCounts = 0;//门店商品无销量则为0，不应用默认的商家商品销量

            }
            return pageModel;
        }
        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool Exists(CommonModel.ShopBranchQuery query)
        {
            return _shopBranchService.Exists(query);
        }

        /// <summary>
        /// 查询门店配送范围
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<DeliveryScope> GetShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            var shopDeliveryScopeInfos = _shopBranchService.GetShopDeliveryScope(query);
            QueryPageModel<DeliveryScope> shopBranchs = new QueryPageModel<DeliveryScope>
            {
                Models = shopDeliveryScopeInfos.Models.Select(e => new DeliveryScope
                {
                    FullRegionPath = e.FullRegionPath,
                    Id = e.Id,
                    RegionId = e.RegionId,
                    RegionName = e.RegionName,
                    ShopBranchId = e.ShopBranchId
                }).ToList(),
                Total = shopDeliveryScopeInfos.Total
            };
            return shopBranchs;
        }
        public static QueryPageModel<DeliveryScope> GetShopDeliveryScopeAll(ShopDeliveryScopeQuery query)
        {
            var shopDeliveryScopeInfos = _shopBranchService.GetShopDeliveryScopeAll(query);
            QueryPageModel<DeliveryScope> shopBranchs = new QueryPageModel<DeliveryScope>
            {
                Models = shopDeliveryScopeInfos.Models.Select(e => new DeliveryScope
                {
                    FullRegionPath = e.FullRegionPath,
                    Id = e.Id,
                    RegionId = e.RegionId,
                    RegionName = e.RegionName,
                    ShopBranchId = e.ShopBranchId
                }).ToList()
            };
            return shopBranchs;
        }

        #endregion

        #region 门店管理
        /// <summary>
        /// 新增门店
        /// </summary>
        public static void AddShopBranch(ShopBranch shopBranch, out long shopBranchId)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            var branchManangerInfo = _shopBranchService.GetShopBranchManagersByName(shopBranch.UserName);
            if (branchManangerInfo != null)
            {
                throw new HimallException("门店管理员名称不能重复！");
            }
            if (ManagerApplication.CheckUserNameExist(shopBranch.UserName))
            {
                throw new HimallException("门店管理员名称不能与商家重复！");
            }
            AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);
            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            _shopBranchService.AddShopBranch(shopBranchInfo);
            shopBranchId = shopBranchInfo.Id;
            var salt = GetSalt();
            var shopBranchManagerInfo = new ShopBranchManagersInfo
            {
                CreateDate = DateTime.Now,
                UserName = shopBranch.UserName,
                ShopBranchId = shopBranchInfo.Id,
                PasswordSalt = salt,
                Password = GetPasswrodEncode(shopBranch.PasswordOne, salt)
            };
            _shopBranchService.AddShopBranchManagers(shopBranchManagerInfo);
        }
        /// <summary>
        /// 更新门店信息、管理员密码
        /// </summary>
        /// <param name="shopBranch"></param>
        public static void UpdateShopBranch(ShopBranch shopBranch)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);

            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            _shopBranchService.UpdateShopBranch(shopBranchInfo);
            if (!string.IsNullOrWhiteSpace(shopBranch.PasswordOne) && !string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
            {//编辑时可以不输入密码
                var salt = GetSalt();//取salt
                var encodePwd = GetPasswrodEncode(shopBranch.PasswordOne, salt);

                _shopBranchService.UpdateShopBranchManagerPwd(shopBranch.Id, shopBranch.UserName, encodePwd, salt);
            }
        }

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        public static void UpdateShopBranchManagerPwd(long managerId, string password)
        {
            _shopBranchService.UpdateShopBranchManagerPwd(managerId, password);
        }

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="branchId"></param>
        public static void DeleteShopBranch(long branchId)
        {
            //TODO:门店删除逻辑

            _shopBranchService.DeleteShopBranch(branchId);
        }
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void Freeze(long shopBranchId)
        {
            _shopBranchService.FreezeShopBranch(shopBranchId);
        }
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void UnFreeze(long shopBranchId)
        {
            _shopBranchService.UnFreezeShopBranch(shopBranchId);
        }
        #endregion 门店管理

        #region 门店登录
        /// <summary>
        /// 门店登录验证
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ShopBranchManager ShopBranchLogin(string userName, string password)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersByName(userName);
            if (managerInfo == null)
                return null;

            password = GetPasswrodEncode(password, managerInfo.PasswordSalt);
            if (!string.Equals(password, managerInfo.Password))
                return null;

            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            return manager;
        }
        #endregion 门店登录

        #region 门店商品管理
        /// <summary>
        /// 添加SKU，并过滤已添加的
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void AddProductSkus(IEnumerable<long> pids, long shopBranchId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProductsByIds(pids).Where(e => e.ShopId == shopId);
            if (productsInfo == null)
                throw new HimallException("未找到商品数据");
            //查询已添加的SKU，用于添加时过滤
            var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId },null).Select(e => e.SkuId);
            var allSkus = SKUApplication.GetByProductIds(productsInfo.Select(p => p.Id));
            var shopBranchSkus = new List<ShopBranchSkusInfo> { };

            var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
            {
                ProductId = e.ProductId,
                SkuId = e.Id,
                ShopId = shopId,
                ShopBranchId = shopBranchId,
                Stock = 0,
                CreateDate = DateTime.Now
            });
            shopBranchSkus.AddRange(skus);

            _shopBranchService.AddSkus(shopBranchSkus);
        }
        /// <summary>
        /// 修正商品sku
        /// <para>0库存添加新的sku</para>
        /// </summary>
        /// <param name="productId"></param>
        public static void CorrectBranchProductSkus(long productId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProduct(productId);
            if (productsInfo == null || productsInfo.ShopId != shopId)
            {
                throw new HimallException("未找到商品数据");
            }
            var shopbrids = _shopBranchService.GetAgentShopBranchIds(productId);
            List<long> pids = new List<long>();
            pids.Add(productId);

            foreach (var shopBranchId in shopbrids)
            {
                //查询已添加的SKU，用于添加时过滤
                var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
                var allSkus = SKUApplication.GetByProductIds(pids);
                var shopBranchSkus = new List<ShopBranchSkusInfo> { };

                var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
                {
                    ProductId = e.ProductId,
                    SkuId = e.Id,
                    ShopId = shopId,
                    ShopBranchId = shopBranchId,
                    Stock = 0,
                    CreateDate = DateTime.Now
                });
                shopBranchSkus.AddRange(skus);

                _shopBranchService.AddSkus(shopBranchSkus);
            }
        }
        /// <summary>
        /// 设置门店SKU库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetSkuStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceStock(shopBranchId, skuIds, stock);
                    break;
            }
        }
        /// <summary>
        /// 修改门店商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceProductStock(shopBranchId, pids, stock);
                    break;
            }
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void UnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 下架所有门店的商品
        /// <para></para>
        /// </summary>
        /// <param name="productId"></param>
        public static void UnSaleProduct(long productId)
        {
            _shopBranchService.SetBranchProductStatus(productId, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void OnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.Normal);
        }
        #endregion 门店商品管理

        #region 私有方法
        private static bool isRepeatBranchName(long shopId, long shopBranchId, string branchName)
        {
            var exists = _shopBranchService.Exists(shopId, shopBranchId, branchName);
            return exists;
        }
        #endregion
        /// <summary>
        /// 取门店商品数量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<ShopBranchSkusInfo> GetShopBranchProductCount(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var skus = _shopBranchService.SearchShopBranchSkus(shopBranchId, startDate, endDate);
            return skus;
        }
        #region 门店配送范围
        /// <summary>
        /// 新增门店配送范围
        /// </summary>
        /// <summary>
        /// 批量新增门店配送范围
        /// </summary>
        /// <param name="deliveryScopeList"></param>
        public static void AddShopDeliveryScope(List<DeliveryScope> deliveryScopeList)
        {
            List<DeliveryScopeInfo> list = new List<DeliveryScopeInfo>();
            foreach (var item in deliveryScopeList)
            {
                AutoMapper.Mapper.CreateMap<DeliveryScope, DeliveryScopeInfo>();
                var deliveryScopeInfo = AutoMapper.Mapper.Map<DeliveryScope, DeliveryScopeInfo>(item);
                list.Add(deliveryScopeInfo);
            }
            _shopBranchService.AddShopDeliveryScope(list);
        }
        /// <summary>
        /// 更新门店配送范围
        /// </summary>
        /// <param name="shopBranch"></param>
        public static void UpdateShopDeliveryScope(DeliveryScope deliveryScope)
        {
            AutoMapper.Mapper.CreateMap<DeliveryScope, DeliveryScopeInfo>();
            var shopDeliveryScopeInfo = AutoMapper.Mapper.Map<DeliveryScope, DeliveryScopeInfo>(deliveryScope);
            _shopBranchService.UpdateShopDeliveryScope(shopDeliveryScopeInfo);
        }
        /// <summary>
        /// 删除门店配送范围
        /// </summary>
        /// <param name="branchId"></param>
        public static void DeleteShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            _shopBranchService.DeleteShopDeliveryScope(query);
        }
        /// <summary>
        /// 是否已存在相同的门店配送范围
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool ExistsShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            return _shopBranchService.ExistsShopDeliveryScope(query);
        }
        #endregion
        #region 周边门店
        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetNearShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetNearShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        /// <summary>
        /// 组合新地址
        /// </summary>
        /// <param name="addressPath"></param>
        /// <param name="address"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string RenderAddress(string addressPath, string address, int level)
        {
            if (!string.IsNullOrWhiteSpace(addressPath))
            {
                string fullName = RegionApplication.GetRegionName(addressPath);
                string[] arr = fullName.Split(',');//省，市，区，街道
                if (arr.Length > 0)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        address = address.Replace(arr[i], "");//去掉原详细地址中的省市区街道。(为兼容旧门店数据)
                    }
                }

                if (level <= arr.Length)
                {
                    for (int i = 0; i < level; i++)
                    {
                        fullName = fullName.Replace(arr[i], "");
                    }
                    address = fullName + address;
                }
            }
            return address.Replace(",", "");
        }
        public static QueryPageModel<ShopBranch> GetShopBranchsAll(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchsAll(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = TypeHelper.ObjectToInt(e.ServeRadius),
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    AddressPath = e.AddressPath
                }).ToList()
            };
            return shopBranchs;
        }
        #endregion
        #region 商家手动分配门店
        /// <summary>
        /// 获取商家下该区域范围内的可选门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetArealShopBranchsAll(int areaId, int shopId)
        {
            var shopBranchInfos = _shopBranchService.GetArealShopBranchsAll(areaId, shopId);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                }).ToList()
            };
            return shopBranchs;
        }
        #endregion
    }
}
