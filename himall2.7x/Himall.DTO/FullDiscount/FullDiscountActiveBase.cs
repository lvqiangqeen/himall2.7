using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.DTO
{
    /// <summary>
    /// 满减-活动
    /// </summary>
    public class FullDiscountActiveBase
    {
        public long Id { get; set; }
        /// <summary>
        /// 所属店铺
        /// </summary>
        public long ShopId { get; set; }
        public string ActiveName { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        /// <summary>
        /// 是否全部商品
        /// </summary>
        public bool IsAllProduct { get; set; }
        /// <summary>
        /// 是否全部门店
        /// </summary>
        public bool IsAllStore { get; set; }
        public Himall.CommonModel.MarketActiveType ActiveType { get; set; }
        /// <summary>
        /// 活动状态
        /// <para></para>
        /// </summary>
        [ObsoleteAttribute("请勿直接使用，请使用FullDiscountActiveStatus")]
        public int ActiveStatus { get; set; }
        /// <summary>
        /// 满减活动状态
        /// <para>不可以参与搜索</para>
        /// </summary>
        public FullDiscountStatus FullDiscountActiveStatus
        {
            get
            {
                FullDiscountStatus result = FullDiscountStatus.Ending;
                DateTime curTime = DateTime.Now;
                if (StartTime <= curTime && EndTime >= curTime)
                {
                    result = FullDiscountStatus.Ongoing;
                }
                else if (StartTime > curTime)
                {
                    result = FullDiscountStatus.WillStart;
                }
                return result;
            }
        }
        /// <summary>
        /// 活动状态显示名称
        /// </summary>
        public string ShowActiveStatus
        {
            get
            {
                return FullDiscountActiveStatus.ToDescription();
            }
        }
        /// <summary>
        /// 活动商品数
        /// <para>手动维护,-1表示所有商品</para>
        /// </summary>
        public int ProductCount { get; set; }
    }
}
