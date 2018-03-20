
using System.Collections.Generic;
namespace Himall.Core.Plugins.Express
{
    /// <summary>
    /// 打印元素
    /// </summary>
    public class  ExpressPrintElement
    {

       #region 模板打印元素

        static Dictionary<int, string> _allPrintElements;

        /// <summary>
        /// 模板打印元素
        /// </summary>
        public static Dictionary<int, string> AllPrintElements
        {
            get
            {
                if (_allPrintElements == null)
                    initExpressAllElements();
                return _allPrintElements;
            }
        }

        static void initExpressAllElements()
        {
            _allPrintElements = new Dictionary<int, string>();
            _allPrintElements.Add(1, "收货人-姓名");
            _allPrintElements.Add(2, "收货人-地址");
            _allPrintElements.Add(3, "收货人-电话");
           // _allPrintElements.Add(4, "收货人-邮编");
            _allPrintElements.Add(5, "收货人-地区1级");
            _allPrintElements.Add(6, "收货人-地区2级");
            _allPrintElements.Add(7, "收货人-地区3级");
            _allPrintElements.Add(8, "发货人-姓名");
            _allPrintElements.Add(9, "发货人-地区1级");
            _allPrintElements.Add(10, "发货人-地区2级");
            _allPrintElements.Add(11, "发货人-地区3级");
            _allPrintElements.Add(12, "发货人-地址");
           // _allPrintElements.Add(13, "发货人-邮编");
            _allPrintElements.Add(14, "发货人-电话");
            _allPrintElements.Add(15, "订单-订单号");
            _allPrintElements.Add(16, "订单-总金额");
            _allPrintElements.Add(17, "订单-物品总重量");
          //  _allPrintElements.Add(18, "订单-备注");
          //  _allPrintElements.Add(19, "订单-详情");
            //_allPrintElements.Add(20, "订单-送货时间");
            _allPrintElements.Add(21, "网店名称");
            _allPrintElements.Add(22, "对号-√");
        }

        #endregion


        /// <summary>
        /// 打印元素序号
        /// </summary>
        public int PrintElementIndex { get; set; }

        /// <summary>
        /// 左上顶点位置
        /// </summary>
        public Point LeftTopPoint { get; set; }

        /// <summary>
        /// 右下顶点位置
        /// </summary>
        public Point RightBottomPoint { get; set; }




        /// <summary>
        /// 点
        /// </summary>
        public class Point
        {
            /// <summary>
            /// X坐标
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// Y坐标
            /// </summary>
            public int Y { get; set; }
        }



    }
}
