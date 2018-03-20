$(function () {
	var namespace = ".assess";

	$.extend({
		isNull: function (obj) {
			return obj == null || obj == undefined;
		},
		//是null或空
		isNullOrEmpty: function (str) {
			if (typeof str === 'string')
				return str == '';
			return $.isNull(str);
		},
		notNullOrEmpty: function (str) {
			return $.isNullOrEmpty(str) == false;
		},
		uploadFile: function (options) {
			///	<summary>
			/// 文件上传
			///	</summary>
			///	<param name="options" type="Object">
			///		name:上传文件input的name
			///		url:上传文件的地址，默认/api/file/post
			///		accept:文件过滤，例如图片image/*，默认*
			///		data:上传数据
			///		headers:httpheader
			///		multiple:表示是否可以选多个文件，默认false
			///		form:form表单，不传则自动创建
			///		maxSize:文件的最大大小，以M为单位，默认2M
			///		change:上传文件input的change事件，返回false将不提交表单
			///		beforeSubmit:提交前执行方法，参数：准备上传的file元素。方法中返回false可以终止提交
			///		success:提交成功后回调方法，参数：服务端返回数据
			///		error:提交出错处理方法
			///	</param>
			///	<returns type="jQuery" />
			var defaults = {
				name: 'files',
				url: '/api/file/post',
				multiple: false,
			    //accept: '*',//全匹配模式时谷歌浏览器会变慢
				accept: 'image/jpg,image/jpeg,image/png',
				maxSize: 2
			};
			var o = $.extend(defaults, options);
			var uploader = {};
			var fileInput = $('<input type="file" name="{0}"/>'.format(o.name));
			if ($.notNullOrEmpty(o.accept))
				fileInput.attr('accept', o.accept)
			if ($.notNullOrEmpty(o.multiple))
				fileInput.attr("multiple", 'multiple');

			fileInput.bind("change" + namespace, function () {
				var form = fileInput.$form;
				if (form == null) {
					if ($.notNullOrEmpty(o.form))
						form = $(o.form);
					else {
						var div = $('<div style="display:none"><form action="{0}" method="post" enctype="multipart/form-data"><input name="upload" type="submit" value="submit"/></form></div>'.format(o.url));
						$('body').append(div);
						form = div.children();
					}
					form.prepend(fileInput);
					fileInput.$form = form;
				}
				var canContinued = true;
				if ($.isFunction(o.beforeSubmit))
					canContinued = o.beforeSubmit.call(o._this, fileInput[0]);
				else {
					if (this.files.length == 0)
						return false;
					for (var i = 0; i < this.files.length; i++) {
						var file = this.files[i];
						if (file.size > o.maxSize * 1024 * 1024) {
							alert('图片大小不能超过{0}M！'.format(o.maxSize), 'error');
							return false;
						}
					}
				}
				if (canContinued == false)
					return;

				if ($.isFunction(o.change) && o.change.call(fileInput[0], form) == false)
					return;

				var ajaxSubmitOption = {
					headers: o.headers,
					data: o.data,
					success: function (result) {
						if ($.isFunction(o.success))
							o.success.call(o._this, result);
						fileInput.val('');
					},
					error: function (e) {
						if ($.isFunction(o.error))
							o.error.call(o._this, e);
						fileInput.val('');
					}
				};

				form.ajaxSubmit(ajaxSubmitOption);
			});

			uploader.fileInput = fileInput;
			uploader.upload = function (caller) {
				if (caller)
					o._this = caller;
				else
					o._this = fileInput;
				fileInput.click();
			};
			return uploader;
		},
		uploadImage: function (options) {
			///	<summary>
			/// 文件上传
			///	</summary>
			///	<param name="options" type="Object">
			///		_this:回调函数的调用者,默认为上传文件的控件jquery对像
			///		url:上传文件的地址，默认/api/file/post
			///		data:上传数据
			///		headers:httpheader
			///		multiple:表示是否可以选多个文件，默认false
			///		form:form表单，不传则自动创建
			///		beforeSubmit:提交前执行方法，参数：准备上传的file元素。方法中返回false可以终止提交
			///		success:提交成功后回调方法，参数：服务端返回数据
			///		error:提交出错处理方法
			///	</param>
			///	<returns type="jQuery" />
		    var defaults = {
		        //accept: 'image/*'//全匹配模式时谷歌浏览器会变慢
			    accept: 'image/jpg,image/jpeg,image/png'
			};
			var o = $.extend(defaults, options);
			return $.uploadFile(o);
		}
	});

	$.fn.extend({
		//添加单选或多选下拉框
		appendDropdown: function (options) {
			var defaults = {
				id: null,
				defaultText: '',
				items: [],//items:[{value,text,selected}]
				isMultiple: false,//是否可以多选
				onChange: function () { }
			};

			var o = $.extend(defaults, options, { onChange: function () { } });
			//bootstrap下拉框
			var html =
		'<div class="dropdown">' +
			'<div class="close hidden" style="position: fixed;top: 0;left: 0;width: 100%;height: 100%;"></div>' +
			'<button class="btn btn-default dropdown-toggle" type="button" ' + ($.isNullOrEmpty(o.id) ? '' : 'id="' + o.id + '"') + ' data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">' +
				'<span style="display: inline-block; width: 80px; text-align: left; overflow: hidden;vertical-align:middle;">' + o.defaultText + '</span>' +
				'<span class="caret"></span>' +
			'</button>' +
			'<ul class="dropdown-menu" aria-labelledby="dropdownMenu1">';

			if ($.notNullOrEmpty(o.defaultText))
				html += '<li class="selected default"><a>' + o.defaultText + '</a></li>';

			for (var i = 0; i < o.items.length; i++) {
				var item = o.items[i];
				html += '<li index="{0}"><a data-id="{1}">{2}</a></li>'.format(i, item.value, item.text);
			}
			html +=
			'</ul>' +
		'</div>';

			var dropdown = $(html);
			var toggle = function () {
				if (dropdown.hasClass('open')) {
					dropdown.removeClass('open');
					dropdown.children('.close').addClass('hidden');
				} else {
					dropdown.addClass('open');
					dropdown.children('.close').removeClass('hidden');
				}
			};

			dropdown.children('button').bind('click' + namespace, function () {
				toggle();
			});
			dropdown.children('.close').bind('click' + namespace, function () {
				toggle();
			});

			function liSelected() {
				var $me = $(this);
				var parent = $me.parent();
				var span = $('button span:first', $me.closest('.dropdown'));

				if (o.isMultiple != true) {
					if ($me.hasClass('selected'))
						return;

					parent.find('li.selected').removeClass('selected');
				} else {
					var selectedCount = parent.find('li.selected').length;
					if ($me.hasClass('default')) {
						if (selectedCount > 0 || $me.hasClass('selected'))//多选时不能选择默认项
							return;
					} else if ($me.hasClass('selected') && selectedCount == 1) {
						$me.removeClass('selected');
						var defaultLi = parent.find('li.default').addClass('selected');
						span.text($('a', defaultLi).text());
						if ($.isFunction(o.onChange))
							o.onChange.call(dropdown.get(0), { value: '' });
						return;
					}
					else
						$('li.selected.default', parent).removeClass('selected');
				}

				$me.toggleClass('selected');


				if (o.isMultiple == true) {
					var selected = [];
					var selectedIds = [];
					$('li.selected', parent).each(function () {
						var selectedA = $('a', this);
						selected.push(selectedA.text());
						selectedIds.push(selectedA.data('id'));
					});

					span.text('[{0}]'.format(selected.join('][')));

					if ($.isFunction(o.onChange))
						o.onChange.call(dropdown.get(0), selectedIds.newitem(function (p) { return { value: p }; }));
				} else {
					span.text($me.hasClass('selected') ? $('a', $me).text() : '');

					if ($.isFunction(o.onChange))
						o.onChange.call(dropdown.get(0), { value: $('a', $me).data('id') });
				}
			}

			$('.dropdown-menu li', dropdown).bind('click' + namespace, function () {
				toggle();
				liSelected.call(this);
			});

			for (var i = 0; i < o.items.length; i++) {
				var item = o.items[i];
				if (item.selected == true) {
					var li = $('.dropdown-menu li[index={0}]'.format(i), dropdown);
					if (li.length > 0)
						liSelected.call(li.get(0));
				}
			}
			o.onChange = options.onChange;

			if ($('style#dropdownCss').length == 0) {
				$('head').append("<style id='dropdownCss'> .dropdown-menu li{ position: relative; cursor: pointer;} .dropdown-menu li.selected:before{ content: '√'; color: green; position: absolute; display: block; left: 5px; top: 3px;}</style>");
			}

			return this.append(dropdown);
		},
		dropdown: function (command) {
			var $me = $(this);
			//如果不是dropdown或dropdown的button
			if (!$me.hasClass('dropdown') && !$me.parent().hasClass('dropdown'))
				return;
			var dropdown = $me.hasClass('dropdown') ? $me : $me.parent();

			if (command == 'get value') {
				var values = [];
				$('li.selected:not(.default)', dropdown).each(function () {
					values.push($('a', this).data('id'));
				});

				return values;
			}
			//else if...
		}
	});

	String.prototype.format = function () {
		var ps = arguments;
		return this.replace(/\{\{/g, '{').replace(/\}\}/g, '}').replace(/\{[^\{\}]+\}/g, function (str) {
			var number, _default, format;
			var temp = str.replace('{', '').replace('}', '').split('?');
			number = temp[0];
			_default = temp[1];
			if (number.indexOf(':') > -1) {
				temp = number.split(':');
				number = temp[0];
				format = temp[1];
			} else if (temp.length > 1 && _default.indexOf(':') > -1) {
				temp = _default.split(':');
				_default = temp[0];
				format = temp[1];
			}
			var index = parseInt(number);
			if (isNaN(index))
				return '';
			var value = ps[index];
			if ($.isNullOrEmpty(value))
				value = _default || '';
			if ($.notNullOrEmpty(format) && (format == 'src' || format == "'src'" || format == '"src"'))
				value = 'src="' + value + '"';
			return value;
		});
	};

	String.prototype.formatProperty = function (obj) {
		var dataRegex = /\/Date\(-?\d+\)\//;
		return this.replace(/\{(\{)?[^}]+(\})?\}/g, function (str) {
			var split = str.replace(/[\{\}]/g, '').split('.');
			var value = obj;
			for (var i = 0; i < split.length; i++) {
				var property = split[i];
				var temp = property, format, defaultValue;
				var j = property.indexOf(':');
				if (j > 0) {
					temp = property.substr(0, j);
					format = property.substr(j + 1);
				}
				j = temp.indexOf('?');
				if (j > 0) {
					temp = property.substr(0, j);
					defaultValue = property.substr(j + 1);
				}
				value = value[temp];
				if ($.isNull(value)) {
					value = defaultValue || '';
					break;
				}
				if (typeof value === 'string' && dataRegex.test(value))
					value = value.formatDate(format || 'yyyy-MM-dd HH:mm:ss');
				else if (format == 'src' || format == "'src'" || format == '"src"')
					value = 'src=' + value;
			}
			return value;
		});
	};

	Array.prototype.contains = function (obj) {
		var flag = false;
		this.forEach(function (item, i) {
			if (item == obj) {
				flag = true;
				return flag;
			}
		});
		return flag;
	};
	Array.prototype.clear = function () {
		if (this.length > 0)
			this.splice(0, this.length);
	};
	Array.prototype.pushArray = function (array, distinct) {
		for (var i = 0; i < array.length; i++) {
			var item = array[i];
			if (distinct == true && this.contains(item))
				continue;
			else if (typeof distinct == 'function' && distinct(item, i) == true)
				continue;
			this.push(array[i]);
		}
	};
	Array.prototype.remove = function (item) {
		var self = this;
		if ($.isFunction(item)) {
			self.where(item).forEach(function (p) {
				var index = self.indexOf(p);
				while (index >= 0) {
					self.splice(index, 1);
					index = self.indexOf(p);
				}
			});
		}
		var index = self.indexOf(item);
		while (index >= 0) {
			self.splice(index, 1);
			index = self.indexOf(item);
		}
	};
	Array.prototype.removeAt = function (index) {
		this.splice(index, 1);
	};
	Array.prototype.removeArray = function (array) {
		for (var i = 0; i < array.length; i++) {
			this.remove(array[i]);
		}
	};
	Array.prototype.first = function (where) {
		if (!$.isFunction(where))
			return this[0];
		var result;
		this.forEach(function (item, i) {
			if (where(item, i)) {
				result = item;
				return;
			}
		});
		return result;
	};
	Array.prototype.last = function (where) {
		if (!$.isFunction(where))
			return this[this.length - 1];
		var result;
		for (var i = this.length - 1; i >= 0; i--) {
			result = this[i];
			if (where(result, i))
				break;
		}
		return result;
	};
	Array.prototype.clone = function () {
		var newArray = [];
		for (var i = 0; i < this.length; i++) {
			newArray.push(this[i]);
		}
		return newArray;
	};
	Array.prototype.all = function (fn) {
		var me = this;
		for (var i = 0; i < me.length; i++) {
			if (fn(me[i], i) == false)
				return false;
		}
		return true;
	};
	Array.prototype.any = function (fn) {
		var me = this;
		for (var i = 0; i < me.length; i++) {
			if (fn(me[i], i) == true)
				return true;
		}
		return false;
	};
	Array.prototype.newitem = function (fn) {
		var result = [];
		this.forEach(function (item, i) {
			var newitem = fn(item, i);
			if (newitem != undefined)
				result.push(newitem);
		});
		return result;
	};
	Array.prototype.sum = function (fn) {
		var number = 0;
		var _fn = $.isFunction(fn) ? fn : null;
		this.forEach(function (item, i) {
			if (_fn == null && !isNaN(item))
				number += item;
			else if (_fn != null)
				number += fn(item, i);
		});
		return number;
	};
	Array.prototype.where = function (fn) {
		var result = [];
		this.forEach(function (item, i) {
			if (fn(item, i) == true)
				result.push(item);
		});
		return result;
	};

	$.extend(Array.prototype, {
		indexOf: function (o) {
			for (var i = 0, len = this.length; i < len; i++) {
				if (this[i] == o) {
					return i;
				}
			}
			return -1;
		}, remove: function (o) {
			var index = this.indexOf(o);
			if (index != -1) {
				this.splice(index, 1);
			}
			return this;
		}, removeById: function (filed, id) {
			for (var i = 0, len = this.length; i < len; i++) {
				if (this[i][filed] == id) {
					this.splice(i, 1);
					return this;
				}
			}
			return this;
		}
	});

	//#region 重新设置页面的标题，因为页面文档结构去除了iframe，以前跳转到iframe显示的页面都没有设置标题
	if (window.localStorage) {
		$('a').click(function () {
			var text = $(this).text();
			saveToLocalStorage('CommonJs.Storage.Key.Title', text);
		});

		if (document.title.length == 0 || /^\w+$/.test(document.title)) {//判断是否没有设置页面标题或设置的页面标题全是字母,如果全是字母表示是以前iframe里面的页面
			var title = getFromLocalStorate('CommonJs.Storage.Key.Title');
			if (title) {
				if (typeof title == 'string') {
					document.title = title;
					saveToLocalStorage('CommonJs.Storage.Key.Title', { href: location.href, title: title });
					console.debug('页面的标题被设置成了[{0}]'.format(title));
				} else if (title.href == location.href) {
					document.title = title.title;
					console.debug('页面的标题被设置成了[{0}]'.format(title.title));
				}
			} else {
				$('.dropdown-menu a').each(function () {
					var a = $(this);
					var href = a.attr('href');
					var index = location.href.indexOf(href);
					if (index >= 0 && index + href.length == location.href.length) {//判断地址栏的地址是否以a的href结尾
						document.title = a.text();
						console.debug('页面的标题被设置成了[{0}]'.format(document.title));
					}
				});
			}
		} else
			window.localStorage.removeItem('CommonJs.Storage.Key.Title');
	}
	//#endregion


});

