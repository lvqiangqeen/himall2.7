// pages/address/address.js
var config = require("../../utils/config.js");
var app = getApp();

Page({
  data: {
    addressData: []
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    this.initData();
  },
  initData: function () {
    var that = this;
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.getUserShippingAddress), parameters, that.getUserShippingAddressData);
    })
  },
  getUserShippingAddressData: function (res) {

    var that = this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      that.setData({
        addressData: res.Data
      })
      wx.hideNavigationBarLoading();
    } else if (res.Status == "NO") {
      that.setData({
        addressData: []
      })
      wx.hideNavigationBarLoading();
    } else {
      wx.hideNavigationBarLoading();
    }
  },

  getAddressResultData: function (res) {
    var that = this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      app.getOpenId(function (openId) {
        var parameters = {
          openId: openId
        }
        wx.hideNavigationBarLoading();
        config.httpGet(app.getUrl(app.globalData.getUserShippingAddress), parameters, that.getUserShippingAddressData);
      })
    } else {
      wx.hideNavigationBarLoading();
    }
  },

  bindRadioAddressChange: function (e) {
    var that = this;
    var shippingId = e.currentTarget.dataset.shippingid;
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        shippingId: shippingId
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.setDefaultShippingAddress), parameters, that.getAddressResultData);
    })
  },

  bindDeleteAddressTap: function (e) {
    var that = this;
    var shippingId = e.currentTarget.dataset.shippingid;
    wx.showModal({
      title: '确定删除该地址吗？',
      success: function (res) {
        if (res.confirm) {
          app.getOpenId(function (openId) {
            var parameters = {
              openId: openId,
              shippingId: shippingId
            }
            wx.showNavigationBarLoading();
            config.httpGet(app.getUrl(app.globalData.delShippingAddress), parameters, that.getAddressResultData);
          })
        }
      }
    })
  },

  bindEditAddressTap: function (e) {
    var addressData = e.currentTarget.dataset.addressdata;
    console.log(JSON.stringify(addressData));
    wx.navigateTo({
      url: '../editaddress/editaddress?extra=' + JSON.stringify(addressData) + '&title=' + '编辑收货地址'
    })
  },
  gotoAddAddress: function () {
    wx.navigateTo({
      url: '../editaddress/editaddress' + '?title=' + '新增收货地址'
    })
  },
  bindAddAddressTap: function (e) {
    var that = this;
    wx.showModal({
      title: '提示',
      content: '是否使用微信收货地址',
      cancelText: '否',
      confirmText: '是',
      success: function (res) {
        if (res.confirm) {
          wx.chooseAddress({
            success: function (res) {
              if (res) {
                app.getOpenId(function (openId) {
                  //处理添加收货地址
                  var parameters = {
                    openId: openId,
                    shipTo: res.userName,
                    address: res.detailInfo,
                    cellphone: res.telNumber,
                    city: res.cityName,
                    county: res.countyName,
                  }
                  config.httpPost(app.getUrl(app.globalData.AddWXChooseAddress), parameters, function () {
                    that.initData();
                  });
                });
              }
            }
          });
        } else if (res.cancel) {
          that.gotoAddAddress();
        }
      }
    })
  }
})