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
using Himall.DTO;
using System.Data;
using AutoMapper;
using Himall.Web.Framework;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 商家
    /// </summary>
    public class UserCenterController : BaseApiController
    {
        public object GetUserCollectionProduct(int pageNo, int pageSize = 16)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IProductService>.Create.GetUserConcernProducts(CurrentUser.Id, pageNo, pageSize);
                DataTable dt = new DataTable();
                dt.Columns.Add("ActiveId");
                dt.Columns.Add("ProductId");
                dt.Columns.Add("Image");
                dt.Columns.Add("ProductName");
                dt.Columns.Add("SalePrice");
                dt.Columns.Add("GroupPrice");
                dt.Columns.Add("Description");

                IFightGroupService fightGroupService = ServiceProvider.Instance<IFightGroupService>.Create;
                IProductService productService = ServiceProvider.Instance<IProductService>.Create;
                DataRow dr;
                FightGroupActiveInfo fightGroupInfo = null;
                ProductInfo productInfo = null;
                foreach (var fightGroupModel in model.Models)
                {
                    dr = dt.NewRow();
                    fightGroupInfo = fightGroupService.GetActiveByProId(fightGroupModel.ProductId);
                    if (fightGroupInfo == null)
                    {
                        continue;
                    }
                    productInfo = productService.GetProduct(fightGroupModel.ProductId);
                    if (productInfo == null)
                    {
                        continue;
                    }
                    dr["ActiveId"] = fightGroupInfo.Id;
                    dr["ProductId"] = fightGroupModel.ProductId;
                    dr["Image"] = Core.HimallIO.GetRomoteProductSizeImage(productInfo.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220);
                    dr["ProductName"] = productInfo.ProductName;
                    dr["SalePrice"] = productInfo.MinSalePrice.ToString("F2");
                    dr["GroupPrice"] = fightGroupInfo.MiniGroupPrice;
                    dr["Description"] = productInfo.ShortDescription;
                    dt.Rows.Add(dr);
                }

                //var result = model.Models.ToArray().Select(item => new
                //{
                //    ActiveId = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(item.ProductId).Id,
                //    ProductId = item.ProductId,
                //    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ProductInfo.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
                //    ProductName = item.ProductInfo.ProductName,
                //    SalePrice = item.ProductInfo.MinSalePrice.ToString("F2"),
                //    GroupPrice = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(item.ProductId).MiniGroupPrice,
                //    Description = item.ProductInfo.ShortDescription
                //});
                return Json(new { Success = true, Data = dt, Total = dt.Rows.Count });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        public object GetUserCollectionShop(string Lng, string Lat, int pageNo, int pageSize = 8)
        {
            if (string.IsNullOrEmpty(Lng) || string.IsNullOrEmpty(Lat))
            {
                return Json(new { Success = true, Message = "缺少经纬度参数" });
            }

            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IShopService>.Create.GetUserConcernShops(CurrentUser.Id, pageNo, pageSize);

                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("Logo");
                dt.Columns.Add("Name");
                dt.Columns.Add("OpeningTime");
                dt.Columns.Add("FavoriteShopCount");
                dt.Columns.Add("ShopDescription");
                dt.Columns.Add("Distance");

                IShopService shopService = ServiceProvider.Instance<IShopService>.Create;
                DataRow dr;
                ShopInfo shopInfo = null;
                foreach (var shopModel in model.Models)
                {
                    dr = dt.NewRow();
                    shopInfo = shopService.GetShop(shopModel.ShopId);
                    dr["Id"] = shopInfo.Id;
                    dr["Logo"] = shopInfo.Logo;
                    dr["Name"] = shopInfo.ShopName;
                    dr["OpeningTime"] = shopInfo.OpeningTime;
                    dr["FavoriteShopCount"] = shopService.GetShopConcernedCount(shopInfo.Id);
                    dr["ShopDescription"] = string.IsNullOrEmpty(shopInfo.WelcomeTitle) ? "" : shopInfo.WelcomeTitle;
                    dr["Distance"] = GetLatLngDistancesFromAPI(shopInfo.Lat + "," + shopInfo.Lng, Lat + "," + Lng);
                    dt.Rows.Add(dr);
                }

                //var result = model.Models.Select(item => new
                //{
                //    Id = item.Id,
                //    Logo = item.Himall_Shops.Logo,
                //    Name = item.Himall_Shops.ShopName,
                //    OpeningTime = item.Himall_Shops.OpeningTime,
                //    FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopConcernedCount(item.Himall_Shops.Id),
                //    ShopDescription = item.Himall_Shops.ShopDescription,
                //    Distance = GetLatLngDistancesFromAPI(item.Himall_Shops.Lat + "," + item.Himall_Shops.Lng , Lat + "," + Lng)
                //});
                return Json(new { Success = true, Data = dt });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        public object GetUserInfo()
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                UserMemberInfo memberInfo = CurrentUser;
                var userInte = MemberIntegralApplication.GetMemberIntegral(memberInfo.Id);
                var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);

                if (!string.IsNullOrEmpty(memberInfo.Photo))
                {
                    if (memberInfo.Photo.StartsWith("http"))
                    {
                        memberInfo.Photo = memberInfo.Photo.Replace("http://wzp.duoyunfenxiao.com", "");
                    }
                }

                var data = new
                {
                    Id = memberInfo.Id,
                    Photo = memberInfo.Photo,
                    Nick = memberInfo.Nick,
                    GradeName = userGrade.GradeName,
                    Sex = memberInfo.Sex,
                    CellPhone = memberInfo.CellPhone,
                    Hobby = memberInfo.Hobby
                };

                return Json(new { Success = true, Data = data });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        [HttpGet]
        public object UpdateMemberInfo(string photo = "", string nick = "", int sex = 0, string hobby = "")
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                IMemberService memberService = ServiceProvider.Instance<IMemberService>.Create;
                UserMemberInfo memberInfo = CurrentUser;
                if (photo != "")
                {
                    memberInfo.Photo = photo;
                }

                if (nick != "")
                {
                    memberInfo.Nick = nick;
                }

                if (sex != 0)
                {
                    memberInfo.Sex = (SexType)sex;
                }

                if (hobby != "")
                {
                    memberInfo.Hobby = hobby;
                }

                memberService.UpdateMemberInfo(memberInfo);
                return Json(new { Success = true, Message = "修改成功" });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }


        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        public object PostUploadImage()
        {
            //CheckUserLogin();
            IList<string> images = new List<string>();
            HttpFileCollection files = HttpContext.Current.Request.Files;
            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];

                    string filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".png";
                    var fname = "/temp/" + filename;
                    var ioname = Core.HimallIO.GetImagePath(fname);
                    try
                    {
                        Core.HimallIO.CreateFile(fname, file.InputStream);
                        images.Add(ioname);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("WeChatApplet_FileUpload_Error:" + ex.Message);
                        images.Add("upload error");
                    }
                }
            }
            return Json(new
            {
                Status = "OK",
                Count = images.Count,
                Data = images.Select(c => new
                {
                    ImageUrl = c,
                })
            });
        }


        /// <summary>
        /// 获取一个起点坐标到多个终点坐标之间的距离
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="toLatLngs"></param>
        /// <returns></returns>
        private double GetLatLngDistancesFromAPI(string fromLatLng, string latlng)
        {
            if (!string.IsNullOrWhiteSpace(fromLatLng) && (!string.IsNullOrWhiteSpace(latlng)))
            {
                try
                {
                    var aryLatlng = fromLatLng.Split(',');
                    var fromlat = double.Parse(aryLatlng[0]);
                    var fromlng = double.Parse(aryLatlng[1]);
                    double EARTH_RADIUS = 6378.137;//地球半径

                    var aryToLatlng = latlng.Split(',');
                    var tolat = double.Parse(aryToLatlng[0]);
                    var tolng = double.Parse(aryToLatlng[1]);
                    var fromRadLat = fromlat * Math.PI / 180.0;
                    var toRadLat = tolat * Math.PI / 180.0;
                    double a = fromRadLat - toRadLat;
                    double b = (fromlng * Math.PI / 180.0) - (tolng * Math.PI / 180.0);
                    double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                     Math.Cos(fromRadLat) * Math.Cos(toRadLat) * Math.Pow(Math.Sin(b / 2), 2)));
                    s = s * EARTH_RADIUS;
                    return Math.Round((Math.Round(s * 10000) / 10000), 2);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("计算经纬度距离异常", ex);
                    return 0;
                }
            }
            return 0;
        }


        public object GetTodayCommission()
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                return Json(new { Success = true, Data = ServiceProvider.Instance<IMemberService>.Create.GetTodayCommission(CurrentUser.Id) });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        public object GetTotalCommission(long userId)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                return Json(new { Success = true, Data = ServiceProvider.Instance<IMemberService>.Create.GetTotalCommission(CurrentUser.Id) });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        public object GetCommissionList(long userId)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                return Json(new { Success = true, Data = ServiceProvider.Instance<IMemberService>.Create.GetCommissionList(CurrentUser.Id) });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        /// <summary>
        /// 获取用户奖励信息
        /// </summary>
        /// <returns></returns>
        public object GetCapitalInfo()
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                CapitalDetailQuery query = new CapitalDetailQuery();
                query.memberId = CurrentUser.Id;
                query.capitalType = null;

                List<CapitalDetailInfo> list = ServiceProvider.Instance<IMemberCapitalService>.Create.GetCapitalDetailList(query);
                var data = new {
                    CanWithDraw = list.Sum(a => a.Amount),//可提现
                    Rewards = list.Where(a => a.SourceType == CapitalDetailInfo.CapitalDetailType.OpenGroupReward || a.SourceType == CapitalDetailInfo.CapitalDetailType.InvitationReward || a.SourceType == CapitalDetailInfo.CapitalDetailType.ReturnMoney).Sum(a => a.Amount),//累计奖励
                    HasSettlement = list.Where(a => a.SourceType == CapitalDetailInfo.CapitalDetailType.WithDraw).Sum(a => a.Amount)//已结算
                };

                return Json(new { Success = true, Data = data });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        /// <summary>
        /// 获取用户奖励列表
        /// </summary>
        /// <param name="cType"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetCapitalDetails(int cType = 0, int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                IMemberCapitalService _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;
                CapitalDetailQuery query = new CapitalDetailQuery();
                query.memberId = CurrentUser.Id;
                query.capitalType = (CapitalDetailInfo.CapitalDetailType)cType;
                query.PageNo = pageNo;
                query.PageSize = pageSize;

                ObsoletePageModel<CapitalDetailInfo> list = ServiceProvider.Instance<IMemberCapitalService>.Create.GetCapitalDetails(query);
                List<CapitalDetailInfo> data = new List<CapitalDetailInfo>();
                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("CapitalID");
                dt.Columns.Add("Amount");
                dt.Columns.Add("CreateTime");
                dt.Columns.Add("Remark");
                dt.Columns.Add("SourceData");
                dt.Columns.Add("SourceType");
                dt.Columns.Add("ShopName");
                dt.Columns.Add("Status");
                dt.Columns.Add("UserLogo");
                foreach (CapitalDetailInfo detail in list.Models)
                {
                    DataRow dr = dt.NewRow();
                    dr["Id"] = detail.Id;
                    dr["CapitalID"] = detail.CapitalID;
                    dr["Amount"] = detail.Amount;
                    dr["CreateTime"] = detail.CreateTime;
                    dr["Remark"] = detail.Remark;
                    dr["SourceData"] = detail.SourceData;
                    dr["SourceType"] = detail.SourceType;
                    dr["ShopName"] = "";
                    dr["Status"] = "";
                    dr["UserLogo"] = CurrentUser.Photo;

                    if (detail.SourceType == CapitalDetailInfo.CapitalDetailType.WithDraw)
                    {
                        dr["Status"] = _iMemberCapitalService.GetWithDraw(Convert.ToInt64(detail.SourceData)).ApplyStatus.ToDescription();
                    }
                    else
                    {
                        var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(Convert.ToInt64(detail.SourceData));
                        if (order != null)
                        {
                            dr["ShopName"] = order.ShopName;
                        }
                    }


                    dt.Rows.Add(dr);
                }

                return Json(new { Success = true, Data = dt, Total = list.Total });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        /// <summary>
        /// 获取用户奖励明细
        /// </summary>
        /// <param name="detailId"></param>
        /// <returns></returns>
        public object GetCapitalDetailInfo(long id)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                IMemberCapitalService _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;
                CapitalDetailInfo detail = _iMemberCapitalService.GetCapitalDetailInfo(id);
                if (detail != null)
                {
                    decimal amount = 0; // 金额，提现、奖励、消费均有
                    var shopName = "";//商家名称，奖励、消费均有
                    var productName = "";//商品名称，奖励有
                    var typeName = "";//类别名称，提现、奖励、消费均有
                    var orderNum = "";//单据编号，提现、奖励、消费均有
                    var orderTime = "";//单据时间，提现、奖励、消费均有
                    var status = "";//状态，提现有

                    amount = detail.Amount;
                    typeName = detail.SourceType.ToDescription();
                    orderNum = detail.SourceData;
                    orderTime = ((DateTime)detail.CreateTime).ToString("yyyy/MM/dd HH:mm");

                    if (detail.SourceType == CapitalDetailInfo.CapitalDetailType.WithDraw)
                    {
                        status = _iMemberCapitalService.GetWithDraw(Convert.ToInt64(detail.SourceData)).ApplyStatus.ToDescription();
                        var data = new
                        {
                            Amount = amount,
                            ShopName = shopName,
                            ProductName = productName,
                            TypeName = typeName,
                            OrderNum = orderNum,
                            OrderTime = orderTime,
                            Status = status
                        };

                        return Json(new { Success = true, Data = data });
                    }
                    else
                    {
                        var order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(Convert.ToInt64(detail.SourceData));
                        if (order != null)
                        {
                            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(order.OrderItemInfo.FirstOrDefault().ProductId);

                            var data = new
                            {
                                Amount = amount,
                                ShopName = order.ShopName,
                                ProductName = product.ProductName,
                                TypeName = typeName,
                                OrderNum = orderNum,
                                OrderTime = orderTime,
                                Status = status
                            };

                            return Json(new { Success = true, Data = data });
                        }
                        else
                        {
                            return Json(new { Success = false, Message = "订单数据异常" });
                        }
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = "未查到相关数据" });
                }
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost]
        public object ApplyWithDraw(ApplayWithDraw applay)
        {
            var userInfo = Application.MemberApplication.GetMemberByOpenId(SmallProgServiceProvider, applay.openid);
            if (userInfo != null)
            {
                var _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;

                var capitalInfo = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
                if (applay.amount > capitalInfo.Balance)
                {
                    return Json(new { Success = false, Message = "提现金额不能超出可用金额" });
                }
                if (applay.amount <= 0)
                {
                    return Json(new { Success = false, Message = "提现金额不能小于等于0" });
                }
                ApplyWithDrawInfo model = new ApplyWithDrawInfo()
                {
                    ApplyAmount = applay.amount,
                    ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                    ApplyTime = DateTime.Now,
                    MemId = CurrentUser.Id,
                    OpenId = applay.openid,
                    NickName = CurrentUser.Nick
                };
                _iMemberCapitalService.AddWithDrawApply(model);
                return Json(new { success = true, Message = "申请提现成功" });
            }
            else
            {
                return Json(new { Success = false, Message = "未登录" });
            }
        }

        #region zzz
        /// <summary>
        /// 新增团购活动
        /// </summary>
        /// <returns></returns>
        public object AddActive(FightGroupActiveModel model)
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
            //model.CheckValidation();

            /*var skudata = FightGroupApplication.GetNewActiveItems(model.ProductId.Value).skulist;
            foreach (var item in model.ActiveItems)
            {
                var cursku = skudata.FirstOrDefault(d => d.SkuId == item.SkuId);
                if (cursku != null)
                {
                    if (item.ActiveStock > cursku.ProductStock)
                    {
                        throw new HimallException(item.SkuId + "错误的活动库存");
                    }
                }
                else
                {
                    model.ActiveItems.Remove(item);
                }
            }*/

            try
            {
                //model.ShopId = CurrentUserId;
                //model.ShopId = 1;
                model.LimitQuantity = 1;
                model.Seconds = 86400;
                model.AddTime = DateTime.Now;
                model.IconUrl = model.ProductDefaultImage;
                FightGroupApplication.AddActive(model);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            catch (Exception ex)
            {
                result = new Result { success = false, msg = ex.Message, status = -1 };
            }
            return Json(result);
        }

        public object EditActive(FightGroupActiveModel model)
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
            try
            {
                //数据有效性验证
                //model.CheckValidation();
                data.ProductName = model.ProductName;
                data.ProductShortDescription = model.ProductShortDescription;
                data.ProductDefaultImage = model.ProductDefaultImage;
                data.ProductImages = model.ProductImages;
                data.ShowMobileDescription = model.ShowMobileDescription;
                data.ActiveItems = model.ActiveItems;
                data.OpenGroupReward = model.OpenGroupReward;
                data.InvitationReward = model.InvitationReward;
                data.LimitQuantity = model.LimitQuantity;
                data.LimitedNumber = model.LimitedNumber;
                data.MiniSalePrice = model.MiniSalePrice;
                data.StartTime = model.StartTime;
                data.EndTime = model.EndTime;
                data.IsCombo = model.IsCombo;
                data.Seconds = model.Seconds;
                data.LimitedHour = model.LimitedHour;
                data.GroupNotice = model.GroupNotice;
                data.IconUrl = model.ProductDefaultImage;
                //订单有效日期，使用时间是什么鬼
                data.OrderDate = model.OrderDate;
                data.UseDate = model.UseDate;
                data.PayCode = model.PayCode;
                data.ReturnMoney = model.ReturnMoney;
                FightGroupApplication.UpdateActive(data);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            catch (Exception ex)
            {
                result = new Result { success = false, msg = ex.Message, status = -1 };
            }
            return Json(result);
        }

        public object GetActiveById(int id)
        {
            try
            {
                //CheckUserLogin();
                FightGroupActiveModel data = FightGroupApplication.XcxGetActive(id, false);
                if (data == null)
                {
                    throw new HimallException("错误的活动信息");
                }
                //data.InitProductImages();
                FightGroupActiveImgInfo imgInfo = FightGroupApplication.XcxGetActiveImg(id);
                AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveDetailModel>();

                FightGroupActiveDetailModel model = AutoMapper.Mapper.Map<FightGroupActiveDetailModel>(data);
                model.ShareTitle = data.ActiveStatus == FightGroupActiveStatus.WillStart ? "限时限量火拼 即将开始" : "限时限量火拼 正在进行";

                if (imgInfo != null)
                {
                    model.ProductDefaultImage = imgInfo.ProductDefaultImage;
                    model.ProductImages = imgInfo.ProductImages.Split(',').ToList();
                    model.ShowMobileDescription = imgInfo.ShowMobileDescription;
                    model.ShareImage = imgInfo.ProductDefaultImage;
                    model.IconUrl = imgInfo.ProductDefaultImage;
                }
                model.GroupNotice = data.GroupNotice;
                model.ShareDesc = data.ProductName;
                model.ReturnMoney = data.ReturnMoney;
                /*if (!string.IsNullOrWhiteSpace(model.ShareImage))
                {
                    if (model.ShareImage.Substring(0, 4) != "http")
                    {
                        model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage);
                    }
                }*/

                /*if (model.IconUrl.Substring(0, 4) != "http")
                {
                    model.IconUrl = HimallIO.GetRomoteImagePath(model.IconUrl);
                }
                if (model.ProductImgPath.Substring(0, 4) != "http")
                {
                    model.ProductImgPath = HimallIO.GetRomoteImagePath(model.ProductImgPath);
                }
                if (model.ProductDefaultImage.Substring(0, 4) != "http")
                {
                    model.ProductDefaultImage = HimallIO.GetRomoteImagePath(model.ProductDefaultImage);
                }
                for (int i = 0; i < model.ProductImages.Count; i++)
                {
                    model.ProductImages[i] = HimallIO.GetRomoteImagePath(model.ProductImages[i]);
                }*/


                /*if (!string.IsNullOrWhiteSpace(data.ProductShortDescription))
                {
                    model.ShareDesc += "，(" + data.ProductShortDescription + ")";
                }
                var ProductDescription = ServiceHelper.Create<IProductService>().GetProductDescription(model.ProductId.Value);
                if (ProductDescription == null)
                {
                    throw new Himall.Core.HimallException("错误的商品编号");
                }
                model.ShowMobileDescription = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/",
                    "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
                model.GroupNotice = "1、活动结束后将从拼单成功的所有订单中，随机选取1名使用者赠送。\n2、一等奖为iPhoneX,二等奖为88元优惠券。";//TODO 功能待实现data.GroupNotice;*/
                /*if (model.ProductId.HasValue)
                {
                    //统计商品浏览量、店铺浏览人数
                    StatisticApplication.StatisticVisitCount(model.ProductId.Value, model.ShopId);
                }
                IProductService _iProductService = ServiceProvider.Instance<IProductService>.Create;
                bool isSub = _iProductService.IsFavorite((long)model.ProductId, CurrentUserId);
                model.IsSubscribe = isSub ? 1 : 0;*/
                //model.IsVirtualProduct = 1;
                List<FightGroupActiveItemInfo> sourceList = FightGroupApplication.GetActiveItemsInfos(model.Id);
                List<FightGroupActiveItemModel> targetList = new List<FightGroupActiveItemModel>();
                foreach (var itemInfo in sourceList)
                {
                    FightGroupActiveItemModel m = new FightGroupActiveItemModel();
                    m.Id = itemInfo.Id;
                    m.ActiveId = itemInfo.ActiveId.Value;
                    m.ActivePrice = itemInfo.ActivePrice;
                    m.ActiveStock = itemInfo.ActiveStock;
                    m.BuyCount = itemInfo.BuyCount;
                    m.Color = itemInfo.Color;
                    m.ProductCostPrice = itemInfo.ProductCostPrice;
                    m.ProductId = itemInfo.ProductId.Value;
                    m.ProductPrice = itemInfo.ProductPrice;
                    m.ProductStock = itemInfo.ProductStock;
                    m.ShowPic = itemInfo.ShowPic;
                    m.Size = itemInfo.Size;
                    m.SkuName = itemInfo.SkuName;
                    m.SkuId = itemInfo.SkuId;
                    m.Version = itemInfo.Version;
                    targetList.Add(m);
                }
                model.ActiveItems = targetList;
                return Json(new { Status = "OK", FightGroupDetail = model });
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Msg = ex.Message });
            }

        }

        public object GetActiveList(int pageIndex, int auditStatus, long shopId, string productName)
        {//-1,0,1表示售完，正常，下架
            int pagesize = 5;
            List<FightGroupActiveStatus> seastatus = new List<FightGroupActiveStatus>();
            seastatus.Add(FightGroupActiveStatus.Ongoing);
            seastatus.Add(FightGroupActiveStatus.WillStart);

            var data = ServiceProvider.Instance<IFightGroupService>.Create.GetActives(null, null, null, productName, null, shopId, pageIndex, pagesize, auditStatus);

            var datalist = data.Models.ToList();
            foreach (FightGroupActiveInfo item in datalist)
            {
                /*if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    item.IconUrl = Core.HimallIO.GetRomoteImagePath(item.IconUrl);*/
                FightGroupActiveImgInfo imgInfo = FightGroupApplication.XcxGetActiveImg(item.Id);
                if (imgInfo != null)
                {
                    item.ProductDefaultImage = imgInfo.ProductDefaultImage;
                    item.ProductImages = imgInfo.ProductImages.Split(',').ToList();
                    item.ShowMobileDescription = imgInfo.ShowMobileDescription;
                    item.IconUrl = imgInfo.ProductDefaultImage;
                }

                item.ActiveItems = FightGroupApplication.GetActiveItemsInfos(item.Id);
            }
            return Json(new { Total = data.Total, Data = datalist });
        }
        [HttpGet]
        public object CancelActive(int id, string mremark = "小程序下架操作")
        {
            Result result;
            try
            {
                FightGroupApplication.XcxCancelActive(id);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            catch (Exception)
            {
                result = new Result { success = false, msg = "操作失败", status = -1 };
            }
            return Json(result);
        }

        [HttpGet]
        public object DeleteActive(int id)
        {
            Result result;
            try
            {
                FightGroupApplication.DeleteActive(id);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            catch (Exception)
            {
                result = new Result { success = false, msg = "操作失败", status = -1 };
            }
            return Json(result);
        }
        #endregion



    }

    public class ApplayWithDraw
    {
        public string openid { get; set; }

        public decimal amount { get; set; }

    }

}
