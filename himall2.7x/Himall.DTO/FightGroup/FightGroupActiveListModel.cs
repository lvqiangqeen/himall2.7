using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO
{
    /// <summary>
    /// 拼团活动列表
    /// </summary>
    public class FightGroupActiveListModel
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 店铺编号
        ///</summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 商品编号
        ///</summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 图片
        ///</summary>
        public string IconUrl { get; set; }
        /// <summary>
        /// 开始时间
        ///</summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        ///</summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 组团数量
        /// </summary>
        public int GroupCount { get; set; }
        /// <summary>
        /// 成功成团数量
        ///</summary>
        public int OkGroupCount { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
        public FightGroupActiveStatus ActiveStatus {
            get
            {
                FightGroupActiveStatus result = FightGroupActiveStatus.Ending;
                if (EndTime < DateTime.Now)
                {
                    result = FightGroupActiveStatus.Ending;
                }
                else
                {
                    if (StartTime > DateTime.Now)
                    {
                        result = FightGroupActiveStatus.WillStart;
                    }
                    else
                    {
                        result = FightGroupActiveStatus.Ongoing;
                    }
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
                return this.ActiveStatus.ToDescription();
            }
        }
        /// <summary>
        /// 人数限制
        ///</summary>
        public int LimitedNumber { get; set; }
        /// <summary>
        /// 火拼价
        /// </summary>
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal MiniSalePrice { get; set; }
        /// <summary>
        /// 管理审核状态
        /// </summary>
        public FightGroupManageAuditStatus FightGroupManageAuditStatus { get; set; }
        /// <summary>
        /// 下架原因
        /// </summary>
        public string ManageRemark { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public Nullable<System.DateTime> ManageDate { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public Nullable<long> ManagerId { get; set; }
        /// <summary>
        /// 商品是否还有库存
        /// </summary>
        public bool HasStock { get; set; }

        public string ProductShortDescription { get; set; }
    }
}