function ajaxRequest(option) {
	$.ajax({
		type: option.type,
		url: option.url,
		cache: false,
		data: option.param,
		dataType: option.dataType,
		success: option.success,
		error: option.error
	});
}

//检查上传的图片格式
function checkImgType(filename) {
	var pos = filename.lastIndexOf(".");
	var str = filename.substring(pos, filename.length)
	var str1 = str.toLowerCase();
	if (!/\.(gif|jpg|jpeg|png|bmp)$/.test(str1)) {
		return false;
	}
	return true;
}

//通用loading变量
var loadingobj;

function showLoading(msg, delay) {
	/// <param name="msg" type="String">待显示的文本,非必填</param>
	/// <param name="delay" type="Int">延时显示的毫秒数，默认100毫秒显示,非必填</param>
	if (!delay)
		delay = 100;
	var loading = $('<div class="ajax-loading" style="display:none"><table height="100%" width="100%"><tr><td align="center"><p>' + (msg ? msg : '') + '</p></td></tr></table></div>');
	loading.appendTo('body');
	var s = setTimeout(function () {
		if ($(".ajax-loading").length > 0) {
			loading.show();
			$('.container,.login-box').addClass('blur');
		}
	}, delay);
	return {
		close: function () {
			clearTimeout(s);
			loading.remove();
			$('.container,.login-box').removeClass('blur');
		}
	}

}

