using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Application;
using Himall.DTO;
using Himall.Core;
using Himall.CommonModel;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Model;
using System.Drawing;
using System.IO;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    [StoreAuthorization]
    public class ShopBranchController : BaseSellerController
    {

        // GET: SellerAdmin/ShopBranch
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (string.IsNullOrWhiteSpace(shopBranch.PasswordOne) || string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
                {
                    throw new HimallException("密码不能为空！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                string regionIDList = Request.Form["txtRegionScop"];
                string regionNameList = Request.Form["txtRegionScopName"];
                string[] regionIdArr = regionIDList.Split(',');
                string[] regionNameArr = regionNameList.Split(',');

                if (shopBranch.ServeRadius <= 0 && string.IsNullOrWhiteSpace(regionIDList.Trim()))
                {
                    throw new HimallException("配送半径和配送范围不能同时为空！");
                }
                shopBranch.ShopId = CurrentSellerManager.ShopId;
                shopBranch.CreateDate = DateTime.Now;
                long shopBranchId;
                ShopBranchApplication.AddShopBranch(shopBranch, out shopBranchId);
                if (shopBranchId > 0)
                {
                    #region 添加门店配送范围
                    List<DeliveryScope> deliveryScopList = new List<DeliveryScope>();
                    List<int> regionIdList = new List<int>();
                    DeliveryScope info = null;
                    for (int i = 0; i < regionIdArr.Length; i++)
                    {
                        int tempRegionId = 0;
                        if (int.TryParse(regionIdArr[i], out tempRegionId) && regionNameArr.Length >= i)
                        {
                            regionIdList.Add(tempRegionId);
                            if (!ShopBranchApplication.ExistsShopDeliveryScope(new ShopDeliveryScopeQuery() { ShopBranchId = shopBranchId, RegionId = tempRegionId }))
                            {
                                info = new DeliveryScope();
                                info.RegionId = tempRegionId;
                                info.RegionName = regionNameArr[i];
                                info.FullRegionPath = RegionApplication.GetRegionPath(tempRegionId);
                                info.FullRegionPath = CommonConst.ADDRESS_PATH_SPLIT + info.FullRegionPath + CommonConst.ADDRESS_PATH_SPLIT;//默认在结尾增加分隔符
                                info.ShopBranchId = shopBranchId;
                                deliveryScopList.Add(info);
                            }
                        }
                    }
                    if (deliveryScopList.Count > 0)
                    {
                        ShopBranchApplication.AddShopDeliveryScope(deliveryScopList);
                    }
                    if (regionIdList.Count > 0)
                    {
                        ShopBranchApplication.DeleteShopDeliveryScope(new ShopDeliveryScopeQuery() { RegionIdList = regionIdList, ShopBranchId = shopBranchId });//清除旧数据
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message});
            }
            return Json(new Result() { success = true });
        }
        public ActionResult Edit(long id)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            #region 初始化门店配送范围 
            QueryPageModel<DeliveryScope> scopeList = ShopBranchApplication.GetShopDeliveryScope(new ShopDeliveryScopeQuery() { ShopBranchId = shopBranch.Id });
            string regionIdList = "", regionNameList = "";
            foreach (DeliveryScope scopeInfo in scopeList.Models)
            {
                regionIdList += scopeInfo.RegionId + ",";
                regionNameList += scopeInfo.RegionName + ",";
            }
            ViewBag.RegionIdList = regionIdList.TrimEnd(',');
            ViewBag.RegionNameList = regionNameList.TrimEnd(',');
            #endregion
            return View(shopBranch);
        }
        [HttpPost]
        public ActionResult Edit(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                //判断是否编辑自己的门店
                shopBranch.ShopId = CurrentSellerManager.ShopId;//当前登录商家
                                                                //门店所属商家
                var oldBranch = ShopBranchApplication.GetShopBranchById(shopBranch.Id);
                if (oldBranch != null && oldBranch.ShopId != shopBranch.ShopId)
                    throw new HimallException("不能修改其他商家的门店！");

                string regionIDList = Request.Form["txtRegionScop"];
                string regionNameList = Request.Form["txtRegionScopName"];
                string[] regionIdArr = regionIDList.Split(',');
                string[] regionNameArr = regionNameList.Split(',');
                if (shopBranch.ServeRadius <= 0 && string.IsNullOrWhiteSpace(regionIDList.Trim()))
                {
                    throw new HimallException("配送半径和配送范围不能同时为空！");
                }

                ShopBranchApplication.UpdateShopBranch(shopBranch);

                #region 门店配送范围
                List<DeliveryScope> deliveryScopList = new List<DeliveryScope>();
                List<int> regionIdList = new List<int>();
                DeliveryScope info = null;
                for (int i = 0; i < regionIdArr.Length; i++)
                {
                    int tempRegionId = 0;
                    if (int.TryParse(regionIdArr[i], out tempRegionId) && regionNameArr.Length >= i)
                    {
                        regionIdList.Add(tempRegionId);
                        if (!ShopBranchApplication.ExistsShopDeliveryScope(new ShopDeliveryScopeQuery() { ShopBranchId = shopBranch.Id, RegionId = tempRegionId }))
                        {
                            info = new DeliveryScope();
                            info.RegionId = tempRegionId;
                            info.RegionName = regionNameArr[i];
                            info.FullRegionPath = RegionApplication.GetRegionPath(tempRegionId);
                            info.FullRegionPath = CommonConst.ADDRESS_PATH_SPLIT + info.FullRegionPath + CommonConst.ADDRESS_PATH_SPLIT;//默认在结尾增加分隔符
                            info.ShopBranchId = shopBranch.Id;
                            deliveryScopList.Add(info);
                        }
                    }
                }
                if (deliveryScopList.Count > 0)
                {
                    ShopBranchApplication.AddShopDeliveryScope(deliveryScopList);
                }
                if (regionIdList.Count > 0)
                {
                    ShopBranchApplication.DeleteShopDeliveryScope(new ShopDeliveryScopeQuery() { RegionIdList = regionIdList, ShopBranchId = shopBranch.Id });//更新数据
                }
                else
                {
                    ShopBranchApplication.DeleteShopDeliveryScope(new ShopDeliveryScopeQuery() { RegionIdList = new List<int>(), ShopBranchId = shopBranch.Id });//更新数据
                }
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message });
            }
            return Json(new Result() { success = true });
        }
        //public JsonResult Delete(long id)
        //{
        //    //判断是否删除自己的门店
        //    var oldBranch = ShopBranchApplication.GetShopBranchById(id);
        //    if (oldBranch != null && oldBranch.ShopId != CurrentSellerManager.ShopId)
        //        throw new HimallException("不能删除其他商家的门店！");
        //    ShopBranchApplication.DeleteShopBranch(id);
        //    ShopBranchApplication.DeleteShopDeliveryScope(new ShopDeliveryScopeQuery() { RegionIdList = new List<int>(), ShopBranchId = id });//删除所属门店配送范围
        //    return Json(new { success = true, msg = "删除成功！" });
        //}
        public ActionResult Management()
        {
            return View();
        }
        public JsonResult List(ShopBranchQuery query, int rows, int page)
        {
            query.PageNo = page;
            query.PageSize = rows;
            query.ShopId = (int)CurrentSellerManager.ShopId;
            if (query.AddressId.HasValue)
                query.AddressPath = RegionApplication.GetRegionPath(query.AddressId.Value);
            var shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            var dataGrid = new DataGridModel<ShopBranch>()
            {
                rows = shopBranchs.Models,
                total = shopBranchs.Total
            };
            return Json(dataGrid);
        }

        public JsonResult Freeze(long shopBranchId)
        {
            ShopBranchApplication.Freeze(shopBranchId);
            return Json(new { success = true, msg = "冻结成功！" });
        }
        public JsonResult UnFreeze(long shopBranchId)
        {
            ShopBranchApplication.UnFreeze(shopBranchId);
            return Json(new { success = true, msg = "解冻成功！" });
        }

        /// <summary>
        /// 门店设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            var shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            if (shopInfo != null)
            {
                ViewBag.AutoAllotOrder = shopInfo.AutoAllotOrder;
            }
            return View();
        }

        [HttpPost]
        public JsonResult Setting(bool autoAllotOrder)
        {
            try
            {
                Himall.DTO.Shop info = ShopApplication.GetShop(CurrentSellerManager.ShopId);
                info.AutoAllotOrder = autoAllotOrder;
                ShopApplication.UpdateShop(info);
                Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(CurrentSellerManager.ShopId, false));
                Cache.Remove(CacheKeyCollection.CACHE_SHOP(CurrentSellerManager.ShopId, false));

                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(
                new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("{0}:订单自动分配到门店", autoAllotOrder ? "开启" : "关闭"),
                    IPAddress = Request.UserHostAddress,
                    PageUrl = "/ShopBranch/Setting",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
            });
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        /// <summary>
        /// 门店链接二维码
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult StoresLink(string vshopUrl)
        {
            string qrCodeImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(vshopUrl))
            {
                Bitmap map;
                map = Core.Helper.QRCodeHelper.Create(vshopUrl);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray()); // 将图片内存流转成base64,图片以DataURI形式显示  
                ms.Dispose();
            }
            return Json(new { successful = true, qrCodeImagePath = qrCodeImagePath }, JsonRequestBehavior.AllowGet);
        }
    }
}