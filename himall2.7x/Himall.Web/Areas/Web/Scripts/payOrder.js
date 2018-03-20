$(function () {
    $('.progress-').hide();
    var orderIds = $('#orderIds').val();
    $('input[name="requestUrl"]').change(function () {
        var _t = $(this);
        var _nextbtn = $('#nextBtn');
        var url = _t.val();
        if (_t.attr('urlType') != '-1') {
            _nextbtn.removeAttr('href');
            _nextbtn.removeAttr('target');
            url = "/Pay/?pmtid=" + _t.attr("id") + "&ids=" + orderIds;
            _nextbtn.attr('urlType', _t.attr('urlType'));
            _nextbtn.attr('href', url);
            _nextbtn.attr('target', "_blank");
        }
        else {
            _nextbtn.attr('urlType', _t.attr('urlType'));
            _nextbtn.removeAttr('href');
        }
    });

    $('#nextBtn').click(function () {
        var t = $("input[name='requestUrl']:checked").val();
        if (t == undefined) {
            $.dialog.tips('请选择支付方式！');
            return;
        }

        if ($(this).attr('urlType') == "-1") {
            if (parseFloat($('#capitalAmount').text()) < parseFloat($('#totalAmount').text())) {
                $.dialog.alert('预付款金额少于订单金额');
                return false;
            }

            $.ajax({
                type: 'post',
                url: 'GetPayPwd',
                async: false,
                dataType: 'json',
                success: function (result) {
                    if (!result.success) {
                        window.top.open('/UserCapital/SetPayPwd', '_blank');
                    }
                }
            });
            $.dialog({
                title: '确认支付',
                lock: true,
                id: 'goodCheck',
                width: '280px',
                height: '100px',
                content: ['<div class="dialog-form">',
                    '<div class="form-group">',
                       '<div class="item">\
                                 <span class="label" style="position:relative;top:-15px;">支付密码：</span>\
                                    <div class="">\
                                    <input type="text" value="" id="payPwd" onkeyup="getVal(this)" name="userVo.realName" maxlength="20" class="itxt fl" style="width:0;height:30px;position:absolute;z-index:-999;">\
                                    <div id="pwdShow" onclick="focusInput()"></div>\
                            </div>\
                            </div>',
                    '</div>',
                '</div>'].join(''),
                padding: '10px',
                init: function () { $("#auditMsgBox").focus(); },
                button: [
                {
                    name: '付款',
                    callback: function () {
                        if ($("#payPwd").val().length == 0) {
                            $.dialog.alert("请输入支付密码");
                            return false;
                        }
                        $.post('PayByCapital', { orderIds: $('#orderIds').val(), pwd: $('#payPwd').val()}, function (result) {
                            if (result.success) {
                                $.dialog.succeedTips(result.msg, function () {
                                    location.href = "/userorder";
                                });
                            }
                            else {
                                $.dialog.alert(result.msg);
                                return false;
                            }
                        });
                    },
                    focus: true
                },
                {
                    name: '取消',
                    callback: function () { }
                }]
            });
        }
        else {
            if ($(this).attr('href') != 'javascript:;' || $(this).attr('urlType') == "2") {
                $.dialog({
                    title: '登录平台支付',
                    lock: true,
                    content: '<p>请您在新打开的支付平台页面进行支付，支付完成前请不要关闭该窗口</p>',
                    padding: '30px 20px',
                    button: [
                    {
                        name: '已完成支付',
                        callback: function () {
                            location.href = '/userorder';
                        }
                    },
                    {
                        name: '支付遇到问题',
                        callback: function () { }
                    }]
                });

                if ($(this).attr('urlType') == "2") {
                    var url = $(this).attr('formdata');
                    BuildPostForm('pay_form', url, '_blank').submit();
                }
            }
        }

    });
    var urltype = $("input[name='requestUrl']:checked").attr("urltype");//返回该页时如果有选中的支付方式则触发一次change事件
    $("input:radio[urltype='" + urltype + "']").change();
});
function getVal(obj){
	var txtLen = $(obj).val().length, html="";
	for(var i=0,len=txtLen;i<len;i++){
		html = html+"<span></span>";
	}
	$("#pwdShow").html(html);
}
function focusInput(){
	$("#payPwd").focus();
}
function BuildPostForm(fm, url, target) {
    var e = null, el = [];
    if (!fm || !url)
        return e;
    target = target || '_blank';
    e = document.getElementById(fm);
    if (!e) {
        e = document.createElement('Form');
        e.Id = fm;
        document.body.appendChild(e);
    }

    e.method = 'post';
    e.target = target;
    e.style.display = 'none';
    e.enctype = 'application/x-www-form-urlencoded';

    var idx = url.indexOf('?');
    var para = [], op = [];
    if (idx > 0) {
        para = url.substring(idx + 1, url.length).split('&');
        url = url.substr(0, idx);//截取URL
        var keypair = [];
        for (var p = 0 ; p < para.length; p++) {
            idx = para[p].indexOf('=');
            if (idx > 0) {
                el.push('<input type="hidden" name="' + para[p].substr(0, idx) + '" id="frm' + para[p].substr(0, idx) + '" value="' + para[p].substring(idx + 1, para[p].length) + '" />');
            }
        }
    }

    e.innerHTML = el.join('');
    e.action = url;
    return e;
};