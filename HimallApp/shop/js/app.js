/**
 * Five-xlx 2015.9.1
 * QQ:673921852
 **/ 
   var URL='http://119.29.164.13:5697/',
//var URL='http://192.168.10.87/',
	MAPKEY='SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP',
	TENXUNMAP='http://apis.map.qq.com/ws/coord/v1/translate',
	AppName='HiMall',
	app_key='himalltest';

(function($, app) {
	app.md5Data=function(oldData){
		var data=JSON.parse(JSON.stringify(oldData||{})) //保存原始数据
		data=data||{};
		data.app_key=app_key;
		data.timestamp =app.timeFormat();
		var keys=[];
		for(var k in data){
			if(data[k]===0||data[k]===false||data[k]){
				keys.push(k);
			}else{
				delete data[k];
			}
		}
		      
		keys=keys.sort();
		var signStr='';
		for(var i=0; i<keys.length; i++){
			signStr+=keys[i].toLowerCase()+data[keys[i]];
		}
		console.log(signStr);
		//plus.push.getClientInfo().appkey
		data.sign=md5(signStr+'has2f5zbd4');
		console.log(data.sign);
		return data;
		
	}
	
	app.immersed=function(){
		var immersed = 0;
		var ms=(/Html5Plus\/.+\s\(.*(Immersed\/(\d+\.?\d*).*)\)/gi).exec(navigator.userAgent);
		if(ms&&ms.length>=3){
		    immersed=parseFloat(ms[2]);
		}
		return immersed;
	}
	
	app.immersedCom=function(){
		var immersed=app.immersed();
		if(!immersed){
			return;
		}
		var head=document.querySelector('header'),
			content=document.querySelector('header~.mui-content'),
			scroll=document.querySelector('header~.scroll-div');
		if(head){
			head.style.paddingTop=immersed+'px';
		}
		if(content){
			content.style.paddingTop=head.offsetHeight+'px';
		}
		if(scroll){
			scroll.style.top=head.offsetHeight+'px';
		}
	}
	
	app.immersedSide=function(){
		var immersed=app.immersed();
		if(!immersed){
			return;
		}
		document.body.className+='immersed';
	}
	
	app.immersedNav=function(){
		var immersed=app.immersed();
		if(!immersed){
			return;
		}
		var nav=document.getElementById('fixedNav');
		if(nav){
			nav.style.top=immersed+44+'px';
		}
	}
	
	app.getState = function() {
		var stateText = localStorage.getItem('$state') || "{}";
		return JSON.parse(stateText);
	};

	app.setState = function(state) {
		
		state = state || {};
		localStorage.setItem('$state', JSON.stringify(state));
		plus.storage.setItem('appuserkey',state.userkey||'');
		plus.storage.setItem('appuserid',state.userId||'');
	};

	app.checkEmail = function(email) {
		email = email || '';
		return (email.length > 3 && email.indexOf('@') > -1);
	};
	
	app.checkPhone = function(phone) {
		phone = phone || '';
		var reg = /^0?(13|15|18|14|17)[0-9]{9}$/;
		return (reg.test(phone));
	};
	
	//判断是否是ios
	app.ios = function() {
		if (plus.os.name.toLocaleLowerCase() == 'ios') {
			return true;
		} else {
			return false;
		}
	};
	
	//IOS判断QQ是否已经安装
	app.isQQInstalled = function() {
		var TencentOAuth = plus.ios.import("TencentOAuth");
		var isQQInstalled = TencentOAuth.iphoneQQInstalled();
		return isQQInstalled == '0' ? false : true
	};
	//IOS判断微信是否已经安装
	app.isWXInstalled = function() {
		var Weixin = plus.ios.import("WXApi");
		var isWXInstalled = Weixin.isWXAppInstalled();
		return isWXInstalled == '0' ? false : true;
	};
	
	/*大于零显示元素*/
	app.whichShow = function(data,el) {
		if(data>0){
			document.getElementById(el).innerText=data.toString();
			document.getElementById(el).style.display='block';
		}else{
			document.getElementById(el).style.display='none';
		}
			
	}
	
	/*不为空显示*/
	app.nullShow = function(data,el) {
		if(data!=''&&data!=null){
			document.getElementById(el).innerText=data;
			document.getElementById(el).style.display='block';
		}else{
			document.getElementById(el).style.display='none';
		}
			
	}
	
	//阻止a链接跳转
	app.stopHref = function(el) {
		el.on('tap', 'a', function(e) {
			e.preventDefault();
		});
	}
	
	app.trim = function(str) {
		return str.replace(/(^\s*)|(\s*$)/g, "");
	}
	
	app.removeClass = function(el,name) {
		if(!el)
			return;
		if(el.className.indexOf(name)>=0)
			el.className=el.className.replace(name,'');
	}
	app.removeAllClass = function(el,name) {
		if(!el)
			return;
		if(el.className.indexOf(name)>=0){
			var reg = new RegExp(name,"g");
			el.className=el.className.replace(reg,'');
		}
			
	}
	
	app.hasClass = function(el,name) {
		return (el.className.indexOf(name)>=0);
	}
	
	app.selectValue = function(el) {
		return el.options[el.selectedIndex].value;
	}
	
	app.isLogin = function(){
		var userState = JSON.parse(localStorage.getItem('$state') || "{}");
		if (userState.userkey && userState.userkey != '') {
			return true;
		} else {
			return false;
		}
	}
	
	//QQ在线咨询
	app.openQQ=function(qq) {
//		if(!himall.isQQInstalled()){  
//			mui.alert('你的手机尚未安装QQ');
//			return ;  
//		}
		if ( plus.os.name == "Android" ) {
			plus.runtime.openURL("mqqwpa://im/chat?chat_type=wpa&uin="+qq);
		} else if ( plus.os.name == "iOS" ) {
			plus.runtime.openURL("mqq://im/chat?chat_type=wpa&uin="+qq+"&version=1&src_type=web");
		}
	}
	
	
	//重写退出应用
	app.quitApp = function() {
		$.oldBack = mui.back;
		var backButtonPress = 0;
		$.back = function(event) {
			backButtonPress++;
			if (backButtonPress > 1) {
				plus.runtime.quit();
			} else {
				plus.nativeUI.toast('再按一次退出'+AppName);
			}
			setTimeout(function() {
				backButtonPress = 0;
			}, 1000);
			return false;
		};
	}
	
	//复制到剪切板
	app.copyclip = function(text) {
		if (plus.os.name.toLocaleLowerCase() == 'ios') {
			var UIPasteboard  = plus.ios.importClass("UIPasteboard");
			var generalPasteboard = UIPasteboard.generalPasteboard();
			generalPasteboard.setValueforPasteboardType(text, "public.utf8-plain-text");
			//var value = generalPasteboard.valueForPasteboardType("public.utf8-plain-text"); 
			mui.toast('复制成功');
		} else {
			var Context = plus.android.importClass("android.content.Context");
			var main = plus.android.runtimeMainActivity();
			var clip = main.getSystemService(Context.CLIPBOARD_SERVICE);
			plus.android.invoke(clip, "setText", text);
			mui.toast('复制成功');
		}
	};
	
	//打开软键盘
	app.openSoftKeyboard = function() {
		if (mui.os.ios) {
			var webView = plus.webview.currentWebview().nativeInstanceObject();
			webView.plusCallMethod({
				"setKeyboardDisplayRequiresUserAction": false
			});
		} else {
			var webview = plus.android.currentWebview();
			plus.android.importClass(webview);
			webview.requestFocus();
			var Context = plus.android.importClass("android.content.Context");
			var InputMethodManager = plus.android.importClass("android.view.inputmethod.InputMethodManager");
			var main = plus.android.runtimeMainActivity();
			var imm = main.getSystemService(Context.INPUT_METHOD_SERVICE);
			imm.toggleSoftInput(0, InputMethodManager.SHOW_FORCED);
		}
	};
	
	//时间处理
	app.countDown=function(time,callback){
		var day = 0,
            hour = 0,
            minute = 0,
            second = 0;
        if ( time > 0 ) {
            day = '' + Math.floor( time/( 24*60*60 ) );
            hour = '' + Math.floor( time/(60*60) - (day*24) );
            minute = '' + Math.floor( time/60 - (day*24*60) - (hour*60) );
            second = '' + Math.floor( time - (day*24*60*60) - (hour*60*60) - (minute*60) );
        }
        if ( day <10 ) { day = '0' + day; }
        if ( hour <10 ) { hour = '0' + hour; }
        if ( minute <10 ) { minute = '0' + minute; }
        if ( second <10 ) { second = '0' + second; }
        callback(day,hour,minute,second);
	}
	
	app.initSwiper=function(){
		var swiper = new Swiper('.swiper-container', {
			pagination: '.swiper-pagination',
			autoplay:4000,
			speed:500
		});
	}
	
	//打开新窗口
	app.openVW=function(id,extras){
		$.openWindow({
			id:id,
			url:id,
			extras:extras,
			show: {
				autoShow:true,
				aniShow: 'pop-in',
				duration:300
			},
			waiting: {
				autoShow: false
			}
		});
	}
	//双击头部返回顶部
	app.backTop=function(){
        document.querySelector('header').addEventListener('doubletap', function() {
            mui('#pullrefresh').pullRefresh().scrollTo(0, 0, 100);
        })
    }
	
	app.timeFormat=function(){
		var d=new Date(),
			year=d.getFullYear(),
			month=d.getMonth()+1,
			day=d.getDate(),
			hours=d.getHours(),
			minutes=d.getMinutes(),
			seconds=d.getSeconds();
		
		if(month<10) month='0'+month;
		if(day<10) day='0'+day;
		if(hours<10) hours='0'+hours;
		if(minutes<10) minutes='0'+minutes;
		if(seconds<10) seconds='0'+seconds;
			
		return (year+'-'+month+'-'+day+' '+hours+':'+minutes+':'+seconds);
	}
	
	app.parseDomImg=function(str){
		var objE = document.createElement("div");
		objE.innerHTML = str;
		
		var imgs=objE.querySelectorAll('img');
		for(var i=0; i<imgs.length; i++){
			var img=imgs[i];
			img.setAttribute('data-delay',img.src);
			img.src='images/blank.gif';
			img.setAttribute('height','100px');
			img.setAttribute('width','100%');
		}
		return objE.innerHTML;
	};
	
	app.customHref=function(){
		mui('.mui-content').on('tap', '.custom-href', function() {
			var href=this.getAttribute('data-href').toLowerCase(),
				id;
			if(href.indexOf(URL)>=0){
				if(href.indexOf('product/detail/')>=0){
					id=href.split('product/detail/')[1];
					showProduct(id);
				}else if(href.indexOf('topic/detail/')>=0){
					id=href.split('topic/detail/')[1];
					himall.openVW('special.html',{topicId:id})   
				}else if(href.indexOf('gift/detail/')>=0){
					id=href.split('gift/detail/')[1];
					himall.openVW('integral-detail.html',{giftId:id})
				}else{
					plus.runtime.openURL(href)
				}
			}else{
				plus.runtime.openURL(href)
			}
			
		});
	}
	
	app.update=function(){
		setTimeout(function(){
			mui.ajax(URL+'api/home/GetUpdateApp',{
				data:app.md5Data({
					appVersion:plus.runtime.version,
					type:plus.os.name=='Android'?2:3
				}),
				dataType:'json',
				type:'get',
				timeout:10000,
				success:function(data){
					if(data.Success=='true'){
						plus.nativeUI.confirm('发现新版本'+AppName+'，马上体验吧！\r\n更新说明：\r\n'+data.Description, function(event){
							if ( 0==event.index ) {
								plus.runtime.openURL(data.DownLoadUrl);
							}
						}, '', ["立即更新","取 消"] );
					}
				}
			});
		},5000)
	}
	
	app.login = function(loginInfo, callback) {
		callback = callback || $.noop;
		loginInfo = loginInfo || {};
		loginInfo.account = loginInfo.account || '';
		loginInfo.password = loginInfo.password || '';
		if (loginInfo.account.length < 4) {
			return callback('账号最短为 4 个字符');
		}
		if (loginInfo.password.length < 6) {
			return callback('密码最短为 6 个字符');
		}
		var waitLogin = plus.nativeUI.showWaiting();
		$.ajax(URL+'api/Login/GetUser',{
			data:app.md5Data({
				userName:loginInfo.account,
				password:loginInfo.password
			}),
			dataType:'json',
			type:'get',
			timeout:20000,
			success:function(data){
				console.log(JSON.stringify(data));
				waitLogin.close();
				if(data.Success=="true"){
					return app.createState(loginInfo.account,data.UserId,data.UserKey,data.IsPromoter,callback);
				}else{
					callback(data.ErrorMsg);
				}
			},
			error:function(xhr,type,errorThrown){
				waitLogin.close();
				callback('请求失败，请检查网络');
			}
		});
	};

	app.createState = function(name,userId,userkey,isPromoter,callback) {
		var state = app.getState();
		state.account = name;
		state.userId = userId;
		state.userkey = userkey;
		state.isPromoter = isPromoter;
		app.setState(state);
		return callback();
	};

	app.reg = function(regInfo, callback) {
		callback = callback || $.noop;
		regInfo = regInfo || {};
		regInfo.account = regInfo.account || '';
		regInfo.password = regInfo.password || '';
		var re=/[^\d]+/;
		if (regInfo.account.length < 4 || !re.test(regInfo.account)) {
			return callback('用户名最短 4 个字符且非纯数字');
		}
		if (regInfo.password.length < 6) {
			return callback('密码最短需要 6 个字符');
		}
		var waitReg = plus.nativeUI.showWaiting();
		$.ajax(URL+'api/Register/PostRegisterUser',{
			data: app.md5Data({
				userName:regInfo.account,
				password:regInfo.password,
				email:regInfo.email,
				code:regInfo.emailCode
			}),
			dataType:'json',
			type:'post',
			timeout:10000,
			success:function(data){
				waitReg.close();
				if(data.Success=="true"){
					return callback(null,data);
				}else{
					return callback(data.ErrorMsg);
				}
			},
			error:function(xhr,type,errorThrown){
				waitReg.close();
				return callback(xhr.responseText.split('ErrorMsg =')[1])
			}
		});
		
	};
	app.goBackIndex = function() {
		mui.plusReady(function() {
			var subpages = ['home', 'home.html', 'vshop', 'vshop.html', 'usercenter', 'usercenter.html', 'index.html', 'index', 'HBuilder','product-detail','product-detail.html'];
			var current = plus.webview.currentWebview();
			var all = plus.webview.all();
			for(var i = 0; i < all.length; i++) {

				if(all[i] == plus.webview.getLaunchWebview() || all[i].id == current.id || all[i].id == subpages[0] || all[i].id == subpages[1] ||
					all[i].id == subpages[2] || all[i].id == subpages[3] ||
					all[i].id == subpages[4] || all[i].id == subpages[5] ||
					all[i].id == subpages[6] || all[i].id == subpages[7] || all[i].id == subpages[8]|| all[i].id == subpages[9] || all[i].id == subpages[10]) {
					continue;
				}
				all[i].close("none");
			}
			setTimeout(function() {
				current.close();
			}, 50)

		})

	};
	
}(mui, window.himall = {}));

