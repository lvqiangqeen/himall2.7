using System.Collections.Generic;

namespace Himall.ViewModel
{
    public class PrintModel
    {
        /// <summary>
        /// 打印区域宽
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 打印区域高
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { get; set; }


        public IEnumerable<PrintElement> Elements { get; set; }

        public class PrintElement
        {

            public int X { get; set; }

            public int Y { get; set; }
            
            public int Width { get; set; }

            public int Height { get; set; }

            public string Value { get; set; }

        }
    }
}