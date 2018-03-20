using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.CommonModel;
using Himall.Model;
using Himall.DTO;
using AutoMapper;

namespace Himall.Application
{
    /// <summary>
    /// 统计应用服务
    /// </summary>
    public class StatisticApplication
    {
        private static IStatisticsService _StatisticsService = ObjectContainer.Current.Resolve<IStatisticsService>();

        #region 前台统计
        /// <summary>
        /// 商品访问锁
        /// </summary>
        static Dictionary<long, object> _productVisitLockerDict = new Dictionary<long, object>();
        /// <summary>
        /// 平台统计锁
        /// </summary>
        static object _platVisitLocker = new object();
        static object _productVisitLocker = new object();
        static object _shopVisitLocker = new object();
        /// <summary>
        /// 店铺统计锁
        /// </summary>
        static Dictionary<long, object> _shopVisitLockerDict = new Dictionary<long, object>();

        /// <summary>
        /// 1、商品流量统计
        /// 2、商品浏览人数统计
        /// 3、店铺浏览人数统计（浏览商品时需统计店铺，所以组合在一起）
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="shopid"></param>
        public static void StatisticVisitCount(long pid, long shopid)
        {
            StatisticApplication.StatisticShopVisitUserCount(shopid);//店铺浏览人数
            StatisticApplication.StatisticProductVisitCount(pid,shopid);//商品浏览量
            StatisticApplication.StatisticProductVisitUserCount(pid,shopid);//商品浏览人数
        }