himall.immersedCom();

//获取索引
function getIndex(el){
	var child=el.parentNode.children;
	for(var i=0; i<child.length; i++){
		if(el==child[i])
			return i;
	}
}

//是否为空对象
function isEmptyObject(el){
    for(var n in el){return false} 
    return true; 
}

function siblings(el,childEl) {
	var r = [],n;
	if(!childEl)
		n = el.parentNode.children;
	else
		n = el.parentNode.querySelectorAll(childEl);
	for(var i =0,pl= n.length;i<pl;i++) {
		if(n[i] !== el) 
			r.push(n[i]);
	}
	return r;
}


function showLogin(params){
	//plus.nativeUI.toast('请先登录');
	mui.openWindow({
		id:'login.html',
		url:'login.html',
		extras:{
			params:params
		},
		show: {
			autoShow:true,
			aniShow: 'zoom-fade-out'
		},
		waiting: {
			autoShow: false
		}
	});
}


function showProduct(id){  
	mui.fire(plus.webview.getWebviewById('product-detail.html'),'updateData',{productId:id});
	mui.openWindow({
		id:'product-detail.html',
		url:'product-detail.html',
		show: {
			autoShow:true,
			aniShow: 'pop-in',
			duration:300
		},
		waiting: {
			autoShow: false
		}
	});     
}

