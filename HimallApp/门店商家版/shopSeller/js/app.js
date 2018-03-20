/**
 * Five-xlx 2015.9.1
 * QQ:673921852
 **/
var URL='http://119.29.164.13:5697/',
//var URL='http://192.168.10.87/',
	AppName='HiMall商家',
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
//		data.sign=md5(signStr+'plus.push.getClientInfo().appkey');
		data.sign=md5(signStr+'has2f5zbd4');
		return data;
		
	}
	
	app.isJoin = function(){
		var userState = JSON.parse(localStorage.getItem('$state') || "{}");
		if (userState.type ==1) {
			return true;
		} else {
			return false;
		}
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
		if(immersed){
			return;
		}
		var head=document.querySelector('header'),
			content=document.querySelector('header~.mui-content'),
			scroll=document.querySelector('header~.scroll-div');
		if(head){
			head.style.paddingTop=0;
		}
		if(content){
			content.style.paddingTop=head.offsetHeight+'px';
		}
		if(scroll){
			scroll.style.top=head.offsetHeight+'px';
		}
	}
	
	app.immersedNav=function(){
		var immersed=app.immersed();
		if(immersed){
			return;
		}
		var nav=document.getElementById('fixedNav');
		if(nav){
			nav.style.top=44+'px';
		}
	}
	
	app.immersedSide=function(){
		var immersed=app.immersed();
		if(immersed){
			return;
		}
		var side=document.getElementById('filterBox');
		if(side){
			side.style.top=44+'px';
		}
	}
	
	app.immersedNoTop=function(){
		var immersed=app.immersed();
		if(immersed){
			return;
		}
		
		document.body.className+=' notimmersed';
	}
	
	app.getState = function() {
		var stateText = localStorage.getItem('$state') || "{}";
		return JSON.parse(stateText);
	};

	app.setState = function(state) {
		state = state || {};
		localStorage.setItem('$state', JSON.stringify(state));
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
		if(!isQQInstalled()){
			mui.alert('你的手机尚未安装QQ');
			return ;
		}
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
	app.openVW=function(id,extras,url){
		$.openWindow({
			id:id,
			url:url||id,
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
	
	app.timeFormat=function(time,params){
		var d=time?new Date(time):new Date(),
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
		
		if(params){
			return {year:year,month:month,day:day};
		}else{
			return (year+'-'+month+'-'+day+' '+hours+':'+minutes+':'+seconds);
		}
	}
	
	app.timeSection=function(d,el){
		var nowDate=app.timeFormat('','arg'),
			year=nowDate.year,
			month=nowDate.month,
			day=nowDate.day,
			resultStart,
			resultEnd;
		switch(d){
			case '1':
				resultStart=resultEnd=year+'-'+month+'-'+day;
				break;
			case '7':
				resultEnd=year+'-'+month+'-'+day;
		        var first =[7,1,2,3,4,5,6][new Date().getDay()],
		        	f = new Date(),
		        	f = f.setDate(f.getDate()-first+1);
		        resultStart=app.timeFormat(f).substring(0,10);
				break;
			case '30':
				resultStart=year+'-'+month+'-01';
				resultEnd=year+'-'+month+'-'+day;
				break;
			case '90':
				resultEnd=year+'-'+month+'-'+day;
				if(month-2==0){
					year-=1;
					month=12;
				}else if(month-2==-1){
					year-=1;
					month=11;
				}else if(month-2<10){
					month='0'+(month-2);
				}else{
					month=month-2;
				}
				resultStart=year+'-'+month+'-01';
				break;
		}
		el.setAttribute('data-start',resultStart);
		el.setAttribute('data-end',resultEnd);
	}
	
	app.parseDomImg=function(str,base){
		var objE = document.createElement("div");
		objE.innerHTML = str;
		
		base=base||'';
		var imgs=objE.querySelectorAll('img');
		for(var i=0; i<imgs.length; i++){
			var img=imgs[i];
			img.setAttribute('data-delay',img.src);
			img.src=base+'images/blank.gif';
			img.setAttribute('height','100px');
			img.setAttribute('width','100%');
		}
		return objE.innerHTML;
	};
	
	app.update=function(){
		setTimeout(function(){
			mui.ajax(URL+'api/ShopBranchHome/GetUpdateApp',{
				data:app.md5Data({
					appVersion:plus.runtime.version,
					type:plus.os.name=='Android'?2:3
				}),
				dataType:'json',
				type:'get',
				timeout:10000,
				success:function(data){
					if(data.Success=='true'){
						plus.nativeUI.confirm('发现新版本'+AppName+'，马上体验吧！', function(event){
							if ( 0==event.index ) {
								plus.runtime.openURL(data.DownLoadUrl);
							}
						}, '', ["立即更新","取 消"] );
					}
				}
			});
		},5000)
	}

	app.createState = function(userId,userkey,type,callback) {
		var state = app.getState();
		state.userId = userId;
		state.userkey = userkey;
		state.type=type;
		app.setState(state);
		return callback();
	};

	app.getSiblings = function(elm){
		var a = [];
		var p = elm.parentNode.children;
		for(var i =0,pl= p.length;i<pl;i++) {
			if(p[i] !== elm) a.push(p[i]);
		}
		return a;
	}
}(mui, window.himall = {}));

himall.immersedCom();

var commonFn={
	search:function(){
		var search=document.querySelector('.header-search'),
			keywordsText=document.getElementById('keywordsText');
		document.getElementById('search').addEventListener('tap',function(){
			search.className+=' active';
		});
		
		document.querySelector('.search-cancel').addEventListener('tap',function(){
			himall.removeClass(search,' active');
		});
		
		keywordsText.addEventListener("keyup",function(event){
			if((event||window.event).keyCode==13){
				keywordsText.blur();
				searchVal=keywordsText.value;
				reloadPage();
			}
		});
	},
	filterData:function(){
		var filterBox=document.getElementById('filterBox'),
			orderFilter=document.getElementById('orderFilter');
		
		himall.removeClass(orderFilter,' hidden');
		document.getElementById('search').className+=' hidden';
		orderFilter.addEventListener('tap',function(){
			if(himall.hasClass(filterBox,'active')){
				himall.removeClass(filterBox,' active');
				himall.removeClass(this,' active');
			}else{
				filterBox.className+=' active';
				this.className+=' active';
			}
		})
		document.getElementById('filterBtn').addEventListener('tap',function(){
			himall.removeClass(filterBox,'active');
			himall.removeClass(orderFilter,' active');
			searchVal=searchText.value;
			mobileKey=storeSelect.value;
			reloadPage();
		});
		
		document.getElementById('filterReset').addEventListener('tap',function(){
			searchText.value='';
			storeSelect.options[0].selected = true;
		});
	},
	getStore:function(){
		setTimeout(function(){
			mui.ajax(URL+'api/ShopOrder/GetShopBranchs', {
	            data:himall.md5Data({
	                userkey: userkey
	            }),
	            dataType: 'json',
	            type: 'get',
	            timeout: 10000,
	            success: function(data) {
	            	var opt='<option value="">请选择门店</option><option value="0">总店</option>';
	            	for(var i=0; i<data.branchs.length; i++){
	            		opt+='<option value="'+data.branchs[i].Id+'">'+data.branchs[i].ShopBranchName+'</option>';
	            	}
	                storeSelect.innerHTML = opt;
	            }
	        });
		},1000)
	}
};


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


function showLogin(params,url){
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
    errorText.innerHTML = '<h4>网络不给力，请检查网络！</h4><button id="reloadWv" class="mui-btn mui-btn-blue">重新加载</button>';
    errorText.setAttribute('class','empty-show');
    document.body.appendChild(errorText);
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
	msg.thumbs=[params.productImg];
	msg.pictures=[params.productImg];
	
	share.send(msg, function() {
		plus.nativeUI.toast("分享到" + share.description + "成功！ ");
		if(params.callback){
			params.callback;
		}
	}, function(e) {
		plus.nativeUI.toast("分享到" + share.description + "取消");
	});
}