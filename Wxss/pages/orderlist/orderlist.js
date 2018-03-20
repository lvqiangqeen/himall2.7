var app = getApp();
Page({
  data:{
    isEmpty:true,
    Status:0,
    OrderList:null,
    AllActive:'active',
    WaitPayActive:'',
    WaitSendActive:'',
    WaitReceiveActive:'',
    WaitReviewActive: '',
    PageIndex: 1,
    PageSize: 100,
    nullOrder: app.getRequestUrl + '/Templates/xcxshop/images/nullOrder.png'
  },
  onLoad:function(options){
    // 页面初始化 options为页面跳转所带来的参数
    var status = options.status;
    if(options.status=="" || options.status==undefined) status=0;
    const that = this;
    that.setData({
      Status: status
    });
  },
  onReady:function(){
    // 页面渲染完成
  },
  onShow:function(){
    // 页面显示
    var that = this;
    that.setData({
      PageIndex:1,
      OrderList:[],
    });
    that.loadData(that.data.Status, that, false);
  },
  onHide:function(){
    // 页面隐藏
  },
  onUnload:function(){
    // 页面关闭
  },
  onReachBottom: function () {
    var that = this;
    var pageIndex = that.data.PageIndex + 1;
    that.setData({
      PageIndex: pageIndex
    })
    that.loadData(that.data.Status,that,true);
  },
  closeOrder:function(e){
    var that = this;
    var orderid = e.target.dataset.orderid;
    wx.showModal({
      title: '提示',
      content: '确定要取消订单吗？',
      success: function(res) {
        if (res.confirm) {
          app.getOpenId(function(openid){
            wx.request({
              url: app.getUrl(app.globalData.closeOrder),
              data:{
                openId:openid,
                orderId:orderid
              },
              success: function(result) {
                if(result.data.Status=="OK"){
                  wx.showModal({
                    title: '提示',
                    content:result.data.Message,
                    showCancel:false,
                    success: function(res) {
                      if (res.confirm) {
                        wx.navigateTo({
                          url: '../orderlist/orderlist?status='+that.data.Status
                        })
                      }
                    }
                  })
                }
                else if (result.data.Message == 'NOUser') {
                  wx.navigateTo({
                    url: '../login/login'
                  })
                }
                else{
                  app.showErrorModal(result.data.Message);
                }
              }
            })
          })
        } 
      }
    })
  },
  orderPay:function(e){
    var that = this;
    var orderid = e.currentTarget.dataset.orderid;
    app.orderPay(orderid,that.data.Status,true);
  },
  orderFinish:function(e){
    var that = this;
    var orderid = e.currentTarget.dataset.orderid;
    app.getOpenId(function(openid){
      wx.request({
        url: app.getUrl(app.globalData.finishOrder),
        data:{
          openId:openid,
          orderId:orderid
        },
        success: function(result) {
          if(result.data.Status=="OK"){
            wx.showModal({
              title: '提示',
              content:"确认收货成功！",
              showCancel:false,
              success: function(res) {
                if (res.confirm) {
                  wx.navigateTo({
                    url: '../orderlist/orderlist?status='+that.data.Status
                  })
                }
              }
            })
          }
          else if (result.data.Message == 'NOUser') {
            wx.navigateTo({
              url: '../login/login'
            })
          }
          else{
              app.showErrorModal(result.data.Message);
          }
        }
      })
    })
  },
  toproduct: function (e) {
    wx.switchTab({
      url: '../productcategory/productcategory'
    })
  },
  onTabClick:function(e){
    var that = this;
    var status = e.currentTarget.dataset.status;
    that.setData({
      PageIndex: 1
    })
    that.loadData(status,that,false);
  },
  showLogistics:function(e){
    var orderid = e.currentTarget.dataset.orderid;
    wx.navigateTo({
      url: '../logistics/logistics?orderid='+orderid
    })
  },
  showReview: function (e) {
    var orderid = e.currentTarget.dataset.orderid;
    wx.navigateTo({
      url: '../comment/comment?id=' + orderid
    })
  },
  goToOrderDetail:function(e){
    var orderid = e.currentTarget.dataset.orderid;
    wx.navigateTo({
      url: '../orderdetails/orderdetails?orderid='+orderid
    })
  }, 
  RefundOrder:function(e){
    var orderid = e.currentTarget.dataset.orderid;
    var moneys = e.currentTarget.dataset.money;
    wx.navigateTo({
      url: '../ApplyRefund/ApplyRefund?orderid=' + orderid+"&&m="+moneys
    })
  },
  ReturnsOrder:function(e){
    var orderid = e.currentTarget.dataset.orderid;
    var skuId = e.currentTarget.dataset.skuId;
    var proname = e.currentTarget.dataset.skuname;
    var num = e.currentTarget.dataset.num;
    var moneys=e.currentTarget.dataset.money;

    wx.navigateTo({
      url: '../ApplyReturns/ApplyReturns?orderid=' + orderid + '&&skuId=' + skuId + '&&pro=' + proname + '&&num=' + num + '&&m=' + moneys
    })
  },
  loadData(status,that,isNextPage){ 
    wx.showLoading({
      title: '加载中',
    }); 
    this.pageActive(status,that);
    app.getOpenId(function(openid){
      wx.request({
        url: app.getUrl(app.globalData.orderList),
        data:{
          openId:openid,
          status:status,
          pageIndex:that.data.PageIndex,
          pageSize: that.data.PageSize
        }, 
        success: function(result) {
          if(result.data.Status=="OK"){
            var r = result.data.Data;

            if(isNextPage){
              var old = that.data.OrderList;
              old.push.apply(old, r);
              that.setData({
                OrderList: old
              })
            }else{
              var isempty = r.length>0;
              that.setData({
                Status: status,
                OrderList: r,
                isEmpty:isempty
              })
            }
          }
          else if (result.data.Message == 'NOUser') {
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
        },complete:function(){
          wx.hideLoading()
        }
      })
    })
  },
  pageActive(status,that){
    if(status==0){
      that.setData({
        AllActive:'active',
        WaitPayActive:'',
        WaitSendActive:'',
        WaitReceiveActive:'',
        WaitReviewActive: ''
      })
    }
    else if(status==1){
      that.setData({
        AllActive:'',
        WaitPayActive:'active',
        WaitSendActive:'',
        WaitReceiveActive: '',
        WaitReviewActive: ''
      })
    }
    else if(status==2){
      that.setData({
        AllActive:'',
        WaitPayActive:'',
        WaitSendActive:'active',
        WaitReceiveActive: '',
        WaitReviewActive: ''
      })
    }
    else if(status==3){
      that.setData({
        AllActive:'',
        WaitPayActive:'',
        WaitSendActive:'',
        WaitReceiveActive: 'active',
        WaitReviewActive: ''
      })
    }
    else if (status == 5) {
      that.setData({
        AllActive: '',
        WaitPayActive: '',
        WaitSendActive: '',
        WaitReceiveActive: '',
        WaitReviewActive: 'active'
      })
    }
  }
})