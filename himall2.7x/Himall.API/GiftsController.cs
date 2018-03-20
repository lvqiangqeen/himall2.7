using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Application;
using Himall.Model;
using Himall.Web.Framework;
using Himall.API.Model;
using Himall.CommonModel;

namespace Himall.API
{
    public class GiftsController : BaseApiController
    {
        private IGiftService _iGiftService;
        private IGiftsOrderService _iGiftsOrderService;
        private IMemberService _iMemberService;
        private IMemberGradeService _iMemberGradeService;
        public GiftsController()
        {
            _iGiftService = ServiceHelper.Create<IGiftService>();
            _iGiftsOrderService = ServiceHelper.Create<IGiftsOrderService>();
            _iMemberService = ServiceHelper.Create<IMemberService>();
            _iMemberGradeService = ServiceHelper.Create<IMemberGradeService>();
        }

        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        public List<SlideAdInfo> GetSlideAds()
        {
            ISlideAdsService _ISlideAdsService = ServiceHelper.Create<ISlideAdsService>();
            var sql = _ISlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.AppGifts);
            var result = sql.ToList();
            foreach (var item in result)
            {
                item.ImageUrl = HimallIO.GetRomoteImagePath(item.ImageUrl);
            }
            return result;
        }

        public List<IntegralMallAdInfo> GetIntegralMallAd()
        {
            List<IntegralMallAdInfo> result = new List<IntegralMallAdInfo>();
            var robj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.Roulette, IntegralMallAdInfo.AdShowPlatform.APP);
            if (robj != null)
            {
                robj.LinkUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/m-app/BigWheel/index/" + robj.ActivityId;
                result.Add(robj);
            }
            var cobj = _iGiftService.GetAdInfo(IntegralMallAdInfo.AdActivityType.ScratchCard, IntegralMallAdInfo.AdShowPlatform.APP);
            if (cobj != null)
            {
                cobj.LinkUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/m-app/ScratchCard/index/" + cobj.ActivityId;
                result.Add(cobj);
            }
            return result;
        }

        #region 礼品信息
        /// <summary>
        /// 获取礼品列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public GiftsListModel GetList(int page = 1, int pagesize = 10)
        {
            GiftsListModel result = new GiftsListModel();
            //礼品数据
            result.PageSize = pagesize;
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = result.PageSize;
            query.PageNo = page;
            ObsoletePageModel<GiftModel> gifts = _iGiftService.GetGifts(query);
            result.DataList = gifts.Models.ToList();
            foreach (var item in result.DataList)
            {
                item.DefaultShowImage = HimallIO.GetRomoteImagePath(item.GetImage(ImageSize.Size_100));
            }
            result.Total = gifts.Total;
            result.MaxPage = GetMaxPage(result.Total, result.PageSize);

            return result;
        }
        /// <summary>
        /// 礼品详情
        /// </summary>
        /// <returns></returns>
        public GiftsDetailModel GetGifts(long id)
        {
            GiftsDetailModel result = new GiftsDetailModel();
            var data = _iGiftService.GetById(id);
            Mapper.CreateMap<GiftInfo, GiftsDetailModel>();
            result = Mapper.Map<GiftsDetailModel>(data);
            if (result == null)
            {
                throw new Exception("礼品信息无效！");
            }
            string tmpdefaultimg = result.GetImage(ImageSize.Size_100);
            result.DefaultShowImage = HimallIO.GetRomoteImagePath(tmpdefaultimg);
            result.Images = new List<string>();
            result.Description = result.Description.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));

            //补充图片信息
            for (var _n = 1; _n < 6; _n++)
            {
                string _imgpath = data.ImagePath + "/" + _n.ToString() + ".png";
                if (HimallIO.ExistFile(_imgpath))
                {
                    var tmp = HimallIO.GetRomoteImagePath(result.GetImage(ImageSize.Size_500, _n));
                    result.Images.Add(tmp);
                }
            }

            #region 礼品是否可兑
            result.CanBuy = true;
            //礼品信息
            if (result.CanBuy)
            {
                if (result.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品" + result.ShowSalesStatus;
                }
            }

            if (result.CanBuy)
            {
                //库存判断
                if (result.StockQuantity < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品已兑完";
                }
            }

            if (result.CanBuy)
            {
                //积分数
                if (result.NeedIntegral < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品信息错误";
                }
            }
            #endregion

            #region 用户信息判断

            if (result.CanBuy && CurrentUser != null)
            {
                //限购数量
                if (result.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber >= result.LimtQuantity)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "兑换次数已满";
                    }
                }
                if (result.CanBuy)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < result.NeedIntegral)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "积分不足";
                    }
                }
            }
            #endregion

            return result;
        }
        /// <summary>
        /// 下单前判断
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Result GetCanBuy(long id, int count)
        {
            Result result = new Result();
            bool isdataok = true;

            if (CurrentUser == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "您还未登录！";
                result.status = -1;
                return result;
            }
            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);

            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (isdataok)
            {
                if (giftdata == null)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品不存在！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                if (giftdata.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品已失效！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                //库存判断
                if (count > giftdata.StockQuantity)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！";
                    result.status = -3;
                }
            }

            if (isdataok)
            {
                //积分数
                if (giftdata.NeedIntegral < 1)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品关联等级信息有误或礼品积分数据有误！";
                    result.status = -5;
                    return result;
                }
            }

            #endregion

            #region 用户信息判断

            if (isdataok)
            {
                //限购数量
                if (giftdata.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber + count > giftdata.LimtQuantity)
                    {
                        isdataok = false;
                        result.success = false;
                        result.msg = "超过礼品限兑数量！";
                        result.status = -4;
                    }
                }
            }

            if (isdataok)
            {
                if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "积分不足！";
                    result.status = -6;
                }
            }

            if (isdataok && giftdata.NeedGrade > 0)
            {
                var memgradeid = _iMemberGradeService.GetMemberGradeByUserId(CurrentUser.Id);
                //等级判定
                if (!_iMemberGradeService.IsOneGreaterOrEqualTwo(memgradeid,giftdata.NeedGrade))
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "用户等级不足！";
                    result.status = -6;
                }
            }
            #endregion

            if (isdataok)
            {
                result.success = true;
                result.msg = "可以购买！";
                result.status = 1;
            }

            return result;
        }
        #endregion

        #region 礼品下单
        /// <summary>
        /// 确认订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public GiftOrderConfirmPageModel ConfirmOrder(GiftConfirmOrder value)
        {
            if (value.Count < 1)
            {
                value.Count = 1;
            }
            var id = value.ID;
            var regionId = value.RegionId;
            var count = value.Count;
            //Checkout
            GiftOrderConfirmPageModel data = new GiftOrderConfirmPageModel();
            List<GiftOrderItemDtoModel> gorditemlist = new List<GiftOrderItemDtoModel>();
            GiftOrderItemDtoModel gorditem;     //订单项

            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (giftdata == null)
            {
                throw new Exception("错误的礼品编号！");
            }
            #endregion

            gorditem = new GiftOrderItemDtoModel(); //补充订单项
            gorditem.GiftId = giftdata.Id;
            gorditem.GiftName = giftdata.GiftName;
            gorditem.GiftValue = giftdata.GiftValue;
            gorditem.ImagePath = giftdata.ImagePath;
            gorditem.OrderId = 0;
            gorditem.Quantity = count;
            gorditem.SaleIntegral = giftdata.NeedIntegral;
            if (!string.IsNullOrWhiteSpace(gorditem.ImagePath))
            {
                gorditem.DefaultImage = HimallIO.GetRomoteProductSizeImage(gorditem.ImagePath, 1, ImageSize.Size_100.GetHashCode());
            }
            gorditemlist.Add(gorditem);

            data.GiftList = gorditemlist;

            data.GiftValueTotal = (decimal)data.GiftList.Sum(d => d.Quantity * d.GiftValue);
            data.TotalAmount = (int)data.GiftList.Sum(d => d.SaleIntegral * d.Quantity);

            //用户地址
            ShippingAddressInfo shipdata = GetShippingAddress(regionId);
            Mapper.CreateMap<ShippingAddressInfo, ShippingAddressDtoModel>();
            ShippingAddressDtoModel shipobj = Mapper.Map<ShippingAddressDtoModel>(shipdata);
            data.ShipAddress = shipobj;

            return data;
        }
        /// <summary>
        /// 提交并处理订单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="regionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Result SubmitOrder(GiftConfirmOrder value)
        {
            Result result = new Result() { success = false, msg = "未知错误", status = 0 };
            bool isdataok = true;
            long id = value.ID;
            var regionId = value.RegionId;
            if (regionId < 1)
            {
                result.success = false;
                result.msg = "错误的收货地址！";
                result.status = -8;
                return result;
            }
            int count = value.Count;

            if (count < 1)
            {
                isdataok = false;
                result.success = false;
                result.msg = "错误的兑换数量！";
                result.status = -8;
                return result;
            }
            if (CurrentUser == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "用户未登录！";
                result.status = -6;
                return result;
            }
            //Checkout
            List<GiftOrderItemModel> gorditemlist = new List<GiftOrderItemModel>();
            var curUser = _iMemberService.GetMember(CurrentUser.Id);
            if (curUser == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "用户登录错误！";
                result.status = -6;
                return result;
            }
            var userInte = MemberIntegralApplication.GetMemberIntegral(curUser.Id);

            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (giftdata == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品不存在！";
                result.status = -2;

                return result;
            }

            if (giftdata.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品已失效！";
                result.status = -2;

                return result;
            }

            //库存判断
            if (count > giftdata.StockQuantity)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！";
                result.status = -3;

                return result;
            }

            //积分数
            if (giftdata.NeedIntegral < 1)
            {
                isdataok = false;
                result.success = false;
                result.msg = "礼品关联等级信息有误或礼品积分数据有误！";
                result.status = -5;

                return result;
            }
            #endregion

            #region 用户信息判断
            //限购数量
            if (giftdata.LimtQuantity > 0)
            {
                int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                if (ownbuynumber + count > giftdata.LimtQuantity)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "超过礼品限兑数量！";
                    result.status = -4;

                    return result;
                }
            }
            if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
            {
                isdataok = false;
                result.success = false;
                result.msg = "积分不足！";
                result.status = -6;

                return result;
            }
            if (giftdata.NeedGrade > 0)
            {
                var memgradeid = _iMemberGradeService.GetMemberGradeByUserId(curUser.Id);
                //等级判定
                if (!_iMemberGradeService.IsOneGreaterOrEqualTwo(memgradeid,giftdata.NeedGrade))
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "用户等级不足！";
                    result.status = -6;

                    return result;
                }
            }
            #endregion

            ShippingAddressInfo shipdata = GetShippingAddress(regionId);
            if (shipdata == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "错误的收货人地址信息！";
                result.status = -6;

                return result;
            }

            if (isdataok)
            {
                gorditemlist.Add(new GiftOrderItemModel { GiftId = giftdata.Id, Counts = count });
                GiftOrderModel createorderinfo = new GiftOrderModel();
                createorderinfo.Gifts = gorditemlist;
                createorderinfo.CurrentUser = curUser;
                createorderinfo.ReceiveAddress = shipdata;
                GiftOrderInfo orderdata = _iGiftsOrderService.CreateOrder(createorderinfo);
                result.success = true;
                result.msg = orderdata.Id.ToString();
                result.status = 1;
            }

            return result;
        }
        #endregion

        #region 我的礼品订单
        /// <summary>
        /// 获取礼品订单列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public QueryPageModel<GiftsOrderDtoModel> GetMyOrderList(string skey = "", GiftOrderInfo.GiftOrderStatus? status = null, int page = 1, int pagesize = 10)
        {
            if (CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            int rows = pagesize;
            GiftsOrderQuery query = new GiftsOrderQuery();
            query.skey = skey;
            if (status != null)
            {
                if ((int)status != 0)
                {
                    query.status = status;
                }
            }
            query.UserId = CurrentUser.Id;
            query.PageSize = rows;
            query.PageNo = page;
            var orderdata = _iGiftsOrderService.GetOrders(query);
            List<GiftOrderInfo> orderlist = orderdata.Models.ToList();
            //_iGiftsOrderService.OrderAddUserInfo(orderlist);
            var result = orderlist.ToList();
            Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
            Mapper.CreateMap<GiftOrderItemInfo, GiftsOrderItemDtoModel>();
            List<GiftsOrderDtoModel> pagedata = new List<GiftsOrderDtoModel>();
            foreach (var item in result)
            {
                item.Address = ClearHtmlString(item.Address);
                item.CloseReason = ClearHtmlString(item.CloseReason);
                item.UserRemark = ClearHtmlString(item.UserRemark);

                var tmpordobj = Mapper.Map<GiftsOrderDtoModel>(item);
                tmpordobj.Items = new List<GiftsOrderItemDtoModel>();
                foreach (var subitem in item.Himall_GiftOrderItem)
                {
                    var tmporditemobj = Mapper.Map<GiftsOrderItemDtoModel>(subitem);
                    tmporditemobj.DefaultImage = HimallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                    tmpordobj.Items.Add(tmporditemobj);
                }
                pagedata.Add(tmpordobj);
            }

            QueryPageModel<GiftsOrderDtoModel> pageresult = new QueryPageModel<GiftsOrderDtoModel>();
            pageresult.Total = orderdata.Total;
            pageresult.Models = pagedata;
            return pageresult;
        }
        /// <summary>
        /// 获取订单综合数据
        /// </summary>
        /// <returns></returns>
        public GiftsOrderAggregateDataModel GetOrderCount()
        {
            if (CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            GiftsOrderAggregateDataModel result = new GiftsOrderAggregateDataModel();
            long curid = CurrentUser.Id;
            result.AllCount = _iGiftsOrderService.GetOrderCount(null,curid);
            result.WaitDeliveryCount = _iGiftsOrderService.GetOrderCount( GiftOrderInfo.GiftOrderStatus.WaitDelivery, curid);
            result.WaitReceivingCount = _iGiftsOrderService.GetOrderCount(GiftOrderInfo.GiftOrderStatus.WaitReceiving, curid);
            result.FinishCount = _iGiftsOrderService.GetOrderCount(GiftOrderInfo.GiftOrderStatus.Finish, curid);
            return result;
        }
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public GiftsOrderDtoModel GetOrder(long id)
        {
            if (CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            var orderdata = _iGiftsOrderService.GetOrder(id, CurrentUser.Id);
            if (orderdata == null)
            {
                throw new HimallException("错误的订单编号");
            }
            Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
            Mapper.CreateMap<GiftOrderItemInfo, GiftsOrderItemDtoModel>();
            //_iGiftsOrderService.OrderAddUserInfo(orderlist);
            orderdata.Address = ClearHtmlString(orderdata.Address);
            orderdata.CloseReason = ClearHtmlString(orderdata.CloseReason);
            orderdata.UserRemark = ClearHtmlString(orderdata.UserRemark);

            var result = Mapper.Map<GiftsOrderDtoModel>(orderdata);
            result.Items = new List<GiftsOrderItemDtoModel>();
            foreach (var subitem in orderdata.Himall_GiftOrderItem)
            {
                var tmporditemobj = Mapper.Map<GiftsOrderItemDtoModel>(subitem);
                tmporditemobj.DefaultImage = HimallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                result.Items.Add(tmporditemobj);
            }
            return result;
        }
        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetExpressInfo(long orderId)
        {
            if (CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            var order = _iGiftsOrderService.GetOrder(orderId, CurrentUser.Id);
            var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);

            if (expressData == null)
            {
                return Json(new { Success = false, ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = "" });
            }

            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
            var json = new
            {
                Success = expressData.Success,
                Msg = expressData.Message,
                Data = expressData.ExpressDataItems.Select(item => new
                {
                    time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    content = item.Content
                })
            };
            return Json(new { Success = true, ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = json });
        }
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Result ConfirmOrderOver(GiftConfirmOrderOver value)
        {
            if (CurrentUser == null)
            {
                throw new HimallException("错误的用户信息");
            }
            long id = value.OrderId;
            if (id < 1)
            {
                throw new HimallException("错误的订单编号");
            }
            Result result = new Result();
            _iGiftsOrderService.ConfirmOrder(id, CurrentUser.Id);
            result.success = true;
            result.status = 1;
            result.msg = "订单完成";
            return result;
        }
        #endregion

        #region 私有
        /// <summary>
        /// 计算最大页数
        /// </summary>
        /// <param name="total"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        private int GetMaxPage(int total, int pagesize)
        {
            int result = 1;
            if (total > 0 && pagesize > 0)
            {
                result = (int)Math.Ceiling((double)total / (double)pagesize);
            }
            return result;
        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        private ShippingAddressInfo GetShippingAddress(long? regionId)
        {
            ShippingAddressInfo result = null;
            var _iShippingAddressService = ServiceHelper.Create<IShippingAddressService>();
            if (regionId != null)
            {
                result = _iShippingAddressService.GetUserShippingAddress((long)regionId);
            }
            else
            {
                if (CurrentUser != null)
                {
                    result = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                }
            }
            return result;
        }
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
        #endregion
	}
}