// 支付模块
function checkServices(pc){
	if(!pc.serviceReady){
		var txt=null;
		switch(pc.id){
			case "alipay":
			txt="检测到系统未安装“支付宝快捷支付服务”，无法完成支付操作，是否立即安装？";
			break;
			default:
			txt="系统未安装“"+pc.description+"”服务，无法完成支付，是否立即安装？";
			break;
		}
		plus.nativeUI.confirm(txt,function(e){
			if(e.index==0){
				pc.installService();
			}
		},pc.description);
	}
}

function initPay(dcontent){
	plus.payment.getChannels(function(channels){
		var content=document.getElementById(dcontent);
		for(var i in channels){
			var channel=channels[i];
			if(channel.id=='alipay' || channel.id=='wxpay'){
				if (himall.ios()) {
					if (channel.id == 'wxpay' && !himall.isWXInstalled()) {
						continue;
					}
				}
				pays[channel.id]=channel;
				var de=document.createElement('div');
				de.setAttribute('class','custom-btn '+channel.id);
				//de.setAttribute('onclick','pay(this.id)');
				de.id=channel.id;
				de.innerText=channel.description+"支付";
				content.appendChild(de);
				if (!himall.ios()) {
					checkServices(channel);
				}
			}
		}
	},function(e){
		plus.nativeUI.alert("获取支付方式失败："+e.message);
	});
}

