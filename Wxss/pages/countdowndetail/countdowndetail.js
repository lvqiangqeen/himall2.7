var app = getApp();
var WxParse = require('../wxParse/wxParse.js');
Page({
  data:{
    CountDownId:0,
    MaxCount:0,
    CountDownStatus:'',
    StartDate:'',
    EndDate:'',
    NowTime:'',
    ProductId:0,
    ProductName:'',
    TempMetaDescription: '',
    MetaDescription:'',
    ShortDescription:'',
    //SaleCounts:'',
    ShowSaleCounts:'',
    //Weight:'',
    MarketPrice:'',
    IsfreeShipping:'',
    MaxSalePrice:'',
    MinSalePrice:'',
    ReviewCount: 0,
    ProductImgs:'',
    //DefaultSku:'',
    SkuItemList:'',
    Skus:'',
    Freight:'',
    Coupons:'',
    //IsUnSale:'',
    ShowPrice:'',
    backShow:'none',
    SkuShow:'none',
    couponShow:'none',
    skuImg:'',
    skuPrice:0,
    skuStock:0,
    selectedSku:'',
    selectedSkuContent:'',
    buyAmount:1,
    selectedskuList:[],
    activeDateMsg:'',
    StartClock:'',
    EndClock:'',
  },
  onReachBottom: function () {
    var that=this;
    if (this.data.metaDescription == null || this.metaDescription==''){
    var metaDescription = that.data.TempMetaDescription;
    if (metaDescription != null && metaDescription != undefined) {
      WxParse.wxParse('metaDescription', 'html', metaDescription, that);
    }
    } 
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    const that = this;
    // 页面初始化 options为页面跳转所带来的参数
    const countdownid = options.id;
    app.getOpenId(function(openid){
        wx.request({
          url: app.getUrl(app.globalData.getLimitBuyProduct),
        data:{
          openId:openid,
          countDownId:countdownid
        },
        success: function(result) {
          if(result.data.Status=="OK"){
            const r = result.data.Data;
           
            
            if(r.NowTime<r.StartDate){
              var st = new Date(r.NowTime);
              var nt = new Date(r.StartDate);
              var mi_se = nt.getTime()-st.getTime();
              var totalseconds = mi_se/1000;
              startcountdown(that,totalseconds);
            }
            if(r.NowTime>r.StartDate && r.NowTime<r.EndDate){
              var st = new Date(r.NowTime);
              var nt = new Date(r.EndDate);
              var mi_se = nt.getTime()-st.getTime();
              var totalseconds = mi_se/1000;
              endcountdown(that,totalseconds);
            }
            var tempSku = '';
            if(r.SkuItemList.length==0){
                tempSku = r.Skus[0].SkuId;
            }
            that.setData({
              CountDownId:r.CountDownId,
              MaxCount:r.MaxCount,
              CountDownStatus:r.CountDownStatus,
              StartDate:r.StartDate,
              EndDate:r.EndDate,
              NowTime:r.NowTime,
              ProductId: r.ProductId,
              ProductName:r.ProductName,
              ShortDescription: r.ShortDescription ? r.ShortDescription : '',
              //SaleCounts: r.SaleCounts,
              ShowSaleCounts: r.ShowSaleCounts,
              //Weight: r.Weight,
              MarketPrice:r.MarketPrice,
              IsfreeShipping:r.IsfreeShipping,
              MaxSalePrice:r.MaxSalePrice,
              MinSalePrice:r.MinSalePrice,
              ReviewCount: r.ReviewCount,
              ProductImgs:r.ProductImgs,
              //DefaultSku:r.DefaultSku,
              SkuItemList:r.SkuItemList,
              Skus:r.Skus,
              Freight:r.Freight,
              Coupons: r.Coupons,
              ShowPrice:r.MaxSalePrice==r.MinSalePrice?r.MinSalePrice:r.MinSalePrice+'～'+r.MaxSalePrice,
              skuImg:r.ThumbnailUrl60,
              skuPrice:r.MinSalePrice,
              skuStock:r.Stock,
              selectedSku:tempSku,
              selectedSkuContent:'',
              TempMetaDescription: r.MetaDescription,
              buyAmount:1
            })
          }
          else{
            if (result.data.Message == 'NOUser') {
              wx.navigateTo({
                url: '../login/login'
              })
            }
            else{
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
  onShareAppMessage: function () {
    var that = this;
    return {
      title:'限时抢购'+that.data.ProductName,
      path: '',
      success: function (res) {
      },
      fail: function (res) {
        // 转发失败
      }
    }
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
        url: app.getUrl(app.globalData.userGetCoupon),
        data: {
          openId: openid,
          couponId: couponid
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            wx.showToast({
              title: '领取成功',
              image:'../../images/succes.png'
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
              })
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
  onCouponHide: function (e) {
    this.setData({
      backShow: 'none',
      couponShow: 'none'
    })
  },
  clickSku:function(e){
    this.setData({
      backShow:'',
      SkuShow:''
    })
  },
  clickback:function(e){
    this.setData({
      backShow:'none',
      SkuShow:'none',
      couponShow:'none'
    })
  },
  onSkuHide:function(e){
    this.setData({
      backShow:'none',
      SkuShow:'none'
    })
  },
  changeAmount: function (e) {
    const that = this;
    var amount = parseInt(e.detail.value);
    var stock = this.data.MaxCount > this.data.skuStock ? this.data.skuStock : this.data.MaxCount;
    
    if (isNaN(amount) || amount > stock || amount <= 0) {
      that.setData({
        buyAmount : stock
      })
      app.showErrorModal("请输入正确的数量,不能大于最大抢购数量和商品库存或者小于等于0");
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
    amount=amount-1;
    if(amount<=0)
      return;
    else{
      this.setData({
        buyAmount:amount
      })
    }
  },
  addAmount:function(e){
    var amount = this.data.buyAmount;
    var stock = this.data.MaxCount > this.data.skuStock ? this.data.skuStock : this.data.MaxCount;
    amount=amount+1;
    if(amount>stock)
      return;
    else{
      this.setData({
        buyAmount:amount
      })
    }
  },
  commitBuy:function(e){
    var isselectsku = true;
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
    if(this.data.buyAmount<=0){
        app.showErrorModal("请输入要购买的数量");
      return;
    }
    var amount = this.data.buyAmount;
    var skuid = this.data.selectedSku;
    var countdownid = this.data.CountDownId;
    wx.navigateTo({
      url: '../submitorder/submitorder?productsku='+skuid+'&buyamount='+amount+'&frompage=1&countdownid='+countdownid
    })
  },
  onSkuClick:function(e){
    var that = this;
    var index = e.target.dataset.indexcount;
    var valueid = e.target.id;
    var value = e.target.dataset.skuvalue;
    var selInfo = new Object();
    selInfo.valueid=valueid;
    selInfo.value=value;
    var selSku = this.data.selectedskuList;
    selSku[index] = selInfo;
    var selContent="";
    var isAlSelected=false;
    var itemList = this.data.SkuItemList;
    if(itemList.length==selSku.length) isAlSelected=true;
    var skuId = this.data.ProductId;
    for (var i = 0; i < selSku.length; i++) {
      var info = selSku[i];
      if (info != undefined) {
        selContent += selContent == "" ? info.value : "," + info.value;
      }
    }
    var currentProductSku = null;
    that.data.Skus.forEach(function (item, index, array) {
      var found = true;
      for (var i = 0; i < selSku.length; i++) {
        if (selSku[i] == undefined || item.SkuId.indexOf('_' + selSku[i].valueid) == -1)
          found = false;
      }
      if (found && itemList.length == selSku.length) {
        currentProductSku = item;
        skuId = item.SkuId;
        that.data.buyAmount = item.CartQuantity > 0 ? item.CartQuantity : 1;
        return;
      }
    });
    var curentItem = itemList[index];
    for(var j=0;j<itemList[index].AttributeValue.length;j++){
      var item = itemList[index].AttributeValue[j];
      if(item.ValueId==valueid){
        itemList[index].AttributeValue[j].UseAttributeImage='selected';
      }
      else{
        itemList[index].AttributeValue[j].UseAttributeImage='False';
      }
    }
    this.setData({
      selectedskuList:selSku,
      selectedSku:skuId,
      selectedSkuContent:selContent,
      SkuItemList:itemList
    })
    if(currentProductSku!=null){
      this.setData({
        skuPrice:currentProductSku.ActivityPrice,
        skuStock:currentProductSku.ActivityStock,
      })
      if(currentProductSku.ThumbnailUrl40!="" && currentProductSku.ThumbnailUrl40!=null)
      {
        this.setData({
          skuImg:currentProductSku.ThumbnailUrl40
        })
      }
    }
  },
  
})

function showTime(totalseconds){
  var day = parseInt(totalseconds / 86400);
  var hour = parseInt(totalseconds % 86400 / 3600);
  var min = parseInt((totalseconds % 3600) / 60);
  var sec = parseInt((totalseconds % 3600) % 60);
  var result="";
  if (day>0){
    result += day + "天";
  }
  if (hour<10){
    hour = "0" + hour;
  }
  if (min < 10) {
    min = "0" + min;
  }
  if (sec < 10) {
    sec = "0" + sec;
  }
  result += hour + ":" + min + ":" + sec + "";
  return result;
}

function startcountdown(that,total_second){
  that.setData({
    StartClock: showTime(total_second)
  });
  if (total_second <= 0) {
    that.setData({
      StartClock: "",
      CountDownStatus:"Normal"
    });
    return;
  }
  setTimeout(function () {
    total_second -= 1;
    startcountdown(that,total_second);
  }, 1000);
}
function endcountdown(that,total_second){
  that.setData({
    EndClock: showTime(total_second)
  });
  if (total_second <= 0) {
    that.setData({
      EndClock: "",
      CountDownStatus:"ActivityEnd"
    });
    return;
  }
  setTimeout(function () {
    total_second -= 1;
    endcountdown(that,total_second);
  }, 1000);
}