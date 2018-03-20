var config = require("../../utils/config.js");
var app = getApp();

Page({
  data: {
    pageIndex: 1,
    pageSize:10,
    isDataEnd:false,
    choiceProducts: [],
    refreshSuccess: true,
    keyword: "",
    TopicUrl:"",
    VersionNumber:"",
    TopicData:null,
    RequestUrl: app.getRequestUrl,
    CurrentProduct:null,    //当前商品信息
    CurrentSku:null,
    selectedSkuContent:null,
    isShowSkuSelectBox:false,
    TotalNum:0
},
onShow:function(){
  this.GetShopCart();
},
GetShopCart:function(){
  var that=this;
  var totalnum = 0;
  var tempshopcart = that.data.choiceProducts;
  app.getOpenId(function (openid) {
    wx.request({
      url: app.getUrl("Cart/GetCartProduct"),
      data: { openId: openid },
      success: function (result) {
        if (result.data.Status == "OK") {
   
          var shopcarttemp = result.data.Data;
          var changeshopcart={};
          shopcarttemp.forEach(function (cartitem, index, array) {
            cartitem.forEach(function(item,idx,arr){
              if (item.IsValid) {
                if (changeshopcart[item.Id] != undefined) {
                  changeshopcart[item.Id] = parseInt(changeshopcart[item.Id]) + parseInt(item.Count);
                } else {
                  changeshopcart[item.Id] = item.Count;
                }
                totalnum += parseInt(item.Count);
              }
            });
          });
          tempshopcart.forEach(function(item,index,array){
            if (changeshopcart[item.ProductId]!=undefined){
              item.CartQuantity = parseInt(changeshopcart[item.ProductId]);
            }else{
              item.CartQuantity = 0;
            }
          });

        } else if (result.data.Message == 'NOUser') {
          //wx.redirectTo({
          //  url: '../login/login'
          //})
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
        if (tempshopcart!=null){
          that.setData({
            choiceProducts: tempshopcart,
            TotalNum: totalnum
          });
        }
      }
    })

  });
},
  onLoad: function (options) {
    // 生命周期函数--监听页面加载
    var that = this;
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,//"oGbUa0R09MxweCW1VWcYVpk4Dgm4"
      }
      wx.showNavigationBarLoading();
      config.httpGet(app.getUrl(app.globalData.getIndexData), parameters, that.getHomeData);
    });
  },
  ClickSwiper:function(e){
    var that=this;
    var urllink= e.currentTarget.dataset.link;
    var showtype = e.currentTarget.dataset.showtype;
    that.JumpUrlByType(showtype,urllink);
  },
  JumpUrlByType:function(typeId,urls){
    switch(typeId){
      case 10:
        wx.navigateToMiniProgram({
          appId: '',
          extarData: { },
          envVersion: 'develop',
          success(res) {
            // 打开成功
          }
        })
      break;
      case 23:
        wx.makePhoneCall({
          phoneNumber: urls //仅为示例，并非真实的电话号码
        })
      break;
      case 7:
        wx.switchTab({
          url: urls
        })
        break;
      case 8:
        wx.switchTab({
          url: urls
        })
        break;
      default:
        wx.navigateTo({
          url: urls
        });
      break;
    };
  },
  onShareAppMessage: function () {
    return {
      title: '首页',
      path: '',
      success: function (res) {
      },
      fail: function (res) {
        // 转发失败
      }
    }

  },
  getHomeData: function (res) {
    var that = this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      });
      return;
    } 
    if (res.Status == "OK") {
      that.getHomeProductData(that.data.pageIndex,true);
      that.setData({
        refreshSuccess: true,
        imageList: res.Data.ImgList,
        countDownList: res.Data.CountDownList,
        TopicUrl: res.Data.HomeTopicPath,
        VersionNumber: res.Data.Vid
      });
      that.CheckVersionNumber(that);
    }else{
      wx.showToast({
        title: '系统数据异常',
      })
    }
    wx.hideNavigationBarLoading();
  },
  getHomeProductData:function(pageIndex,addPageIndex){
    var that=this;
    if (addPageIndex == undefined){
      addPageIndex=false;
    }
    if (pageIndex < 1) pageIndex=1;

    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        pageIndex: pageIndex,
        pageSize: that.data.pageSize
      }
      wx.showLoading({title:"商品信息加载中..."});
      config.httpGet(app.getUrl(app.globalData.GetIndexProductData), parameters, function (res){
        if (res.Status == "OK") {
          
          var choiceProducts = that.data.choiceProducts;
          if (res.Data.ChoiceProducts.length > 0) {
            for (var i in res.Data.ChoiceProducts) {
              var prodata = res.Data.ChoiceProducts[i];
              choiceProducts.push(prodata);
            }
            var newdata = {
              choiceProducts: choiceProducts
            };
            if (!res.Data.ChoiceProducts || res.Data.ChoiceProducts.length < that.data.pageSize){
              newdata.isDataEnd=true;
            }
            if (addPageIndex){
              newdata.pageIndex = pageIndex+1;
            }
            that.setData(newdata);
          }
        }
        wx.hideLoading();
      });
    });
  },
  CheckVersionNumber:function(that){
    // var currentversion = wx.getStorageSync("versionnumber");//获取版本号
    // if (currentversion == null || currentversion == "" || currentversion == "undefined" || parseInt(currentversion) < parseInt(that.data.VersionNumber)){//需要更新
    //   wx.setStorageSync('versionnumber',that.data.VersionNumber);
    //   that.DownloadTopcis(that);
    // }else{
     
    //   that.HomeTopicData(that);
    // }
    that.DownloadTopcis(that);
  },
  DownloadTopcis:function(that){//保存数据
    wx.request({
      url: that.data.TopicUrl,
      dataType:'json',
      success:function(res){
        wx.setStorage({
          key: 'topiclist',
          data: res.data.LModules,
        });
      },
      complete:function(){
        that.HomeTopicData(that);
      }
    })
  },
  HomeTopicData:function(that){
    wx.getStorage({
      key: 'topiclist',
      success: function(res) {
        that.setData({
          TopicData:res.data
        });
      },
      complete:function(){
        console.log(that.data.TopicData);
      }
    })
  },
  bindSearchInput: function (e) {
    var keyword = e.detail.value;
    if (keyword.length > 0) {
      this.setData({
        keyword: keyword
      })
    }
  },

  bindConfirmSearchInput: function (e) {
    const keyword = e.detail.value;
    if (keyword.length > 0) {
      wx.setStorage({
        key: "keyword",
        data: keyword
      })
      wx.switchTab({
        url: "../searchresult/searchresult",
        success: function (res) {
          wx.hideKeyboard()
        }
      })
    }
  },

  bindBlurInput: function (e) {
    wx.hideKeyboard()
  },

  bindSearchAction: function (e) {
    const keyword = this.data.keyword;
    if (keyword.length > 0) {
      wx.setStorage({
        key: "keyword",
        data: keyword
      })
      wx.switchTab({
        url: "../searchresult/searchresult",
        success: function (res) {
          wx.hideKeyboard()
        }
      })
    }
  },
  gotoKeyWordPage: function (e) {
    wx.navigateTo({
      url: '../search/search'
    })
  },
  findProductById:function(id){
    var _pro = this.data.choiceProducts.find(function (d) {
      return d.ProductId == id;
    });
    return _pro;
  },
  setProductCartQuantity: function (id, num, operator){  //修改商品购物车中存在数量
      var that=this;
      var hasEdit=false;
      var _Products = that.data.choiceProducts;
      var _pro=_Products.find(function(d){
        return d.ProductId==id;
      });
      if (_pro){
        num = parseInt(num);
        switch (operator){
          case "=":
            _pro.CartQuantity = num;
          break;
          case "+":
            _pro.CartQuantity += num;
            break;
        }
        if (_pro.CartQuantity < 0) {
          _pro.CartQuantity = 0;
        }
        hasEdit = true;
      }
        if (hasEdit){
          var newdata = {
            choiceProducts: _Products
          };
          that.setData(newdata);
        }
  },
  setSkuCartQuantity: function (skuId, num, operator) {
    //修改商品失规格购物车中存在数量,只能操作this.data.CurrentProduct中的规格
    var that = this;
    var hasEdit = false;
    var _curProduct = that.data.CurrentProduct;
    if (_curProduct && _curProduct.Skus) {
      var _sku = _curProduct.Skus.find(function (d) {
        return d.SkuId == skuId;
      });
      var _cursku = that.data.CurrentSku;
      if (_sku) {
        num = parseInt(num);
        switch (operator) {
          case "=":
            _sku.CartQuantity = num;
            break;
          case "+":
            _sku.CartQuantity += num;
            break;
        }
        if (_sku.CartQuantity < 0) {
          _sku.CartQuantity = 0;
        }
        if (_cursku && _cursku.SkuId == _sku.SkuId) {
          _cursku.CartQuantity = _sku.CartQuantity;
        }
        hasEdit = true;
      }
    }
    if (hasEdit) {
      var newdata = {
        CurrentProduct: _curProduct,
        CurrentSku: _cursku,
      };
      that.setData(newdata);
    }
  },
  catchAddCart:function(e){
    var that=this;

    var _domThis = e.currentTarget;
    var curProId = _domThis.dataset.productid;
    var activeid = _domThis.dataset.activeid;
    var activetype = _domThis.dataset.activetype;
    var url = '';
    if (activetype == 1){
 wx.navigateTo({
        url: '../countdowndetail/countdowndetail?id=' + activeid
      });
      return;
    }
   

    var curOP = _domThis.dataset.operator;
    var num = parseInt(curOP+"1");
    var opensku =_domThis.dataset.opensku+'';
    var _pro = that.findProductById(curProId);
    if (!_pro.HasSKU || (_pro.HasSKU && opensku=='false')){
        if (that.data.CurrentSku.Stock==0){
            app.showErrorModal('当前所选规格库存为0');
            return;
        }
        var curSku = _domThis.dataset.sku;
        that.addToCart(curProId, curSku, num);
    } else {
      wx.showLoading({ title: "商品信息加载中..." });
      app.getOpenId(function (openid) {
        wx.request({
          url: app.getUrl("product/GetProductSkus"),
          data: { 
            ProductId: curProId,
            openId: openid,
             },
          success: function (result) {
            wx.hideLoading();
            if (result.data.Status == "OK") {
              var productInfo = result.data.Data;
              var cursku = productInfo.DefaultSku;
              var curskuselContent=null;
              var selectsku=[];
              if (productInfo!=null){
                productInfo.SkuItems.forEach(function(item,index,array){
                  item.AttributeValue.reverse();
                  item.AttributeValue[0].UseAttributeImage ='selected';
                  var defaultsku=new Object();
                  defaultsku.ValueId = item.AttributeValue[0].ValueId;
                  defaultsku.Value = item.AttributeValue[0].Value;
                  selectsku.push(defaultsku);
                });
              }

              that.setData({
                CurrentProduct: productInfo,
                CurrentSku: cursku,
                selectedskuList: selectsku,
                selectedSku: cursku.SkuId
              });
              that.showSkuDOM();
            }
          },
          complete: function () {
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
    selInfo.ValueId = valueid;
    selInfo.Value = value;
    var selSku = this.data.selectedskuList;
    selSku[index] = selInfo;

    var selContent = "";
    var isAlSelected = false;
    var tempcurrentproduct = this.data.CurrentProduct;
    var itemList = this.data.CurrentProduct.SkuItems;
    if (tempcurrentproduct.SkuItems.length == selSku.length) isAlSelected = true;
    var skuId = tempcurrentproduct.ProductId;
    for (var i = 0; i < selSku.length; i++) {
      var info = selSku[i];
      if (info != undefined) {
        selContent += selContent == "" ? info.Value : "," + info.Value;
        skuId += "_" + info.ValueId;
      }
    }
    //var curentItem = itemList[index];
    for (var j = 0; j < tempcurrentproduct.SkuItems[index].AttributeValue.length; j++) {
      var item = tempcurrentproduct.SkuItems[index].AttributeValue[j];
      if (item.ValueId == valueid) {
        tempcurrentproduct.SkuItems[index].AttributeValue[j].UseAttributeImage = 'selected';
      }
      else {
        tempcurrentproduct.SkuItems[index].AttributeValue[j].UseAttributeImage = 'False';
      }
    }



    var currentProductSku = null;
 
    this.data.CurrentProduct.Skus.forEach(function(item,index,array){
      var found=true;
      for (var i = 0; i < selSku.length; i++) {
        if (selSku[i] ==undefined||item.SkuId.indexOf('_' + selSku[i].ValueId) == -1)
          found = false;
      }
      if (found && itemList.length == selSku.length) {
        currentProductSku = item;
        skuId=item.SkuId;
        that.data.buyAmount = item.CartQuantity > 0 ? item.CartQuantity : 1;
        return;
      }
    });

   
    this.setData({
      selectedskuList: selSku,
      selectedSku: skuId,
      selectedSkuContent: selContent,
      SkuItemList: itemList,
      CurrentProduct: tempcurrentproduct,
      CurrentSku: currentProductSku

    })
  },
  addToCart: function (id,skuId, quantity){
    var that=this;
    if(!skuId || skuId.lenght<1){
        app.showErrorModal("请选择规格");
      return;
    }
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getUpdateToCart),
        data: {
          openId: openid,
          SkuID: skuId,
          Quantity: quantity,
          GiftID: 0
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            that.setProductCartQuantity(id, quantity, "+");
            that.setSkuCartQuantity(skuId, quantity, "+");
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
          var totalnum = parseInt(that.data.TotalNum);
          that.setData({
            TotalNum: totalnum + parseInt(quantity)
          });
        }
      });
    });
  },
  hideSkuDOM: function () {
    this.setData({
      isShowSkuSelectBox:false,
    });
  },
  showSkuDOM: function () {
    this.setData({
      isShowSkuSelectBox: true,
    });
  },
  bindCountDownTap: function (e) {
    var countdownid = e.currentTarget.dataset.countdownid;
    wx.navigateTo({
      url: '../countdowndetail/countdowndetail?id=' + countdownid
    })
  },

  bindGoodsTap: function (e) {
    var productid = e.currentTarget.dataset.productid;
    var activeid = e.currentTarget.dataset.activeid;
    var activetype = e.currentTarget.dataset.activetype;
    var toUrl = '../productdetail/productdetail?id=' + productid;
    if(activetype==1)
      toUrl = '../countdowndetail/countdowndetail?id=' + activeid;

    wx.navigateTo({
      url: toUrl,
    })
  },

  onReachBottom: function () {
    // 页面上拉触底事件的处理函数
    var that = this;
    var refreshSuccess = that.data.refreshSuccess;
    if (refreshSuccess == true) {
      var pageIndex = that.data.pageIndex;
        that.getHomeProductData(pageIndex,true);
    }
  }
})