function payOrder(id,payOrderId,successBack,errorBack){
	var typeid;
	var _st;
	if(wPay){
		mask.close();
		return;
	}//检查是否请求订单中
	if(id=='alipay'){
		typeid='Alipay_App';
	}else if(id=='wxpay'){
		typeid='WeiXinPay_App';
	}else{
		plus.nativeUI.alert("不支持此支付通道！");
		return;
	}
	wPay=plus.nativeUI.showWaiting();
	setTimeout(function(){
		if(wPay){
			wPay.close();
			wPay=null;
			mask&&mask.close();
		}
	},30000);
	// 请求支付订单
	mui.ajax(URL + 'api/payment/GetPaymentList', {
    	data:himall.md5Data({
			orderIds: payOrderId,
            typeid: typeid,
            userkey:himall.getState().userkey
		}),
        dataType:'json',
        type:'get',
        timeout: 10000,
        success: function(data) {
        	if(!data[0]){
        		wPay.close();wPay=null;
        		plus.nativeUI.alert('支付配置无效，请联系管理员');
        		return;
        	}
        	var order=data[0].url;
			if(id=='wxpay'){
				order=JSON.parse(order);
			}
			pollingOrderPayStatus(payOrderId,successBack,errorBack);
			plus.payment.request(pays[id],order,function(result){
				wPay.close();wPay=null;
				//successBack();
			},function(e){
				wPay.close();wPay=null;
				errorBack();
			});
        }
    });
}

