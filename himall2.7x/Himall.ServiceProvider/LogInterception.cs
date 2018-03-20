using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Himall.ServiceProvider
{
    public class LogInterception 
    {
        //[Interception(typeof(IService), "LogException", InterceptionType.OnLogException)]
        //public static void LogException(string methodName, Dictionary<string, object> parameters, Exception ex)
        //{
        //    string paraString = "";
        //    var serializer = new Newtonsoft.Json.JsonSerializerSettings();
        //    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //    foreach (var d in parameters)
        //    {
        //        paraString += string.Format( "【 {0} : {1} 】，" , d.Key , Newtonsoft.Json.JsonConvert.SerializeObject( d.Value , serializer ) );
        //    }
        //    paraString = string.IsNullOrEmpty(paraString) ? "" : paraString.Substring(0, paraString.Length - 1);
        //    var erroMsg = string.Format("Method:{0} , Parameters:{1}", methodName, paraString);
        //    Himall.Core.Log.Error(erroMsg, ex);
        //}
    }
}
