using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web.Models;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class PortfolioBuyController : BaseMobileMemberController
    {
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ICashDepositsService _iCashDepositsService;
        private IFreightTemplateService _iFreightTemplateService;
        private IRegionService _iRegionService;
        public PortfolioBuyController(IShopService iShopService, IVShopService iVShopService, IProductService iProductService,
            ICashDepositsService iCashDepositsService, IFreightTemplateService iFreightTemplateService, IRegionService iRegionService
            )
        {
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iCashDepositsService = iCashDepositsService;
            _iFreightTemplateService = iFreightTemplateService;
            _iRegionService = iRegionService;
        }

        // GET: Mobile/PortfolioBuy
        public ActionResult ProductDetail(long productId)
        {

            var collocation = ServiceHelper.Create<ICollocationService>().GetCollocationByProductId(productId);
            List<CollocationProducts> products = null;
            if (collocation != null)
            {
                products = collocation.Himall_CollocationPoruducts
                    .Where(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                    .Select(a =>
                    new CollocationProducts()
                    {
                        DisplaySequence = a.DisplaySequence.Value,
                        IsMain = a.IsMain,
                        Stock = a.Himall_Products.SKUInfo.Sum(t => t.Stock),
                        MaxCollPrice = a.Himall_CollocationSkus.Max(x => x.Price),
                        MaxSalePrice = a.Himall_CollocationSkus.Max(x => x.SkuPirce).GetValueOrDefault(),
                        MinCollPrice = a.Himall_CollocationSkus.Min(x => x.Price),
                        MinSalePrice = a.Himall_CollocationSkus.Min(x => x.SkuPirce).GetValueOrDefault(),
                        ProductName = a.Himall_Products.ProductName,
                        ProductId = a.ProductId,
                        ColloPid = a.Id,
                        Image = Core.HimallIO.GetRomoteProductSizeImage(a.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                        IsShowSku = isShowSku(a.ProductId)
                    }).OrderBy(a => a.DisplaySequence).ToList();
            }
            return View(products);

        }

        public bool isShowSku(long id)
        {
            bool result = false;
            var proser = _iProductService;
            result = proser.HasSKU(id);
            return result;
        }

        public JsonResult GetSKUInfo(long pId, long colloPid = 0)
        {
            var product = ServiceHelper.Create<IProductService>().GetProduct(pId);
            List<Himall.Model.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceHelper.Create<ICollocationService>().GetProductColloSKU(pId, colloPid);
            }
            var skuArray = new List<ProductSKUModel>();
            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {

                var price = sku.SalePrice;
                if (collProduct != null && collProduct.Count > 0)
                {
                    var collsku = collProduct.FirstOrDefault(a => a.SkuID == sku.Id);
                    if (collsku != null)
                        price = collsku.Price;
                }
                skuArray.Add(new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                });
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SKUId.Split('_');
            //    item.SKUId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            return Json(new
            {
                skuArray = skuArray
            }, JsonRequestBehavior.AllowGet);
        }


    }
}