//轮询订单支付状态
function pollingOrderPayStatus(orderIds,successBack,errorBack)
{
	// 请求支付订单
	mui.ajax(URL + 'api/MemberOrder/GetOrerStatus', {
    	data:himall.md5Data({
			orderIds: orderIds,
            userkey:himall.getState().userkey
		}),
        dataType:'json',
        type:'get',
        timeout: 10000,
        success: function(data) {
        	if(data && data.Success){
        		var item = data.list[0];
        		console.log(JSON.stringify(item));
        		if(item.status==1){
        			setTimeout(function(){
        				pollingOrderPayStatus(orderIds,successBack,errorBack)
        			}, 2000);
        		}else{
        			plus.nativeUI.closeWaiting();
        			successBack(item);
        			return;
        		}
        	}
        },
        error: function(xhr, type, errorThrown) {
        	plus.nativeUI.closeWaiting();
        	errorBack();
        }
    });
}

//分享模块
function shareWeixin(_this,mask){
	var w=plus.nativeUI.showWaiting('',{padlock:true}),
		msg = {
			extra: {
				scene: 'WXSceneSession'
			}
		};
	var shares = {};
	plus.share.getServices(function(s) {
		if (s && s.length > 0) {
			for (var i = 0; i < s.length; i++) {
				var t = s[i];
				shares[t.id] = t;
			}
			var share=shares['weixin'];
			msg.href = _this.getAttribute('data-href');
			msg.title = _this.getAttribute('data-title');
			msg.content = _this.getAttribute('data-content');
			msg.pictures = ["_www/images/logo.png"];
			msg.thumbs = ["_www/images/logo.png"];
			share.send(msg, function() {
				w.close();
				if(mask)
					mask.close();
				plus.nativeUI.toast("分享到" + share.description + "成功！ ");
			}, function(e) {
				w.close();
				plus.nativeUI.toast("分享到" + share.description + "取消");
			});
		}
	}, function() {
		w.close();
		plus.nativeUI.toast('获取分享列表失败');
	});
}

