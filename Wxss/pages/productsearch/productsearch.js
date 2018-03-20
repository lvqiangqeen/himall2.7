// /pages/productsearch/productsearch.js

var app = getApp();
Page({
  data: {
    Css: { LHeight: 0, FirstIndex: 0, SecondIndex: 0, SortIndex:1 },
    CategoryList: [],
    CurrentCategory:null,
    ProductList: null,
    CurrentProduct:null,
    CurrentSku:null,
    Cid:0,
    SortBy: '',
    SortOrder: 'asc',
    KeyWord: '',
    PageIndex: 1,
    PageSize: 10,
    Num: 0,
    SortClass: '',
    isShow: true,
    selectedskuList:[],
    buyAmount: 1, 
    selectedSku:'',
    SkuItemList:null,
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    const that = this;
    that.loadCategory(that);//获取商品分类
    that.loadData(that, false);
  },
  loadCategory: function (that) {//加载商品分类
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getAllCategories),
        data: {},
        success: function (result) {
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            that.setData({
              CategoryList: r,
              CurrentCategory:r[0]
            });
          }
          else if (result.data.Message == 'NOUser') {
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
      })
    })

  },
  BuyProduct:function(e){
    var that=this;
    var index = e.currentTarget.dataset.index;//商品索引
    var hassku = e.currentTarget.dataset.sku;//是否存在规格
    var productId = e.currentTarget.dataset.productid;//商品编号
    var tempproduct = that.data.ProductList;
    if(hassku==false){
      tempproduct[index].CartQuantity = 1;

    }else{//存在规格弹出规格框
      wx.request({
        url: app.getUrl(app.globalData.getProductSkus),
        data:{ProductId:productId},
        success:function(result){
          if(result.data.Status=="OK"){
            var productInfo=result.data.Data;
            that.setData({ 
              CurrentProduct: productInfo,
              CurrentSku:productInfo.DefaultSku
            });
          }
        }
      });
    }
    
    that.setData({
      isShow:false,
      ProductList:tempproduct
    })
  },
  minusCount:function(e){
    var index = e.currentTarget.dataset.index;
    var tempproduct = this.data.ProductList;
    var cartquantiy=tempproduct[index].CartQuantity;

    tempproduct[index].CartQuantity =cartquantiy<=1?1:cartquantiy-1;
    
    this.setData({
      ProductList:tempproduct
    });
  },
  addCount:function(e){
    var index = e.currentTarget.dataset.index;
    var tempproduct = this.data.ProductList;
    var cartquantiy = tempproduct[index].CartQuantity;

    tempproduct[index].CartQuantity =cartquantiy + 1;

    this.setData({
      ProductList: tempproduct
    });

  },
  onSkuHide:function(e){
    that.setData({
      isShow:true
    });
  },
  onSkuClick:function(e){
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
    this.data.CurrentProduct.Skus.find(function(skuitem,index){
      if(skuId==skuitem.SkuId){
        currentProductSku=skuitem;
        return;
      }
    });
    this.setData({
      selectedskuList: selSku,
      selectedSku: skuId,
      selectedSkuContent: selContent,
      SkuItemList: itemList,
      CurrentProduct: tempcurrentproduct
      
    })
    if (currentProductSku != null) {
      this.setData({
        CurrentSku: currentProductSku
      })
    }
  },
  changeAmount:function(e){

    const that = this;
    var amount = parseInt(e.detail.value);

    var stock = this.data.CurrentSkuStock;
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
  reduceAmount:function(e){
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
    var stock = this.data.CurrentSku.Stock;
    amount = amount + 1;
    if (amount > stock)
      return;
    else {
      this.setData({
        buyAmount: amount
      })
    }
  },
  loadData: function (that, isNextPage) {
    wx.showNavigationBarLoading();
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getProducts),
        data: {
          openId: openid,
          keyword: that.data.KeyWord,
          pageIndex: that.data.PageIndex,
          pageSize: that.data.PageSize,
          sortBy: that.data.SortBy,
          sortOrder: that.data.SortOrder,
          cId: that.data.Cid
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
            wx.hideNavigationBarLoading();
          }
          else if (result.data.Message == 'NOUser') {
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
        },
        complete: function () {

          wx.getSystemInfo({
            success: function (res) {
              var l_height = res.windowHeight - 53;
              var tempCss=that.data.Css;
              tempCss.LHeight = l_height;
              that.setData({
                CSS: tempCss
              });
            }
          });
        }
      })
    })
  },
  commitBuy: function (e) {
    var that=this;
    var isselectsku = true;
    for (var x = 0; x < that.data.selectedskuList.length; x++) {
      if (this.data.selectedskuList[x] == undefined || that.data.selectedskuList[x] == '' || this.data.selectedskuList[x] == null) {
        isselectsku = false;
        break;
      }
    }
    if (this.data.SkuItemList==null||that.data.selectedskuList.length != this.data.SkuItemList.length || !isselectsku) {
      app.showErrorModal("请选择规格");
      return;
    }
    if (that.data.buyAmount <= 0) {
      app.showErrorModal("请输入要购买的数量");
      return;
    }
    var amount = this.data.buyAmount;
    var skuid = this.data.selectedSku;
   // wx.redirectTo({
     // url: '../submitorder/submitorder?productsku=' + skuid + '&buyamount=' + amount + '&frompage=signbuy'
    //})
    var tempproduct = this.data.ProductList;//获取商品列表
    tempproduct.find(function(item,index){
      if (item.ProductId == that.data.CurrentProduct.ProductId){
        item.CartQuantity+=amount;
        return;
      }
    });
    this.setData({
      ProductList:tempproduct
    });
    that.onSkuHide(e);
  },
  onSkuHide:function(e){
    this.setData({
      isShow:true,
      CurrentSku:null,
      CurrentProduct:null,
      selectedSku:'',
      buyAmount:1
    });
  },
  ChooseCategory: function (e) {
    var that = this;
    var cid = e.currentTarget.dataset.cid;//获取分类Id
    var grade = e.currentTarget.dataset.grade;
    var currentIndex=e.currentTarget.dataset.index;
    var tempCss = that.data.Css;
    if(grade=="1"){
      that.data.CategoryList.find(function (item, index) {
        tempCss.FirstIndex=currentIndex;
        tempCss.SecondIndex=0;
        if (item.cid == cid) {
          that.setData({
            CurrentCategory: item,
            Css: tempCss,
            Cid:cid
          });
        }
      });
    }else{
      tempCss.SecondIndex=currentIndex;
      that.setData({
        Css: tempCss,
        Cid:cid
      });
    }
    that.loadData(that, false);
  },
  SortClick:function(e){
    var that=this;
    var sortby = e.currentTarget.dataset.sortby;
    var currentIndex = e.currentTarget.dataset.index;
    var tempCss = that.data.Css;
    tempCss.SortIndex=currentIndex;
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
      SortClass: classname,
      Css: tempCss
    })
    that.loadData(that, false);
  },
  ChooseProduct:function(e){
    var productId = e.currentTarget.dataset.productId; 
    wx.navigateTo({
      url: '../productdetail/productdetail?id=' + productId
    })
  },
  onReachBottom: function () {
    var that = this;
    var pageIndex = that.data.PageIndex + 1;
    that.setData({
      PageIndex: pageIndex
    })
    that.loadData(that, true);
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