$(function() {
    initCountButton();
    if ($(".btn-goshop_in").hasClass("disabled")) {
        $(".btn-goshop_in").attr('disabled', "true");
    }
    
    $('#choose').himallSku({
    	data : {id : $('#mainId').val()},
		productId : $('#gid').val(),
		resultClass:{
			price:'#salePrice',
			stock:'#stockNum'
		},
		ajaxType : 'POST',
		ajaxUrl:'../GetSkus',
		skuPosition:'Details',
    });
});
function initCountButton() {
    $("#buy-num").blur(function() {
        var max = parseInt($("#maxSaleCount").val());
        if (parseInt($('#buy-num').val()) < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
        }
        if (parseInt($('#buy-num').val()) > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            $('#buy-num').val(max);
        }
    });
    $('#btn-add').click(function() {
        var max = parseInt($("#maxSaleCount").val());
        if (max < parseInt($('#buy-num').val()) ) {
            $('#buy-num').val(max);
            $.dialog.errorTips('每个ID限购 ' + max + '件');
        }
    });
}

function checkFirstSKUWhenHas() {
    if ($(".spec").length == 0)
        return;
    $(".spec").each(function () {
        $(this).children("div:first").not(".disabled").find("span:first").trigger("click");
    });
}

var skuId = new Array(3);
// chooseResult();
function chooseResult() {
    // 已选择显示
    var len = $('#choose .spec .selected').length;
    for (var i = 0; i < len; i++) {
        var index = parseInt($('#choose .spec .selected').eq(i).attr('st'));
        skuId[index] = $('#choose .spec .selected').eq(i).attr('cid');
    }
    // 请求Ajax获取价格
    if (len === $(".spec").length) {
        var gid = $("#gid").val();
        var sku = '';
        for (var i = 0; i < skuId.length; i++) {
            sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
        }
        if (sku.length === 0) { sku = "0_0_0_"; }
    }
}

$("#justBuy").click(function() {
    checkIsLogin(function(func) {
        justBuy(func);
    });
});

function justBuy(callBack) {
    chooseResult();
    var has = $("#has").val();
    var dis = $("#justBuy").hasClass('disabled');
    if (has != 1 || dis) return;
    if (dis) return false;
    var len = $('#choose .spec .selected').length;
    if (len === $(".spec").length) {
        var sku = getskuid();
        var num = $("#buy-num").val();
        $.post('../CheckLimitTimeBuy', { skuIds: sku, counts: num }, function(result) {
            if (result.success) {
                var url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=submit&skuIds={0}&counts={1}&isLimit=1'.format(sku, num);
                location.href = url;
                //location.href = "../../Order/Submit?skuIds=" + sku + "&counts=" + num+"&isLimit=1";
            } else if (result.remain <= 0) {
                $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，不能再买了哦");
            } else {
                $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，您最多还能买" + result.remain + "件");
            }
        });
    } else {
        $.dialog.errorTips("请选择商品规格！");
    }
}

function checkIsLogin(callBack) {
    var memberId = $("#logined").val();
    if (memberId == "1") {
        callBack();
    }
    else {
        location.href = "/" + areaName + "/Redirect?redirectUrl=" + location.href;
        // $.fn.login( {}, function () {
        //     callBack( function () { location.reload(); } );
        // }, '', '', '/Login/Login' );
    }
}

function getskuid() {
    var gid = $("#gid").val();
    var sku = '';
    for (var i = 0; i < skuId.length; i++) {
        sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
    }
    if (sku.length === 0) { sku = "0_0_0_"; }
    sku = gid + '_' + sku.substring(0, sku.length - 1);
    return sku;
}