function initShare(){
	var shares = {};
	plus.share.getServices(function(s) {
		if (s && s.length > 0) {
			for (var i = 0; i < s.length; i++) {
				var t = s[i];
				shares[t.id] = t;
			}
			return shares;
		}
	}, function() {
		plus.nativeUI.toast('获取分享列表失败');
	});
	return shares;
}

function loadShare(params){
	var ids=[],
		bts=[];
	if (himall.ios()) {
		if (himall.isWXInstalled()) {
			ids.push({id: "weixin",ex: "WXSceneSession"}, {id: "weixin",ex: "WXSceneTimeline"});
			bts.push({title: "发送给微信好友"}, {title: "分享到微信朋友圈"});
		}
		if (himall.isQQInstalled()) {
			ids.push({id: "qq"});
			bts.push({title: "分享给QQ好友"});
		}
	}else{
		ids.push({id: "weixin",ex: "WXSceneSession"}, {id: "weixin",ex: "WXSceneTimeline"},{id: "qq"});
		bts.push({title: "发送给微信好友"}, {title: "分享到微信朋友圈"},{title: "分享给QQ好友"});
	}
	var shares=initShare();
	plus.nativeUI.actionSheet({
		cancel: "取消",
		buttons: bts
	}, function(e) {
		var i = e.index;
		if (i > 0) {
			var s_id = ids[i - 1].id;
			var share = shares[s_id];
			if (share.authenticated) {
				shareMessage(share, ids[i - 1].ex, params);
			} else {
				share.authorize(function() {
					shareMessage(share, ids[i - 1].ex, params);
				}, function(e) {
					plus.nativeUI.toast("认证授权失败!");
				});
			}
		}
	});
}

function shareMessage(share, ex,params) {
	var msg = {
		extra: {
			scene: ex
		}
	};
	msg.href = params.href;
	msg.title = params.title;
	msg.content = params.content;
	/*if (~share.id.indexOf('weibo')>=0) {
		msg.content += '，<a href="'+ProductHref+'">Go血拼！ ></a>';
	}*/
	//msg.thumbs=productImg;
	//msg.pictures=productImg;
	msg.pictures = [""+params.pictures+""];
	msg.thumbs = [""+params.pictures+""];
	share.send(msg, function() {
		plus.nativeUI.toast("分享到" + share.description + "成功！ ");
		if (typeof params.callBack == 'function') {
			params.callBack();
		}
	}, function(e) {
		plus.nativeUI.toast("分享到" + share.description + "取消");
	});
}


