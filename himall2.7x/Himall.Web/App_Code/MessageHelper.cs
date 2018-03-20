using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web
{
    public class MessageHelper
    {
        /// <summary>
        /// 设置发送次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns>返回最新的发送次数</returns>
        public int SetErrorTimes(string username)
        {
            var times = GetErrorTimes(username) + 1;
            Core.Cache.Insert(CacheKeyCollection.MessageSendNum(username), times, DateTime.Now.AddMinutes(30.0));//写入缓存
            return times;
        }
        /// <summary>
        /// 获取发送次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public int GetErrorTimes(string username)
        {
            var timesObject = Core.Cache.Get(CacheKeyCollection.MessageSendNum(username));
            var times = timesObject == null ? 0 : (int)timesObject;
            return times;
        }

        public void ClearErrorTimes(string username)
        {
            Core.Cache.Remove(CacheKeyCollection.MessageSendNum(username));
        }
    }
}