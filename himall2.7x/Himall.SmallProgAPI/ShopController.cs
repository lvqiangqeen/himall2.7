using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Himall.DTO;
using Himall.Web.Framework;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 商家
    /// </summary>
    public class ShopController : BaseApiController
    {
        private IShopAppletService iShopAppletService;
        public ShopController()
        {
            iShopAppletService = ServiceProvider.Instance<IShopAppletService>.Create;
        }
    
        /// <summary>
        /// 获取附近商家列表
        /// </summary>
        /// <param name="Lng"></param>
        /// <param name="Lat"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="rangeSize"></param>
        /// <returns></returns>
        public object GetRoundShops(string Lng, string Lat, int pageIndex=1,int pageSize=10, int rangeSize = 1000)
        {
            ///112.998338,28.184037
            if (string.IsNullOrEmpty(Lng) || string.IsNullOrEmpty(Lat))
            {
                return Json(new { Status = "No",Msg="缺少经纬度参数" });
            }
            CheckUserLogin();
            int totalCount = 0;
            AppletShopQuery query = new AppletShopQuery();
            query.PageNo = pageIndex;
            query.PageSize= pageSize;
            var shops = iShopAppletService.GetRoundShops(Lng, Lat, CurrentUser.Id, query,out totalCount, rangeSize);
            var regionService = ServiceProvider.Instance<IRegionService>.Create;
            foreach (var m in shops)
            {
                m.CompanyRegionAddress = regionService.GetFullName(m.CompanyRegionId);
                m.Logo= Himall.Core.HimallIO.GetRomoteImagePath(m.Logo);
                if (m.DistanceMi >= 1000) //单位转换
                {
                    decimal tempKm = Math.Floor(m.DistanceMi/1000);
                    decimal tempMi = m.DistanceMi%1000;
                    m.Distance = tempKm + "km "+tempMi+"m";
                }
                else
                {
                    m.Distance = m.DistanceMi+"米";
                }

                if (string.IsNullOrEmpty(m.ContactsPhone))
                    m.ContactsPhone = "";
                if (string.IsNullOrEmpty(m.OpeningTime))
                    m.OpeningTime = "";
                if (string.IsNullOrEmpty(m.WelcomeTitle))
                    m.WelcomeTitle = "";
            }
            return Json(new { Status = "OK", VShop = shops, TotalCount= totalCount });
        }
        /// <summary>
        /// 获取店铺详情，包含拼团商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetShopDetail(long shopId,int pageIndex=1,int pageSize=10)
        {
            CheckUserLogin();
            var shopInfo = iShopAppletService.GetShopInfoDetail(shopId, CurrentUser.Id);

            List<FightGroupActiveStatus> seastatus = new List<FightGroupActiveStatus>();
            seastatus.Add(FightGroupActiveStatus.Ongoing);
            seastatus.Add(FightGroupActiveStatus.WillStart);
            var data = FightGroupApplication.GetActives(seastatus, "", "", shopId, pageIndex, pageSize);
            var datalist = data.Models.ToList();
            if (!string.IsNullOrEmpty(shopInfo.Logo))
            {
                shopInfo.Logo = Himall.Core.HimallIO.GetRomoteImagePath(shopInfo.Logo);
            }
            
            if (string.IsNullOrEmpty(shopInfo.ContactsPhone))
                shopInfo.ContactsPhone = "";
            if (string.IsNullOrEmpty(shopInfo.OpeningTime))
                shopInfo.OpeningTime = "";
            if (string.IsNullOrEmpty(shopInfo.WelcomeTitle))
                shopInfo.WelcomeTitle = "";
            foreach (var d in datalist)
            {
                d.IconUrl= Himall.Core.HimallIO.GetRomoteImagePath(d.IconUrl);
              
            }
            shopInfo.MsgCount = 2;
            return Json(new { Status = "OK", ShopInfo= shopInfo, FightGroupData = datalist,TotalCount= data.Total,UserId= CurrentUser.Id });
        }
        /// <summary>
        /// 获取拼团活动详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetFightGroupDetail(long id, long groupid = 0)
        {
            CheckUserLogin();
            IFightGroupService _iFightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
            FightGroupActiveModel data = FightGroupApplication.GetActive(id, false);
            if (data == null)
            {
                throw new HimallException("错误的活动信息");
            }
            data.InitProductImages();
            AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveDetailModel>();

            FightGroupActiveDetailModel model = AutoMapper.Mapper.Map<FightGroupActiveDetailModel>(data);

            var shopInfo = iShopAppletService.GetShopInfo(model.ShopId);
            if (shopInfo != null)
            {
                if (!string.IsNullOrEmpty(shopInfo.Logo))
                {
                    shopInfo.Logo = Himall.Core.HimallIO.GetRomoteImagePath(shopInfo.Logo);
                }

                bool shopIsSub = ServiceProvider.Instance<IShopService>.Create.IsFavoriteShop(CurrentUser.Id, model.ShopId);
                shopInfo.IsSubscribe = shopIsSub ? "1" : "0";
            }
            

            model.ShareTitle = data.ActiveStatus == FightGroupActiveStatus.WillStart ? "限时限量火拼 即将开始" : "限时限量火拼 正在进行";
            model.ShareImage = data.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model.ShareImage))
            {
                if (model.ShareImage.Substring(0, 4) != "http")
                {
                    model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage);
                }
            }

            if (model.IconUrl.Substring(0, 4) != "http")
            {
                model.IconUrl = HimallIO.GetRomoteImagePath(model.IconUrl);
            }
            if (!string.IsNullOrEmpty(model.ProductImgPath) && model.ProductImgPath.Substring(0, 4) != "http")
            {
                model.ProductImgPath = HimallIO.GetRomoteImagePath(model.ProductImgPath);
            }
            if (!string.IsNullOrEmpty(model.ProductDefaultImage) && model.ProductDefaultImage.Substring(0, 4) != "http")
            {
                model.ProductDefaultImage = HimallIO.GetRomoteImagePath(model.ProductDefaultImage);
            }
            for (int i = 0; i < model.ProductImages.Count; i++)
            {
                model.ProductImages[i] = HimallIO.GetRomoteImagePath(model.ProductImages[i]);
            }
            model.ShareDesc = data.ProductName;
            if (!string.IsNullOrWhiteSpace(data.ProductShortDescription))
            {
                model.ShareDesc += "，(" + data.ProductShortDescription + ")";
            }
            var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription(model.ProductId.Value);
            if (ProductDescription == null)
            {
                //throw new Himall.Core.HimallException("错误的商品编号");

                if (model.ShowMobileDescription == null)
                {
                    model.ShowMobileDescription = "";
                }
            }
            else
            {
                model.ShowMobileDescription = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/",
                "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }

            model.GroupNotice = data.GroupNotice;
            if (model.ProductId.HasValue)
            {
                //统计商品浏览量、店铺浏览人数
                StatisticApplication.StatisticVisitCount(model.ProductId.Value, model.ShopId);
            }
            IProductService _iProductService = ServiceProvider.Instance<IProductService>.Create;
            bool isSub = _iProductService.IsFavorite((long)model.ProductId, CurrentUser.Id);
            model.IsSubscribe = isSub ? 1 : 0;
            model.IsVirtualProduct = 1;
            model.ComboList = new List<DTO.ComboDetail>();

            List<Himall.Model.ComboDetail> comnoDetailList = _iFightGroupService.GetAddComboList((long)data.Id);
            foreach (Himall.Model.ComboDetail m in comnoDetailList)
            {
                Himall.DTO.ComboDetail detail = new Himall.DTO.ComboDetail();
                detail.ComboName = m.ComboName;
                detail.ComboQuantity = m.ComboQuantity;
                detail.ComboPrice = m.ComboPrice;
                model.ComboList.Add(detail);
            }

            #region 商品规格
            ProductInfo product = ServiceProvider.Instance<IProductService>.Create.GetProduct(model.ProductId.Value);

            //参数没有缺省值的话不传可能会报404
            ProductShowSkuInfoModel skuModel = new ProductShowSkuInfoModel();
            skuModel.MinSalePrice = data.MiniSalePrice;
            skuModel.ProductImagePath = string.IsNullOrWhiteSpace(data.ProductImgPath) ? "" : HimallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)Himall.CommonModel.ImageSize.Size_350);
            foreach (var item in data.ActiveItems)
            {
                item.ShowPic = HimallIO.GetRomoteImagePath(item.ShowPic);
            }
            //List<SKUDataModel> skudata = data.ActiveItems.Where(d => d.ActiveStock > 0).Select(d => new SKUDataModel
            //{
            //    SkuId = d.SkuId,
            //    Color = d.Color,
            //    Size = d.Size,
            //    Version = d.Version,
            //    Stock = (int)d.ActiveStock,
            //    CostPrice = d.ProductCostPrice,
            //    SalePrice = d.ProductPrice,
            //    Price = d.ActivePrice,
            //}).ToList();

            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            skuModel.ColorAlias = colorAlias;
            skuModel.SizeAlias = sizeAlias;
            skuModel.VersionAlias = versionAlias;

            if (model.ActiveItems != null && model.ActiveItems.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                var skus = ServiceProvider.Instance<IProductService>.Create.GetSKUs((long)model.ProductId);

                int JoinedNumber = 0;
                if (groupid != 0)
                {
                    FightGroupsInfo fightGroupInfo = _iFightGroupService.GetGroup(id, groupid);
                    if (fightGroupInfo != null)
                    {
                        JoinedNumber = Convert.ToInt32(fightGroupInfo.JoinedNumber);
                    }
                }
                

                foreach (var sku in model.ActiveItems)
                {
                    if (JoinedNumber > 0)
                    {
                        sku.ActivePrice = sku.ActivePrice - model.ReturnMoney * JoinedNumber;
                    }
                    var specs = sku.SkuId.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!skuModel.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = model.ActiveItems.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.ActiveStock);
                                skuModel.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? " " : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = string.IsNullOrWhiteSpace(sku.ShowPic) ? "" : Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!skuModel.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = model.ActiveItems.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.ActiveStock);
                                skuModel.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!skuModel.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = model.ActiveItems.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.ActiveStock);
                                skuModel.Version.Add(new ProductSKU
                                {
                                    //Name = "选择规格",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }



            }
            #endregion

            return Json(new { Status = "OK", FightGroupDetail = model, ShopInfo = shopInfo, ShowSkuInfo = skuModel });
        }
        /// <summary>
        /// 关注店铺
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]        
        public object AddFavorite(long shopId)
        {
            CheckUserLogin();
            if (CurrentUser == null)
                return  Json(new { Status = "No", Message = "请先登录" }); 
            IShopService _iShopService =ServiceProvider.Instance<IShopService>.Create;
            try
            {
                _iShopService.AddFavoriteShop(CurrentUser.Id, shopId);
            }
            catch (Exception ex)
            {
                if(ex is HimallException&&ex.Message== "您已经关注过该店铺")
                    return Json(new { Status = "OK", Message = "您已经关注过该店铺" });
            }
          
            return Json(new { Status = "OK", Message = "成功关注该微店" }); 
        }
        /// <summary>
        /// 取消关注
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]
        public object DeleteFavorite(long shopId)
        {
            CheckUserLogin();
            if (CurrentUser == null)
                return Json(new { Status = "No", Message = "请先登录" });
            IShopService _iShopService = ServiceProvider.Instance<IShopService>.Create;
            _iShopService.CancelConcernShops(shopId, CurrentUser.Id);
            return Json(new { Status = "OK", Message = "成功取消关注该微店" });
        }

        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <param name="skuId">规则ID</param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet]
        public object CheckBuyNumber(long id, string skuId)
        {
            CheckUserLogin();
            int exist = 0;
            if (CurrentUser != null)
            {
                exist = FightGroupApplication.GetMarketSaleCountForUserId(id, CurrentUser.Id);
            }
            else
            {
                exist = 0;  //未登录用户默认为未买
            }
            return Json(new { Status = "OK", HasBuyCount = exist }); 

        }

        [HttpGet]
        public object CanJoin(long id, long groupId)
        {
            CheckUserLogin();
            if (FightGroupApplication.CanJoinGroup(id, groupId, CurrentUser.Id))
            {
                return Json(new { Status = "OK", JoinStatus = true, Message = "可参团" });
            }
            else
            {
                return Json(new { Status = "OK", JoinStatus = false, Message = "不可重复参团" });
            }          
        }

        /// <summary>
        /// 获取某个用户最后关注的店铺，没有关注的店铺则返回0
        /// </summary>
        /// <returns></returns>
        public object GetLastFavoriteShop()
        {
            CheckUserLogin();
            long shopId=iShopAppletService.GetLastFavoriteShop(CurrentUser.Id);
            return Json(new { Status = "OK", ShopId = shopId });
        }
        /// <summary>
        /// 提交拼团订单
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="recieveAddressId"></param>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public object PostSubmitFightGroupOrder(long shopId, string skuId,long count,long recieveAddressId,long activeId, long groupId=0, long invitationUserId = 0)
        {
            CheckUserLogin();
           
            bool isCashOnDelivery = false;
            int invoiceType =(int) InvoiceType.None ;//发票类型，默认不需要
            string invoiceTitle = "";
            string invoiceContext = "";

            string orderRemarks = "";//value.orderRemarks;//订单备注
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            var groupData = ServiceProvider.Instance<IFightGroupService>.Create.GetActive(activeId, false, false);

            if (count > groupData.LimitQuantity)
            {
                return Json(new { Status = "No", Message = string.Format("每人限购数量：{0}！", groupData.LimitQuantity) });
            }
            if (groupId > 0)   //非开团，判断人数限制
            {
                var orderData = ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderByGroupId(seastatus, groupId);
                if (orderData != null && groupData.LimitedNumber <= orderData.Count)
                {
                    return Json(new { Status = "No", Message = "该团参加人数已满！" });
                }
            }

            try
            {
                var model = OrderApplication.GetGroupOrder(CurrentUser.Id, skuId, count.ToString(), recieveAddressId, invoiceType, invoiceTitle, invoiceContext, activeId, PlatformType.Android, groupId, isCashOnDelivery, orderRemarks, invitationUserId);
                //CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
                //model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
                //model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
                model.ShopId = shopId;
                var orderIds = OrderApplication.SmallProgOrderSubmit(model,out groupId);
                AddVshopBuyNumber(orderIds);//添加微店购买数量
                return Json(new { Status = "OK", OrderIds = orderIds,GroupId= groupId,UserId= CurrentUser.Id });
            }
            catch (HimallException he)
            {
                return Json(new { Status = "No", Message = he.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Status = "No", Message = "提交订单异常" });
            }
        }
        void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            var shopIds = ServiceProvider.Instance<IOrderService>.Create.GetOrders(orderIds).Select(item => item.ShopId);//从订单信息获取店铺id
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var vshopIds = shopIds.Select(item =>
            {
                var vshop = vshopService.GetVShopByShopId(item);
                if (vshop != null)
                    return vshop.Id;
                else
                    return 0;
            }
                ).Where(item => item > 0);//从店铺id反查vshopId

            foreach (var vshopId in vshopIds)
                vshopService.AddBuyNumber(vshopId);
        }
        /// <summary>
        /// 我的开团列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetJoinGroups(int pageIndex=1, int status=-99, int pageSize=5)
        {
            CheckUserLogin();
            long curUserId = CurrentUser.Id;
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            //seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            /// <summary>
            /// 参团失败
            /// <para>未付款等情况</para>
            /// </summary>
            //[Description("参团失败")]
            //JoinFailed = -1,
            ///// <summary>
            ///// 正在参与
            ///// </summary>
            //[Description("正在参与")]
            //Ongoing = 0,
            ///// <summary>
            ///// 参团成功
            ///// <para>等待其他团友，不可发货</para>
            ///// </summary>
            //[Description("参团成功")]
            //JoinSuccess = 1,
            ///// <summary>
            ///// 拼团失败
            ///// <para>已付款，但拼团超时人数未满</para>
            ///// </summary>
            //[Description("拼团失败")]
            //BuildFailed = 2,
            ///// <summary>
            ///// 组团成功
            ///// <para>可以发货</para>
            ///// </summary>
            //[Description("组团成功")]
            //BuildSuccess = 4,
            if (status == -99)
            {
                seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
                seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
                seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
                seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
                seastatus.Add(FightGroupOrderJoinStatus.JoinFailed);
            }
            else if (status == 0)
            {
                seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
                seastatus.Add(FightGroupOrderJoinStatus.BuildOpening);
                seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            }
            else if (status == 1)
            {
                seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            }

            //else if (status ==1)//等待开团（未付款的不算)
            //{
            //    seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            //    seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            //}
            //else if (status == 2)//拼团成功
            //{
            //    seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            //}
            //else if (status ==3)//拼团失败
            //{
            //    seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            //    seastatus.Add(FightGroupOrderJoinStatus.JoinFailed);
            //}
            var data = FightGroupApplication.GetJoinGroups(curUserId, seastatus, pageIndex, pageSize);
            var datalist = data.Models.ToList();
            List<MyFightGroupPostJoinGroupsModel> resultlist = new List<MyFightGroupPostJoinGroupsModel>();
            foreach (var item in datalist)
            {
                //var shopInfo = ShopApplication.GetShop(model.ShopId);
                MyFightGroupPostJoinGroupsModel _tmp = new MyFightGroupPostJoinGroupsModel();
                _tmp.Id = item.Id;
                _tmp.ActiveId = item.ActiveId;
                _tmp.ProductName = item.ProductName;
                _tmp.ProductId = item.ProductId;
                _tmp.ProductImgPath = HimallIO.GetRomoteImagePath(item.ProductImgPath);
                _tmp.ProductDefaultImage = HimallIO.GetRomoteImagePath(HimallIO.GetProductSizeImage(item.ProductImgPath, 1, (int)ImageSize.Size_350));
                _tmp.GroupEndTime = item.OverTime.HasValue ? item.OverTime.Value : item.GroupEndTime;
                _tmp.BuildStatus = item.BuildStatus;
                _tmp.BuildStatusText = item.BuildStatus.ToDescription();
                _tmp.NeedNumber = item.LimitedNumber - item.JoinedNumber;

                _tmp.ShopName = item.ShopName;
                _tmp.ShopLogo = HimallIO.GetRomoteImagePath(item.ShopLogo);
                _tmp.ShopId = item.ShopId;
             
                _tmp.UserIcons = new List<string>();
                foreach (var sitem in item.GroupOrders)
                {
                    _tmp.UserIcons.Add(sitem.Photo);
                    if (sitem.OrderUserId == curUserId)
                    {
                        OrderDetailView view = OrderApplication.Detail(sitem.OrderId, curUserId, PlatformType.WeiXinSmallProg, "");
                        foreach (var info in view.Order.OrderItemInfo)
                        {
                            _tmp.BuyNum = info.Quantity;
                            _tmp.Sku = info.Color + " " + info.Size + " " + info.Version;
                            _tmp.SalePrice = iShopAppletService.GetSkuSalePrice(info.SkuId, info.ProductId);
                        }
                        _tmp.OrderId = sitem.OrderId;
                        _tmp.GroupPrice = sitem.SalePrice;
                    }
                }
                resultlist.Add(_tmp);
            }
            return Json(new { Status = "OK", GroupDatas = resultlist, TotalCount = data.Total,OrderUserId=curUserId });
        }


        [HttpGet]
        public object AddFavoriteProduct(long pid)
        {
            CheckUserLogin();
            int status = 0;
            IProductService _iProductService = ServiceProvider.Instance<IProductService>.Create;
            _iProductService.AddFavorite(pid, CurrentUser.Id, out status);
            return Json(new { Status = "OK", Message = "成功关注" });
        }

        [HttpGet]
        public object DeleteFavoriteProduct(long pid)
        {
            CheckUserLogin();
            IProductService _iProductService = ServiceProvider.Instance<IProductService>.Create;
            _iProductService.DeleteFavorite(pid, CurrentUser.Id);
            return Json(new { Status = "OK", Message = "已取消关注" });
        }

        /// <summary>
        /// 会员店铺消息
        /// </summary> 
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetShopMsgList(int pageIndex = 1, int pageSize = 10)
        {
            List< ShopMsgModel > infos =new List<ShopMsgModel>();
            ShopMsgModel model = new ShopMsgModel();
            model.ShopId = 1;
            model.ShopName = "官方自营店";
            model.ShopLogo = "http://wzp.duoyunfenxiao.com//Storage/Shop/1/Products/749/4_50.png";
            model.MsgTitle = "消费成功";
            model.Msg = "订单号[10086]消费成功";
            model.MsgTime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");

            ShopMsgModel msg = new ShopMsgModel();
            msg.ShopId = 1;
            msg.ShopName = "官方自营店";
            msg.ShopLogo = "http://wzp.duoyunfenxiao.com//Storage/Shop/1/Products/749/4_50.png";
            msg.MsgTitle = "参团成功";
            msg.Msg = "恭喜您，您的团号[1008611]已有好友参团";
            msg.MsgTime = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");

            infos.Add(msg);
            infos.Add(model);
            return Json(new { Status = "OK",ShopMsgList = infos, TotalCount = 2});
        }

        /// <summary>
        /// 获取拼团团组详情
        /// </summary>
        /// <param name="activeId">拼团活动ID</param>
        /// <param name="groupId">拼团团组ID</param>
        /// <returns></returns>
        public object GetGroupDetail(long activeId, long groupId)
        {
            CheckUserLogin();
            FightGroupActiveModel data = FightGroupApplication.GetActive(activeId, false);
            if (data == null)
            {
                throw new HimallException("错误的活动信息");
            }
            data.InitProductImages();
            AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveDetailModel>();

            FightGroupActiveDetailModel model = AutoMapper.Mapper.Map<FightGroupActiveDetailModel>(data);
          
            var shopInfo = iShopAppletService.GetShopInfoDetail(model.ShopId, CurrentUser.Id);
            shopInfo.Logo = Himall.Core.HimallIO.GetRomoteImagePath(shopInfo.Logo);
            // ViewBag.IsSelf = shopInfo.IsSelf;

            //AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveResult>();
            //var fightGroupData = AutoMapper.Mapper.Map<FightGroupActiveResult>(model);
             
            model.ShareTitle = data.ActiveStatus == FightGroupActiveStatus.WillStart ? "限时限量火拼 即将开始" : "限时限量火拼 正在进行";
            model.ShareImage = data.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model.ShareImage))
            {
                if (model.ShareImage.Substring(0, 4) != "http")
                {
                    model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage);
                }
            }

            if (model.IconUrl.Substring(0, 4) != "http")
            {
                model.IconUrl = HimallIO.GetRomoteImagePath(model.IconUrl);
            }
            if (!string.IsNullOrEmpty(model.ProductImgPath) && model.ProductImgPath.Substring(0, 4) != "http")
            {
                model.ProductImgPath = HimallIO.GetRomoteImagePath(model.ProductImgPath);
            }
            if (!string.IsNullOrEmpty(model.ProductDefaultImage) && model.ProductDefaultImage.Substring(0, 4) != "http")
            {
                model.ProductDefaultImage = HimallIO.GetRomoteImagePath(model.ProductDefaultImage);
            }
            for (int i = 0; i < model.ProductImages.Count; i++)
            {
                model.ProductImages[i] = HimallIO.GetRomoteImagePath(model.ProductImages[i]);
            }
            model.ShareDesc = data.ProductName;
            if (!string.IsNullOrWhiteSpace(data.ProductShortDescription))
            {
                model.ShareDesc += "，(" + data.ProductShortDescription + ")";
            }
            var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription(model.ProductId.Value);
            if (ProductDescription == null)
            {
                //throw new Himall.Core.HimallException("错误的商品编号");
                model.ShowMobileDescription = "";
            }
            else
            {
                model.ShowMobileDescription = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/",
                   "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }
            

            FightGroupsModel groupsdata = FightGroupApplication.GetGroup(activeId, groupId);
            if (groupsdata == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            model.AddGroupTime = groupsdata.AddGroupTime;
        
            TimeSpan mids = DateTime.Now - (DateTime)model.AddGroupTime;
            model.Seconds = (int)(data.LimitedHour * 3600) - (int)mids.TotalSeconds;
            //拼装已参团成功的用户
            var userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers(activeId, groupId);
            foreach (var userItem in userList)
            {
                var userInfo = new UserInfo();
                userInfo.Photo = !string.IsNullOrWhiteSpace(userItem.Photo) ? Core.HimallIO.GetRomoteImagePath(userItem.Photo) : "";
                userInfo.UserName = userItem.UserName;
                userInfo.JoinTime = userItem.JoinTime;
                model.UserInfo.Add(userInfo);
            }
            //获取团长信息
            var GroupsData = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup(activeId, groupId);
            if (GroupsData != null)
            {
                model.HeadUserName = GroupsData.HeadUserName;
                model.HeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.HeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.HeadUserIcon) : "";
                model.ShowHeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.ShowHeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.ShowHeadUserIcon) : "";
            }

            model.GroupNotice = data.GroupNotice;
            model.ShareGroupmoney = 6.5M;
            model.OpenGroupmoney = 5M;


            IProductService _iProductService = ServiceProvider.Instance<IProductService>.Create;
            bool isSub = _iProductService.IsFavorite((long)model.ProductId, CurrentUser.Id);
            model.IsSubscribe = isSub ? 1 : 0;

            return Json(new { Status = "OK", FightGroupDetail = model, ShopInfo = shopInfo });
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object ShareGetShopBonus(int pageIndex = 1, int pageSize = 10)
        {
            List<ShopMsgModel> infos = new List<ShopMsgModel>();
            ShopMsgModel model = new ShopMsgModel();
            model.ShopId = 1;
      
             

            ShopMsgModel msg = new ShopMsgModel();
            msg.ShopId = 1;
      
            infos.Add(msg);
            infos.Add(model);
            return Json(new { Status = "OK", ShopMsgList = infos, TotalCount = 2 });
        }

        /// <summary>
        /// 商家订单  虚拟订单
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
         [HttpGet]
        public object GetOrders(int? status, int pageIndex, int pageSize = 10,int shopId=0, string orderId="", string productName="")
        {
          //  CheckUserLogin();
            IShopAppletService  iShopAppletService = ServiceProvider.Instance<IShopAppletService>.Create;
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var allOrders = orderService.GetOrdersOnshop(int.MaxValue, shopId);

           FightGroupsInfo  fightGroup;
           FightGroupActiveInfo  fightGroupctive;
            //待评价
            var queryModelAll = new OrderQuery()
            {
                Status = OrderInfo.OrderOperateStatus.Finish,
                ShopId=shopId,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            var allOrderCounts = allOrders.Count();
            var waitingForComments = orderService.GetOrdersOnshop<OrderInfo>(queryModelAll).Total;
            var waitingForRecieve = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving);//获取待收货订单数
            var waitingForPay = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数

            if (status.HasValue && status == 0)
            {
                status = null;
            }
            //获取了状态
            var queryModel = new OrderQuery()
            {
               Status = (OrderInfo.OrderOperateStatus?)status,
            
                PageSize = pageSize,
                PageNo = pageIndex,
                ShopId=shopId,
                OrderType=3
            };
            //if (queryModel.Status.HasValue && queryModel.Status.Value == OrderInfo.OrderOperateStatus.WaitReceiving)
            //{
            //    if (queryModel.MoreStatus == null)
            //    {
            //        queryModel.MoreStatus = new List<OrderInfo.OrderOperateStatus>() { };
            //    }
            //    queryModel.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitSelfPickUp);
            //}
            //if (status.GetValueOrDefault() == (int)OrderInfo.OrderOperateStatus.Finish)
            //    queryModel.Commented = false;//只查询未评价的订单
            ObsoletePageModel<OrderInfo> orders = orderService.GetOrdersOnshop<OrderInfo>(queryModel);
           var total = orderService.GetOrdersOnshop<OrderInfo>(queryModel).Total;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            var orderRefunds = OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));
            decimal ?returnMoneyOff = 0;
             decimal ?returnMoneyOn = 0;
             System.TimeSpan? jiezhitime1;
              System.TimeSpan jiezhitime0;
             string jiezhitime=null;
           decimal  realTotalPrice=0;
             decimal ?tuanPrice = 0;
             //增加拼团信息
             FightGroupOrderInfo model = new FightGroupOrderInfo();
                
            var result = orders.Models.ToArray().Select(item =>
            {
                if (item.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    orderService.CalculateOrderItemRefund(item.Id);
                }
                var vshop = vshopService.GetVShopByShopId(item.ShopId);
                var _ordrefobj = orderRefundService.GetOrderRefundByOrderId(item.Id) ?? new OrderRefundInfo { Id = 0 };
                if (item.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && item.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    _ordrefobj = new OrderRefundInfo { Id = 0 };
                }
                int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
                ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
                //参照PC端会员中心的状态描述信息
                string statusText = item.OrderStatus.ToDescription();
                if (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    if (ordrefstate.HasValue && ordrefstate != 0 && ordrefstate!=4)
                    {
                        statusText = "退款中";
                    }
                }
             //   var shopInfo = iShopAppletService.GetShopInfoDetail(item.ShopId, CurrentUser.Id);
              //  shopInfo.Logo = Himall.Core.HimallIO.GetRomoteImagePath(shopInfo.Logo);
                //是否可退货、退款
                bool IsShowReturn = (item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    && !item.RefundStats.HasValue && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.CashOnDelivery && item.PaymentType != Himall.Model.OrderInfo.PaymentTypes.None
                    && (item.FightGroupCanRefund == null || item.FightGroupCanRefund == true) && ordrefstate.GetValueOrDefault().Equals(0);
                //拼团
                if (ServiceProvider.Instance<IFightGroupService>.Create.IsExistOrder(item.Id, shopId))
                {
                       model= ServiceProvider.Instance<IFightGroupService>.Create.GetOrder(item.Id);
                    //获取团信息
                fightGroup = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup((long)model.ActiveId,(long)model.GroupId);
                }
                else
                {
                      // model= ServiceProvider.Instance<IFightGroupService>.Create.GetOrder(item.Id);
                    //获取团信息
                fightGroup = null;
                }
                fightGroupctive=ServiceProvider.Instance<IFightGroupService>.Create.GetActive((long)model.ActiveId);
                for (int i = 1; i <= model.JoinedNumber; i++)
                {
                    returnMoneyOff += (i - 1) * fightGroupctive.ReturnMoney;
                }
                 for (int i = 1; i <= fightGroupctive.LimitQuantity-model.JoinedNumber; i++)
                {
                    returnMoneyOn += (i - 1) * fightGroupctive.ReturnMoney;
                }
                 if (fightGroup.OverTime != null)
                 {

                     jiezhitime1 = fightGroup.OverTime - DateTime.Now;

                     jiezhitime0 = (TimeSpan)jiezhitime1;
                     if (jiezhitime0.Seconds < 0)
                     {
                         jiezhitime = "已过期" + jiezhitime0.Days.ToString() + "天" + jiezhitime0.Hours.ToString() + "时" + jiezhitime0.Minutes.ToString() + "分" + jiezhitime0.Seconds.ToString() + "秒";
                     }
                     else 
                     { 
                           jiezhitime = jiezhitime0.Days.ToString() + "天" + jiezhitime0.Hours.ToString() + "时" + jiezhitime0.Minutes.ToString() + "分" + jiezhitime0.Seconds.ToString() + "秒";
                     }
                   
                     //jiezhitime = jiezhitime0.ToString();
                 }
                 else { jiezhitime = null; }

                return new
                {
                    ShopId=item.ShopId,
                  //  ShopLogo=shopInfo.Logo,
                    OrderAmount=item.OrderAmount,
                    OrderId = item.Id,
                    StatusText = statusText,
                    Status = item.OrderStatus,
                    orderType = item.OrderType,
                    orderTypeName = item.OrderType.ToDescription(),
                    shopname = item.ShopName,
                    vshopId = vshop == null ? 0 : vshop.Id,
                    Amount = item.OrderTotalAmount.ToString("F2"),
                    Quantity = item.OrderProductQuantity,
                    commentCount = item.OrderCommentInfo.Count(),
                    pickupCode = item.PickupCode,
                    EnabledRefundAmount = item.OrderEnabledRefundAmount,
                    LineItems = item.OrderItemInfo.Select(a =>
                    {
                        var prodata = productService.GetProduct(a.ProductId);
                        ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(prodata.TypeId);
                        string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                        var itemStatusText = "";
                        var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                        int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                        itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                      //  string img = Core.HimallIO.GetRomoteProductSizeImage(a.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_350);
                        string img = fightGroupctive.ProductDefaultImage;
                        if (itemrefund != null)
                        {//默认为商家处理进度
                            if (itemrefstate == 4)
                            {//商家拒绝
                                itemStatusText = "";
                            }
                            else
                            {
                                itemStatusText = "售后处理中";
                            }
                        }
                        if (itemrefstate > 4)
                        {//如果商家已经处理完，则显示平台处理进度
                            if (itemrefstate == 7)
                            {
                                itemStatusText = "退款成功";
                            }
                        }
                        return new
                        {
                            Status= itemrefstate,
                            StatusText= itemStatusText,
                            Id=a.SkuId,
                            productId = a.ProductId,
                            Name = a.ProductName,
                            Image = img,
                            Amount = a.Quantity,
                            Price = a.SalePrice,
                            TuanPrice=a.SalePrice-fightGroupctive.ReturnMoney,
                            Unit = prodata == null ? "" : prodata.MeasureUnit,
                            SkuText=colorAlias+":"+a.Color+" "+sizeAlias+":"+a.Size+" "+versionAlias+":"+a.Version,
                            color = a.Color,
                            size = a.Size,
                            version = a.Version,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            RefundStats = itemrefstate,
                            OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                            EnabledRefundAmount = a.EnabledRefundAmount,
                            IsShowRefund= IsShowReturn,
                            IsShowAfterSale= IsShowReturn,
                            SalePrice = iShopAppletService.GetSkuSalePrice(a.SkuId, a.ProductId)
                    };
                    }),
                    RefundStats = ordrefstate,
                    OrderRefundId = _ordrefobj.Id,
                    IsShowLogistics = !string.IsNullOrWhiteSpace(item.ShipOrderNumber),
                    ShipOrderNumber = item.ShipOrderNumber,
                    IsShowCreview = item.OrderStatus == OrderInfo.OrderOperateStatus.Finish,
                    IsShowPreview=false,
                    Invoice = item.InvoiceType.ToDescription(),
                    InvoiceValue = (int)item.InvoiceType,
                    InvoiceContext = item.InvoiceContext,
                    InvoiceTitle = item.InvoiceTitle,
                    PaymentType = item.PaymentType.ToDescription(),
                    PaymentTypeValue = (int)item.PaymentType,
                    IsShowClose = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay),
                    IsShowFinishOrder = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving),
                    IsShowRefund = IsShowReturn,
                    IsShowReturn = IsShowReturn,
                    IsShowTakeCodeQRCode = !string.IsNullOrWhiteSpace(item.PickupCode),
                    OrderDate = item.OrderDate,
                    SupplierId = 0,
                    ShipperName=string.Empty,
                    StoreName=item.ShopName,
                    IsShowCertification=false,
                    CreviewText = !HasAppendComment(item)?"评价订单":"追加评论",
                    ProductCommentPoint=0,
                    FightGroup=fightGroup,
                    FightGroupOrder=model,
                    ReturnMoneyOff=returnMoneyOff,
                    ReturnMoneyOn=returnMoneyOn,
                    Jiezhitime=jiezhitime
                };
            });
            return Json(new { Status = "OK", AllOrderCounts = allOrderCounts, WaitingForComments = waitingForComments, WaitingForRecieve = waitingForRecieve, WaitingForPay = waitingForPay,
                    FightGroupTatal=total, Data = result });
        }
        
        private bool HasAppendComment(OrderInfo list)
        {
            var item = list.OrderItemInfo.FirstOrDefault();
            var result = ServiceProvider.Instance<ICommentService>.Create.HasAppendComment(item.Id);
            return result;
        }
        /// <summary>
        /// 拼团订单
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // public object GetfightGroupOrder(int? status, int pageIndex=1, int pageSize = 10,long shopId=1,long OrderId=0,string producName="")
        //{
        //  //  CheckUserLogin();
        //    IShopAppletService  iShopAppletService = ServiceProvider.Instance<IShopAppletService>.Create;
        //  var  fightGroupOrderInfo = ServiceProvider.Instance<IFightGroupService>.Create;

      
            
        ////var  fightGroupOrderInfo=ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderByUser(pageIndex,pageSize,CurrentUser.Id,status);
        //  var resut = fightGroupOrderInfo.GetFightGroupOrderByShopid(1, 10, shopId,OrderId,producName, (OrderInfo.OrderOperateStatus?)status);
        //    return Json(new { Status = "OK", Data = resut });
        //}
           /// <summary>
        /// 获取拼团订单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public object GetFightGroupOrderDetail(long id)
        //{
        //    CheckUserLogin();
        //    IFightGroupService  iFightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;

        //    FightGroupOrderInfo model = new FightGroupOrderInfo();
   
        //   model= iFightGroupService.GetFightGroupOrderById(id );
         
        //    //获取团信息
          
        //    //获取团员信息
        //    var activeUsers=ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers((long)model.ActiveId,(long)model.GroupId);
        //   // var address = ServiceProvider.Instance<IShopBranchService>.Create.GetShopBranchById(fightGroup.ShopId);
        //    //消费地址
        //      var address = ServiceProvider.Instance<IShopBranchService>.Create.GetShopBranchById(model.Id);
        // //商品
            
        //    var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription(model.ProductId.Value);
           

        //    FightGroupsModel groupsdata = FightGroupApplication.GetGroup((long)model.ActiveId,(long)model.GroupId);
        //    if (groupsdata == null)
        //    {
        //        throw new HimallException("错误的拼团信息");
        //    }
        //    model.AddGroupTime = groupsdata.AddGroupTime;
        
         
        //    //拼装已参团成功的用户
        //    var userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers((long)model.ActiveId,(long)model.GroupId);
        //    foreach (var userItem in userList)
        //    {
        //        var userInfo = new UserInfo();
        //        userInfo.Photo = !string.IsNullOrWhiteSpace(userItem.Photo) ? Core.HimallIO.GetRomoteImagePath(userItem.Photo) : "";
        //        userInfo.UserName = userItem.UserName;
        //        userInfo.JoinTime = userItem.JoinTime;
        //        model.UserInfo.Add(userInfo);
        //    }
        //    //获取团长信息
        //    var GroupsData = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup((long)model.ActiveId,(long)model.GroupId);
        //    if (GroupsData != null)
        //    {
        //        model.HeadUserName = GroupsData.HeadUserName;
        //        model.HeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.HeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.HeadUserIcon) : "";
        //        model.ShowHeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.ShowHeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.ShowHeadUserIcon) : "";
        //    }
            
        //    var FightGroupOrder = new
        //    {
        //       FightGroupmodel=model,
        //       UserList=userList,
        //       ItemStatusText = "",
        //       GroupsData=GroupsData,
        //       Address=address
        //    };



        //    return Json(new { Status = "OK",  FightGroupDetail = FightGroupOrder });
        //}
                /// <summary>
        /// 获取拼团订单详情
        /// </summary>
        /// <param name="Id">订单Id</param>
        /// <returns></returns>
        public object GetFightGroupOrderDetail(long Id)
        {
            var userList = new List<FightGroupOrderInfo>();

            var orderDetail =FightGroupApplication.GetFightGroupOrderStatusByOrderId(Id);
            //团组活动信息
            orderDetail.UserInfo = new List<UserInfo>();
            var data = FightGroupApplication.GetActive((long)orderDetail.ActiveId, false, true);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            if (data != null)
            {
                //商品图片地址修正
                data.ProductDefaultImage = HimallIO.GetProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);
                data.ProductImgPath = HimallIO.GetRomoteImagePath(data.ProductImgPath);
            }
            FightGroupsModel groupsdata = FightGroupApplication.GetGroup(orderDetail.ActiveId.Value, orderDetail.GroupId.Value);
            if (groupsdata == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            orderDetail.AddGroupTime = groupsdata.AddGroupTime;
            //if (!string.IsNullOrWhiteSpace(result.IconUrl))
            //{
            //    result.IconUrl = Himall.Core.HimallIO.GetImagePath(result.IconUrl);
            //}
            orderDetail.ProductId = data.ProductId;
            orderDetail.ProductName = data.ProductName;
            orderDetail.IconUrl = HimallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);//result.IconUrl;
            orderDetail.MiniGroupPrice = data.MiniGroupPrice;
            TimeSpan mids = DateTime.Now - (DateTime)orderDetail.AddGroupTime;
            orderDetail.Seconds = (int)(data.LimitedHour * 3600) - (int)mids.TotalSeconds;
            orderDetail.LimitedHour = data.LimitedHour.GetValueOrDefault();
            orderDetail.LimitedNumber = data.LimitedNumber.GetValueOrDefault();
            orderDetail.JoinedNumber = groupsdata.JoinedNumber;
            orderDetail.OverTime = groupsdata.OverTime.HasValue? groupsdata.OverTime:orderDetail.AddGroupTime.AddHours((double)orderDetail.LimitedHour);
            //拼装已参团成功的用户
            userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers((long)orderDetail.ActiveId, (long)orderDetail.GroupId);
            foreach (var userItem in userList)
            {
                var userInfo = new UserInfo();
                userInfo.Photo = !string.IsNullOrWhiteSpace(userItem.Photo) ? Core.HimallIO.GetRomoteImagePath(userItem.Photo) : "";
                userInfo.UserName = userItem.UserName;
                userInfo.JoinTime = userItem.JoinTime;
                orderDetail.UserInfo.Add(userInfo);
            }
            //获取团长信息
            var GroupsData = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup((long)orderDetail.ActiveId, (long)orderDetail.GroupId);
            if (GroupsData != null)
            {
                orderDetail.HeadUserName = GroupsData.HeadUserName;
                orderDetail.HeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.HeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.HeadUserIcon) : "";
                orderDetail.ShowHeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.ShowHeadUserIcon) ? Core.HimallIO.GetRomoteImagePath(GroupsData.ShowHeadUserIcon) : "";
            }
            //商品评论数
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct((long)orderDetail.ProductId);
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            //商品描述

            //var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription((long)orderDetail.ProductId);
            //if (ProductDescription == null)
            //{
            //    throw new Himall.Core.HimallException("错误的商品编号");
            //}

            //string description = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));//商品描述

            return Json(new
            {
                OrderDetail = orderDetail,
                ComCount = comCount
              //  ProductDescription = description
            });
        }
        /// <summary>
        /// 获取商家详细订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetOrderDetail(long orderId)
        {
          //  CheckUserLogin();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
          
            OrderInfo order = orderService.GetOrder(orderId);

            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var coupon = ServiceProvider.Instance<ICouponService>.Create.GetCouponRecordInfo(order.UserId, order.Id);

            bool isCanApply = false;
            string couponName = "";
            decimal couponAmout = 0;
            if (coupon != null)
            {
                couponName = coupon.Himall_Coupon.CouponName;
                couponAmout = coupon.Himall_Coupon.Price;
            }

            //订单信息是否正常
            if (order==null)
            {
                throw new HimallException("订单号不存在！");
            }
            dynamic expressTrace =new ExpandoObject();

            //取订单物流信息
            if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
            {
                var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                if (expressData.Success)
                {
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                    expressTrace.traces = expressData.ExpressDataItems.Select(item => new
                    {
                        acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        acceptStation = item.Content
                    });
                    
                }
            }
            var orderRefunds = OrderApplication.GetOrderRefunds(order.OrderItemInfo.Select(p => p.Id));
            var isCanOrderReturn = false;
            //获取订单商品项数据
            var orderDetail = new
            {
                ShopId = order.ShopId,
                OrderItems = order.OrderItemInfo.Select(item =>
                {
                    var productinfo = productService.GetProduct(item.ProductId);
                    //是否有售后记录
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = orderRefundService.CanApplyRefund(orderId, item.Id,true);
                    }
                    else
                    {
                        isCanApply = orderRefundService.CanApplyRefund(orderId, item.Id, false);
                    }
                    ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(productinfo.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    var itemStatusText = "";
                    var itemrefund = orderRefunds.Where(or => or.OrderItemId == item.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                    itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                    if (itemrefund != null)
                    {//默认为商家处理进度
                        if (itemrefstate == 4)
                        {//商家拒绝,可以再发起申请
                            itemStatusText = "";
                        }
                        else
                        {
                            itemStatusText = "售后处理中";
                        }
                    }
                    if (itemrefstate > 4)
                    {//如果商家已经处理完，则显示平台处理进度
                        if (itemrefstate==7)
                        {
                            itemStatusText = "退款成功";
                        }
                    }

                    return new
                    {
                        Status= itemrefstate,
                        StatusText= itemStatusText,
                        Id = item.Id,
                        SkuId=item.SkuId,
                        ProductId = item.ProductId,
                        Name = item.ProductName,
                        Amount = item.Quantity,
                        Price = item.SalePrice,
                        //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                        Image = Core.HimallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                        color = item.Color,
                        size = item.Size,
                        version = item.Version,
                        IsCanRefund = isCanApply,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias,
                        SkuText= colorAlias+":"+item.Color+";"+ sizeAlias+":"+item.Size+";"+versionAlias+":"+item.Version,
                        EnabledRefundAmount = item.EnabledRefundAmount
                    };
                })
            };
            //取拼团订单状态
            var fightGroupOrderInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetFightGroupOrderStatusByOrderId(order.Id);
           
            //获取团信息
            var fightGroup = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup((long)fightGroupOrderInfo.ActiveId,(long)fightGroupOrderInfo.GroupId);
            //获取团员信息
            var activeUsers=ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers((long)fightGroupOrderInfo.ActiveId,(long)fightGroupOrderInfo.GroupId);
           // var address = ServiceProvider.Instance<IShopBranchService>.Create.GetShopBranchById(fightGroup.ShopId);
              var address = ServiceProvider.Instance<IShopBranchService>.Create.GetShopBranchById(orderDetail.ShopId);
             FightGroupOrderInfo model = new FightGroupOrderInfo();
   
           model= ServiceProvider.Instance<IFightGroupService>.Create.GetOrder(order.Id);
            var orderModel = new
            {
                OrderId = order.Id,
                Status = (int)order.OrderStatus,
                StatusText = order.OrderStatus.ToDescription(),
                OrderTotal = order.OrderTotalAmount,
               
                OrderAmount = order.ProductTotalAmount,
                DeductionPoints = 0,
                DeductionMoney = order.IntegralDiscount,
                CouponAmount = couponAmout.ToString("F2"),//优惠劵金额
                CouponName =couponName,//优惠劵名称
                RefundAmount = order.RefundTotalAmount,
                Tax = 0,
                AdjustedFreight = order.Freight,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ItemStatus = 0,
                ItemStatusText = "",
                ShipTo = order.ShipTo,
                ShipToDate = order.ShippingDate.HasValue ? order.ShippingDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                Cellphone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                FreightFreePromotionName = string.Empty,
                ReducedPromotionName = string.Empty,
                ReducedPromotionAmount = string.Empty,
                SentTimesPointPromotionName = string.Empty,
                CanBackReturn = !string.IsNullOrWhiteSpace(order.PaymentTypeGateway),
                CanCashierReturn = false,
                PaymentType = order.PaymentType.ToDescription(),
                Remark = string.IsNullOrEmpty(order.OrderRemarks) ? "" : order.OrderRemarks,
                InvoiceTitle = order.InvoiceTitle,
                ModeName = order.DeliveryType.ToDescription(),
                LogisticsData = expressTrace,
                TakeCode=order.PickupCode,
                LineItems=orderDetail.OrderItems,
                IsCanRefund = (orderDetail.OrderItems.Where(e => e.IsCanRefund == false).Count() == 0) && !orderService.IsRefundTimeOut(order.Id),
                ShopBranchAddres=address,
                FightGroupOrderInfo=model,
                FightGroup=fightGroup,
                ActiveUsers=activeUsers
            };

            return Json(new { Status = "OK", Data = orderModel });
        }



        public object GetTest()
        {
            IOrderService service = ServiceProvider.Instance<IOrderService>.Create;
            ServiceProvider.Instance<IOrderService>.Create.WritePendingSettlnment(service.GetOrder(2017101247732358));
            return Json(new { Status = "OK", Data = "" });
        }
    }
}
