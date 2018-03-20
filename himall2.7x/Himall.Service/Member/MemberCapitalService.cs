using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Entity;
using System.Transactions;
using Himall.Core;

namespace Himall.Service
{
    public class MemberCapitalService : ServiceBase, IMemberCapitalService
    {

        public ObsoletePageModel<CapitalInfo> GetCapitals(CapitalQuery query)
        {
            var capital = Context.CapitalInfo.Where(e => true);
            if (query.memberId.HasValue)
                capital = Context.CapitalInfo.Where(e => e.MemId == query.memberId);
            int total = 0;
            var page = capital.GetPage(out total, query.PageNo, query.PageSize, o => o.OrderByDescending(e => e.Id));
            ObsoletePageModel<CapitalInfo> result = new ObsoletePageModel<CapitalInfo> { Models = page, Total = total };
            return result;
        }

        public ObsoletePageModel<CapitalDetailInfo> GetCapitalDetails(CapitalDetailQuery query)
        {
            var capitalDetail = Context.CapitalDetailInfo.Where(item => item.Himall_Capital.MemId == query.memberId);
            if (query.capitalType.HasValue && query.capitalType.Value != 0)
            {
                capitalDetail = capitalDetail.Where(e => e.SourceType == query.capitalType.Value);
            }
            if (query.startTime.HasValue)
            {
                capitalDetail = capitalDetail.Where(e => e.CreateTime >= query.startTime);
            }
            if (query.endTime.HasValue)
            {
                capitalDetail = capitalDetail.Where(e => e.CreateTime < query.endTime);
            }
            int total = 0;
            var model = capitalDetail.GetPage(out total, query.PageNo, query.PageSize, p => p.OrderByDescending(e => e.CreateTime));

            ObsoletePageModel<CapitalDetailInfo> result = new ObsoletePageModel<CapitalDetailInfo> { Models = model, Total = total };
            return result;
        }

        public List<CapitalDetailInfo> GetCapitalDetailList(CapitalDetailQuery query)
        {
            var capitalDetail = Context.CapitalDetailInfo.Where(item => item.Himall_Capital.MemId == query.memberId);
            if (query.capitalType.HasValue && query.capitalType.Value != 0)
            {
                capitalDetail = capitalDetail.Where(e => e.SourceType == query.capitalType.Value);
            }
            if (query.startTime.HasValue)
            {
                capitalDetail = capitalDetail.Where(e => e.CreateTime >= query.startTime);
            }
            if (query.endTime.HasValue)
            {
                capitalDetail = capitalDetail.Where(e => e.CreateTime < query.endTime);
            }

            return capitalDetail.ToList();
        }

        public ObsoletePageModel<ApplyWithDrawInfo> GetApplyWithDraw(ApplyWithDrawQuery query)
        {
            var withDraw = Context.ApplyWithDrawInfo.AsQueryable();
            if (query.memberId.HasValue)
            {
                withDraw = withDraw.Where(e => e.MemId == query.memberId);
            }
            if (query.withDrawNo.HasValue)
            {
                withDraw = withDraw.Where(e => e.Id == query.withDrawNo);
            }
            if (query.withDrawStatus.HasValue && query.withDrawStatus.Value != 0)
            {
                withDraw = withDraw.Where(e => e.ApplyStatus == query.withDrawStatus.Value);
            }
            int total = 0;
            var model = withDraw;
            if (string.IsNullOrWhiteSpace(query.Sort))
            {
                model = withDraw.GetPage(out total, query.PageNo, query.PageSize, p => p.OrderBy(e => e.ApplyStatus).ThenByDescending(e => e.ApplyTime));
            }
            else
            {
                model = withDraw.GetPage(out total, query.PageNo, query.PageSize, p => p.OrderByDescending(e => e.ApplyTime));
            }
            ObsoletePageModel<ApplyWithDrawInfo> result = new ObsoletePageModel<ApplyWithDrawInfo> { Models = model, Total = total };
            return result;
        }

        public ApplyWithDrawInfo GetWithDraw(long id)
        {
            var model = Context.ApplyWithDrawInfo.FirstOrDefault(e => e.Id == id);
            return model;
        }

        public void ConfirmApplyWithDraw(ApplyWithDrawInfo info)
        {
            var model = Context.ApplyWithDrawInfo.FirstOrDefault(e => e.Id == info.Id);

            using (TransactionScope scope = new TransactionScope())
            {
                model.ApplyStatus = info.ApplyStatus;
                model.OpUser = info.OpUser;
                model.Remark = info.Remark;
                model.ConfirmTime = info.ConfirmTime.HasValue ? info.ConfirmTime.Value : DateTime.Now;
                Context.SaveChanges();
                if (info.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess)
                {
                    model.PayNo = info.PayNo;
                    model.PayTime = info.PayTime.HasValue ? info.PayTime.Value : DateTime.Now;
                    CapitalDetailModel capitalDetail = new CapitalDetailModel
                    {
                        Amount = -info.ApplyAmount,
                        UserId = model.MemId,
                        PayWay = info.Remark,
                        SourceType = CapitalDetailInfo.CapitalDetailType.WithDraw,
                        SourceData = info.Id.ToString()
                    };
                    AddCapital(capitalDetail);
                }
                scope.Complete();
            }
        }

