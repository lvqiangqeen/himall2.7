using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;
using Himall.Entity;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Core.Helper;
using Dapper;
using MySql.Data.MySqlClient;

namespace Himall.Service
{
    public class ShopBranchService : ServiceBase, IShopBranchService
    {
        /// <summary>
        /// 添加分店
        /// </summary>
        /// <param name="shopBranchInfo"></param>
        public void AddShopBranch(Model.ShopBranchInfo shopBranchInfo)
        {
            Context.ShopBranchInfo.Add(shopBranchInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 添加分店管理员
        /// </summary>
        /// <param name="shopBranchManagersInfo"></param>
        public void AddShopBranchManagers(Model.ShopBranchManagersInfo shopBranchManagersInfo)
        {
            Context.ShopBranchManagersInfo.Add(shopBranchManagersInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 判断门店名称是否重复
        /// </summary>
        /// <param name="shopId">商家店铺ID</param>
        /// <param name="shopBranchName">门店名字</param>
        /// <returns></returns>
        public bool Exists(long shopId, long shopBranchId, string shopBranchName)
        {
            return Context.ShopBranchInfo.Any(e => e.ShopBranchName == shopBranchName && e.ShopId == shopId && e.Id != shopBranchId);
        }

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool Exists(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = ToWhere(query);

            return shopBranchs.Any();
        }

        public ShopBranchInfo GetShopBranchById(long id)
        {
            return Context.ShopBranchInfo.FirstOrDefault(e => e.Id == id);
        }


        /// <summary>
        /// 根据门店IDs获取门店
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public List<ShopBranchInfo> GetShopBranchByIds(IEnumerable<long> Ids)
        {
            return Context.ShopBranchInfo.Where(e => Ids.Contains(e.Id)).ToList();
        }


        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ShopBranchInfo GetShopBranchByContact(string contact)
        {
            return this.Context.ShopBranchInfo.FirstOrDefault(p => p.ContactPhone == contact);
        }

        public QueryPageModel<ShopBranchInfo> GetShopBranchs(CommonModel.ShopBranchQuery query)
        {
            int total = 0;
            var shopBranchs = ToWhere(query);
            shopBranchs = shopBranchs.GetPage(out total, query.PageNo, query.PageSize, s => s.OrderBy(e => e.Id));
            QueryPageModel<ShopBranchInfo> pageModel = new QueryPageModel<ShopBranchInfo>() { Models = shopBranchs.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> GetNearShopBranchs(CommonModel.ShopBranchQuery query)
        {
            decimal latitude = 0, longitude = 0;
            if (query.FromLatLng.Split(',').Length != 2) return new QueryPageModel<ShopBranchInfo>();
            latitude = TypeHelper.ObjectToDecimal(query.FromLatLng.Split(',')[0]);
            longitude = TypeHelper.ObjectToDecimal(query.FromLatLng.Split(',')[1]);

            QueryPageModel<ShopBranchInfo> result = new QueryPageModel<ShopBranchInfo>();
            DynamicParameters parms = new DynamicParameters();
            string countsql = "select count(1) from himall_shopbranch ";
            string sql = string.Format("select AddressDetail,AddressPath,ContactPhone,Id,ShopBranchName,Status,Latitude,Longitude,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({0}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({1}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance from himall_shopbranch", latitude, longitude);
            string where = GetSearchWhere(query, parms);
            string order = GetSearchOrder(query);
            if (query.OrderKey == 2)
            {
                order = string.Format(" {0}, id desc ", order.TrimEnd(','));//如果存在相同距离，则按ID再次排序
            }
            //暂时不考虑索引
            string page = GetSearchPage(query);
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                result.Models = conn.Query<ShopBranchInfo>(string.Concat(sql, where, order, page), parms).ToList();
                result.Total = int.Parse(conn.ExecuteScalar(string.Concat(countsql, where), parms).ToString());
            }
            return result;
        }
        public QueryPageModel<ShopBranchInfo> GetShopBranchsAll(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = ToWhere(query).ToList();
            shopBranchs.ForEach(p =>
            {
                if (p.Latitude.HasValue && p.Longitude.HasValue && p.Latitude.Value > 0 && p.Longitude.Value > 0)
                    p.Distance = GetLatLngDistancesFromAPI(query.FromLatLng, string.Format("{0},{1}", p.Latitude, p.Longitude));
                else
                    p.Distance = 0;
            });
            return new QueryPageModel<ShopBranchInfo>() { Models = shopBranchs.AsQueryable().OrderBy(p => p.Distance).ToList() };
        }
        /// <summary>
        /// 获取门店配送范围在同一区域的门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> GetArealShopBranchsAll(int areaId, int shopId)
        {
            return new QueryPageModel<ShopBranchInfo>() { Models = ToArealShopWhere(areaId, shopId).ToList() };
        }
        /// <summary>
        /// 获取一个起点坐标到多个终点坐标之间的距离
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="toLatLngs"></param>
        /// <returns></returns>
        private double GetLatLngDistancesFromAPI(string fromLatLng, string latlng)
        {
            if (!string.IsNullOrWhiteSpace(fromLatLng) && (!string.IsNullOrWhiteSpace(latlng)))
            {
                try
                {
                    var aryLatlng = fromLatLng.Split(',');
                    var fromlat = double.Parse(aryLatlng[0]);
                    var fromlng = double.Parse(aryLatlng[1]);
                    double EARTH_RADIUS = 6378.137;//地球半径

                    var aryToLatlng = latlng.Split(',');
                    var tolat = double.Parse(aryToLatlng[0]);
                    var tolng = double.Parse(aryToLatlng[1]);
                    var fromRadLat = fromlat * Math.PI / 180.0;
                    var toRadLat = tolat * Math.PI / 180.0;
                    double a = fromRadLat - toRadLat;
                    double b = (fromlng * Math.PI / 180.0) - (tolng * Math.PI / 180.0);
                    double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                     Math.Cos(fromRadLat) * Math.Cos(toRadLat) * Math.Pow(Math.Sin(b / 2), 2)));
                    s = s * EARTH_RADIUS;
                    return Math.Round((Math.Round(s * 10000) / 10000), 2);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("计算经纬度距离异常", ex);
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ShopBranchInfo> GetShopBranchs(IEnumerable<long> ids)
        {
            return this.Context.ShopBranchInfo.Where(p => ids.Contains(p.Id)).ToList();
        }

        public void FreezeShopBranch(long shopBranchId)
        {
            var shopBranch = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == shopBranchId);
            shopBranch.Status = CommonModel.ShopBranchStatus.Freeze;
            Context.SaveChanges();
        }

        public void UnFreezeShopBranch(long shopBranchId)
        {
            var shopBranch = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == shopBranchId);
            shopBranch.Status = CommonModel.ShopBranchStatus.Normal;
            Context.SaveChanges();
        }

        public IEnumerable<ShopBranchManagersInfo> GetShopBranchManagers(long branchId)
        {
            var managers = Context.ShopBranchManagersInfo.Where(e => e.ShopBranchId == branchId);
            return managers.ToList();
        }

        public void UpdateShopBranch(ShopBranchInfo shopBranch)
        {
            var branchEntity = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == shopBranch.Id);
            if (branchEntity == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");
            branchEntity.ShopBranchName = shopBranch.ShopBranchName;
            branchEntity.AddressDetail = shopBranch.AddressDetail;
            branchEntity.AddressId = shopBranch.AddressId;
            branchEntity.ContactPhone = shopBranch.ContactPhone;
            branchEntity.ContactUser = shopBranch.ContactUser;
            branchEntity.AddressPath = shopBranch.AddressPath;
            branchEntity.ShopImages = shopBranch.ShopImages;
            branchEntity.Longitude = shopBranch.Longitude;
            branchEntity.Latitude = shopBranch.Latitude;
            branchEntity.ServeRadius = shopBranch.ServeRadius;
            Context.SaveChanges();
        }

        public void UpdateShopBranchManagerPwd(long branchId, string userName, string pwd, string pwdSalt)
        {
            var branchManager = Context.ShopBranchManagersInfo.FirstOrDefault(e => e.ShopBranchId == branchId && e.UserName == userName);
            if (branchManager == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");
            branchManager.Password = pwd;
            branchManager.PasswordSalt = pwdSalt;
            Context.SaveChanges();
        }

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        public void UpdateShopBranchManagerPwd(long managerId, string password)
        {
            var branchManager = Context.ShopBranchManagersInfo.FirstOrDefault(e => e.Id == managerId);
            if (branchManager == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");

            branchManager.Password = SecureHelper.MD5(SecureHelper.MD5(password) + branchManager.PasswordSalt);
            Context.SaveChanges();
        }

        public void DeleteShopBranch(long id)
        {
            var branchShop = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == id);
            if (branchShop == null)
                throw new Himall.Core.HimallException("未找到门店，删除失败！");
            Context.ShopBranchInfo.Remove(branchShop);
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <param name="status">null表示所有</param>
        /// <returns></returns>
        public List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds, ShopBranchSkuStatus? status= ShopBranchSkuStatus.Normal)
        {
            var sql = Context.ShopBranchSkusInfo.Where(p => p.ShopId == shopId && shopBranchIds.Contains(p.ShopBranchId));
            if (status.HasValue)
            {
                sql = sql.Where(p=> p.Status== status.Value);
            }
            return sql.ToList();
        }
        /// <summary>
        /// 根据SKUID取门店SKU
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuIds)
        {
            return this.Context.ShopBranchSkusInfo.Where(p => skuIds.Contains(p.SkuId) && p.ShopBranchId == shopBranchId).ToList();
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopBranchManagersInfo GetShopBranchManagersById(long id)
        {
            return Context.ShopBranchManagersInfo.FirstOrDefault(e => e.Id == id);
        }


        public ShopBranchManagersInfo GetShopBranchManagersByName(string userName)
        {
            return Context.ShopBranchManagersInfo.FirstOrDefault(e => e.UserName == userName);
        }

        /// <summary>
        /// 添加门店sku
        /// </summary>
        /// <param name="infos"></param>
        public void AddSkus(IEnumerable<ShopBranchSkusInfo> infos)
        {
            Context.ShopBranchSkusInfo.AddRange(infos);
            Context.Database.ExecuteSqlCommandAsync("DELETE from himall_shopbranchskus where SkuId not in (SELECT Id from himall_skus)");
            Context.SaveChanges();
        }

        #region 修改库存
        public void SetStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = stk[i];
                i++;
            }
            Context.SaveChanges();
        }
        public void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = stock;
            }
            Context.SaveChanges();
        }
        public void AddStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stk[i];
            }
            Context.SaveChanges();
        }
        public void AddProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stock;
            }
            Context.SaveChanges();
        }
        public void ReduceStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stk[i]) > 0 ? sku.Stock - stk[i] : 0;
            }
            Context.SaveChanges();
        }

        public void ReduceProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stock) > 0 ? sku.Stock - stock : 0;
            }
            Context.SaveChanges();
        }
        #endregion 修改库存

        public void SetBranchProductStatus(long shopBranchId, IEnumerable<long> pIds, ShopBranchSkuStatus status)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pIds.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Status = status;
            }
            Context.SaveChanges();
        }

        public void SetBranchProductStatus(long productId, ShopBranchSkuStatus status)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ProductId== productId);
            //if (skus.Count() == 0)
            //    throw new HimallException("参数异常,未找到数据");
            if (skus != null && skus.Count() > 0)
            {
                foreach (var sku in skus)
                {
                    sku.Status = status;
                }
                Context.SaveChanges();
            }
        }

        public QueryPageModel<ProductInfo> SearchProduct(ShopBranchProductQuery productQueryModel)
        {
            var products = Context.ProductInfo.Where(item => true);
            //过滤已删除的商品
            products = products.Where(item => item.IsDeleted == false);

            if (productQueryModel.Ids != null && productQueryModel.Ids.Count() > 0)//条件 编号
                products = products.Where(item => productQueryModel.Ids.Contains(item.Id));

            if (!string.IsNullOrWhiteSpace(productQueryModel.ProductCode))
                products = products.Where(item => item.ProductCode == productQueryModel.ProductCode);

            if (productQueryModel.ShopId.HasValue)//过滤店铺
            {
                products = products.Where(item => item.ShopId == productQueryModel.ShopId);
                if (productQueryModel.IsOverSafeStock.HasValue)
                {
                    List<long> pids = products.Select(e => e.Id).ToList();
                    if (productQueryModel.IsOverSafeStock.Value)
                    {
                        pids = Context.SKUInfo.Where(e => e.SafeStock.Value >= e.Stock && pids.Contains(e.ProductId)).Select(e => e.ProductId).ToList();
                    }
                    else
                    {
                        pids = Context.SKUInfo.Where(e => e.SafeStock.Value < e.Stock && pids.Contains(e.ProductId)).Select(e => e.ProductId).ToList();
                    }
                    products = products.Where(e => pids.Contains(e.Id));
                }
            }
            if (productQueryModel.AuditStatus != null)//条件 审核状态
                products = products.Where(item => productQueryModel.AuditStatus.Contains(item.AuditStatus));

            if (productQueryModel.SaleStatus.HasValue)
            {
                products = products.Where(item => item.SaleStatus == productQueryModel.SaleStatus);
            }

            if (productQueryModel.CategoryId.HasValue)//条件 分类编号
                products = products.Where(item => ("|" + item.CategoryPath + "|").Contains("|" + productQueryModel.CategoryId.Value + "|"));

            if (productQueryModel.NotIncludedInDraft)
            {
                products = products.Where(item => item.SaleStatus != ProductInfo.ProductSaleStatus.InDraft);
            }

            if (productQueryModel.StartDate.HasValue)//添加日期筛选
                products = products.Where(item => item.AddedDate >= productQueryModel.StartDate);
            if (productQueryModel.EndDate.HasValue)//添加日期筛选
            {
                var end = productQueryModel.EndDate.Value.Date.AddDays(1);
                products = products.Where(item => item.AddedDate < end);
            }
            if (!string.IsNullOrWhiteSpace(productQueryModel.KeyWords))// 条件 关键字
                products = products.Where(item => item.ProductName.Contains(productQueryModel.KeyWords));

            if (!string.IsNullOrWhiteSpace(productQueryModel.ShopName))//查询商家关键字
            {
                var shopIds = Context.ShopInfo.FindBy(item => item.ShopName.Contains(productQueryModel.ShopName)).Select(item => item.Id);
                products = products.Where(item => shopIds.Contains(item.ShopId));
            }
            if (productQueryModel.IsLimitTimeBuy)
            {
                var limits = Context.LimitTimeMarketInfo.Where(l => l.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing).Select(l => l.ProductId);
                products = products.Where(p => !limits.Contains(p.Id));
            }
            if (productQueryModel.shopBranchId.HasValue && productQueryModel.shopBranchId.Value != 0)
            {//过滤门店已选商品
                var pid = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == productQueryModel.shopBranchId.Value).Select(item => item.ProductId).Distinct();
                products = products.Where(e => pid.Any(id => id == e.Id));
            }
            if (productQueryModel.ShopBranchProductStatus.HasValue)
            {//门店商品状态
                var pid = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == productQueryModel.shopBranchId.Value && e.Status == productQueryModel.ShopBranchProductStatus.Value).Select(item => item.ProductId).Distinct();
                products = products.Where(e => pid.Any(id => id == e.Id));
            }

            long shopCateogryId = productQueryModel.ShopCategoryId.GetValueOrDefault();

            var proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.Id));
            switch (productQueryModel.OrderKey)
            {
                case 2:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.AddedDate));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.AddedDate));
                    }
                    break;
                case 3:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.SaleCounts));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.SaleCounts));
                    }
                    break;
                default:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.Id));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.Id));
                    }
                    break;
            }

            //店铺分类
            IEnumerable<long> productIds = new long[] { };
            if (productQueryModel.ShopCategoryId.HasValue)
            {
                productIds = Context.ProductShopCategoryInfo
                    .Where(
                          item => item.ShopCategoryInfo.Id == shopCateogryId ||
                                  item.ShopCategoryInfo.ParentCategoryId == shopCateogryId).Select(item => item.ProductId);
            }

            int total = products.Count();
            products = products.Where(item => (shopCateogryId == 0 || productIds.Contains(item.Id)));
            products = products.GetPage(out total, proorder, productQueryModel.PageNo, productQueryModel.PageSize);

            QueryPageModel<ProductInfo> pageModel = new QueryPageModel<ProductInfo>()
            {
                Total = total,
                Models = products.ToList()
            };
            return pageModel;
        }
        #region 私有方法
        private IQueryable<ShopBranchInfo> ToWhere(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = Context.ShopBranchInfo.Where(p => true);
            if (query.Id > 0)
            {
                shopBranchs = shopBranchs.Where(p => p.Id == query.Id);//查询单个门店的时候要用到
            }
            if (query.ShopId > 0)//取商家下的门店数据
            {
                shopBranchs = shopBranchs.Where(p => p.ShopId == query.ShopId);
            }
            if (query.CityId > 0)//同城市的门店 省,市,区,街
            {
                shopBranchs = shopBranchs.Where(p => p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.CityId + CommonConst.ADDRESS_PATH_SPLIT));
            }

            if (!string.IsNullOrWhiteSpace(query.ShopBranchName))
            {
                shopBranchs = shopBranchs.Where(e => e.ShopBranchName.Contains(query.ShopBranchName));
            }
            if (!string.IsNullOrWhiteSpace(query.ContactUser))
            {
                shopBranchs = shopBranchs.Where(e => e.ContactUser.Contains(query.ContactUser));
            }
            if (!string.IsNullOrWhiteSpace(query.ContactPhone))
            {
                shopBranchs = shopBranchs.Where(e => e.ContactPhone.Contains(query.ContactPhone));
            }
            if (query.Status.HasValue)
            {
                var status = query.Status.Value;
                shopBranchs = shopBranchs.Where(e => e.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(query.AddressPath))
            {
                var addressPath = query.AddressPath;
                if (!addressPath.EndsWith(CommonConst.ADDRESS_PATH_SPLIT))
                    addressPath += CommonConst.ADDRESS_PATH_SPLIT;
                shopBranchs = shopBranchs.Where(p => p.AddressPath.StartsWith(addressPath));
            }
            if (query.ProductIds != null && query.ProductIds.Length > 0)
            {
                var pids = query.ProductIds.Distinct().ToArray();
                var length = pids.Length;

                var shopBranchIds = this.Context.ShopBranchSkusInfo.Where(p => p.ShopId == query.ShopId && pids.Contains(p.ProductId))
                    .GroupBy(p => new { p.ShopBranchId, p.ProductId })
                    .GroupBy(p => p.Key.ShopBranchId)
                    .Where(p => p.Count() == length)
                    .Select(p => p.Key);

                shopBranchs = shopBranchs.Where(p => shopBranchIds.Contains(p.Id));
            }

            return shopBranchs;
        }
        #endregion


        public IEnumerable<ShopBranchSkusInfo> SearchShopBranchSkus(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var branchSkus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId);
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                branchSkus = branchSkus.Where(e => e.CreateDate >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                branchSkus = branchSkus.Where(e => e.CreateDate < end);
            }
            return branchSkus.ToList();

        }
        /// <summary>
        /// 周边门店匹配查询条件
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IQueryable<ShopBranchInfo> ToNearShopWhere(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = Context.ShopBranchInfo.Where(p => p.Longitude > 0 && p.Latitude > 0);//周边门店只取定位了经纬度的数据
            if (query.ShopId > 0)//取商家下的门店数据
            {
                shopBranchs = shopBranchs.Where(p => p.ShopId == query.ShopId);
            }
            if (query.CityId > 0)//同城门店
            {
                shopBranchs = shopBranchs.Where(p => p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.CityId + CommonConst.ADDRESS_PATH_SPLIT));
            }
            if (query.Status.HasValue)
            {
                shopBranchs = shopBranchs.Where(e => e.Status == query.Status.Value);
            }
            return shopBranchs;
        }

        /// <summary>
        /// 获取门店配送范围在同一区域的正常门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        private IQueryable<ShopBranchInfo> ToArealShopWhere(int areaId, int shopId)
        {
            var shopBranchs = Context.ShopBranchInfo.Where(p => p.Status == CommonModel.ShopBranchStatus.Normal);
            if (shopId > 0)
            {
                shopBranchs = shopBranchs.Where(p => p.ShopId == shopId);//过滤出商家下门店
            }
            if (areaId > 0)
            {
                var shopBranchIds = this.Context.DeliveryScopeInfo.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + areaId + CommonConst.ADDRESS_PATH_SPLIT))
                    .GroupBy(p => new { p.ShopBranchId })
                    .GroupBy(p => p.Key.ShopBranchId)
                    .Select(p => p.Key);
                shopBranchs = shopBranchs.Where(p => shopBranchIds.Contains(p.Id));
            }
            return shopBranchs;
        }
        public IEnumerable<ShopBranchInfo> GetShopBranchByShopId(long shopId)
        {
            return Context.ShopBranchInfo.Where(e => e.ShopId == shopId);
        }
        /// <summary>
        /// 分页查询门店配送范围
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<DeliveryScopeInfo> GetShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            int total = 0;
            var shopDeliveryScope = ToWhereDeliveryScope(query);
            shopDeliveryScope = shopDeliveryScope.GetPage(out total, query.PageNo, query.PageSize, s => s.OrderBy(e => e.Id));
            return new QueryPageModel<DeliveryScopeInfo>() { Models = shopDeliveryScope.ToList(), Total = total };
        }
        public QueryPageModel<DeliveryScopeInfo> GetShopDeliveryScopeAll(ShopDeliveryScopeQuery query)
        {
            return new QueryPageModel<DeliveryScopeInfo>() { Models = ToWhereDeliveryScope(query).ToList() };
        }
        private IQueryable<DeliveryScopeInfo> ToWhereDeliveryScope(ShopDeliveryScopeQuery query)
        {
            var shopBranchDeliveryScope = Context.DeliveryScopeInfo.Where(p => true);
            if (query.ShopBranchId > 0)
            {
                shopBranchDeliveryScope = Context.DeliveryScopeInfo.Where(p => p.ShopBranchId == query.ShopBranchId);
            }
            return shopBranchDeliveryScope;
        }
        /// <summary>
        /// 批量新增门店配送范围
        /// </summary>
        /// <param name="deliveryScopeInfoList"></param>
        public void AddShopDeliveryScope(IEnumerable<DeliveryScopeInfo> deliveryScopeInfoList)
        {
            Context.DeliveryScopeInfo.AddRange(deliveryScopeInfoList);
            Context.SaveChanges();
        }
        /// <summary>
        /// 更新门店配送范围
        /// </summary>
        /// <param name="deliveryScopeInfo"></param>
        public void UpdateShopDeliveryScope(DeliveryScopeInfo deliveryScopeInfo)
        {
            var deliveryScopeEntity = Context.DeliveryScopeInfo.FirstOrDefault(e => e.Id == deliveryScopeInfo.Id);
            if (deliveryScopeEntity == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");
            deliveryScopeEntity.RegionId = deliveryScopeInfo.RegionId;
            deliveryScopeEntity.RegionName = deliveryScopeInfo.RegionName;
            deliveryScopeEntity.FullRegionPath = deliveryScopeInfo.FullRegionPath;
            Context.SaveChanges();
        }
        /// <summary>
        /// 根据条件删除门店配送范围
        /// </summary>
        /// <param name="query"></param>
        public void DeleteShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            var deliveryScope = Context.DeliveryScopeInfo.Where(e => e.ShopBranchId == query.ShopBranchId);
            if (deliveryScope == null)
                throw new Himall.Core.HimallException("未找到门店配送范围，删除失败！");
            if (query.RegionIdList != null && query.RegionIdList.Count > 0)
            {
                string deleteIds = string.Join(",", query.RegionIdList.ToArray());
                deliveryScope = deliveryScope.Where(e => !deleteIds.Contains(e.RegionId.ToString()));
            }
            Context.DeliveryScopeInfo.RemoveRange(deliveryScope);
            Context.SaveChanges();
        }
        /// <summary>
        /// 同一门店是否存在相同的区域标识
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool ExistsShopDeliveryScope(ShopDeliveryScopeQuery query)
        {
            return Context.DeliveryScopeInfo.Any(e => e.ShopBranchId == query.ShopBranchId && e.RegionId == query.RegionId);
        }
        /// <summary>
        /// 订单自动分配到门店
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skuIds">订单内商品SkuId</param>
        /// <param name="counts">订单内商品购买数量</param>
        /// <returns></returns>
        public ShopBranchInfo GetAutoMatchShopBranch(ShopBranchQuery query, string[] skuIds, int[] counts)
        {
            ShopBranchInfo resultObj = null;
            var skuInfos = Context.SKUInfo.Where(p => skuIds.Contains(p.Id)).ToList();
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            var data = GetShopBranchsAll(query);//获取商家下的有该产品SKU的状态正常门店

            var shopBranchSkus = GetSkus(query.ShopId, data.Models.Select(p => p.Id));//获取当前商家下门店的SKU
            data.Models.ForEach(p =>
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id))
            );

            var result = data.Models.Where(p => p.Enabled).ToList();//只取商家下都有该商品SKU库存的门店数据

            bool fromLatLng = false;//用户收货地址是否定位了经纬度
            if (!string.IsNullOrWhiteSpace(query.FromLatLng))
            {
                fromLatLng = query.FromLatLng.Split(',').Length == 2;
            }

            if (result.Count > 0)
            {
                if (fromLatLng)//优先用服务半径匹配,取距离最近的、又有库存的门店。前提要收货地址定位了经纬度
                    resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && p.ServeRadius >= p.Distance).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();

                if (resultObj == null)//如果服务半径无法满足，则根据配送范围去匹配
                {
                    var deliveryScope = GetShopDeliveryScopeAll(new ShopDeliveryScopeQuery() { });
                    if (query.StreetId > 0) //优先筛选出与买家收货地址同街道的所有门店
                    {
                        List<long> shopBrachIds = deliveryScope.Models.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.StreetId + CommonConst.ADDRESS_PATH_SPLIT)).Select(p => p.ShopBranchId).Distinct().ToList();
                        if (shopBrachIds.Count > 0)
                        {
                            if (shopBrachIds.Count > 1)//如果匹配多个则取距离最近一个
                            {
                                if (fromLatLng)
                                    resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && shopBrachIds.Contains(p.Id)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();
                            }
                            else
                            {
                                resultObj = result.Where(p => shopBrachIds.Contains(p.Id)).FirstOrDefault<ShopBranchInfo>();
                            }
                        }
                    }
                    if (resultObj == null)//如果街道无法满足，则根据区县匹配
                    {
                        if (query.DistrictId > 0)
                        {
                            List<long> shopBrachIds = deliveryScope.Models.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.DistrictId + CommonConst.ADDRESS_PATH_SPLIT)).Select(p => p.ShopBranchId).Distinct().ToList();
                            if (shopBrachIds.Count > 0)
                            {
                                if (shopBrachIds.Count > 1)//如果匹配多个则取距离最近一个
                                {
                                    if (fromLatLng)
                                        resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && shopBrachIds.Contains(p.Id)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();
                                }
                                else
                                {
                                    resultObj = result.Where(p => shopBrachIds.Contains(p.Id)).FirstOrDefault<ShopBranchInfo>();
                                }
                            }
                        }
                    }
                }
            }
            return resultObj;
        }

        #region 组合Sql
        private string GetSearchWhere(ShopBranchQuery query, DynamicParameters parms)
        {
            StringBuilder where = new StringBuilder();
            where.Append(" where Longitude>0 and Latitude>0 ");//周边门店只取定位了经纬度的数据

            if (query.ShopId > 0)//取商家下的门店数据
            {
                where.Append(" and ShopId=@ShopId ");
                parms.Add("@ShopId", query.ShopId);
            }
            if (query.CityId > 0)//同城门店
            {
                where.Append(" and AddressPath like concat('%',@AddressPath,'%') ");
                parms.Add("@AddressPath", CommonConst.ADDRESS_PATH_SPLIT + query.CityId + CommonConst.ADDRESS_PATH_SPLIT);
            }
            if (query.Status.HasValue)
            {
                where.Append(" and Status=@Status ");
                parms.Add("@Status", query.Status.Value);
            }

            return where.ToString();
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchOrder(ShopBranchQuery query)
        {
            string order = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    order = " ORDER BY Distance ";
                    break;

                default:
                    order = " ORDER BY Id ";
                    break;
            }
            if (!query.OrderType)
                order += " DESC ";
            else
                order += " ASC ";

            return order;
        }
        private string GetSearchPage(ShopBranchQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNo - 1) * query.PageSize, query.PageSize);
        }
        #endregion


        /// <summary>
        /// 获取代理商品的门店编号集
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
       public List<long> GetAgentShopBranchIds(long productId)
        {
            List<long> result = new List<long>();
            result = Context.ShopBranchSkusInfo.Where(d => d.ProductId == productId).Select(d => d.ShopBranchId).Distinct().ToList();
            return result;
        }
    }
}