var himallSku={
	checkDo:function(el,data,skuId,productId){
		for(var i=0; i<el.length; i++){
        	var that=el[i];
        	skuId[parseInt(that.getAttribute('st'))] = that.getAttribute('cid');
        	if(!data[productId + '_' + skuId.join('_')]){
        		that.className='disabled';
        	}else{
        		that.className=that.className.replace('disabled','enabled');
        	}
        }
	},
	skuBind:function(data,option){
		var skuId =[0,0,0],
			self=this;
        if(data.length==0){     	
            if (typeof option.noStockCallBack == 'function') {
                option.noStockCallBack();
            }
        	plus.nativeUI.toast('该宝贝已卖光了');   
        }
        
        //sku重组
    	var skuArr={};
    	for(var i=0; i<data.length; i++){
    		skuArr[data[i].SkuId]=data[i];
    	}
        
        if(option.skuLen==1){
        	mui('#choose .enabled').each(function(){
		        skuId[parseInt(this.getAttribute('st'))] = this.getAttribute('cid');
		        if(!skuArr[option.productId + '_' + skuId.join('_')]){
		        	this.className=this.className.replace('disabled','enabled');
	        	}
        	});
        }
        if(option.skuLen==2){
        	mui('#choose').on('tap','.enabled',function () {
		        skuId[parseInt(this.getAttribute('st'))] = this.getAttribute('cid');
		        
		        var sibC=siblings(this.parentNode.parentNode,'.choose-sku')[0].getElementsByTagName('span');
		        self.checkDo(sibC,skuArr,skuId,option.productId);
		        
        	});
        }
        if(option.skuLen==3){
        	mui('#choose').on('tap','.enabled',function () {
		        skuId[parseInt(this.getAttribute('st'))] = this.getAttribute('cid');
		    	var sibSku=siblings(this.parentNode.parentNode,'.choose-sku'),
		    		sibOne=sibSku[0].querySelector('.selected');
		    	if(sibOne){
			        skuId[parseInt(sibOne.getAttribute('st'))] = sibOne.getAttribute('cid');
			        self.checkDo(sibSku[1].getElementsByTagName('span'),skuArr,skuId,option.productId);
		       	}
		    	var sibTwo=sibSku[1].querySelector('.selected');
        		if(sibTwo){
			        skuId[parseInt(sibTwo.getAttribute('st'))] = sibTwo.getAttribute('cid');
			        self.checkDo(sibSku[0].getElementsByTagName('span'),skuArr,skuId,option.productId);
		        }
        	});
        }
        mui('#choose').on('tap','.enabled',function () {
        	if(himall.hasClass(this,'selected')){
        		return;
        	}
        	if(this.parentNode.querySelector(".selected")){
        		var selected=this.parentNode.getElementsByClassName('selected')[0];
        		selected.className=selected.className.replace(' selected','');
        	}
            this.className+=' selected';
            if(this.getAttribute('data-img')&&this.getAttribute('data-img')!=''){
            	document.getElementById("colorImg").src=this.getAttribute('data-img');
            }
            var len = choose.getElementsByClassName('selected').length;
            if(len===option.skuLen){
			    for (var i = 0; i < len; i++) {
			    	var cSelect=choose.getElementsByClassName('selected')[i];
			        skuId[parseInt(cSelect.getAttribute('st'))] = cSelect.getAttribute('cid');
			    }
			    var select=skuArr[option.productId + '_' + skuId.join('_')];
			    document.getElementById('pPrice').innerText='¥ '+select.Price.toFixed(2);
			    document.getElementById('stock').innerText=select.Stock;
			    option.callBack(select);
		    }
        });
        if (option.skuLen != 0){
		    mui(".choose-sku").each(function () {
		    	if(this.getElementsByClassName('enabled')[0]){
		    		mui.trigger(this.getElementsByClassName('enabled')[0],'tap');
		    	}
		    });
		}else if(option.skuLen==0 && data.length!=0){
			document.getElementById('pPrice').innerText='¥ '+data[0].Price.toFixed(2);
			document.getElementById('stock').innerText=data[0].Stock;
			option.callBack(data[0]);
		}
	},
	setSKUInfo:function(option){
		var SKUDATA = null,
			self=this;
		if(!option.data){
			mui.ajax(URL+option.ajaxUrl,{
				data:himall.md5Data({productId:option.productId,userkey:userkey}),
				dataType:'json',
				type:'get',
				timeout:20000,
				success:function(data){
					self.skuBind(data.SkuArray,option);
				}
			});
		}else{
			self.skuBind(option.data,option);
		}
	}
}


