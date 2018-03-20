// applylist.js
var app=getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    pageIndex: 1,
    pageSize:10,
    AfterList:null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
   

  },
  loadData:function(that,isNextPage){
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getAllAfterSaleList),
        data: {
          openId: openid,
          pageIndex: that.data.PageIndex,
          pageSize: that.data.PageSize
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            if (isNextPage) {
              var old = that.data.AfterList;
              old.push.apply(old, r);
              that.setData({
                AfterList: old
              })
            } else {
              that.setData({
                AfterList: r
              })
            }
          }
          else if (result.data.Message == 'NOUser') {
            wx.navigateTo({
              url: '../login/login'
            })
          }
          else {
            wx.showModal({
              title: '提示',
              content: result.data.Message,
              showCancel: false,
              success: function (res) {
                if (res.confirm) {
                  wx.navigateBack({ delta: 1 })
                }
              }
            })
          }
        }
      })
    })
  },
  applydetail:function(e){
    var typeapply = e.currentTarget.dataset.type;//1代表订单退款，2代表退货，3代表退货退款
    var id = e.currentTarget.dataset.id;
    if (typeapply==1){
      wx.navigateTo({
        url: '../refunddetail/refunddetail?id=' + id
      });
    }else{
      wx.navigateTo({
        url: '../returndetail/returndetail?id=' + id
      });
    }
  },
  SendGood:function(e){
    var id = e.currentTarget.dataset.id;
    var skuid = e.currentTarget.dataset.skuid;
    wx.navigateTo({
      url: '../applysendgood/applysendgood?id=' + id +'&&skuId='+skuid
    });

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
    const that = this;
    that.loadData(that, false);
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