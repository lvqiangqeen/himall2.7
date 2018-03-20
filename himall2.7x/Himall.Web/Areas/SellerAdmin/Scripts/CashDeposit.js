// JavaScript source code
$("#shopDatagrid").hiMallDatagrid({
    url: './CashDepositDetail',
    nowrap: false,
    rownumbers: true,
    NoDataMsg: '没有找到符合条件的数据',
    border: false,
    fit: true,
    fitColumns: true,
    pagination: true,
    idField: "Id",
    pageSize: 15,
    pagePosition: 'bottom',
    pageNumber: 1,
    queryParams: { cashDepositId: $("#mid").val() },
    columns:
    [[
        { field: "Date", title: "时间", width: 120, align: "left" },
        { field: "Balance", title: "金额", width: 120, align: "left" },
        { field: "Operator", title: "操作人", width: 120, align: "left" },
        { field: "Description", title: "说明", width: 120, align: "left" },
    ]]
});



function ShowCashDeposit() {
    $("#cashDeposit").show();
}

function Pay() {
    if ($("#balance").val() == "") {
        $.dialog.tips("请输入要充值金额");
        return false;
    }
    if (parseFloat($("#balance").val()) && parseFloat($("#balance").val()) < 0) {
        $.dialog.errorTips("只能输入正数");
        return false;
    }
    if (parseFloat($("#balance").val()) < parseFloat($("#needPayCashDeposit").attr("needpay"))) {
        $.dialog.errorTips("充值金额不能小于应缴金额");
        return false;
    }
    var loading = showLoading();
    $.post("PaymentList", { balance: parseFloat($("#balance").val()) }, function (result) {
        loading.close()
        var html = '';
        $.each(result, function (index, item) {
            html += '<li>\
                        <label>\
                            <input type="radio" style="margin-top:15px;" class="jdradio" value="' + item.RequestUrl + '" name="requestUrl" id="' + item.Id + '" urlType="' + parseInt(item.UrlType) + '" />\
                            <img width="185" height="46" alt="微信PC" src="'+ item.Logo + '" />\
                        </label>\
                    </li>'
        });
        $("#payMent").find("ul").html('');
        $("#payMent").find("ul").append(html);

        $.dialog({
            title: '缴纳保证金',
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



    $('input[name="requestUrl"]').change(function () {
        var url = $(this).val();
        if ($(this).attr('urlType') == "1")
            url = '/pay/QRPay?url=' + url + '&id=' + $(this).attr('id') + '&orderIds=' + orderIds;
    });
}

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
