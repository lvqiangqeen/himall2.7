// pages/commentlist/commentlist.js
var config = require("../../utils/config.js");
var app = getApp();


Page({

  /**
   * 页面的初始数据
   */
  data: {
    ReviewInfo:null,
    positive: 0,
    commentList:null,
    pageIndex: 1,
    pageSize:10,
    commentType: 0,
    ProductId:null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    // 页面初始化 options为页面跳转所带来的参数
    var that = this;
    var ProductId = options.id;
   that.setData({
     ProductId: ProductId
   });
    wx.request({
      url: app.getUrl("product/GetStatisticsReview"),
      data: {
        ProductId: ProductId
      },
      success: function (result) {
        if (result.data.Status == "OK") {
          var r = result.data.Data;
          var positive = (r.reviewNum1 / r.reviewNum).toFixed(4) * 100;
          that.setData({
            ReviewInfo: r,
            positive: positive
          });
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
    });
    that.loadData(that,false);
  },
  prevImage:function(e){
    var that=this;
    var idx=e.target.dataset.index;
    var current = e.target.dataset.src;

    var previmglist = [];
    var item=that.data.commentList[idx]
      if (item.ImageUrl1 != '') {
        previmglist.push(item.ImageUrl1);
      }
      if (item.ImageUrl2 != '') {
        previmglist.push(item.ImageUrl2);
      }
      if (item.ImageUrl3 != '') {
        previmglist.push(item.ImageUrl3);
      }
      if (item.ImageUrl4 != '') {
        previmglist.push(item.ImageUrl4);
      }
      if (item.ImageUrl5 != '') {
        previmglist.push(item.ImageUrl5);
      }
 
    that.setData({
      ImgList: previmglist
    });



    wx.previewImage({
      current: current, 
      urls: that.data.ImgList
    });
  },
  loadData:function(that,isNextpage){
    
    app.getOpenId(function (openid) {
      wx.request({
        url: app.getUrl("product/GetLoadReview"),
        data: {
          openId: openid,
          PageIndex: that.data.pageIndex,
          PageSize: that.data.pageSize,
          type: that.data.commentType,
          ProductId: that.data.ProductId
        },
        success: function (result) {
          console.log(result);
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            if (isNextpage) {
              var old = that.data.commentList;
              old.push.apply(old, r);
              that.setData({
                commentList: old
              })
            } else {
              that.setData({
                commentList: r
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
    });
  },

  bingComment: function (e) {
    var that = this;
    var typeId = e.currentTarget.dataset.typeid;
    that.setData({
      pageIndex: 1,
      commentType:typeId
    })
    that.loadData(that,false);
  },
  onReachBottom: function () {
    var that = this;
   var oldpageindex = that.data.pageIndex;
   oldpageindex = parseInt(oldpageindex)+1;
   that.setData({
      pageIndex: oldpageindex
    });
   that.loadData(that, true);
  }

})