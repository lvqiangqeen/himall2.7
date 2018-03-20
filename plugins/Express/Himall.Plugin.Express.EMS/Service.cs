using Himall.Core.Plugins;
using Himall.ExpressPlugin;
using System;
using System.Linq;

namespace Himall.Plugin.Express.EMS
{
    public class Service : ExpressPluginBase, IExpress
    {
        public override string NextExpressCode(string currentExpressCode)
        {
            var serialNo = Convert.ToInt64(currentExpressCode.Substring(2, 8));
            if (serialNo < 99999999)
                serialNo++;
            var strSerialNo = serialNo.ToString().PadLeft(8, '0');
            var temp = currentExpressCode.Substring(0, 2) + strSerialNo + currentExpressCode.Substring(10, 1);
            temp = currentExpressCode.Substring(0, 2) + strSerialNo + getEMSLastNum(temp) + currentExpressCode.Substring(11, 2);
            return temp;  
        }


        private string getEMSLastNum(string emsno)
        {
            var arrems = emsno.ToList();
            var emslastno = int.Parse(arrems[2].ToString()) * 8;
            emslastno += int.Parse(arrems[3].ToString()) * 6;
            emslastno += int.Parse(arrems[4].ToString()) * 4;
            emslastno += int.Parse(arrems[5].ToString()) * 2;
            emslastno += int.Parse(arrems[6].ToString()) * 3;
            emslastno += int.Parse(arrems[7].ToString()) * 5;
            emslastno += int.Parse(arrems[8].ToString()) * 9;
            emslastno += int.Parse(arrems[9].ToString()) * 7;
            emslastno = 11 - emslastno % 11;
            if (emslastno == 10)
                emslastno = 0;
            else if (emslastno == 11)
                emslastno = 5;
            return emslastno.ToString();
        }

    }
}
