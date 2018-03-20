$(function () {
    
    //搜索切换
    $('.search-form label').click(function () {
        $(this).siblings('ul').css("display","block");
    });
    $('.search-form ul li').click(function () {
        $(this).parent().css("display","none").siblings('label').text($(this).text());
    });

    //上下页切换



    //右侧菜单
    $('#right_cart em').text($('shopping-amount').text());

    var rightSide = $('.right-side');
    var rightContent = $('.side-content');
    $('.right-side .menu-top li[arrow]').click(function () {
        var islogin = $("#isLogin").val();
        if (islogin!="true") {
            $.fn.login({}, function () {
                location.href = "/";
            }, '', '', '/Login/Login');
        } else {

            var tag = $(this).data('tag');
            switch (tag) {
                case 'cart':
                    loadCartInfo();
                    rightContent.find('.side-cart-c').show().siblings().hide();
                    break;
                case 'asset':
                    rightContent.find('.side-asset-c').show().siblings().hide();
                    break;
                case 'at-goods':
                    rightContent.find('.side-atgoods-c').show().siblings().hide();
                    break;
                case 'history':
                    rightContent.find('.side-history-c').show().siblings().hide();
                    break;
            }
            if (rightContent.is(':hidden')) {
                rightContent.show();
                rightSide.css('right', '-220px').animate({ right: '0' }, 300);
            }
            $(this).addClass('active').siblings().removeClass('active');
        }
    });

    $('.right-side-menu li').hover(function () {
        $(this).find('span').show().stop(false, true).animate({ 'right': 40, 'opacity': 1 }, 300);
    }, function () {
        $(this).find('span').stop(false, true).animate({ 'right': 60, 'opacity': 0 }, 300, function () { $(this).hide() });
    });

    $(document).on('click', function (e) {
        event = e ? e : window.event;
        var obj = event.srcElement ? event.srcElement : event.target;
        if (!$.contains(rightSide.get(0), obj)) {
            sideRightClose();
        }
    });

    function sideRightClose() {
        if (rightContent.is(':visible')) {
            if (!rightSide.is(':animated')) {
                rightSide.animate({ right: '-220px' }, 200, function () {
                    rightContent.hide();
                    rightSide.css('right', 0);
                    $('.right-side .menu-top li[arrow]').removeClass('active')
                });
            }
        }
    }

    $('.side-close').click(function () {
        sideRightClose();
    });

    $('.go-top').click(function () {
        $('body,html').animate({ 'scrollTop': 0 }, 300);
    });

    $('.side-cart-c .cart-list').height($(window).height() - 120);
    $(window).resize(function () {
        $('.side-cart-c .cart-list').height($(window).height() - 120);
    });

    $(".side-asset-c .side-bd,.side-atgoods-c .side-bd,.side-history-c .side-bd,.side-cart-c .cart-list").niceScroll({
        cursorwidth: 5,
        cursorcolor: "#616161",
        cursorborder: 0,
        cursorborderradius: 0
    });

    $('#toSubmitOrder').click(function () {
        var arr = [], str = '';
        $('input[name="checkItem"]').each(function (i, e) {
            if ($(e).attr('checked')) {
                arr.push($(e).data('cartid'));
            }
        });
        str = (arr && arr.join(','));

        if (str != "")
            location.href = '/order/submit?' + 'cartItemIds=' + str;
        else
            $.dialog.errorTips("没有可结算的商品！");
    });

    InitialBanner();

    if ($('.category').css('display') == 'none') {
        $('.categorys').mouseDelay().hover(function () {
            $('.category').show();
        });
        $('.categorys').mouseleave(function () {
            $('.category').hide();
        });
    }

    function queryForm(pageNo) {
        var keyWords = $.trim($('#searchBox').val());
        var exp_keyWords = "";
        if (!typeof ($("#text-stock-search").val()) == "undefined") {
            exp_keyWords = $("#text-stock-search").val();
        }
        var cid = getQueryString('cid');
        var b_id = getQueryString('b_id');
        var orderType = getQueryString('orderType');
        var orderKey = getQueryString('orderKey');
        //location.href = "/search?pageNo=" + pageNo + "&keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
        //                        + "&exp_keywords=" + exp_keyWords + "&cid=" + cid + "&b_id=" + b_id + "&orderType=" + orderType
        //                        + "&orderKey=" + orderKey;
        location.href = "/search/searchAd?pageNo=" + pageNo + "&keywords=" + encodeURIComponent(keyWords)
                               + "&exp_keywords=" + exp_keyWords + "&cid=" + cid + "&b_id=" + b_id + "&orderType=" + orderType
                               + "&orderKey=" + orderKey;
    }

    $('#searchBtn').click(function () {
        var keyWords = $.trim($('#searchBox').val());

        var selected = $(".search .search-form label").html();
        if (selected == "店铺") {
            //if (keyWords == '') {
            //    $.dialog.alert('请输入关键词！');
            //    return;
            //}
            location.href = "/shopsearch?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
        }
        else {
            //location.href = "/search?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'));
            location.href = "/search/searchAd?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'));
        }

    });

    $("#searchBox").keydown(function (e) {
        if (e.keyCode == 13) {
            var keyWords = $.trim($('#searchBox').val());

            var selected = $(".search .search-form label").html();
            if (selected == "店铺") {
                if (keyWords == '') {
                    return;
                }
                location.href = "/shopsearch?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
            }
            else {
                location.href = "/search/searchAd?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
            }
        }
    });
    
    //$('#searchBox').autocomplete({
    //    source: function (query, process) {
    //        $.get("../search/GetQueryWords", { "query": $('#searchBox').val()}, function (respData) {
    //            return process(respData.words);
    //        });
    //    },
    //    formatItem: function (item) {
    //        return item;
    //    },
    //    setValue: function (item) {
    //        return { 'data-value': item, 'real-value': item };
    //    }
    //});
    
    

    $('#btn-re-search').click(function () {
        var keyWords = $('#key-re-search').val();
        location.href = "/search?keywords=" + encodeURIComponent(keyWords);
    });

    $('#btn-stock-search').click(function () {
        queryForm(1);
    });

    $('#pageJump').click(function () {
        var pageNo = parseInt($("#jumpInput").val());
        var pagecount = parseInt($("#pageCount").html());
        if (pageNo > pagecount || pageNo < 1) {
            alert("请您输入有效的页码!");
            return;
        }
        if (isNaN(pageNo)) {
            pageNo = 1;
        }
        queryForm(pageNo);
    });

    $("#right_cart").click(function () {
        //location.href = "/cart/cart";
        //window.open("/cart/cart");
    });

    $("#right_userCenter").click(function () {
        var islogin = $("#isLogin").val();
        if (islogin == "true") {
            window.open("/usercenter");
        }
        else {
            $.fn.login({}, function () {
                location.href = "/";
            }, '', '', '/Login/Login');
        }
    });

});
var data = {};
function loadCartInfo() {

    $.post('/cart/GetCartProducts', {}, function (cart) {
        data = {};
        $.each(cart.products, function (i, e) {
            if (data[e.shopId]) {
                if (!data[e.shopId]['name']) {
                    data[e.shopId]['name'] = e.shopName;
                }
                data[e.shopId]['shop'].push(e);
            } else {
                data[e.shopId] = {};
                data[e.shopId]['shop'] = [];
                data[e.shopId]['name'] = e.shopName;
                data[e.shopId]['status'] = e.productstatus;
                data[e.shopId]['shop'].push(e);
            }
        });
        var strproductstatus = $("#hidSaleStatus").val();
        var strproductauditstatus = $("#hidAuditStatus").val();
        var list = '';
        if (cart.products.length > 0) {
            $.each(data, function (i, e) {
                var shopPrice = 0;
                var str = '';
                $.each(e.shop, function (j, product) {

                    if (product.productstatus == strproductstatus) {
                        if (product.productauditstatus == strproductauditstatus) {
                            str += '\
							<div class="cart-list-goods cl">\
								<input class="checkbox" type="checkbox"  data-cartid="'+ product.cartItemId + '" name="checkItem" value="' + product.shopId + '" checked />';

                            str += '\
								<a href="/product/detail/' + product.id + '" title="' + product.name + '" target="_blank"><img src="' + product.imgUrl + '" alt="" /></a>\
								<div class="s-num"><span>' + product.count + '</span></div>\
								<div class="s-g-price">'+ (product.price * product.count).toFixed(2) + '</div>\
							</div>';

                            shopPrice = shopPrice + product.price * product.count;
                        }
                    }
                });
                list += '<li><div class="cart-list-shop cl">\
						<input class="shopSelect" type="checkbox" value="' + i + '" name="checkShop" checked />\
						<p><a href="/shop/home/'+ i + '" target="_blank">' + e.name + '</a></p>\
						<span class="cart-shop-price" data-shoprice="'+ shopPrice.toFixed(2) + '">' + shopPrice.toFixed(2) + '</span>\
						</div>'+ str + '</li>';

            });
            $('#side-cart-list').html(list);
            $('#s-total-num').html(cart.totalCount);
            $('#s-total-money').html(cart.amount.toFixed(2));
            bindSelectAll();
        }
    });
}

