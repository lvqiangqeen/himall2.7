// refunddetail.js
var util = require('../../utils/util.js');
var app=getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    RefundInfo: null,
    Credentials:[],
    ProgressStatue:[],
    isExpend: true
  },

  ExpendProgress: function () {
    var isexpend = !this.data.isExpend;

    this.setData({
      isExpend: isexpend
    });
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    const that=this;
    var id = options.id;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getRefundDetail),
        data: {
          openId: openid,
          RefundId: id
        },
        success: function (result) {
          if (result.data.Status == "OK") {
           var temprefund=result.data.Data;
           //var strimg=temprefund.UserCredentials;
           var strimg="";
           var arrayimg=[];
           if(strimg.length>0){
             arrayimg=strimg.split(',');
           }
            that.setData({
              RefundInfo: temprefund,
              Credentials:arrayimg
            });
            that.ShowProgress(temprefund);
          } else {
            if (result.data.Message == 'NOUser') {
              wx.navigateTo({
                url: '../login/login'
              })
            }
            else {
              wx.showModal({
                title: '提示',
                content: result.data.ErrorResponse.ErrorMsg,
                showCancel: false,
                success: function (res) {
                  if (res.confirm) {
                    wx.navigateBack({ delta: 1 })
                  }
                }
              })
            }
          }
        },
        complete: function () {

        }
      });
    });
  },
ShowProgress:function(refundInfo){
  const that=this;
  var refundstat = parseInt(refundInfo.Status);
  var progressstr = [
    {
      statue: 0,
      statuename:  '申请退款中' ,
      time: refundInfo.ApplyForTime,
      ishidden: false,
      isactive: true,
      activestatusname: '申请退款中'
    },
    {
      statue: 1,
      statuename: '待商家审核',
      time: refundInfo.DealTime,
      ishidden: false,
      isactive: refundstat > 1 ? true : false,
      activestatusname: (refundstat == 2 || refundstat == 3 || refundstat == 5 || refundstat == 6 || refundstat == 7 ) ? "商家通过审核" : (refundstat == 4 ? "商家拒绝" : "")
    },
    {
      statue: 6,
      statuename: '待平台确认',
      time: refundInfo.FinishTime,
      ishidden: refundstat == 4 ? true : false,
      isactive: refundstat > 6 ? true : false,
      activestatusname: '退款完成' 
    }
  ];

  /* if (refundstat>=0){
    var progressinfo = {
      status: 1,
      statuename: '退款申请中',
      time: util.formatTime(refundInfo.ApplyForTime)
    };
    progressstr.push(progressinfo);
  }
 if (refundstat==1){
    var progressinfo = {
      statuename: '商家同意退款',
      time: refundInfo.dealTime
    };
    progressstr.push(progressinfo);
    progressinfo = {
      statuename: '退款完成',
      time: refundInfo.dealTime
    };
    progressstr.push(progressinfo);
  }else{
    var progressinfo = {
      statuename: '拒绝退款',
      time: refundInfo.dealTime
    };
    progressstr.push(progressinfo);
    progressinfo = {
      statuename: '退款完成',
      time: refundInfo.dealTime
    };
    progressstr.push(progressinfo);
  }*/
  that.setData({
    ProgressStatue: progressstr
  });
},
goToProductDetail: function (e) {
  var that = this;
  var productid = e.currentTarget.dataset.productid;
  wx.navigateTo({
    url: '../productdetail/productdetail?id=' + productid
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