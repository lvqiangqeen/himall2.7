using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Himall.Web.App_Code.Common;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class DistributionRulesController : BaseSellerController
    {
        private IDistributionService _iDistributionService;
        /// <summary>
        /// 当前shopid
        /// </summary>
        private long curshopid = 0;
        private ShopDistributorSettingInfo distributorConfig;
        /// <summary>
        /// 参数是否已配置
        /// <para>佣金比错误时不可以新增分销商品</para>
        /// </summary>
        private bool isConfigRight = false;
        public DistributionRulesController(IDistributionService iDistributionService)
        {
            _iDistributionService = iDistributionService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            bool isclose = true;
            var dissetting = _iDistributionService.GetDistributionSetting();
            if (dissetting != null)
            {
                if (dissetting.Enable)
                {
                    isclose = false;
                }
            }
            if (isclose && RouteData.Values["action"].ToString().ToLower() != "nosetting")
            {
                Response.Clear();
                Response.BufferOutput = true;
                Response.Redirect(Url.Action("NoSetting"));
            }

            curshopid = CurrentSellerManager.ShopId;
            distributorConfig = _iDistributionService.getShopDistributorSettingInfo(curshopid);
            if (distributorConfig.DistributorDefaultRate > 0)
            {
                isConfigRight = true;
            }
        }

        /// <summary>
        /// 分销商品佣金管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            if (!isConfigRight)
            {
                return RedirectToAction("DefaultBrokerage");
            }

            #region 二维码
            string sharePath = Url.Action("ShopDetail", "DistributionMarket", new { Area = "Mobile", id = curshopid });
            string fullPath = ""; string strUrl = "";
            WeiXinHelper.CreateQCode(sharePath, out fullPath, out strUrl);

            ViewBag.ShopQUrl = fullPath;
            ViewBag.ShopQCode = strUrl;

            #endregion

            return View();
        }
        /// <summary>
        /// 获取分销商品列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductList(string skey, int rows, int page, string categoryPatha, string categoryPathb, string categoryPathc, long categoryId = 0)
        {
            //查询条件
            ProductBrokerageQuery query = new ProductBrokerageQuery();
            query.skey = skey;
            query.PageSize = rows;
            query.PageNo = page;
            query.ProductBrokerageState = ProductBrokerageInfo.ProductBrokerageStatus.Normal;
            query.ShopId = curshopid;
            if (categoryId != 0)
            {
                query.CategoryId = categoryId;
            }
            query.CategoryPathA = categoryPatha;
            query.CategoryPathB = categoryPathb;
            query.CategoryPathC = categoryPathc;


            ObsoletePageModel<ProductBrokerageInfo> datasql = _iDistributionService.GetDistributionProducts(query);

            List<DistributionProductListModel> datalist = new List<DistributionProductListModel>();
            if (datasql.Models != null)
            {
                datalist = datasql.Models.Select(d => new DistributionProductListModel
                {
                    BrokerageId = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product.ProductName,
                    //Image = d.Product.GetImage(ProductInfo.ImageSize.Size_50),
                    CategoryId = d.Product.CategoryId,
                    CategoryName = d.CategoryName,
                    DistributorRate = d.rate,
                    ProductBrokerageState = d.Status,
                    ProductSaleState = d.Product.SaleStatus,
                    SellPrice = d.Product.MinSalePrice,
                    ShowProductBrokerageState = d.Status.ToDescription(),
                    ShowProductSaleState = d.Product.ShowProductState
                }).ToList();
            }
            var result = new { rows = datalist, total = datasql.Total };
            return Json(result);
        }
        /// <summary>
        /// 获取分销的商品编号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAllProductIds()
        {
            //string result = "";
            List<long> proids = new List<long>();
            proids = _iDistributionService.GetAllDistributionProductIds(curshopid);
            //if (proids.Count > 0)
            //{
            //    foreach (var item in proids)
            //    {
            //        if (!string.IsNullOrWhiteSpace(result))
            //        {
            //            result += ",";
            //        }
            //        result += item.ToString();
            //    }
            //}
            return Json(proids);
        }
        /// <summary>
        /// 增加分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddProducts(string ids)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            bool isDataOk = true;
            List<long> productids = new List<long>();
            if (!isConfigRight)
            {
                isDataOk = false;
                result = new Result { success = false, msg = "错误的默认佣金比，请设置默认佣金比" };
            }
            if (isDataOk)
            {
                if (!Regex.IsMatch(ids, @"^(\d|,)+?"))
                {
                    isDataOk = false;
                    result = new Result { success = false, msg = "错误的商品编号列表" };
                }
            }
            //组装id数据
            if (isDataOk)
            {
                string[] _arrid = ids.Split(',');
                foreach (string item in _arrid)
                {
                    long _proid = 0;
                    if (long.TryParse(item, out _proid))
                    {
                        if (_proid > 0)
                        {
                            productids.Add(_proid);
                        }
                    }
                }
                if (productids.Count < 1)
                {
                    isDataOk = false;
                    result = new Result { success = false, msg = "错误的商品编号列表" };
                }
                else
                {
                    _iDistributionService.BatAddDistributionProducts(productids, curshopid, distributorConfig.DistributorDefaultRate);
                    result = new Result { success = true, msg = "添加分销商品成功" };
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 取消分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CancelProduct(long id)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
            {
                ids.Add(id);
                _iDistributionService.CancelDistributionProduct(ids, curshopid);
                result = new Result { success = true, msg = "取消推广成功" };
            }
            else
            {
                result = new Result { success = false, msg = "错误的商品编号" };
            }
            return Json(result);
        }/// <summary>
        /// 取消分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateRate(long id, decimal rate)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
            {
                ids.Add(id);
                bool isdataok = true;
                if (rate < 0.1m || rate > 90)
                {
                    result.success = false;
                    result.msg = "比例需在 0.1% ~ 90% 之间";
                    isdataok = false;
                }
                if (isdataok)
                {
                    string _str = rate.ToString();
                    //验证格式
                    if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                    {
                        result.success = false;
                        result.msg = "错误的数据格式，只可以保留一位小数";
                        isdataok = false;
                    }
                }
                if (isdataok)
                {
                    _iDistributionService.SetProductBrokerage(rate, ids, curshopid);
                    result.success = true;
                    result.msg = "修改佣金比例成功";
                }
            }
            else
            {
                result = new Result { success = false, msg = "错误的商品编号" };
            }
            return Json(result);
        }

        #region 默认佣金设置
        /// <summary>
        /// 默认佣金设置
        /// </summary>
        /// <returns></returns>
        public ActionResult DefaultBrokerage()
        {
            return View(distributorConfig.DistributorDefaultRate);
        }
        [HttpPost]
        public JsonResult DefaultBrokerage(decimal defaultrate)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            bool isdataok = true;
            if (defaultrate < 0.1m || defaultrate > 90)
            {
                result.success = false;
                result.msg = "比例需在 0.1% ~ 90% 之间";
                isdataok = false;
            }
            if (isdataok)
            {
                string _str = defaultrate.ToString();
                //验证格式
                if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                {
                    result.success = false;
                    result.msg = "错误的数据格式，只可以保留一位小数";
                    isdataok = false;
                }
            }

            if (isdataok)
            {
                _iDistributionService.UpdateDefaultBrokerage(defaultrate, curshopid);
                result.success = true;
                result.msg = "设置默认佣金比例成功！";
            }
            return Json(result);
        }
        #endregion

        #region 推广页设置
        /// <summary>
        /// 推广页设置
        /// </summary>
        /// <returns></returns>
        public ActionResult AdvPageSet()
        {
            //return NoSetting();
            ShopDistributorSettingInfo config = distributorConfig;
            ShopDistributorAdvPageSetModel result = new ShopDistributorAdvPageSetModel();
            result.ShopId = config.ShopId;
            result.DistributorShareLogo = config.DistributorShareLogo;
            result.DistributorShareName = config.DistributorShareName;
            result.DistributorShareContent = config.DistributorShareContent;
            return View(result);
        }
        [HttpPost]
        public JsonResult AdvPageSet(ShopDistributorAdvPageSetModel model)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            bool isdataok = true;
            if (string.IsNullOrWhiteSpace(model.DistributorShareLogo))
            {
                result = new Result { success = false, msg = "请上传分享Logo" };
                isdataok = false;
            }
            string _sourceimg = "";
            if (isdataok)
            {
                if (ModelState.IsValid)
                {
                    #region 转移图片
                    if (model.DistributorShareLogo.Contains("/temp/"))
                    {
                        string oriUrl = model.DistributorShareLogo;
                        oriUrl = oriUrl.Substring(oriUrl.LastIndexOf("/temp"));
                        string ext = oriUrl.Substring(oriUrl.LastIndexOf('.'));
                        string _savepath = @"/Storage/Shop/" + curshopid.ToString() + "/Distribution/";
                        string _savefile = "sharelogo" + ext;
                        _savepath = _savepath + _savefile;
                        Core.HimallIO.CopyFile(oriUrl, _savepath, true);
                        model.DistributorShareLogo = _savepath;
                    }
                    else if (model.DistributorShareLogo.Contains("/Storage/"))
                    {
                        model.DistributorShareLogo = model.DistributorShareLogo.Substring(model.DistributorShareLogo.LastIndexOf("/Storage/"));
                    }
                    else
                    {
                        model.DistributorShareLogo = "";
                    }
                    #endregion

                    ShopDistributorSettingInfo config = distributorConfig;
                    config.DistributorShareLogo = model.DistributorShareLogo;
                    config.DistributorShareName = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(model.DistributorShareName);
                    config.DistributorShareContent = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(model.DistributorShareContent);
                    config.ShopId = curshopid;
                    _iDistributionService.UpdateShopDistributor(config);
                    result.success = true;
                    result.msg = "分销聚合页推广设置成功！";
                }
                else
                {
                    result = new Result { success = false, msg = "数据验证未通过，请检查输入" };
                }
            }
            return Json(result);
        }
        #endregion

        public ActionResult RateDetail()
        {
            var model = _iDistributionService.GetDistributionSetting();
            return View(model);
        }

        public ActionResult NoSetting()
        {
            return View("Nosetting");
        }
    }
}