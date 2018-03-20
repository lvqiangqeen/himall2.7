using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ProductTypeInfo
    {
        [NotMapped]
        public string ColorValue { get; set; }

        [NotMapped]
        public string SizeValue { get; set; }

        [NotMapped]
        public string VersionValue { get; set; }

        public ProductTypeInfo(bool initialSpec):this()
        {
            this.ColorValue = "紫色,红色,绿色,花色,蓝色,褐色,透明,酒红色,黄色,黑色,深灰色,深紫色,深蓝色";
            this.SizeValue = "160/80(XS),190/110(XXXL),165/84(S),170/88(M),175/92(L),180/96(XL),185/100(XXL),160/84(XS),165/88(S),170/92(M)";
            this.VersionValue = "版本1,版本2,版本3,版本4,版本5";
        }
    }
}
