using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IMemberLabelService : IService
    {
        /// </summary>
        /// 添加标签
        /// <param name="model"></param>
        void AddLabel(LabelInfo model);
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="model"></param>
        void DeleteLabel(LabelInfo model);
        /// <summary>
        /// 更新标签
        /// </summary>
        /// <param name="model"></param>
        void UpdateLabel(LabelInfo model);
        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LabelInfo GetLabel(long id);
        
        /// <summary>
        /// 查询标签列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ObsoletePageModel<LabelInfo> GetMemberLabelList(LabelQuery model);
        /// <summary>
        /// 名称是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool CheckNameIsExist(string name);
        
    }
}
