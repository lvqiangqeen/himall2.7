/*
  * 注意：
  * 1. 所有的JS接口只能在公众号绑定的域名下调用，公众号开发者需要先登录微信公众平台进入“公众号设置”的“功能设置”里填写“JS接口安全域名”。
  * 2. 需先引用 http://res.wx.qq.com/open/js/jweixin-1.0.0.js 文件
  */

wx.config({
    debug: false,
    appId: 'wxf8b4f85f3a794e77',
    timestamp: 1427962826,
    nonceStr: '9nxk5MBrj2pHIKgJ',
    signature: '343897920cc5652e0671859c50eafc6a1ceede4a',
    jsApiList: [
      'checkJsApi',
      'onMenuShareTimeline',
      'onMenuShareAppMessage',
      'onMenuShareQQ',
      'onMenuShareWeibo',
      'hideMenuItems',
      'showMenuItems',
      'hideAllNonBaseMenuItem',
      'showAllNonBaseMenuItem',
      'translateVoice',
      'startRecord',
      'stopRecord',
      'onRecordEnd',
      'playVoice',
      'pauseVoice',
      'stopVoice',
      'uploadVoice',
      'downloadVoice',
      'chooseImage',
      'previewImage',
      'uploadImage',
      'downloadImage',
      'getNetworkType',
      'openLocation',
      'getLocation',
      'hideOptionMenu',
      'showOptionMenu',
      'closeWindow',
      'scanQRCode',
      'chooseWXPay',
      'openProductSpecificView',
      'addCard',
      'chooseCard',
      'openCard'
    ]
});
wx.ready(function () {
    AddImgPreview();
});
function AddImgPreview() {

};