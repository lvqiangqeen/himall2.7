// 属性选择弹窗
$(".att-choice").click(function () {
    var _t = $(this);
    var pid = _t.data("pid"),
        collopid = _t.data('collopid');
    var gidtxt = "<input type=\"hidden\" id=\"gid\" value=\"\" />";
    var hastxt = "<input type=\"hidden\" id=\"has\" value=\"\" />";
    var giddom = $("#gid");
    var hasdom = $("#has");
    if (giddom.length == 0) {
        giddom = $(gidtxt);
        $("body").append(giddom);
    }
    giddom.val(pid);
    //获取SKUS
    ShowSKUInfo(pid, function () {
        $('#J_shop_att .modul-popup').addClass('is-visible');
        $('#J_timeBuy_att .modul-popup').addClass('is-visible');

        $('.att-popup-footer button').css('display', 'none');
        $('#addToCart').removeAttr('style');

        $('#choose').himallSku({
            data: { pId: pid, colloPid: collopid },
            resultClass: {
                price: '#jd-price',
                stock: '#stockNum'
            },
            ajaxUrl: 'GetSKUInfo',
            skuPosition: 'skuArray',
            callBack: function (select) {
                $('#choose').data('choose', select.SkuId);
            }
        });

        // 点击关闭
        $('.modul-popup').on('click', function (event) {
            if ($(event.target).is('.modul-popup-close') || $(event.target).is('.modul-popup')) {
                $(this).removeClass('is-visible');
            }
        });

        $('#colorSpec .enabled').click(function () {
            if ($(this).data('img') != '') {
                $('.att-popup-header .thumb img')[0].src = $(this).data('img');
            }
        });

        $('.buy-num').css('display', 'none');

        $("#addToCart").click(function () {
            addcart();
        });
    });
    
});

$('.hmui-after .choice').unbind('click').click(function () {
    var checked = $(this).hasClass('active');
    if (checked) {
        $(this).removeClass('active');
        if ($(this).parent().siblings().not(".hmui-after").find('.choice').hasClass('active')) {
            $(this).parent().siblings().not(".hmui-after").find('.choice').removeClass('active');
        }
    } else {
        $(this).addClass('active');
        var allGoodsCheck = true;
        $(this).parent().parent().find(".hmui-after .choice").each(function () {
            if (!$(this).hasClass('active')) {
                allGoodsCheck = false;
            }
        });
        if (allGoodsCheck) {
            $(this).parent().siblings().not(".hmui-after").find('.choice').addClass('active');

        }
    }
    ComputeShowCartPrice();
});

ComputeShowCartPrice();

//计算并显示金额
function ComputeShowCartPrice() {
    //加载数据
    var PriceTotal = parseFloat($('.pbuy-content .price').find("strong")[0].innerHTML.replace('￥', ''));
    var CouponTotal = parseFloat($('.pbuy-content .price').find("del")[0].innerHTML.replace('￥', ''));
    $(".pbuy-content").find('.hmui-after .choice').each(function () {
        var _t = $(this);
        var _p = $(this).parents('.hmui-after');
        if (_t.hasClass('active')) {
            var a = _p.find('.price strong').html(),
                b = a.replace('￥', ''),
                c = _p.find('.price del').html(),
                d = c.replace('￥', '');
            PriceTotal += parseFloat(b);
            CouponTotal += parseFloat(d);
        }
    });
    PriceTotal = parseFloat(PriceTotal );
    CouponTotal = parseFloat(CouponTotal) - PriceTotal;
    $('#totalSkuPrice').html('￥' + PriceTotal.toFixed(2));
    $('#selectedCount').html('￥' + CouponTotal.toFixed(2));
}

function addcart() {
    var pid = $("#gid").val(),
        len = $('#choose .spec .selected').length;
    if (len === $(".spec").length) {
        var item = $('#choose .spec .selected');
        var resturn = "";
        for (var i = 0; i < item.length; i++) {
            resturn += item[i].innerHTML + "，";
        }
        resturn = resturn.substring(0, resturn.length - 1);

        $('#' + pid).parents('.hmui-after').data('sku', $('#choose').data('choose'));
        $("#" + pid).text("已选择 " + resturn);
        $("#" + pid).removeClass("active");//设置选择按钮样式
        $(".modul-popup").removeClass('is-visible');//关闭属性选择框
        $("#" + pid).parent().parent().find(".choice").addClass("active")//赋值选中
        $("#" + pid).siblings().find('strong').text($('.price-con').text());
        ComputeShowCartPrice();//重新计算价格
    } else {
        $.dialog.errorTips('请选择商品规格');
    }
}

function CollocationBuy() {
    var flag = true,
        arrSku=[],
        arrCounts=[],
        arrCollpids = [];
    if ($('.choice.active').length == 0) {
        $.dialog.errorTips('请至少选择一个组合商品');
        return false;
    }
    $('.hmui-after').each(function (index) {
        if(index==0){
            if ($(this).find('.att-choice.active').length > 0) {
                $.dialog.errorTips('请选择商品规格');
                flag = false;
            } else {
                arrSku.push($(this).data('sku'));
                arrCollpids.push($(this).data('collopid'));
                arrCounts.push('1');
            }
        } else {
            if ($(this).find('.att-choice.active').length > 0 && $(this).find('.choice.active').length > 0) {
                $.dialog.errorTips('请选择商品规格');
                flag = false;
            }
            if ($(this).find('.choice.active').length > 0) {
                arrSku.push($(this).data('sku'));
                arrCollpids.push($(this).data('collopid'));
                arrCounts.push('1');
            }
        }
    });
    $("#skuids").val(arrSku.join(','));
    $('#counts').val(arrCounts.join(','));
    $('#collpids').val(arrCollpids.join(','));
    if (flag) {
        $('#CollProducts').submit();
    }
}
    