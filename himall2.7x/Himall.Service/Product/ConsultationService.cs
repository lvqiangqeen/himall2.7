using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.IServices;
using System.Linq.Expressions;

namespace Himall.Service
{
    public class ConsultationService : ServiceBase, IConsultationService
    {
        public void AddConsultation(ProductConsultationInfo model)
        {
            var product = Context.ProductInfo.Where(a => a.Id == model.ProductId && a.IsDeleted==false).FirstOrDefault();
            if(product!=null)
            {
                model.ShopId = product.ShopId;
                model.ShopName = Context.ShopInfo.Where(a => a.Id == model.ShopId).Select(p => p.ShopName).FirstOrDefault();
            }
            else
            {
                throw new Himall.Core.HimallException("咨询的商品不存在，或者已删除");
            }
            Context.ProductConsultationInfo.Add(model);
            Context.SaveChanges();
        }


        public void DeleteConsultation(long id)
        {
            Context.ProductConsultationInfo.Remove(id);
            Context.SaveChanges();
        }

        public ObsoletePageModel<ProductConsultationInfo> GetConsultations(ConsultationQuery query)
        {
            int total = 0;

            IQueryable<ProductConsultationInfo> consultation = Context.ProductConsultationInfo.Include(a => a.ProductInfo).AsQueryable();

            #region 条件组合
            if (query.IsReply.HasValue){
                consultation=consultation.Where(item=>(query.IsReply.Value ? item.ReplyDate.HasValue : !item.ReplyDate.HasValue));
            }
            if(!string.IsNullOrWhiteSpace(query.KeyWords)){
                consultation=consultation.Where(item=>item.ConsultationContent.Contains(query.KeyWords));
            }
            if(query.ShopID>0){
                consultation=consultation.Where(item=>query.ShopID == item.ShopId);
            }
            if(query.ProductID>0){
                consultation=consultation.Where(item=>query.ProductID == item.ProductId);
            }
            if(query.UserID>0){
                consultation=consultation.Where(item=>query.UserID == item.UserId);
            }
            #endregion

            consultation = consultation.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<ProductConsultationInfo> pageModel = new ObsoletePageModel<ProductConsultationInfo>() { Models = consultation, Total = total };
            return pageModel;
        }

        public ProductConsultationInfo GetConsultation(long id)
        {
            return Context.ProductConsultationInfo.FindById(id);
        }
        public void ReplyConsultation(long id, string replyContent, long shopId)
        {
            var model = Context.ProductConsultationInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (shopId == 0 || model == null)
            {
                throw new Himall.Core.HimallException("不存在该商品评论");
            }
            model.ReplyContent = replyContent;
            model.ReplyDate = DateTime.Now;
            Context.SaveChanges();
        }


        public IQueryable<ProductConsultationInfo> GetConsultations(long pid)
        {
            return Context.ProductConsultationInfo.FindBy(c => c.ProductId.Equals(pid));
        }
    }
}
