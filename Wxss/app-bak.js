App({
    //合并两个数组并去重 
    mergeArray: function (arr1, arr2) {
        for (var i = 0; i < arr1.length; i++) {
            for (var j = 0; j < arr2.length; j++) {
                if (arr1[i] === arr2[j]) {
                    arr1.splice(i, 1); //利用splice函数删除元素，从第i个位置，截取长度为1的元素
                }
            }
        }
        //alert(arr1.length)
        for (var i = 0; i < arr2.length; i++) {
            arr1.push(arr2[i]);
        }
        return arr1;
    },
    //从数组中删除指定值元素
    removeByValue: function (arr, val) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] == val) {
                arr.splice(i, 1);
                break;
            }
        }
        return arr;
    },
  onLaunch: function () {
  },
  //错误提示框
  showErrorModal: function (content,callback){
      wx.showModal({
          title: '提示',
          content: content,
          showCancel: false,
          confirmColor: '#ff5722',
          success: function (res) {
              callback &&  (callback(res));
          }
      })
  },
  getUserInfo: function (cb) {
    var that = this;
    if (that.globalData.userInfo && that.globalData.isReloadUser=='0') {
      typeof cb == "function" && cb(that.globalData.userInfo)
      wx.hideNavigationBarLoading();
    } else {
      that.globalData.isReloadUser='0';
      wx.showNavigationBarLoading(); //在标题栏中显示
      that.getOpenId(function (openid) {
        wx.request({
          url: that.getUrl(that.globalData.loginByOpenId),
          data: { openId: openid },
          success: function (result) {
            if (result.data.Status == "OK") {
              that.globalData.userInfo = result.data.Data;
              typeof cb == "function" && cb(that.globalData.userInfo)
            }
            else {
              wx.redirectTo({
                url: '../login/login'
              })
            }
          },
          complete: function () {
            // complete
            wx.hideNavigationBarLoading() //完成停止加载
            wx.stopPullDownRefresh() //停止下拉刷新
          }
        })
      })
    }
  },
  getOpenId: function (cb) {
    var that = this;
    if (that.globalData.openId != '' && that.globalData.openId != undefined) {
      typeof cb == "function" && cb(that.globalData.openId)
    } else {
      //调用登录接口
      wx.login({
        success: function (res) {
          if (res.code) {
            //发起网络请求
            wx.request({
              url: that.getUrl('login/GetOpenId'),
              data: {
                appid: that.globalData.appId,
                secret: that.globalData.secret,
                js_code: res.code,
              },
              success: function (result) {
                if (result.data != undefined && result.data.openid != undefined) {
                  that.globalData.openId = result.data.openid;
                  typeof cb == "function" && cb(that.globalData.openId)
                }
              }
            })
          } else {
            console.log('获取用户登录态失败！' + res.errMsg);
          }
        }
      })
    }
  },
  getWxUserInfo:function(cb){
    var that = this;
    if(that.globalData.wxUserInfo){
      typeof cb == "function" && cb(that.globalData.wxUserInfo)
    }else{
    //调用登录接口
      wx.login({success: function(res) {
            if (res.code) {
              const code = res.code;
              wx.getUserInfo({success: function (wxuser) {
              //发起网络请求
                wx.request({
                  url: that.getUrl('login/GetOpenId'),
                  data: {
                    appid: that.globalData.appId,
                    secret: that.globalData.secret,
                    js_code: code,
                  },
                  success: function (result) {
                    if (result.data != undefined && result.data.openid != undefined) {
                      const user = {
                        openId:result.data.openid,
                        nikeName:wxuser.userInfo.nickName,
                        unionId:'',
                        headImage:wxuser.userInfo.avatarUrl,
                        encryptedData: wxuser.encryptedData,
                        session_key: result.data.session_key,
                        iv:wxuser.iv
                      };
                      that.globalData.wxUserInfo = user;
                      typeof cb == "function" && cb(that.globalData.wxUserInfo)
                    }
                  }
                })
              }
            })
            } else {
              console.log('获取用户登录态失败！' + res.errMsg);
            }
          }
      })
    }
  },
  setUserInfo(userInfo){
      this.globalData.userInfo = userInfo;
  },
  orderPay(orderid,orderstatus,islistpage){
    var that = this;
    that.getOpenId(function (openid) {
      wx.request({
        url: that.getUrl(that.globalData.getPayParam),
        data: { 
          openId: openid,
          orderId:orderid
        },
        success: function (result) {
          if (result.data.Status == "OK") {
            var r = result.data.Data;
            wx.requestPayment({
              'timeStamp': r.timeStamp,
              'nonceStr': r.nonceStr,
              'package': 'prepay_id='+r.prepayId,
              'signType': 'MD5',
              'paySign': r.sign,
              'success':function(res){
                wx.showModal({
                  title: '提示',
                  content:"支付成功！",
                  showCancel:false,
                  success: function(res) {
                    if (res.confirm) {
                      wx.redirectTo({
                        url: '../orderlist/orderlist?status='+orderstatus
                      })
                    }
                  }
                })
              },
              'fail':function(res){
                wx.showModal({
                  title: '提示',
                  content:"支付失败！",
                  showCancel:false,
                  success: function(res) {
                    if(!islistpage){
                      if (res.confirm) {
                        wx.redirectTo({
                          url: '../orderlist/orderlist?status='+orderstatus
                        })
                      }
                    }
                  }
                })
              }
            })
          }
          else {
            wx.showModal({
              title: '提示',
              content:result.data.Message,
              showCancel:false,
              success: function(res) {
                if(!islistpage){
                  if (res.confirm) {
                    wx.redirectTo({
                      url: '../orderlist/orderlist?status='+orderstatus
                    })
                  }
                }
              }
            })
          }
        }
      })
    })
  },
  getRequestUrl:'https://www.hualanzi.cn/',
  getUrl(route){
    return `https://www.hualanzi.cn/SmallProgAPI/${route}`;
    },
  globalData:{
    appId:'wx242cf71c47293b29',
    secret:'7d09620bdb28671971a0ff2aeaab1333',
    userInfo:null,
    openId:'',
    wxUserInfo:null,
    isReloadUser:'0',

    loginByOpenId: "Login/GetLoginByOpenId",                         //根据OpenId判断是否有账号，根据OpenId进行登录
    loginByUserName: "Login/GetLoginByUserName",                     //账号密码登录
    quickLogin: "Login/GetQuickLogin",                               //一键登录
    prcesslogout:"Login/GetProcessLogout",

    getIndexData: "Login/GetIndexData",                     //获取首页数据 
    GetIndexProductData: "Login/GetIndexProductData",             //获取首页商品数据
    getProducts: "Product/GetProducts",                             //商品搜索
    getProductSkus:"Product/GetProductSkus",
    getProductDetail: "GetProductDetail",                   //获取商品详情
    getLimitBuyList:"LimitTimeBuy/GetLimitBuyList",
    getLimitBuyProduct: "LimitTimeBuy/GetLimitBuyProduct", //获取限时抢购商品详情
    userGetCoupon: "Coupon/GetUserCoupon",                         //领取优惠券
    loadCoupon: "Coupon/GetLoadCoupon",                               //获取优惠券列表数据
    LoadSiteCoupon: "LoadSiteCoupon",                               //获商城可领取优惠券列表

    getCartProduct:"Cart/GetCartProduct",                        //获取购物车列表
    getAddToCart:"Cart/GetAddToCart",
    getUpdateToCart: "Cart/GetUpdateToCart",                    //更新购物车    
    getUserShippingAddress: "ShippingAddress/GetList",       //获取用户收货地址
    addShippingAddress: "ShippingAddress/PostAddAddress",               //添加收货地址
    updateShippingAddress: "ShippingAddress/PostUpdateAddress",         //修改收货地址
    setDefaultShippingAddress: "ShippingAddress/GetSetDefault", //设置默认地址
    delShippingAddress: "ShippingAddress/GetDeleteAddress",               //删除地址
    AddWXChooseAddress:"ShippingAddress/PostAddWXAddress",        //添加微信收货地址
    orderList: "Order/GetOrders",                                 //获取订单列表
    closeOrder: "Order/GetCloseOrder",                               //取消订单
    finishOrder: "Order/GetConfirmOrder",                             //确认收货
    getLogistic: "Order/GetExpressInfo",                             //获取物流信息
    addProductReview:"",
    

    getPayParam: "Payment/GetPaymentList",                             //支付前检查并获取支付参数信息
    getShoppingCart: "GetShoppingCart",                     //获取提交订单页面所需信息
    sumbitOrder: "SumbitOrder",                              //提交订单
    getRegionsOfProvinceCity:"GetRegionsOfProvinceCity",//获取省市数据
    getRegions: "GetRegions",//获取区和街道数据,
    getAllCategories:"Category/GetAllCategories",//获取商品分类
    loadOrderProduct:"Order/GetOrderCommentProduct",//根据订单详情获取评价商品
    loadReview: "LoadReview",//评论列表
    loadCouponDetails:"Coupon/GetCouponDetail",
    getOrderDetail:"Order/GetOrderDetail",
    applyRefund: "OrderRefund/PostApplyRefund",
    getAfterSalePreCheck:"OrderRefund/AfterSalePreCheck",//退款退货申请前状态检测
    getAllAfterSaleList:"OrderRefund/GetList",
    getRefundDetail:"OrderRefund/GetRefundDetail",
    getReturnDetail:"OrderRefund/GetReturnDetail",
    getExpressList:"OrderRefund/GetExpressList",
    returnSendGoods:"OrderRefund/PostReturnSendGoods"
  }
})