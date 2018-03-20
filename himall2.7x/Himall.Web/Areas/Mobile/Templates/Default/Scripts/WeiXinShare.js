//appId:必填，公众号的唯一标识
//timestamp:必填，生成签名的时间戳
//noncestr:必填，生成签名的随机串
//signature:必填，签名
//success:配置成功回调
//error:配置失败回调
//share.title:必填，分享标题
//share.desc:分享描述
//share.link:分享链接
//share.imgUrl:分享图标
//share.type:分享类型,music、video或link，不填默认为link
//share.dataUrl:如果type是music或video，则要提供数据链接，默认为空
//share.success:分享成功回调
//share.cancel:用户点击取消时的回调函数，仅部分有用户取消操作的api才会用到。
//share.fail:接口调用失败时执行的回调函数。
//share.complete:接口调用完成时执行的回调函数，无论成功或失败都会执行。
//share.trigger:监听Menu中的按钮点击时触发的方法，该方法仅支持Menu中的相关接口。
function initWinxin(options) {
	var defaultOption = {
		appId: null,
		timestamp: null,
		noncestr: null,
		signature: null,
		success: null,
		error: null,
		share: {
			title: null,
			desc: null,
			link: null,
			imgUrl: null,
			type: '',
			dataUrl: '',
			success: null,
			cancel: null,
			fail: null,
			complete: null,
			trigger: null
		}
	};

	options = $.extend(defaultOption, options);

	if (wx && isWeiXin()) {
		wx.ready(function () {
			var readyArguments = arguments;
			wx.checkJsApi({
				jsApiList: ['onMenuShareTimeline', 'onMenuShareAppMessage', 'onMenuShareQQ', 'onMenuShareWeibo', 'onMenuShareQZone'], // 需要检测的JS接口列表，所有JS接口列表见附录2,
				success: function (res) {
					var flag1 = res.checkResult['onMenuShareTimeline'] == true;
					var flag2 = res.checkResult['onMenuShareAppMessage'] == true;
					var flag3 = res.checkResult['onMenuShareQQ'] == true;
					var flag4 = res.checkResult['onMenuShareWeibo'] == true;
					var flag5 = res.checkResult['onMenuShareQZone'] == true;

					if (flag1)
						//获取“分享到朋友圈”按钮点击状态及自定义分享内容接口
						wx.onMenuShareTimeline(options.share);
					if (flag2)
						//获取“分享给朋友”按钮点击状态及自定义分享内容接口
						wx.onMenuShareAppMessage(options.share);
					if (flag3)
						//获取“分享到QQ”按钮点击状态及自定义分享内容接口
						wx.onMenuShareQQ(options.share);
					if (flag4)
						//获取“分享到腾讯微博”按钮点击状态及自定义分享内容接口
						wx.onMenuShareWeibo(options.share);
					if (flag5)
						//获取“分享到QQ空间”按钮点击状态及自定义分享内容接口
						wx.onMenuShareQZone(options.share);

					if (flag1 || flag2 || flag3 || flag4 || flag5) {
						if ($.isFunction(options.success))
							options.success.call(this, readyArguments);
					} else if ($.isFunction(options.error))
						options.error.call(this, readyArguments);
				}
			});
		});

		wx.error(function () {
			if ($.isFunction(options.error))
				options.error.call(this, arguments);
		});		
		wx.config({
			debug: false, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
			appId: options.appId, // 必填，公众号的唯一标识
			timestamp: options.timestamp, // 必填，生成签名的时间戳
			nonceStr: options.noncestr, // 必填，生成签名的随机串
			signature: options.signature,// 必填，签名，见附录1
			jsApiList: [
				'onMenuShareTimeline'
				, 'onMenuShareAppMessage'
				, 'onMenuShareQQ'
				, 'onMenuShareWeibo'
				, 'onMenuShareQZone'
			] // 必填，需要使用的JS接口列表，所有JS接口列表见附录2
		});
	} else if ($.isFunction(options.error)) {
		options.error();
	}
}

function isWeiXin() {
	var ua = window.navigator.userAgent.toLowerCase();
	if (ua.match(/MicroMessenger/i) == 'micromessenger') {
		return true;
	} else {
		return false;
	}
}