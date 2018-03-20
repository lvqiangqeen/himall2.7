// ApplyRefund.js
var config = require("../../utils/config.js")
var app=getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    OrderId: '',
    SkuId: '',
    RefundType: 0,
    RefundTypeText: '请选择退款方式',
    RefundMoney: 0.00,
    RefundReason: 0,
    RefundReasonText: '请选择退款原因',
    Remark: '',
    BankName: '',
    BankAccountName: '',
    BankAccountNo: '',
    ShowReason: true,
    ShowType: true,
    ShowReasonList: ['拍错/多拍/不想要', '缺货', '未按约定时间发货'],
    ShowReasonIndex:-1,
    RefundTextList: ['原路返回', '','退到预付款'],
    ShowRefundIndex:-1
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    var orderId = options.orderid;
    var moneys = options.m;
    that.setData({
      OrderId: orderId
    });
    app.getOpenId(function (openId) {
      wx.request({
        url: app.getUrl(app.globalData.getAfterSalePreCheck),
        data: {
          openId: openId,
          IsReturn: false,
          OrderId: orderId,
          SkuId: "",
        },
        success: function (result) {
          that.GetCheckData(result);
        }
    })
    })    
  },
  GetCheckData:function(result){
    var res=result.data;
    var that=this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      var tempRefundTextList = [];
      if (res.CanBackReturn) {
        tempRefundTextList.push("原路返回");
      }
      if (res.CanToBalance) {
        tempRefundTextList.push("退到预付款");
      }
      var reasonList=[];
      res.RefundReasons.forEach(function(item,idx){
        reasonList.push(item.AfterSalesText);
      });
      
      //tempRefundTextList.push("退到银行卡");
      this.setData({
        RefundMoney: res.MaxRefundAmount,
        RefundTextList: tempRefundTextList,
        ShowReasonList: reasonList
      });
    }
    else {
      app.showErrorModal(res.Message, function (res1) {
          if (res1.confirm) {
              wx.navigateBack({ delta: 1 })
          }
      });
    }
  },
  InputText: function (e) {
    var that = this;
    var inputyname = e.currentTarget.dataset.names;
    var val = e.detail.value;

    switch (inputyname) {
      case "BankName":
        that.setData({
          BankName: val
        });
        break;
      case "BankAccountName":
        that.setData({
          BankAccountName: val
        });
        break;
      case "BankAccountNo":
        that.setData({
          BankAccountNo: val
        });
        break;
      default:
        that.setData({
          Remark: val
        });
        break;
    };
  },
  ShowReason:function(e){
    /*this.setData({
      ShowReason:false,
      ShowType:true
    });*/
    var that=this;
    wx.showActionSheet({
      itemList: that.data.ShowReasonList,
      success: function (res) {
        console.log(res)
        that.setData({
          ShowReasonIndex: res.tapIndex
        });
      },
      fail: function (res) {
        console.log(res.errMsg)
      }
    })

  },
  ShowType: function (e) {
    /*this.setData({
      ShowReason: true,
      ShowType: false
    });*/

    var that = this;
    wx.showActionSheet({
      itemList: that.data.RefundTextList,
      success: function (res) {
        if (!res.cancel) {
        var text = that.data.RefundTextList[res.tapIndex];
        var refundtype = that.GetRefundTypeId(text);
        console.log(text);
        console.log(refundtype);
        that.setData({
          ShowRefundIndex: res.tapIndex,
          RefundTypeText:text,
          RefundType: refundtype,
        });
        }
      },
      fail: function (res) {
        console.log(res.errMsg)
      }
    })
  },
  ChooseReason:function(e){
    var that=this;
    var reasonname = e.currentTarget.dataset.name;
    that.setData({
      RefundReasonText:reasonname,
      ShowType: true,
      ShowReason: true
    });
  },
  ChooseType:function(e){
    
    var that=this;
    var typename = that.RefundTextList[e.currentTarget.dataset.id];
    var refundtype = GetRefundTypeId(typename);
    console.log(e.currentTarget.dataset.id);
    console.log(refundtype);
    that.setData({
      RefundType:refundtype,
      RefundTypeText: typename,
      ShowType:true,
      ShowReason:true
    });
  },
  GetRefundTypeId:function(typeName){
   
    if (typeName == "退到预付款"){
      return 3;
    } else if (typeName == "退到银行卡"){
      return 2;
    } else if (typeName == "原路返回"){
      return 1;
    }else{
      return 4;
    }
  },
  formSubmit:function(e){
    var that = this;
    var reasonId=parseInt(that.data.ShowReasonIndex);
   
    var formId=e.detail.formId;
    var bankname =that.ToTrim(e.detail.value.txtBankName);
    var bankaccountname = that.ToTrim( e.detail.value.txtBankAccountName);
    var bankno = that.ToTrim(e.detail.value.txtBankAccountNo);

    var refundtype = that.data.RefundType;//获取退款方式
    if (refundtype==2){
      if(bankname.length<=0||bankaccountname.length<=0||bankno.length<=0){
        app.showErrorModal("银行卡信息不允许为空！");
        return;
      }
    }
    if (that.data.GetRefundTypeId < 0){
      app.showErrorModal("请选择要退款的方式");
      return;
    }

    if (reasonId < 0){
      app.showErrorModal("请选择要退款的原因");
      return;
    }
    if(that.data.OrderId.length<=0){
      app.showErrorModal("请选择要退款的订单");
      return;
    }
        
    app.getOpenId(function (openid) {
      var para = {
        openId: openid,
        skuId: that.data.SkuId,
        orderId: that.data.OrderId,
        RefundType: refundtype,
        RefundReason: that.data.ShowReasonList[reasonId],
        Remark: that.data.Remark,
        BankName: bankname,
        BankAccountName: bankaccountname,
        BankAccountNo: bankno,
        FormId: formId
      }
      config.httpPost(app.getUrl(app.globalData.applyRefund), para, function (result) {
        if (result.Status == "OK") {
          app.showErrorModal(result.Message, function (res1) {
              wx.redirectTo({
                  url: '../applylist/applylist',
              });
          });
        } else {
          if (result.Message == 'NOUser') {
            wx.navigateTo({
              url: '../login/login'
            })
          }
          else {
            app.showErrorModal(result.Message, function (res) {
                if (res.confirm) {
                    wx.navigateBack({ delta: 1 })
                }
            });
          }
        }
      });
      // wx.request({
      //   url: app.getUrl(app.globalData.applyRefund),
      //   data: {
      //     openId: openid,
      //     skuId: that.data.SkuId,
      //     orderId: that.data.OrderId,
      //     RefundType: refundtype,
      //     RefundReason: that.data.ShowReasonList[reasonId],
      //     Remark: that.data.Remark,
      //     BankName:bankname,
      //     BankAccountName:bankaccountname,
      //     BankAccountNo:bankno,
      //     FormId: formId
      //   },
      //   success: function (result) {
      //     if (result.data.Status == "OK") {
      //       wx.showModal({
      //         title: '提示',
      //         content: result.data.Message,
      //         showCancel: false,
      //         confirmColor: '#ff5722',
      //         success: function (res) {
      //             wx.redirectTo({
      //               url: '../applylist/applylist',
      //             });
      //         }
      //       });
      //     } else {
      //       if (result.data.Message == 'NOUser') {
      //         wx.navigateTo({
      //           url: '../login/login'
      //         })
      //       }
      //       else {
      //         wx.showModal({
      //           title: '提示',
      //           content: result.data.Message,
      //           showCancel: false,
      //           confirmColor: '#ff5722',
      //           success: function (res) {
      //             if (res.confirm) {
      //               wx.navigateBack({ delta: 1 })
      //             }
      //           }
      //         })
      //       }
      //     }
      //   },
      //   complete: function () {

      //   }
      // });
    });


  },
  ToTrim:function(str)
         { 
    return str.replace(/(^\s*)|(\s*$)/g, ""); 
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