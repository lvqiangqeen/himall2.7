var app = getApp();
Page({
  data:{
    OrderInfo:null,
    LogisticsData:null,
    SendGifts:null,
    OrderId:0,
  },
  onLoad:function(options){
    // 页面初始化 options为页面跳转所带来的参数
    this.setData({ OrderId: options.orderid});
  },
  goToProductDetail: function (e) {
    var that=this;
    var productid = e.currentTarget.dataset.productid;
    if (that.data.OrderInfo.CountDownId>0){//跳转限购
      wx.navigateTo({
     url: '../countdowndetail/countdowndetail?id=' + that.data.OrderInfo.CountDownId});
    } 
//    else if (that.data.LogisticsData.GroupBuyId>0)//跳转到团购订单
  //  { } else if (that.data.LogisticsData.PreSaleId > 0)//预售订单
  //  {}
    else{
      wx.navigateTo({
        url: '../productdetail/productdetail?id=' + productid});
    }
    
  },
  orderPay:function(e){
    var orderid = e.currentTarget.dataset.orderid;
    app.orderPay(orderid,0,false);
  },
  orderFinish:function(e){
    var that = this;
    var orderid = e.currentTarget.dataset.orderid;
    app.getOpenId(function(openid){
      wx.request({
        url: app.getUrl(app.globalData.finishOrder),
        data:{
          openId:openid,
          orderId:orderid
        },
        success: function(result) {
          if(result.data.Status=="OK"){
            wx.showModal({
              title: '提示',
              content:"确认收货成功！",
              showCancel:false,
              success: function(res) {
                if (res.confirm) {
                  wx.navigateTo({
                    url: '../orderlist/orderlist?status=0'
                  })
                }
              }
            })
          }
          else if (result.data.Message == 'NOUser') {
            wx.navigateTo({
              url: '../login/login'
            })
          }
          else{
            app.showErrorModal(result.data.Message, function (res) {
                if (res.confirm) {
                    wx.navigateTo({
                        url: '../orderlist/orderlist?status=0'
                    })
                }
            });
          }
        }
      })
    })
  },
  onReady:function(){
    // 页面渲染完成
  },
  onShow:function(){
    // 页面显示
    var that = this;
    var orderid = that.data.OrderId;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getOrderDetail),
        data: {
          openId: openid,
          orderId: orderid
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            var newList = "";
            if (r.LogisticsData.traces != null && r.LogisticsData.traces.length>0) {
              newList = r.LogisticsData;
            }
            var SendGifts = "";
            for (var k in r.Gifts) {
              if (SendGifts.length > 0) {
                SendGifts += ",";
              }
              SendGifts += r.Gifts[k].GiftName + "×" + r.Gifts[k].Quantity;
            }
            that.setData({
              OrderInfo: r,
              SendGifts: SendGifts,
              LogisticsData: newList
            })
          }
          else if (result.data.Message == 'NOUser') {
            wx.navigateTo({
              url: '../login/login'
            })
          }
          else {
              app.showErrorModal(result.data.Message, function (res) {
                  if (res.confirm) {
                      wx.navigateBack({ delta: 1 })
                  }
              });
          }
        }
      })
    })
  },
  onHide:function(){
    // 页面隐藏
  },
  onUnload:function(){
    // 页面关闭
  }
})