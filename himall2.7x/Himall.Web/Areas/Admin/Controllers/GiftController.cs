using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using AutoMapper;
using Himall.Core;
using System.Drawing;
using Himall.Core.Helper;
using Himall.IServices.QueryModel;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using System.Threading.Tasks;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// 积分礼品
    /// </summary>
    
    public class GiftController : BaseAdminController
    {
        private IGiftService _iGiftService;
        private IGiftsOrderService _iGiftsOrderService;
        private IMemberGradeService _iMemberGradeService;
        private IExpressService _iExpressService;
        private ISlideAdsService _iSlideAdsService;


        public GiftController(IGiftService iGiftService, IGiftsOrderService iGiftsOrderService,
            IMemberGradeService iMemberGradeService, IExpressService iExpressService, ISlideAdsService iSlideAdsService)
        {
            _iGiftService = iGiftService;
            _iGiftsOrderService = iGiftsOrderService;
            _iExpressService = iExpressService;
            _iMemberGradeService = iMemberGradeService;
            _iSlideAdsService = iSlideAdsService;


            #region 数据关系映射
            Mapper.CreateMap<GiftInfo, GiftViewModel>();
            Mapper.CreateMap<GiftViewModel, GiftInfo>();
            #endregion
        }

        #region 礼品列表
        public ActionResult Management()
        {
            return View();
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult List(GiftInfo.GiftSalesStatus? status, string skey, int rows, int page)
        {
            GiftQuery query = new GiftQuery();
            query.skey = skey;
            query.status = status;
            query.PageSize = rows;
            query.PageNo = page;
            var datalist = _iGiftService.GetGifts(query);
            var data = datalist.Models.ToList();
            var result = new { rows = data, total = datalist.Total };
            return Json(result);
        }

        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateSequence(long id, int sequence)
        {
            _iGiftService.UpdateSequence(id, sequence);
            return Json(new { success = true });
        }

        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult ChangeStatus(long id, bool state)
        {
            _iGiftService.ChangeStatus(id, state);
            return Json(new { success = true });
        }
        #endregion

        #region 添加修改礼品信息
        public ActionResult Add()
        {
            GiftViewModel model = new GiftViewModel();
            model = new GiftViewModel();
            model.Sequence = 100; //默认排序
            model.VirtualSales = 0;
            model.NeedGrade = 0;
            model.GiftValue = 0;
            model.EndDate = DateTime.Now.AddMonths(1);

            #region 会员等级列表
            List<SelectListItem> MemGradeSelList = GetMemberGradeSelectList(model.NeedGrade);
            ViewBag.MemberGradeSelect = MemGradeSelList;
            #endregion

            return View(model);
        }
        public ActionResult Edit(long id)
        {
            GiftViewModel model = new GiftViewModel();
            GiftInfo data = new GiftInfo();
            data = _iGiftService.GetById(id);
            if (data == null)
            {
                throw new HimallException("错误的礼品编号。");
            }
            model = Mapper.Map<GiftInfo, GiftViewModel>(data);
            //补充图片数据
            if (string.IsNullOrWhiteSpace(model.ImagePath)) model.ImagePath = string.Format(@"/Storage/Gift/{0}", id);
            string path = model.ImagePath;
            string paths = model.ImagePath;
            string _imgpath = paths + "/1.png";
            if (HimallIO.ExistFile(_imgpath))
            {
                model.PicUrl1 = Core.HimallIO.GetImagePath(path + "/1.png");
            }
            _imgpath = paths + "/2.png";
            if (HimallIO.ExistFile(_imgpath))
            {
                model.PicUrl2 = Core.HimallIO.GetImagePath(path + "/2.png");
            }
            _imgpath = paths + "/3.png";
            if (HimallIO.ExistFile(_imgpath))
            {
                model.PicUrl3 = Core.HimallIO.GetImagePath(path + "/3.png");
            }
            _imgpath = paths + "/4.png";
            if (HimallIO.ExistFile(_imgpath))
            {
                model.PicUrl4 = Core.HimallIO.GetImagePath(path + "/4.png");
            }
            _imgpath = paths + "/5.png";
            if (HimallIO.ExistFile(_imgpath))
            {
                model.PicUrl5 = Core.HimallIO.GetImagePath(path + "/5.png");
            }
            #region 会员等级列表
            List<SelectListItem> MemGradeSelList = GetMemberGradeSelectList(model.NeedGrade);
            ViewBag.MemberGradeSelect = MemGradeSelList;
            #endregion

            return View(model);
        }
        [UnAuthorize]
        [ValidateInput(false)]
        [HttpPost]
        [OperationLog(Message = "操作礼品信息")]
        public JsonResult Edit(GiftViewModel model)
        {
            var result = new AjaxReturnData { success = false, msg = "未知错误" };
            if (ModelState.IsValid)
            {
                GiftViewModel postdata = new GiftViewModel();
                if (model.Id > 0)
                {
                    GiftInfo dbdata = _iGiftService.GetByIdAsNoTracking(model.Id);
                    //数据补充
                    if (dbdata == null)
                    {
                        result.success = false;
                        result.msg = "编号有误";
                        return Json(result);
                    }
                    postdata = Mapper.Map<GiftInfo, GiftViewModel>(dbdata);
                }
                else
                {
                    if (model.StockQuantity < 1)
                    {
                        result.success = false;
                        result.msg = "库存必须大于0";
                        return Json(result);
                    }
                }
                UpdateModel(postdata);
                GiftInfo data = new GiftInfo();
                data = Mapper.Map<GiftViewModel, GiftInfo>(postdata);
                if (model.Id > 0)
                {
                    _iGiftService.UpdateGift(data);
                }
                else
                {
                    data.Sequence = 100;
                    data.AddDate = DateTime.Now;
                    data.SalesStatus = GiftInfo.GiftSalesStatus.Normal;
                    _iGiftService.AddGift(data);
                }

                #region 转移图片
                int index = 1;
                List<string> piclist = new List<string>();

                piclist.Add(model.PicUrl1);
                piclist.Add(model.PicUrl2);
                piclist.Add(model.PicUrl3);
                piclist.Add(model.PicUrl4);
                piclist.Add(model.PicUrl5);

                string path = data.ImagePath;
                foreach (var item in piclist)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        string source = string.Empty;

                        if (item.IndexOf("temp/") > 0)
                        {
                            source = item.Substring(item.LastIndexOf("/temp"));
                        }
                        else if (item.Contains(data.ImagePath))
                        {
                            source = item.Substring(item.LastIndexOf(data.ImagePath));
                        }

                        try
                        {
                            string dest = string.Format("{0}/{1}.png", path, index);
                            if (source == dest)
                            {
                                index++;
                                continue;
                            }
                            if (!string.IsNullOrWhiteSpace(source))
                            {
                                Core.HimallIO.CopyFile(source, dest, true);
                            }
                            var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);

                            foreach (var imageSize in imageSizes)
                            {
                                string size = string.Format("{0}/{1}_{2}.png", path, index, imageSize);
                                Core.HimallIO.CreateThumbnail(dest, size, imageSize, imageSize);
                            }

                            //using (Image image = Image.FromFile(source))
                            //{

                            //    image.Save(dest, System.Drawing.Imaging.ImageFormat.Png);

                            //    var imageSizes = EnumHelper.ToDictionary<GiftInfo.ImageSize>().Select(t => t.Key);
                            //    foreach (var imageSize in imageSizes)
                            //    {
                            //        string size = string.Format("{0}/{1}_{2}.png", path, index, imageSize);
                            //        ImageHelper.CreateThumbnail(dest, size, imageSize, imageSize);
                            //    }

                            //}
                            index++;
                        }
                        catch (FileNotFoundException fex)
                        {
                            index++;
                            Core.Log.Error("发布礼品时候，没有找到文件", fex);
                        }
                        catch (System.Runtime.InteropServices.ExternalException eex)
                        {
                            index++;
                            Core.Log.Error("发布礼品时候，ExternalException异常", eex);

                        }
                        catch (Exception ex)
                        {
                            index++;
                            Core.Log.Error("发布礼品时候，Exception异常", ex);
                        }
                    }
                    else
                    {
                        string dest = string.Format("{0}/{1}.png", path, index);
                        if (HimallIO.ExistFile(dest))
                           HimallIO.DeleteFile(dest);

                        var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);
                        foreach (var imageSize in imageSizes)
                        {
                            string size = string.Format("{0}/{1}_{2}.png", path, index, imageSize);
                            if (HimallIO.ExistFile(size))
                                HimallIO.DeleteFile(size);
                        }
                        index++;
                    }
                }

                #endregion

                result.success = true;
                result.msg = "操作成功";
            }
            else
            {
                result.success = false;
                result.msg = "数据有误";
            }

            return Json(result);
        }
        /// <summary>
        /// 会员等级下拉菜单
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public List<SelectListItem> GetMemberGradeSelectList(int v)
        {
            List<MemberGrade> mglist = _iMemberGradeService.GetMemberGradeList().ToList();
            List<SelectListItem> MemGradeSelList = mglist.Select(d => new SelectListItem { Text = d.GradeName + " (" + d.Integral + ")", Value = d.Id.ToString() }).ToList();
            if (MemGradeSelList == null) MemGradeSelList = new List<SelectListItem>();
            MemGradeSelList.Insert(0, new SelectListItem { Text = "等级不限", Value = "0" });
            MemGradeSelList.Insert(0, new SelectListItem { Text = "选择会员等级要求", Value = "" });
            foreach (var item in MemGradeSelList)
            {
                if (item.Value == v.ToString())
                {
                    item.Selected = true;
                }
            }
            return MemGradeSelList;
        }
        #endregion

        #region 订单列表
        public ActionResult Order()
        {
            return View();
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult OrderList(string skey, GiftOrderInfo.GiftOrderStatus? status, int rows, int page)
        {
            GiftsOrderQuery query = new GiftsOrderQuery();
            query.skey = skey;
            query.status = status;
            query.PageSize = rows;
            query.PageNo = page;
            var orderdata = _iGiftsOrderService.GetOrders(query);
            List<GiftOrderInfo> orderlist = orderdata.Models.ToList();
            _iGiftsOrderService.OrderAddUserInfo(orderlist);
            var datalist = orderlist.Select(d =>
            {
                var orditem = d.Himall_GiftOrderItem.FirstOrDefault();
                return new GiftOrderPageModel
                {
                    Id = d.Id,
                    OrderStatus = d.OrderStatus,
                    UserId = d.UserId,
                    UserRemark = ClearHtmlString(d.UserRemark),
                    ShipTo = d.ShipTo,
                    CellPhone = d.CellPhone,
                    TopRegionId = d.TopRegionId,
                    RegionId = d.RegionId,
                    RegionFullName = d.RegionFullName,
                    Address = ClearHtmlString(d.Address),
                    ExpressCompanyName = d.ExpressCompanyName,
                    ShipOrderNumber = d.ShipOrderNumber,
                    ShippingDate = d.ShippingDate,
                    OrderDate = d.OrderDate,
                    FinishDate = d.FinishDate,
                    TotalIntegral = d.TotalIntegral,
                    CloseReason = ClearHtmlString(d.CloseReason),
                    FirstGiftId = orditem.GiftId,
                    FirstGiftName = ClearHtmlString(orditem.GiftName),
                    FirstGiftBuyQuantity = orditem.Quantity,
                    UserName = d.UserName
                };
            });
            var result = new { rows = datalist.ToList(), total = orderdata.Total };
            return Json(result);
        }
        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expname"></param>
        /// <param name="expnum"></param>
        /// <returns></returns>
        [HttpPost]
        [OperationLog(Message = "礼品订单发货")]
        public JsonResult SendGift(long id, string expname, string expnum)
        {
            Result result = new Result();
            _iGiftsOrderService.SendGood(id, expname, expnum);

            string host = Request.Url.Host;
            host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
            var returnurl = String.Format("http://{0}/Common/ExpressData/SaveExpressData", host);
            var key = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings().Kuaidi100Key;
            if (!string.IsNullOrEmpty(key))
            {
                Task.Factory.StartNew(() => ServiceHelper.Create<IExpressService>().SubscribeExpress100(expname, expnum, key, string.Empty, returnurl));
            }
            result.success = true;
            result.status = 1;
            result.msg = "发货成功";
            return Json(result);
        }

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <param name="expressCompanyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(expressCompanyName) || string.IsNullOrWhiteSpace(shipOrderNumber))
            {
                throw new HimallException("错误的订单信息");
            }
            string kuaidi100Code = _iExpressService.GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;

            string content = "暂时没有此快递单号的信息";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                    // 读取流字符串内容
                    content = streamReader.ReadToEnd();
                    content = content.Replace("&amp;", "");
                    content = content.Replace("&nbsp;", "");
                    content = content.Replace("&", "");
                }
            }
            catch
            {
            }
            return Json(content);
        }
        #endregion

        #region App积分商城首页配置
        public ActionResult AppManage()
        {
            Admin.Models.GiftsAppManageModel model = new GiftsAppManageModel();
            var robj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.Roulette, IntegralMallAdInfo.AdShowPlatform.APP);
            var cobj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.ScratchCard, IntegralMallAdInfo.AdShowPlatform.APP);
            if (robj != null)
            {
                model.RouletteId = robj.ActivityId;
            }
            if (cobj != null)
            {
                model.ScratchCardId = cobj.ActivityId;
            }
            return View(model);
        }
        #region 轮播图
        public JsonResult AppAddSlideImage(string id, string description, string imageUrl, string url)
        {
            Result result = new Result();
            var slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.AppGifts;
            slideAdInfo.Url = url.ToLower().Replace("/m-wap", "/m-app").Replace("/m-weixin", "/m-app");
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0;
            slideAdInfo.ImageUrl = imageUrl;

            if (slideAdInfo.Id > 0)
                _iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                _iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult AppDeleteSlideImage(string id)
        {
            Result result = new Result();
            _iSlideAdsService.DeleteSlidAd(0, Convert.ToInt64(id));
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult AppSlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iSlideAdsService.UpdateWeixinSlideSequence(0, oriRowNumber, newRowNumber, SlideAdInfo.SlideAdType.AppGifts);
            return Json(new { success = true });
        }

        public JsonResult AppGetSlideImages()
        {
            //轮播图
            var slideImageSettings = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.AppGifts).ToArray();
            var slideImageService = _iSlideAdsService;
            var slideModel = slideImageSettings.Select(item =>
            {
                var slideImage = slideImageService.GetSlidAd(0, item.Id);
                return new
                {
                    id = item.Id,
                    imgUrl = Core.HimallIO.GetImagePath(item.ImageUrl),
                    displaySequence = item.DisplaySequence,
                    url = item.Url,
                    description = item.Description
                };
            });
            return Json(new { rows = slideModel, total = slideModel.Count() }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pic"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsonResult AppUpdateImageAd(long id, string pic, string url)
        {
            var image = _iSlideAdsService.GetImageAd(0, id);
            if (!string.IsNullOrWhiteSpace(pic) && (!image.ImageUrl.Equals(pic)))
            {
                //转移图片
                if (pic.Contains("/temp/"))
                {
                    string source = pic.Substring(pic.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    pic = Path.Combine(dest, Path.GetFileName(source));
                    Core.HimallIO.CopyFile(source, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage"));
                }
            }
            var imageAd = new ImageAdInfo { ShopId = 0, Url = url, ImageUrl = pic, Id = id };
            _iSlideAdsService.UpdateImageAd(imageAd);
            return Json(new { success = true });
        }
        #endregion

        #region 大转盘
        public JsonResult GetRouletteList(string name, int page = 1, int rows = 10)
        {
            var result = WeiActivityApplication.Get(name, CommonModel.WeiActivityType.Roulette, page, rows, true, false);

            return Json(new { rows = result.Models, total = result.Total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SelectAppAds(long id, IntegralMallAdInfo.AdActivityType adtype)
        {
            Result result = new Result() { success = false, msg = "未知错误" };
            string cover = "";
            //获取广告图地址
            switch (adtype)
            {
                case IntegralMallAdInfo.AdActivityType.Roulette:
                    var tmpobj1 = WeiActivityApplication.GetActivityModel(id);
                    if (tmpobj1 == null)
                    {
                        result = new Result() { success = false, msg = "错误活动编号" };
                        return Json(result);
                    }
                    cover = tmpobj1.activityUrl;
                    break;
                case IntegralMallAdInfo.AdActivityType.ScratchCard:
                    var tmpobj2 = WeiActivityApplication.GetActivityModel(id);
                    if (tmpobj2 == null)
                    {
                        result = new Result() { success = false, msg = "错误活动编号" };
                        return Json(result);
                    }
                    cover = tmpobj2.activityUrl;
                    break;
            }
            var data = _iGiftService.UpdateAdInfo(adtype, id, cover, IntegralMallAdInfo.AdShowStatus.Show, IntegralMallAdInfo.AdShowPlatform.APP);
            long curactid = 0;
            if (data.ShowAdStatus != IntegralMallAdInfo.AdShowStatus.Show)
            {
                curactid = data.ActivityId;
            }
            result = new Result() { success = true, msg = curactid.ToString() };
            return Json(result);
        }
        #endregion

        #region 刮刮卡
        public JsonResult GetScratchCardList(string name, int page = 1, int rows = 10)
        {
            var result = WeiActivityApplication.Get(name, CommonModel.WeiActivityType.ScratchCard, page, rows, true, false);

            return Json(new { rows = result.Models, total = result.Total }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion


        /// <summary>
        /// 清理引号类字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ClearHtmlString(string str)
        {
            string result = str;
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("'", "&#39;");
                result = result.Replace("\"", "&#34;");
                result = result.Replace(">", "&gt;");
                result = result.Replace("<", "&lt;");
            }
            return result;
        }
    }

    public class AjaxReturnData
    {
        public bool success { get; set; }
        public string msg { get; set; }
    }
}