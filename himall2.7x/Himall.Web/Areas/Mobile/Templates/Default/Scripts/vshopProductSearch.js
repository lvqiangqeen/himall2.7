$(function () {

    init();
    loadProducts();
    bindSearchBtn();
    initListShowType();

});

function initListShowType() {//初始化列表显示方式
    var listDisplayType = $.cookie('COOKIE_LIST_DISPLAY_TYPE');
    if (listDisplayType) {
        if (listDisplayType == 0) {
            $('.fixed-inner .switch').addClass('state_switch');
            $('.search-list').addClass('crossrange');
        } else {
            $('.fixed-inner .switch').removeClass('state_switch');
            $('.search-list').removeClass('crossrange');
        }
		square($('.search-list .p-img'));
    }
}

function init() {

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();

        if (scrollTop + windowHeight >= scrollHeight) {
            loadProducts();
        }
    });

    $('.fixed-box').height($('.fixed-box').height());
    $(window).scroll(function () {
        var _scroll = $(window).scrollTop();
        var top = $('.fixed-box').offset().top;
        if (_scroll >= top) {
            $('.fixed-inner').addClass('fixed')
        } else {
            $('.fixed-inner').removeClass('fixed')
        }
    });

    //列表显示方式
    $('.switch').click(function () {
        $(this).toggleClass('state_switch');
        $('.search-list').toggleClass('crossrange');
		square($('.search-list .p-img'));
        if ($(this).hasClass('state_switch')) {
            $.cookie('COOKIE_LIST_DISPLAY_TYPE', 0);
        }
        else
            $.cookie('COOKIE_LIST_DISPLAY_TYPE', 1);

    });

    //排序交互
    $('.fixed-inner a').not('.switch').click(function () {
        var keywords = QueryString('keywords');
        var cid = QueryString('cid');
        var b_id = QueryString('b_id');
        var orderType = 2;
        var vshopId = QueryString('vshopid');
        if (orderKey == "3") {//价格
            if ($('.fixed-inner .price').hasClass('state_switch'))
                orderType = 1;
            else
                orderType = 2;
        }
        if (!$.trim(keywords))
            keywords = '';
        location.href = '?vshopid=' + vshopId + '&keywords=' + keywords + '&cid=' + cid + '&b_id=' + b_id + '&orderKey=' + $(this).attr('orderKey') + '&orderType=' + orderType;
    });

    var orderKey = QueryString('orderKey');
    if (orderKey) {
        $('.fixed-inner a[orderKey="' + orderKey + '"]').addClass('select').siblings().removeClass('select');
        if (orderKey == "3") {
            var orderType = QueryString('orderType');
            if (orderType == "2")
                $('.fixed-inner .price').addClass('state_switch');
        }
    }


    $('.fixed-inner .price').click(function () {
        if ($(this).hasClass('select')) {
            $(this).toggleClass('state_switch');
        }
    });

    //筛选操作
    $('.search_screen').click(function () {
        $('.screen').animate({ 'right': 0 }, 300);
        $('.cover').fadeIn();
    });
    $('.screen .cancel,.cover').click(function () {
        $('.screen').animate({ 'right': '-90%' }, 200);
        $('.cover').fadeOut();
    });

    $('#filterSearch').click(function () {
        var query = '?';
        var lis = $('.screen .list-group-item li.selected');
        $.each(lis, function (i) {
            var and = i > 0 ? '&' : '';
            query += and + $(this).attr('key') + '=' + $(this).attr('value');
        });
        location.href = query;
    });


    $('.screen .list-group-item').click(function () {
        $(this).children('.glyphicon').toggleClass('glyphicon-menu-down glyphicon-menu-up');

        $(this).find('ol').toggle();
        /*if(ol.is(':visible')){
			ol.hide();
			
		}*/

    });
    $('.screen .list-group-item li').click(function (e) {
        $(this).addClass('selected').siblings().removeClass();
        $(this).parent().siblings('em').html($(this).text());

    });

    $('#searchList').on('click', 'li', function () {
        location.href = '/' + areaName + '/product/detail/' + $(this).attr('productId');
    });

}


function bindSearchBtn() {
    $('#searchBtn').click(function () {
        page = 1;
    });
}

var page = 1;
var isnodata = false;

function loadProducts() {
    var pageSize = 6;
    var keywords = $('#searchBox').val();
    page++;
    if (!isnodata) {
        $.post('/' + areaName + '/vshop/search?' + location.href.split('?')[1],
            {
                pageNo: page,
                pageSize: pageSize
            }
            , function (products) {
                if (products.length > 0) {
                    var html = '';
                    $.each(products, function (i, product) {
                        html += ' <li>\
            <a class="p-img" href="/'+ areaName + '/product/detail/' + product.id + '"><img src="' + product.img + '" alt=""></a>\
            <h3><a>'+ product.name + '</a></h3>\
            <p class="red">¥'+ product.price + '</p>\
            <p>'+ product.commentsCount + '人评价</p>\
            </li>';
                    });
                    var list = $('#searchList');
                    if (page == 1)
                        list.html('');
                    list.append(html);
                    square($('.search-list .p-img'));
                }
                else {
                    isnodata = true;
                    $('#autoLoad').html('没有更多商品了');
                }
            }
        );
    }
}