function QueryString(name) {
	/// 获取QueryString
	if (name == null || name == "" || name == undefined) {
		var AllVars = window.location.search.substring(1);
		return AllVars.split("&");
	}
	else {
		var AllVars = window.location.search.substring(1);
		var Vars = AllVars.split("&");
		for (i = 0; i < Vars.length; i++) {
			var Var = Vars[i].split("=");
			if (Var[0] == name) return Var[1];
		}
		return "";
	}
};

function AddFavorite(sURL, sTitle) {
	try {
		window.external.addFavorite(sURL, sTitle);
	}
	catch (e) {
		try {
			window.sidebar.addPanel(sTitle, sURL, "");
		}
		catch (e) {
			alert("加入收藏失败，请使用Ctrl+D进行添加");
		}
	}
}

//表示全局唯一标识符 (GUID)。
function Guid(g) {
	var arr = new Array(); //存放32位数值的数组
	if (typeof (g) == "string") { //如果构造函数的参数为字符串
		InitByString(arr, g);
	}
	else {
		InitByOther(arr);
	}

	//返回一个值，该值指示 Guid 的两个实例是否表示同一个值。
	this.Equals = function (o) {
		if (o && o.IsGuid) {
			return this.ToString() == o.ToString();
		}
		else {
			return false;
		}
	}
	//Guid对象的标记
	this.IsGuid = function () { }
	//返回 Guid 类的此实例值的 String 表示形式。
	this.ToString = function (format) {
		if (typeof (format) == "string") {
			if (format == "N" || format == "D" || format == "B" || format == "P") {
				return ToStringWithFormat(arr, format);
			}
			else {
				return ToStringWithFormat(arr, "D");
			}
		}
		else {
			return ToStringWithFormat(arr, "D");
		}
	}
	//由字符串加载
	function InitByString(arr, g) {
		g = g.replace(/\{|\(|\)|\}|-/g, "");
		g = g.toLowerCase();
		if (g.length != 32 || g.search(/[^0-9,a-f]/i) != -1) {
			InitByOther(arr);
		}
		else {
			for (var i = 0; i < g.length; i++) {
				arr.push(g[i]);
			}
		}
	}
	//由其他类型加载
	function InitByOther(arr) {
		var i = 32;
		while (i--) {
			arr.push("0");
		}
	}
	/*
    根据所提供的格式说明符，返回此 Guid 实例值的 String 表示形式。
    N  32 位： xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    D  由连字符分隔的 32 位数字 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    B  括在大括号中、由连字符分隔的 32 位数字：{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
    P  括在圆括号中、由连字符分隔的 32 位数字：(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
    */
	function ToStringWithFormat(arr, format) {
		switch (format) {
			case "N":
				return arr.toString().replace(/,/g, "");
			case "D":
				var str = arr.slice(0, 8) + "-" + arr.slice(8, 12) + "-" + arr.slice(12, 16) + "-" + arr.slice(16, 20) + "-" + arr.slice(20, 32);
				str = str.replace(/,/g, "");
				return str;
			case "B":
				var str = ToStringWithFormat(arr, "D");
				str = "{" + str + "}";
				return str;
			case "P":
				var str = ToStringWithFormat(arr, "D");
				str = "(" + str + ")";
				return str;
			default:
				return new Guid();
		}
	}
}
//Guid 类的默认实例，其值保证均为零。
Guid.Empty = new Guid();
//初始化 Guid 类的一个新实例。
Guid.NewGuid = function () {
	var g = "";
	var i = 32;
	while (i--) {
		g += Math.floor(Math.random() * 16.0).toString(16);
	}
	return new Guid(g);
}

