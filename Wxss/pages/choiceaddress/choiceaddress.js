var config = require("../../utils/config.js");
var app = getApp();
Page({
  data:{
    ProductSku:'',
    BuyAmount:0,
    FromPage:'',
    CountdownId:'',
    ShipAddressId:'',
    AddressList:null,
    AddressCount:0,
    DelshipId:0,
    IsCheck:0,
    jumpUrl:"",
  },
 

  onLoad:function(options){
    // 页面初始化 options为页面跳转所带来的参数
    const that = this;
    var productsku = options.productsku;
    var cartItemIds = options.cartItemIds;
    var buyamount = options.buyamount;
    var frompage = options.frompage;
    var countdownid = options.countdownid;
    var shipaddressid = options.shipaddressid;
    var jumpUrl = '../editaddress/editaddress?Source=choiceaddress&productsku=' + productsku + '&cartItemIds=' + cartItemIds + '&buyamount=' + buyamount + '&frompage=' + frompage + '&countdownid=' + countdownid;
    that.setData({
      jumpUrl: jumpUrl,
      ProductSku: productsku,
      cartItemIds: cartItemIds,
      BuyAmount: buyamount,
      FromPage: frompage,
      CountdownId: countdownid,
      ShipAddressId: shipaddressid
    })
    that.initData();

  },
  initData:function(){
    var that=this;
    app.getOpenId(function (openid) {

      wx.request({
        url: app.getUrl("ShippingAddress/GetList"),
        data: {
          openId: openid
        },
        success: function (result) {
            var r = result.data.Data;
            if (result.data.Status == "OK") {
                that.setData({
                    AddressCount: r == "[]" ? 0 : r.length,
                    AddressList: r
                })   
            }else{
                wx.redirectTo({
                    url: jumpUrl
                })
            }
        }
      })
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
            that.setData({
              DelshipId:shippingId            
            });
            wx.showNavigationBarLoading();
            config.httpGet(app.getUrl(app.globalData.delShippingAddress), parameters, that.getAddressResultData);
          })
        }
      }
    })
  },
  getAddressResultData:function(res){
    var that = this;
    if (res.Message == 'NOUser') {
      wx.redirectTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      app.getOpenId(function (openId) {
        var parameters = {
          openId: openId
        }
        wx.hideNavigationBarLoading();
        var oldaddress = that.data.AddressList;
        var newaddress = oldaddress.filter(function (item, index, array) {
          return item.ShippingId != that.data.DelshipId;
        });
        that.setData({
          AddressList: newaddress
        });

        //config.httpGet(app.getUrl(app.globalData.getUserShippingAddress), parameters, that.getUserShippingAddressData);
      })
    } else {
      wx.hideNavigationBarLoading();
    }
  },
  bindEditAddressTap:function(e){
    console.log(e);
    var addressData = e.currentTarget.dataset.addressdata;
    var that=this;
    if(this.data.IsCheck==0){
    wx.redirectTo({
      url: '../editaddress/editaddress?extra=' + JSON.stringify(addressData) + '&title=' + '编辑收货地址' +'&Source=choiceaddress&'+
      'productsku=' + that.data.ProductSku + '&cartItemIds=' + that.data.cartItemIds + '&buyamount=' + that.data.BuyAmount + '&frompage=' + that.data.FromPage + '&countdownid=' + that.data.CountdownId
    })
    }
  },
  onReady:function(){
    // 页面渲染完成
  },
  onShow:function(){
    // 页面显示
  },
  onHide:function(){
    // 页面隐藏
  },
  onUnload:function(){
    // 页面关闭
  },
  //点击添加收货地址
  onAddShippingAddress:function(e){
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
  },
  gotoAddAddress:function(){
    wx.redirectTo({
        url: '../editaddress/editaddress?Source=choiceaddress&productsku=' + this.data.ProductSku + '&cartItemIds=' + this.data.cartItemIds + '&buyamount=' + this.data.BuyAmount + '&frompage=' + this.data.FromPage + '&countdownid=' + this.data.CountdownId + '&title=' + '新增收货地址'
    })
  },
  onAddressCheck:function(e){
    var that = this;
    console.log(e);
    var shipid = e.detail.value;
    that.data.IsCheck=1;
    app.getOpenId(function (openid) {
        wx.request({
            url: app.getUrl("ShippingAddress/GetSetDefault"),
            data: {
                openId: openid,
                shippingId: shipid
            },
            success: function (result) {
                if (result.data.Status == "OK") {
                    wx.redirectTo({
                        url: '../submitorder/submitorder?productsku=' + that.data.ProductSku + '&cartItemIds=' + that.data.cartItemIds + '&buyamount=' + that.data.BuyAmount + '&frompage=' + that.data.FromPage + '&countdownid=' + that.data.CountdownId + '&shipaddressid=' + shipid
                    })
                }
            }
        })
    })
    
    /*wx.setStorage({
      key: "submitShipAddressId",
      data: shipid,
      success: function () {
        wx.navigateBack({delta: 1})
      }
    })*/
  }
})