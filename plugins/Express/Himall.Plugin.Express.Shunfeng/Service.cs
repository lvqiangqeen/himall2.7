using Himall.Core.Plugins;
using Himall.ExpressPlugin;
using System;
using System.Linq;

namespace Himall.Plugin.Express.Shunfeng
{
    public class Service : ExpressPluginBase, IExpress
    {
        public override string NextExpressCode(string currentExpressCode)
        {
            int[] oldNum = new int[12];
            int[] newNum = new int[12];
            var sfArr = currentExpressCode.ToList();
            string fri = currentExpressCode.Substring(0, 11);
            string Nfri = string.Empty;
            //先将前11位加1，存储为新前11位
            if (currentExpressCode.Substring(0, 1) == "0")
            {
                Nfri = "0" + (Convert.ToInt64(fri) + 1).ToString();
            }
            else
            {
                Nfri = (Convert.ToInt64(fri) + 1).ToString();
            }
            ///原始前12位
            /// 
            for (int i = 0; i < 12; i++)
            {
                oldNum[i] = int.Parse(sfArr[i].ToString());
            }
            //新11位
            var NewfriArr = Nfri.ToList();
            for (int i = 0; i < 11; i++)
            {
                newNum[i] = int.Parse(Nfri[i].ToString());
            }
            if (newNum[8] - oldNum[8] == 1 && (oldNum[8] % 2 == 1))
            {
                if (oldNum[11] - 8 >= 0)
                    newNum[11] = oldNum[11] - 8;
                else
                    newNum[11] = oldNum[11] - 8 + 10;

            }
            else if (newNum[8] - oldNum[8] == 1 && (oldNum[8] % 2 == 0))
            {
                if (oldNum[11] - 7 >= 0)
                    newNum[11] = oldNum[11] - 7;
                else
                    newNum[11] = oldNum[11] - 7 + 10;
            }
            else
            {
                if ((oldNum[9] == 3 || oldNum[9] == 6) && oldNum[10] == 9)
                {
                    if (oldNum[11] - 5 >= 0)
                        newNum[11] = oldNum[11] - 5;
                    else
                        newNum[11] = oldNum[11] - 5 + 10;

                }
                else if (oldNum[10] == 9)
                {
                    if (oldNum[11] - 4 >= 0)
                        newNum[11] = oldNum[11] - 4;
                    else
                        newNum[11] = oldNum[11] - 4 + 10;
                }
                else
                {
                    if (oldNum[11] - 1 >= 0)
                        newNum[11] = oldNum[11] - 1;
                    else
                        newNum[11] = oldNum[11] - 1 + 10;
                }

            }
            return Nfri + newNum[11].ToString();
        }
    }
}
