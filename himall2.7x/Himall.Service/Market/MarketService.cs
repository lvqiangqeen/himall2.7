using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Entity;
using Himall.Core;
using Himall.Service;

namespace Himall.Service
{
    public class MarketService : ServiceBase, IMarketService
    {
        public ActiveMarketServiceInfo GetMarketService(long shopId, MarketType type)
        {
            var model = Context.ActiveMarketServiceInfo.FirstOrDefault(m => m.TypeId == type && m.ShopId == shopId);
            return model;
        }

        public MarketSettingInfo GetServiceSetting(MarketType type)
        {
            var model = Context.MarketSettingInfo.FirstOrDefault(m => m.TypeId == type);
            return model;
        }

        public ObsoletePageModel<MarketServiceRecordInfo> GetBoughtShopList(MarketBoughtQuery query)
        {
            IQueryable<MarketServiceRecordInfo> markets = Context.MarketServiceRecordInfo.AsQueryable();
            if (query.MarketType.HasValue)
            {
                markets = markets.Where(d => d.ActiveMarketServiceInfo.TypeId == query.MarketType.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                markets = markets.Where(d => d.ActiveMarketServiceInfo.ShopName.Contains(query.ShopName));
            }
            int total = 0;
            markets = markets.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<MarketServiceRecordInfo> pageModel = new ObsoletePageModel<MarketServiceRecordInfo>() { Models = markets, Total = total };
            return pageModel;
        }

        /// <summary>
        ///根据ID获取服务购买记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MarketServiceRecordInfo GetShopMarketServiceRecordInfo(long Id)
        {
            var model = Context.MarketServiceRecordInfo.Include("ActiveMarketServiceInfo").Where(a => a.Id == Id).FirstOrDefault();
            return model;
        }

        public void AddOrUpdateServiceSetting(MarketSettingInfo info)
        {
            var model = Context.MarketSettingInfo.FirstOrDefault(a => a.TypeId == info.TypeId);
            if (model == null)
            {
                Context.MarketSettingInfo.Add(info);
            }
            else
            {
                model.Price = info.Price;
            }
            Context.SaveChanges();

        }


        public void OrderMarketService(int monthCount, long shopId, MarketType type)
        {
            if (shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }
            if (monthCount <= 0)
            {
                throw new HimallException("购买服务时长必须大于零");
            }
            var shop = Context.ShopInfo.FindById(shopId);
            if (shop == null || shopId <= 0)
            {
                throw new HimallException("ShopId不能识别");
            }
            var market = Context.ActiveMarketServiceInfo.Where(a => a.ShopId == shopId && a.TypeId == type).FirstOrDefault();
            var price = Context.MarketSettingInfo.Where(a => a.TypeId == type).Select(a => a.Price).FirstOrDefault();
            var StartTime = DateTime.Now;
            MarketServiceRecordInfo model = new MarketServiceRecordInfo();
            model.StartTime = StartTime;
            model.Price = price * monthCount;
            var shopAccount = Context.ShopAccountInfo.Where(a => a.ShopId == shopId).FirstOrDefault();//店铺帐户信息
            if (shopAccount.Balance < model.Price) //店铺余额不足以支付服务费用
            {
                throw new HimallException("您的店铺余额为：" + shopAccount.Balance + "元,不足以支付此次营销服务购买费用，请先充值。");
            }
            if (market != null)
            {
                var maxTime = market.MarketServiceRecordInfo.Max(a => a.EndTime);
                if (maxTime > DateTime.Now) //如果结束时间大于当前时间，续费从结束时间加上购买月数，否则从当前时间加上购买月数
                    StartTime = maxTime;
                model.StartTime = StartTime;
                model.BuyTime = DateTime.Now;
                model.EndTime = StartTime.AddMonths(monthCount);
                // model.MarketServiceId = market.Id;
                model.SettlementFlag = 1;
                market.MarketServiceRecordInfo.Add(model);
            }
            else
            {
                model.StartTime = StartTime;
                model.EndTime = StartTime.AddMonths(monthCount);
                model.SettlementFlag = 1;
                model.BuyTime = DateTime.Now;
                ActiveMarketServiceInfo activeMarketServiceInfo = new Model.ActiveMarketServiceInfo();
                activeMarketServiceInfo.ShopId = shopId;
                activeMarketServiceInfo.ShopName = shop.ShopName;
                activeMarketServiceInfo.TypeId = type;
                activeMarketServiceInfo.MarketServiceRecordInfo.Add(model);
                Context.ActiveMarketServiceInfo.Add(activeMarketServiceInfo);
            }
            Context.SaveChanges();
            var ShopAccount = Context.ShopAccountInfo.FirstOrDefault(a => a.ShopId == shopId);
            ShopAccountItemInfo info = new ShopAccountItemInfo();
            info.IsIncome = false;
            info.ShopId = shopId;
            info.DetailId = model.Id.ToString();
            info.ShopName = shop.ShopName;
            info.AccountNo = shopId + info.DetailId + new Random().Next(10000);
            info.ReMark = "店铺购买" + type.ToDescription() + "服务," + monthCount + "个月";
            info.TradeType = CommonModel.ShopAccountType.MarketingServices;
            info.CreateTime = DateTime.Now;
            info.Amount = price * monthCount;
            info.AccoutID = ShopAccount.Id;
            ShopAccount.Balance -= info.Amount;//总余额减钱
            info.Balance = ShopAccount.Balance;//变动后当前剩余金额
            Context.ShopAccountItemInfo.Add(info);
            var PlatAccount = Context.PlatAccountInfo.FirstOrDefault();
            PlatAccountItemInfo pinfo = new PlatAccountItemInfo();
            pinfo.IsIncome = true;
            pinfo.DetailId = model.Id.ToString();
            pinfo.AccountNo = info.AccountNo;
            pinfo.ReMark = "店铺购买" + type.ToDescription() + "服务," + monthCount + "个月";
            pinfo.TradeType = CommonModel.PlatAccountType.MarketingServices;
            pinfo.CreateTime = DateTime.Now;
            pinfo.Amount = price * monthCount;
            pinfo.AccoutID = PlatAccount.Id;
            PlatAccount.Balance += info.Amount;//总余额加钱

            pinfo.Balance = PlatAccount.Balance;//变动后当前剩余金额
            Context.PlatAccountItemInfo.Add(pinfo);
            Context.SaveChanges();

        }
    }
}
