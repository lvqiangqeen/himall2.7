using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.API;
using Himall.IServices.QueryModel;
using Himall.Application;
using System.IO;
using Himall.CommonModel;

namespace Himall.API
{
    public class ShopMessageController : BaseShopApiController
    {
        /// <summary>
        /// 获取未读消息数
        /// </summary>
        /// <returns></returns>
        public object GetMessages(
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/)
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;
            AppMessageQuery query = new AppMessageQuery();
            query.ShopId = shopid;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.StartTime = DateTime.Now.AddDays(-30).Date;
            var data = AppMessageApplication.GetMessages(query);
            return Json(new { Success = "true", rows = data.Models, total = data.Total });
        }
        /// <summary>
        /// 消息状态改己读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostReadMessage(ShopAppReadMessageModel model)
        {
            CheckUserLogin();
            AppMessageApplication.ReadMessage(model.id);
            return Json(new { Success="true" });
        }
    }
}
