// pages/SubmmitOrder/SubmmitOrder.js
var config = require("../../utils/config.js");
var app = getApp();
Page({
  data: {
     isSubmitting:false
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    const that = this;
    var productsku = options.productsku;
    var buyamount = options.buyamount;
    var frompage = options.frompage;
    var countdownid = options.countdownid;
    var shipaddressid = options.shipaddressid;
    var cartItemIds = options.cartItemIds;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("Order/GetSubmitModel"),
        data: {
          openId: openid,
          productSku: productsku?productsku:'',
          cartItemIds: cartItemIds?cartItemIds:'',
          fromPage: frompage,
          countDownId: countdownid?countdownid:0,
          buyAmount: buyamount?buyamount:0,
          shipAddressId: shipaddressid?shipaddressid:0
        },
        success: function (result) {
            if (result.data.Status == "OK") {
            const r = result.data.Data;
            var shops = r.products;
            shops.forEach(function (objs, idx, arr) {
                objs.ShopTotal = objs.ShopTotal ? objs.ShopTotal:0;
                if (objs.OneCoupons!=null)
                {
                  objs.OneCoupons.BasePrice = objs.OneCoupons.BasePrice ? objs.OneCoupons.BasePrice:0;
                  objs.totalPrice = (objs.ShopTotal - objs.OneCoupons.BasePrice).toFixed(2);
                }
                else
                { 
                  objs.totalPrice = objs.ShopTotal.toFixed(2);
                }
            });
            that.setData({
              ProductInfo: shops,
              orderAmount: r.orderAmount,
              OrderFreight: r.Freight,
              TotalAmount: r.TotalAmount,
              userIntegrals: r.userIntegrals,
              integralPerMoney: r.integralPerMoney,
              orderTotal: r.orderAmount,
              ShippingAddressInfo: r.Address,
              ProductSku: productsku,
              cartItemIds: cartItemIds,
              BuyAmount: buyamount,
              FromPage: frompage,
              CountdownId: countdownid,
              ShipAddressId: shipaddressid
            });
          }
          else {
            if (result.data.Message == 'NOUser') {
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
        }
      })
    })
  },
  onReady: function () {
    // 页面渲染完成
  },
  onShow: function () {
    // 页面显示

  },
  onHide: function () {
    // 页面隐藏
  },
  onUnload: function () {
    // 页面关闭
  },
  gotoAddress: function () {
    wx.navigateTo({
        url: '../choiceaddress/choiceaddress?productsku=' + this.data.ProductSku + '&cartItemIds=' + this.data.cartItemIds + '&buyamount=' + this.data.BuyAmount + '&frompage=' + this.data.FromPage + '&countdownid=' + this.data.CountdownId + '&shipaddressid=' + this.data.ShipAddressId
    })
  },
  addAddresstap: function () {
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
                    shippingId: 0,
                    regionId: 0,
                    isDefault: 1
                  }
                  config.httpPost(app.getUrl(app.globalData.AddWXChooseAddress), parameters, function (sd) {
                    if (sd.Status == "OK"){
                        config.httpGet(app.getUrl("ShippingAddress/GetShippingAddress"), { openId: openId, shippingId: sd.Message }, function (sdata) {
                      that.setData({
                        ShippingAddressInfo: sdata.Data.ShippingAddressInfo
                      });
                    });
                    }else{
                      wx.showToast({
                        title: sd.Message,
                        icon: 'success',
                      })
                    }
                  });
                });
              }
            }
          });
        } else if (res.cancel) {
          that.gotoAddress();
        }
      }
    })
  },

  bindRemarkInput: function (e) {
    var idx = e.currentTarget.dataset.idx;
    this.data.ProductInfo[idx].remark = e.detail.value;
    this.setData({
        ProductInfo: this.data.ProductInfo
    })
  },
  ChkUsePoint: function (e) {
    const that = this;
    var amount = (that.data.orderTotal - that.data.integralPerMoney).toFixed(2);
    if (e.detail.value) {
        that.setData({
            orderAmount: amount,
            DeductionPoints: that.data.userIntegrals
        });
    } else {
        that.setData({
            orderAmount: that.data.orderTotal,
            DeductionPoints: 0
        });
    }
  
  },
  submitOrder: function (e) {
    var that = this;
    if (that.data.isSubmitting){
        return;
    }
   
    if (that.data.ShippingAddressInfo == null || that.data.ShippingAddressInfo == undefined) {
      app.showErrorModal("请选择收货地址");
      return;
    }
    that.setData({
        isSubmitting: true
    });
    var shops = that.data.ProductInfo, couponIds = [], orderShops=[];
    shops.forEach(function (objs, idx, arr) {
        if (objs.OneCoupons!=null)
        {
          couponIds.push(objs.OneCoupons.BaseId + '_' + objs.OneCoupons.BaseType);
        }
        var orderShop = {};
        orderShop.Remark = objs.remark;
        orderShops.push(orderShop);
    });
    
    app.getOpenId(function (openid) {
        var parameters = {
            openId: openid,
            fromPage: that.data.FromPage,
            shippingId: that.data.ShippingAddressInfo.ShippingId,
            couponCode: couponIds.join(','),
            countDownId: that.data.CountdownId ? that.data.CountdownId : 0,
            buyAmount: that.data.BuyAmount ? that.data.BuyAmount : 0,
            productSku: that.data.ProductSku ? that.data.ProductSku : '',
            cartItemIds: that.data.cartItemIds ? that.data.cartItemIds : '',
            jsonOrderShops: JSON.stringify(orderShops),
            deductionPoints: that.data.DeductionPoints,
            formId: e.detail.formId,
        };
        config.httpPost(app.getUrl("Order/SubmitOrder"), parameters, function (sd) {
            if (sd.Status == "OK") {
                //如果订单金额大于0则去支付否则跳转订单中心
                if (sd.OrderTotal > 0) {
                    var orderId = sd.OrderId;
                    //支付
                    app.orderPay(orderId, 0, false);

                }
                else {
                    wx.redirectTo({
                        url: '../orderlist/orderlist?status=2'
                    })
                }
            }
            else {
                app.showErrorModal(sd.Message);
                that.setData({
                    isSubmitting: false
                });
            }
        });
        
    })
  }
})