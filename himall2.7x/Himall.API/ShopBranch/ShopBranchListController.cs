using Himall.API.Helper;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core.Helper;
using Himall.IServices.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    /// <summary>
    /// 用户APP门店部分
    /// </summary>
    public class ShopBranchListController : BaseApiController
    {
        /// <summary>
        /// 获取周边门店
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="shopId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetStoreList(
            string fromLatLng = "", /* 用户当前位置经纬度 */
            string shopId = "",  /* 商家ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/
            )
        {
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
                return Json(new { Success = false, Message = "无法获取您的当前位置，请确认是否开启定位服务！" });

            if (!string.IsNullOrWhiteSpace(shopId))//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0)
                    return Json(new { Success = false, Message = "无法定位到商家！" });
            }
            else//否则取用户同城门店
            {
                string address = "", province = "", city = "", district = "", street = "";
                ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                    return Json(new { Success = false, Message = "无法定位到城市！" });

                Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
            }
            var shopBranchs = ShopBranchApplication.GetNearShopBranchs(query);
            var storelist = shopBranchs.Models.ToList().Select(item =>
            {
                return new
                {
                    Id = item.Id,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    DistanceUnit = item.DistanceUnit,
                    ShopBranchName = item.ShopBranchName,
                    ContactPhone = item.ContactPhone,
                    AddressDetail = item.AddressDetail
                };
            });
            var result = new
            {
                Success = true,
                Storelist = storelist,
                total = shopBranchs.Total
            };
            return Json(result);
        }

        /// <summary>
        /// 门店首页
        /// </summary>
        /// <param name="shopBranchId">门店ID</param>
        /// <returns></returns>
        public object GetStoreHome(long shopBranchId)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
                return Json(new { Success = false, Message = "获取当前门店信息错误！" });

            shopBranch.ShopImages = string.IsNullOrWhiteSpace(shopBranch.ShopImages) ? "" : Core.HimallIO.GetRomoteImagePath(shopBranch.ShopImages);
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail,2);
            var shopCategory = ShopCategoryApplication.GetCategoryByParentId(0, shopBranch.ShopId).Select(item =>
            {
                return new
                {
                    Id = item.Id,
                    Name = item.Name
                };
            });

            var result = new
            {
                Success = true,
                StoreInfo = shopBranch,
                ShopCategory = shopCategory
            };
            return Json(result);
        }

        /// <summary>
        /// 门店首页获取商品列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <param name="shopCategoryId">商家一级分类</param>
        /// <param name="shopId">商家ID</param>
        /// <param name="shopBranchId">门店ID</param>
        /// <returns></returns>
        public object GetProductList(int pageSize, int pageNo, string shopCategoryId, string shopId, string shopBranchId)
        {
            ShopBranchProductQuery query = new ShopBranchProductQuery();
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            //query.ShopCategoryId = TypeHelper.ObjectToInt(shopCategoryId, 0);
            query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
            query.shopBranchId = TypeHelper.ObjectToInt(shopBranchId, 0);
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;

            if (query.ShopId <= 0)
                return Json(new { Success = false, Message = "无法定位到商家！" });

            //if (query.ShopCategoryId <= 0)
            //    return Json(new { Success = false, Message = "无法定位到商家分类！" });
            if (TypeHelper.ObjectToInt(shopCategoryId, 0) > 0)
            {
                query.ShopCategoryId = TypeHelper.ObjectToInt(shopCategoryId);
            }

            if (query.shopBranchId <= 0)
                return Json(new { Success = false, Message = "无法定位到门店！" });

            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);
            if (pageModel.Models != null && pageModel.Models.Count > 0)
            {
                #region 处理商品 官方自营店会员折扣价，各活动价等。
                var flashSalePriceList = LimitTimeApplication.GetPriceByProducrIds(pageModel.Models.Select(p => p.Id).ToList());
                var fightGroupSalePriceList = FightGroupApplication.GetActiveByProductIds(pageModel.Models.Select(p => p.Id).ToArray());
                if (CurrentUser != null)
                {
                    var shopInfo = ShopApplication.GetShop(query.ShopId.Value);
                    if (shopInfo != null && shopInfo.IsSelf)//当前商家是否是官方自营店
                    {
                        decimal discount = 1M;
                        discount = CurrentUser.MemberDiscount;
                        foreach (var item in pageModel.Models)
                        {
                            item.MinSalePrice = Math.Round(item.MinSalePrice * discount,2);
                        }
                    }
                }
                foreach (var item in pageModel.Models)
                {
                    var flashSale = flashSalePriceList.Any(p => p.ProductId == item.Id);
                    var fightGroupSale = fightGroupSalePriceList.Any(p => p.ProductId == item.Id);

                    if (flashSale && !fightGroupSale)
                    {
                        item.MinSalePrice = TypeHelper.ObjectToDecimal(flashSalePriceList.FirstOrDefault(p => p.ProductId == item.Id).MinPrice.ToString("f2"));
                    }
                    else if (!flashSale && fightGroupSale)
                    {
                        item.MinSalePrice = TypeHelper.ObjectToDecimal(fightGroupSalePriceList.FirstOrDefault(p => p.ProductId == item.Id).MiniGroupPrice.ToString("f2"));
                    }
                }
                #endregion
            }

            var productlist = pageModel.Models.ToList().Select(item =>
            {
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    MeasureUnit = item.MeasureUnit,
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.SaleCounts,//销量统计没有考虑订单支付完成。
                    RelativePath = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),//150-350
                };
            });
            var result = new
            {
                Success = true,
                ProductList = productlist,
                total = pageModel.Total
            };

            return Json(result);
        }
    }
}
