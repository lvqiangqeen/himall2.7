var app = getApp();
Page({
  data: {
    ProductList: null,
    SortBy: '',
    SortOrder: 'asc',
    KeyWord: '',
    CategoryId: '',
    PageIndex: 1,
    PageSize: 10,
    Num: 0,
    SortClass: '',
    CurrentProduct: null,    //当前商品信息
    CurrentSku: null,
    selectedSkuContent: null,
    isShowSkuSelectBox: false,
    index: 0,
    TotalNum: 0
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    //var keyword = options.keyword;
    var keyword = wx.getStorageSync('keyword');
    if (keyword == undefined) keyword = "";

    var categoryId = options.cid;
    if (categoryId == undefined) {
      categoryId = "";
    }
    else {
      keyword = "";
    }
    const that = this;
    that.setData({
      KeyWord: keyword,
      CategoryId: categoryId
    })
    that.loadData(that, false);
  },
  onReady: function () {
    // 页面渲染完成
  },
  onShow: function () {
    this.GetShopCart();
  },
  onHide: function () {
    // 页面隐藏
  },
  onUnload: function () {
    // 页面关闭
  },
  onSearch: function (e) {
    var that = this;
    that.setData({
      PageIndex: 1
    })
    that.loadData(that, false);
  },
  onReachBottom: function () {
    var that = this;
    var pageIndex = that.data.PageIndex + 1;
    that.setData({
      PageIndex: pageIndex
    })
    that.loadData(that, true);
  },
  bindKeyWordInput: function (e) {
    this.setData({
      KeyWord: e.detail.value
    })
  },
  onConfirmSearch: function (e) {
    var that = this;
    const keyword = e.detail.value;
    that.setData({
      KeyWord: keyword,
      PageIndex: 1
    })
    that.loadData(that, false);
  },
  bindBlurInput: function (e) {
    wx.hideKeyboard()
  },
  gotoKeyWordPage: function (e) {
    wx.navigateTo({
      url: '../search/search'
    })
  },
  onSortClick: function (e) {
    var that = this;
    var sortby = e.target.dataset.sortby;
    var suoyin = e.currentTarget.dataset.num;
    var sortorder = "asc";
    var classname = "shengxu";
    if (that.data.SortOrder == sortorder) {
      sortorder = "desc";
      classname = "jiangxu";

    }
    that.setData({
      PageIndex: 1,
      SortBy: sortby,
      SortOrder: sortorder,
      Num: suoyin,
      SortClass: classname
    })
    that.loadData(that, false);
  },
  goToProductDetail: function (e) {
    var productid = e.currentTarget.dataset.productid;
    var activeid = e.currentTarget.dataset.activeid;
    var activetype = e.currentTarget.dataset.activetype;
    var toUrl = '../productdetail/productdetail?id=' + productid;
    if (activetype == 1)
      toUrl = '../countdowndetail/countdowndetail?id=' + activeid;

    wx.navigateTo({
      url: toUrl,
    })
  },
  loadData(that, isNextPage) {
    wx.showNavigationBarLoading();
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getProducts),
        data: {
          openId: openid,
          keyword: that.data.KeyWord,
          cId: that.data.CategoryId = " " ? 0 : that.data.CategoryId,
          pageIndex: that.data.PageIndex,
          pageSize: that.data.PageSize,
          sortBy: that.data.SortBy,
          sortOrder: that.data.SortOrder
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            if (isNextPage) {
              var old = that.data.ProductList;
              old.push.apply(old, r);
              that.setData({
                ProductList: old
              })
            }
            else {
              that.setData({
                ProductList: r
              })
            }
          }
          else if (result.data.Message == 'NOUser') {
            //wx.navigateTo({
            //  url: '../login/login'
            //})
          }
          else {
              app.showErrorModal(result.data.Message, function (res) {
                  if (res.confirm) {
                      wx.navigateBack({ delta: 1 })
                  }
              });
          }
        }, complete: function () {
          wx.hideNavigationBarLoading();
        }
      })
    })
  },
  GetShopCart: function () {
    var that = this;
    var totalnum = 0;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getCartProduct),
        data: { openId: openid },
        success: function (result) {
          if (result.data.Status == "OK") {
            result.data.Data.forEach(function (cartitem, index, array) {
              if(cartitem[index]!=null)
              {
                if (parseInt(cartitem[index].Count)>0) {
                  totalnum += parseInt(array[index].Count);
                }
              }
            });

          } else if (result.data.Message == 'NOUser') {
            wx.redirectTo({
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
          that.setData({
            TotalNum: totalnum
          });
        }
      })

    });
  },
  findProductById: function (id) {
    var _pro = this.data.ProductList.find(function (d) {
      return d.ProductId == id;
    });
    return _pro;
  },
  setProductCartQuantity: function (id, num, operator) {  //修改商品购物车中存在数量
    var that = this;
    var hasEdit = false;
    var _Products = that.data.ProductList;
    var _pro = _Products.find(function (d) {
      return d.ProductId == id;
    });
    if (_pro) {
      num = parseInt(num);
      switch (operator) {
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
    if (hasEdit) {
      var newdata = {
        ProductList: _Products
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
  catchAddCart: function (e) {
    var that = this;
    var _domThis = e.currentTarget;
    var curProId = _domThis.dataset.productid;
    var curOP = _domThis.dataset.operator;
    var num = parseInt(curOP + "1");
    var opensku = _domThis.dataset.opensku;
    var _pro = that.findProductById(curProId);
    if (!_pro.HasSKU || (_pro.HasSKU && opensku == "false")) {
        if (that.data.CurrentSku.Stock == 0) {
            app.showErrorModal('当前所选规格库存为0');
            return;
        }
      var curSku = _domThis.dataset.sku;
      that.addToCart(curProId, curSku, num);
    } else {
      wx.showLoading({ title: "商品信息加载中..." });
      app.getOpenId(function (openid) {
        wx.request({
          url: app.getUrl(app.globalData.getProductSkus),
          data: {
            ProductId: curProId,
            openId: openid,
          },
          success: function (result) {
            wx.hideLoading();
            if (result.data.Status == "OK") {
              var productInfo = result.data.Data;
              var cursku = productInfo.DefaultSku;
              var selectsku = [];
              if (productInfo != null) {
                productInfo.SkuItems.forEach(function (item, index, array) {
                  item.AttributeValue.reverse();
                  item.AttributeValue[0].UseAttributeImage = 'selected';
                  var defaultsku = new Object();
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
    this.data.CurrentProduct.Skus.forEach(function (item, index, array) {
      var found = true;
      for (var i = 0; i < selSku.length; i++) {
        if (item.SkuId.indexOf('_' + selSku[i].ValueId) == -1)
          found = false;
      }
      if (found && itemList.length == selSku.length) {
        currentProductSku = item;
        skuId = item.SkuId;
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
  addToCart: function (id, skuId, quantity) {
    var that = this;
    if (!skuId || skuId.lenght < 1) {
        app.showErrorModal("请选择规格");
      return;
    }
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getUpdateToCart),
        data: {
          openId: openid,
          SkuID: skuId,
          Quantity: quantity
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
      isShowSkuSelectBox: false,
    });
  },
  showSkuDOM: function () {
    this.setData({
      isShowSkuSelectBox: true,
    });
  },
})