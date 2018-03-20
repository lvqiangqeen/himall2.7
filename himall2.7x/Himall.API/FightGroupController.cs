using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Http;
using System.Net;

using Himall.CommonModel;
using Himall.DTO;
using AutoMapper;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.API.Model;
using Himall.Application;


namespace Himall.API
{
    public class FightGroupController : BaseApiController
    {
        /// <summary>
        /// 拼团活动列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public object GetActiveList(int page)
        {
            int pagesize = 5;
            List<FightGroupActiveStatus> seastatus = new List<FightGroupActiveStatus>();
            seastatus.Add(FightGroupActiveStatus.Ongoing);
            seastatus.Add(FightGroupActiveStatus.WillStart);
            var data = ServiceProvider.Instance<IFightGroupService>.Create.GetActives(seastatus, null, null, null, null, null, page, pagesize);

            var datalist = data.Models.ToList();
            foreach (FightGroupActiveInfo item in datalist)
            {
                if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    item.IconUrl = Core.HimallIO.GetRomoteImagePath(item.IconUrl);
            }
            return Json(new { Total = data.Total, Data = datalist });
        }
        public bool IsExist(string uri)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "HEAD";
                req.Timeout = 3000;
                res = (HttpWebResponse)req.GetResponse();
                return (res.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                    res = null;
                }
                if (req != null)
                {
                    req.Abort();
                    req = null;
                }
            }
        }
        /// <summary>
        /// 拼团活动商品详情
        /// </summary>
        /// <param name="id">拼团活动ID</param>
        /// /// <param name="grouId">团活动ID</param>
        /// <returns></returns>
        public object GetActiveDetail(long id, long grouId = 0, bool isFirst = true, string ids = "")
        {
            var userList = new List<FightGroupOrderInfo>();
            var data = ServiceProvider.Instance<IFightGroupService>.Create.GetActive(id, true, true);

            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();


            FightGroupActiveModel result = Mapper.Map<FightGroupActiveInfo, FightGroupActiveModel>(data);

            if (result != null)
            {
                result.IsEnd = true;
                if (data.EndTime.Value.Date >= DateTime.Now.Date)
                {
                    result.IsEnd = false;
                }
                //商品图片地址修正
                result.ProductDefaultImage = HimallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);
                result.ProductImgPath = HimallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1);
            }
            //result.InitProductImages();
            if (!string.IsNullOrWhiteSpace(result.ProductDefaultImage))
            {
                //补充图片地址
                for (var n = 2; n < 6; n++)
                {
                    var _imgurl = HimallIO.GetProductSizeImage(result.ProductDefaultImage, n, (int)ImageSize.Size_350);
                    if (this.IsExist(_imgurl))
                    {
                        result.ProductImages.Add(_imgurl);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(result.IconUrl))
            {
                result.IconUrl = Himall.Core.HimallIO.GetRomoteImagePath(result.IconUrl);
            }
            bool IsUserEnter = false;
            long currentUser = 0;
            if (CurrentUser != null)
                currentUser = CurrentUser.Id;
            if (grouId > 0)//获取已参团的用户
            {
                userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers(id, grouId);
                foreach (var item in userList)
                {
                    item.Photo = !string.IsNullOrWhiteSpace(item.Photo) ? Core.HimallIO.GetRomoteImagePath(item.Photo) : "";
                    if (currentUser.Equals(item.OrderUserId))
                        IsUserEnter = true;
                }
            }
            #region 商品规格
            ProductInfo product = ServiceProvider.Instance<IProductService>.Create.GetProduct((long)result.ProductId);

            //if (product == null)
            //{
            //    throw new Himall.Core.HimallException("产品编号错误");
            //}

            //if (product.IsDeleted)
            //{
            //    throw new Himall.Core.HimallException("产品编号错误");
            //}


            ProductShowSkuInfoModel model = new ProductShowSkuInfoModel();
            model.MinSalePrice = data.MiniSalePrice;
            model.ProductImagePath = string.IsNullOrWhiteSpace(data.ProductImgPath) ? "" : HimallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)Himall.CommonModel.ImageSize.Size_350);

            List<SKUDataModel> skudata = data.ActiveItems.Where(d => d.ActiveStock > 0).Select(d => new SKUDataModel
            {
                SkuId = d.SkuId,
                Color = d.Color,
                Size = d.Size,
                Version = d.Version,
                Stock = (int)d.ActiveStock,
                CostPrice = d.ProductCostPrice,
                SalePrice = d.ProductPrice,
                Price = d.ActivePrice,
            }).ToList();

            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;

            if (result.ActiveItems != null && result.ActiveItems.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                var skus = ServiceProvider.Instance<IProductService>.Create.GetSKUs((long)result.ProductId);
                foreach (var sku in result.ActiveItems)
                {
                    var specs = sku.SkuId.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = result.ActiveItems.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.ActiveStock);
                                model.Color.Add(new ProductSKU
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
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = result.ActiveItems.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.ActiveStock);
                                model.Size.Add(new ProductSKU
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
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = result.ActiveItems.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.ActiveStock);
                                model.Version.Add(new ProductSKU
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

            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation((long)result.ProductId);//提供服务（消费者保障、七天无理由、及时发货）

            var GroupsData = new List<FightGroupsListModel>();
            List<FightGroupBuildStatus> stlist = new List<FightGroupBuildStatus>();
            stlist.Add(FightGroupBuildStatus.Ongoing);
            GroupsData = FightGroupApplication.GetGroups(id, stlist, null, null, 1, 10).Models.ToList();
            foreach (var item in GroupsData)
            {
                TimeSpan mid = item.AddGroupTime.AddHours((double)item.LimitedHour) - DateTime.Now;
                item.Seconds = (int)mid.TotalSeconds;
                item.EndHourOrMinute = item.ShowHourOrMinute(item.GetEndHour);
            }

            #region 商品评论
            ProductCommentShowModel modelSay = new ProductCommentShowModel();
            modelSay.ProductId = (long)result.ProductId;
            var productSay = ServiceProvider.Instance<IProductService>.Create.GetProduct((long)result.ProductId);
            modelSay.CommentList = new List<ProductDetailCommentModel>();
            modelSay.IsShowColumnTitle = true;
            modelSay.IsShowCommentList = true;

            if (productSay == null)
            {
                //跳转到404页面
                throw new Core.HimallException("商品不存在");
            }
            if (product.IsDeleted)
            {
                //跳转到404页面
                throw new Core.HimallException("商品不存在");
            }
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            modelSay.CommentCount = comCount;
            if (comCount > 0)
            {
                modelSay.CommentList = com.OrderByDescending(a => a.ReviewDate).Take(1).Select(c => new ProductDetailCommentModel
                {
                    Sku = ServiceProvider.Instance<IProductService>.Create.GetSkuString(c.Himall_OrderItems.SkuId),
                    UserName = c.UserName,
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate,
                    Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                    AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                    ReviewDate = c.ReviewDate,
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate

                }).ToList();
                foreach (var citem in modelSay.CommentList)
                {
                    if (citem.Images.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.Images.Count; _imgn++)
                        {
                            citem.Images[_imgn] = Himall.Core.HimallIO.GetImagePath(citem.Images[_imgn]);
                        }
                    }
                    if (citem.AppendImages.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.AppendImages.Count; _imgn++)
                        {
                            citem.AppendImages[_imgn] = Himall.Core.HimallIO.GetImagePath(citem.AppendImages[_imgn]);
                        }
                    }
                }
            }
            #endregion

            #region 店铺信息
            VShopShowShopScoreModel modelShopScore = new VShopShowShopScoreModel();
            modelShopScore.ShopId = result.ShopId;
            var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(result.ShopId);
            if (shop == null)
            {
                throw new HimallException("错误的店铺信息");
            }

            modelShopScore.ShopName = shop.ShopName;

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(result.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;

            modelShopScore.SellerServiceAttitude = defaultValue;
            modelShopScore.SellerServiceAttitudePeer = defaultValue;
            modelShopScore.SellerServiceAttitudeMax = defaultValue;
            modelShopScore.SellerServiceAttitudeMin = defaultValue;

            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null && !shop.IsSelf)
            {
                modelShopScore.ProductAndDescription = productAndDescription.CommentValue;
                modelShopScore.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                modelShopScore.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                modelShopScore.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                modelShopScore.ProductAndDescription = defaultValue;
                modelShopScore.ProductAndDescriptionPeer = defaultValue;
                modelShopScore.ProductAndDescriptionMin = defaultValue;
                modelShopScore.ProductAndDescriptionMax = defaultValue;
            }

            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null && !shop.IsSelf)
            {
                modelShopScore.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                modelShopScore.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                modelShopScore.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                modelShopScore.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                modelShopScore.SellerServiceAttitude = defaultValue;
                modelShopScore.SellerServiceAttitudePeer = defaultValue;
                modelShopScore.SellerServiceAttitudeMax = defaultValue;
                modelShopScore.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null && !shop.IsSelf)
            {
                modelShopScore.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                modelShopScore.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                modelShopScore.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                modelShopScore.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                modelShopScore.SellerDeliverySpeed = defaultValue;
                modelShopScore.SellerDeliverySpeedPeer = defaultValue;
                modelShopScore.SellerDeliverySpeedMax = defaultValue;
                modelShopScore.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            modelShopScore.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(result.ShopId);
            modelShopScore.IsFavoriteShop = false;
            modelShopScore.FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopFavoritesCount(result.ShopId);
            if (CurrentUser != null)
            {
                modelShopScore.IsFavoriteShop = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == result.ShopId);
            }

            long vShopId;
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
            }
            else
            {
                vShopId = vshopinfo.Id;
            }
            modelShopScore.VShopId = vShopId;
            modelShopScore.VShopLog = ServiceProvider.Instance<IVShopService>.Create.GetVShopLog(vShopId);

            if (!string.IsNullOrWhiteSpace(modelShopScore.VShopLog))
            {
                modelShopScore.VShopLog = Himall.Core.HimallIO.GetRomoteImagePath(modelShopScore.VShopLog);
            }

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerService(shop.Id);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(shop.Id).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
            #endregion
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceHelper.Create<IFreightTemplateService>();
            FreightTemplateInfo template = freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            string productAddress = string.Empty;
            if (template != null && template.SourceAddress.HasValue)
            {
                var fullName = ServiceHelper.Create<IRegionService>().GetFullName(template.SourceAddress.Value);
                if (fullName != null)
                {
                    var ass = fullName.Split(' ');
                    if (ass.Length >= 2)
                    {
                        productAddress = ass[0] + " " + ass[1];
                    }
                    else
                    {
                        productAddress = ass[0];
                    }
                }
            }

            var ProductAddress = productAddress;
            var FreightTemplate = template;
            #endregion

            #region 获取店铺优惠信息
            VShopShowPromotionModel modelVshop = new VShopShowPromotionModel();
            modelVshop.ShopId = result.ShopId;
            var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(result.ShopId);
            if (shopInfo == null)
            {
                throw new HimallException("错误的店铺编号");
            }

            modelVshop.FreeFreight = shop.FreeFreight;


            var bonus = ServiceHelper.Create<IShopBonusService>().GetByShopId(result.ShopId);
            if (bonus != null)
            {
                modelVshop.BonusCount = bonus.Count;
                modelVshop.BonusGrantPrice = bonus.GrantPrice;
                modelVshop.BonusRandomAmountStart = bonus.RandomAmountStart;
                modelVshop.BonusRandomAmountEnd = bonus.RandomAmountEnd;
            }
            #endregion
            //商品描述
            var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription((long)result.ProductId);
            if (ProductDescription == null)
            {
                throw new Himall.Core.HimallException("错误的商品编号");
            }
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveResult>();
            var fightGroupData = AutoMapper.Mapper.Map<FightGroupActiveResult>(result);
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopItem = ShopApplication.GetShop(result.ShopId);
            fightGroupData.MiniSalePrice = shopItem.IsSelf ? fightGroupData.MiniSalePrice * discount : fightGroupData.MiniSalePrice;

            return Json(new
            {
                FightGroupData = fightGroupData,
                ShowSkuInfo = model,
                ShowPromotion = modelVshop,
                ShowNewCanJoinGroup = GroupsData,
                ProductCommentShow = modelSay,
                ProductDescription = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/")),
                ShopScore = modelShopScore,
                CashDepositsServer = cashDepositModel,
                ProductAddress = ProductAddress,
                Free = FreightTemplate.IsFree == FreightTemplateType.Free ? "免运费" : "",
                userList = userList,
                IsUserEnter = IsUserEnter,
                SkuData = skudata,
                CustomerServices = customerServices
            });
        }
        public object GetActiveDetailSec(string ids = "")
        {
            ////获取能参加的拼团活动
            List<FightGroupBuildStatus> status = new List<FightGroupBuildStatus>();
            status.Add(FightGroupBuildStatus.Ongoing);
            var GroupsData = new List<FightGroupsListModel>();

            string[] activeIds = ids.Split(',');
            long[] unActiveId = new long[activeIds.Length];
            for (int i = 0; i < activeIds.Length; i++)
            {
                unActiveId[i] = Int64.Parse(activeIds[i]);
            }
            GroupsData = FightGroupApplication.GetCanJoinGroupsSecond(unActiveId, status);

            foreach (var item in GroupsData)
            {
                TimeSpan mid = item.AddGroupTime.AddHours((double)item.LimitedHour) - DateTime.Now;
                item.Seconds = (int)mid.TotalSeconds;
            }
            return Json(new
            {
                ShowNewCanJoinGroup = GroupsData
            });
        }
        /// <summary>
        /// 获取拼团团组详情
        /// </summary>
        /// <param name="activeId">拼团活动ID</param>
        /// <param name="groupId">拼团团组ID</param>
        /// <returns></returns>
        public object GetGroupDetail(long activeId, long groupId)
        {
            var data = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup(activeId, groupId);
            if (data == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            return Json(data);

        }
        /// <summary>
        /// 根据用户ID获取拼团订单列表
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public object GetFightGroupOrderByUser(int page)
        {
            CheckUserLogin();
            int pagesize = 5;
            long UserId = CurrentUserId;
            var userList = new List<FightGroupOrderInfo>();
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            var data = FightGroupApplication.GetJoinGroups(UserId, seastatus, page, pagesize);
            var datalist = data.Models.ToList();

            List<FightGroupGetFightGroupOrderByUserModel> resultlist = new List<FightGroupGetFightGroupOrderByUserModel>();
            foreach (var item in datalist)
            {
                FightGroupGetFightGroupOrderByUserModel _tmp = new FightGroupGetFightGroupOrderByUserModel();
                _tmp.Id = item.Id;
                _tmp.ActiveId = item.ActiveId;
                _tmp.ProductName = item.ProductName;
                _tmp.ProductImgPath = item.ProductImgPath;
                _tmp.ProductDefaultImage = HimallIO.GetProductSizeImage(_tmp.ProductImgPath, 1, (int)ImageSize.Size_350);
                _tmp.GroupEndTime = item.OverTime.HasValue ? item.OverTime.Value : item.GroupEndTime;
                _tmp.BuildStatus = item.BuildStatus;
                _tmp.NeedNumber = item.LimitedNumber - item.JoinedNumber;
                _tmp.UserIcons = new List<string>();
                foreach (var sitem in item.GroupOrders)
                {
                    _tmp.UserIcons.Add(sitem.Photo);
                    if (sitem.OrderUserId == UserId)
                    {
                        _tmp.OrderId = sitem.OrderId;
                        _tmp.GroupPrice = sitem.SalePrice;
                    }
                }
                resultlist.Add(_tmp);
            }
            return Json(new
            {
                orderData = resultlist
            });
        }
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

            var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription((long)orderDetail.ProductId);
            if (ProductDescription == null)
            {
                throw new Himall.Core.HimallException("错误的商品编号");
            }

            string description = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));//商品描述

            return Json(new
            {
                OrderDetail = orderDetail,
                ComCount = comCount,
                ProductDescription = description
            });
        }
    }
}
