
using System.Configuration;
namespace Himall.Model
{
    /// <summary>
    /// 模型接口
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// 获取图片服务器所在路径
        /// </summary>
        //protected  string ImageServerUrl = ConfigurationManager.AppSettings["ImageServerUrl"];     //TODO:DZY[150811] 暂时不开放此功能
        protected string ImageServerUrl = "";


    }
}
