using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IConsultationService : IService
    {
       /// <summary>
       /// 添加一个产品咨询
       /// </summary>
       /// <param name="model"></param>
       void AddConsultation(ProductConsultationInfo model);
       /// <summary>
       ///回复咨询
       /// </summary>
       /// <param name="model"></param>
       void ReplyConsultation(long id,string ReplyContent,long shopId);
       /// <summary>
       /// 删除一个咨询
       /// </summary>
       /// <param name="id"></param>
       void DeleteConsultation(long id);
     /// <summary>
     /// 分页获取咨询列表
     /// </summary>
     /// <param name="query">咨询查询实体</param>
     /// <returns>咨询分页实体</returns>
       ObsoletePageModel<ProductConsultationInfo> GetConsultations(ConsultationQuery query);

        /// <summary>
        /// 获取某一个商品的所有咨询
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
       IQueryable<ProductConsultationInfo> GetConsultations(long pid);

        /// <summary>
        /// 获取一个咨询信息
        /// </summary>
        /// <param name="id">咨询ID</param>
        /// <returns></returns>
       ProductConsultationInfo GetConsultation(long id);
    }
}