        public void AddWithDrawApply(ApplyWithDrawInfo model)
        {
            Context.ApplyWithDrawInfo.Add(model);
            var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == model.MemId);
            capital.Balance -= model.ApplyAmount;
            capital.FreezeAmount = capital.FreezeAmount.HasValue ? capital.FreezeAmount.Value + model.ApplyAmount : model.ApplyAmount;
            Context.SaveChanges();
        }

        public void AddCapital(CapitalDetailModel model)
        {
            Log.Debug("AddCapital:" + model.UserId);
            var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == model.UserId);
            decimal balance = 0;
            decimal chargeAmount = 0;
            decimal freezeAmount = 0;
            StringBuilder strBuilder = new StringBuilder();
            //备注、支付方式保存到remark
            strBuilder.Append(model.Remark);
            strBuilder.Append(" ");
            strBuilder.Append(model.PayWay);
            switch (model.SourceType)
            {
                case CapitalDetailInfo.CapitalDetailType.ChargeAmount:
                    balance = chargeAmount = model.Amount;
                    break;
                case CapitalDetailInfo.CapitalDetailType.WithDraw:
                    freezeAmount = model.Amount;
                    break;
                default:
                    balance = model.Amount;
                    break;
            }
            if (capital == null)
            {
                capital = new CapitalInfo
                {
                    MemId = model.UserId,
                    Balance = balance,
                    ChargeAmount = chargeAmount,
                    FreezeAmount = freezeAmount,
                    Himall_CapitalDetail = new List<CapitalDetailInfo> { 
                        new CapitalDetailInfo { 
                            Id = CreateCode(model.SourceType),
                            Amount=balance,
                            CreateTime=DateTime.Parse(model.CreateTime),
                            SourceType=model.SourceType,
                            SourceData=model.SourceData,
                            Remark=strBuilder.ToString()
                        }
                    }
                };
                Context.CapitalInfo.Add(capital);
            }
            else
            {
                var capitalDetail = Context.CapitalDetailInfo.FirstOrDefault(e => e.Id == model.Id && e.Id != 0);
                if (capitalDetail == null)
                {
                    capitalDetail = new CapitalDetailInfo()
                    {
                        Id = CreateCode(model.SourceType),
                        Amount = model.Amount,
                        CreateTime = DateTime.Parse(model.CreateTime),
                        CapitalID = capital.Id,
                        SourceType = model.SourceType,
                        SourceData = model.SourceData,
                        Remark = strBuilder.ToString()
                    };
                    Context.CapitalDetailInfo.Add(capitalDetail);
                    capital.Balance += balance;
                    capital.ChargeAmount += chargeAmount;
                    capital.FreezeAmount += freezeAmount;
                }
            }
            Context.SaveChanges();
        }

		/// <summary>
		/// 充值成功
		/// </summary>
		/// <param name="chargeDetailId"></param>
		public void ChargeSuccess(long chargeDetailId)
		{
			var chargeDetail = this.Context.ChargeDetailInfo.FirstOrDefault(p => p.Id == chargeDetailId);
			if (chargeDetail == null)
				return;

			chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;

			var capital = this.Context.CapitalInfo.FirstOrDefault(p => p.MemId == chargeDetail.MemId);
            if (capital == null)
            {
                CapitalInfo newCapital = new CapitalInfo()
                {
                    Balance = 0,
                    ChargeAmount = 0,
                    MemId = chargeDetail.MemId,
                    Himall_CapitalDetail = new List<CapitalDetailInfo> { new CapitalDetailInfo {
                        Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount),
                        Amount = chargeDetail.ChargeAmount,
                        CreateTime = DateTime.Now,
                        SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                        SourceData = chargeDetailId.ToString()
                } }
                };
                this.Context.CapitalInfo.Add(newCapital);
            }
            else
            {
                var capitalDetail = this.Context.CapitalDetailInfo.FirstOrDefault(e => e.SourceData == chargeDetailId.ToString() && e.SourceType == CapitalDetailInfo.CapitalDetailType.ChargeAmount);
                if (capitalDetail != null)//已经处理过直接返回
                    return;
                this.Context.CapitalDetailInfo.Add(new CapitalDetailInfo()
                {
                    Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount),
                    Amount = chargeDetail.ChargeAmount,
                    CreateTime = DateTime.Now,
                    SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                    SourceData = chargeDetailId.ToString(),
                    CapitalID = capital.Id
                });
            }

            this.Context.SaveChanges();

			var sql = "UPDATE Himall_Capital SET Balance=Balance+@p1,ChargeAmount=ChargeAmount+@p1 WHERE MemId=@p0";
			this.Context.Database.ExecuteSqlCommand(sql, capital.MemId, chargeDetail.ChargeAmount);
		}

        public void UpdateCapitalAmount(long memid, decimal amount, decimal freezeAmount, decimal chargeAmount)
        {
            throw new NotImplementedException();
        }


        private static object obj = new object();
        public long CreateCode(CapitalDetailInfo.CapitalDetailType type)
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 4; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                //17位
                return long.Parse(DateTime.Now.ToString("yyMMddHHmmss") + (int)type + orderId);
            }
        }


        public CapitalDetailInfo GetCapitalDetailInfo(long id)
        {
            return Context.CapitalDetailInfo.FirstOrDefault(e => e.Id == id);
        }


        public CapitalInfo GetCapitalInfo(long userid)
        {
            return Context.CapitalInfo.FirstOrDefault(e => e.MemId == userid);
        }


        public void SetPayPwd(long memid, string pwd)
        {
            pwd = pwd.Trim();
            var salt = Guid.NewGuid().ToString("N");
            var pwdmd5 = Himall.Core.Helper.SecureHelper.MD5(Himall.Core.Helper.SecureHelper.MD5(pwd) + salt);
            var member = Context.UserMemberInfo.FirstOrDefault(e => e.Id == memid);
            if (member != null)
            {
                member.PayPwd = pwdmd5;
                member.PayPwdSalt = salt;
                Context.SaveChanges();
                string CACHE_USER_KEY = CacheKeyCollection.Member(memid);
                Core.Cache.Remove(CACHE_USER_KEY);
            }
        }

        public void RefuseApplyWithDraw(long id, ApplyWithDrawInfo.ApplyWithDrawStatus status, string opuser, string remark)
        {
            var model = Context.ApplyWithDrawInfo.FirstOrDefault(e => e.Id == id);
            model.ApplyStatus = status;
            model.OpUser = opuser;
            model.Remark = remark;
            model.ConfirmTime = DateTime.Now;
            var capital = Context.CapitalInfo.FirstOrDefault(e => e.MemId == model.MemId);
            capital.Balance = capital.Balance.Value + model.ApplyAmount;
            capital.FreezeAmount = capital.FreezeAmount - model.ApplyAmount;

            Context.SaveChanges();
        }

        public long AddChargeApply(ChargeDetailInfo model)
        {
            if (model.Id == 0)
            {
                model.Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount);
            }
            Context.ChargeDetailInfo.Add(model);
            Context.SaveChanges();
            return model.Id;
        }

        public ChargeDetailInfo GetChargeDetail(long id)
        {
            return Context.ChargeDetailInfo.FirstOrDefault(e => e.Id == id);
        }

        public void UpdateChargeDetail(ChargeDetailInfo model)
        {
            var oldmodel = Context.ChargeDetailInfo.FirstOrDefault(e => e.Id == model.Id);
            using (TransactionScope scope = new TransactionScope())
            {
                oldmodel.ChargeStatus = model.ChargeStatus;
                oldmodel.ChargeTime = model.ChargeTime.Value;
                oldmodel.ChargeWay = model.ChargeWay;
                Context.SaveChanges();
                CapitalDetailModel capitalDetail = new CapitalDetailModel
                {
                    Amount = oldmodel.ChargeAmount,
                    UserId = oldmodel.MemId,
                    PayWay = model.ChargeWay,
                    SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                    SourceData = oldmodel.Id.ToString()
                };

                AddCapital(capitalDetail);
                scope.Complete();
            }

        }

        public ObsoletePageModel<ChargeDetailInfo> GetChargeLists(ChargeQuery query)
        {
            var charges = Context.ChargeDetailInfo.AsQueryable();
            if (query.ChargeStatus.HasValue)
            {
                charges = charges.Where(e => e.ChargeStatus == query.ChargeStatus.Value);
            }
            if (query.memberId.HasValue)
            {
                charges = charges.Where(e => e.MemId == query.memberId.Value);
            }
            if (query.ChargeNo.HasValue)
            {
                charges = charges.Where(e => e.Id == query.ChargeNo.Value);
            }
            int total = 0;
            var model = charges.GetPage(out total, query.PageNo, query.PageSize, p => p.OrderByDescending(o => o.CreateTime));
            ObsoletePageModel<ChargeDetailInfo> result = new ObsoletePageModel<ChargeDetailInfo> { Models = model, Total = total };
            return result;
        }



        /// <summary>
        /// 添加店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long AddChargeDetailShop(ChargeDetailShopInfo model)
        {
            if (model.Id == 0)
            {
                model.Id = CreateCode(CapitalDetailInfo.CapitalDetailType.ChargeAmount);
            }
            Context.ChargeDetailShopInfo.Add(model);
            Context.SaveChanges();
            return model.Id;
        }

        /// <summary>
        /// 修改店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        public void UpdateChargeDetailShop(ChargeDetailShopInfo model)
        {
            var oldmodel = Context.ChargeDetailShopInfo.FirstOrDefault(e => e.Id == model.Id);
            oldmodel.ChargeStatus = model.ChargeStatus;
            oldmodel.ChargeTime = model.ChargeTime;
            oldmodel.ChargeWay = model.ChargeWay;
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取店铺充值流水信息
        /// </summary>
        /// <param name="Id">流水ID</param>
        /// <returns></returns>
        public ChargeDetailShopInfo GetChargeDetailShop(long Id)
        {
            return Context.ChargeDetailShopInfo.FirstOrDefault(e => e.Id == Id);
        }
    }
}
