
$(function () { 
    var stae1, stae2, stae3;
    var pwdErrMsg = '密码不能为空！';
    $('#firstPwd').blur(function () {
        var d = $(this).val();
        if (d.length < 6) {
            $('#firstPwd').css({ borderColor: '#f60' });
            pwdErrMsg = '密码长度不能少于6位';
            stae2 = '';
        } else {
            $('#firstPwd').css({ borderColor: '#ccc' });
            stae2 = d;
            if ($('#secondPwd').val() != '' && $('#secondPwd').val() == $('#firstPwd').val()) {
                $('#secondPwd').css({ borderColor: '#ccc' });
                stae3 = d;
            }
            else {
                stae3 = '';
                pwdErrMsg = '两次密码不一致！';
            }
        }
    });
    $('#secondPwd').blur(function () {
        var d = $(this).val();
        if (d == $('#firstPwd').val()) {
            $('#secondPwd').css({ borderColor: '#ccc' });
            stae3 = d;
        } else {
            $('#secondPwd').css({ borderColor: '#f60' });
            pwdErrMsg = '两次密码不一致！';
            stae3 = '';
        }
    });
    $('#submitPwd').bind('click', function () {
        //console.log(stae1)
        if (!stae2) {
            $('#firstPwd').css({ borderColor: '#f60' });
            $.dialog.alert(pwdErrMsg);
        }
        if (!stae3) {
            $('#secondPwd').css({ borderColor: '#f60' });
            $.dialog.alert(pwdErrMsg);
        }
        if (stae2 && stae3) {
            var loading = showLoading();
            $.ajax({
                type: 'post',
                //url: 'SetPayPwd',
                url: "/" + areaName + "/Capital/SetPayPwd",
                data: { "pwd": stae3 },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips('设置成功！');
                        pwdflag = 'true';
                        $('#stepone').hide();
                        $('#steptwo').show();
                    }
                }
            });
        }
    });
    $('#btnWithDraw').click(function () {
        var userBalance = parseFloat($('#balanceValue').text());
        if (userBalance <= 0)
        {
            $.dialog.alert('可用金额为零，不能提现！');
            return;
        }
        if (areaName.toLowerCase() == 'm-weixin') {
            $("#J_assets_layer").addClass("cover");
            //$(".steponeee").height($(".steponeee").width() * 120 / 141);
            if (pwdflag.toLowerCase() == 'true') {
                $('#steptwo').show();
            }
            else {
                $('#stepone').show();
            }
        }
        else {
            $.dialog.alert('请在平台微信公众号内进行提现，或登录平台PC端进行提现');
        }
    });
    $(".steponeee .close").click(function(){
        $(this).parent().hide();
        $("#J_assets_layer").removeClass("cover");
    });
    $('#submitApply').click(function () {
        var reg = /^[0-9]+([.]{1}[0-9]{1,2})?$/;
        if (!reg.test($('#inputMoney').val())) {
            $.dialog.alert("提现金额不能为非数字字符");
            return;
        }
        var applyWithDrawAmount = parseFloat($('#inputMoney').val());
        var userBalance = parseFloat($('#balanceValue').text());
        var inputWithDrawMinimum = parseFloat($('#inputWithDrawMinimum').val()) || 0;
        var inputWithDrawMaximum = parseFloat($('#inputWithDrawMaximum').val()) || 0;
        
        if (parseFloat(applyWithDrawAmount) > userBalance) {
            $.dialog.alert("提现金额不能超出可用金额");
            return;
        }
        if (parseFloat(applyWithDrawAmount) < 1) {
            $.dialog.alert("提现金额不能小于1元");
            return;
        }
        if (!(parseFloat(applyWithDrawAmount) <= inputWithDrawMaximum && parseFloat(applyWithDrawAmount) >= inputWithDrawMinimum)) {
            $.dialog.alert("提现金额不能小于：" + inputWithDrawMinimum + " 元,不能大于：" + inputWithDrawMaximum+" 元");
            return;
        }
        var loading = showLoading();
        $.post("/" + areaName + "/Capital/ApplyWithDrawSubmit", { nickname: '', amount: parseFloat($('#inputMoney').val()), pwd: $('#payPwd').val() },
            function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.succeedTips('提现申请成功!', function () {
                        $('#steptwo').hide();
                        $('#stepone').hide();
                        window.location.reload();
                    });
                }
                else {
                    $.dialog.errorTips(result.msg);
                }
            }
        );
    });
	//充值
    $('#btnCharge').click(function () {
    	//if (areaName.toLowerCase() != 'm-weixin') {
    	//	$.dialog.alert('请在微信端进行充值');
    	//	return;
    	//}
        var ua = navigator.userAgent.toLowerCase();
        if (ua.match(/MicroMessenger/i) != "micromessenger") {
            $.dialog.alert('请在微信端进行充值');
            return;
        }
		$('#rechargePay').show();
		$("#J_assets_layer").addClass("cover");
		
		
    	/*$.dialog({
    		title: '请输入充值金额',
    		lock: true,
    		width: 430,
    		padding: '0 40px',
    		id: 'advertisement',
    		content: ['<div class="dialog-form">',
						'<div class="form-group clearfix">',
							'<input class="form-control input-sm" type="text" id="chargeAmount">',
						'</div>',
					'</div>'].join(''),
    		okVal: '确定',
    		ok: function () {
    			
    		}
    	});*/
    });
    
    $('#submitPayBtn').click(function(){
    	var amount = $('#chargeAmount').val();
		if (/^\d+(\.\d{1,2})?$/.test(amount)==false)
		{
			$.dialog.errorTips('请输入正确的金额');
			return false;
		}

		var loading = showLoading();
	    $.post('/' + areaName + '/Capital/Charge', { pluginId: 'Himall.Plugin.Payment.WeiXinPay', amount: $('#chargeAmount').val() },
			function (data) {
				loading.close();
				if (data.success == true) {
					$('#rechargePay').hide();
					$("#J_assets_layer").removeClass();
					//通过A链接模拟跳转
				    //jumpUrl在weixin中为一段js脚本，直接调用jsapi
				    if ($('#payJumpUrl').length > 0) {
				        $('#payJumpUrl').attr('href', data.href);
				    }
				    else {
				        $('body').append('<a id="payJumpUrl" style="display:none;" href="' + data.href + '"></a>');
				    }
				    document.getElementById("payJumpUrl").click();
				} else
					$.dialog.errorTips(data.msg);
			}).error(function (data) {
				loading.close();
				$.dialog.errorTips('操作失败,请稍候尝试.');
			});
    });

    var page = 1;

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        $('#autoLoad').show();
        if (scrollTop + windowHeight >= scrollHeight - 30) {

            loadRecords(++page);
        }
    });
});
var lodeEnd = false, total = 0;
function loadRecords(page) {
    if (lodeEnd)
        return;
    var url = "/" + areaName + '/Capital/List';
    $.post(url, { page: page, rows: 15 }, function (result) {
        var html = '';
        var str = '';
        if (result.model.length > 0) {
             total = result.Total;
            $.each(result.model, function (i, model) {
                if (parseFloat(model.Amount) > 0)
                {
                    str = '<td style="color:green">' + model.Amount + '</td>';
                }
                else {
                    str = '<td>' + model.Amount + '</td>';
                }
                html = [html
                    , '<tr>'
                    , '<td>' + model.CreateTime + '</td>'
                    , str
                    , '<td>' + model.Remark + '</td>'
                    , '</tr>'
                ].join('');
            });
            $('#ulList').append(html);
            if (total == result.model.length)
                lodeEnd = true;
        }
        else {
            $('#autoLoad').html('没有更多记录了');
            lodeEnd = true;
        }
    });

}