        /// <summary>
        /// 统计平台访问人数(cookie机制)
        /// 按天统计，一天内计一次
        /// </summary>
        public static void StatisticPlatVisitUserCount()
        {
            var platVisitTimestamp = WebHelper.GetCookie(CommonConst.HIMALL_PLAT_VISIT_COUNT);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(platVisitTimestamp))
            {
                var statisticTimestamp = Decrypt(platVisitTimestamp);//解密
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常
                    return;
                }
                if (ticks >= nowTicks)
                {//有今天的统计cookie，说明已经统计
                    return;
                }
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_platVisitLocker)
                {
                    int count = 1;
                    if (Core.Cache.Exists(CommonConst.HIMALL_PLAT_VISIT_COUNT))
                    {
                        count = (int)Core.Cache.Get(CommonConst.HIMALL_PLAT_VISIT_COUNT);
                        count++;
                    }
                    if (count >= CommonConst.HIMALL_PLAT_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddPlatVisitUser(nowDate, count);
                        count = 0;
                    }
                    Core.Cache.Insert(CommonConst.HIMALL_PLAT_VISIT_COUNT, count);
                }
            });
            
            //加密
            platVisitTimestamp = Encrypt(nowTicks.ToString());
            WebHelper.SetCookie(CommonConst.HIMALL_PLAT_VISIT_COUNT, platVisitTimestamp);
        }
        /// <summary>
        /// 统计店铺访问人数(cookie机制)
        /// 按天统计，一天内计一次
        /// </summary>
        /// <param name="shopId"></param>
        public static void StatisticShopVisitUserCount(long shopId)
        {
            var cookieKey = CommonConst.HIMALL_SHOP_VISIT_COUNT(shopId.ToString());
            var shopVisit = WebHelper.GetCookie(cookieKey);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(shopVisit))
            {
                var statisticTimestamp = Decrypt(shopVisit);//解密
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常
                    return;
                }
                if (ticks >= nowTicks)
                {//有今天的统计cookie，说明已经统计
                    return;
                }
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock(_shopVisitLocker)
                {
                    if (!_shopVisitLockerDict.Keys.Contains(shopId))
                    {
                        _shopVisitLockerDict.Add(shopId, new object());
                    }
                }
                lock (_shopVisitLockerDict[shopId])
                {
                    string key = CommonConst.HIMALL_SHOP_VISIT_COUNT(shopId.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = (int)Core.Cache.Get(key);
                        count++;
                    }
                    if (count >= CommonConst.HIMALL_SHOP_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddShopVisitUser(nowDate, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }
            });
            
            //加密
            shopVisit = Encrypt(nowTicks.ToString());
            WebHelper.SetCookie(cookieKey, shopVisit);
        }
        /// <summary>
        /// 统计商品访问人数（cookie机制）
        /// </summary>
        /// <param name="pid"></param>
        public static void StatisticProductVisitUserCount(long pid,long shopId)
        {
            var cookieKey = CommonConst.HIMALL_PRODUCT_VISIT_COUNT_COOKIE;
            var cookieValue = WebHelper.GetCookie(cookieKey);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(cookieValue))
            {
                cookieValue = Decrypt(cookieValue);//解密 格式："时间|商品ID,商品ID,"
                string[] strArray = cookieValue.Split('|');
                if (strArray.Length < 2)//格式异常
                    return;
                var statisticTimestamp = strArray[0];//时间戳
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常，为非法数据
                    return;
                }
                if (ticks > nowTicks)
                {//时间戳比当前的大，为非法数据
                    return;
                }
                if (ticks == nowTicks)
                {//时间戳为当天，看是否浏览过些商品
                    if (strArray[1].Contains(pid.ToString() + ","))
                    {//当天已浏览过，不统计
                        return;
                    }
                    cookieValue = cookieValue + pid.ToString() + ",";
                }
                else{//没有当天cookie，直接组装数据
                    cookieValue = nowTicks.ToString() + "|" + pid.ToString() + ",";
                }
            }
            else
            {
                cookieValue = nowTicks.ToString() + "|" + pid.ToString() + ",";
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_productVisitLocker)
                {
                    if (!_productVisitLockerDict.Keys.Contains(pid))
                    {
                        _productVisitLockerDict.Add(pid, new object());
                    }
                }
                lock (_productVisitLockerDict[pid])
                {
                    string key = CommonConst.HIMALL_PRODUCT_VISITUSER_COUNT(pid.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = (int)Core.Cache.Get(key);
                        count++;
                    }
                    if (count >= CommonConst.HIMALL_PRODUCT_VISITUSER_COUNT_MAX)
                    {
                        _StatisticsService.AddProductVisitUser(nowDate, pid, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }
            });

            //加密
            cookieValue = Encrypt(cookieValue);
            WebHelper.SetCookie(cookieKey, cookieValue);
        }
        /// <summary>
        /// 统计商品访问量
        /// </summary>
        /// <param name="pid"></param>
        public static void StatisticProductVisitCount(long pid,long shopId)
        {
            Task.Factory.StartNew(() => {
                lock (_productVisitLocker)
                {
                    if (!_productVisitLockerDict.Keys.Contains(pid))
                    {
                        _productVisitLockerDict.Add(pid, new object());
                    }
                }
                lock (_productVisitLockerDict[pid])
                {
                    string key = CommonConst.HIMALL_PRODUCT_VISIT_COUNT(pid.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = (int)Core.Cache.Get(key);
                        count++;
                    }
                    if (count >= CommonConst.HIMALL_PRODUCT_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddProductVisit(DateTime.Now.Date, pid, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }

            });
        }
        #endregion 前台统计

        #region 辅助方法
        public static string Encrypt(string value, string key = "KFIOS")
        {
            string text = string.Empty;
            try
            {
                string plainText = value;
                text = Core.Helper.SecureHelper.AESEncrypt(plainText, key);
                text = Core.Helper.SecureHelper.EncodeBase64(text);
                return text;
            }
            catch (Exception ex)
            {
                Core.Log.Error("StatisticApplication加密异常：", ex);
                throw;
            }
        }
        public static string Decrypt(string value, string key = "KFIOS")
        {
            string plainText = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = System.Web.HttpUtility.UrlDecode(value);
                    value = Core.Helper.SecureHelper.DecodeBase64(value);
                    plainText = Core.Helper.SecureHelper.AESDecrypt(value, key);//解密
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("StatisticApplication解密异常：", ex);
            }
            return plainText;
        }
        static string ConvertToStr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
        static DateTime ConvertToDt(string str)
        {
            return DateTime.Parse(str);
        }
        #endregion 辅助方法

        #region 商品、交易统计图表、列表方法
        /// <summary>
        /// 取商品统计
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductStatisticModel> GetProductSales(ProductStatisticQuery query)
        {
            var productVisits = _StatisticsService.GetProductVisits(query);
            //补充商品名称
            var pids = productVisits.Models.Select(e => e.ProductId);
            var products = ProductManagerApplication.GetAllStatusProductByIds(pids);
            foreach(var item in productVisits.Models)
            {
                var product = products.FirstOrDefault(e => e.Id == item.ProductId);
                if (product != null)
                    item.ProductName = product.ProductName;
            }
            return productVisits;
        }
        /// <summary>
        /// 取商品销售分类统计
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static IEnumerable<ProductCategoryStatisticModel> GetProductCategorySales(DateTime startDate,DateTime endDate,long shopId=0)
        {
            ProductStatisticQuery query = new ProductStatisticQuery()
            {
                StartDate = startDate,
                EndDate = endDate
            };
            if (shopId > 0)
            {//如果不等于0，增加店铺ID条件
                query.ShopId = shopId;
            }
            //销售记录
            var productSales = _StatisticsService.GetProductAllVisits(query);
            var pids = productSales.Select(e => e.ProductId).Distinct();
            //商品信息，分类
            var products = ProductManagerApplication.GetAllStatusProductByIds(pids);
            var categorys = CategoryApplication.GetMainCategory();
            //补充分类
            var productCateInfo= productSales.Select(e =>
            {
                var mainCategoryId = products.FirstOrDefault(item => item.Id == e.ProductId).CategoryTopId;
                var mainCategory = categorys.FirstOrDefault(c => c.Id == mainCategoryId);
                return new
                {
                    CategoryId = mainCategoryId,
                    CategoryName=mainCategory==null?string.Empty:mainCategory.Name,
                    ProductId = e.ProductId,
                    ShopId = e.ShopId,
                    SaleAmounts = e.SaleAmounts,
                    SaleCounts = e.SaleCounts
                };
            });
            var totalAmount = productCateInfo.Sum(e => e.SaleAmounts);
            var totalCount = productCateInfo.Sum(e => e.SaleCounts);
            //根据分类统计
            var productCateStat = productCateInfo.GroupBy(e => e.CategoryId).Select(e => new ProductCategoryStatisticModel
            {
                CategoryId = e.Key,
                CategoryName = e.FirstOrDefault().CategoryName,
                SaleAmounts = e.Sum(item => item.SaleAmounts),
                SaleCounts = e.Sum(item => item.SaleCounts),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                StartDate = startDate.ToString("yyyy-MM-dd"),
                AmountRate = 0.0M,
                CountRate = 0.0M
            });
            //计算比率
            totalAmount = totalAmount == 0 ? 1 : totalAmount;
            totalCount = totalCount == 0 ? 1 : totalCount;
            foreach(var cate in productCateStat)
            {
                cate.AmountRate = (decimal)cate.SaleAmounts / totalAmount;
                cate.CountRate = (decimal)cate.SaleCounts / totalCount;
            }
            return productCateStat;
            
        }
        /// <summary>
        /// 平台交易统计
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static TradeStatisticModel GetPlatTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platVisit = _StatisticsService.GetPlatVisits(startDate, endDate);
            Mapper.CreateMap<List<PlatVisitsInfo>, List<TradeStatisticModel>>();
            Mapper.CreateMap<PlatVisitsInfo, TradeStatisticModel>().ForMember(dest => dest.VistiCounts, op => op.MapFrom(src => src.VisitCounts));
            var model = platVisit.Select(e => new TradeStatisticModel
            {
                VistiCounts = e.VisitCounts,
                Date = e.Date,
                OrderAmount = e.OrderAmount,
                OrderCount = e.OrderCount,
                OrderPayCount = e.OrderPayCount,
                OrderPayUserCount = e.OrderPayUserCount,
                OrderProductCount = e.OrderProductCount,
                OrderUserCount = e.OrderUserCount,
                SaleAmounts = e.SaleAmounts,
                SaleCounts = e.SaleCounts,
                StatisticFlag = e.StatisticFlag
            });
            var tradeModel = new TradeStatisticModel
            {
                OrderUserCount = platVisit.Sum(e => e.OrderUserCount),
                OrderCount = platVisit.Sum(e => e.OrderCount),
                OrderProductCount = platVisit.Sum(e => e.OrderProductCount),
                OrderAmount = platVisit.Sum(e => e.OrderAmount),
                OrderPayUserCount = platVisit.Sum(e => e.OrderPayUserCount),
                OrderPayCount = platVisit.Sum(e => e.OrderPayCount),
                SaleCounts = platVisit.Sum(e => e.SaleCounts),
                SaleAmounts = platVisit.Sum(e => e.SaleAmounts),
                VistiCounts = platVisit.Sum(e => e.VisitCounts)
            };
            tradeModel.ChartModelPayAmounts = GetChartDataModel<decimal>(startDate, endDate, model, item => item.SaleAmounts);
            tradeModel.ChartModelPayPieces = GetChartDataModel<long>(startDate, endDate, model, item => item.SaleCounts);
            tradeModel.ChartModelPayUsers = GetChartDataModel<long>( startDate, endDate, model, item => item.OrderPayUserCount);
            tradeModel.ChartModelOrderConversionsRates = GetChartDataModel<decimal>(startDate, endDate, model,
                item =>{
                    if (item.VistiCounts > 0)
                    { return Math.Round(Convert.ToDecimal(item.OrderUserCount) / item.VistiCounts * 100, 2); }
                    else
                    {
                        return 0;
                    }
                });
            tradeModel.ChartModelPayConversionsRates = GetChartDataModel<decimal>( startDate, endDate, model,
                item =>
                {
                    if (item.OrderUserCount > 0)
                    {
                        return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.OrderUserCount*100,2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                );
            tradeModel.ChartModelTransactionConversionRate = GetChartDataModel<decimal>(startDate, endDate, model,
                item =>
                {
                    if (item.VistiCounts > 0)
                    { return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.VistiCounts*100,2) ; }
                    else
                    {
                        return 0;
                    }
                }
                );
            return tradeModel;
        }
        /// <summary>
        /// 平台交易统计(按天)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List< TradeStatisticModel> GetPlatTradeStatisticOnDay(DateTime startDate, DateTime endDate)
        {
            var platVisit = _StatisticsService.GetPlatVisits(startDate, endDate);
            var model = platVisit.Select(e => new TradeStatisticModel
            {
                VistiCounts = e.VisitCounts,
                Date = e.Date,
                OrderAmount = e.OrderAmount,
                OrderCount = e.OrderCount,
                OrderPayCount = e.OrderPayCount,
                OrderPayUserCount = e.OrderPayUserCount,
                OrderProductCount = e.OrderProductCount,
                OrderUserCount = e.OrderUserCount,
                SaleAmounts = e.SaleAmounts,
                SaleCounts = e.SaleCounts,
                StatisticFlag = e.StatisticFlag
            }).ToList();
            return model;
        }
        /// <summary>
        /// 图表对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="dataSource"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        static LineChartDataModel<T> GetChartDataModel<T>(DateTime dt1, DateTime dt2, IEnumerable<TradeStatisticModel> dataSource, Func<TradeStatisticModel, T> selector) where T : struct
        {
            LineChartDataModel<T> chartData = new LineChartDataModel<T>();
            chartData.XAxisData = GetDateXAxisData(dt1, dt2);//X轴数据（日期）
            chartData.SeriesData = GetSeriesData<T>(dt1, dt2, dataSource, selector);
            return chartData;
        }
        /// <summary>
        /// X轴
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        static string[] GetDateXAxisData(DateTime dt1,DateTime dt2)
        {
            List<string> listXAxis = new List<string>();
            while (dt1 <= dt2)
            {
                listXAxis.Add(dt1.ToString("yyyy-MM-dd"));
                dt1 = dt1.AddDays(1);
            }
            return listXAxis.ToArray();
        }
        /// <summary>
        /// Y轴
        /// </summary>
        /// <typeparam name="T">返回的值类型</typeparam>
        /// <param name="name"></param>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="dataSource"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        static IList<ChartSeries<T>> GetSeriesData<T>(DateTime dt1, DateTime dt2, IEnumerable<TradeStatisticModel> dataSource, Func<TradeStatisticModel, T> selector)
            where T : struct
        {
            int day = (dt2 - dt1).Days + 1;
            int count = 0;
            var seriesData = new ChartSeries<T>()
            {
                Data = new T[day]
            };
            while (dt1 <= dt2)
            {
                var selectObj = dataSource.Where(e => e.Date.Date == dt1.Date).Select(selector);
                if (selectObj != null)
                    seriesData.Data[count] = selectObj.FirstOrDefault();
                else
                    seriesData.Data[count] = default(T);
                dt1 = dt1.AddDays(1);
                count++;
            }
            return new List<ChartSeries<T>>() { seriesData };
        }
        /// <summary>
        /// 店铺交易统计
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static TradeStatisticModel GetShopTradeStatistic(DateTime startDate, DateTime endDate,long shopId)
        {
            var shopVisit = _StatisticsService.GetShopVisits(startDate, endDate, shopId).ToList();
            Mapper.CreateMap<ShopVistiInfo, TradeStatisticModel>();
            Mapper.CreateMap<List<ShopVistiInfo>, List<TradeStatisticModel>>();

            var model = shopVisit.Select(e => new TradeStatisticModel
            {
                VistiCounts = e.VistiCounts,
                Date = e.Date,
                OrderAmount = e.OrderAmount,
                OrderCount = e.OrderCount,
                OrderPayCount = e.OrderPayCount,
                OrderPayUserCount = e.OrderPayUserCount,
                OrderProductCount = e.OrderProductCount,
                OrderUserCount = e.OrderUserCount,
                SaleAmounts = e.SaleAmounts,
                SaleCounts = e.SaleCounts,
                StatisticFlag = e.StatisticFlag
            });
            var tradeModel = new TradeStatisticModel
            {
                OrderUserCount = shopVisit.Sum(e => e.OrderUserCount),
                OrderCount = shopVisit.Sum(e => e.OrderCount),
                OrderProductCount = shopVisit.Sum(e => e.OrderProductCount),
                OrderAmount = shopVisit.Sum(e => e.OrderAmount),
                OrderPayUserCount = shopVisit.Sum(e => e.OrderPayUserCount),
                OrderPayCount = shopVisit.Sum(e => e.OrderPayCount),
                SaleCounts = shopVisit.Sum(e => e.SaleCounts),
                SaleAmounts = shopVisit.Sum(e => e.SaleAmounts),
                VistiCounts = shopVisit.Sum(e => e.VistiCounts)
            };
            tradeModel.ChartModelPayAmounts = GetChartDataModel<decimal>(startDate, endDate, model, item => item.SaleAmounts);
            tradeModel.ChartModelPayPieces = GetChartDataModel<long>( startDate, endDate, model, item => item.SaleCounts);
            tradeModel.ChartModelPayUsers = GetChartDataModel<long>(startDate, endDate, model, item => item.OrderPayUserCount);
            tradeModel.ChartModelOrderConversionsRates = GetChartDataModel<decimal>(startDate, endDate, model,
                item =>
                {
                    if (item.VistiCounts > 0)
                    { return Math.Round(Convert.ToDecimal(item.OrderUserCount) / item.VistiCounts*100,2); }
                    else
                    {
                        return 0;
                    }
                });
            tradeModel.ChartModelPayConversionsRates = GetChartDataModel<decimal>(startDate, endDate, model,
                item =>
                {
                    if (item.OrderUserCount > 0)
                    {
                        return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.OrderUserCount*100,2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                );
            tradeModel.ChartModelTransactionConversionRate = GetChartDataModel<decimal>(startDate, endDate, model,
                item =>
                {
                    if (item.VistiCounts > 0)
                    { return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.VistiCounts*100,2) ; }
                    else
                    {
                        return 0;
                    }
                }
                );
            return tradeModel;
        }
        /// <summary>
        /// 取店铺交易统计(按天)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<TradeStatisticModel> GetShopTradeStatisticOnDay(DateTime startDate, DateTime endDate, long shopId)
        {
            var shopVisit = _StatisticsService.GetShopVisits(startDate, endDate, shopId);
            var model = shopVisit.Select(e => new TradeStatisticModel
            {
                VistiCounts = e.VistiCounts,
                Date = e.Date,
                OrderAmount = e.OrderAmount,
                OrderCount = e.OrderCount,
                OrderPayCount = e.OrderPayCount,
                OrderPayUserCount = e.OrderPayUserCount,
                OrderProductCount = e.OrderProductCount,
                OrderUserCount = e.OrderUserCount,
                SaleAmounts = e.SaleAmounts,
                SaleCounts = e.SaleCounts,
                StatisticFlag = e.StatisticFlag
            }).ToList();
            return model;
        }
        #endregion 后台图表、列表方法

        /// <summary>
        /// 订单区域分析图
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MapChartDataModel  GetAreaOrderChart(Model.OrderDimension dimension, int year, int month)
        {
            var model = _StatisticsService.GetAreaOrderChart(dimension, year, month);
            foreach (var d in model.Series.Data)
            {
                d.name = RegionApplication.GetRegion(long.Parse(d.name)).ShortName;
            }
            return model;

        }
    }
}
