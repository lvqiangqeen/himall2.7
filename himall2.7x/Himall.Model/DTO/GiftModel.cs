using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class GiftModel:GiftInfo
    {
        /// <summary>
        /// 销售状态显示字符
        /// </summary>
        public string ShowSalesStatus
        {
            get
            {
                string result = "有错误";
                if (this != null)
                {
                    switch (GetSalesStatus)
                    {
                        case GiftInfo.GiftSalesStatus.IsDelete:
                            result = "已删除";
                            break;
                        case GiftInfo.GiftSalesStatus.Normal:
                            result = "可兑换";
                            break;
                        case GiftInfo.GiftSalesStatus.OffShelves:
                            result = "已下架";
                            break;
                        case GiftInfo.GiftSalesStatus.HasExpired:
                            result = "已过期";
                            break;
                    }
                }
                return result;
            }
        }

    }
}