//获取区域路径
//eg: /admin/home/index 页面调用此方法后返回 /admin/
function getAreaPath() {
	var path = location.pathname + '/';
	path = path.substring(1, path.length);
	path = path.substring(0, path.indexOf('/'));
	return '/' + path + '/';
}
//转换json传输date
function date_string(str, df) {
	df = df || "yyyy-MM-dd";
	return time_string(str, df);
}

//时间转换前位加零
function dFormat(i) { return i < 10 ? "0" + i.toString() : i; }

//转换json传输date
function time_string(str, df) {
	df = df || "yyyy-MM-dd HH:mm:ss";
	var result = "";
	if (str == null || str.length < 1) {
		return result;
	}
	var d = eval('new ' + str.substr(1, str.length - 2));
	var ar_date = [d.getFullYear(), d.getMonth() + 1, d.getDate()];
	result = formatdata(d, df);
	return result;
}

function formatdata(data, fmt) {
	var o = {
		"M+": data.getMonth() + 1, //月份         
		"d+": data.getDate(), //日         
		"h+": data.getHours() % 12 == 0 ? 12 : data.getHours() % 12, //小时         
		"H+": data.getHours(), //小时         
		"m+": data.getMinutes(), //分         
		"s+": data.getSeconds(), //秒         
		"q+": Math.floor((data.getMonth() + 3) / 3), //季度         
		"S": data.getMilliseconds() //毫秒         
	};
	var week = {
		"0": "/u65e5",
		"1": "/u4e00",
		"2": "/u4e8c",
		"3": "/u4e09",
		"4": "/u56db",
		"5": "/u4e94",
		"6": "/u516d"
	};
	if (/(y+)/.test(fmt)) {
		fmt = fmt.replace(RegExp.$1, (data.getFullYear() + "").substr(4 - RegExp.$1.length));
	}
	if (/(E+)/.test(fmt)) {
		fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "/u661f/u671f" : "/u5468") : "") + week[data.getDay() + ""]);
	}
	for (var k in o) {
		if (new RegExp("(" + k + ")").test(fmt)) {
			fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
		}
	}
	return fmt;
}