var commonFn={
	favShop:function(userkey){
		mui('.mui-content').on('tap', '.favShop', function() {
			var _this=this;
			if(himall.isLogin()){
				var w=plus.nativeUI.showWaiting('',{padlock:true});
				var shopid=_this.getAttribute('data-shopid');
				mui.ajax(URL+'api/VShop/PostAddFavoriteShop',{
					data: himall.md5Data({
                    	shopId:shopid,
                    	userkey:userkey
                    }),
					dataType:'json',
					type:'POST',
					timeout:10000,
					success:function(data){
						w.close();
						if(data.Success=="true"){
							plus.nativeUI.toast(data.Msg);
							if(himall.hasClass(_this,'red')){
								himall.removeClass(_this,'red');
							}else{
								_this.className+=' red';
							}
						}else{
							plus.nativeUI.toast('关注店铺失败');
						}
					},
					error:function(xhr,type,errorThrown){
						w.close();
						plus.nativeUI.toast('关注失败，请检查网络')
					}
				});
			}else{
				showLogin();
			}
		});
	}
}

mui('body').on('tap', '#closeWv', function() {
	if(plus.webview.currentWebview().parent()!=null)
		plus.webview.currentWebview().parent().close();
	else
		plus.webview.currentWebview().close();
});

mui('body').on('tap', '#reloadWv', function() {
	plus.webview.currentWebview().reload();
});
function reloadWvLoad(){
	var errorText = document.createElement('div');
    errorText.innerHTML = '<h4>网络不给力，请检查网络！</h4><button id="reloadWv" class="mui-btn mui-btn-negative">重新加载</button>';
    errorText.setAttribute('class','empty-show');
    document.body.appendChild(errorText);
}

function customerService(data){
	if(document.getElementsByClassName('customer-service').length>0||!data||data.length==0){  
		return;
	}  
	var div=document.createElement('div');
	div.className='customer-service';
	var html="";
	var account="";
	console.log(JSON.stringify(data));  
	function a(){
		 console.log("123");
	} 
	for(var i=0;i<data.length;i++){   
		if(data[i].Tool==1){
		   html+='<div class="service_list-item qq" data-ac="'+data[i].AccountCode+'"></div>'; 
		}else
		if(data[i].Tool==3){
		   html+='<div  class="service_list-item mq"  data-ac="'+data[i].AccountCode+'"></div>';
		}     
	}
	console.log()
	div.innerHTML='<div class="keng-lb-1"><span class="lb-1"><i></i><em>客服</em></span></div><div style="width:'+((data.length*36)+54)+'px;display:none;" class="service_list">'+html+'</div>'; 
	document.body.appendChild(div); 
	mui("body").on("tap",".service_list-item",function(){  
		  var ac=this.getAttribute('data-ac');   
		  console.log("acappjs:"+ac);
		  if(this.className.indexOf(' qq')!=-1){
		  	 himall.openQQ(ac);  
		  }else{
		  	 console.log("进来了")
		  	 mui.openWindow({
		  	 	id:'meiqia.html',
		  	 	url:'meiqia.html',
		  	 	extras:{ac:ac}
		  	 })
		  }
	});
	mui("body").on("tap",".keng-lb-1",function(){
		   if(mui('.service_list')[0].style.display=="none"){
		          mui('.service_list')[0].style.display='block';
		          mui(".lb-1 em")[0].style.display='none';
		          mui(".lb-1 i")[0].style.marginTop='5px';
		   }else{
		   	      mui('.service_list')[0].style.display='none';
		          mui(".lb-1 em")[0].style.display='block';
		          mui(".lb-1 i")[0].style.marginTop='0px';
		   }
	});
}
  
