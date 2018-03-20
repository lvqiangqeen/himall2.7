using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /**
     * 商城主题设置实体类
     * 2016-05-16
     * **/
    public class Theme
    {
        private long _ThemeId = 0;
        /// <summary>
        /// 主题ID
        /// </summary>
        public long ThemeId { get { return _ThemeId; } set { _ThemeId = value; } }

        /// <summary>
        /// 0、默认设置；1、自定义设置
        /// </summary>
        public Himall.CommonModel.ThemeType TypeId { get; set; }

        /// <summary>
        /// 商城主色
        /// </summary>
        public string MainColor { get; set; }

        /// <summary>
        /// 商城辅色
        /// </summary>
        public string SecondaryColor { get; set; }


        /// <summary>
        /// 字体颜色
        /// </summary>
        public string WritingColor { get; set; }


        /// <summary>
        /// 边框颜色
        /// </summary>
        public string FrameColor { get; set; }

        /// <summary>
        /// 商品分类栏
        /// </summary>
        public string ClassifiedsColor { get; set; }
    }
}
