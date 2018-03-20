var pid = 0;
var activeid = 0;
var shopid = 0;
var groupid = null;
var isFightGroupBuy = true;
var getskusurl = "/" + areaName + "/FightGroup/GetSkus";
var isloding = false;

$(function () {
    pid = $("#gid").val();
    activeid = $("#aid").val();
    shopid = $("#sid").val();
    var gpiddom = $("#gpid");
    if (gpiddom) {
        groupid = gpiddom.val();
    }

    $('#choose').himallSku({
        data: { id: activeid },
        productId: pid,
        spec: '.spec',
        resultClass: {
            stock: '#stockNum',
            chose: '#choose-result .dd'
        },
        ajaxType: 'POST',
        ajaxUrl: getskusurl,
        skuPosition: '',
        callBack: function (select) {
            $("#salePrice").html((select.Price).toFixed(2));
        }
    });

    InitBuyEvent();

    //购买数量加减
    $('.wrap-num .glyphicon-minus').click(function () {
        if (parseInt($('#buy-num').val()) > 1) {
            $('#buy-num').val(parseInt($('#buy-num').val()) - 1);
            checkBuyNum();
        }
    });
    $('.wrap-num .glyphicon-plus').click(function () {
        $('#buy-num').val(parseInt($('#buy-num').val()) + 1);
        checkBuyNum();
    });

    $("#buy-num").blur(function () {
        checkBuyNum();
    });

    $("#easyBuyBtn").click(function () {
        easybuy();
    });

    $('#colorSpec .enabled').click(function () {
        if ($(this).data('img') != '') {
            $('.thumb img')[0].src = $(this).data('img');
        }
    });

    $("#bt_govshop").click(function () {
        var _t = $(this);
        var vshopid = _t.attr("vshopid");
        if (vshopid.length < 1) {
            vshopid = -1;
        }
        vshopid = Number(vshopid);
        if (isNaN(vshopid)) {
            vshopid = -1;
        }
        if (vshopid < 1) {
            $.dialog.errorTips("商家暂未开通微店！");
            return false;
        }
    });
})

//事件绑定
function InitBuyEvent() {
    // 属性选择弹窗
    $('.att-popup-trigger,.buy').on('click', function (event) {
        $('#J_shop_att .modul-popup').addClass('is-visible');
        $('#J_timeBuy_att .modul-popup').addClass('is-visible');
        if ($(event.target).is('.buy')) {
            $('.att-popup-footer button').css('display', 'none');
            $('#easyBuyBtn').show();
        }
        else if ($(event.target).is('.att-popup-trigger') || $(event.target).is('.limi-btn')) {
            $('.att-popup-footer button').css('display', 'none');
            $('#easyBuyBtn2, #addToCart2, #justBuy').removeAttr('style');
        }
    });

    // 点击关闭
    $('.modul-popup').on('click', function (event) {
        if ($(event.target).is('.modul-popup-close') || $(event.target).is('.modul-popup')) {
            $(this).removeClass('is-visible');
        }
    });

    escClose('.modul-popup', 'is-visible');
    escClose('#J_pbuy_cover', 'hmui-cover-show');

    // 商品无属性隐藏已选择
    if ($('#choose').length == 0) {
        $('#choose-result').css('display', 'none');
    }
    setScrollAttr();
    $(window).resize(setScrollAttr);
}

function easybuy() {
    var has = $("#has").val();
    var maxnum = $("#maxSaleCount").val();
    maxnum = parseFloatOrZero(maxnum);
    if (has != 1) return;
    var len = $('#choose .spec .selected').length;
    if (len === $(".spec").length) {
        returnHref = "/" + areaName + "/FightGroup/Detail/" + activeid;  
        if (groupid) {
            returnHref = "/" + areaName + "/FightGroup/GroupDetail/" + groupid + "?aid=" + activeid;
        }
        returnHref = encodeURIComponent(returnHref);

        var sku = getskuid();
        if (checkBuyNum()) {
            var num = $("#buy-num").val();
            num = parseFloatOrZero(num);
            if (num <= 0) {
                $.dialog.errorTips("请输入购买数量！");
                return false;
            }
            if (isFightGroupBuy == true) {
                if (maxnum > 0) {
                    if (num > maxnum) {
                        $.dialog.errorTips("每ID限购" + maxnum + "件！");
                        return false;
                    }
                } else {
                    if (num < 0) {
                        $.dialog.errorTips("请输入购买数量！");
                        return false;
                    }
                }
            }
            checkLogin(returnHref, function () {
                $.post('../CheckBuyNumber', { id: activeid, skuId: sku, count: num }, function (result) {
                    if (result.success) {
                        hasbuy = result.hasbuy;
                        if (hasbuy + num <= maxnum) {
                        	//var _url = "/" + areaName + "/Order/SubmitGroup?skuId=" + sku + "&count=" + num + "&GroupActionId=" + activeid ;
                            var _url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=SubmitGroup&skuId={0}&count={1}&groupActionId={2}'.format(sku, num, activeid);
                            if (groupid)
                            {
                                var canjoin = false;
                                $.ajax({
                                    type: "post",
                                    url: '/' + areaName + '/FightGroup/CanJoin',
                                    data: { aid: activeid, gpid: groupid },
                                    async: false,   //必须同步
                                    success: function (data) {
                                        if (data.success == true) {
                                            canjoin = true;
                                        }
                                    }
                                });

                                if (!canjoin) {
                                    $.dialog.errorTips("不可参团，可能重复参团或火拼团不在进行中！");
                                    return;
                                }
                                _url += "&GroupId=" + groupid;
                            }
                            location.href = _url;
                        } else if (hasbuy > maxnum) {
                            $.dialog.errorTips("亲，限购" + maxnum + "件，不能再买了哦");
                        } else {
                            $.dialog.errorTips("亲，限购" + maxnum + "件，您最多还能买" + (maxnum-hasbuy) + "件");
                        }
                    } else {
                        $.dialog.errorTips(result.msg);
                    }
                });
            }, shopid);
        }
    } else {
        $.dialog.errorTips('请选择商品规格');

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

function checkBuyNum() {
    var stockNum = $("#stockNum").html();
    var maxbuynum = $("#maxSaleCount").val();
    stockNum = parseFloatOrZero(stockNum);
    maxbuynum = parseFloatOrZero(maxbuynum);
    var maxnum = stockNum > maxbuynum ? maxbuynum : stockNum;
    return checkSkuBuyNum($("#buy-num"), maxnum);
}

function getskuid() {
    var gid = $("#gid").val();
    return getskuidbypid(gid);
}

function addFavoriteFun() {
    var url;
    var css;
    if ($('#favoriteProduct').attr("class").indexOf("red") >= 0) {
        url = "/" + areaName + "/Product/DeleteFavoriteProduct";
        css = 'addfav cell iconfont icon-fav-c';
    } else {
        url = "/" + areaName + "/Product/AddFavoriteProduct";
        css = 'addfav cell iconfont icon-fav-c red';
    }
    $.post(url, {
        pid: $("#gid").val()
    }, function (result) {
        if (result.success == true) {
            $('#favoriteProduct').removeClass().addClass(css);
            $.dialog.succeedTips(result.msg, '', 1);
        }
    });
}


// ESC关闭弹出层
function escClose(obj, claName) {
    $(document).keyup(function (event) {
        if (event.which == '27') {
            $(obj).removeClass(claName);
        }
    });
}


function setScrollAttr() {
    var DocW = $('#slides').width();
    $('.detail-bd:first').css('marginTop', DocW);
}

function notOpenVShop() {
    $.dialog.tips('暂未开通微店');
}