function bindSelectAll() {
    $('input[name="s-checkAll"]').change(function () {
        var checked = this.checked;
        $('#side-cart-list input[type=checkbox]').prop('checked', this.checked);
        if (checked) {
            $('#s-total-money').html(getCheckProductPrice());
            $('#side-cart-list li').each(function () {
                var shopP = $(this).find('.cart-shop-price');
                shopP.html(shopP.data('shoprice').toFixed(2));
            });
        }
        else {
            $('#s-total-money').html("0.00");
            $('#side-cart-list .cart-shop-price').html("0.00");

        }
        $('#s-total-num').html(getCheckProductCount());
    });

    $('input[name="checkShop"]').change(function () {
        var checked = this.checked;
        var total = $(this).siblings('.cart-shop-price').html();
        $(this).parent().siblings().find('input[type="checkbox"]').prop('checked', checked);

        var allShopChecked = true;
        $('#side-cart-list input[type="checkbox"]').each(function () {
            if (!$(this).prop('checked')) {
                allShopChecked = false;
            }
        });
        if (allShopChecked)
            $('input[name="s-checkAll"]').prop('checked', true);
        else
            $('input[name="s-checkAll"]').prop('checked', false);

        var t = 0;
        $.each($(this).parent().siblings(), function () {
            var a = $(this).find('input[name="checkItem"]:checked').siblings('.s-g-price').html();
            t += (+a);
        })
        $(this).siblings('.cart-shop-price').html(t.toFixed(2));
        $('#s-total-money').html(getCheckProductPrice());
        $('#s-total-num').html(getCheckProductCount());
    });

    $('input[name="checkItem"]').change(function () {
        var checked = this.checked;
        if (checked)
            $(this).prop('checked', checked);
        else
            $(this).removeAttr('checked');

        //判断店铺下的所有商品是否全选中
        var allProductChecked = true;
        $(this).parent().siblings('.cart-list-goods').each(function () {
            if (!$(this).find('input').prop('checked'))
                allProductChecked = false;
        });
        if (allProductChecked)
            $(this).parent().siblings().find('input[name="checkShop"]').prop('checked', checked);
        else
            $(this).parent().siblings().find('input[name="checkShop"]').removeAttr('checked');;

        //判断所有店铺是否都选中了
        var allShopChecked = true;
        $('#side-cart-list input[type="checkbox"]').each(function (i, e) {
            if (!$(this).prop('checked')) {
                allShopChecked = false;
            }
        });
        if (allShopChecked)
            $('input[name="s-checkAll"]').prop('checked', true);
        else
            $('input[name="s-checkAll"]').removeAttr('checked');

        var t = 0;
        $.each($(this).parents('li').find('input[name="checkItem"]:checked'), function () {
            var a = $(this).siblings('.s-g-price').html();
            t += (+a);
        })
        $(this).parent().siblings('.cart-list-shop').find('.cart-shop-price').html(t.toFixed(2));

        $('#s-total-money').html(getCheckProductPrice());
        $('#s-total-num').html(getCheckProductCount());
    });

}

