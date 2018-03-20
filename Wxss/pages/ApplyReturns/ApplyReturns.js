// ApplyRetuns.js
var config = require("../../utils/config.js");
var app=getApp();
Page({

  /**
   * 页面的初始数据
   */
  data: {
    OrderId: '',
    SkuId: '',
    Name: '',
    AfterSaleType: 0,
    AfterSaleTypeText: '请选择售后类型',
    RefundType: 0,
    RefundTypeText: '请选择退款方式',
    RefundReasonText: '请选择退货原因',
    Remark: '',
    BankName: '',
    BankAccountName: '',
    BankAccountNo: '',
    UserCredentials: ['../../images/return-img_03.jpg', '../../images/return-img_03.jpg', '../../images/return-img_03.jpg'],
    ReturnNum: 1,
    MostMoney: 0.00,
    ShowReason: true,
    ShowType: true,
    ShowAfterType: true,
    ApplyReturnNum: 1,
    TotalMoney: 0.00,
    UploadGredentials:[],
    FormId:'',
    ReturnMoney:0.00,
    ImageIndex:0,
    ShowReasonList: ['拍错/多拍/不想要', '缺货', '未按约定时间发货'],
    ShowReasonIndex: -1,
    RefundTextList: ['退到预付款', '原路返回', '到店退款'],
    ShowRefundIndex: -1,
    AfterSaleTypeList: ['仅退款', '退款退货'],
    AfterSaleTypeId:-1,
    OneReundAmount:0,
    returnId:null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var that = this;
    var orderId = options.orderid;
    var skuId = options.skuId;
    var proname = options.pro;
    var num = options.num;
    var moneys = options.m;
    var returnId = options.returnId;
    that.setData({
      OrderId: orderId,
      SkuId: skuId,
      Name: proname,
      ReturnNum: num,
      MostMoney: moneys,
      TotalMoney: moneys,
      returnId: returnId
    });
    app.getOpenId(function (openId) {
      wx.request({
        url: app.getUrl(app.globalData.getAfterSalePreCheck),
        data: {
          openId: openId,
          IsReturn: true,
          OrderId: orderId,
          SkuId: skuId,
        },
        success: function (result) {
          that.GetCheckData(result);
        }
      })
    })
  },
  GetCheckData: function (result) {
    var res = result.data;
    var that = this;
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
      if (res.CanReturnOnStore) {
        tempRefundTextList.push("到店退款");
      }
      var reasonList = [];
      res.RefundReasons.forEach(function (item, idx) {
        reasonList.push(item.AfterSalesText);
      });
      this.setData({
        MostMoney: res.MaxRefundAmount,
        RefundTextList: tempRefundTextList,
        TotalMoney: res.MaxRefundAmount,
        ReturnNum: res.MaxRefundQuantity,
        ApplyReturnNum: res.MaxRefundQuantity,
        OneReundAmount: res.oneReundAmount,
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
  uploadImg: function (e) {
    var that = this;
    var tempgreden = that.data.UserCredentials;
    var idx = e.currentTarget.dataset.index;
    wx.chooseImage({
      success: function (res) {
        tempgreden[idx] = res.tempFilePaths[0];
        var imgidx = parseInt(that.data.ImageIndex);
        imgidx=imgidx>=2?2:imgidx++;
        that.setData({
          UserCredentials: tempgreden,
          ImageIndex:imgidx
        });
      },
    })
  },
  ShowAfterType: function (e) {
    var that = this;
    wx.showActionSheet({
      itemList: that.data.AfterSaleTypeList,
      success: function (res) {
        if (!res.cancel) {
          that.setData({
            AfterSaleTypeId: res.tapIndex
          });
        }
      },
      fail: function (res) {
        console.log(res.errMsg)
      }
    })
  },
  ShowResaon: function (e) {
    var that = this;
    wx.showActionSheet({
      itemList: that.data.ShowReasonList,
      success: function (res) {
        that.setData({
          ShowReasonIndex: res.tapIndex
        });
      },
      fail: function (res) {
        console.log(res.errMsg)
      }
    })
  },
  ShowRefundType: function (e) {
    var that = this;
    wx.showActionSheet({
      itemList: that.data.RefundTextList,
      success: function (res) {
        if (!res.cancel){
        var text = that.data.RefundTextList[res.tapIndex];
        var refundtype = that.GetRefundTypeId(text);
        that.setData({
          ShowRefundIndex: res.tapIndex,
          RefundTypeText: text,
          RefundType: refundtype,
        });
        }
      },
      fail: function (res) {
        console.log(res.errMsg)
      }
    })
  },
  ChooseReason: function (e) {
    var that = this;
    var reasonname = e.currentTarget.dataset.name;
    that.setData({
      RefundReasonText: reasonname,
      ShowType: true,
      ShowReason: true,
      ShowAfterType: true
    });
  },
  GetRefundTypeId: function (typeName) {

    if (typeName == "退到预付款") {
      return 3;
    } else if (typeName == "退到银行卡") {
      return 2;
    } else if (typeName == "原路返回") {
      return 1;
    } else {
      return 4;
    }
  },
  GetAfterSaleTypeId: function (typeName) {
    if (typeName == "退货退款") {
      return 3;
    } else if (typeName == "仅退款") {
      return 2;
    } else {
      return 1;
    }
  },
  ChooseAfterType: function (e) {
    var refundtype = e.currentTarget.dataset.id;
    var that = this;
    var typename = that.ShowAfterTypeName[refundtype];
    that.setData({
      AfterSaleType: refundtype,
      AfterSaleTypeText: typename,
      ShowType: true,
      ShowReason: true,
      ShowAfterType: true
    });

  },
 
  MuseNum: function (e) {
    var that = this;
    var tempreturnnum = that.data.ApplyReturnNum;

    if (tempreturnnum <= 1) {
      app.showErrorModal('最少退1件商品');
      return;
    }
    tempreturnnum = tempreturnnum - 1;
    var totalmoney = parseFloat(tempreturnnum * that.data.OneReundAmount).toFixed(2)
    that.setData({
      ApplyReturnNum: tempreturnnum,
      TotalMoney: totalmoney
    });
  },
  AddNum: function (e) {
    var that = this;
    var tempactreturnnum = parseInt(that.data.ApplyReturnNum);
    var tempreturnnum = parseInt(that.data.ReturnNum);

    if (tempactreturnnum >= tempreturnnum) {
      app.showErrorModal('最多退' + tempreturnnum + '件商品');
      return;
    }
    tempactreturnnum = tempactreturnnum + 1;
    var totalmoney = parseFloat(tempactreturnnum * that.data.OneReundAmount).toFixed(2);
    that.setData({
      ApplyReturnNum: tempactreturnnum,
      TotalMoney: totalmoney
    });
  },
  formSubmit: function (e) {
    var that = this;
    var reasonId = parseInt(that.data.ShowReasonIndex);
    var text = that.data.AfterSaleTypeList[that.data.AfterSaleTypeId];
    var aftertypeId = that.GetAfterSaleTypeId(text);
    var formId = e.detail.formId;
    var bankname = that.ToTrim(e.detail.value.txtBankName);
    var bankaccountname = that.ToTrim(e.detail.value.txtBankAccountName);
    var bankno = that.ToTrim(e.detail.value.txtBankAccountNo);
    var returnmoney = parseFloat(e.detail.value.txtmoney.replace("￥",""));
    var returnnum = aftertypeId==2?0:parseInt(that.data.ApplyReturnNum);

    if ((aftertypeId==3 &&  returnnum <= 0) || returnnum > that.data.ReturnNum){
      app.showErrorModal("请输入正确的退货数量");
      return;
    }
    if (returnmoney > that.data.OneReundAmount * parseInt(that.data.ApplyReturnNum))
   {
      app.showErrorModal("请输入正确的退款金额,金额必须小于等于" + that.data.OneReundAmount * returnnum + "元");
      return;
   }
    
    var remark = e.detail.value.txtarea;

    var refundtype = that.data.RefundType;//获取退款方式
    if (refundtype == 2) {
      if (bankname.length <= 0 || bankaccountname.length <= 0 || bankno.length <= 0) {
        app.showErrorModal("银行卡信息不允许为空！");
        return;
      }
    }
    if (refundtype <= 0) {
      app.showErrorModal("请选择要退款的方式");
      return;
    }

    if (reasonId<0) {
      app.showErrorModal("请选择要退款的原因");
      return;
    }

    if (aftertypeId<0) {
      app.showErrorModal("请选择售后类型");
      return;
    }
    if (that.data.OrderId.length <= 0) {
      app.showErrorModal("请选择要退款的订单");
      return;
    }
     that.setData({
       formId: formId,
       AfterSaleTypeId:aftertypeId,
       Remark: remark,
       BankName: bankname,
       BankAccountName: bankaccountname,
       BankAccountNo: bankno,
       ApplyReturnNum: returnnum,
       ReturnMoney: returnmoney,
       UploadGredentials:[]
    });
    var temp=[];
    that.data.UserCredentials.find(function(item,index){
      if (item !='../../images/return-img_03.jpg'){
        temp.push(item);
      }
    });

    that.UploadBatchImages(that, temp);//上传图片
  },
  UploadBatchImages: function (that,imglist) {
    var currentuploadimg = imglist.shift();
    if (currentuploadimg!=undefined){
      app.getOpenId(function (openId) {
        wx.uploadFile({
          url: app.getUrl("OrderRefund/PostUploadAppletImage"),
          filePath: currentuploadimg,
          name: 'file',
          formData: {
            openId:openId
          },
          success: function (r) {
            var result=JSON.parse(r.data);
            if (result.Status == "OK") {
              var temgredentials = that.data.UploadGredentials;
              temgredentials.push(result.Data[0].ImageUrl);
              that.setData({
                UploadGredentials: temgredentials
              });
            } else {
              if (result.Message == 'NOUser') {
                wx.navigateTo({
                  url: '../login/login'
                })
              }
              else {
                app.showErrorModal(result.ErrorResponse.ErrorMsg, function (res) {
                    if (res.confirm) {
                        wx.navigateBack({ delta: 1 })
                    }
                });
              }
            }
          },
          complete: function () {
            if (imglist.length > 0) {
              that.UploadBatchImages(that, imglist);
            } else {
              that.AddReturnInfo();
            }
          }
        })
      })
    }else{
      that.AddReturnInfo();
    }
  },
 
  AddReturnInfo:function(){
    const that=this;
    app.getOpenId(function (openid) {
        var para={
          openId: openid,
          skuId: that.data.SkuId,
          orderId: that.data.OrderId,
          Quantity: that.data.ApplyReturnNum,
          RefundAmount: that.data.ReturnMoney,
          afterSaleType: that.data.AfterSaleTypeId,
          RefundType: that.data.RefundType,
          RefundReason: that.data.ShowReasonList[that.data.ShowReasonIndex],
          Remark: that.data.Remark,
          BankName: that.data.BankName,
          BankAccountName: that.data.BankAccountName,
          BankAccountNo: that.data.BankAccountNo,
          UserCredentials: that.data.UploadGredentials.join(','),
        formId: that.data.formId,
        refundId: that.data.returnId
        };
        config.httpPost(app.getUrl("OrderRefund/PostApplyReturn"), para, function (result) {
          if (result.Status == "OK") {
            wx.showModal({
              title: '提示',
              confirmColor: '#ff5722',
              content: result.Message,
              showCancel: false,
              success: function (res) {
                wx.redirectTo({
                  url: '../applylist/applylist',
                });
              }
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
      //   url: app.getUrl("OrderRefund/PostApplyReturn"),
      //   data: {
      //     openId: openid,
      //     skuId: that.data.SkuId,
      //     orderId: that.data.OrderId,
      //     Quantity: that.data.ApplyReturnNum,
      //     RefundAmount: that.data.ReturnMoney,
      //     afterSaleType: that.data.AfterSaleTypeId,
      //     RefundType: that.data.RefundType,
      //     RefundReason: that.data.ShowReasonList[that.data.ShowReasonIndex],
      //     Remark: that.data.Remark,
      //     BankName: that.data.BankName,
      //     BankAccountName: that.data.BankAccountName,
      //     BankAccountNo: that.data.BankAccountNo,
      //     UserCredentials: that.data.UploadGredentials.join(','),
      //     formId: that.data.formId
      //   },
      //   success: function (result) {
      //     if (result.data.Status == "OK") {
      //       wx.showModal({
      //         title: '提示',
      //         confirmColor: '#ff5722',
      //         content: result.data.Message,
      //         showCancel: false,
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
      //           confirmColor: '#ff5722',
      //           content: result.data.Message,
      //           showCancel: false,
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
  ToTrim: function (str) {
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