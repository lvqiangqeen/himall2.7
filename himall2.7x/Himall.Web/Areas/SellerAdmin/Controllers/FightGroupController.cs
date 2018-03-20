using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using Newtonsoft.Json;
using Himall.Core;
using Himall.IServices;

using Himall.CommonModel;
using Himall.Model;
using Himall.DTO;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Application;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    /// <summary>
    /// 拼团
    /// </summary>
    public class FightGroupController : BaseSellerController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private long CurShopId { get; set; }
        public FightGroupController(ILimitTimeBuyService iLimitTimeBuyService)
        {
            //退出登录后，直接进入controller异常处理
            if (CurrentSellerManager != null)
            {
                CurShopId = CurrentSellerManager.ShopId;
            }
            _iLimitTimeBuyService = iLimitTimeBuyService;
        }

        /// <summary>
        /// 重写验证，以过滤未购买服务或已过期情况
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //路由处理
            var route = filterContext.RouteData;
            //string controller = route.Values["controller"].ToString().ToLower();
            string action = route.Values["action"].ToString().ToLower();
            if (action.ToLower() != "nosetting")
            {
                bool iscanmarket = FightGroupApplication.IsCanUseMarketService(CurShopId);
                if (!iscanmarket && action.ToLower() != "buymarketservice")
                {
                    if (FightGroupApplication.IsOpenMarketService())
                    {
                        filterContext.Result = RedirectToAction("BuyMarketService");
                        return;
                    }
                    else
                    {
                        filterContext.Result = RedirectToAction("Nosetting");
                        return;
                    }
                }
            }
            base.OnActionExecuting(filterContext);

        }

        #region 活动列表
        /// <summary>
        /// 拼团管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="activeStatus"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostActiveList(string productName, FightGroupActiveStatus? activeStatus, int page, int rows)
        {
            List<FightGroupActiveStatus> seastatus = new List<FightGroupActiveStatus>();
            if (activeStatus.HasValue)
            {
                seastatus.Add(activeStatus.Value);
            }
            var data = FightGroupApplication.GetActives(seastatus, productName, "", CurShopId, page, rows);
            var datalist = data.Models.ToList();
            return Json(new { rows = datalist, total = data.Total });
        }

        private string GetActiveShowUrl(long id)
        {
            string result = "http://" +Request.Url.Authority+"/m-"+ PlatformType.Wap.ToString() + "/FightGroup/Detail/" + id.ToString();
            return result;
        }
        /// <summary>
        /// 显示二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowActiveQRCode(long id)
        {
            string showurl = GetActiveShowUrl(id);
            Image map;
            map = Core.Helper.QRCodeHelper.Create(showurl);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

            return File(ms.ToArray(), "image/png");
        }
        #endregion

        #region 拼团信息操作

        #region 添加拼团活动
        /// <summary>
        /// 添加拼团活动
        /// </summary>
        /// <returns></returns>
        public ActionResult AddActive()
        {
            FightGroupActiveModel model = new FightGroupActiveModel();
            model.ShopId = CurShopId;
            model.StartTime = DateTime.Now;
            model.EndTime = DateTime.Now.AddMonths(1);

            return View(model);
        }
        /// <summary>
        /// 保存拼团活动修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddActive(FightGroupActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            FightGroupActiveModel data = new FightGroupActiveModel();
            if (model.EndTime.Date < DateTime.Now.Date)
            {
                throw new HimallException("错误的结束时间");
            }
            if (model.EndTime.Date < model.StartTime.Date)
            {
                throw new HimallException("错误的结束时间");
            }
            //数据有效性验证
            model.CheckValidation();
            if (!FightGroupApplication.ProductCanJoinActive(model.ProductId.Value))
            {
                throw new HimallException("该商品已参与拼团或其他营销活动，请重新选择");
            }
            
            var skudata = FightGroupApplication.GetNewActiveItems(model.ProductId.Value).skulist;
            foreach(var item in model.ActiveItems)
            {
                var cursku = skudata.FirstOrDefault(d=>d.SkuId==item.SkuId);
                if(cursku!=null)
                {
                    if(item.ActiveStock>cursku.ProductStock)
                    {
                        throw new HimallException(item.SkuId+"错误的活动库存");
                    }
                }
                else
                {
                    model.ActiveItems.Remove(item);
                }
            }


            if (ModelState.IsValid)
            {
                UpdateModel(data);
                data.ShopId = CurShopId;
                data.IconUrl = SaveActiveIcon(data.IconUrl);

                model = data;

                FightGroupApplication.AddActive(data);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            else
            {
                result = new Result { success = false, msg = "数据异常，请检查数据有效性", status = -1 };
            }
            return Json(result);
        }
        /// <summary>
        /// 获取规格信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSkuList(long productId)
        {
            FightGroupGetSkuListModel result = new FightGroupGetSkuListModel();
            result = FightGroupApplication.GetNewActiveItems(productId);
            return Json(result);
        }
        /// <summary>
        /// 商品是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanAdd(long productId)
        {
            Result result = new Result { success=false, msg= "该商品正在参加拼团活动，无法同时参加拼团活动" };
            if(FightGroupApplication.ProductCanJoinActive(productId))
            {
                if (_iLimitTimeBuyService.IsAdd(productId))
                {
                    result.success = true;
                    result.msg = "";
                }
                else
                {
                    result = new Result { success = false, msg = "该商品正在参加限时购活动，无法同时参加拼团活动" };
                }
            }
            return Json(result);
        }
        #endregion

        #region 修改拼团活动
        /// <summary>
        /// 修改拼团活动
        /// </summary>
        /// <returns></returns>
        public ActionResult EditActive(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id);
            if (model == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if(model.StartTime<DateTime.Now || model.EndTime<DateTime.Now)
            {
                throw new HimallException("仅有未开始的活动可以编辑");
            }
            return View(model);
        }
        /// <summary>
        /// 保存拼团活动修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditActive(FightGroupActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            FightGroupActiveModel data = FightGroupApplication.GetActive(model.Id);
            if (data == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if (model.EndTime < DateTime.Now)
            {
                throw new HimallException("错误的结束时间");
            }
            if (model.EndTime < model.StartTime)
            {
                throw new HimallException("错误的结束时间");
            }

            if (ModelState.IsValid)
            {
                UpdateModel(data);

                model = data;
                //数据有效性验证
                model.CheckValidation();

                data.IconUrl = SaveActiveIcon(data.IconUrl);


                FightGroupApplication.UpdateActive(data);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            else
            {
                result = new Result { success = false, msg = "数据异常，请检查数据有效性", status = -1 };
            }
            return Json(result);
        }
        #endregion

        /// <summary>
        /// 保存活动图标
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string SaveActiveIcon(string filepath)
        {
            string result = filepath;
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/Market/", CurShopId);

                if (result.Contains("/temp/"))
                {
                    var d = result.Substring(result.LastIndexOf("/temp/"));

                    var destimg = Path.Combine(dest, Path.GetFileName(result));
                    Core.HimallIO.CopyFile(d, destimg, true);
                    result = destimg;
                }
                else if (result.Contains("/Storage/"))
                {
                    result = result.Substring(result.LastIndexOf("/Storage/"));
                }
                else
                {
                    result = "";
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 查看活动
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewActive(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id);
            if (model == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if(model.ShopId!=CurShopId)
            {
                throw new HimallException("错误的活动编号");
            }
            model.ProductDefaultImage = HimallIO.GetProductSizeImage(model.ProductImgPath, 1,ImageSize.Size_150.GetHashCode() );
            return View(model);
        }
        /// <summary>
        /// 查看拼团情况
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewGroupList(long id)
        {
            var actobj = FightGroupApplication.GetActive(id,false,false);
            if(actobj==null)
            {
                throw new HimallException("错误的活动编号");
            }
            if (actobj.ShopId != CurShopId)
            {
                throw new HimallException("错误的活动编号");
            }
            ViewBag.ActionId = id;
            return View();
        }

        [HttpPost]
        public JsonResult PostGroupList(long actionId, FightGroupBuildStatus? groupStatus, DateTime? startTime, DateTime? endTime, int page, int rows)
        {
            var actobj = FightGroupApplication.GetActive(actionId, false, false);
            if (actobj == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if (actobj.ShopId != CurShopId)
            {
                throw new HimallException("错误的活动编号");
            }
            List<FightGroupBuildStatus> seastatus = new List<FightGroupBuildStatus>();
            if (groupStatus.HasValue)
            {
                seastatus.Add(groupStatus.Value);
            }
            var data = FightGroupApplication.GetGroups(actionId, seastatus, startTime, endTime, page, rows);
            return Json(new { rows = data.Models.ToList(), total = data.Total });
        }
        /// <summary>
        /// 购买营销服务
        /// </summary>
        /// <returns></returns>
        public ActionResult BuyMarketService()
        {
            var model = FightGroupApplication.GetMarketService(CurShopId);
            return View(model);
        }
        [HttpPost]
        public JsonResult BuyMarketService(int month)
        {
            Result result = new Result();
            FightGroupApplication.BuyMarketService(month, CurrentSellerManager.ShopId);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }

        /// <summary>
        /// 营销功能未开放
        /// </summary>
        /// <returns></returns>
        public ActionResult Nosetting()
        {
            return View("Nosetting");
        }
    }
}