String.prototype.trim = function () {
	return this.replace(/(^\s*)|(\s*$)/g, "");
}

// 倒计时
function countDown(intDiff, callback) {
	var intDiff = parseInt(intDiff); // 倒计时总秒数量
	window.setInterval(function () {
		var day = 0,
            hour = 0,
            minute = 0,
            second = 0; // 时间默认值
		if (intDiff > 0) {
			day = Math.floor(intDiff / (60 * 60 * 24));
			hour = Math.floor(intDiff / (60 * 60)) - (day * 24);
			minute = Math.floor(intDiff / 60) - (day * 24 * 60) - (hour * 60);
			second = Math.floor(intDiff) - (day * 24 * 60 * 60) - (hour * 60 * 60) - (minute * 60);
		}
		if (minute <= 9) minute = '0' + minute;
		if (second <= 9) second = '0' + second;
		callback(day, hour, minute, second);
		intDiff--;
	}, 1000);
}

function formatMoney(s) {
	if (/[^0-9\.]/.test(s))
		return "0";
	if (s == null || s == "")
		return "0";
	s = s.toString().replace(/^(\d*)$/, "$1.");
	s = (s + "00").replace(/(\d*\.\d\d)\d*/, "$1");
	s = s.replace(".", ",");
	var re = /(\d)(\d{3},)/;
	while (re.test(s))
		s = s.replace(re, "$1,$2");
	s = s.replace(/,(\d\d)$/, ".$1");
	return s;
}

