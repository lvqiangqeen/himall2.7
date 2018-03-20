// v 1.0.1
var v=({
	win:null,
	doc:null,
	utilsPointer:null,
	handlers:[],
	beTrigger:!1,
	options:[],
	items:[],
	ajaxCount:0,
	form: null,
	init:function(win,doc){
		var that=this,utils;
		that.win=win;
		that.doc=doc;
		that.utilsPointer=utils=that.utils(that);
		return function(opts){
			if (typeof opts==='undefined'){
				return this;
			}
			var form=utils.$(opts.form);
			if(!form){return;}
			that.form=form;
			that.items=[];
			that.options=[];
			utils.addEvent(form,'submit',function(e){
				var len=that.items.length,
						i=0,
						hasError=!1,
						flag;
				utils.preventDefault(e);
				if(that.ajaxCount>0){
					alert('由于您的网速问题，请等待异步验证返回结果！');
				}
				utils.validateAll.call(that, that.options, false);
				for(;i<len;i++){
					if(utils.hasClass(utils.$(that.items[i]),that.classObj['inputError'])){
						hasError=true;
						break;
					}
				}
                //错误前期判断
				if(flag===false){
					return false;
				}
				if (hasError)
				{
				    return false;
				}
				if(opts.beforeSubmit && utils.isFunction(opts.beforeSubmit)){
					flag=opts.beforeSubmit();
				}
				if (flag === false) {
				    return false;
				}
			    //提交前判断
				if (hasError) {
				    return false;
				}
				opts.ajaxSubmit ? utils.ajaxForm(form,opts.afterSubmit) : form.submit();
			});
			return that.method.call(that,form);
		};
	},
	method:function(elem){
		var doc=this.doc,
				that=this,
				utils=that.utilsPointer;
		return{
			v:'1.0.1',
			add:function(opts){
				if(!opts){
					return this;
				};
				var i=0,
						arr=Array.prototype.slice.call(arguments),
						options=that.options,
						len=arr.length;
				for(i=0;i<len;i++){
					that.items.push(arr[i].target);
					that.options.push(arr[i]);
					utils.bindHandlers.call(this,arr[i]);
				}
				return this;
			},
			remove:function(el){
				var i=0,n,
						len=that.options.length,
						element,handler,tip,opt;
				for(;i<len;i++){
					if(el===that.options[i].target){
						n=i;
						break;
					};
				};
				if(n==undefined){
					return this;
				}
				that.items.splice(n,1);
				opt=that.options.splice(n,1);
				handler=that.handlers.splice(n,1)[0];
				element=utils.$(el);
				if(opt.action){
					that.ajaxCount--;
				}
				tip=utils.$$(that.classObj['tip'],element.parentNode,'div')[0];
				utils.removeClass(element,that.classObj['inputError'] + ' ' + that.classObj['inputPass']);
				tip && tip.parentNode.removeChild(tip);
				utils.removeEvent(element,'focus',handler['focusFn']);
				utils.removeEvent(element,'blur',handler['blurFn']);
				utils.removeEvent(element,'change',handler['changeFn']);
				utils.removeEvent(element,'keyup',handler['keyupFn']);
				return this;
			},
			reset:function(){
				var i=0,
						len=that.items.length,item;
				for(;i<len;i++){
					item=utils.$((that.items)[i]);
					utils.removeClass(item, that.classObj['inputError'] + ' ' + that.classObj['inputPass']);
					item.value='';
				};
				utils.hideAllTip(that.form);
				that.ajaxCount=0;
				return this;
			},
			trigger:function(el,callback){
				var i=0,n,
						len=that.options.length,handler;
				for(;i<len;i++){
					if(el===that.options[i].target){
						n=i;
						break;
					}
				}
				if(n==undefined){
					return this;
				}
				that.beTrigger=true;
				utils.blurHandler(that.options[n])();
				that.beTrigger=false;
				callback && callback.call(this);
				return this;
			},
			config:function(options){
				var obj=that.config,i;
				if(typeof options==='object'){
					for(i in options){
						obj[i]=options[i];
					}
				}
				return this;
			}
		};
	},
	utils:function(obj){
		var that=obj,
				doc=that.doc,
				utils=that.utilsPointer,
				item=that.config;
		return{
			$:function(elem){
				return (typeof elem==='string')?doc.getElementById(elem):elem;
			},
			trim:function(str){
				return str.replace(/^\s+|\s+$/,'').replace(/\s+/,' ');
			},
			escaping:function(val){
				return val.replace(/^\s+|\s+$/g,'').replace(/(['"])/g,
				function(a,b){
					return '\\' + b;
				}).replace(/[\r\n]/g,'');
			},
			isFunction:function(obj){
				return (typeof obj=='function' ?true:false);
			},
			hasClass:function(elem, oClass){
			    oClass = ' ' + oClass + ' ';
				return(' ' + elem.className + ' ').indexOf(oClass) > -1 ?true:false;
			},
			isNumber:function(val){
				var reg=/^[-+]?(0|[1-9]\d*)(\.\d+)?$/;
				return reg.test(val);
			},
			check:function(reg,str){
				return reg.test(str);
			},
			extend:function(target,source){
				for(var i in source){
					target[i]=source[i];
				};
				return target;
			},
			confirms:function(opts){
				var utils=that.utilsPointer,
						elem=utils.$(opts.target),
						theSame=utils.$(opts.confirms),
						hasPass;
				if (!theSame) { return; }
				hasPass=utils.hasClass(theSame, that.classObj['inputPass']);
				if(hasPass){
					if(elem.value === theSame.value){
						utils.successTips(opts);
					}else{
						utils.errorTips(opts);
					}
				}
			},
			$$:function(oClass,parent,nodename){
				var i=0,
						len=0,
						re=[],
						elem;
				nodename=nodename || '*';
				parent=parent || that.doc;
				elem=parent.getElementsByTagName(nodename);
				for(len=elem.length;i<len;i++){
					if(this.hasClass(elem[i],oClass)){re.push(elem[i]);}
				};
				return re;
			},
			encodeNameAndValue:function(sName,sValue){
				return encodeURIComponent(sName) + '=' + encodeURIComponent(sValue);
			},
			serializeForm:function(form){
				var dom=that.utilsPointer.$(form),
						elemList=dom.elements,
						len=elemList.length,
						i=0,
						temp,
						re=[];
				for(;i<len;i++){
					temp=elemList[i];
					switch(temp.type){
						case 'select-one':
							case 'select-multipe':
								for(var j=0,l=temp.options.length;j<l;j++){
									var opt=temp.options[j];
									if(opt.selected){
										var v='';
										if(opt.hasAttribute){
											v=opt.hasAttribute('value')?opt.value:opt.text;
										}else{
											v=opt.attributes['value'].specified?opt.value:opt.text;
										}
										re.push(that.utilsPointer.encodeNameAndValue(temp.name,v));
									}
								}
						break;
						case undefined:
							case 'fieldset':
								case 'button':
									case 'submit':
										case 'reset':
											case 'file':
						break;
						case 'checkbox':
							case 'radio':
								if(!temp.checked){
									break;
								};
						default:re.push(that.utilsPointer.encodeNameAndValue(temp.name,temp.value));
						break;
					};
				};
				return re.join('&');
			},
			addEvent:function(elem,type,fn){
				if(typeof elem.addEventListener != 'undefined'){
					elem.addEventListener(type, fn, false);
				}else if(typeof elem.attachEvent != 'undefined'){
					elem.attachEvent('on' + type,fn);
				}else{
					elem['on' + type]=fn;
				};
			},
			removeEvent:function(elem,type,fn){
				if(typeof elem.removeEventListener!='undefined'){
					elem.removeEventListener(type,fn,false);
				}else if(typeof elem.detachEvent!='undefined'){
					elem.detachEvent('on' + type,fn);
				}else{
					elem['on' + type]=null;
				};
			},
			fireEvent:function(elem,type){
				if(typeof document.createEventObject=='object'){
					return elem.fireEvent('on' + type);
				}else{
					var e=document.createEvent('HTMLEvents');
					e.initEvent(type,true,true);
					return !elem.dispatchEvent(e);
				};
			},
			preventDefault:function(e){
				e=e || window.event;
				if(e.preventDefault){
					e.preventDefault();
				}else{
					e.returnValue=false;
				};
			},
			show:function(elem){
				elem && (elem.style.cssText='inline-block;*display:inline;*zoom:1;');
			},
			hide:function(elem){
				elem && (elem.style.display='none');
			},
			removeClass:function(elem,oClass){
				var C=this.trim(oClass).split(' '),
						eClass=elem.className,
						i=0,
						len=C.length;
				for(;i<len;i++){
					if(this.hasClass(elem,C[i])){
						eClass=eClass.replace(C[i],'');
					}
				}
				elem.className=this.trim(eClass);
			},
			addClass:function(elem,oClass){
				var C=this.trim(oClass).split(' '),
						eClass=elem.className,
						i=0,
						len=C.length;
				for(;i<len;i++){
					if(!this.hasClass(elem,C[i])){
						eClass += ' ' + C[i];
					}
				}
				elem.className=this.trim(eClass);
			},
			bindHandlers:function(opts){
				var utils=that.utilsPointer,
						elem=utils.$(opts.target),
						theSame;
				if(opts.confirms){
					theSame=utils.$(opts.confirms);
				}
				focusFn=utils.focusHandler.call(this,opts);
				blurFn=utils.blurHandler.call(this,opts);
				changeFn=utils.changeHandler.call(this,opts);
				keyupFn=utils.keyupHandler.call(this,opts);
				utils.addEvent(elem,'focus',focusFn);
				utils.addEvent(elem,'blur',blurFn);
				utils.addEvent(elem,'keyup',keyupFn);
				if(theSame){
					utils.addEvent(theSame,'blur',function(){
						utils.fireEvent(elem,'blur');
					});
				}
				if(elem.type=='select' || elem.type=='file'){
					utils.addEvent(elem,'change',changeFn);
				}
				that.handlers.push({
					'target':opts.target,
					'focusFn':focusFn,
					'blurFn':blurFn,
					'changeFn':changeFn,
					'keyupFn':keyupFn
				});
			},
			focusHandler:function(opts){
				var utils=that.utilsPointer;
				return function(){
					var elem=utils.$(opts.target),
							val=elem.value,
							defaultval=elem.getAttribute('placeholder') || '';
					if(opts.beforeFocus && utils.isFunction(opts.beforeFocus)){
						opts.beforeFocus(opts);
					}
					if((utils.hasClass(elem, that.classObj['inputError']) || utils.hasClass(elem, that.classObj['inputPass'])) && !(val === '' || val === defaultval)) return;
					if(opts.focusTips){
						utils.resetItem(opts);
					}else{
						utils.tips(opts);
					}
				};
			},
			blurHandler:function(opts){
				var _this=this,
						utils=that.utilsPointer;
				return function(){
					var elem=utils.$(opts.target),
							val=elem.value,
							defaultVal=elem.getAttribute('placeholder'),
							flag=true;
					if((!that.beTrigger && !opts.empty) && (val === '' || val === defaultVal)){
						utils.resetItem(opts);
						return;
					}
					if(opts.beforeBlur && utils.isFunction(opts.beforeBlur)){
						flag=opts.beforeBlur(opts);
					}
					if(flag===false){
						utils.errorTips(opts);
						return;
					}
					utils.validateItem.call(_this,opts);
					if(opts.afterBlur && utils.isFunction(opts.afterBlur)){
						opts.afterBlur.call(_this,opts);
					}
				};
			},
			changeHandler:function(opts){
				var utils=that.utilsPointer;
				return function(){
					utils.validate(opts)?utils.errorTips(opts):successTips(opts);
					if(opts.afterChange && utils.isFunction(opts.afterChange)){
						opts.afterChange.call(utils.$(opts.target),opts);
					};
				};
			},
			keyupHandler:function(opts){
				var utils=that.utilsPointer;
				return function(){
					if(opts.onkeypress && utils.isFunction(opts.onkeypress)){
						opts.onkeypress.call(utils.$(opts.target),opts);
					}
				};
			},
			createTip:function(){
				var div=document.createElement('div');
				div.className=that.classObj['tip'];
				return div;
			},
			tip:function(opts,type){
				var ruleType=opts.ruleType && (opts.ruleType.match(/\w+/g))[0],
						msg=opts[type] || (item[ruleType] && item[ruleType][type]) || '',
						elem=this.$(opts.target),
						tip=this.$$(that.classObj['tip'],elem.parentNode,'div')[0];
				if(!tip){
					tip=this.createTip();
					elem.parentNode.appendChild(tip,elem.nextSibling);
				}
				tip.innerHTML='<span>' + msg + '</span>';
				this.show(tip);
				switch(type){
					case 'tips':
						this.removeClass(elem, that.classObj['inputError'] + ' ' + that.classObj['inputPass']);
						this.removeClass(tip, that.classObj['error'] + ' ' + that.classObj['pass']);
					break;
					case 'error':
						this.removeClass(elem, that.classObj['inputPass']);
						this.addClass(elem, that.classObj['inputError']);
						this.removeClass(tip, that.classObj['pass']);
						this.addClass(tip, that.classObj['error']);
					break;
					case 'pass':
						this.removeClass(elem, that.classObj['inputError']);
						this.addClass(elem, that.classObj['inputPass']);
						this.removeClass(tip, that.classObj['error']);
						this.addClass(tip, that.classObj['pass']);
					break;
					case 'warning':
						this.removeClass(elem, that.classObj['inputPass']);
						this.addClass(elem, that.classObj['inputError']);
						this.removeClass(tip, that.classObj['pass']);
						this.addClass(tip, that.classObj['error']);
					break;
				}
				if(opts.noTips){this.hide(tip);}
			},
			tips:function(opts){
				this.tip(opts,'tips');
			},
			errorTips:function(opts){
				this.tip(opts,'error');
			},
			successTips:function(opts){
				var elem=this.$(opts.target);
				this.tip(opts,'pass');
				if(elem.value===''){
					this.resetItem(opts);
				};
			},
			warnTips:function(opts){
				this.tip(opts,'warning');
			},
			hideAllTip:function(form){
				var i=0,
				tips=this.$$(that.classObj['tip'],form,'div'),
				len=tips.length,
				item;
				for(;i<len;i++){
					item=tips[i];
					item.parentNode.removeChild(item);
				}
			},
			resetItem:function(opts){
				var item=this.$(opts.target),
						tip=this.$$(that.classObj['tip'],item.parentNode,'div')[0];
				this.removeClass(item,that.classObj['inputError']+' '+that.classObj['inputPass']);
				item.value='';
				this.hide(tip);
			},
			validate:function(opts){
				var elem=this.$(opts.target),
						reg='',
						fnRule=opts.fnRule,
						utils=that.utilsPointer,
						defaultValue=elem.getAttribute('placeholder');
				if(elem.value===defaultValue){
					elem.value='';
				};
				if(opts.ruleType){
					var type=opts.ruleType,
					rule_item=type.match(/\(?(\w+)[<>!=]*\w+\)?/g);
					for(var i=0,len=rule_item.length;i<len;i++){
						if(rule_item[i][0]=='('){
							if(elem.value){
							    if (utils.isNumber(elem.value)) {
							        var _tv = elem.value || elem.value >> 0;
							        type = type.replace('value', _tv);
								}else{
									type=type.replace('value',(elem.value.length>>0));
								}
							}else{
								type=type.replace('value',0);
							}
						}else{
							type=type.replace(rule_item[i],'utils.check(' + item[rule_item[i]].rules + ',\'' + utils.escaping(elem.value) + '\')');
						}
					};
					reg=type;
				}else if(opts.rule){
					reg='utils.check(' + opts.rule + ',\'' + utils.escaping(elem.value) + '\');';
				}else{
					return;
				};
				return !(fnRule ? ((new Function('utils,elem', 'return ' + reg)(utils, elem)) && fnRule.call(elem, opts) !== false) : (new Function('utils,elem', 'return ' + reg)(utils, elem)));
			},
			validateAll:function(options,bool){
				var utils=that.utilsPointer;
				for(var i=0,o;o=options[i++];){
					utils.validateItem.call(this,o,bool);
				}
			},
			validateItem:function(opts,bool){
				var utils=that.utilsPointer,
						el=utils.$(opts.target),
						_this=that,
						bool=bool || true,
						hasError=utils.validate(opts);
				if(hasError){
					utils.errorTips(opts);
				}else{
					if(opts.action && bool){
						_this.ajaxCount+=1;
						utils.ajaxValidate(opts,function(pass){
							_this.ajaxCount--;
							if(!pass){
								utils.warnTips(opts);
							}else{
								if(opts.success && typeof opts.success=='function'){
									if(!opts.success.call(utils,opts,pass)){
										return;
									}
								}
								utils.successTips(opts);
								if(opts.confirms){
									utils.confirms(opts);
								}
							}
						});
					}else{
						utils.successTips(opts);
						if(opts.confirms){
							utils.confirms(opts);
						}
					}
				}
			},
			ajaxValidate:function(opts,callback){
				var el=that.utilsPointer.$(opts.target),
						val=el.value,
						name=el.name || el.id;
				that.utilsPointer.ajax({
					type:"GET",
					url:opts.action,
					noCache:true,
					data:name + '=' + encodeURIComponent(val),
					onsuccess:function(){
						var data;
						if(!this.responseText){
							data='';
						}else{
							data=eval('(' + this.responseText + ')');
						}
						callback(data);
					}
				});
			},
			createXhr:function(){
				if(typeof XMLHttpRequest != 'undefined'){
					return new XMLHttpRequest();
				}else{
					var xhr=null;
					try{
						xhr=new ActiveXObject('MSXML2.XmlHttp.6.0');
						return xhr;
					}catch(e){
						try {
							xhr=new ActiveXObject('MSXML2.XmlHttp.3.0');
							return xhr;
						}catch(e){
							throw Error('cannot create XMLHttp object!');
						};
					}
				}
			},
			ajax:function(opts){
				var set=that.utilsPointer.extend({
					url:'',
					data:'',
					type:'POST',
					timeout:5000,
					onbeforerequest:function(){},
					onsuccess:function(){},
					onnotmodified:function(){},
					onfailure:function(){}
				},opts||{});
				var xhr=that.utilsPointer.createXhr();
				if((set.type).toUpperCase()=='GET'){
					if(set.data){
						set.url +=(set.url.indexOf('?') >= 0 ? '&': '?') + set.data;
						set.data=null;
					};
					if(set.noCache){
						set.url +=(set.url.indexOf('?') >= 0 ? '&': '?') + 't=' + ( + new Date());
					}
				}
				xhr.onreadystatechange=function(){
					if(xhr.readyState==4){
						if(xhr.status>=200 && xhr.status<300){
							set.onsuccess.call(xhr,new Function('return '+xhr.responseText)());
						}else if(xhr.status==304){
							set.onnotmodified.call(xhr,xhr.responseText);
						}else{
							set.onfailure.call(xhr,xhr.responseText);
						}
					}
				}
				xhr.open(set.type,set.url);
				if((set.type).toUpperCase()=='POST'){
					xhr.setRequestHeader('content-Type','application/x-www-form-urlencoded');
					xhr.setRequestHeader('X-Requested-With','XMLHttpRequest');
				}
				set.onbeforerequest();
				if(set.timeout){
					setTimeout(function(){
						xhr.onreadystatechange=function(){};
						xhr.abort();
						set.onfailure();
					},
					set.timeout);
				}
				xhr.send(set.data);
			},
			ajaxForm:function(form,onsuccess){
				that.utilsPointer.ajax({
					type: form.method,
					url: form.action,
					data: that.utilsPointer.serializeForm(form),
					onsuccess: onsuccess
				});
			}
		};
	},
	classObj:{
		'tip':'tip',
		'pass':'tip-pass',
		'error':'tip-error',
		'inputPass':'item-pass',
		'inputError':'item-error'
	},
	config:{
		'required':{
			'rules':/.+/,
			'tips':'该信息为必填项，请填写！',
			'error':'对不起，必填信息不能为空，请填写！'
		},
		'username':{
			'rules':/^[\u4E00-\u9FA5A-Za-z0-9_\ ]{4,20}$/i,
			'tips':"4~20个字符，由中文、英文字母和下划线组成。",
			'error':"对不起，用户名格式不正确。",
			'warning':"对不起，该用户名已经被注册。"
		},
		'password':{
			'rules':/^[a-zA-Z0-9\_\-\~\!\%\*\@\#\$\&\.\(\)\[\]\{\}\<\>\?\\\/\'\"]{6,20}$/,
			'tips':"6~20个字符，由英文字母，数字和特殊符号组成。",
			'error':"对不起，您填写的密码有误。"
		},
		'number':{
			'rules':/^[-+]?(0|[1-9]\d*)(\.\d+)?$/,
			'tips':'请输入数字！',
			'error':'对不起，您填写的不是数字。'
		},
		'mallnumber': {
		    'rules': /^[0-9]+([.]{1}[0-9]+){0,1}$/,
		    'tips': '请输入数字！',
		    'error': '对不起，您填写的不是数字。'
		},
		'date':{
			'rules':/^\d{4}(\-|\.)\d{2}(\-|\.)\d{2}$/,
			'tips':'请填写日期！格式为：2014-01-01或者2014.01.01',
			'error':'对不起，您填写的日期格式不正确.'
		},
		'money':{
			'rules':/^[-+]?(0|[1-9]\d*)(\.\d+)?$/,
			'tips':'请输入金额！',
			'error':'金额格式不正确。正确格式如：“60” 或 “60.5”。'
		},
		'per':{
			'rules':/^(?:[1-9][0-9]?|100)(?:\.[0-9]{1,2})?$/,
			'tips':'请输入百分比！',
			'error':'对不起，您填写的百分比格式不正确！'
		},
		'email':{
			'rules':/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/,
			'tips':'请输入您常用的E-mail邮箱号，以便我们联系您，为您提供更好的服务！',
			'error':'对不起，您填写的E-mail格式不正确！正确的格式：yourname@gmail.com。',
			'warning':'对不起，该E-mail帐号已经被注册。请更换一个。'
		},
		'phone':{
			'rules':/^(([0\+]\d{2,3}-)?(0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/,
			'tips':'请输入可以联系到您常用的电话号码！',
			'error':'对不起，您填写的电话号码格式不正确！'
		},
		'mobile':{
			'rules':/^[1-9]\d{10}$/,
			'tips':'请输入可以联系到您的手机号码！',
			'error':'对不起，您填写的手机号码格式不正确！'
		},
		'url':{
			'rules':/^(http|https):\/\/[A-Za-z0-9]+\.[A-Za-z0-9]+[\/=\?%\-&_~`@[\]\':+!]*([^<>\"])*$/,
			'tips':'请输入网站地址！',
			'error':'对不起，您填写的网站地址格式不正确！正确的网站地址如：http://www.abc.com/。'
		},
		'ip':{
			'rules':/^(0|[1-9]\d?|[0-1]\d{2}|2[0-4]\d|25[0-5]).(0|[1-9]\d?|[0-1]\d{2}|2[0-4]\d|25[0-5]).(0|[1-9]\d?|[0-1]\d{2}|2[0-4]\d|25[0-5]).(0|[1-9]\d?|[0-1]\d{2}|2[0-4]\d|25[0-5])$/,
			'tips':'请输入IP地址！',
			'error':'对不起，您填写的IP地址格式不正确！正确的IP地址如：192.168.1.1。'
		},
		'postal':{
			'rules':/^[1-9]\d{5}$/,
			'tips':'请输入邮政编码！',
			'error':'对不起，您填写的邮政编码格式不正确！正确的邮政编码如：410000。'
		},
		'qq':{
			'rules':/^[1-9]\d{4,}$/,
			'tips':'请输入您的QQ号！',
			'error':'对不起，您填写的QQ号格式不正确！正确的QQ号如：12345678。'
		},
		'english':{
			'rules':/^[A-Za-z]+$/,
			'tips':'请输入英文字母！',
			'error':'对不起，您填写的内容含有英文字母（A-Z,a-z）以外的字符！'
		},
		'chinese':{
			'rules':/^[\u0391-\uFFE5]+$/,
			'tips':'请输入中文字符！',
			'error':'对不起，您填写的内容含非中文字符！'
		},
		'ce':{
			'rules':/^[-\w\u0391-\uFFE5]+$/,
			'tips':'请输入中文或英文或数字字符！',
			'error':'对不起，您填写的内容不正确！'
		},
		'select':{
			'rules':/^[-\+]?[1-9]+$/,
			'tips':'请选择！',
			'error':'对不起，您选择的内容不正确！'
		},
		'integer':{
			'rules':/^[-\+]?\d+$/,
			'tips':'请输入整数！',
			'error':'对不起，您填写的内容不是整数！'
		},
		'uint':{
			'rules':/^\d+$/,
			'tips':'请输入整数！',
			'error':'对不起，您填写的内容不是整数！'
		},
		'idcard':{
			'rules':/(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3})|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])(\d{4}|\d{3}[x]))$/,
			'tips':'请输入身份证号码！',
			'error':'对不起，您填写的身份证号码格式不正确！'
		},
		'empty':{
			'rules':/^\s*$/
		},
		'anything':{
			'rules':/^[\s\S]*$/
		}
	}
}).init(window,document);