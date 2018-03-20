using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using Himall.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Himall.Core.Helper;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class TemplateVisualizationAjaxController : BaseSellerController
    {
        private IBonusService _iBonusService;
        private ITopicService _iTopicService;
        private ICouponService _iCouponService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IProductService _iProductService;
        private IVShopService _iVShopService;
        private IPhotoSpaceService _iPhotoSpaceService = null;
        public TemplateVisualizationAjaxController(
           IBonusService iBonusService,
            ITopicService iTopicService,
            ICouponService iCouponService,
            ILimitTimeBuyService iLimitTimeBuyService,
             IProductService iProductService,
            IPhotoSpaceService iPhotoSpaceService,
            IVShopService iVShopService)
        {
            _iBonusService = iBonusService;
            _iTopicService = iTopicService;
            _iCouponService = iCouponService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iProductService = iProductService;
            _iPhotoSpaceService = iPhotoSpaceService;
            _iVShopService = iVShopService;
        }




        #region Hi_Ajax_Bonus
        public ActionResult Hi_Ajax_Bonus(string title = "", int p = 1)
        {
            int pageNo = p;
            BonusAjaxModel model = new BonusAjaxModel() { list = new List<BonusContent>() };
            InitialBonusModel(model, title, pageNo);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void InitialBonusModel(BonusAjaxModel model, string name = "", int pageNo = 1)
        {
            var brandList = _iBonusService.Get(1, 1, name, pageNo, 10);
            int pageCount = TemplatePageHelper.GetPageCount(brandList.Total, 10);

            if (brandList != null)
            {
                model.status = 1;
                model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
                InitialBonusContentModel(brandList.Models, model);
            }
        }
        private void InitialBonusContentModel(IQueryable<Himall.Model.BonusInfo> bonusList, BonusAjaxModel model)
        {

            foreach (var bouns in bonusList)
            {
                model.list.Add(new BonusContent
                {
                    create_time = "",
                    item_id = bouns.Id,
                    link = "",
                    pic = bouns.ImagePath,
                    title = bouns.Name,
                    endTime = bouns.EndTime.ToShortDateString(),
                    startTime = bouns.StartTime.ToShortDateString(),
                    price = bouns.TotalPrice.ToString()
                });
            }
        }

        #endregion


        #region Hi_Ajax_Topic
        public ActionResult Hi_Ajax_Topic(string title = "", int p = 1)
        {
            int pageNo = p;
            long shopId = CurrentSellerManager.ShopId;
            TopicAjaxModel model = new TopicAjaxModel() { list = new List<TopicContent>() };
            InitialCateModel(model, title, pageNo, shopId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void InitialCateModel(TopicAjaxModel model, string name, int pageNo, long shopId)
        {
            var topicList = _iTopicService.GetTopics(new IServices.QueryModel.TopicQuery
            {
                PageNo = pageNo,
                PageSize = 10,
                ShopId = shopId,
                PlatformType = Core.PlatformType.Mobile,
                Name = name,
            });
            int pageCount = TemplatePageHelper.GetPageCount(topicList.Total, 10);

            if (topicList != null)
            {
                model.status = 1;
                model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
                InitialCateContentModel(topicList.Models, model);
            }
        }
        private void InitialCateContentModel(IQueryable<TopicInfo> topicList, TopicAjaxModel model)
        {

            foreach (var topic in topicList)
            {
                model.list.Add(new TopicContent
                {
                    create_time = "",
                    item_id = topic.Id,
                    link = "/m-Wap/topic/detail/" + topic.Id,
                    pic = "",
                    title = topic.Name,
                    tag = topic.Tags
                });
            }
        }

        #endregion


        #region Hi_Ajax_Coupons
        public ActionResult Hi_Ajax_Coupons(int p = 1, long shopId = 1, string title = "")
        {
            int pageNo = p;
            shopId = CurrentSellerManager.ShopId;
            CouponsAjaxModel model = new CouponsAjaxModel() { list = new List<CouponsContent>() };
            InitialCouponsModel(model, shopId, title, pageNo);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void InitialCouponsModel(CouponsAjaxModel model, long shopId, string name = "", int pageNo = 1)
        {
            var couponsList = _iCouponService.GetCouponList(new
            Himall.IServices.QueryModel.CouponQuery
            {
                CouponName = name,
                ShopId = shopId,
                IsOnlyShowNormal = true,
                IsShowAll = false,
                ShowPlatform = Himall.Core.PlatformType.Wap,
                PageNo = pageNo,
                PageSize = 10
            });
            int pageCount = TemplatePageHelper.GetPageCount(couponsList.Total, 10);

            if (couponsList != null)
            {
                model.status = 1;
                model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
                InitialCouponsContentModel(couponsList.Models, model);
            }
        }
        private void InitialCouponsContentModel(IQueryable<Himall.Model.CouponInfo> couponsList, CouponsAjaxModel model)
        {

            foreach (var coupon in couponsList)
            {
                model.list.Add(new CouponsContent
                {
                    create_time = coupon.CreateTime.ToString(),
                    game_id = coupon.Id,
                    link = "/m-wap/vshop/CouponInfo/" + coupon.Id,
                    type = 1,
                    title = coupon.CouponName,
                    condition = coupon.OrderAmount.ToString(),
                    endTime = coupon.EndTime.ToShortDateString(),
                    price = coupon.Price.ToString(),
                    shopName = coupon.ShopName
                });
            }
        }

        #endregion




        #region Hi_Ajax_GetGoodsList
        public ActionResult Hi_Ajax_GetGoodsList(int status = 2, string title = "", int p = 1)
        {
            int pageNo = p;
            long shopId = CurrentSellerManager.ShopId;
            ProductAjaxModel model = new ProductAjaxModel() { list = new List<ProductContent>() };
            InitialProductModel(model, status, title, pageNo, shopId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void InitialProductModel(ProductAjaxModel model, int status, string name, int pageNo, long shopId)
        {
            var productList = _iProductService.GetProducts(new IServices.QueryModel.ProductQuery
            {
                AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)status },
                KeyWords = name,
                PageNo = pageNo,
                PageSize = 10,
                Sort = "Id",
                IsAsc = true,
                ShopId = shopId
            });
            int pageCount = TemplatePageHelper.GetPageCount(productList.Total, 10);

            if (productList != null)
            {
                model.status = 1;
                model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
                InitialProductContentModel(productList.Models, model);
            }
        }
        private void InitialProductContentModel(IQueryable<ProductInfo> productList, ProductAjaxModel model)
        {

            foreach (var product in productList)
            {
                model.list.Add(new ProductContent
                {
                    create_time = "",
                    item_id = product.Id,
                    link = "/m-wap/Product/Detail/" + product.Id.ToString(),
                    pic = product.GetImage(ImageSize.Size_350),
                    title = product.ProductName,
                    price = product.MinSalePrice.ToString("F2"),
                    original_price = product.MarketPrice.ToString("F2"),
                    is_compress = "0"
                });
            }
        }

        #endregion


        #region Hi_Ajax_LimitBuy
        public ActionResult Hi_Ajax_LimitBuy(int p = 1, long shopId = 1, string title = "")
        {
            int pageNo = p;
            shopId = CurrentSellerManager.ShopId;
            LimitBuyAjaxModel model = new LimitBuyAjaxModel() { list = new List<LimitBuyContent>() };
            InitialLimitBuyModel(model, shopId, title, pageNo);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void InitialLimitBuyModel(LimitBuyAjaxModel model, long shopId, string name = "", int pageNo = 1)
        {
            var limitBuyList = _iLimitTimeBuyService.GetAll(
                new IServices.QueryModel.FlashSaleQuery
                {
                    ItemName = name,
                    ShopId = shopId,
                    PageNo = pageNo,
                    PageSize = 10,
                    AuditStatus = Himall.Model.FlashSaleInfo.FlashSaleStatus.Ongoing,
                    CheckProductStatus = true
                });
            int pageCount = TemplatePageHelper.GetPageCount(limitBuyList.Total, 10);

            if (limitBuyList != null)
            {
                model.status = 1;
                model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
                InitialLimitBuyContentModel(limitBuyList.Models, model);
            }
        }
        private void InitialLimitBuyContentModel(IQueryable<Himall.Model.FlashSaleInfo> limitBuyList, LimitBuyAjaxModel model)
        {

            foreach (var limitBuy in limitBuyList)
            {
                model.list.Add(new LimitBuyContent
                {
                    create_time = "",
                    item_id = limitBuy.Id,
                    link = "/m-wap/limittimebuy/detail/" + limitBuy.Id,
                    title = limitBuy.Himall_Products.ProductName,
                    endTime = limitBuy.EndDate.ToShortDateString(),
                    startTime = limitBuy.BeginDate.ToShortDateString(),
                    price = limitBuy.MinPrice.ToString(),
                    shopName = limitBuy.Himall_Shops.ShopName
                });
            }
        }

        #endregion


        #region Hi_Ajax_SaveTemplate
        [ValidateInput(false)]
        public ActionResult Hi_Ajax_SaveTemplate(int is_preview, string client, string content = "", int type = 2, long shopId = 0)
        {
            string dataName = client;
            string json = content;
            long shopid = CurrentSellerManager.ShopId;
            var tmpobj = _iVShopService.GetVShopByShopId(shopid);
            if (tmpobj == null)
            {
                throw new Himall.Core.HimallException("未开通微店");
            }
            long vshopid = tmpobj.Id;
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            var jpage = jo["page"];
            string title = TryGetJsonString(jpage, "title");
            string describe = TryGetJsonString(jpage, "describe");
            string tags = TryGetJsonString(jpage, "tags");
            string icon = TryGetJsonString(jpage, "icon");
            VTemplateClientTypes clientType = (VTemplateClientTypes)type;

            string pagetitle = CurrentSiteSetting.SiteName + "首页";
            if (clientType == VTemplateClientTypes.SellerWapSpecial)
            {
                int topicid = 0;
                if (!int.TryParse(client, out topicid))
                {
                    topicid = 0;
                }
                icon = SaveIcon(icon);
                if (!string.IsNullOrWhiteSpace(icon))
                {
                    //回写
                    jo["page"]["icon"] = icon;
                    json = JsonConvert.SerializeObject(jo);
                }
                client = AddOrUpdateTopic(topicid, title, tags,icon, PlatformType.Mobile).ToString();
                string basetemp = Server.MapPath(VTemplateHelper.GetTemplatePath("0", clientType, shopid));
                string _curtemp = Server.MapPath(VTemplateHelper.GetTemplatePath(client, clientType, shopid));
                if (!System.IO.Directory.Exists(_curtemp))
                {
                    VTemplateHelper.CopyFolder(basetemp, _curtemp);
                }
                pagetitle = "专题-" + title;
            }

            string templatePath = VTemplateHelper.GetTemplatePath(client, clientType, shopid);
            string datapath = templatePath + "data/default.json";
            string cshtmlpath = templatePath + "Skin-HomePage.cshtml";
            string abTemplatePath = Server.MapPath(templatePath);
            string abDataPath = Server.MapPath(datapath);
            string abCshtmlPath = Server.MapPath(cshtmlpath);


            string msg = "保存成功";
            string status = "1";
            try
            {
                StreamWriter sw = new StreamWriter(
                    abDataPath,
                    false,
                    Encoding.UTF8);
                foreach (var s in json)
                {
                    sw.Write(s);
                }
                sw.Close();
                StringBuilder htmlsb = new StringBuilder();
                if (clientType == VTemplateClientTypes.SellerWapSpecial)
                {
                    htmlsb.Append("@{ViewBag.Title = \"" + pagetitle + "\";}\n");
                }
                htmlsb.Append("@{Layout = \"/Areas/Mobile/Templates/Default/Views/Shared/_Base.cshtml\";}\n");
                htmlsb.Append("@{\n");
                htmlsb.Append("long curshopid=" + shopid.ToString() + ",curvshopid=" + vshopid.ToString() + ";\n");
                htmlsb.Append("}\n");
                htmlsb.Append("<div class=\"container\">\n");
                if (clientType != VTemplateClientTypes.SellerWapSpecial)
                {
                    htmlsb.Append("@(Html.Action(\"VShopHeader\", \"vshop\", new { id = curvshopid }))\n");
                }
                htmlsb.Append("<link rel=\"stylesheet\" href=\"/Content/PublicMob/css/style.css\" />\n");
                htmlsb.Append("<link rel=\"stylesheet\" href=\"/Areas/Admin/templates/common/style/css.css\" rev=\"stylesheet\" type=\"text/css\">\n");
                htmlsb.Append("<link rel=\"stylesheet\" href=\"/Areas/Admin/templates/common/style/mycss.css\" rev=\"stylesheet\" type=\"text/css\">\n");
                htmlsb.Append("<link rel=\"stylesheet\" href=\"/Areas/Admin/templates/common/style/main.css\" rev=\"stylesheet\" type=\"text/css\">\n");
                htmlsb.Append("<link rel=\"stylesheet\" href=\"/Areas/Admin/templates/common/style/head.css\">\n");
                htmlsb.Append("<script type=\"text/javascript\">\n");
                htmlsb.Append("var curshopid=" + shopid.ToString() + ",curvshopid=" + vshopid.ToString() + ";\n");
                htmlsb.Append("</script>\n");

                htmlsb.Append(GetPModulesHtml(jo));
                string lModuleHtml = GetLModulesHtml(jo, dataName);
                htmlsb.Append(lModuleHtml);
                if (clientType == VTemplateClientTypes.SellerWapSpecial)
                {
                    htmlsb.Append("@{Html.RenderPartial(\"~/Areas/Mobile/Templates/Default/Views/Shared/_4ButtonsFoot.cshtml\");}\n");
                    htmlsb.Append("</div>\n");
                    htmlsb.Append("<a class=\"WX-backtop\"></a>\n");
                    htmlsb.Append("<script src=\"~/Areas/Mobile/Templates/Default/Scripts/mui.min.js\"></script>\n");
                    htmlsb.Append("<script src=\"~/Areas/Mobile/Templates/Default/Scripts/AppAuto.js\"></script>\n");
                    htmlsb.Append("<script src=\"~/Areas/Mobile/Templates/Default/Scripts/home.js\"></script>\n");
                }
                else
                {
                    htmlsb.Append("@{Html.RenderPartial(\"~/Areas/Mobile/Templates/Default/Views/Shared/_4ButtonsFoot_shop.cshtml\");}\n");
                    htmlsb.Append("</div>\n");
                    htmlsb.Append("<a class=\"WX-backtop\"></a>\n");
                    htmlsb.Append("<script src=\"~/Areas/Mobile/Templates/Default/Scripts/shophome.js\"></script>\n");
                }
                htmlsb.Append(" <script src=\"~/Scripts/swipe-template.js\"></script>\n");
                if (!System.IO.Directory.Exists(abTemplatePath))
                    System.IO.Directory.CreateDirectory(abTemplatePath);
                StreamWriter swhtml = new StreamWriter(abCshtmlPath, false, Encoding.UTF8);

                string html = htmlsb.ToString();
                foreach (var s in html)
                {
                    swhtml.Write(s);
                }
                swhtml.Close();
                VTemplateHelper.ClearCache(client, VTemplateClientTypes.SellerWapIndex, shopid);
                //生成静态Html
                /* 暂时不进行全部的静态化
                 var vshop = VshopApplication.GetVShop(vshopid);
                 ShopApplication.CheckInitTemplate(vshop.ShopId);

                 this.ControllerContext.Controller.ViewBag.ShopId = vshop.ShopId;
                 this.ControllerContext.Controller.ViewBag.Title = vshop.HomePageTitle;

                 List<CustomerServiceInfo> customerServiceInfo = CustomerServiceApplication.GetMobileCustomerService(vshop.ShopId);
                 this.ControllerContext.Controller.ViewBag.HasServices = customerServiceInfo != null ? (customerServiceInfo.Count > 0 ? true : false) : false;
                 this.ControllerContext.Controller.ViewBag.CustomerServices = CustomerServiceApplication.GetMobileCustomerService(vshop.ShopId);
                 this.ControllerContext.Controller.ViewBag.ShowAside = 1;

                 this.ControllerContext.Controller.ViewBag.Keywords = "";
                 this.ControllerContext.Controller.ViewBag.AreaName = "m-wap";
                 this.ControllerContext.Controller.ViewBag.MAppType = "0";
                 this.ControllerContext.Controller.ViewBag.FootIndex = 0;
                 this.ControllerContext.Controller.ViewBag.VshopId = vshopid;
                 html = this.ControllerContext.RenderViewToString(tempPath + "/Skin-HomePage.cshtml", new object() { });
                 StreamWriter swcontent = new StreamWriter(Server.MapPath(tempPath + "/Skin-HomePage.cshtml"), false, Encoding.UTF8);
                 foreach (var s in html)
                 {
                     swcontent.Write(s);
                 }
                 swcontent.Flush();
                 swcontent.Close();
                 * */
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                status = "0";
            }
            if (is_preview == 1)
            {
                var vshopId = _iVShopService.GetVShopByShopId(CurrentSellerManager.ShopId).Id;
                return Json(new
                {
                    status = status,
                    msg = msg,
                    link = "/m-wap/vshop/detail/" + vshopId.ToString() + "?ispv=1&tn=" + dataName,
                    tname = dataName
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { status = status, msg = "", link = "/Selleradmin/VTemplate/EditTemplate?client=" + type.ToString() + "&tName=" + client, tname = client }, JsonRequestBehavior.AllowGet);


        }

        public string GetPModulesHtml(JObject jo)
        {

            string templateHtml = "";
            foreach (var module in jo["PModules"])
            {
                templateHtml += Base64Decode(module["dom_conitem"].ToString());
            }
            return templateHtml;

        }
        public string GetLModulesHtml(JObject jo, string template)
        {
            long shopid = CurrentSellerManager.ShopId;
            string templateHtml = "";
            StringBuilder tmpsb = new StringBuilder();
            int firstPageNum = 7, lRecords = 1;
            foreach (var module in jo["LModules"])
            {
                string mtype = module.TryGetJsonString("type");
                switch (mtype)
                {
                    case "5":
                        tmpsb.Append(GetGoodGroupTag(Base64Decode(module["dom_conitem"].ToString()), module));
                        break;
                    case "4":
                        tmpsb.Append(GetGoodTag(module));
                        break;
                    default:
                        //if (lRecords > firstPageNum)
                        //{
                        //    tmpsb.Append("<div class=\"scrollLoading\" data-url=\"/m-wap/VShop/GetTemplateItem/" + module.TryGetJsonString("id") + "?shopid=" + shopid.ToString() + "\"><div class=\"htmlloading\"></div></div>");
                        //}
                        //else
                        //{
                        tmpsb.Append(Base64Decode(module["dom_conitem"].ToString()));
                        //}
                        break;
                }
                lRecords++;
            }
            templateHtml = tmpsb.ToString();
            return templateHtml;
        }
        /// <summary>
        /// 商品模块
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetGoodTag(JToken data)
        {
            try
            {
                string ids = "";
                List<long> arr_id = new List<long>();
                foreach (var item in data["content"]["goodslist"])
                {
                    long _tmp;
                    if (long.TryParse(item["item_id"].ToString(), out _tmp))
                    {
                        arr_id.Add(_tmp);
                    }
                }

                bool showIco = bool.Parse(data["content"]["showIco"].ToString());
                bool showPrice = bool.Parse(data["content"]["showPrice"].ToString());
                string showName = data["content"]["showName"].ToString();

                int idlen = arr_id.Count();
                StringBuilder html = new StringBuilder(200);
                if (arr_id != null & idlen > 0)
                {
                    string gdboxid = "goods_" + Guid.NewGuid().ToString("N");
                    //首节数量
                    string layout = data["content"]["layout"].ToString();
                    //int modNum = 4;//根据不同显示样式，设置最小商品模块包括的商品数量
                    //if (layout == "1" || layout == "5")
                    //{//小图是两个一组
                    //    modNum = 8;
                    //}
                    //else if (layout == "3")
                    //{//一大两小是三个一组
                    //    modNum = 12;
                    //}
                    //加载首节
                    //List<long> _tmparr = arr_id.Take(modNum).ToList();
                    ids = string.Join(",", arr_id);
                    html.Append("@{Html.RenderAction(\"GoodsListAction\", \"TemplateVisualizationProcess\",new { Layout=\""
                        + layout + "\", ShowName=\"" + showName +
                        "\", IDs=\"" + ids +
                        "\", ShowIco=\"" + showIco +
                        "\", ShowPrice=\"" + showPrice +
                        "\", DataUrl=\"" + Request.Form["getGoodUrl"] +
                        "\", ID=\"" + gdboxid + "\"});}");

                    ////分节延迟
                    //for (var _n = modNum; _n < idlen; _n = _n + modNum)
                    //{
                    //    if (_n > idlen) break;
                    //    _tmparr = arr_id.Skip(_n).Take(modNum).ToList();
                    //    string _pids = string.Join(",", _tmparr);
                    //    string dataurl = "/m-wap/TemplateVisualizationProcess/GoodsListAction?Layout=" + layout + "&ids=" + _pids + "";
                    //    dataurl += "&ShowIco=" + showIco + "&showPrice=" + showPrice + "&showName=" + showName + "&showWarp=false";
                    //    html.Append("<div class=\"scrollLoading\" data-url=\"" + dataurl + "\" data-gdid=\"" + gdboxid + "\"><div class=\"htmlloading\"></div></div>");
                    //}
                }
                else
                {
                    html.Append(Base64Decode(data["dom_conitem"].ToString()));
                }
                return html.ToString();
            }
            catch
            {
                return "";
            }
        }

        private string CreateProductHtml(JToken data, List<long> idArray)
        {
            string layout = data["content"]["layout"].ToString();//1:小图，2：大图，3：一大两小，4：列表，5：小图有标题
            var name = "~/Views/Shared/GoodGroup" + layout + ".cshtml";
            ProductAjaxModel model = new ProductAjaxModel() { list = new List<ProductContent>() };
            model.showIco = bool.Parse(data["content"]["showIco"].ToString());
            model.showPrice = bool.Parse(data["content"]["showPrice"].ToString());
            model.showName = data["content"]["showName"].ToString() == "1";
            var prod = _iProductService.GetProductByIds(idArray);
            foreach (var id in idArray)
            {
                var pro = prod.FirstOrDefault(d => d.Id == id);
                if (pro != null)
                {
                    model.list.Add(
                   new ProductContent
                   {
                       product_id = pro.Id,
                       link = "/m-wap/Product/Detail/" + pro.Id.ToString(),
                       price = pro.MinSalePrice.ToString("f2"),
                       original_price = pro.MarketPrice.ToString("f2"),
                       pic = pro.ImagePath + "/1_350.png",
                       title = pro.ProductName,
                       is_limitbuy = _iProductService.IsLimitBuy(pro.Id),
                       SaleCounts = pro.SaleCounts
                   });
                }
            }
            return this.ControllerContext.RenderViewToString(name, model);
        }

        /// <summary>
        /// 商品分组模块
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetGoodGroupTag(string path, JToken data)
        {
            try
            {
                string html = "@{Html.RenderAction(\"GoodsListAction\", \"TemplateVisualizationProcess\",new { Layout=\""
                    + data["content"]["layout"] +
                    "\", ShowName=\"" + data["content"]["showName"] +
                    "\", ShowIco=\"" + data["content"]["showIco"] +
                    "\", ShowPrice=\"" + data["content"]["showPrice"] +
                    "\", DataUrl=\"" + Request.Form["getGoodGroupUrl"] +
                    "\", TemplateFile=\"" + path +
                    "\", GoodListSize=\"" + data["content"]["goodsize"] +
                    "\", FirstPriority=\"" + data["content"]["firstPriority"] +
                    "\", SecondPriority=\"" + data["content"]["secondPriority"] +
                    "\", ID=\"goods_" + Guid.NewGuid().ToString("N") + "\"});}";
                return html;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>  
        /// Base64加密  
        /// </summary>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public string Base64Code(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>  
        /// Base64解密  
        /// </summary>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public string Base64Decode(string message)
        {
            byte[] bytes = Convert.FromBase64String(message);
            return Encoding.UTF8.GetString(bytes);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion



        #region Hi_Ajax_GetTemplateByID
        public ActionResult Hi_Ajax_GetTemplateByID(string client, int type = 2)
        {
            string templatePath = VTemplateHelper.GetTemplatePath(client, (VTemplateClientTypes)type, CurrentSellerManager.ShopId);
            var datapath = templatePath + "data/default.json";
            StreamReader sr = new StreamReader(Server.MapPath(datapath),
                System.Text.Encoding.UTF8);
            try
            {
                string input = sr.ReadToEnd();
                sr.Close();
                input = input.Replace("\r\n", "").Replace("\n", "");
                return Content(input);
            }
            catch
            {
                return Content("");
            }
        }


        #endregion




        #region Hi_Ajax_GetFolderTree
        public ActionResult Hi_Ajax_GetFolderTree(string areaName = "")
        {
            long shopId = CurrentSellerManager.ShopId;
            CouponsAjaxModel model = new CouponsAjaxModel() { list = new List<CouponsContent>() };
            return Json(GetTreeListJson(shopId), JsonRequestBehavior.AllowGet);
        }

        private PhotoCategoryAjaxModel GetTreeListJson(long shopId = 0)
        {
            PhotoCategoryAjaxModel model = new PhotoCategoryAjaxModel() { status = "1", msg = "", data = new photoCateNumber() };
            model.data.total = _iPhotoSpaceService.GetPhotoList("", 0, 10, 1, 0, shopId).Total.ToString();
            GetImgTypeJson(model, shopId);
            return model;
        }
        private void GetImgTypeJson(PhotoCategoryAjaxModel model, long shopId = 0)
        {

            //string json = "{\"name\":\"所有图片\",\"subFolder\":[],\"id\":0,\"picNum\":" + GalleryHelper.GetPhotoList("", 0, 10, PhotoListOrder.UploadTimeDesc).TotalRecords + "},";
            var list = new List<photoCateContent>();
            string json = "";
            var cate = _iPhotoSpaceService.GetPhotoCategories(shopId).ToList();
            for (int i = 0; i < cate.Count(); i++)
            {
                list.Add(new photoCateContent
                {
                    id = cate[i].Id.ToString(),
                    name = cate[i].PhotoSpaceCatrgoryName,
                    parent_id = 0,
                    picNum = _iPhotoSpaceService.GetPhotoList("", 0, 10, 1, cate[i].Id, shopId).Total.ToString()
                });
            }
            model.data.tree = list;
        }

        #endregion


        #region Hi_Ajax_AddFolder

        public ActionResult Hi_Ajax_AddFolder(string areaName = "")
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                var photoSpace = _iPhotoSpaceService.AddPhotoCategory("新建文件夹", shopId);
                return Json(new { status = "1", data = photoSpace.Id, msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "1", data = 0, msg = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion



        #region Hi_Ajax_GetImgList

        public ActionResult Hi_Ajax_GetImgList(int id = 0, string areaName = "", int p = 0, string file_Name = "")
        {
            long shopId = CurrentSellerManager.ShopId;
            var photo = _iPhotoSpaceService.GetPhotoList(file_Name, p, 24, 1, id, shopId);
            PhotoSpaceAjaxModel model = new PhotoSpaceAjaxModel() { status = "1", msg = "" };
            InitialPhotoSpaceModel(model, photo, p);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        private void InitialPhotoSpaceModel(PhotoSpaceAjaxModel model, ObsoletePageModel<PhotoSpaceInfo> photo, int pageNo = 1)
        {

            var list = new List<photoContent>();
            int pageCount = TemplatePageHelper.GetPageCount(photo.Total, 24);
            model.page = TemplatePageHelper.GetPageHtml(pageCount, pageNo);
            foreach (var item in photo.Models)
            {
                list.Add(new photoContent
                {
                    file = Core.HimallIO.GetImagePath(item.PhotoPath),
                    id = item.Id.ToString(),
                    name = item.PhotoName
                });
            }
            model.data = list;
        }


        #endregion


        #region Hi_Ajax_RenameFolder

        public ActionResult Hi_Ajax_RenameFolder(long category_img_id, string name)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                Dictionary<long, string> photoCategorys = new Dictionary<long, string>();
                photoCategorys.Add(category_img_id, name);
                _iPhotoSpaceService.UpdatePhotoCategories(photoCategorys, shopId);
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion



        #region Hi_Ajax_RenameImg

        public ActionResult Hi_Ajax_RenameImg(long file_id, string file_name)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                _iPhotoSpaceService.RenamePhoto(file_id, file_name, shopId);
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion



        #region Hi_Ajax_RemoveImgByFolder

        public ActionResult Hi_Ajax_RemoveImgByFolder(long cid, int cate_id)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                var mamagerRecordset = _iPhotoSpaceService.GetPhotoList("", 1, 100000000, 1, cid, shopId);
                List<long> list = new List<long>();
                foreach (var item in mamagerRecordset.Models)
                {
                    list.Add(item.Id);
                }
                _iPhotoSpaceService.MovePhotoType(list, cate_id);
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "请选择一个分类" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion




        #region Hi_Ajax_MoveImg

        public ActionResult Hi_Ajax_MoveImg(int cate_id, string file_id)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                List<long> ids = file_id.Split(',').ToList<string>().Select(x => long.Parse(x)).ToList();
                _iPhotoSpaceService.MovePhotoType(ids, cate_id, shopId);
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "请选择一个分类" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion



        #region Hi_Ajax_DelImg

        public ActionResult Hi_Ajax_DelImg(string file_id)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                string[] ids = file_id.Split(',');
                foreach (string id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    _iPhotoSpaceService.DeletePhoto(Convert.ToInt64(id), shopId);
                }
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "请勾选图片" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region Hi_Ajax_DelFolder

        public ActionResult Hi_Ajax_DelFolder(string id)
        {
            try
            {
                long shopId = CurrentSellerManager.ShopId;
                _iPhotoSpaceService.DeletePhotoCategory(Convert.ToInt64(id), shopId);
                return Json(new { status = "1", msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "0", msg = "请选择一个分类" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region 私有方法
        /// <summary>
        /// 添加或修改专题
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="title"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private long AddOrUpdateTopic(long topicId, string title, string tags,string icon, PlatformType platform)
        {
            long result = 0;
            long shopid = CurrentSellerManager.ShopId;
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(tags))
            {
                throw new HimallException("请填写专题的标题与标签");
            }
            if (string.IsNullOrWhiteSpace(icon))
            {
                throw new HimallException("请上传专题的图标");
            }
            TopicInfo topic = new TopicInfo();
            if (topicId > 0)
            {
                topic = _iTopicService.GetTopicInfo(topicId);
                if (topic == null || topic.ShopId != shopid)
                {
                    throw new HimallException("错误的专题编号");
                }
                topic.Name = title;
                topic.Tags = tags;
                topic.TopImage = icon;
                topic.PlatForm = platform;
                topic.ShopId = shopid;
                _iTopicService.UpdateTopic(topic);
            }
            else
            {
                topic.Name = title;
                topic.Tags = tags;
                topic.TopImage = icon;
                topic.PlatForm = platform;
                topic.ShopId = shopid;
                _iTopicService.AddTopic(topic);
                topicId = topic.Id;
            }
            result = topicId;
            if (result <= 0)
            {
                throw new HimallException("数据添加异常");
            }
            return result;
        }
        /// <summary>
        /// 保存图标
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string SaveIcon(string filepath)
        {
            string result = filepath;
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                string dest = @"/Storage/Special/Icon/";

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
        /// <summary>
        /// 获取json对应值
        /// </summary>
        /// <param name="jt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string TryGetJsonString(JToken jt, string name)
        {
            string result = "";
            var _tmp = jt[name];
            if (_tmp != null)
            {
                result = _tmp.ToString();
            }
            return result;
        }
        /// <summary>
        /// 获取json对应值
        /// </summary>
        /// <param name="jt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string TryGetJsonString(JObject jt, string name)
        {
            string result = "";
            var _tmp = jt[name];
            if (_tmp != null)
            {
                result = _tmp.ToString();
            }
            return result;
        }
        #endregion
    }


    public static class TemplatePageHelper
    {
        public static int GetPageCount(int totalRecords, int pageSize)
        {
            int pageCount = 1;
            if (totalRecords % pageSize != 0)
                pageCount = (totalRecords / pageSize) + 1;
            else
                pageCount = totalRecords / pageSize;
            return pageCount;
        }
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <param name="pageCount">总页数</param>
        /// <param name="pageIndex">当前页</param>
        /// <returns>返回分页HTML</returns>
        public static string GetPageHtml(int pageCount, int pageIndex)
        {
            if (pageIndex < 1)
                pageIndex = 1;


            string prevPageHtml = "<a href='javascript:;' class='prev' href='javascript:void(0);' page='" + (pageIndex - 1) + "'></a>";
            if (pageIndex == 1)
                prevPageHtml = "<a href='javascript:;' class='prev disabled' ></a>";
            string pageNumHtml = "";
            if (pageCount > 10)
                prevPageHtml += PageNumHtmlMoreThanTen(pageCount, pageIndex);
            else
                prevPageHtml += PageNumHtmlLessThanTen(pageCount, pageIndex);
            string nextPageHtml = "<a href='javascript:;' class='next' href='javascript:void(0);' page='" + (pageIndex + 1) + "'></a>";
            if (pageIndex == pageCount)
                nextPageHtml = "<a href='javascript:;' class='next disabled' ></a>";
            return prevPageHtml + pageNumHtml + nextPageHtml;
        }

        public static string PageNumHtmlMoreThanTen(int pageCount, int pageIndex)
        {
            string pageNumHtml = "";
            bool showHidePage = true;
            if (pageIndex < 0)
                pageIndex = 1;


            int firstPage = 1;
            if (pageIndex > 10)
                firstPage = pageIndex - 5;

            int hidePage = firstPage + 10 + 1;

            if (hidePage >= pageCount)
            {
                hidePage = pageCount;
                showHidePage = false;
            }

            int lastPage = pageCount;
            if ((hidePage + 2) > pageCount)
                lastPage = 0;

            for (int i = firstPage; i < hidePage; i++)
            {
                if (i == pageIndex)
                    pageNumHtml += "<a class='cur' >" + i + "</a>";
                else
                    pageNumHtml += "<a href='javascript:void(0);' page='" + i + "' >" + i + "</a>";
            }
            if (showHidePage)
                pageNumHtml += "<a href='javascript:void(0);' page='" + hidePage + "' >.....</a>";

            if (lastPage != 0)
            {
                pageNumHtml += "<a href='javascript:void(0);' page='" + (lastPage - 2) + "' >" + (lastPage - 2) + "</a>";
                pageNumHtml += "<a href='javascript:void(0);' page='" + (lastPage - 1) + "' >" + (lastPage - 1) + "</a>";
            }
            return pageNumHtml;
        }

        public static string PageNumHtmlLessThanTen(int pageCount, int pageIndex)
        {
            string pageNumHtml = "";
            for (int i = 1; i <= pageCount; i++)
            {
                if (i == pageIndex)
                    pageNumHtml += "<a class='cur' >" + i + "</a>";
                else
                    pageNumHtml += "<a href='javascript:void(0);' page='" + i + "' >" + i + "</a>";
            }
            return pageNumHtml;
        }

        public static string RenderViewToString(this ControllerContext context, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = context.RouteData.GetRequiredString("action");

            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                var viewContext = new ViewContext(context,
                                  viewResult.View,
                                  context.Controller.ViewData,
                                  context.Controller.TempData,
                                  sw);
                try
                {
                    viewResult.View.Render(viewContext, sw);
                }
                catch (Exception ex)
                {
                    throw;
                }

                return sw.GetStringBuilder().ToString();
            }
        }

    }

}