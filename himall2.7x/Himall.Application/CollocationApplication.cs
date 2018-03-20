using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;

namespace Himall.Application
{
    public class CollocationApplication
    {
        private static ICollocationService _iCollocationService = ObjectContainer.Current.Resolve<ICollocationService>();
        /// <summary>
        /// 商家添加一个组合购
        /// </summary>
        /// <param name="info"></param>
        public static void AddCollocation(CollocationInfo info)
        {
            _iCollocationService.AddCollocation(info);
        }


        /// <summary>
        /// 商家修改一个组合购
        /// </summary>
        /// <param name="info"></param>
        public static void EditCollocation(CollocationInfo info)
        {
            _iCollocationService.EditCollocation(info);
        }



        //使组合购失效
        public static void CancelCollocation(long CollocationId, long shopId)
        {
            _iCollocationService.CancelCollocation(CollocationId, shopId);
        }

        /// <summary>
        /// 获取商家添加的组合购列表
        /// </summary>
        /// <returns></returns>
        public static ObsoletePageModel<CollocationInfo> GetCollocationList(CollocationQuery query)
        {
            return _iCollocationService.GetCollocationList(query);
        }


        /// <summary>
        /// 根据商品ID获取组合购信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static CollocationInfo GetCollocationByProductId(long productId)
        {
            return _iCollocationService.GetCollocationByProductId(productId);
        }
        /// <summary>
        /// 根据组合购ID获取组合购信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static CollocationInfo GetCollocation(long Id)
        {
            return _iCollocationService.GetCollocation(Id);
        }

        /// <summary>
        /// 根据组合商品获取组合SKU信息
        /// </summary>
        /// <param name="colloPid"></param>
        /// <param name="skuid"></param>
        /// <returns></returns>
        public static CollocationSkuInfo GetColloSku(long colloPid, string skuid)
        {
            return _iCollocationService.GetColloSku(colloPid, skuid);
        }

        //获取一个商品的组合购SKU信息
        public static List<CollocationSkuInfo> GetProductColloSKU(long productid, long colloPid)
        {
            return _iCollocationService.GetProductColloSKU(productid, colloPid);
        }
        public static string GetChineseNumber(int number)
        {
            string numberStr = NumberToChinese(number);
            string firstNumber = string.Empty;
            string lastNumber = string.Empty;
            string str = number.ToString();
            firstNumber = str.Substring(0, 1);
            lastNumber = str.Substring(str.Length - 1);
            if (str.Length > 1 && lastNumber == "0")
            {
                numberStr = numberStr.Substring(0, numberStr.Length - 1);
            }
            if (str.Length == 2 && firstNumber == "1")
            {
                numberStr = numberStr.Substring(1);
            }

            return numberStr;
        }
        /// <summary>
        /// 数字转中文
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToChinese(int number)
        {
            string res = string.Empty;
            string str = number.ToString();
            string schar = str.Substring(0, 1);
            switch (schar)
            {
                case "1":
                    res = "一";
                    break;
                case "2":
                    res = "二";
                    break;
                case "3":
                    res = "三";
                    break;
                case "4":
                    res = "四";
                    break;
                case "5":
                    res = "五";
                    break;
                case "6":
                    res = "六";
                    break;
                case "7":
                    res = "七";
                    break;
                case "8":
                    res = "八";
                    break;
                case "9":
                    res = "九";
                    break;
                default:
                    res = "零";
                    break;
            }
            if (str.Length > 1)
            {
                switch (str.Length)
                {
                    case 2:
                    case 6:
                        res += "十";
                        break;
                    case 3:
                    case 7:
                        res += "百";
                        break;
                    case 4:
                        res += "千";
                        break;
                    case 5:
                        res += "万";
                        break;
                    default:
                        res += "";
                        break;
                }
                res += NumberToChinese(int.Parse(str.Substring(1, str.Length - 1)));
            }
            return res;
        }
    }
}
