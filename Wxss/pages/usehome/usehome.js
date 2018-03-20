// pages/usehome/usehome.js
var app = getApp();

Page({
  data: {
    userInfo: {}
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
   // var that=this;
    //that.loadData(that);
  },
  onShow: function () {
    var that = this;
    that.loadData(that);
  },
  onPullDownRefresh: function () {
    var that = this;
    app.globalData.userInfo=null;
    that.loadData(that);
  },
  loadData(that){
    var that = this;
    app.globalData.isReloadUser = '1';
    app.getUserInfo(function (userInfo) {
      that.setData({
        userInfo: userInfo
      })
    });
  },
  bindWaitPayTap:function(e){
      wx.navigateTo({
        url: "../orderlist/orderlist?status=1",
      })
  },

  bindWaitSendTap:function(e){
      wx.navigateTo({
        url: "../orderlist/orderlist?status=2",
      })
  },

  bindWaitFinishTap:function(e){
      wx.navigateTo({
        url: "../orderlist/orderlist?status=3",
      })
  },
  bindReviewTap: function (e) {
    wx.navigateTo({
      url: "../orderlist/orderlist?status=5",
    })
  },
  bindAllOrderTap:function(e){
      wx.navigateTo({
        url: "../orderlist/orderlist?status=0",
      })
  },  
  bindApply: function (e) {
    wx.navigateTo({
      url: "../applylist/applylist",
    })
  }, 
  bindMyAddressTap:function(e){
    wx.navigateTo({
      url: "../address/address",
    })
  },

  bindMyCouponsTap:function(e){
    wx.navigateTo({
      url: "../coupon/coupon",
    })
  },
  ExitLoginout:function(){
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.prcesslogout),
        data: {
          openId: openid
        },
        success: function (result) {
          wx.navigateTo({
            url: '../login/login'
          })
        }
      })
    })
  },
  bindTelPhone:function(e){
    var tel = e.currentTarget.dataset.tel;
    wx.makePhoneCall({
      phoneNumber: tel //仅为示例，并非真实的电话号码
    })
  }
})