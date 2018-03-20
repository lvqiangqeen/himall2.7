using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    /// <summary>
    /// 敏感关键词服务
    /// </summary>
    public interface ISensitiveWordService : IService
    {
        /// <summary>
        /// 获取敏感关键词列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<SensitiveWordsInfo> GetSensitiveWords(SensitiveWordQuery query);

        /// <summary>
        /// 获取敏感词类别
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetCategories();

        /// <summary>
        /// 获取敏感关键词
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SensitiveWordsInfo GetSensitiveWord(int id);

        /// <summary>
        /// 添加敏感关键词
        /// </summary>
        /// <param name="model"></param>
        void AddSensitiveWord(SensitiveWordsInfo model);

        /// <summary>
        /// 修改敏感关键词
        /// </summary>
        /// <param name="model"></param>
        void UpdateSensitiveWord(SensitiveWordsInfo model);

        /// <summary>
        /// 删除敏感关键词
        /// </summary>
        /// <param name="id"></param>
        void DeleteSensitiveWord(int id);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        void BatchDeleteSensitiveWord(int[] ids);

        /// <summary>
        /// 判断敏感关键词是否存在
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        bool ExistSensitiveWord(string word);
    }
}
