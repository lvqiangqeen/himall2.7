// pages/comment/comment.js
var app = getApp();
var config = require("../../utils/config.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    OrderId: '',
    ProductList: [],
    UserCredentials: ['../../images/return-img_12.jpg'],
    UploadCredentials: [],
    ScoreGrade: [],
    Remark: [],
    TxtareaName: [],
    TotalImg: 0,
    UploadNum: 0,
    isSubmit:false,
    uploadComplete: true
  },
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    const that = this;
    // 页面初始化 options为页面跳转所带来的参数
    const orderId = options.id;
    app.getOpenId(function (openId) {
      var parameters = {
        openId: openId,
        orderId: orderId
      }
      config.httpGet(app.getUrl(app.globalData.loadOrderProduct), parameters, that.getProductData);
    })

    that.setData({
      OrderId: orderId
    });

  },
  getProductData: function (res) {
    var that = this;
    if (res.Message == 'NOUser') {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      var tempscoregrade = [];
      var temusercredentials = [];
      var tempareaname = [];
      res.Data.forEach(function (val, index, array) {
        var scoregrade = {
          skuId: val.SkuId,
          skucontent: val.SkuContent,
          grade: parseInt(5),
          remark: ''
        }
        var img = {
          img1: '../../images/return-img_03.jpg',
          img2: '../../images/return-img_03.jpg',
          img3: '../../images/return-img_03.jpg',
          ImgSize: 0,
          skuId: val.SkuId
        };
        tempscoregrade.push(scoregrade);
        temusercredentials.push(img);
        tempareaname.push('txt_' + val.SkuId);
      });
      that.setData({
        ProductList: res.Data,
        ScoreGrade: tempscoregrade,
        UserCredentials: temusercredentials,
        TxtareaName: tempareaname
      });
    }
    else {
        app.showErrorModal(result.data.Message, function (res) {
            if (res.confirm) {
                wx.navigateBack({ delta: 1 })
            }
        });
    }
  },
  ScoreGrade: function (e) {
    var grade = e.currentTarget.dataset.grade;
    var idx = e.currentTarget.dataset.index;
    var tempscoregrade = this.data.ScoreGrade;
    tempscoregrade[idx].grade = parseInt(grade);

    this.setData({
      ScoreGrade: tempscoregrade
    });
  },
  ChooseImg: function (e) {
      var that = this;
      var tempgreden = that.data.UserCredentials;
      var idx = e.currentTarget.dataset.index;
      var column = e.currentTarget.dataset.coloum;
      var skuid = e.currentTarget.dataset.skuid;
    var that = this;
    wx.chooseImage({
      success: function (res) {
        var filepath = res.tempFilePaths[0];
        
        if (column == 1) {
            tempgreden[idx].img1 = filepath;
        } else if (column == 2) {
            tempgreden[idx].img2 = filepath;
        } else {
            tempgreden[idx].img3 = filepath;
        }
        var imgidx = parseInt(tempgreden[idx].ImgSize);
        imgidx = imgidx >= 2 ? 2 : parseInt(imgidx + 1);

        tempgreden[idx].ImgSize = imgidx;
        that.setData({
            UserCredentials: tempgreden
        });
        that.UploadImg(filepath,e);
      },
    })
  },
  UploadImg: function (imgPath, e) {
      var that = this;
      var idx = e.currentTarget.dataset.index;
      var column = e.currentTarget.dataset.coloum;
      var skuid = e.currentTarget.dataset.skuid;
      that.setData({
          uploadComplete: false
      });
      app.getOpenId(function (openId) {
          wx.uploadFile({
              url: app.getUrl("OrderRefund/PostUploadAppletImage"),
              filePath: imgPath,
              name: 'file',
              formData: {
                  openId: openId
              },
              success: function (r) {
                  var tempgreden = that.data.UserCredentials;
                  var result = JSON.parse(r.data);
                  if (result.Status == "OK") {
                      
                      if (column == 1) {
                          tempgreden[idx].imgU1 = result.Data[0].ImageUrl;
                      } else if (column == 2) {
                          tempgreden[idx].imgU2 = result.Data[0].ImageUrl;
                      } else {
                          tempgreden[idx].imgU3 = result.Data[0].ImageUrl;
                      }
                      that.setData({
                          UserCredentials: tempgreden,
                          uploadComplete: true
                      });
                  } else {
                      that.setData({
                          uploadComplete: true
                      });
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
              }

          })
      })

  },
  formSubmit: function (e) {
    var that = this;
    if (that.data.isSubmit){
      return false;
    }
    var formId = e.detail.formId;
    var scorestr = that.data.ScoreGrade;//获取等级评分
    var areanamestr = that.data.TxtareaName;//获取控件id

    if (areanamestr.length <= 0) {
      app.showErrorModal('文本框不存在');
      return false;
    }
    var isnull = false;
    areanamestr.forEach(function (val, index, array) {
      if (that.ToTrim(e.detail.value[val]).length <= 0) {
        isnull = true;
        return;
      }
      scorestr[index].remark = that.ToTrim(e.detail.value[val]);
    });
    if (isnull) {
      app.showErrorModal('请输入评价内容');
      return false;
    }
    if (!that.data.uploadComplete) {
        app.showErrorModal('图片正在上传');
        return false;
    }
   
    that.setData({
      ScoreGrade: scorestr,
      isSubmit: true
    });
    var uploadImg={};
    that.data.UserCredentials.forEach(function (item, index, array) {
      var tempimg = [];
      if (item.img1 != '../../images/return-img_03.jpg') {
        tempimg.push(item.imgU1);
      }
      if (item.img2 != '../../images/return-img_03.jpg') {
        tempimg.push(item.imgU2);
      }
      if (item.img3 != '../../images/return-img_03.jpg') {
        tempimg.push(item.imgU3);
      }
      uploadImg[item.skuId] = tempimg;
    });
    that.setData({
        uploadImg: uploadImg
    });
    that.AddComments();
  },
  

  
  ToTrim: function (str) {
    return str.replace(/(^\s*)|(\s*$)/g, "");
  },
  AddComments: function () {
    var that = this;

    var tempconmment = that.data.ScoreGrade;
    var CommentInfoList = []; 
    var uploadimg = that.data.uploadImg;//获取已上传成功的图片集合
    tempconmment.forEach(function (item, index, array) {
      var currentinfo = {
        ProductId: item.skuId.substr(0, item.skuId.indexOf('_')),
        OrderId:that.data.OrderId,
        SkuId: item.skuId,
        ReviewText: item.remark,
        SkuContent: item.skucontent,
        Score:item.grade,
        ImageUrl1: ''
      };

      var key = item.skuId;
      if (uploadimg[key]!= undefined) {
        currentinfo.ImageUrl1 = uploadimg[key].join(",");
      }
      CommentInfoList.push(currentinfo);
    });

    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("product/GetAddProductReview"),
        data: {
          openId: openid,
          DataJson: CommentInfoList
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            wx.showModal({
              title: '提示',
              confirmColor: '#ff5722',
              content: result.data.Message,
              showCancel: false,
              success: function (res) {
                if (res.confirm) {
                  wx.redirectTo({
                    url: '../orderlist/orderlist'
                  });
                }
              }
            });
          } else {
            if (result.data.Message == 'NOUser') {
              wx.navigateTo({
                url: '../login/login'
              })
            }
            else {
              wx.showModal({
                title: '提示',
                confirmColor: '#ff5722',
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
        },
        complete: function () {

        }
      });
    });
  }
})