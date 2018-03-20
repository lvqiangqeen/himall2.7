using Himall.Core;
using Himall.IServices;
using Himall.Model;
using System.Linq;
using Himall.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Himall.CommonModel;

namespace Himall.Service
{
    public class FreightTemplateService : ServiceBase, IFreightTemplateService
    {

        public List<FreightTemplateInfo> GetShopFreightTemplate(long ShopID)
        {
            return Context.FreightTemplateInfo.Where(item => item.ShopID == ShopID).ToList();
        }

        public FreightTemplateInfo GetFreightTemplate(long TemplateId)
        {
            string cacheKey = CacheKeyCollection.CACHE_FREIGHTTEMPLATE(TemplateId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<FreightTemplateInfo>(cacheKey);
            var result= Context.FreightTemplateInfo.Where(e => e.Id == TemplateId).FirstOrDefault();
            Cache.Insert<FreightTemplateInfo>(cacheKey, result, 1800);
            return result;
        }

        public List<FreightAreaDetailInfo> GetFreightAreaDetail(long TemplateId)
        {
            string cacheKey = CacheKeyCollection.CACHE_FREIGHTAREADETAIL(TemplateId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<List<FreightAreaDetailInfo>>(cacheKey);
            var result = Context.FreightAreaDetailInfo.Where(a => a.FreightTemplateId == TemplateId).ToList();
            Cache.Insert<List<FreightAreaDetailInfo>>(cacheKey, result, 1800);
            return result;
        }

        public List<FreightAreaContentInfo> GetFreightAreaContent(long TemplateId)
        {
            return Context.FreightAreaContentInfo.Where(e => e.FreightTemplateId == TemplateId).ToList();
        }


        //public void CopyTemplate(long templateId)
        //{
        //    var model = Context.FreightTemplateInfo.Where(e => e.Id == templateId).FirstOrDefault();
        //    FreightTemplateInfo templateInfo = new FreightTemplateInfo();
        //    templateInfo.IsFree = model.IsFree;
        //    templateInfo.Name = model.Name + "复制";
        //    templateInfo.SendTime = model.SendTime;
        //    templateInfo.ShippingMethod = model.ShippingMethod;
        //    templateInfo.ShopID = model.ShopID;
        //    templateInfo.ValuationMethod = model.ValuationMethod;
        //    templateInfo.SourceAddress = model.SourceAddress;
        //    templateInfo.Himall_FreightAreaContent = model.Himall_FreightAreaContent;
        //    Context.FreightTemplateInfo.Add(templateInfo);
        //    Context.SaveChanges();
        //    var oldArea = Context.FreightAreaDetailInfo.Where(a => a.FreightTemplateId == templateId).ToList();

        //    List<FreightAreaDetailInfo> infos = new List<FreightAreaDetailInfo>();
        //    var newAreas = templateInfo.Himall_FreightAreaContent.ToList();
        //    for (int i= 0;i < newAreas.Count; i++)
        //    {
        //        FreightAreaDetailInfo info = new FreightAreaDetailInfo();
        //        info.FreightAreaId = newAreas[i].Id;
        //        info.FreightTemplateId = newAreas[i].FreightTemplateId;
        //        info.CityId
        //    }
        //}








        public void UpdateFreightTemplate(FreightTemplateInfo templateInfo)
        {
            FreightTemplateInfo model;
            if (templateInfo.Id == 0)
            {
                model = Context.FreightTemplateInfo.Add(templateInfo);
                Context.SaveChanges();
                foreach (var t in templateInfo.Himall_FreightAreaContent)
                {
                    foreach (var d in t.FreightAreaDetailInfo)
                    {
                        d.FreightAreaId = t.Id;
                        d.FreightTemplateId = t.FreightTemplateId;
                        Context.FreightAreaDetailInfo.Add(d);
                    }
                }
                Context.SaveChanges();
            }
            else
            {
                model = Context.FreightTemplateInfo.Where(e => e.Id == templateInfo.Id).FirstOrDefault();
                model.Name = templateInfo.Name;
                model.IsFree = templateInfo.IsFree;
                model.ValuationMethod = templateInfo.ValuationMethod;
                model.ShopID = templateInfo.ShopID;
                model.SourceAddress = templateInfo.SourceAddress;
                model.SendTime = templateInfo.SendTime;
                using (TransactionScope scope = new TransactionScope())
                {
                    //先删除
                    Context.FreightAreaContentInfo.RemoveRange(Context.FreightAreaContentInfo.Where(e => e.FreightTemplateId == model.Id).ToList());
                    //删除详情表
                    Context.FreightAreaDetailInfo.RemoveRange(Context.FreightAreaDetailInfo.Where(a => a.FreightTemplateId == model.Id).ToList());
                    Context.SaveChanges();//保存主表

                    if (model.IsFree == FreightTemplateType.SelfDefine)
                    {
                        //重新插入地区运费
                        //model = context.FreightTemplateInfo.Where(e => e.Id == templateInfo.Id).FirstOrDefault();

                      //  List<FreightAreaContentInfo> fre = new List<FreightAreaContentInfo>();

                        templateInfo.Himall_FreightAreaContent.ToList().ForEach(e =>
                        {
                            //var freightContent = new FreightAreaContentInfo();
                            //freightContent.AreaContent = e.AreaContent;
                            //freightContent.FirstUnit = e.FirstUnit;
                            //freightContent.FirstUnitMonry = e.FirstUnitMonry;
                            //freightContent.AccumulationUnit = e.AccumulationUnit;
                            //freightContent.AccumulationUnitMoney = e.AccumulationUnitMoney;
                            //freightContent.IsDefault = e.IsDefault;
                            //freightContent.FreightTemplateId = model.Id;
                            e.FreightTemplateId = model.Id;
                           // fre.Add(freightContent);
                        });
                        Context.FreightAreaContentInfo.AddRange(templateInfo.Himall_FreightAreaContent.ToList());
                        Context.SaveChanges();
                      //  var index = 0;
                        foreach (var t in templateInfo.Himall_FreightAreaContent)
                        {
                            foreach (var d in t.FreightAreaDetailInfo)
                            {
                                d.FreightAreaId = t.Id;
                                d.FreightTemplateId = model.Id;
                                Context.FreightAreaDetailInfo.Add(d);
                            }
                        }
                        Context.SaveChanges();
                    }
                    scope.Complete();
                }
                Cache.Remove(CacheKeyCollection.CACHE_FREIGHTTEMPLATE(templateInfo.Id));
                Cache.Remove(CacheKeyCollection.CACHE_FREIGHTAREADETAIL(templateInfo.Id));
            }
        }


        public void DeleteFreightTemplate(long TemplateId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                Context.FreightTemplateInfo.Remove(e => e.Id == TemplateId);
                Context.FreightAreaContentInfo.Remove(e => e.FreightTemplateId == TemplateId);
                Context.FreightAreaDetailInfo.Remove(e => e.FreightTemplateId == TemplateId);
                Context.SaveChanges();
                scope.Complete();
            }
        }

        /// <summary>
        /// 是否有商品使用过该运费模板
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <returns></returns>
        public bool IsProductUseFreightTemp(long TemplateId)
        {
            //TODO:[LLY]过滤已删除的商品
            return Context.ProductInfo.Any(item => item.FreightTemplateId == TemplateId && item.IsDeleted == false);
        }
    }
}
