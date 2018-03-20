
var gid = 0;
var skuId = new Array(3);

//chooseResult();
function chooseResult() {
    //已选择显示
    var str = '<em>已选择&nbsp;</em>';
    var len = $('#choose li .dd .selected').length;
    for (var i = 0; i < len; i++) {
        if (i < len - 1) {
            if ($('#choose li .dd .selected a').eq(i).text() != null)
                str += '<strong>“' + $('#choose li .dd .selected a').eq(i).text() + '”</strong>，';
        }
        else {
            if ($('#choose li .dd .selected a').eq(i).text() != null)
                str += '<strong>“' + $('#choose li .dd .selected a').eq(i).text() + '”</strong>';
        }
        var index = parseInt($('#choose li .dd .selected').eq(i).attr('st'));
        skuId[index] = $('#choose li .dd .selected').eq(i).attr('cid');
    }
    //console.log(skuId);
    //$( '#choose-result .dd' ).html( str )

    //请求Ajax获取价格
    if (len === $(".choose-sku").length) {
        var sku = '';
        for (var i = 0; i < skuId.length; i++) {
            sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
        }
        if (sku.length === 0) { sku = "0_0_0_"; }
    }
}

//转换0
function parseFloatOrZero(n) {
    result = 0;
    try {
        if (n.length < 1) n = 0;
        if (isNaN(n)) n = 0;
        result = parseFloat(n);
    } catch (ex) {
        result = 0;
    }
    return result;
}



function LoadActives()
{
    $.post("/product/GetProductActives", { shopid: $('#shopid').val(), productId: $("#gid").val() }, function (data) {
        if (parseFloat(data.freeFreight) > 0) {
            $("#summary-promotion").append("<div class='dt l l01'>促销</div>" +
                    "<div class='promotion-l' style='float:left;width:440px;'>" +
                            "<div style='margin-bottom:5px;'><em class='hl_red_bg'>满免</em><em class='hl_red'>单笔订单满<span>" + data.freeFreight + "</span>元免运费</em></div>" +
                    "</div>");
        }
        if (data.ProductBonus != undefined && data.ProductBonus != null && typeof data.ProductBonus.GrantPrice != "undefined") {
            var html = '<div class="dd d02" style="padding:0;margin:0;"><em class="hl_red_bg">红包</em><em class="hl_red">满<span>' + data.ProductBonus.GrantPrice + '</span>元送红包（' + data.ProductBonus.Count + '个' + data.ProductBonus.RandomAmountStart + '—' + data.ProductBonus.RandomAmountEnd + '元代金券红包）</em></div>';
            if ($(".promotion-l").length > 0) {
                $(html).appendTo(".promotion-l");
            }
            else {
                $('#summary-promotion').append('<div class="dt l l01">促销</div>' + html);
            }
        }
        if (data.FullDiscount != undefined && data.FullDiscount != null) {

            var html = '<div class="dd d02" style="padding:0;margin:0;"><em class="hl_red_bg">满减</em>'
            $(data.FullDiscount.Rules).each(function (index, item) {
                html += '<em>满<span>' + item.Quota + '</span>元减' + item.Discount + (index == data.FullDiscount.Rules.length - 1 ? '' : '，') + '</em>';
            });
            html += '</div>';
            if ($(".promotion-l").length > 0) {
                $(html).appendTo(".promotion-l");
            }
            else {
                $('#summary-promotion').append('<div class="dt l l01">促销</div>' + html);
            }
        }
    });
}




