using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 删除购物车商品-参数模型
    /// </summary>
    public class CartDeleteCartProductModel
    {
        public string skuIds { get; set; }
    }
}