function getCheckProductPrice() {
    var t = 0;
    $.each($('input[name="checkItem"]:checked'), function () {
        var a = $(this).siblings('.s-g-price').html();
        t += (+a);
    })
    return t.toFixed(2);
}

function getCheckProductCount() {
    var t = 0;
    $.each($('input[name="checkItem"]:checked'), function () {
        var c = $(this).siblings('.s-num').children().html();
        d = parseInt(c);
        t += d;
    })
    return t;
}

var sUserAgent = navigator.userAgent.toLowerCase();
var bIsIpad = sUserAgent.match(/ipad/i) == "ipad";
var bIsIphoneOs = sUserAgent.match(/iphone os/i) == "iphone os";
var bIsMidp = sUserAgent.match(/midp/i) == "midp";
var bIsUc7 = sUserAgent.match(/rv:1.2.3.4/i) == "rv:1.2.3.4";
var bIsUc = sUserAgent.match(/ucweb/i) == "ucweb";
var bIsAndroid = sUserAgent.match(/android/i) == "android";
var bIsCE = sUserAgent.match(/windows ce/i) == "windows ce";
var bIsWM = sUserAgent.match(/windows mobile/i) == "windows mobile";
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}

function InitialBanner() {
    var isSelect = false;
    $("._banner").each(function () {
        $(this).removeClass("cur");
        if ($(this).attr("index") === window.location.href) {
            $(this).addClass("cur");
            isSelect = true;
        }
        if ($(this).attr("index").indexOf("/LimitTimeBuy/") >= 0
            && window.location.href.indexOf("/LimitTimeBuy/") >= 0) {
            $(this).addClass("cur");
            isSelect = true;
        }
        else {
            if ($(this).attr("index").toLowerCase().indexOf("/topic/list") >= 0
            && window.location.href.toLowerCase().indexOf("/topic/list") >= 0) {
                $(this).addClass("cur");
                isSelect = true;
            }
        }

        if ($(this).attr("index").indexOf("/IntegralMall") >= 0
            && window.location.href.toLowerCase().indexOf("/integralmall") >= 0) {
            $(this).addClass("cur");
            isSelect = true;
        }
    });

    if (!isSelect) {
        $("#homePage").addClass("cur");
    }
}


function bindCartItemDelete() {
    $('#productsList').on('click', 'a.delete', function () {
        var skuId = $(this).attr('skuId');
        removeFromCart(skuId);
    });
}

function removeFromCart(skuId) {
    $.post('/cart/RemoveFromCart', { skuId: skuId }, function (result) {
        if (result.success)
            refreshCartProducts();
        else
            alert(result.msg);
    });
}