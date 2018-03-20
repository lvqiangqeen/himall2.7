using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Himall.Model;
using Himall.Core;

namespace Himall.DTO
{
    /// <summary>
    /// 拼团活动项
    /// </summary>
    public class FightGroupActiveItemModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 活动编号
        /// </summary>
        public long ActiveId { get; set; }
        /// <summary>
        /// 商品编号
        ///</summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 商品SKU
        ///</summary>
        [Required(ErrorMessage = "错误的规格")]
        public string SkuId { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 显示图片
        /// </summary>
        public string ShowPic { get; set; }
        /// <summary>
        /// 规格名称
        /// </summary>
        public string SkuName { get; set; }
        /// <summary>
        /// 商品售价
        /// </summary>
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// 商品成本价
        /// </summary>
        public decimal ProductCostPrice { get; set; }
        /// <summary>
        /// 售价
        ///</summary>
        [Required(ErrorMessage = "请填写活动售价")]
        [Range(0.01, double.MaxValue, ErrorMessage = "错误的活动售价")]
        public decimal ActivePrice { get; set; }
        /// <summary>
        /// 库存
        ///</summary>
        [Required(ErrorMessage = "请填写活动库存")]
        [Range(0, int.MaxValue, ErrorMessage = "错误的活动库存")]
        public long? ActiveStock { get; set; }
        /// <summary>
        /// 已售
        ///</summary>
        public long? ProductStock { get; set; }
        /// <summary>
        /// 已购数量
        /// </summary>
        public int? BuyCount { get; set; }


        /// <summary>
        /// 验证信息有效性
        /// </summary>
        public void CheckValidation()
        {
            if (string.IsNullOrWhiteSpace(this.SkuId))
            {
                throw new HimallException("错误的SKU信息");
            }

            #region 库存
            if (this.ActiveStock <0)
            {
                throw new HimallException("错误的活动库存");
            }

            if (this.ProductStock.HasValue)
            {
                if (this.ProductStock > 0)
                {
                    if (this.ActiveStock == 0)
                    {
                        throw new HimallException("错误的活动库存");
                    }
                }
                if (this.ActiveStock > this.ProductStock)
                {
                    throw new HimallException("错误的活动库存");
                }
            }
            else
            {
                throw new HimallException("错误的活动库存");
            }
            #endregion

            #region 价格
            if (ActivePrice < 0.01m)
            {
                throw new HimallException("错误的活动价格");
            }
            if(ActivePrice>ProductPrice)
            {
                throw new HimallException("错误的活动价格");
            }
            #endregion
        }
    }
}
