
namespace Himall.Core.Helper
{
    public static class ObjectHelper
    {
        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="obj">待复制的对象</param>
        /// <returns></returns>
        public static object DeepColne(object obj)
        {
            var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var objCopy = Newtonsoft.Json.JsonConvert.DeserializeObject(objJson);
            return objCopy;
        }


        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="obj">待复制的对象</param>
        /// <returns></returns>
        public static T DeepColne<T>(T t)
        {
            var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            var objCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objJson);
            return objCopy;
        }
    }
}
