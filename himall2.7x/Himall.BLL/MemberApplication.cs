using AutoMapper;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using System;
using System.Collections.Generic;

namespace Himall.Application
{
    public class MemberApplication 
    {
        private static IMessageService _iMessageService= ObjectContainer.Current.Resolve<IMessageService>();
        private static IMemberService _iMemberService= ObjectContainer.Current.Resolve<IMemberService>();
   

        #region 插件相关
        /// <summary>
        /// 获取会员认证情况
        /// </summary>
        /// <param name="UserId">会员ID</param>
        /// <returns></returns>
        public static Himall.DTO.MemberAccountSafety GetMemberAccountSafety(long UserId)
        {
            Himall.DTO.MemberAccountSafety model = new Himall.DTO.MemberAccountSafety();
            model.UserId = UserId;
            List<Himall.Model.MemberContactsInfo> lmMemberContactsInfo = _iMessageService.GetMemberContactsInfo(UserId);

            foreach (Himall.Model.MemberContactsInfo item in lmMemberContactsInfo)
            {
                if (item.ServiceProvider.Contains("SMS"))
                {
                    model.Phone = item.Contact;
                    model.BindPhone = true;
                }
                else if (item.ServiceProvider.Contains("Email"))
                {
                    model.Email = item.Contact;
                    model.BindEmail = true;
                }
            }

            return model;
        }

        /// <summary>
        /// 更新插件信息
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMemberContacts(Himall.DTO.MemberContacts model)
        {
            Mapper.CreateMap<Himall.DTO.MemberContacts, Himall.Model.MemberContactsInfo>();
            var mm = Mapper.Map<Himall.DTO.MemberContacts, Himall.Model.MemberContactsInfo>(model);
            _iMessageService.UpdateMemberContacts(mm);
        }

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Himall.DTO.MemberContacts GetMemberContactsInfo(string pluginId, string contact, Himall.Model.MemberContactsInfo.UserTypes type)
        {
            var model = _iMessageService.GetMemberContactsInfo(pluginId, contact, type);
            Mapper.CreateMap<Himall.Model.MemberContactsInfo, Himall.DTO.MemberContacts>();
            return Mapper.Map<Himall.Model.MemberContactsInfo, Himall.DTO.MemberContacts>(model);
        }

        /// <summary>
        /// 发送验证码，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static Himall.CommonModel.SendMemberCodeReturn SendMemberCode(string pluginId, string destination, string UserName, string SiteName)
        {
            //判断号码是否绑定
            if (_iMessageService.GetMemberContactsInfo(pluginId, destination, Himall.Model.MemberContactsInfo.UserTypes.General) != null)
            {
                return Himall.CommonModel.SendMemberCodeReturn.repeat;
            }
            else
            {
                var timeout = CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId); //验证码超时时间
                if (Core.Cache.Get(timeout) != null)
                {
                    return Himall.CommonModel.SendMemberCodeReturn.limit;
                }
                var checkCode = new Random().Next(10000, 99999);
                Log.Debug("Code:" + checkCode);
                var cacheTimeout = DateTime.Now.AddMinutes(15);
                Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(UserName, pluginId + destination), checkCode, cacheTimeout);
                var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
                _iMessageService.SendMessageCode(destination, pluginId, user);
                Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
                return Himall.CommonModel.SendMemberCodeReturn.success;
            }
        }

        /// <summary>
        /// 验证码验证，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckMemberCode(string pluginId, string code, string destination, long userId)
        {
            var member = _iMemberService.GetMember(userId);
            int result = 0;
            var cache = CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                if (_iMessageService.GetMemberContactsInfo(pluginId, destination, Himall.Model.MemberContactsInfo.UserTypes.General) != null)
                {
                    result = -1;
                }
                else
                {
                    if (pluginId.ToLower().Contains("email"))
                    {
                        member.Email = destination;
                    }
                    else if (pluginId.ToLower().Contains("sms"))
                    {
                        member.CellPhone = destination;
                    }

                    _iMemberService.UpdateMember(member);

                    _iMessageService.UpdateMemberContacts(new Himall.Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = userId, UserType = Himall.Model.MemberContactsInfo.UserTypes.General });

                    Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId));
                    Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                    Core.Cache.Remove("Rebind" + userId);
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool SendCode(string pluginId, string destination, string UserName, string SiteName)
        {
            var timeout = CacheKeyCollection.MemberPluginAuthenticateTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Get(timeout) != null)
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginAuthenticate(UserName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginAuthenticateTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// 验证码验证
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckCode(string pluginId, string code, string destination, long userId)
        {
            var member = _iMemberService.GetMember(userId);
            int result = 0;
            var cache = CacheKeyCollection.MemberPluginAuthenticate(member.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                Core.Cache.Remove(CacheKeyCollection.MemberPluginAuthenticate(member.UserName, pluginId));
                Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                Core.Cache.Remove("Rebind" + userId);
                result = 1;
            }
            return result;
        }
        #endregion

        #region 会员相关
        /// <summary>
        /// 获取会员信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static Himall.DTO.Members GetMembers(long UserId)
        {
            var model = _iMemberService.GetMember(UserId);
            Mapper.CreateMap<Himall.Model.UserMemberInfo, Himall.DTO.Members>();
            return Mapper.Map<Himall.Model.UserMemberInfo, Himall.DTO.Members>(model);
        }

        /// <summary>
        /// 更新会员信息
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMember(Himall.DTO.Members model)
        {
            var mm = _iMemberService.GetMember(model.Id);
            mm.Email = model.Email;
            mm.CellPhone = model.CellPhone;
            mm.RealName = model.RealName;
            _iMemberService.UpdateMember(mm);
        }

        #endregion
    }
}