window.saveToLocalStorage = function (key, value) {
	window.localStorage.setItem(key, JSON.stringify(value));
};

window.getFromLocalStorate = function (key) {
	var value = window.localStorage.getItem(key);
	if (value != null)
		return JSON.parse(value);
	return value;
};

//表示当前脚本名称的对象，在使用的时候就知道是哪个脚本里面的
window.commonJS = (function () {
	var initMeiQia = false;

	var _commonJs = function () {
		//#region 属性
		this._userIdentity = null;

		//是否是微信浏览器
		this.isWeiXinBrowser = (function () {
			var ua = window.navigator.userAgent.toLowerCase();
			if (ua.match(/MicroMessenger/i) == 'micromessenger') {
				return true;
			} else {
				return false;
			}
		})();
		//#endregion

		//#region 方法

		//调用美洽客服
		//entId:企业id
		//token:客服组的 Token
		this.callMeiQiaCS = function (entId, token, metadata) {
			if (initMeiQia != true) {
				if (typeof (token) == 'object') {
					metadata = token;
					token = null;
				}
				(function (m, ei, q, i, a, j, s) {
					m[i] = m[i] || function () {
						(m[i].a = m[i].a || []).push(arguments)
					};
					j = ei.createElement(q);
					s = ei.getElementsByTagName(q)[0];
					j.async = true;
					j.charset = 'UTF-8';
					j.src = '//static.meiqia.com/dist/meiqia.js';
					s.parentNode.insertBefore(j, s);
				})(window, document, 'script', '_MEIQIA');
				_MEIQIA('entId', entId);
				initMeiQia = true;
				//阻止自动初始化
				//_MEIQIA('manualInit');
				//手动初始化
				//_MEIQIA('init')

				if (metadata) {
					_MEIQIA('metadata', metadata);
				}

				if (token)
					_MEIQIA('showPanel', { groupToken: token });
				else
					_MEIQIA('showPanel');

				//获取聊天窗口可见性
				_MEIQIA('getPanelVisibility', function (visibility) {
					if (visibility === 'visible') {
						//console.log('聊天窗口处于显示状态');
					} else {
						//console.log('聊天窗口处于隐藏状态');
						$('#MEIQIA-BTN-HOLDER').hide().addClass('hide');
					}
				});
			} else
				_MEIQIA('showPanel');
		}

		//生成分页结构
		this.generatePaging = function (index, count, click) {
			var paging = {};
			var div = $('<div class="pagin fr"></div>');
			paging.div = div;
			paging.PageIndex = parseInt(index);
			paging.PageCount = parseInt(count);
			paging.update = function (pi, pc) {
				var _paging = this;
				if ($.isNullOrEmpty(pi))
					pi = _paging.PageIndex;
				else
					_paging.PageIndex = pi;

				if ($.isNullOrEmpty(pc))
					pc = _paging.PageCount;
				else
					_paging.PageCount = pc;

				if (pc <= 0) {
					_paging.div.addClass('hide');
					return;
				} else
					_paging.div.removeClass('hide');

				var a_click = function () {
					var a = $(this);
					var parent = a.parent();
					if (a.is('.disabled') || a.is('.current'))
						return;

					var pi = $('a.current', parent);
					pi = parseInt(pi.html());

					if (a.is('.prev'))
						_paging.PageIndex = pi - 1;
					else if (a.is('.next'))
						_paging.PageIndex = pi + 1;
					else
						_paging.PageIndex = parseInt(a.html());

					_paging.update();

					if ($.isFunction(click)) {
						try {
							click.call(a, _paging.PageIndex);
						}
						catch (e) {
							console.error(e.message, e);
						}
					}
				};

				_paging.div.html('');

				var a = $('<a class="prev">上一页</a>');
				if (pi <= 1)
					a.addClass('hide disabled');
				else
					a.click(a_click);
				_paging.div.append(a);

				if (pi > 4) {
					a = $('<a>1</a>');
					a.click(a_click);
					_paging.div.append(a);
				}

				if (pi > 5)
					_paging.div.append('<span class="disabled">...</span>');

				for (var i = pi - 3; i <= pi + 3; i++) {
					if (i < 1 || i > pc)
						continue;
					a = $('<a>' + i + '</a>');
					if (i == pi)
						a.addClass('current');
					else
						a.click(a_click);
					_paging.div.append(a);
				}

				if (pi < pc - 4)
					_paging.div.append('<span class="disabled">...</span>');

				if (pi < pc - 3) {
					a = $('<a>' + pc + '</a>');
					a.click(a_click);
					_paging.div.append(a);
				}

				a = $('<a class="next">下一页</a>');
				if (pi >= pc)
					a.addClass('hide disabled');
				else
					a.click(a_click);

				_paging.div.append(a);
			};
			paging.update();

			return paging;
		}

		//#endregion
	};

	_commonJs.prototype = {

		//设置userIdentity的get方法
		get userIdentity() {
			var _this = this;
			if (_this._userIdentity == null) {
				$.ajax({
					url: '/userinfo/userIdentity',
					async: false,
					cache: false,
					success: function (result) {
						if (isNaN(result) == false)
							_this._userIdentity = result;
					}
				});
			}

			return _this._userIdentity;
		}
	};

	return new _commonJs();
})();

