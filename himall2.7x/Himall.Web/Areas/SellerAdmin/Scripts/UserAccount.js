
$("#RenewPay").on("click", function () {
    var reg = new RegExp('^[0-9]+([.]{1}[0-9]{1,2})?$');
    if ($("#balance").val() == "") {
        $.dialog.tips("请输入要充值金额");
        return false;
    }
    if (!reg.test($("#balance").val())) {
        $.dialog.errorTips("金额格式不对");
        return false;
    }
    if (parseFloat($("#balance").val()) <= 0) {
        $.dialog.errorTips("充值金额必需大于零");
        return false;
    }
    if (parseFloat($("#balance").val()) > 5000) {
        $.dialog.errorTips("充值金额最大不能超过5000");
        return false;
    }
    var loading = showLoading();
    $.post("ChargeSubmit", { amount: parseFloat($("#balance").val()) }, function (result) {
        loading.close()
        var html = '';
        $.each(result, function (index, item) {
            html += '<li style="margin-bottom:20px;margin-left:10px;">\
                        <label>\
                            <input style="position:relative;top:10px;" type="radio" class="jdradio" value="' + item.RequestUrl + '" name="requestUrl" id="' + item.Id + '" urlType="' + parseInt(item.UrlType) + '" />\
                            <img width="180" height="46" style="margin-left:15px;" alt="微信PC" src="'+ item.Logo + '" />\
                        </label>\
                    </li>'
        });
        $("#payMent").find("ul").html('');
        $("#payMent").find("ul").append(html);

        $.dialog({
            title: '余额充值',
            lock: true,
            width: 300,
            id: 'goodCheck',
            content: $("#payMent")[0],
            padding: '20px',
            button: [
            {
                name: '支付',
                callback: function () {
                    if ($('input[name="requestUrl"]:checked').length != 0) {
                        PayMessage();

                        if ($('input[name="requestUrl"]:checked').attr('urlType') == "2") {
                            var url = $('input[name="requestUrl"]:checked').val();
                            BuildPostForm('pay_form', url, '_blank').submit();
                        }
                        else
                            window.open($('input[name="requestUrl"]:checked').val());
                    }
                    else
                        $.dialog.tips("请选择支付方式");
                },
                focus: true
            }]
        });
    });
})


function PayMessage() {
    $.dialog({
        title: '登录平台支付',
        lock: true,
        content: '<p>请您在新打开的支付平台页面进行支付，支付完成前请不要关闭该窗口</p>',
        padding: '30px 20px',
        button: [
        {
            name: '已完成支付',
            callback: function () {
                location.href = location.href;
            }
        },
        {
            name: '支付遇到问题',
            callback: function () {

            }
        }]
    });
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
}
