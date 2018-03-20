using Himall.Core;
using System.Linq.Expressions;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Himall.Service
{
    public class CollocationService : ServiceBase, ICollocationService
    {
        public void AddCollocation(CollocationInfo info)
        {
            CheckCollocationDate(info.ShopId, info.EndTime);
            var date = DateTime.Now.Date;
            var mainId = info.Himall_CollocationPoruducts.Where(a => a.IsMain = true).Select(a => a.ProductId).FirstOrDefault();
            if (Context.CollocationPoruductInfo.Any(a => a.IsMain && a.ProductId == mainId && a.Himall_Collocation.EndTime > date))
            {
                throw new HimallException("此主商品已存在组合购，请勿重复添加！");
            }
            Context.CollocationInfo.Add(info);
            Context.SaveChanges();
        }



        private void CheckCollocationDate(long shopId, DateTime endDate)
        {
            var co = Context.ActiveMarketServiceInfo.FirstOrDefault(a => a.TypeId == MarketType.Collocation && a.ShopId == shopId);
            if (co == null)
            {
                throw new HimallException("您没有订购此服务");
            }
            if (co.MarketServiceRecordInfo.Max(item => item.EndTime.Date) < endDate)
            {
                throw new HimallException("结束日期不能超过购买组合购服务的日期");
            }
        }

        public void EditCollocation(CollocationInfo info)
        {
            CheckCollocationDate(info.ShopId, info.EndTime);
            var coll = Context.CollocationInfo.FirstOrDefault(a => a.Id == info.Id);
            if (coll.EndTime < DateTime.Now.Date)
            {
                throw new HimallException("该活动已结束，无法修改！");
            }
            Context.CollocationPoruductInfo.RemoveRange(coll.Himall_CollocationPoruducts);
            coll.Title = info.Title;
            coll.ShortDesc = info.ShortDesc;
            coll.StartTime = info.StartTime;
            coll.EndTime = info.EndTime;
            coll.ShopId = info.ShopId;
            coll.Himall_CollocationPoruducts = info.Himall_CollocationPoruducts;
            Context.SaveChanges();
        }


        public void CancelCollocation(long CollocationId, long shopId)
        {
            var coll = Context.CollocationInfo.FirstOrDefault(a => a.Id == CollocationId && a.ShopId == shopId);
            if (coll != null)
                coll.EndTime = DateTime.Now.Date;
            Context.SaveChanges();
        }

        public ObsoletePageModel<CollocationInfo> GetCollocationList(CollocationQuery query)
        {
            int total = 0;
            var coll = Context.CollocationInfo.Join(Context.ShopInfo, a => a.ShopId, b => b.Id, (a, b) => new CollocationModel
            {
                Id = a.Id,
                CreateTime = a.CreateTime.Value,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Title = a.Title,
                ShortDesc = a.ShortDesc,
                ShopName = b.ShopName,
                ShopId = a.ShopId,
                ProductId = 0,
            });
            coll = coll.Join(Context.CollocationPoruductInfo.Where(t => t.IsMain == true), a => a.Id, b => b.ColloId, (a, b) => new CollocationModel
            {
                Id = a.Id,
                CreateTime = a.CreateTime.Value,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Title = a.Title,
                ShortDesc = a.ShortDesc,
                ShopName = a.ShopName,
                ShopId = a.ShopId,
                ProductId = b.ProductId
            }
                );
            if (!string.IsNullOrEmpty(query.Title))
            {
                coll = coll.Where(d => d.Title.Contains(query.Title));
            }
            if (query.ShopId.HasValue)
            {
                coll = coll.Where(d => d.ShopId == query.ShopId.Value);
            }
            coll = coll.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(item => item.CreateTime));
            ObsoletePageModel<CollocationInfo> pageModel = new ObsoletePageModel<CollocationInfo>() { Models = coll, Total = total };
            return pageModel;
        }

        public CollocationInfo GetCollocationByProductId(long productId)
        {
            var date = DateTime.Now.Date;
            var collItem = Context.CollocationPoruductInfo
                .FirstOrDefault(a => a.ProductId == productId && a.IsMain && a.Himall_Collocation.StartTime <= date
                    && a.Himall_Collocation.EndTime > date);
            if (collItem != null)
            {
                return collItem.Himall_Collocation;
            }
            return null;
        }
        public List<CollocationPoruductInfo> GetCollocationListByProductId(long productId)
        {
            var date = DateTime.Now.Date;
            var product = Context.ProductInfo.FirstOrDefault(d => d.Id == productId
            && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
            && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
            && d.IsDeleted == false
            );
            if (product != null)
            {
                var collItem = Context.CollocationPoruductInfo
                    .Where(a => a.ProductId == productId && a.Himall_Collocation.StartTime <= date && a.Himall_Collocation.EndTime > date)
                    .ToList();
                if (collItem != null && collItem.Count > 0)
                {
                    return collItem;
                }
            }
            return null;
        }
        public CollocationSkuInfo GetColloSku(long colloPid, string skuid)
        {
            var model = Context.CollocationSkuInfo.FirstOrDefault(a => a.ColloProductId == colloPid && a.SkuID == skuid);
            return model;
        }


        public CollocationInfo GetCollocation(long Id)
        {
            var coll = Context.CollocationInfo.Include("Himall_CollocationPoruducts").FirstOrDefault(a => a.Id == Id);

            return coll;
        }

        public List<CollocationSkuInfo> GetProductColloSKU(long productid, long colloPid)
        {
            var model = Context.CollocationSkuInfo.Where(a => a.ColloProductId == colloPid && a.ProductId == productid);
            return model.ToList();
        }
    }
}
