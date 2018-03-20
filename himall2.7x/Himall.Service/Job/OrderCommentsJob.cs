using Himall.Model;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Himall.Service.Job
{
    public class OrderCommentsJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Himall.Core.Log.Debug("OrderCommentsJob : checkDate=" + DateTime.Now);
            try
            {

                using (TransactionScope transaction = new TransactionScope())
                {
                    Entity.Entities entity = new Entity.Entities();

                    //获取所有的评分数据                    
                    var lstOrderComments = (from p in entity.OrderCommentInfo
                                            group p by p.ShopId into g
                                            select new OrderCommentsModel
                                            {
                                                ShopId = g.Key,
                                                AvgDeliveryMark = g.Average(p => p.DeliveryMark),
                                                AvgPackMark = g.Average(p => p.PackMark),
                                                AvgServiceMark = g.Average(p => p.ServiceMark),
                                                CategoryIds = entity.BusinessCategoryInfo.Where(c => c.ShopId == g.Key).Select(c => c.CategoryId).ToList()
                                            }
                                   ).ToList();



                    foreach (var item in lstOrderComments)
                    {
                        //获取同行业的店铺
                        List<OrderCommentsModel> peerShops = new List<OrderCommentsModel>();

                        foreach (var cId in item.CategoryIds)
                        {
                            var shops = lstOrderComments.Where(c => c.CategoryIds.Contains(cId)).Select(c => c);
                            if (shops != null && shops.Count() > 0)
                            {
                                peerShops.AddRange(shops);
                            }

                        }
                        var avgPackMarkPeerShops = peerShops.Count != 0 ? peerShops.Average(c => c.AvgPackMark) : 0d;
                        var avgDeliveryMarkPeerShops = peerShops.Count != 0 ? peerShops.Average(c => c.AvgDeliveryMark) : 0d;
                        var avgServiceMarkPeerShops = peerShops.Count != 0 ? peerShops.Average(c => c.AvgServiceMark) : 0d;


                        var productAndDescriptionMax = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Max(c => c.AvgPackMark) : 0;
                        var productAndDescriptionMin = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Min(c => c.AvgPackMark) : 0;
                        var sellerServiceAttitudeMax = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Max(c => c.AvgServiceMark) : 0;
                        var sellerServiceAttitudeMin = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Min(c => c.AvgServiceMark) : 0;
                        var sellerDeliverySpeedMax = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Max(c => c.AvgDeliveryMark) : 0;
                        var sellerDeliverySpeedMin = peerShops.Count != 0 ? lstOrderComments.Where(c => peerShops.Where(o => o.ShopId == c.ShopId).Count() > 0).Min(c => c.AvgDeliveryMark) : 0;
                        //宝贝与描述相符
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription,
                            CommentValue = (decimal)item.AvgPackMark
                        });

                        //宝贝与描述相符 同行比较
                        Save(entity, new StatisticOrderCommentsInfo
                         {
                             ShopId = item.ShopId,
                             CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer,
                             CommentValue = (decimal)avgPackMarkPeerShops
                         });

                        //宝贝与描述相符 同行业商家最高得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax,
                            CommentValue = (decimal)productAndDescriptionMax
                        });

                        //宝贝与描述相符 同行业商家最低得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin,
                            CommentValue = (decimal)productAndDescriptionMin
                        });

                        //卖家的服务态度 
                        Save(entity, new StatisticOrderCommentsInfo
                         {
                             ShopId = item.ShopId,
                             CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude,
                             CommentValue = (decimal)item.AvgServiceMark
                         });
                        //卖家的服务态度  同行业比对
                        Save(entity, new StatisticOrderCommentsInfo
                         {
                             ShopId = item.ShopId,
                             CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer,
                             CommentValue = (decimal)avgServiceMarkPeerShops
                         });

                        //卖家服务态度 同行业商家最高得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax,
                            CommentValue = (decimal)sellerServiceAttitudeMax
                        });
                        //卖家服务态度 同行业商家最低得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin,
                            CommentValue = (decimal)sellerServiceAttitudeMin
                        });

                        //卖家的发货速度 
                        Save(entity, new StatisticOrderCommentsInfo
                         {
                             ShopId = item.ShopId,
                             CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed,
                             CommentValue = (decimal)item.AvgDeliveryMark
                         });
                        //卖家的发货速度  同行业比对
                        Save(entity, new StatisticOrderCommentsInfo
                         {
                             ShopId = item.ShopId,
                             CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer,
                             CommentValue = (decimal)avgDeliveryMarkPeerShops
                         });
                        //卖家发货速度 同行业商家最高得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax,
                            CommentValue = (decimal)sellerDeliverySpeedMax
                        });
                        //卖家发货速度 同行业商家最低得分
                        Save(entity, new StatisticOrderCommentsInfo
                        {
                            ShopId = item.ShopId,
                            CommentKey = StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin,
                            CommentValue = (decimal)sellerDeliverySpeedMin
                        });
                    }



                    int rows = entity.SaveChanges();
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        private void Save(Entity.Entities entity, StatisticOrderCommentsInfo comment)
        {

            var exists = entity.StatisticOrderCommentsInfo.Where(c => c.ShopId == comment.ShopId && c.CommentKey == comment.CommentKey).FirstOrDefault();
            if (exists == null)
            {
                var shop = entity.ShopInfo.Where(c => c.Id == comment.ShopId).FirstOrDefault();
                if (shop != null)
                {
                    comment.Himall_Shops = shop;
                    entity.StatisticOrderCommentsInfo.Add(comment);
                }

            }
            else
            {
                exists.CommentValue = comment.CommentValue;
            }
        }

        public class OrderCommentsModel
        {
            public double AvgPackMark { get; set; }
            public double AvgDeliveryMark { get; set; }

            public double AvgServiceMark { get; set; }

            public long ShopId { get; set; }

            public IList<long> CategoryIds { get; set; }
        }
    }
}
