using Himall.Core.Plugins;
using Himall.ExpressPlugin;
using System;

namespace Himall.Plugin.Express.Zhaijisong
{
    public class Service : ExpressPluginBase, IExpress
    {
        public override string NextExpressCode(string currentExpressCode)
        {
            var retNo = Convert.ToInt64(currentExpressCode) + 11;
            if (retNo % 10 > 6)
            {
                retNo -= 7;
            }
            return retNo.ToString().PadLeft(currentExpressCode.Length, '0');
        }
    }
}