$(function () {
    var marketPrice = $('#marketPrice').text();
    $('#choose').himallSku({
        data: { id: $('#mainId').val() },
        productId: $('#gid').val(),
        spec: '.choose-sku',
        itemClass: '.item',
        resultClass: {
            price: '#jd-price',
            stock: '#stockNum',
            chose: '#choose-result .dd'
        },
        ajaxType: 'POST',
        ajaxUrl: '/LimitTimeBuy/GetSkus',
        skuPosition: 'Details',
        callBack: function (select) {
            $("#rebate em").html((select.Price / marketPrice * 10).toFixed(2));
        }
    });

    LoadActives();

    //倒计时
    var intDiff = parseInt($('#intDiff').val());//倒计时总秒数量
    function timer(intDiff) {
        window.setInterval(function () {
            var day = 0,
                hour = 0,
                minute = 0,
                second = 0;//时间默认值        
            if (intDiff > 0) {
                day = Math.floor(intDiff / (60 * 60 * 24));
                hour = Math.floor(intDiff / (60 * 60)) - (day * 24);
                minute = Math.floor(intDiff / 60) - (day * 24 * 60) - (hour * 60);
                second = Math.floor(intDiff) - (day * 24 * 60 * 60) - (hour * 60 * 60) - (minute * 60);
            }
            if (minute <= 9) minute = '0' + minute;
            if (second <= 9) second = '0' + second;

            if ($("#isStart").val() == "1") {
                $('.countime').html('<div class="dt" style="line-height: 32px;">还剩：</div><span class="hour">' + day + '</span><em>天</em> <span class="hour">' + hour + '</span><em>时</em> <span class="hour">' + minute + '</span><em>分</em> <span class="hour">' + second + '</span><em>秒</em>');
            }
            else {
                $('.countime').html('<div class="dt" style="line-height: 32px;"></div><span class="hour">' + day + '</span><em>天</em> <span class="hour">' + hour + '</span><em>时</em> <span class="hour">' + minute + '</span><em>分</em> <span class="hour">' + second + '</span><em>秒</em>后开始');
            }

            intDiff--;
        }, 1000);
    }
    timer(intDiff);


    if ($(".btn-goshop_in").hasClass("disabled")) {
        $(".btn-goshop_in").attr('disabled', "true");
    }

    //购买数量加减
    $("#buy-num").blur(function () {
        var max = parseInt($("#maxSaleCount").val());
        var stockNum = parseInt($("#stockNum").html());
        var buynum = parseInt($('#buy-num').val())
        if (buynum < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
        }
        if (buynum > stockNum) {
            $.dialog.errorTips('库存仅余 ' + stockNum + '件');
        }
        if (buynum > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            // $('#buy-num').val(max);
        }
    });

    $('.wrap-input .btn-reduce').click(function () {
        if (parseInt($('#buy-num').val()) > 1) {
            $('#buy-num').val(parseInt($('#buy-num').val()) - 1);
        }
    });

    $('.wrap-input .btn-add').click(function () {
        var max = parseInt($("#maxSaleCount").val());
        var stockNum = parseInt($("#stockNum").html());
        var buynum = parseInt($('#buy-num').val())
        if (max < buynum + 1) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
        } else {
            if (buynum +1> stockNum) {
                $.dialog.errorTips('库存仅余 ' + stockNum + '件');
            } else {
                $('#buy-num').val(buynum + 1);
            }
        }
    });

    $("#easyBuyBtn").click(function () {
        var has = $("#has").val();
        var dis = $(this).parent("#choose-btn-append").hasClass('disabled');
        if (has != 1 || dis) return;
        var len = $('#choose li .dd .selected').length;
        if (len === $(".choose-sku").length) {
            var sku = getskuid();
            var num = $("#buy-num").val();
            location.href = "/Order/EasyBuyToOrder?skuId=" + sku + "&count=" + num;
            //   alert('SKUId：'+sku+'，购买数量：'+num);
        } else {
            $.dialog.errorTips('请选择商品规格');

        }
    });

    function checkLogin(callBack) {
        var memberId = $.cookie('Himall-User');
        if (memberId) {
            callBack();
        }
        else {
            $.fn.login({}, function () {
                callBack(function () { location.reload(); });
            }, './', '', '/Login/Login');
        }
    }



    //导航切换
    $('.tab .comment-li').click(function () {
        $('#product-detail .mc').hide();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#comment').offset().top - 52);
    });
    $('.tab .consult-li').click(function () {
        $('#product-detail .mc').hide();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#consult').offset().top - 52);
    });
    $('.tab .goods-li').click(function () {
        $('#product-detail .mc').show();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#product-detail').offset().top);
    });

    //导航浮动
    $(window).scroll(function () {
        if ($(document).scrollTop() >= $('#product-detail').offset().top)
            $('#product-detail .mt').addClass('nav-fixed');
        else
            $('#product-detail .mt').removeClass('nav-fixed');
    });


    $("#shopInSearch").click(function () {
        var start = isNaN(parseFloat($("#sp-price").val())) ? 0 : parseFloat($("#sp-price").val());
        var end = isNaN(parseFloat($("#sp-price1").val())) ? 0 : parseFloat($("#sp-price1").val());
        var shopid = $("#shopid").val();

        var keyword = $("#sp-keyword").val();
        if (keyword.length === 0 && start == end) {
            $.dialog.errorTips('请输入关键字或者价格区间');
            return;
        }
        location.href = "/Shop/searchAd?pageNo=1&sid=" + shopid + "&keywords=" + keyword + "&startPrice=" + start + "&endPrice=" + end;
    });

    //立即购买
    $("#justBuy").click(function () {
        var flag = true;
        var max = parseInt($("#maxSaleCount").val());
        if (parseInt($('#buy-num').val()) < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
            flag = false;
        }
        if (parseInt($('#buy-num').val()) > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            $('#buy-num').val(max);
            flag = false;
        }
        if (flag) {
            checkLogin(function (func) {
                justBuy(func);
            });
        }
    });



    function justBuy(callBack) {
        var loading = showLoading();
        chooseResult();
        var has = $("#has").val();
        var dis = $("#justBuy").hasClass('disabled');
        if (has != 1 || dis) {
            loading.close();
            return
        };
        if (dis) {
            loading.close();
            return false;
        }
        var len = $('#choose li .dd .selected').length;
        if (len === $(".choose-sku").length) {
            var sku = getskuid();
            var num = $("#buy-num").val();
            $.post('/LimitTimeBuy/CheckLimitTimeBuy', { skuIds: sku, counts: num }, function (result) {
                if (result.success) {
                    location.href = "/Order/SubmitByProductId?skuIds=" + sku + "&counts=" + num;
                }
                else if (result.maxSaleCount <= 0 && result.remain <= 0) {
                    loading.close();
                    $.dialog.errorTips("亲，活动已结束，不能再买了哦");
                }
                else if (result.remain <= 0) {
                    loading.close();
                    $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，不能再买了哦");
                }
                else {
                    loading.close();
                    $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，您最多还能买" + result.remain + "件");
                }
            });
        }
        else {
            $.dialog.errorTips("请选择商品规格！");
        }
    }

    function checkLogin(callBack) {
        var memberId = $.cookie('Himall-User');
        if (memberId) {
            callBack();
        }
        else {
            $.fn.login({}, function () {
                callBack(function () { location.reload(); });
            }, '', '', '/Login/Login');
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
});

function loadGetProductDesc(gid) {
    $.ajax({
        type: 'get',
        url: '/Product/GetProductDesc',
        data: { pid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                if (data.DescriptionPrefix.length > 0) {
                    $("#product-html").append($(data.DescriptionPrefix));
                }
                if (data.ProductDescription.length > 0) {
                    var prodes = $(data.ProductDescription);
                    var imgs = $("img", prodes);
                    imgs.each(function () {
                        var _t = $(this);
                        _t.attr("data-url", _t.attr("src"));  //图片延时加载
                        _t.addClass("lazyload");
                        _t.attr("src", "/Areas/Web/images/blank.gif");  //图片延时加载
                    });
                    $("#product-html").append(prodes);

                    //图片延迟加载
                    $(".lazyload").scrollLoading();
                }
                if (data.DescriptiondSuffix.length > 0) {
                    $("#product-html").append($(data.DescriptiondSuffix));
                }
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

$(function () {
    gid = $("#gid").val();
    $(document).on("click", ".after-service-img img ", function () {
        $(this).addClass("active").siblings().removeClass("active");
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(this).parent().siblings(".preview-img").show();
    });
    $(".preview-img").click(function () {
        $(this).hide()
    });
    $("body").click(function () {
        $(".preview-img").hide()
    });

    $(".startNtc").hover
    (
        function () {
            $(".scan-code").show();
        },
        function () {
            $(".scan-code").hide();
        }
    );

    loadGetProductDesc(gid);
	GetProductComment();
});