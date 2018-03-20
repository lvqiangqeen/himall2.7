// returndetail.js
var util = require('../../utils/util.js');
var app = getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    RefundInfo: null,
    ProgressStatue: [],
    Credentials: [],
    isExpend:true
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    var id = options.id;
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl(app.globalData.getReturnDetail),
        data: {
          openId: openid,
          returnId: id
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            var temprefund = result.data.Data;
            that.setData({
              RefundInfo: temprefund,
              Credentials: temprefund.UserCredentials
            });
            wx.setNavigationBarTitle({
              title: temprefund.IsOnlyRefund ? "退款详情" :"退货详情"
            })
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
        }
      });
    });
  },
  prevImage: function (e) {
    var that = this;
    var idx = e.target.dataset.index;
    var current = e.target.dataset.src;

    wx.previewImage({
      current: current,
      urls: that.data.Credentials
    });
  },
  ExpendProgress:function(){
    var isexpend=!this.data.isExpend;

    this.setData({
      isExpend: isexpend
    });
  },
  ShowProgress: function (refundInfo) {
    const that = this;
    var refundstat = parseInt(refundInfo.Status);
    var returnDiscard=false;//商家同意退货并弃货标记
    if (!refundInfo.IsOnlyRefund && (refundstat == 6 || refundstat == 7) && refundInfo.UserSendGoodsTime == '')
    {//退货申请、商家审核通过、会员没有发货
      returnDiscard=true;
    }
    var progressstr = [
      {
        statue: 0,
        statuename: refundInfo.IsOnlyRefund ? '申请退款中' : '申请退货中',
        time: refundInfo.ApplyForTime,
        ishidden: false,
        isactive: true,
        activestatusname: refundInfo.IsOnlyRefund ? '申请退款中' : '申请退货中'
      },
      {
        statue: 1,
        statuename: '待商家审核',
        time: refundInfo.DealTime,
        ishidden: false,
        isactive: refundstat>1?true: false,
        activestatusname: (refundstat == 2 || refundstat == 3 || refundstat == 5 || refundstat == 6 || refundstat == 7 || refundInfo.UserSendGoodsTime!='') ? "商家通过审核" : (refundstat == 4 ? "商家拒绝" : "")
      },
      {
        statue: 2,
        statuename: '待买家寄货',
        time: refundInfo.UserSendGoodsTime,
        ishidden: returnDiscard || refundInfo.IsOnlyRefund || (refundInfo.UserSendGoodsTime == '' && refundstat==4)?true:false,
        isactive: refundstat > 2 ? true :false,
        activestatusname:"买家已寄货"
      },
      {
        statue: 3,
        statuename: '待商家收货',
        time: refundInfo.ConfirmGoodsTime,
        ishidden: returnDiscard || refundInfo.IsOnlyRefund || (refundInfo.UserSendGoodsTime == '' && refundstat == 4) ? true : false,
        isactive: refundstat > 3 ? true:false,
        activestatusname: refundstat == 5 || (refundInfo.UserSendGoodsTime != '' && (refundstat == 6 || refundstat == 7) ) ? "商家通过审核" : (refundstat == 4?"商家拒绝":"")
      },
      {
        statue: 6,
        statuename: '待平台确认',
        time: refundInfo.FinishTime,
        ishidden: refundstat == 4?true: false,
        isactive: refundstat > 6 ? true :false,
        activestatusname: refundInfo.IsOnlyRefund ? '退款完成' : '退货完成'
      }
    ];


    progressstr.forEach(function (item, index, array) {
            
    });




    /*if (refundstat >= 3 || refundstat == 1) {
      var progressinfo = {
        statuename: '商家同意申请',
        time: refundInfo.dealTime
      };
      progressstr.push(progressinfo);
    }
    if (refundstat >= 4 || refundstat == 1) {
      var progressinfo = {
        statuename: '买家退货',
        time: refundInfo.dealTime
      };
      progressstr.push(progressinfo);
    }
    if (refundstat >= 5 || refundstat == 1) {
      var progressinfo = {
        statuename: '商家确认收货',
        time: refundInfo.dealTime
      };
      progressstr.push(progressinfo);
    }
    if (refundstat == 1) {
      var progressinfo = {
        statuename: '商家同意退款',
        time: util.formatTime(refundInfo.dealTime)
      };
      progressstr.push(progressinfo);
      progressinfo = {
        statuename: '退款完成',
        time: refundInfo.dealTime
      };
      progressstr.push(progressinfo);
    }
    if (refundstat == 2) {
      var progressinfo = {
        statuename: '退货拒绝',
        time: refundInfo.ApplyForTime
      };
      progressstr.push(progressinfo);
    }*/

    that.setData({
      ProgressStatue: progressstr
    });
  },
  SendGood: function (e) {
    var id = e.currentTarget.dataset.id;
    var skuid = e.currentTarget.dataset.skuid;
    wx.navigateTo({
      url: '../applysendgood/applysendgood?id=' + id + '&&skuId=' + skuid
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