using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Himall.DTO
{
    public class AppMessages
    {
        /// ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        [Required(ErrorMessage = "门店ID")]
        public long ShopBranchId { get; set; }
        /// <summary>
        /// 商家ID
        /// </summary>
        [Required(ErrorMessage = "商家ID")]
        public long ShopId { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        [Required(ErrorMessage = "消息类型")]
        public int TypeId { get; set; }
        /// <summary>
        /// 数据来源编号，对应订单ID或者售后ID
        /// </summary>
        [Required(ErrorMessage = "数据来源编号，对应订单ID或者售后ID")]
        public long SourceId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        [Required(ErrorMessage = "消息内容")]
        public string Content { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        [Required(ErrorMessage = "是否已读")]
        public bool IsRead { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        [Required(ErrorMessage = "发送时间")]
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 付款时间
        /// </summary>
        [Required(ErrorMessage = "付款时间")]
        public DateTime? OrderPayDate { get; set; }
        /// <summary>
        /// 消息标题
        /// </summary>
        [Required(ErrorMessage = "消息标题")]
        public string Title { get; set; }
    }
}
