using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;
using System.Data.Entity.Infrastructure;
using Himall.Service.Market.Business;
using System.Drawing;
using System.IO;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
namespace Himall.Service
{
    public class BonusService : ServiceBase, IBonusService
    {
        public bool CanAddBonus()
        {
            int attentionCount = Context.BonusInfo.Where(p =>
               p.Type == BonusInfo.BonusType.Attention &&  //关注红包
               !p.IsInvalid &&                                                   //没有失效
               p.EndTime > DateTime.Now).Count();                //还未结束
            if (attentionCount == 1)
            {
                return false;
            }
            return true;

        }

        public void Add(BonusInfo model, string receiveurl)
        {
            model.EndTime = model.EndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
            model.IsInvalid = false;
            model.ReceiveCount = 0;
            model.QRPath = "";
            model.ReceiveHref = "";
            model = Context.BonusInfo.Add(model);
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();

            model.ReceiveHref = receiveurl + model.Id;
            string path = GenerateQR(model.ReceiveHref);
            model.QRPath = path;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
            Task.Factory.StartNew(() => GenerateBonusDetail(model));

        }

        public void Update(BonusInfo model)
        {
            BonusInfo info = Context.BonusInfo.FindById(model.Id);
            info.Style = model.Style;
            info.Name = model.Name;
            info.MerchantsName = model.MerchantsName;
            info.Remark = model.Remark;
            info.Blessing = model.Blessing;
            info.StartTime = model.StartTime;
            info.EndTime = model.EndTime;
            info.ImagePath = model.ImagePath;
            info.Description = model.Description;
            info.IsAttention = model.IsAttention;
            info.IsGuideShare = model.IsGuideShare;
            info.ReceiveCount = model.ReceiveCount;
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        public void IncrReceiveCount(long id)
        {
            BonusInfo info = Context.BonusInfo.FindById(id);
            info.ReceiveCount++;
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;

        }

       

 

        public void Invalid(long id)
        {
            var model = Context.BonusInfo.FirstOrDefault(p => p.Id == id);
            model.IsInvalid = true;
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        public ObsoletePageModel<BonusInfo> Get(int type, int state = 1, string name = "", int pageIndex = 1, int pageSize = 20)
        {
            IQueryable<BonusInfo> query = Context.BonusInfo;
            if (type > 0)
            {
                query = Context.BonusInfo.Where(p => (int)p.Type == type);
            }
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            if (state == 1)
            {
                query = query.Where(p => p.EndTime > DateTime.Now && !p.IsInvalid);
            }
            else if (state == 2)
            {
                query = query.Where(p => p.IsInvalid || p.EndTime < DateTime.Now);
            }

            int total = 0;
            IQueryable<BonusInfo> datas = query.GetPage(out total, p => p.OrderByDescending(o => o.StartTime), pageIndex, pageSize);
            foreach (BonusInfo item in datas)
            {
                item.TypeStr = item.Type.ToDescription();
                item.StartTimeStr = item.StartTime.ToString("yyyy-MM-dd");
                item.EndTimeStr = item.EndTime.ToString("yyyy-MM-dd");
            }
            ObsoletePageModel<BonusInfo> pageModel = new ObsoletePageModel<BonusInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }

        public ObsoletePageModel<BonusReceiveInfo> GetDetail(long bonusId, int pageIndex = 1, int pageSize = 15)
        {
            if (pageIndex <= 0)
            {


                pageIndex = 1;
            }
            int total = 0;
            var bonusReceiveContext = Context.BonusReceiveInfo.Where(p => p.Himall_Bonus.Id == bonusId);
            IQueryable<BonusReceiveInfo> datas = bonusReceiveContext.GetPage(out total, p => p.OrderByDescending(o => o.ReceiveTime), pageIndex, pageSize);
            ObsoletePageModel<BonusReceiveInfo> pageModel = new ObsoletePageModel<BonusReceiveInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }

        public object Receive(long id, string openId)
        {
            bool isAttention = Context.BonusInfo.Exist(p => p.Id == id && p.IsAttention);
            if (isAttention)  //需要关注
            {
                if (!IsAttention(openId))
                {
                    //没有关注
                    return new ReceiveModel { State = ReceiveStatus.NotAttention, Price = 0 };
                }
            }


            var receives = Context.BonusReceiveInfo.Where(p => p.OpenId == openId && p.BonusId == id);
            int count = receives.Count();
            bool isShare = !(receives.Where(p => p.IsShare == true).Count() == 0);

            if (count > 0)
            {
                if (!isShare && count == 1 || count == 2) //没有分享时，有过一次领取记录，就不能领取。最大领取次数2次
                {
                    //没有领取机会
                    return new ReceiveModel { State = ReceiveStatus.Receive, Price = 0 };
                }
            }

            var query = Context.BonusReceiveInfo.Where(p => p.BonusId == id && string.IsNullOrEmpty(p.OpenId));
            var obj = query.FirstOrDefault();
            if (obj != null)
            {
                if (obj.Himall_Bonus.IsInvalid)  //失效
                {
                    return new ReceiveModel { State = ReceiveStatus.Invalid, Price = 0 };
                }

                //可以领取
                obj.Himall_Bonus.ReceiveCount += 1;
                obj.Himall_Bonus.ReceivePrice += obj.Price;
                obj.OpenId = openId;
                obj.ReceiveTime = DateTime.Now;
                obj.IsTransformedDeposit = false;
                Context.Configuration.ValidateOnSaveEnabled = false;
                Context.SaveChanges();
                Context.Configuration.ValidateOnSaveEnabled = true;

                //if( isAttention )
                //{
                //    Task.Factory.StartNew( () =>
                //    {
                //        Himall.ServiceProvider.Instance<IWXApiService>.Create.Subscribe( openId );
                //    } );
                //}

                Task.Factory.StartNew(() =>
               {
                   DepositToMember(openId, obj.Price);
                   GetSuccessSendWXMessage(obj, openId);
               });
                if (isAttention)
                {
                    return new ReceiveModel { State = ReceiveStatus.CanReceive, Price = obj.Price };
                }
                else
                {
                    return new ReceiveModel { State = ReceiveStatus.CanReceiveNotAttention, Price = obj.Price };
                }

            }
            else
            {
                //红包已被领取完
                return new ReceiveModel { State = ReceiveStatus.HaveNot, Price = 0 };
            }
        }

        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="openId"></param>
        public void GetSuccessSendWXMessage(BonusReceiveInfo data, string openId)
        {
            var user = data.Himall_Members;
            #region 发送模板消息
            //TODO:DZY[150914]此处功能需要整理，暂时只是实现功能
            var msgdata = new WX_MSGGetCouponModel();
            msgdata.first.value = "您好，您已成功领取现金红包。！";
            msgdata.first.color = "#000000";
            msgdata.toName.value = user != null ? user.Nick : "微信会员";
            msgdata.toName.color = "#000000";
            msgdata.gift.value = "现金" + data.Price.ToString() + "元";
            msgdata.gift.color = "#FF0000";
            msgdata.time.value = data.ReceiveTime.Value.ToString("yyyy-MM-dd HH:mm");
            msgdata.time.color = "#FF0000";
            msgdata.remark.value = "红包领取成功会直接计入预存款，可消费与提现。";
            msgdata.remark.color = "#000000";

            //处理url
            var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            string url = _iwxtser.GetMessageTemplateShowUrl(Himall.Core.Plugins.Message.MessageTypeEnum.ReceiveBonus);
            var wxmsgtmpl = _iwxtser.GetWeiXinMsgTemplate(Himall.Core.Plugins.Message.MessageTypeEnum.ReceiveBonus);

            var siteSetting = Himall.ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            if (wxmsgtmpl != null)
            {
                if (!string.IsNullOrWhiteSpace(wxmsgtmpl.TemplateId) && wxmsgtmpl.IsOpen)
                {
                    Himall.ServiceProvider.Instance<IWXApiService>.Create.SendMessageByTemplate(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, openId, wxmsgtmpl.TemplateId, "#000000", url, msgdata);
                }
            }
            #endregion
        }

        public string Receive(string openId)
        {
            bool IsfirstAttention = Context.OpenIdsInfo.Where(p => p.OpenId == openId).Count() <= 0;
            //不是首次关注
            if (!IsfirstAttention)
            {
                return null;
            }

            //类型必须为关注红包
            //不能为失效状态
            //还未结束
            //已经开始
            //OpenId为空
            //没有存到预存款
            var model = Context.BonusReceiveInfo.Where(p =>
                   p.Himall_Bonus.Type == BonusInfo.BonusType.Attention &&
                   !p.Himall_Bonus.IsInvalid &&
                   p.Himall_Bonus.EndTime >= DateTime.Now &&
                   p.Himall_Bonus.StartTime <= DateTime.Now &&
                   string.IsNullOrEmpty(p.OpenId) &&
                   !p.IsTransformedDeposit);



            var receive = model.FirstOrDefault();
            if (receive != null)  //存在符合条件的关注送红包
            {
                receive.Himall_Bonus.ReceiveCount += 1;
                receive.Himall_Bonus.ReceivePrice += receive.Price;
                receive.OpenId = openId;
                receive.ReceiveTime = DateTime.Now;
                receive.IsTransformedDeposit = false;
                Context.Configuration.ValidateOnSaveEnabled = false;
                Context.SaveChanges();
                Context.Configuration.ValidateOnSaveEnabled = true;

                Task.Factory.StartNew(() =>
               {
                   DepositToMember(openId, receive.Price);
                   GetSuccessSendWXMessage(receive, openId);
               });

                string content = "";
                //content = string.Format("感谢关注，您已获得预存款{0}元，通过此公众号进入商城可用预存款购买商品或提现", receive.Price);
                return content;
            }
            return null;
        }

        /// <summary>
        /// 生成红包详情
        /// </summary> 
        private void GenerateBonusDetail(BonusInfo model)
        {
            GenerateDetailContext Generate = new GenerateDetailContext(model);
            Generate.Generate();
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path)
        {
            Bitmap bi = Himall.Core.Helper.QRCodeHelper.Create(path);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Plat", "Bonus");
            string fileFullPath = Path.Combine(fileFolderPath, fileName);
            if (!Directory.Exists(fileFolderPath))
            {
                Directory.CreateDirectory(fileFolderPath);
            }
            bi.Save(fileFullPath);

            return "/Storage/Plat/Bonus/" + fileName;
        }

        public bool IsAttention(string openId)
        {
            var model = Context.OpenIdsInfo.FirstOrDefault(p => p.OpenId == openId);
            //从本地数据里判断是否关注
            if (model != null)
            {
                return model.IsSubscribe;
            }
            return IsAttentionByRPC(openId);
        }

        public void SetShare(long id, string openId)
        {
            var model = Context.BonusReceiveInfo.Where(p => p.BonusId == id && p.OpenId == openId).FirstOrDefault();
            model.IsShare = true;
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        public void SetShareByUserId(long id, bool isShare, long userId)
        {
            var model = Context.BonusReceiveInfo.Where(p => p.BonusId == id && p.IsShare == isShare).FirstOrDefault();
            model.IsShare = true;
            model.UserId = userId;
            model.ReceiveTime = DateTime.Now;
            model.IsTransformedDeposit = true;
            Context.SaveChanges();

            decimal price = GetReceivePriceByUserId(id, userId);
            WeiDepositToMember(userId, price);
        }

        /// <summary>
        /// 访问微信接口查看是否关注
        /// </summary>
        private bool IsAttentionByRPC(string openId)
        {
            var siteSetting = Himall.ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            string accessToken = "";
            if (!string.IsNullOrEmpty(siteSetting.WeixinAppId) || !string.IsNullOrEmpty(siteSetting.WeixinAppSecret))
            {
                accessToken = AccessTokenContainer.TryGetToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
            }
            else
            {
                throw new HimallException("未配置微信相关信息");
            }

            var result = UserApi.Info(accessToken, openId);
            if (result.errcode == Senparc.Weixin.ReturnCode.不合法的OpenID || result.subscribe == 0)
            {
                return false;
            }
            else if (result.errcode != 0)
            {
                throw new Exception(result.errmsg);
            }
            return result.subscribe == 1;
        }


        /// <summary>
        /// 将红包金额存到预存款里  (领取红包时执行)
        /// 只会出现一条红包记录
        /// </summary>
        private void DepositToMember(string openId, decimal price)
        {
            Entities efContext = new Entities();
            //查看用户、OpenId关联表里是否存在数据，存在则证明已经绑定过OpenId
            MemberOpenIdInfo model = efContext.MemberOpenIdInfo.Where(p => p.OpenId == openId).FirstOrDefault();
            if (model != null)
            {
                var receive = efContext.BonusReceiveInfo.Where(p => p.OpenId == openId && !p.IsTransformedDeposit).FirstOrDefault();
                receive.IsTransformedDeposit = true;
                receive.Himall_Members = efContext.UserMemberInfo.FirstOrDefault(p => p.Id == model.UserId);
                efContext.SaveChanges();

                IMemberCapitalService capitalServicer = Himall.ServiceProvider.Instance<IMemberCapitalService>.Create;
                CapitalDetailModel capita = new CapitalDetailModel
                {
                    UserId = model.UserId,
                    SourceType = CapitalDetailInfo.CapitalDetailType.RedPacket,
                    Amount = price,
                    CreateTime = ((DateTime)receive.ReceiveTime).ToString("yyyy-MM-dd HH:mm:ss")
                };
                capitalServicer.AddCapital(capita);
            }
            efContext.Dispose();
        }

        /// <summary>
        /// 微信活动 将红包金额存到预存款里  (领取红包时执行)
        /// 只会出现一条红包记录
        /// </summary>
        private void WeiDepositToMember(long userId, decimal price)
        {

            IMemberCapitalService capitalServicer = Himall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            CapitalDetailModel capita = new CapitalDetailModel
            {
                UserId = userId,
                SourceType = CapitalDetailInfo.CapitalDetailType.RedPacket,
                Amount = price,
                CreateTime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")

            };
            capitalServicer.AddCapital(capita);

        }

        /// <summary>
        /// 获取红包集合
        /// </summary>
        /// <param name="bonusType"></param>
        /// <returns></returns>
        public IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType)
        {

            return Context.BonusInfo.Where(item => (int)item.Type == (int)bonusType && item.EndTime > DateTime.Now && !item.IsInvalid).ToList();
        }


        #region  这里需要移到外部  专门用作用户绑定openid时将要执行的一系列操作
        /// <summary>
        /// 将红包金额存到预存款里  (用户注册、绑定微信时执行)
        /// 可能会存在多条红包记录
        /// </summary>
        public void DepositToRegister(long userid)
        {

            Entities efContext = new Entities();

            //查看用户、OpenId关联表里是否存在OpenId，存在则证明已经绑定过OpenId
            //获取用户所有已经绑定过的OpenId
            var openInfos = efContext.MemberOpenIdInfo.Where(p => p.UserId == userid && !string.IsNullOrEmpty(p.OpenId)).ToList();
            if (openInfos == null || openInfos.Count == 0)
            {
                return;
            }

            foreach (var o in openInfos)
            {
                DepositToRegister(o, efContext);
            }

            foreach (var o in openInfos)
            {
                DepositShopBonus(o, efContext);
            }
            efContext.Dispose();
        }

        //平台红包存储
        private void DepositToRegister(MemberOpenIdInfo openInfo, Entities efContext)
        {
            //获取某个OpenId对应的红包记录
            var receives = efContext.BonusReceiveInfo.Where(p => p.OpenId == openInfo.OpenId && !p.IsTransformedDeposit);
            var list = receives.ToList();
            List<CapitalDetailModel> capitals = new List<CapitalDetailModel>();
            //存在数据则证明有可用红包，可以存到预存款里
            if (list.Count > 0)
            {
                foreach (var model in list)
                {
                    model.IsTransformedDeposit = true;
                    model.Himall_Members = efContext.UserMemberInfo.FirstOrDefault(p => p.Id == openInfo.UserId);


                    CapitalDetailModel capital = new CapitalDetailModel()
                    {
                        UserId = openInfo.UserId,
                        SourceType = CapitalDetailInfo.CapitalDetailType.RedPacket,
                        Amount = model.Price,
                        CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    new MemberCapitalService().AddCapital(capital);
                }
                efContext.SaveChanges();
            }
            IMemberCapitalService capitalServicer = Himall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            foreach (var c in capitals)
            {
                capitalServicer.AddCapital(c);
            }
        }

        //商家红包存储
        private void DepositShopBonus(MemberOpenIdInfo openInfo, Entities efContext)
        {
            var receives = efContext.ShopBonusReceiveInfo.Where(p => p.OpenId == openInfo.OpenId && p.UserId == null).ToList();
            if (receives.Count() <= 0)
            {
                return;
            }

            DateTime now = DateTime.Now;
            foreach (var r in receives)
            {
                r.UserId = openInfo.UserId;
                r.ReceiveTime = now;
            }
            try
            {
                efContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Info("商家红包存储出错：", e);
            }
        }
        #endregion

        public BonusInfo Get(long id)
        {
            return Context.BonusInfo.FindById(id);
        }

        public decimal GetReceivePriceByOpendId(long id, string openId)
        {
            return Context.BonusReceiveInfo.Where(p => p.OpenId == openId && p.BonusId == id).FirstOrDefault().Price;
        }
        public decimal GetReceivePriceByUserId(long id, long userId)
        {
            return Context.BonusReceiveInfo.Where(p => p.UserId == userId && p.BonusId == id).OrderByDescending(t => t.ReceiveTime).FirstOrDefault().Price;
        }
        /// <summary>
        /// 获取红包剩余数量
        /// </summary>
        public string GetBonusSurplus(long bonusId)
        {
            return Context.BonusReceiveInfo.Where(p => p.BonusId == bonusId && p.UserId == null).Count().ToString();
        }


    }


}
