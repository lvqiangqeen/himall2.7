using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Areas.Web.Models;
using Himall.Model;
using Himall.Application;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ShopConcernController : BaseMemberController
    {
        private IShopService _iShopService;
        private ICustomerService _iCustomerService;
        private IProductService _iProductService;
        public ShopConcernController(IShopService iShopService, ICustomerService iCustomerService,IProductService iProductService)
        {
            _iShopService = iShopService;
            _iCustomerService = iCustomerService;
            _iProductService = iProductService;
        }
        // GET: Web/ProductConcern
        public ActionResult Index(int pageSize = 10, int pageNo = 1)
        {
            var model = _iShopService.GetUserConcernShops(CurrentUser.Id, pageNo, pageSize);
            List<ShopConcernModel> list = new List<Models.ShopConcernModel>();
            foreach (var m in model.Models.ToList())
            {
                if (null != m.Himall_Shops)
                {
                    ShopConcernModel concern = new ShopConcernModel();
                    concern.FavoriteShopInfo.Id = m.Id;
                    concern.FavoriteShopInfo.Logo = m.Himall_Shops.Logo;
                    concern.FavoriteShopInfo.ConcernTime = m.Date;
                    concern.FavoriteShopInfo.ShopId = m.ShopId;
                    concern.FavoriteShopInfo.ShopName = m.Himall_Shops.ShopName;
                    concern.FavoriteShopInfo.ConcernCount = m.Himall_Shops.Himall_FavoriteShops.Count();
                    #region 热门销售
                    var sale = _iProductService.GetHotSaleProduct(m.ShopId, 10);
                    if (sale != null)
                    {
                        foreach (var item in sale)
                        {
                            concern.HotSaleProducts.Add(new HotProductInfo
                            {
                                ImgPath = item.ImagePath,
                                Name = item.ProductName,
                                Price = item.MinSalePrice,
                                Id = item.Id,
                                SaleCount = (int)item.SaleCounts
                            });
                        }
                    }
                    #endregion

                    #region 最新上架
                    var newsale = _iProductService.GetNewSaleProduct(m.ShopId, 10);
                    if (newsale != null)
                    {
                        foreach (var item in newsale)
                        {
                            concern.NewSaleProducts.Add(new HotProductInfo
                            {
                                ImgPath = item.ImagePath,
                                Name = item.ProductName,
                                Price = item.MinSalePrice,
                                Id = item.Id,
                                SaleCount = (int)item.ConcernedCount
                            });
                        }
                    }
                    list.Add(concern);
                    #endregion
                }
            }
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            return View(list);
        }

        public JsonResult CancelConcernShops(string ids)
        {
            var strArr = ids.Split(',');
            List<long> listid = new List<long>();
            foreach (var arr in strArr)
            {
                listid.Add(Convert.ToInt64(arr));
            }
            _iShopService.CancelConcernShops(listid, CurrentUser.Id);
            return Json(new Result() { success = true, msg = "取消成功！" });
        }

        [ChildActionOnly]
        public ActionResult CustmerServices(long shopId)
        {
            var model = CustomerServiceApplication.GetAfterSaleByShopId(shopId).OrderBy(m => m.Tool).ToList();
            //List<CustomerServiceInfo> info = new List<CustomerServiceInfo>();
            //var qqGuid = Guid.NewGuid().ToString();
            //var msnGuid = Guid.NewGuid().ToString();
            //var qq = model.Where(a => a.Tool == CustomerServiceInfo.ServiceTool.QQ && a.Type == CustomerServiceInfo.ServiceType.PreSale).OrderBy(t => qqGuid).FirstOrDefault();
            //var msn = model.Where(a => a.Tool == CustomerServiceInfo.ServiceTool.Wangwang && a.Type == CustomerServiceInfo.ServiceType.PreSale).OrderBy(t => msnGuid).FirstOrDefault();
            //if (qq != null)
            //{
            //    info.Add(qq);
            //}
            //if (msn != null)
            //{
            //    info.Add(msn);
            //}
            return PartialView(model);
        }
    }
}


