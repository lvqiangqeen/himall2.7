using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class MemberIntegralConversionFactoryService : ServiceBase, IMemberIntegralConversionFactoryService
    {
        public IConversionMemberIntegralBase Create(MemberIntegral.IntegralType type, int Integral = 0)
        {
            switch (type)
            {
                case MemberIntegral.IntegralType.Reg: return new RegisteGenerateIntegral();
                case MemberIntegral.IntegralType.BindWX: return new BindWXGenerateIntegral();
                case MemberIntegral.IntegralType.Comment: return new CommentGenerateIntegral();
                case MemberIntegral.IntegralType.InvitationMemberRegiste: return new InvitationMemberRegisteGenerateIntegral();
                //case MemberIntegral.IntegralType.ProportionRebate: return new ProportionRebateGenerateIntegral();
                case MemberIntegral.IntegralType.Login: return new LoginGenerateIntegral();
                case MemberIntegral.IntegralType.Exchange: return new ExchangeGenerateIntegral(Integral);
                case MemberIntegral.IntegralType.Cancel:
                case MemberIntegral.IntegralType.SystemOper:
                case MemberIntegral.IntegralType.Consumption:
                case MemberIntegral.IntegralType.Others:
                case MemberIntegral.IntegralType.SignIn:
                case MemberIntegral.IntegralType.WeiActivity:
                    return new GenralIntegral(Integral);
                case MemberIntegral.IntegralType.Share:
                    return new OrderShareGenerateIntegral();
                default: return null;
            }
        }
    }

    #region 产生积分实体
    /// <summary>
    /// 获取注册产生的会员积分
    /// </summary>
    public class RegisteGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {

        public int ConversionIntegral()
        {

            var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Reg);
            if (null == type)
            {
                throw new Exception(string.Format("找不到因注册产生会员积分的规则"));
            }
            return type.Integral;
        }
    }

    public class GenralIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        private int _Integral = 0;
        public GenralIntegral(int Integral)
        {
            _Integral = Integral;
        }
        public int ConversionIntegral()
        {
            return _Integral;
        }
    }



    /// <summary>
    /// 获取绑定微信产生的会员积分
    /// </summary>
    public class BindWXGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.BindWX);
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到绑定微信产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取登录产生的会员积分
    /// </summary>
    public class LoginGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Login);
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到登录产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取评论订单产生的会员积分
    /// </summary>
    public class CommentGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Comment);
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到评论订单产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取邀请会员注册产生的会员积分
    /// </summary>
    public class InvitationMemberRegisteGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            var type = Context.InviteRuleInfo.FirstOrDefault();
            if (null == type)
            {
                throw new Exception(string.Format("找不到邀请会员注册产生会员积分的规则"));
            }
            return type.InviteIntegral.Value;
        }
    }


    ///// <summary>
    ///// 获取返利比例产生的会员积分
    ///// </summary>
    //public class ProportionRebateGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    //{
    //    public int ConversionIntegral()
    //    {
    //        var type = context.MemberIntegralRuleInfo.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.ProportionRebate);
    //        if (null == type)
    //        {
    //            throw new Exception(string.Format("找不到返利比例产生会员积分的规则"));
    //        }
    //        return type.Integral;
    //    }
    //}


    public class ExchangeGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        private int _Integral = 0;
        public ExchangeGenerateIntegral(int Integral)
        {
            if (Integral > 0)
                _Integral = -Integral;
        }
        public int ConversionIntegral()
        {
            return _Integral;
        }

    }
    /// <summary>
    /// 晒单获取积分
    /// </summary>
    public class OrderShareGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Share);
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到晒单积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }
    #endregion
}
