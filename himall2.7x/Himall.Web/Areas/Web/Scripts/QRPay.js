
$(function () {
    checkPayDone();
});

function checkPayDone() {
    var orderIds = QueryString('orderIds');
    var type = QueryString('type') || '';
    if (type == 'charge') {
        $.getJSON('/PayState/CheckCharge', { orderIds: orderIds }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('支付成功!', function () {
                    location.href = "/userCenter?url=/usercapital/&tar=usercapital";
                });
            }
            else {
                setTimeout(checkPayDone, 0);
            }
        });
    }
    else {
        $.getJSON('/PayState/Check', { orderIds: orderIds }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('支付成功!', function () {
                    location.href = "/userCenter?url=/userorder?orderids="+orderIds+"&tar=userorder";
                });
            }
            else {
                setTimeout(checkPayDone, 0);
            }
        });
    }
}

