// pages/coupon/coupon.js
var config = require("../../utils/config.js");
var app = getApp();

Page({
  data: {
    pageIndex: 0,
    couponType: 1,
    couponsList: [],
    refreshSuccess: true,
    counpimg: app.getRequestUrl +'/storage/applet/images/counp-background.jpg',
    use_counpimg: app.getRequestUrl + '/storage/applet/images/use_counp.png',
    over_counpimg: app.getRequestUrl + '/storage/applet/images/over_counp.png'
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    var that = this;
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        pageIndex: that.data.pageIndex + 1,
        pageSize: 10,
        couponType: that.data.couponType
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.loadCoupon), parameters, that.getCouponsData);
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
            limitText = "订单满" + coupons.OrderUseLimit.toFixed(2) + "元可用";
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
            couponsId: coupons.CouponId
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

  bingNoUseTap: function (e) {
    var that = this;
    that.setData({
      pageIndex: 0,
      couponType: 1,
      couponsList: []
    })
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        pageIndex: that.data.pageIndex + 1,
        pageSize: 10,
        couponType: that.data.couponType
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.loadCoupon), parameters, that.getCouponsData);
    })
  },

  binghasUseTap: function (res) {
    var that = this;
    that.setData({
      pageIndex: 0,
      couponType: 2,
      couponsList: []
    })
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        pageIndex: that.data.pageIndex + 1,
        pageSize: 10,
        couponType: that.data.couponType
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.loadCoupon), parameters, that.getCouponsData);
    })
  },

  bingExpiredTap: function (e) {
    var that = this;
    that.setData({
      pageIndex: 0,
      couponType: 3,
      couponsList: []
    })
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        pageIndex: that.data.pageIndex + 1,
        pageSize: 10,
        couponType: that.data.couponType
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.loadCoupon), parameters, that.getCouponsData);
    })
  },

  onReachBottom: function () {
    // 页面上拉触底事件的处理函数
    var that = this;
    var refreshSuccess = that.data.refreshSuccess;
    if (refreshSuccess == true) {
      var pageIndex = that.data.pageIndex + 1;
      app.getOpenId(function (openId) {
        var parameters = {
          openId: openId,
          pageIndex: pageIndex,
          pageSize: 10,
          couponType: that.data.couponType
        }
        wx.showNavigationBarLoading();
        that.setData({
          refreshSuccess: false
        })
        config.httpGet(app.getUrl(app.globalData.loadCoupon), parameters, that.getCouponsData);
      });
    }
  }
})