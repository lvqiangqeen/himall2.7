using AutoMapper;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using System;
using System.Collections.Generic;
using Himall.Model;
using System.Linq;
using Himall.IServices.QueryModel;
using Himall.Application.Mappers;
using Himall.DTO;
using Himall.CommonModel;

[assembly: OnHimallStartMethod(typeof(Himall.Application.MemberApplication), "InitMessageQueue")]
namespace Himall.Application
{
    public class MemberApplication
    {
		#region 静态字段
		private static IMemberService _iMemberService = ObjectContainer.Current.Resolve<IMemberService>();
		private static IOrderService _orderService = ObjectContainer.Current.Resolve<IOrderService>();
		private static IRefundService _refundService = ObjectContainer.Current.Resolve<IRefundService>();
		#endregion

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
            List<Himall.Model.MemberContactsInfo> lmMemberContactsInfo = MessageApplication.GetMemberContactsInfo(UserId);

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
            MessageApplication.UpdateMemberContacts(mm);
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
            var model = MessageApplication.GetMemberContactsInfo(pluginId, contact, type);
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
            if (MessageApplication.GetMemberContactsInfo(pluginId, destination, Himall.Model.MemberContactsInfo.UserTypes.General) != null)
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
                MessageApplication.SendMessageCode(destination, pluginId, user);
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
                if (MessageApplication.GetMemberContactsInfo(pluginId, destination, Himall.Model.MemberContactsInfo.UserTypes.General) != null)
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

                    MessageApplication.UpdateMemberContacts(new Himall.Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = userId, UserType = Himall.Model.MemberContactsInfo.UserTypes.General });

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
            MessageApplication.SendMessageCode(destination, pluginId, user);
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
            //  var model = _iMemberService.GetMember(UserId);
            // Mapper.CreateMap<Himall.Model.UserMemberInfo, Himall.DTO.Members>();
            // return Mapper.Map<Himall.Model.UserMemberInfo, Himall.DTO.Members>(model);

            var model = _iMemberService.GetMember(UserId);
            var m = Mapper.Map<Himall.Model.UserMemberInfo, Himall.DTO.Members>(model);

