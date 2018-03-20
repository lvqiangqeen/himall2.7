; (function ($) {
    $(function () {
        
        $('body').append('<div class="alpha hide" id="id_alpha"></div><div class="mimiBox hide" id="id_mimiBox"><div class="mimiWrap"><div class="mimiTitle"><span>您尚未登录</span></div><div class="mimiCon"><div class="mimiTitleBox"><span class="mimiCurr">登 录</span><a class="mimiLimk" href="/register">注 册</a></div><div class="mimiLog"><span class="mimiLogTitleMesg">用户名</span><div class="mimiInput"><input class="mimiText" type="text" value=""><i class="i-name"></i><label id="loginname_error" class="mimiError hide">您输入的账户名不存在，请核对后重新输入</label></div><span class="mimiLogTitleMesg">密码</span><div class="mimiInput"><input class="mimiText" type="password" value=""><i class="i-pass"></i><label id="loginname_error" class="mimiError hide">您输入的账户名不存在，请核对后重新输入</label></div><span class="mimiLogTitleMesg hide" id="verification_title">验证码</span><div class="mimiInput hide" id="verification"><input class="mimiText w60 fl" type="text" value="" id="verification_input"><label class="fl" style="line-height: 26px;"><img id="verification_img" class="verification_img" src="/Login/GetCheckCode" alt="" style="cursor:pointer;width:88px;height:33px;display:block;"></label><label class="ftx23 fr" style="line-height: 26px;">&nbsp;看不清？<a class="verification_img" href="javascript:;">换一张</a></label><div class="clear"></div><label id="verification_error" class="mimiError hide">验证码不正确或验证码已过期</label></div><div class="mimiAutoentry"><label style="font-size:12px;float:left;line-height:26px;cursor:pointer;"><input class="mimiCheckbox" type="checkbox" name="chkRememberMe" style="margin:3px 3px 3px;vertical-align:middle;">自动登录</label></div><div class="miniTrust"></div><div class="mimiBtn"><input id="loginsubmitframe" class="mimi_btn" type="button" tabindex="8" value="登 录"></div></div></div><div class="mimiClose" style="cursor:pointer;">×</div></div></div>');
        $.ajax({
            type: 'get',
            url: '/Login/GetOAuthList',
            dataType: 'json',
            cache: false,// 开启ajax缓存插件图片不处理
            async: false,
            success: function (data) {
                if (data.length > 0) {
                    var trustBtn = '';
                    for (var i = 0; i < data.length; i++) {
                        trustBtn += '<span class="btns">' +
                            '<a href="' + data[i].Url + '" title="' + data[i].Name + '">' +
                                '<img class="img1" src="' + data[i].LogoDefault + '">' +
                                '<img class="img2" src="' + data[i].LogoHover + '">' +
                            '</a>' +
                        '</span>';
                    }

                    $('.miniTrust').html('<p>使用合作网站账号登录商城：</p><div class="minitrustBtn">' + trustBtn + '</div>');
                }
            }
        });
    });
    $.fn.login = function (selectData, callBack, targetUrl, dataUrl, verifyUrl) {
        var that = $('.mimiText'),
			btnDom = $('#loginsubmitframe'),
			uid = 0,
            response=null,
			trigger = function (uid, selectData, callBack, dataUrl, targetUrl) {
			    if (!dataUrl || !targetUrl) {
			        callBack.call(null, targetUrl);
			    }
			    postAjax(selectData, callBack, dataUrl, targetUrl);
			},
			postAjax = function (data, fn, url, elem) {
                if(response){
                    response.abort();
                };
			    response=$.ajax({
			        type: 'post',
			        dataType: 'json',
			        url: url,
			        data: data,
			        success: function (d) {
			            fn.call(d, elem);
			        }
			    });
			},
            checkCode=function (data) {// 验证码验证
                var data=data||{errorTimes:2,minTimesWithoutCheckCode:1},
                    a = data.errorTimes,
                    b = data.minTimesWithoutCheckCode,
                    l,
                    ver;
                if (+a - b > 0) {
                    $('#verification,#verification_title').show();
                }
                $('#verification_input').keyup(function () {
                    ver = $(this).val();
                    l=ver.length;
                    if (l == 4) {
                        postAjax({ checkCode: ver }, function (elem) {
                            var data = this,
                                str;
                            if (data.success) {
                                uid = 1;
                                $('#verification_error').hide();
                            } else {
                                uid = 0;
                                str = $('#verification_input').val();
                                if(str){
                                    $('#verification_error').show().html('验证码错误!');
                                }else{
                                    $('#verification_error').show().html('验证码不能为空!');
                                }
                            }
                            $(elem).bind('focus', function () {
                                $(this).val('');
                            });
                        }, '/login/checkCode', this);
                        return false;
                    }
                });
                $('.verification_img').bind('click', function () {
                    $("#verification_img").attr('src', '/Login/GetCheckCode?' + (+new Date()));
                });
            };
        that.each(function (i, e) {
            $(e).bind('focus', function () {
                $(this).addClass('mimiBorder');
                $(this).removeClass('mimiBorder_error').siblings('.mimiError').hide();
            }).bind('blur', function () {
                $(this).removeClass('mimiBorder');
            });
        });
        btnDom.unbind('click').bind('click', function () {
            var arr = [];
            that.each(function (i, e) {
                var elem = $(e);
                arr.push(elem.val());
            });
            if($('.mimiCheckbox').attr('checked')){
                arr.push(true);
            }
            arr = 'username=' + arr[0] + '&password=' + arr[1] + '&checkCode=' + arr[2]+'&keep='+(arr[3]||false);// 组成需要post的数据
            if($('#verification').css('display')=='block'){
                checkCode();
                if (!uid) {
                    ver = $("#verification_input").val();
                    if (ver.length > 0) {
                        $('#verification_error').show().html('验证码验证失败!');
                    } else {
                        $('#verification_error').show().html('验证码不能为空!');
                    }
                    return;
                }else{
                    $('#verification_error').hide();
                }
            }
            postAjax(arr, function (elemList) {
                var data = this,
					list = elemList;
                if (data.success) {
                    uid = 1;
                } else {
                    uid = 0;
                    $(list[data.errorType]).addClass('mimiBorder_error').siblings('.mimiError').show(function () {
                        $(this).html(data.msg);
                    });

                    $("#verification_img").attr('src', '/Login/GetCheckCode?' + (+new Date()));
                }
                (($('#verification').css('display')=='block')||data.errorTimes) && (function (data) {// 开始进行验证码验证
                    checkCode(data);
                }(data));
                if (uid) {
                    $('#id_alpha,#id_mimiBox').hide();
                    trigger(uid, selectData, callBack, dataUrl, targetUrl);
                }
                return false;
            }, verifyUrl, that);
        });
        $('.mimiClose').bind('click', function () {
            $('#id_alpha,#id_mimiBox').hide();
        });
        $('#id_alpha,#id_mimiBox').show();
    };
}(jQuery));
/*
$.fn.login({用户勾选的商品数据},function(url){
	//跳转方法函数
	var data=this;
	if(data.state){
		window.location.href=url; 
	}
},'跳转的url','数据url','登陆验证url');
*/