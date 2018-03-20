// shopcart.js
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    ShopCarts: null,
    isEdite: false,
    TotalPrice: 0.00,
    EditeText: '编辑',
    selectAllStatus: false,
    SelectskuId: [],
    SettlementText: '结算',
    isEmpty:false,
    shopSelectAll: []
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    //this.loadData(this);
  },
  loadData: function (that) {
    wx.showLoading({
      title: '正在加载',
    });

    var totalprice=parseFloat(0.00);
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("Cart/GetCartProduct"),
        data: { openId: openid },
        success: function (result) {
          if (result.data.Status == "OK") {
           var  shopcarttemp = result.data.Data;
           var shops=[];
           shopcarttemp.forEach(function (val, index, array) {
                var tmp = {};
                tmp.shopSelect = false;
                shops.push(tmp);
           });
            var isempty = (shopcarttemp == null || shopcarttemp.length <= 0 || shopcarttemp.RecordCount<=0);
            that.setData({
              isEmpty:isempty,
              ShopCarts: shopcarttemp,
              TotalPrice: 0.00,
              shopSelectAll: shops
            });

          } else if (result.data.Message == 'NOUser') {
            wx.navigateTo({
             url: '../login/login'
            })
          } else {
              app.showErrorModal(result.data.Message, function (res) {
                  if (res.confirm) {
                      wx.navigateBack({ delta: 1 })
                  }
              });
          }
        },
        complete: function () {
wx.hideLoading();
        }
      })

    });

  },
  selectList: function (e) {
    var that = this;

    var dataset = e.currentTarget.dataset,
        tempselectskuId = that.data.SelectskuId;
    var checkstatue = false;
    checkstatue = !that.data.ShopCarts[dataset.sidx][dataset.idx].selected;
    that.data.ShopCarts[dataset.sidx][dataset.idx].selected = checkstatue;

    var index = tempselectskuId.indexOf(dataset.skuid);
    if (checkstatue && index<0){
        tempselectskuId.push(dataset.skuid);
    } else if (!checkstatue && index >= 0){
      tempselectskuId.splice(index,1);
    }
    //判断该门店下的商品是否全选
    var everyShopSelect = that.data.ShopCarts[dataset.sidx].every(function (value, index, fulArr) {
        return value.selected;
    });
    if(everyShopSelect){
        that.data.shopSelectAll[dataset.sidx].shopSelect = true;
    }else{
        that.data.shopSelectAll[dataset.sidx].shopSelect = false;
    }
    //判断是否全选了
    var everySelect = that.data.shopSelectAll.every(function (value, index, fulArr) {
        return value.shopSelect;
    });
   
    that.setData({
      ShopCarts: that.data.ShopCarts,
      SelectskuId: tempselectskuId,
      selectAllStatus: everySelect,
      shopSelectAll: that.data.shopSelectAll
    });
    that.GetTotal();
  },
  
  //选择店铺
  selectShop: function (e) {
      var that = this,
          strskuId = [],
          totalmoney = 0,
          idx = e.currentTarget.dataset.idx,
          shopstatus = !that.data.shopSelectAll[idx].shopSelect,
          tempshopcart = that.data.ShopCarts[idx],
          ids = that.data.SelectskuId,
          allstatus = false;
    
      tempshopcart.forEach(function (val, index, array) {
          if (val.IsValid) {
              val.selected = shopstatus;
              if (shopstatus) {
                  strskuId.push(val.SkuId);
              }else{
                  ids && (ids = app.removeByValue(ids, val.SkuId));
              }
          }
      });
      that.data.ShopCarts[idx] = tempshopcart;
      that.data.shopSelectAll[idx].shopSelect = shopstatus;
      if (shopstatus) {
          ids = app.mergeArray(that.data.SelectskuId, strskuId);
          var everySelect = that.data.shopSelectAll.every(function(value,index,fulArr){
              return value.shopSelect;
          });
          allstatus = everySelect;
      }
      that.setData({
          ShopCarts: that.data.ShopCarts,
          selectAllStatus: allstatus,
          SelectskuId: ids?ids:[],
          shopSelectAll: that.data.shopSelectAll
      });
      that.GetTotal();
  },
  GetTotal:function(){
    var totalmoney=parseFloat(0);
    var that=this;
      var tempshopcart=that.data.ShopCarts;
      tempshopcart.forEach(function (objs, idx, arr) {
          objs.forEach(function (item, index, array) {
              if (item.selected) {
                  totalmoney = parseFloat(item.Price * item.Count) + parseFloat(totalmoney);
              }
          });

      });
    that.setData({
      TotalPrice:totalmoney.toFixed(2)
    });
  },
  selectAll: function () {
    var that = this;
    var strskuId = [];
    var totalmoney = 0;
    var allstatus = !that.data.selectAllStatus;
    var tempshopcart = that.data.ShopCarts;
    var shops = [];
    tempshopcart.forEach(function (objs, idx, arr) {
        var tmp = {};
        tmp.shopSelect = allstatus;
        shops.push(tmp);
        objs.forEach(function (val, index, array) {
            if (val.IsValid) {
                val.selected = allstatus;
                if (allstatus) {
                    strskuId.push(val.SkuId);
                }
            }
        });

    });
    that.setData({
      ShopCarts: tempshopcart,
      selectAllStatus: allstatus,
      SelectskuId: strskuId,
      shopSelectAll: shops
    });
    that.GetTotal();
  },
  SwitchEdite: function () {
    var that = this;
    var edittxt = that.data.EditeText;
    if (edittxt == "编辑") {
      that.setData({
        isEdite: true,
        EditeText: "完成",
        SettlementText: '删除',
        DelskuId: ""
      });
    } else {
      that.setData({
        isEdite: false,
        EditeText: "编辑",
        DelskuId: "",
        SettlementText: '结算'
      });
    }
  },
  countNum: function (e) {
    var that = this;
    var dataset = e.currentTarget.dataset,
        item = that.data.ShopCarts[dataset.sidx][dataset.idx],
        oldquantity = parseInt(item.Count);

    if (dataset.dotype =='minus'){
        if (oldquantity <= 1) {
            return;
        }
        that.ChangeQuantiy(that, -1, item.SkuId);
    }else{

        that.ChangeQuantiy(that, 1, item.SkuId);
    }
  },
  bindblurNum: function (e) {
    var that = this;
    var dataset = e.currentTarget.dataset,
        item = that.data.ShopCarts[dataset.sidx][dataset.idx],
        oldquantity = parseInt(item.Count);
    var num = parseInt(e.detail.value);
    // var oldstock = tempshopcart.CartItemInfo[idx].Stock;
    if (isNaN(num) || num < 1) {
      num = 1;
    }
    if (num == oldquantity) {
      return;
    }

    that.ChangeQuantiy(that, num - oldquantity, item.SkuId);
  },
  DelCarts: function (e) {
    var that = this;
    var delSkuid = e.currentTarget.dataset.skuid;
    var delSkuidStr = that.data.SelectskuId;//获取要删除的skuId；
    var totalmoney=0.00;
    if (delSkuid != "") {
      app.getOpenId(function (openid) {//删除
        wx.request({
            url: app.getUrl("Cart/GetdelCartItem"),
          data: {
            openId: openid,
            SkuIds: delSkuid
          },
          success: function (result) {
            if (result.data.Status == "OK") {
              var index = delSkuidStr.indexOf(delSkuid);
              if (index>=0){
                delSkuidStr.splice(index,1);
              }
              that.setData({ SelectskuId: delSkuidStr});
            } else {
              if (result.data.Message == 'NOUser') {
                wx.navigateTo({
                  url: '../login/login'
                })
              }
              else {
                app.showErrorModal(result.data.ErrorResponse.ErrorMsg);
              }
            }
          },
          complete: function () {
           that.loadData(that);
          }
        });
      });
    }
  },
  SettlementShopCart: function () {
    const that = this;
    var skuid = that.data.SelectskuId.join(',');
    var tempshopcart = that.data.ShopCarts;
    var delSkuidStr = that.data.SelectskuId;//获取要删除的skuId；
    if (that.data.isEdite) {
      if (skuid <= 0) {
        app.showErrorModal('请选择要删除的商品');
        return;
      }

      app.getOpenId(function (openid) {//删除
        wx.request({
          url: app.getUrl("Cart/getdelCartItem"),
          data: {
            openId: openid,
            SkuIds: skuid
          },
          success: function (result) {
            if (result.data.Status == "OK") {
          
              that.setData({
                SelectskuId: [],
                selectAllStatus:false
              });

            } else {
              if (result.data.Message == 'NOUser') {
                wx.navigateTo({
                  url: '../login/login'
                })
              }
              else {
                  app.showErrorModal(result.data.ErrorResponse.ErrorMsg);
              }
            }
          },
          complete: function () {
           that.loadData(that);
          }
        });
      });

    } else {

      if (skuid <= 0) {
        app.showErrorModal('请选择要结算的商品');
        return;
      }

      //获取所选商品的CartItemId
      var cartidArr=[],
          selectSkus = that.data.SelectskuId;
      selectSkus.forEach(function (value, ix, fullarr) {
          tempshopcart.forEach(function (objs, idx, arr) {
              objs.forEach(function (val, index, array) {
                  if (val.SkuId==value){
                      cartidArr.push(val.CartItemId);
                  }
              });

          });
      })
      
      //检查商品失效
      app.getOpenId(function (openid) {
        wx.request({
          url: app.getUrl("Cart/GetCanSubmitOrder"),
          data: {
            openId: openid,
            skus: skuid
          },
          success: function (result) {
            if (result.data.Status == "OK") {
              wx.navigateTo({
                  url: '../submitorder/submitorder?frompage=0&cartItemIds=' + cartidArr.join(',')
              });
            } else {
              if (result.data.Message == 'NOUser') {
                wx.navigateTo({
                  url: '../login/login'
                })
              }
              else {//失效，重新加载数据
                that.setData({
                  SelectskuId: [],
                  selectAllStatus: false
                });
                that.loadData(that);
              }
            }
          }
        });
      });
    }
  },
  ChangeQuantiy: function (that,quantity, skuId) {

    app.getOpenId(function (openid) {
      wx.request({
          url: app.getUrl("Cart/GetUpdateToCart"),
        data: {
          openId: openid,
          SkuID: skuId,
          Quantity: quantity
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            that.loadData(that);
          } else {
            if (result.data.Message == 'NOUser') {
              wx.navigateTo({
                url: '../login/login'
              })
            }
            else {
                app.showErrorModal(result.data.ErrorResponse.ErrorMsg);
            }
          }
        },
        complete: function () {
      
        }
      });
    });
  }, 
  goToProductDetail: function (e) {
    var that = this;
    var productid = e.currentTarget.dataset.productid;
    if (!that.data.isEdite) {
    wx.navigateTo({
        url: '../productdetail/productdetail?id=' + productid
      });
    }
  },
  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  /**
   * 生命周期函数--监听页面显示
   */
  onShow: function () {

    this.setData({
      ShopCarts: null,
      isEdite: false,
      TotalPrice: 0.00,
      EditeText: '编辑',
      selectAllStatus: false,
      SelectskuId: [],
      SettlementText: '结算',
      isEmpty:false
    });
    this.loadData(this);
  },



  /**
   * 生命周期函数--监听页面隐藏
   */
  onHide: function () {

  },

  /**
   * 生命周期函数--监听页面卸载
   */
  onUnload: function () {

  },

  /**
   * 页面相关事件处理函数--监听用户下拉动作
   */
  onPullDownRefresh: function () {

  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {

  },

  /**
   * 用户点击右上角分享
   */
  onShareAppMessage: function () {

  }
})