            if (model.InviteUserId.HasValue)
            {
                var inviteUser = _iMemberService.GetMember(model.InviteUserId.Value);
                if (inviteUser != null)
                {
                    m.InviteUserName = inviteUser.UserName;
                }
            }
            m.MemberLabels = MemberLabelApplication.GetMemberLabelList(UserId).Models;
            var userInte = MemberIntegralApplication.GetMemberIntegral(UserId);
            var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);
            m.GradeName = userGrade.GradeName;//方法内部包含获取等级的方法
            return m;
        }

        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static List<DTO.Members> GetMembers(IEnumerable<long> userIds)
        {
			var result= _iMemberService.GetMembers(userIds).Map<List<DTO.Members>>();
			return result;
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

        public static string GetLogo()
        {
            return _iMemberService.GetLogo();
        }

        /// <summary>
        /// 注册一个会员()
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号码</param>
        public static UserMemberInfo Register(string username, string password, string mobile = "", string email = "", long introducer = 0)
        {
            return _iMemberService.Register(username, password, mobile, email, introducer);
        }

        /// <summary>
        /// 注册并绑定一个会员
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像地址</param>
        public static UserMemberInfo Register(string username, string password, string serviceProvider, string openId, string sex = null, string headImage = null, long introducer = 0, string nickname = null, string city = null, string province = null, string unionid = null)
        {
            return _iMemberService.Register(username, password, serviceProvider, openId, sex = null, headImage = null, introducer = 0, nickname = null, city = null, province = null, unionid = null);
        }
        /// <summary>
        /// 注册并绑定一个会员(传model)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static UserMemberInfo Register(OAuthUserModel model)
        {
            return _iMemberService.Register(model);
        }
        /// <summary>
        /// 快速注册
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="openId">OpenId</param>
        /// <param name="nickName">昵称</param>
        /// <param name="realName">真实姓名</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <returns></returns>
		public static UserMemberInfo QuickRegister(string username, string realName, string nickName, string serviceProvider, string openId, string unionid, string sex = null, string headImage = null, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionopenid = null, string city = null, string province = null)
        {
			return _iMemberService.QuickRegister(username, realName, nickName, serviceProvider, openId, unionid, sex = null, headImage = null, appidtype, unionopenid, city, province);
        }

        /// <summary>
        /// 绑定会员
        /// </summary>
        /// <param name="userId">会员id</param>
        /// <param name="serviceProvider">信任登录服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像</param>
        public static void BindMember(long userId, string serviceProvider, string openId, string sex = null, string headImage = null, string unionid = null, string unionopenid = null, string city = null, string province = null)
        {
            _iMemberService.BindMember(userId, serviceProvider, openId, sex, headImage, unionid, unionopenid, city, province);
        }
        /// <summary>
        /// 绑定会员（增加标记，是否是可以支付的openid）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="openId"></param>
        /// <param name="AppidType"></param>
        /// <param name="headImage"></param>
        public static void BindMember(long userId, string serviceProvider, string openId, Model.MemberOpenIdInfo.AppIdTypeEnum AppidType, string sex = null, string headImage = null, string unionid = null)
        {
            _iMemberService.BindMember(userId, serviceProvider, openId, AppidType, sex, headImage, unionid);
        }

        public static void BindMember(OAuthUserModel model)
        {
            _iMemberService.BindMember(model);
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="payPwd"></param>
        public static bool VerificationPayPwd(long memid, string payPwd)
        {
            return _iMemberService.VerificationPayPwd(memid, payPwd);
        }

		/// <summary>
		/// 是否有支付密码
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool HasPayPwd(long id)
		{
			return _iMemberService.HasPayPwd(id);
		}

        /// <summary>
        /// 检查用户名是否重复
        /// </summary>
        /// <param name="username">待检查的用户名</param>
        /// <returns></returns>
        public static bool CheckMemberExist(string username)
        {
            return _iMemberService.CheckMemberExist(username);
        }

        /// <summary>
        /// 检查手机号码是否重复
        /// </summary>
        /// <param name="mobile">待检查的手机号码</param>
        /// <returns></returns>
        public static bool CheckMobileExist(string mobile)
        {
            return _iMemberService.CheckMemberExist(mobile);
        }
        /// <summary>
        /// 检查邮箱是否重复
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckEmailExist(string email)
        {
            return _iMemberService.CheckEmailExist(email);
        }

        /// <summary>
        /// 更新会员信息新方法必须
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateMemberInfo(MemberUpdate model)
        {
            var mm = _iMemberService.GetMember(model.Id);//先查询
                                                         // mm.Email = model.Email;
                                                         //  mm.CellPhone = model.CellPhone;
            mm.RealName = model.RealName;
            mm.Nick = model.Nick;
            mm.QQ = model.QQ;
            if (model.BirthDay.HasValue)
                mm.BirthDay = model.BirthDay;
            mm.Occupation = model.Occupation;
            mm.Sex = model.Sex;
            _iMemberService.UpdateMemberInfo(mm);
        }

        #region 分销用户关系
        /// <summary>
        /// 更改用户的推销员，并建立店铺分佣关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shareUserId">销售员用户编号</param>
        /// <param name="shopId">店铺编号,为0表示仅维护用户与推销员的关系</param>
        public static long UpdateShareUserId(long id, long shareUserId, long shopId = 0)
        {
            return _iMemberService.UpdateShareUserId(id, shareUserId, shopId);
        }
        /// <summary>
        /// 更新用户关系
        /// </summary>
        /// <param name="id">关系信息编号列表</param>
        /// <param name="userId">买家编号</param>
        public static void UpdateDistributionUserLink(IEnumerable<long> ids, long userId)
        {
            _iMemberService.UpdateDistributionUserLink(ids, userId);
        }
        #endregion

        /// <summary>
        /// 更改会员密码
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="password">会员密码</param>
        public static void ChangePassword(long id, string password)
        {
            _iMemberService.ChangePassword(id, password);
        }
		
		/// <summary>
		/// 根据用户名修改密码
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		public static void ChangePassword(string name, string password)
		{
			_iMemberService.ChangePassword(name, password);
		}

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public static void ChangePayPassword(long id, string password)
        {
            _iMemberService.ChangePayPassword(id, password);
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <param name="id"></param>
        public static void LockMember(long id)
        {
            _iMemberService.LockMember(id);
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <param name="id"></param>
        public static void UnLockMember(long id)
        {
            _iMemberService.UnLockMember(id);
        }

        /// <summary>
        /// 删除一个会员
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteMember(long id)
        {
            _iMemberService.DeleteMember(id);
        }


        /// <summary>
        /// 批量删除会员
        /// </summary>
        /// <param name="ids">批量ID数组</param>
        public static void BatchDeleteMember(long[] ids)
        {
            _iMemberService.BatchDeleteMember(ids);
        }

        /// <summary>
        /// 批量锁定
        /// </summary>
        /// <param name="ids"></param>
        public static void BatchLock(long[] ids)
        {
            _iMemberService.BatchLock(ids);
        }

        /// <summary>
        /// 根据查询条件分页获取会员信息(兼容老版本)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<UserMemberInfo> GetMembers(MemberQuery query)
        {
            return _iMemberService.GetMembers(query);
        }



        /// <summary>
        /// 根据查询条件分页获取会员信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<DTO.Members> GetMemberList(MemberQuery query)
        {
            if (query.GradeId.HasValue)
            {
                var expenditureRange = MemberApplication.GetMemberGradeRange(query.GradeId.Value);
                query.MinIntegral = expenditureRange.MinIntegral;
                query.MaxIntegral = expenditureRange.MaxIntegral;
            }
            var list = _iMemberService.GetMembers(query);
            var members = Mapper.Map<QueryPageModel<DTO.Members>>(list);
            var grades = MemberGradeApplication.GetMemberGradeList();
            foreach (var m in members.Models)
            {
                var memberIntegral = MemberIntegralApplication.GetMemberIntegral(m.Id);
                m.GradeName = MemberGradeApplication.GetMemberGradeByIntegral(grades, memberIntegral.HistoryIntegrals).GradeName;
                if (memberIntegral != null)
                {
                    m.AvailableIntegral = memberIntegral.AvailableIntegrals;
                    m.HistoryIntegral = memberIntegral.HistoryIntegrals;
                }
            }
            return members;
        }

        /// <summary>
        /// 通过会员等级ID获取会员消费范围
        /// </summary>
        /// <param name="gradeId"></param>
        /// <returns></returns>
        public static GradeIntegralRange GetMemberGradeRange(long gradeId)
        {
            return _iMemberService.GetMemberGradeRange(gradeId);
        }



        /// <summary>
        /// 获取查询的会员信息列表
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        public static IQueryable<UserMemberInfo> GetMembers(bool? status, string keyWords)
        {
            return _iMemberService.GetMembers(status, keyWords);
        }

        /// <summary>
        /// 获取一个会员信息
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <returns></returns>
        public static UserMemberInfo GetMember(long id)
        {
            return _iMemberService.GetMember(id);
        }

        /// <summary>
        /// 根据用户id和类型获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        public static DTO.MemberOpenId GetMemberOpenIdInfoByuserId(long userId, MemberOpenIdInfo.AppIdTypeEnum appIdType)
        {
            var model = _iMemberService.GetMemberOpenIdInfoByuserId(userId, appIdType);
            return AutoMapper.Mapper.Map<DTO.MemberOpenId>(model);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        public static UserMemberInfo Login(string username, string password)
        {
            return _iMemberService.Login(username, password);
        }
		
		/// <summary>
		/// 修改最后登录时间
		/// </summary>
		/// <param name="id"></param>
		public static void UpdateLastLoginDate(long id)
		{
			_iMemberService.UpdateLastLoginDate(id);
		}

        //根据用户名获取用户信息
        public static UserMemberInfo GetMemberByName(string userName)
        {
            return _iMemberService.GetMemberByName(userName);
        }

        /// <summary>
        ///根据用户ID从缓存获取用户信息
        /// </summary>
        /// <returns></returns>
        public static UserMemberInfo GetUserByCache(long id)
        {
            return _iMemberService.GetUserByCache(id);
        }

        /// <summary>
        /// 获取用户个人中心信息
        /// </summary>
        /// <returns></returns>
        public static UserCenterModel GetUserCenterModel(long id)
        {
            return _iMemberService.GetUserCenterModel(id);
        }

        /// <summary>
        /// 根据服务商返回的OpenId获取系统中对应的用户
        /// </summary>
        /// <param name="serviceProvider">服务商名称</param>
        /// <param name="openId">OpenId</param>
        /// <returns></returns>
        public static UserMemberInfo GetMemberByOpenId(string serviceProvider, string openId)
        {
            return _iMemberService.GetMemberByOpenId(serviceProvider, openId);
        }
        /// <summary>
        /// 根据UnionId、Provider取用户信息
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="UnionId"></param>
        /// <returns></returns>
        public static UserMemberInfo GetMemberByUnionIdAndProvider(string serviceProvider, string UnionId)
        {
            return _iMemberService.GetMemberByUnionId(serviceProvider, UnionId);
        }
        /// <summary>
        /// 根据UnionId取用户信息
        /// </summary>
        /// <param name="UnionId"></param>
        /// <returns></returns>
        public static UserMemberInfo GetMemberByUnionId(string UnionId)
        {
            return _iMemberService.GetMemberByUnionId(UnionId);
        }

        public static void DeleteMemberOpenId(long userid, string openid)
        {
            _iMemberService.DeleteMemberOpenId(userid, openid);
        }

        /// <summary>
        /// 从手机号或者邮箱信息来获取用户信息
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static UserMemberInfo GetMemberByContactInfo(string contact)
        {
            return _iMemberService.GetMemberByContactInfo(contact);
        }

        /// <summary>
        /// 检查是邮箱或手机是否被占用
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <param name="userType"></param>
        public static void CheckContactInfoHasBeenUsed(string serviceProvider, string contact, Himall.Model.MemberContactsInfo.UserTypes userType = Himall.Model.MemberContactsInfo.UserTypes.General)
        {
            _iMemberService.CheckContactInfoHasBeenUsed(serviceProvider, contact, userType);
        }

        public static IQueryable<MemberLabelInfo> GetMembersByLabel(long labelid)
        {
            return _iMemberService.GetMembersByLabel(labelid);
        }

        public static IEnumerable<MemberLabelInfo> GetMemberLabels(long memid)
        {
            return _iMemberService.GetMemberLabels(memid);
        }
        /// <summary>
        /// 设置会员标签(移除原标签)
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        public static void SetMemberLabel(long userid, IEnumerable<long> labelids)
        {
            _iMemberService.SetMemberLabel(userid, labelids);
        }
        /// <summary>
        /// 设置多会员标签，只累加，不移除原标签
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        public static void SetMembersLabel(long[] userid, IEnumerable<long> labelids)
        {
            _iMemberService.SetMembersLabel(userid, labelids);
        }

        public static IEnumerable<int> GetAllTopRegion()
        {
            return _iMemberService.GetAllTopRegion();
        }
        #endregion

		#region 方法
		/// <summary>
		/// 会员购买力列表
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static QueryPageModel<DTO.MemberPurchasingPower> GetPurchasingPowerMember(MemberPowerQuery query)
		{
			var result = _iMemberService.GetPurchasingPowerMember(query);
			QueryPageModel<DTO.MemberPurchasingPower> model = new QueryPageModel<MemberPurchasingPower>();
			model.Total = result.Total;
			model.Models = AutoMapper.Mapper.Map<List<DTO.MemberPurchasingPower>>(result.Models);

			var userIds = model.Models.Select(p => p.Id).ToArray();

			var memberCategorys = _iMemberService.GetMemberBuyCategoryByUserIds(userIds);

			var grades = MemberGradeApplication.GetMemberGradeList();

			var integrals = MemberIntegralApplication.GetMemberIntegrals(userIds);
			foreach (var item in model.Models)
			{
				var intergral = integrals.Where(a => a.MemberId == item.Id).Select(a => a.HistoryIntegrals).FirstOrDefault();
				//填充等级数据
				item.GradeName = MemberGradeApplication.GetMemberGradeByIntegral(grades, intergral).GradeName;

				//填充分类数据
				var categoryNames = memberCategorys.Where(p => p.UserId == item.Id).Select(p => p.CategoryName).Take(3).ToArray();
				if (categoryNames.Length == 0)
					continue;
				item.CategoryNames = string.Join(",", categoryNames);
			}

			return model;
		}

		/// <summary>
		/// 平台获取会员分组信息
		/// </summary>
		/// <returns></returns>
		public static DTO.MemberGroup GetPlatformMemberGroup()
		{
			var memberGroupInfo = _iMemberService.GetMemberGroup();
			return MemberInfoToDto(memberGroupInfo);
		}


		/// <summary>
		/// 根据会员ID发送短信
		/// </summary>
		/// <param name="userIds"></param>
		/// <param name="sendCon"></param>
		public static void SendMsgByUserIds(long[] userIds, string sendCon)
		{
			var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Himall.Plugin.Message.SMS");

			var members = _iMemberService.GetMembers(userIds);

			string[] dests = members.Select(e => e.CellPhone).ToArray();
			List<string> phones = new List<string>();
			foreach (var dest in dests)
			{
				if (messagePlugin.Biz.CheckDestination(dest))
				{
					phones.Add(dest);
				}
			}
			if (phones.Count == 0)
			{
				throw new HimallException("可发送的对象未空！");
			}
			var siteName = SiteSettingApplication.GetSiteSettings().SiteName;
			sendCon = sendCon + "【" + siteName + "】";
			//【TODO使用队列】
			foreach (var phone in phones)
			{
				messagePlugin.Biz.SendTestMessage(phone, sendCon);
			}

			var sendRecord = new Himall.Model.SendMessageRecordInfo
			{
				ContentType = Himall.CommonModel.WXMsgType.text,
				MessageType = Himall.CommonModel.MsgType.SMS,
				SendContent = sendCon == null ? "" : sendCon,
				SendState = 1,
				SendTime = DateTime.Now,
				ToUserLabel = "会员分组"
			};
			WXMsgTemplateApplication.AddSendRecord(sendRecord);
		}


		/// <summary>
		/// 发送短信，根据搜索条件
		/// </summary>
		/// <param name="query"></param>
		/// <param name="couponIds"></param>
		public static void SendMsg(MemberPowerQuery query, string sendCon, string labelinfos = "会员分组")
		{
			var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Himall.Plugin.Message.SMS");
			int result = 0;

			//循环执行发送
			for (int i = 0; i < int.MaxValue; i++)
			{
				query.PageNo = i + 1;
				query.PageSize = 1000;
				var members = MemberApplication.GetPurchasingPowerMember(query);

				string[] dests = members.Models.Select(e => e.CellPhone).ToArray();
				List<string> phones = new List<string>();
				foreach (var dest in dests)
				{
					if (messagePlugin.Biz.CheckDestination(dest))
					{
						phones.Add(dest);
						result++;
					}
				}

				messagePlugin.Biz.SendMessages(phones.ToArray(), sendCon);

				if (members.Models.Count == 0)
					break;
			}

			//记录发送历史
			if (result > 0)
			{
				var sendRecord = new Himall.Model.SendMessageRecordInfo
				{
					ContentType = Himall.CommonModel.WXMsgType.wxcard,
					MessageType = Himall.CommonModel.MsgType.Coupon,
					SendContent = "",
					SendState = 1,
					SendTime = DateTime.Now,
					ToUserLabel = labelinfos //"会员分组"
				};
				WXMsgTemplateApplication.AddSendRecord(sendRecord);
			}
			else
			{
				throw new HimallException("此标签下无符发送的会员;");
			}

		}

		/// <summary>
		/// 指定用户发送微信消息
		/// </summary>
		/// <param name="userIds"></param>
		/// <param name="msgtype"></param>
		/// <param name="mediaid"></param>
		/// <param name="msgcontent"></param>
		public static void SendWeiMessageByUserIds(long[] userIds, string msgtype, string mediaid = "", string msgcontent = "")
		{
			Himall.CommonModel.WXMsgType type;
			if (Enum.TryParse<Himall.CommonModel.WXMsgType>(msgtype, out type))
			{

				var openIds = _iMemberService.GetOpenIdByUserIds(userIds);
                if (openIds.Count() != userIds.Length && openIds.Count() < 2)
                {
                    throw new HimallException("有用户未关注公众号，发送失败！");
                }
				var set = SiteSettingApplication.GetSiteSettings();
				var result = WXMsgTemplateApplication.SendWXMsg(openIds, type, msgcontent, mediaid, set.WeixinAppId, set.WeixinAppSecret);
				if (result.errCode == "0" || result.errMsg.Contains("success"))
				{
					SendMessageRecordInfo sendRecord = new SendMessageRecordInfo()
					{
						ContentType = type,
						MessageType = MsgType.WeiXin,
						SendContent = msgcontent,
						SendTime = DateTime.Now,
						ToUserLabel = "会员分组",
						SendState = 1
					};
					WXMsgTemplateApplication.AddSendRecord(sendRecord);
				}
				else
				{
					throw new HimallException(result.errCode);
				}
			}
		}


		/// <summary>
		/// 指定用户发送微信消息
		/// </summary>
		/// <param name="userIds"></param>
		/// <param name="msgtype"></param>
		/// <param name="mediaid"></param>
		/// <param name="msgcontent"></param>
		public static void SendWeiMessage(MemberPowerQuery query, string msgtype, string mediaid = "", string msgcontent = "")
		{
			Himall.CommonModel.WXMsgType type;
			if (Enum.TryParse<Himall.CommonModel.WXMsgType>(msgtype, out type))
			{
				List<string> allopenIds = new List<string>();
				//循环执行发送
				for (int i = 0; i < int.MaxValue; i++)
				{
					query.PageNo = i + 1;
					query.PageSize = 1000;
					var members = MemberApplication.GetPurchasingPowerMember(query);
					var userIds = members.Models.Select(p => p.Id).ToArray();
					var openIds = _iMemberService.GetOpenIdByUserIds(userIds);
					foreach (var item in openIds)
					{
						allopenIds.Add(item);
					}
					if (members.Models.Count == 0)
						break;
				}

				var set = SiteSettingApplication.GetSiteSettings();
				var result = WXMsgTemplateApplication.SendWXMsg(allopenIds, type, msgcontent, mediaid, set.WeixinAppId, set.WeixinAppSecret);
				if (result.errCode == "0" || result.errMsg.Contains("success"))
				{
					SendMessageRecordInfo sendRecord = new SendMessageRecordInfo()
					{
						ContentType = type,
						MessageType = MsgType.WeiXin,
						SendContent = msgcontent,
						SendTime = DateTime.Now,
						ToUserLabel = "会员分组",
						SendState = 1
					};
					WXMsgTemplateApplication.AddSendRecord(sendRecord);
				}
				else
				{
					throw new HimallException(result.errCode);
				}
			}
		}
		#endregion



        #region 静态方法
        /// <summary>
        /// 初始化队列消息
        /// </summary>
        public static void InitMessageQueue()
		{
			//MessageQueue.ListenTopic<long>(CommonConst.MESSAGEQUEUE_PAYSUCCESSED).OnMessageRecieved += PaySuccessed_OnMessageRecieved;
			//MessageQueue.ListenTopic<long>(CommonConst.MESSAGEQUEUE_REFUNDSUCCESSED).OnMessageRecieved += RefundSuccessed_OnMessageRecieved;
			Application.OrderApplication.OnOrderPaySuccessed += OrderService_OnOrderPaySuccessed;
			Application.RefundApplication.OnRefundSuccessed += RefundService_OnRefundSuccessed;
		}

		static void RefundService_OnRefundSuccessed(long refundId)
		{
			try
			{
				//var refundData = _refundService.GetOrderRefund(refundId);
				//var orderData = _orderService.GetOrder(refundData.OrderId);
			/////	_iMemberService.UpdateNetAmount(refundData.UserId, -refundData.Amount);//减少用户的净消费额

   //           //  OrderApplication.up
   //             if(refundData.RefundMode== OrderRefundInfo.OrderRefundMode.OrderRefund)
   //             {
   //                 _iMemberService.DecreaseMemberOrderNumber(refundData.UserId);//减少用户的下单量
   //             }
			}
			catch (Exception e)
			{
				Log.Error("处理退款成功消息出错", e);
			}
		}

		static void OrderService_OnOrderPaySuccessed(long orderId)
		{
			try
			{
				var orderData = _orderService.GetOrder(orderId);
				_iMemberService.UpdateNetAmount(orderData.UserId, orderData.TotalAmount);
                _iMemberService.IncreaseMemberOrderNumber(orderData.UserId);
				_iMemberService.UpdateLastConsumptionTime(orderData.UserId, DateTime.Now);
                OrderApplication.DealDithOrderCategoryByUserId(orderId, orderData.UserId);
            }
			catch (Exception e)
			{
				Log.Error("处理付款成功消息出错", e);
			}
		}

		///// <summary>
		///// 退款成功消息
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="args"></param>
		//static void RefundSuccessed_OnMessageRecieved(object sender, MQArgs<long> args)
		//{
		//	try
		//	{
		//		var refundId = args.Data;
		//		var refundData = _refundService.GetOrderRefund(refundId);
		//		var orderData = _orderService.GetOrder(refundData.OrderId);

		//		_iMemberService.UpdateNetAmount(orderData.UserId, -refundData.Amount);
		//	}
		//	catch(Exception e)
		//	{
		//		Log.Error("处理退款成功消息出错", e);
		//	}
		//}

		///// <summary>
		///// 付款成功消息
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="args"></param>
		//static void PaySuccessed_OnMessageRecieved(object sender, MQArgs<long> args)
		//{
		//	try
		//	{
		//		var orderId = args.Data;
		//		var orderData = _orderService.GetOrder(orderId);

		//		_iMemberService.UpdateNetAmount(orderData.UserId, orderData.TotalAmount);
		//		_iMemberService.UpdateLastConsumptionTime(orderData.UserId, DateTime.Now);
		//	}
		//	catch(Exception e)
		//	{
		//		Log.Error("处理付款成功消息出错", e);
		//	}
		//}
		#endregion

		#region 私有方法
		/// <summary>
		/// 会员分组数据实体到DTO转换
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		private static DTO.MemberGroup MemberInfoToDto(List<Model.MemberGroupInfo> model)
		{
			DTO.MemberGroup memberGroup = new MemberGroup();

			#region 活跃会员
			//一个月活跃会员
			var memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveOne).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.ActiveOne = 0;
			else
				memberGroup.ActiveOne = memberGroupInfo.Total;

			//三个月活跃会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveThree).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.ActiveThree = 0;
			else
				memberGroup.ActiveThree = memberGroupInfo.Total;

			//六个月活跃会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.ActiveSix).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.ActiveSix = 0;
			else
				memberGroup.ActiveSix = memberGroupInfo.Total;
			#endregion

			#region 沉睡会员
			// 三个月沉睡会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingThree).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.SleepingThree = 0;
			else
				memberGroup.SleepingThree = memberGroupInfo.Total;

			// 六个月沉睡会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingSix).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.SleepingSix = 0;
			else
				memberGroup.SleepingSix = memberGroupInfo.Total;

			// 九个月沉睡会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingNine).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.SleepingNine = 0;
			else
				memberGroup.SleepingNine = memberGroupInfo.Total;

			// 十二个月沉睡会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingTwelve).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.SleepingTwelve = 0;
			else
				memberGroup.SleepingTwelve = memberGroupInfo.Total;

			// 二十四个月沉睡会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.SleepingTwentyFour).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.SleepingTwentyFour = 0;
			else
				memberGroup.SleepingTwentyFour = memberGroupInfo.Total;
			#endregion

			#region 生日会员

			// 今日生日会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayToday).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.BirthdayToday = 0;
			else
				memberGroup.BirthdayToday = memberGroupInfo.Total;

			// 今月生日会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayToMonth).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.BirthdayToMonth = 0;
			else
				memberGroup.BirthdayToMonth = memberGroupInfo.Total;

			// 下月生日会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.BirthdayNextMonth).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.BirthdayNextMonth = 0;
			else
				memberGroup.BirthdayNextMonth = memberGroupInfo.Total;
			#endregion

			#region 注册会员
			// 注册会员
			memberGroupInfo = model.Where(p => p.StatisticsType == MemberStatisticsType.RegisteredMember).FirstOrDefault();
			if (memberGroupInfo == null)
				memberGroup.RegisteredMember = 0;
			else
				memberGroup.RegisteredMember = memberGroupInfo.Total;
			#endregion

			return memberGroup;
		}
		#endregion
    }
}
