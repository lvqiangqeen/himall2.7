// pages/coupon/coupon.js
var config = require("../../utils/config.js");
var app = getApp();

Page({
  data: {
    pageIndex: 0,
    couponsList: [],
    refreshSuccess: true,
    counpimg: app.getRequestUrl +'/Templates/xcxshop/images/counp-background.jpg'
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    var that = this;
    that.initData(1);
  },
initData:function(pageindex){
  var that = this;
  if (pageindex<1){
    pageindex=1;
  }
  that.setData({ pageIndex: pageindex});
  app.getOpenId(function (openId) {
    var parameters = {
      openId: openId,
      pageIndex: that.data.pageIndex ,
      pageSize: 10
    }
    wx.showNavigationBarLoading();
    config.httpGet(app.getUrl(app.globalData.LoadSiteCoupon), parameters, that.getCouponsData);
  })
},
  getCouponsData: function (res) {
    var that = this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      var couponsList = that.data.couponsList;
      if (res.Data.length > 0) {
        for (var i = 0; i < res.Data.length; i++) {
          var coupons = res.Data[i];
          var startTime = coupons.StartTime.substring(0, 10).replace(/\-/g,".");
          var closeTime = coupons.ClosingTime.substring(0, 10).replace(/\-/g, ".");
          var canUseProducts = "";
          if (coupons.CanUseProducts && coupons.CanUseProducts.length>0){
            canUseProducts = "部分商品可用"
          }else{
            canUseProducts = "全场通用"
          }
          var limitText = "";
          if (coupons.OrderUseLimit>0){
            limitText = "订单满" + coupons.OrderUseLimit + "元可用";
          }
          else{
            limitText ="订单金额无限制";
          }
          var coupons = {
            couponsDate: startTime + "~" + closeTime,
            couponsPrice: coupons.Price,
            couponsName: coupons.CouponName,
            couponsCanUseProductse: canUseProducts,
            LimitText:limitText,
            couponsId: coupons.CouponId,
            canReceive:true,
          }
          couponsList.push(coupons);
        }
        that.setData({
          pageIndex: that.data.pageIndex + 1,
          couponsList: couponsList
        });
      }
      that.setData({
        refreshSuccess: true,
      })
      wx.hideNavigationBarLoading();
    } else {
      wx.hideNavigationBarLoading();
    }
  },
  setCanReceive(CouponId, canReceive){
    var couponsList = this.data.couponsList;
    var _coupon = couponsList.find(function(item){
      return item.couponsId == CouponId;
    });
    if (_coupon){
      _coupon.canReceive = canReceive;
      this.setData({
        couponsList: couponsList
      });
    }
  } ,
   getCoupon: function (e) {
    const that = this;
    const couponid = e.currentTarget.id;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("UserGetCoupon"),
        data: {
          openId: openid,
          couponId: couponid
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            wx.showToast({
              title: result.data.Message,
              image: '../../images/succes.png'
            });
          }
          else {
            if (result.data.Message == 'NOUser') {
              wx.navigateTo({
                url: '../login/login'
              })
            }
            else {
              that.setCanReceive(couponid,false);
              wx.showToast({
                title: result.data.Message,
                image: '../../images/warning.png'
              });
            }
          }

        }
      })
    })
  },
  onReachBottom: function () {
    // 页面上拉触底事件的处理函数
    var that = this;
    var refreshSuccess = that.data.refreshSuccess;
    if (refreshSuccess == true) {
      var pageIndex = that.data.pageIndex + 1;
      that.initData(pageIndex);
    }
  }
})