// pages/ProductDetails/ProductDetails.js
var app = getApp();
var WxParse = require('../wxParse/wxParse.js');
Page({
  data: {
    ProductId: 0,
    ProductName: '',
    MetaDescription: '',
    TempMetaDescription: '',
    ShortDescription: '',
    //SaleCounts:'',
    ShowSaleCounts: '',
    Weight: '',
    MarketPrice: '',
    IsfreeShipping: '',
    MaxSalePrice: '',
    MinSalePrice: '',
    ProductImgs: '',
    //DefaultSku:'',
    SkuItemList: '',
    Skus: '',
    Freight: '',
    Coupons: '',
    Promotes: null,
    ShowPromotesText: '',
    IsUnSale: true,
    IsOnSale:false,
    ActiveType:'',
    ActiveText: '',
    ShowPrice: '',
    backShow: 'none',
    SkuShow: 'none',
    couponShow: 'none',
    promoteShow: 'none',
    skuImg: '',
    skuPrice: 0,
    skuStock: 0,
    selectedSku: '',
    selectedSkuContent: '',
    buyAmount: 1,
    selectedskuList: [],
    isbuy: true,
    ReviewCount: 0,
    imgurl: app.getRequestUrl + '/templates/master/default/UploadImage/advertimg/20160623171012_4817.jpg'
  },
  onPullDownRefresh: function () {
    var that = this;
    that.loadData(that);
  },
  onLoad: function (options) {
    const productid = options.id;
    const that = this;
    that.setData({
      ProductId: productid
    })
    that.loadData(that);
  },
  onShareAppMessage: function () {
    var that = this;
    return {
      title: that.data.ProductName,
      path: '',
      success: function (res) {
      },
      fail: function (res) {
        // 转发失败

      }
    }
  },
  onReachBottom: function () {
    var that = this;
    if (this.data.metaDescription == null || this.data.metaDescription==''){
    var metaDescription = this.data.TempMetaDescription;
    if (metaDescription != null && metaDescription != undefined) {
      WxParse.wxParse('metaDescription', 'html', metaDescription, that);
      }
    }
  },
  loadData: function (that) {
    wx.showNavigationBarLoading(); //在标题栏中显示加载

    app.getOpenId(function (openid) {

      wx.request({
        url: app.getUrl("product/GetProductDetail"),
        data: {
          openId: openid,
          productId: that.data.ProductId
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            const r = result.data.Data;

            var tempSku = '';
            if (r.SkuItemList.length == 0) {
              tempSku = r.Skus[0].SkuId;
            }
            var _showPromote = "";
            if (r.Promotes && r.Promotes.ActivityCount && r.Promotes.ActivityCount > 0) {
              for (var key in r.Promotes) {
                var _item = r.Promotes[key];
                if (_item instanceof Array) {
                  for (var _subkey in _item) {
                    var _subitem = _item[_subkey];
                    if (_subitem && _subitem.ActivityName && _subitem.ActivityName.length > 0) {
                      if (_showPromote.length > 0) {
                        _showPromote += ",";
                      }
                      _showPromote += _subitem.ActivityName;
                    }
                  }
                }
              }
            }
            that.setData({
              ProductId: r.ProductId,
              ProductName: r.ProductName,
              ShortDescription: r.ShortDescription ? r.ShortDescription:'',
              //SaleCounts: r.SaleCounts,
              ShowSaleCounts: r.ShowSaleCounts,
              Weight: r.Weight,
              MarketPrice: r.MarketPrice,
              IsfreeShipping: r.IsfreeShipping,
              MaxSalePrice: r.MaxSalePrice,
              MinSalePrice: r.MinSalePrice,
              ProductImgs: r.ProductImgs,
              //DefaultSku:r.DefaultSku,
              SkuItemList: r.SkuItemList,
              Skus: r.Skus,
              Freight: r.Freight,
              Coupons: r.Coupons,
              Promotes: r.Promotes,
              ShowPromotesText: _showPromote,
              IsUnSale: r.IsUnSale,
              IsOnSale: r.IsOnSale,
              ActiveType: r.ActiveType,
              ActiveText:r.ActiveType >= 3 ? '暂时无法购买' :'很抱歉,此商品已下架',
              ShowPrice: r.MaxSalePrice == r.MinSalePrice ? r.MinSalePrice : r.MinSalePrice + '～' + r.MaxSalePrice,
              skuImg: r.ThumbnailUrl60,
              skuPrice: r.MinSalePrice,
              skuStock: r.Stock,
              selectedSku: tempSku,
              selectedSkuContent: '',
              ReviewCount: r.ReviewCount,
              buyAmount: 1,
              TempMetaDescription: r.MetaDescription
            })
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
        },
        complete: function () {
          // complete
          wx.hideNavigationBarLoading() //完成停止加载
          wx.stopPullDownRefresh() //停止下拉刷新
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
  getCoupon: function (e) {
    const that = this;
    const couponid = e.currentTarget.id;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("Coupon/GetUserCoupon"),
        data: {
          openId: openid,
          couponId: couponid
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            wx.showToast({
              title: '领取成功',
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
  clickCouponList: function (e) {
    const that = this;
    if (that.data.Coupons != undefined && that.data.Coupons != null && that.data.Coupons != "" && that.data.Coupons.length > 0) {
      this.setData({
        backShow: '',
        couponShow: ''
      })
    }
    else {
      wx.showToast({
        title: '暂时没有可以领取的优惠券',
        icon: 'loading'
      })
    }
  },
  clickPromoteList: function (e) {
    const that = this;
    var _promoteData = that.data.Promotes;
    if (_promoteData && _promoteData.ActivityCount && _promoteData.ActivityCount > 0) {
      this.setData({
        backShow: '',
        promoteShow: ''
      })
    }
    else {
      wx.showToast({
        title: '暂时没有进行中的满额优惠活动',
        icon: 'loading'
      })
    }
  },
  clickSku: function (e) {
    this.setData({
      backShow: '',
      SkuShow: '',
      isbuy: true
    })
  },
  addShopCart: function (e) {
    this.setData({
      backShow: '',
      SkuShow: '',
      isbuy: false,
    })
  },
  clickback: function (e) {
    this.setData({
      backShow: 'none',
      SkuShow: 'none',
      couponShow: 'none',
      promoteShow: 'none'
    })
  },
  onCouponHide: function (e) {
    this.setData({
      backShow: 'none',
      couponShow: 'none'
    })
  },
  onPromoteHide: function (e) {
    this.setData({
      backShow: 'none',
      promoteShow: 'none'
    })
  },
  onSkuHide: function (e) {
    this.setData({
      backShow: 'none',
      SkuShow: 'none'
    })
  },
  reduceAmount: function (e) {
    var amount = this.data.buyAmount;
    amount = amount - 1;
    if (amount <= 0)
      return;
    else {
      this.setData({
        buyAmount: amount
      })
    }
  },
  addAmount: function (e) {
    var amount = this.data.buyAmount;
    var stock = this.data.skuStock;
    amount = amount + 1;
    if (amount > stock)
      return;
    else {
      this.setData({
        buyAmount: amount
      })
    }
  },
  changeAmount: function (e) {
    const that = this;
    var amount = parseInt(e.detail.value);
    var stock = this.data.skuStock;
    if (isNaN(amount) || amount > stock || amount <= 0) {
      app.showErrorModal("请输入正确的数量,不能大于库存或者小于等于0");
      return;
    }
    else {
      this.setData({
        buyAmount: amount
      })
    }
  },
  doCommit: function (e) {
    var option = e.currentTarget.dataset.option, that = this, isselectsku = true;
    for (var x = 0; x < this.data.selectedskuList.length; x++) {
      if (this.data.selectedskuList[x] == undefined || this.data.selectedskuList[x] == '' || this.data.selectedskuList[x] == null) {
        isselectsku = false;
        break;
      }
    }
    if (this.data.selectedskuList.length != this.data.SkuItemList.length || !isselectsku) {
      app.showErrorModal("请选择规格");
      return;
    }
    if (this.data.skuStock==0){
        app.showErrorModal('当前所选规格库存为0');
        return;
    }
    if (this.data.buyAmount <= 0) {
      app.showErrorModal("请输入要购买的数量");
      return;
    }
    var amount = this.data.buyAmount;
    var skuid = this.data.selectedSku;
    if(option=='buy'){
        wx.redirectTo({
            url: '../submitorder/submitorder?productsku=' + skuid + '&buyamount=' + amount + '&frompage=1'
        })
    }else{
        app.getOpenId(function (openid) {
            wx.request({
                url: app.getUrl("Cart/getaddToCart"),
                data: {
                    openId: openid,
                    SkuID: skuid,
                    Quantity: amount
                },
                success: function (result) {
                    if (result.data.Status == "OK") {
                        wx.showModal({
                            title: '提示',
                            content: '加入购物车成功',
                            showCancel: false,
                            success: function (res) {
                                if (res.confirm) {
                                    that.setData({
                                        backShow: 'none',
                                        SkuShow: 'none'
                                    });
                                }
                            }
                        });
                    } else {
                        if (result.data.Message == 'NOUser') {
                            wx.navigateTo({
                                url: '../login/login'
                            })
                        }
                        else {
                            app.showErrorModal(result.data.Message);
                        }
                    }
                }
               
            });
        });
    }
  },
  onSkuClick: function (e) {
    var that = this;
    var index = e.target.dataset.indexcount;
    var valueid = e.target.id;
    var value = e.target.dataset.skuvalue;
    var selInfo = new Object();
    selInfo.valueid = valueid;
    selInfo.value = value;
    var selSku = this.data.selectedskuList;
    selSku[index] = selInfo;

    var selContent = "";
    var isAlSelected = false;
    var itemList = this.data.SkuItemList;
    if (itemList.length == selSku.length) isAlSelected = true;
    var skuId = this.data.ProductId;
    for (var i = 0; i < selSku.length; i++) {
      var info = selSku[i];
      if (info != undefined) {
        selContent += selContent == "" ? info.value : "," + info.value;
      }
    }

    var currentProductSku = null;
    console.log(itemList.length + "-" + selSku.length);
    that.data.Skus.forEach(function (item, index, array) {
      var found = true;
      // console.log("item");console.log(item);
      // console.log("index"); console.log(index);
      //  console.log("array");console.log(array);
      //  console.log("selSku");console.log(selSku);
      for (var i = 0; i < selSku.length; i++) {
        if (selSku[i] == undefined || item.SkuId.indexOf('_' + selSku[i].valueid) == -1)
          found = false;
      }
      if (found && itemList.length == selSku.length) {
        currentProductSku = item;
        skuId = item.SkuId;
        return;
      }
    });
    console.log(skuId);
    var curentItem = itemList[index];
    for (var j = 0; j < itemList[index].AttributeValue.length; j++) {
      var item = itemList[index].AttributeValue[j];
      if (item.ValueId == valueid) {
        itemList[index].AttributeValue[j].UseAttributeImage = 'selected';
      }
      else {
        itemList[index].AttributeValue[j].UseAttributeImage = 'False';
      }
    }


    // var productSku = this.data.Skus;
    // var currentProductSku=null;
    //  for(var k=0;k<productSku.length;k++){
    //    if(skuId==productSku[k].SkuId){
    //      currentProductSku = productSku[k];
    //      break;
    //    }
    //  }
    this.setData({
      selectedskuList: selSku,
      selectedSku: skuId,
      selectedSkuContent: selContent,
      SkuItemList: itemList
    })
    if (currentProductSku != null) {
      this.setData({
        skuPrice: currentProductSku.SalePrice,
        skuStock: currentProductSku.Stock,
      })
      if (currentProductSku.ThumbnailUrl40 != "" && currentProductSku.ThumbnailUrl40 != null) {
        this.setData({
          skuImg: currentProductSku.ThumbnailUrl40
        })
      }
    }

  },
})