using Himall.CommonModel;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    public interface IMemberService : IService
    {
        string GetLogo();

        /// <summary>
        /// 注册一个会员()
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mobile">手机号码</param>
        UserMemberInfo Register(string username, string password, string mobile = "", string email = "", long introducer = 0);

        /// <summary>
        /// 注册并绑定一个会员
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像地址</param>
        UserMemberInfo Register(string username, string password, string serviceProvider, string openId, string sex = null, string headImage = null, long introducer = 0, string nickname = null, string city = null, string province = null, string unionid = null);
        /// <summary>
        /// 注册并绑定一个会员(传model)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        UserMemberInfo Register(OAuthUserModel model);
        /// <summary>
        /// 快速注册
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="openId">OpenId</param>
        /// <param name="nickName">昵称</param>
        /// <param name="realName">真实姓名</param>
        /// <param name="serviceProvider">服务提供商</param>
        /// <returns></returns>
		UserMemberInfo QuickRegister(string username, string realName, string nickName, string serviceProvider, string openId, string unionid = null, string sex = null, string headImage = null, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionopenid = null, string city = null, string province = null);

        /// <summary>
        /// 绑定会员
        /// </summary>
        /// <param name="userId">会员id</param>
        /// <param name="serviceProvider">信任登录服务提供商</param>
        /// <param name="openId">OpenId</param>
        /// <param name="headImage">头像</param>
        void BindMember(long userId, string serviceProvider, string openId, string sex = null, string headImage = null, string unionid = null, string unionopenid = null, string city = null, string province = null);
        /// <summary>
        /// 绑定会员（增加标记，是否是可以支付的openid）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="openId"></param>
        /// <param name="AppidType"></param>
        /// <param name="headImage"></param>
        void BindMember(long userId, string serviceProvider, string openId, Model.MemberOpenIdInfo.AppIdTypeEnum AppidType, string sex = null, string headImage = null, string unionid = null);

        void BindMember(OAuthUserModel model);

		/// <summary>
		/// 验证支付密码
		/// </summary>
		/// <param name="memid"></param>
		/// <param name="payPwd"></param>
		/// <returns></returns>
		bool VerificationPayPwd(long memid, string payPwd);

		/// <summary>
		/// 是否有支付密码
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		bool HasPayPwd(long id);

        /// <summary>
        /// 检查用户名是否重复
        /// </summary>
        /// <param name="username">待检查的用户名</param>
        /// <returns></returns>
        bool CheckMemberExist(string username);

        /// <summary>
        /// 检查手机号码是否重复
        /// </summary>
        /// <param name="mobile">待检查的手机号码</param>
        /// <returns></returns>
        bool CheckMobileExist(string mobile);
        /// <summary>
        /// 检查邮箱是否重复
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool CheckEmailExist(string email);

        /// <summary>
        /// 修改会员信息（旧方法）
        /// </summary>
        /// <param name="memeber"></param>
        [Obsolete("请使用UpdateMemberInfo,并在Application里规定需要更新的字段")]
        void UpdateMember(UserMemberInfo memeber);


        /// <summary>
        /// 修改会员信息的新方法（需要先在Application中先查询）
        /// </summary>
        /// <param name="model"></param>
        void UpdateMemberInfo(UserMemberInfo model);

       

        #region 分销用户关系
        /// <summary>
        /// 更改用户的推销员，并建立店铺分佣关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shareUserId">销售员用户编号</param>
        /// <param name="shopId">店铺编号,为0表示仅维护用户与推销员的关系</param>
        long UpdateShareUserId(long id, long shareUserId, long shopId = 0);
        /// <summary>
        /// 更新用户关系
        /// </summary>
        /// <param name="id">关系信息编号列表</param>
        /// <param name="userId">买家编号</param>
        void UpdateDistributionUserLink(IEnumerable<long> ids, long userId);
        #endregion

        /// <summary>
        /// 更改会员密码
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <param name="password">会员密码</param>
        void ChangePassword(long id, string password);

		/// <summary>
		/// 根据用户名修改密码
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		void ChangePassword(string name, string password);

		/// <summary>
		/// 修改支付密码
		/// </summary>
		/// <param name="id"></param>
		/// <param name="password"></param>
		void ChangePayPassword(long id, string password);

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <param name="id"></param>
        void LockMember(long id);

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <param name="id"></param>
        void UnLockMember(long id);

        /// <summary>
        /// 删除一个会员
        /// </summary>
        /// <param name="id"></param>
        void DeleteMember(long id);


        /// <summary>
        /// 批量删除会员
        /// </summary>
        /// <param name="ids">批量ID数组</param>
        void BatchDeleteMember(long[] ids);

        /// <summary>
        /// 批量锁定
        /// </summary>
        /// <param name="ids"></param>
        void BatchLock(long[] ids);

        /// <summary>
        /// 根据查询条件分页获取会员信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<UserMemberInfo> GetMembers(MemberQuery query);
		/// <summary>
		/// 根据用户id获取用户信息
        /// <para>不会加载外键关联信息</para>
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		List<UserMemberInfo> GetMembers(IEnumerable<long> userIds);
        /// <summary>
        /// 获取查询的会员信息列表
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        IQueryable<UserMemberInfo> GetMembers(bool? status, string keyWords);

        /// <summary>
        /// 获取一个会员信息
        /// </summary>
        /// <param name="id">会员ID</param>
        /// <returns></returns>
        UserMemberInfo GetMember(long id);

		/// <summary>
		/// 根据用户id和类型获取会员openid信息
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="appIdType"></param>
		/// <returns></returns>
		MemberOpenIdInfo GetMemberOpenIdInfoByuserId(long userId, MemberOpenIdInfo.AppIdTypeEnum appIdType);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        UserMemberInfo Login(string username, string password);

		/// <summary>
		/// 修改最后登录时间
		/// </summary>
		/// <param name="id"></param>
		void UpdateLastLoginDate(long id);


        //根据用户名获取用户信息
        UserMemberInfo GetMemberByName(string userName);

        /// <summary>
        ///根据用户ID从缓存获取用户信息
        /// </summary>
        /// <returns></returns>
        UserMemberInfo GetUserByCache(long id);

        /// <summary>
        /// 获取用户个人中心信息
        /// </summary>
        /// <returns></returns>
        UserCenterModel GetUserCenterModel(long id);

        /// <summary>
        /// 根据服务商返回的OpenId获取系统中对应的用户
        /// </summary>
        /// <param name="serviceProvider">服务商名称</param>
        /// <param name="openId">OpenId</param>
        /// <returns></returns>
        UserMemberInfo GetMemberByOpenId(string serviceProvider, string openId);
        UserMemberInfo GetMemberByUnionId(string serviceProvider, string UnionId);
        UserMemberInfo GetMemberByUnionId(string UnionId);
        void DeleteMemberOpenId(long userid, string openid);

        /// <summary>
        /// 从手机号或者邮箱信息来获取用户信息
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        UserMemberInfo GetMemberByContactInfo(string contact);

        /// <summary>
        /// 检查是邮箱或手机是否被占用
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="contact"></param>
        /// <param name="userType"></param>
        void CheckContactInfoHasBeenUsed(string serviceProvider, string contact, Himall.Model.MemberContactsInfo.UserTypes userType = Himall.Model.MemberContactsInfo.UserTypes.General);

        IQueryable<MemberLabelInfo> GetMembersByLabel(long labelid);

        IEnumerable<MemberLabelInfo> GetMemberLabels(long memid);
        /// <summary>
        /// 设置会员标签(移除原标签)
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        void SetMemberLabel(long userid, IEnumerable<long> labelids);
        /// <summary>
        /// 设置多会员标签，只累加，不移除原标签
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="labelids"></param>
        void SetMembersLabel(long[] userid, IEnumerable<long> labelids);
        
        IEnumerable<int> GetAllTopRegion();

        /// <summary>
        /// 通过会员等级ID获取会员消费范围
        /// </summary>
        /// <param name="gradeId"></param>
        /// <returns></returns>
        GradeIntegralRange GetMemberGradeRange(long gradeId);


        #region 会员分组业务

        /// <summary>
        /// 获取会员分组数据
        /// </summary>
        /// <returns></returns>
        List<MemberGroupInfo> GetMemberGroup();

        /// <summary>
        /// 会员购买力列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<UserMemberInfo> GetPurchasingPowerMember(MemberPowerQuery query);

         /// <summary>
        /// 批量获取会员购买类别
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        List<MemberBuyCategoryInfo> GetMemberBuyCategoryByUserIds(IEnumerable<long> userIds);


        /// <summary>
        /// 批量获取用户OPENID
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        IEnumerable<string> GetOpenIdByUserIds(IEnumerable<long> userIds);


        /// <summary>
        /// 获取平台会员数
        /// </summary>
        /// <returns></returns>
        int PlatformMemberTotal();

		/// <summary>
		/// 修改会员净消费金额
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="netAmount"></param>
		void UpdateNetAmount(long userId, decimal netAmount);


        /// <summary>
        /// 增加会员下单量
        /// </summary>
        /// <param name="userId"></param>
        void IncreaseMemberOrderNumber(long userId);

        /// <summary>
        /// 减少会员下单量
        /// </summary>
        /// <param name="userId"></param>
       void DecreaseMemberOrderNumber(long userId);

        /// <summary>
        /// 修改最后消费时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastConsumptionTime">最后消费时间</param>
        void UpdateLastConsumptionTime(long userId, DateTime lastConsumptionTime);
        #endregion


        /// <summary>
        /// 更新OpenId
        /// </summary>
        void UpdateOpenIdBindMember(MemberOpenIdInfo model);

        /// <summary>
        /// 根据用户id和平台获取会员openid信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appIdType"></param>
        /// <returns></returns>
        MemberOpenIdInfo GetMemberOpenIdInfoByuserIdAndType(long userId, string serviceProvider);



        #region 用户佣金
        /// <summary>
        /// 用户当天获取的佣金总数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        decimal GetTodayCommission(long userId);

        /// <summary>
        /// 用户总的佣金数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        decimal GetTotalCommission(long userId);

        /// <summary>
        /// 获取佣金列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<MemberCommissionInfo> GetCommissionList(long userId);

        /// <summary>
        /// 添加一条佣金记录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        int AddCommission(MemberCommissionInfo info);
        #endregion
    }
}

