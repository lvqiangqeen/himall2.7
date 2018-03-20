using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Himall.API.Model;
using Himall.API.Helper;
using Himall.API.Model.ParamsModel;
using Himall.IServices.QueryModel;
using Himall.Application;
using Himall.Model;
using Himall.Core;
using Himall.API;

namespace Himall.API
{
    public class ShopProductController : BaseShopApiController
    {
  
        private IEnumerable<long> ConvertToIEnumerable(string str, char sp = ',')
        {
            var ids = str.Split(sp).Select(e =>
            {
                long id = 0;
                if (!long.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            });
            return ids;
        }
        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public object GetUnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return Json(new { success = false, msg = "参数异常" });
            var ids = ConvertToIEnumerable(pids);
            ProductManagerApplication.BatchSaleOff(ids, CurrentShop.Id);
            return Json(new { success = true, msg = "已下架" });
        }
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public object GetOnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return Json(new { success = false, msg = "参数异常" });
            var ids = ConvertToIEnumerable(pids);
            ProductManagerApplication.BatchOnSale(ids, CurrentShop.Id);
            return Json(new { success = true, msg = "已上架" });
        }
        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostSetProductStock(SetProductStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.pids))
                return Json(new { success = false, msg = "参数异常" });
            var ids = ConvertToIEnumerable(model.pids);
            var type = (CommonModel.StockOpType)model.optype;
            ProductManagerApplication.SetProductStock(ids, model.stock, type);
            return Json(new { success = true, msg = "设置成功" }); 
        }
        /// <summary>
        /// 设置SKU库存
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostSetSkuStock(SetSkuStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.skus))
                return Json(new { success = false, msg = "参数异常" });

            var ids = model.skus.Split(',').ToList();
            var stk = model.stock.Split(',').Select(e =>
            {
                int id = 0;
                if (!int.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            });
            var type = (CommonModel.StockOpType)model.optype;
            ProductManagerApplication.SetSkuStock(ids, stk, type);
			return Json(new { success = true, msg = "设置成功" });
        }
        /// <summary>
        /// 查询门店商品
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="cid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="branchProductStatus"></param>
        /// <returns></returns>
        public object GetShopProducts(
            string status,/* 销售中0, 仓库中1， 待审核2，违规下架3 */
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/
            )
        {
            CheckUserLogin();
            ProductQuery query = new ProductQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                KeyWords = keywords,
                ShopId = CurrentShop.Id
            };
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch(status)
                {
                    case "0":
                        query.SaleStatus = Himall.Model.ProductInfo.ProductSaleStatus.OnSale;
                        query.AuditStatus = new Himall.Model.ProductInfo.ProductAuditStatus[] { Himall.Model.ProductInfo.ProductAuditStatus.Audited };
                        break;
                    case "1":
                        query.SaleStatus = Himall.Model.ProductInfo.ProductSaleStatus.InStock;
                        query.AuditStatus = new Himall.Model.ProductInfo.ProductAuditStatus[] { Himall.Model.ProductInfo.ProductAuditStatus.Audited, Himall.Model.ProductInfo.ProductAuditStatus.UnAudit, Himall.Model.ProductInfo.ProductAuditStatus.WaitForAuditing };
                        break;
                    case "2":
                        query.SaleStatus = Himall.Model.ProductInfo.ProductSaleStatus.OnSale;
                        query.AuditStatus = new Himall.Model.ProductInfo.ProductAuditStatus[] { Himall.Model.ProductInfo.ProductAuditStatus.WaitForAuditing };
                        break;
                    case "3":
                        query.AuditStatus = new Himall.Model.ProductInfo.ProductAuditStatus[] { Himall.Model.ProductInfo.ProductAuditStatus.InfractionSaleOff };
                        break;
                }
            }

            if (cid > 0)
            {
                query.CategoryId = cid;
            }
            //查询商品
            var pageModel = ProductManagerApplication.GetProducts(query);
            var pids = pageModel.Models.Select(e => e.Id);
            var skus = ProductManagerApplication.GetSKU(pids);
            //查询门店SKU库存
            var product = pageModel.Models.ToList().Select(item =>
            {
                return new
                {
                    id = item.Id,
                    name = item.ProductName,
                    price = item.MinSalePrice,
                    salesCount = item.SaleCounts,
                    img = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    stock = skus.Where(e => e.ProductId == item.Id).Sum(e => e.Stock),
                    productCode=item.ProductCode
                };
            });
            var result = new
            {
                success = true,
                products = product,
                total = pageModel.Total
            };
            return Json(result);
        }
        /// <summary>
        /// 取商品SKU
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public object GetProductSku(long pid)
        {
            CheckUserLogin();
            var sku = ProductManagerApplication.GetSKU(pid);
            return Json(new { success = true, sku = sku });
        }
    }
}
