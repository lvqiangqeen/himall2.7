using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.IServices;
using Himall.Web.Framework;
using Himall.API.Model;
using Himall.CommonModel;

namespace Himall.API.Helper
{
    public class BrowseHistrory
    {
        public static void AddBrowsingProduct(long productId, long userId = 0)
        {
            List<ProductBrowsedHistoryModel> productIdList = new List<ProductBrowsedHistoryModel>();
            string productIds = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_PRODUCT_BROWSING_HISTORY);
            if (!string.IsNullOrEmpty(productIds))
            {
                var arr = productIds.Split(',');
                foreach (var a in arr)
                {
                    var item = a.Split('#');
                    if (item.Length > 1)
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Parse(item[1]) });
                    }
                    else
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Now });
                    }
                }
            }
            if (productIdList.Count < 20 && !productIdList.Any(a => a.ProductId == productId))
            {
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            else if (productIdList.Count >= 20 && !productIdList.Any(a => a.ProductId == productId))
            {
                productIdList.RemoveAt(productIdList.Count - 1);
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            else
            {
                var model = productIdList.Where(a => a.ProductId == productId).FirstOrDefault();
                productIdList.Remove(model);
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            if (userId == 0)
            {
                var productsStr = "";
                foreach (var item in productIdList)
                {
                    productsStr += item.ProductId + "#" + item.BrowseTime.ToString() + ",";
                }
                Core.Helper.WebHelper.SetCookie(CookieKeysCollection.HIMALL_PRODUCT_BROWSING_HISTORY, productsStr.TrimEnd(','), DateTime.Now.AddDays(7));
            }
            else
            {
                foreach (var item in productIdList)
                {
                    try
                    {
                        ServiceHelper.Create<IProductService>().AddBrowsingProduct(new BrowsingHistoryInfo() { MemberId = userId, BrowseTime = item.BrowseTime, ProductId = item.ProductId });
                    }
                    catch
                    {
                        continue;
                    }
                }
                Core.Helper.WebHelper.DeleteCookie(CookieKeysCollection.HIMALL_PRODUCT_BROWSING_HISTORY);
            }
        }

        public static List<ProductBrowsedHistoryModel> GetBrowsingProducts(int num, long userId = 0)
        {
            List<ProductBrowsedHistoryModel> productIdList = new List<ProductBrowsedHistoryModel>();
            string productIds = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_PRODUCT_BROWSING_HISTORY);
            if (!string.IsNullOrEmpty(productIds))
            {
                var arr = productIds.Split(',');
                foreach (var a in arr)
                {
                    var item = a.Split('#');
                    if (item.Length > 1)
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Parse(item[1]) });
                    }
                    else
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Now });
                    }
                }
            }

            var ids = productIdList.Select(p => p.ProductId).ToList();
            List<FlashSalePrice> flashSaleList = ServiceHelper.Create<ILimitTimeBuyService>().GetPriceByProducrIds(ids);

            List<ProductBrowsedHistoryModel> model = new List<ProductBrowsedHistoryModel>();
            if (userId == 0)
            {
                var products = ServiceHelper.Create<IProductService>().GetProductByIds(productIdList.Select(a => a.ProductId))
                    .Where(d => d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                        && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited).ToArray()
                    .Select(a => new ProductBrowsedHistoryModel()
                    {
                        ImagePath = Core.HimallIO.GetProductSizeImage(a.RelativePath, 1, (int)ImageSize.Size_100),
                        ProductId = a.Id,
                        ProductName = a.ProductName,
                        ProductPrice = GetRealPrice(flashSaleList, a.Id, a.MinSalePrice)
                    }).ToList();

                //foreach (var product in products)
                //{
                //    var m = productIdList.Where(item => item.ProductId == product.ProductId).FirstOrDefault();
                //    m.ImagePath = product.ImagePath + "/1_150.png";
                //    m.ProductName = product.ProductName;
                //    m.ProductPrice = GetRealPrice( flashSaleList , product.ProductId , product.ProductPrice );
                //}
                return products.OrderByDescending(a => a.BrowseTime).ToList();
            }
            else
            {
                foreach (var m in productIdList)
                {
                    AddBrowsingProduct(m.ProductId, userId);
                }
                model = ServiceHelper.Create<IProductService>().GetBrowsingProducts(userId)
                    .Where(d => d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                        && d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                   .OrderByDescending(a => a.BrowseTime).Take(num).ToArray().AsEnumerable()
                   .Select(a => new ProductBrowsedHistoryModel()
                   {
                       ImagePath = Core.HimallIO.GetProductSizeImage(a.Himall_Products.RelativePath, 1, (int)ImageSize.Size_100),
                       ProductId = a.ProductId,
                       ProductName = a.Himall_Products.ProductName,
                       ProductPrice = GetRealPrice(flashSaleList, a.ProductId, a.Himall_Products.MinSalePrice),
                       BrowseTime = a.BrowseTime
                   }).ToList();
            }
            return model;
        }

        private static decimal GetRealPrice(List<FlashSalePrice> list, long productid, decimal oldPrice)
        {
            var model = list.Where(p => p.ProductId == productid).FirstOrDefault();
            if (model != null)
            {
                return model.MinPrice;
            }
            return oldPrice;
        }
    }
}
