using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.IServices;
using Himall.IServices.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class MemberLabelApplication
    {
        static IMemberLabelService _iMemberLabelService = ObjectContainer.Current.Resolve<IMemberLabelService>();
        /// <summary>
        /// 查询某个会员标签列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static QueryPageModel<LabelModel> GetMemberLabelList(long userId) {
            var membLabels = MemberApplication.GetMemberLabels(userId);
            var pageModel = new QueryPageModel<LabelModel>(); 
            if (membLabels.Count() > 0)
            {
                var ids = membLabels.Select(e => e.LabelId);
                var labelPageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { LabelIds = ids });
                pageModel.Models = labelPageModel.Models.Select(e => new LabelModel { LabelName = e.LabelName, Id = e.Id }).ToList();
                pageModel.Total = labelPageModel.Total;
            }
            return pageModel;
        }



        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static QueryPageModel<LabelModel> GetLabelList(LabelQuery query) {
            var labelList = _iMemberLabelService.GetMemberLabelList(query);
            QueryPageModel<LabelModel> models = new QueryPageModel<LabelModel>();
            models.Total = labelList.Total;
            var list = AutoMapper.Mapper.Map<List<LabelModel>>(labelList.Models.ToList());
            models.Models = list;
            return models;
        }


        /// <summary>
        /// 增加标签
        /// </summary>
        /// <param name="model"></param>
        public static void AddLabel(LabelModel model)
        {
            var labelInfo = AutoMapper.Mapper.Map<Model.LabelInfo>(model);
            _iMemberLabelService.AddLabel(labelInfo);
        }
        /// <summary>
        /// 名称是否存在
        /// </summary>
        /// <param name="labelName"></param>
        /// <returns></returns>
        public static bool CheckNameIsExist(string labelName)
        {
            return _iMemberLabelService.CheckNameIsExist(labelName);
        }
    }
}
