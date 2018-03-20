$(function () {
    $('#spec-list li').first().addClass('cur');
    $('#spec-list li').click(function () {
        $(this).addClass('cur').siblings().removeClass();
    });
    $("#buy-num").blur(function () {
        checkBuyNum();
    });

    //购买数量加减
    $('.changeNum .btn-reduce').click(function () {
        if (parseInt($('#buy-num').val()) > 1) {
            $('#buy-num').val(parseInt($('#buy-num').val()) - 1);
            checkBuyNum();
        }
    });
    $('.changeNum .btn-add').click(function () {
        $('#buy-num').val(parseInt($('#buy-num').val()) + 1);
    });

    //兑换
    $("#OrderNow").click(function () {
        var dis = $(this).hasClass('disabled');
        if (dis) return;
        if (parseInt($('#buy-num').val()) > parseInt($("#stockProduct").html())) {
            $.dialog.errorTips('不能大于库存数量');
            $('#buy-num').val(1);
            return false;
        }
        checkBuyNum();
        var num = $("#buy-num").val();
        checkLogin(function () {
            $.post('/Gift/CanBuy', { id: giftid, count: num }, function (result) {
                if (result.success == true) {
                    location.href = "/GiftOrder/SubmitOrder/" + giftid + "?count=" + num;
                } else {
                    $.dialog.errorTips(result.msg);
                }
            });
        });
    });
});

function checkBuyNum() {
    var num = 0;
    var result = true;
    try {
        num = parseInt($("#buy-num").val());
    } catch (ex) {
        num = 0;
    }
    if (num < 1) {
        $.dialog.errorTips('购买数量有误');
        $('#buy-num').val(1);
        result = false;
    }
    return result;
}