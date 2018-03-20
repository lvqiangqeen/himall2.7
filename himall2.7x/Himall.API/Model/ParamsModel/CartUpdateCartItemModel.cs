using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 更新购物车商品数量-参数模型
    /// </summary>
    public class CartUpdateCartItemModel
    {
        public string jsonstr { get; set; }
    }

    public class UpdateCartSKusModel
    {
        public UpdateCartSKusItemModel[] skus { get; set; }
    }
    /// <summary>
    /// 规格信息
    /// </summary>
    public class UpdateCartSKusItemModel
    {
        public string skuId { get; set; }
        public int count { get; set; }
    }
}
