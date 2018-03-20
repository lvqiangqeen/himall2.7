var limitPrice = null;
var fightPrice = null;
var discount = 1;
var selfShopId = 0;
$(function () {
    initFilter();
    initPrice();
});

function initListShowType() {
    // 初始化列表显示方式
    var listDisplayType = $.cookie('COOKIE_LIST_DISPLAY_TYPE');
    if (listDisplayType) {
        if (listDisplayType == 0) {
            $('.fixed-inner .switch').addClass('state_switch');
            $('.search-list').addClass('crossrange');
        } else {
            $('.fixed-inner .switch').removeClass('state_switch');
            $('.search-list').removeClass('crossrange');
        }
    }
    $("#searchList").show();
}

function initFilter() {
    $.ajax({
        type: 'get',
        url: '/' + areaName + '/Search/GetSearchFilter',
        data: { keyword: keyword, a_id: a_id, b_id: b_id, cid: cid },
        dataType: 'JSON',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data.success) {
                //console.log(data);
                renderCategory(data.Category);
                renderBrand(data.Brand);
                renderAttr(data.Attr);
                init();
                initListShowType();
            }
        }
    });
}

function initPrice() {
    $.ajax({
        type: 'get',
        url: '/' + areaName + '/Search/GetSalePrice',
        data: {},
        dataType: 'JSON',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data.success) {
                limitPrice = data.LimitProducts;
                fightPrice = data.FightProducts;
                discount = data.Discount;
                selfShopId = data.SelfShopId;
                FlushPrice();
            }
        }
    });
}

function FlushPrice() {
    //会员价
    $("#searchList li[shopid='" + selfShopId + "']").each(function () {
        var disprice = (parseFloat($(this).attr("price")) * parseFloat(discount));
        var showdisprice = disprice.toFixed(3);
        $(this).find(".red").text("￥" + (parseFloat(showdisprice)).toFixed(2));
    });
    //限时购
    $(limitPrice).each(function () {
        $("#searchList li[productid='" + this.ProductId + "']").find(".red").text("￥" + this.MinPrice.toFixed(2));
    });
    //拼团
    $(fightPrice).each(function () {
        $("#searchList li[productid='" + this.ProductId + "']").find(".red").text("￥" + this.ActivePrice.toFixed(2));
    });
}

function renderAttr(data) {
    var template = '<ul class="list-group"><li class="list-group-item">{0}<span class="glyphicon glyphicon-menu-down"></span><em name="全部{3}">全部{1}</em><ol>{2}</ol></li></ul>';
    var html = "";
    $(data).each(function () {
        var second = "";
        $(this.AttrValues).each(function () {
            second += '<li key="a_id" valueaa="{0}"><span>{1}</span><i class="glyphicon glyphicon-ok"></i></li>'.replace("{0}",this.Id).replace("{1}",this.Name);
        });
        html += template.replace("{0}", this.Name).replace("{1}", this.Name).replace("{2}", second).replace("{3}",this.Name);
    });
    $(".btn-block").before(html);
}

function renderCategory(data) {
    var template = '<li key="cid" valueaa="{0}"><span>{1}</span><i class="glyphicon glyphicon-ok"></i></li>';
    var html = "";
    var hasCate = false;
    $(data).each(function () {
        $(this.SubCategory).each(function () {
            $(this.SubCategory).each(function () {
                html += template.replace("{0}", this.Id).replace("{1}", this.Name);
                hasCate = true;
            });
        });
    });

    $("#categorylist ol:first").append(html);
    if (hasCate)
        $("#categorylist").removeClass("hide");
}

function renderBrand(data) {
    var template = '<li key="b_id" valueaa="{0}"><span>{1}</span><i class="glyphicon glyphicon-ok"></i></li>';
    var html = "";
    var hasBrand = false;
    $(data).each(function () {
        html += template.replace("{0}", this.Id).replace("{1}", this.Name);
        hasBrand = true;
    });

    $("#brandlist ol:first").append(html);
    if(hasBrand)
        $("#brandlist").removeClass("hide");
}

function init() {
    if ($('#searchList li').length == 0)
        $('#autoLoad').html('没有搜索相关商品');

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

    // 列表显示方式
    $('.switch').click(function () {
        $(this).toggleClass('state_switch');
        $('.search-list').toggleClass('crossrange');
        if ($(this).hasClass('state_switch')) {
            $.cookie('COOKIE_LIST_DISPLAY_TYPE', 0);
        }
        else
            $.cookie('COOKIE_LIST_DISPLAY_TYPE', 1);
    });

    //排序交互
    $('.fixed-inner a').not('.switch').click(function () {
        var type = 2;
        if (orderKey == "3") {//价格
            if ($('.fixed-inner .price').hasClass('state_switch'))
                type = 1;
            else
                type = 2;
        }

        location.href = "?keywords=" + keyword + '&cid=' + cid + '&b_id=' + b_id + '&a_id=' + a_id + '&orderKey=' + $(this).attr('orderKey') + '&orderType=' + type;
    });

    var orderKey = QueryString('orderKey');
    if (orderKey) {
        $('.fixed-inner a[orderKey="' + orderKey + '"]').addClass('select').siblings().removeClass('select');
        if (orderKey == "3") {
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
        query += "keywords=" + keyword + "&";
        var lis = $('.screen .list-group-item li.selected');
        var aid = "";
        $.each(lis, function (i) {
            if ($(this).attr('key') === "a_id") {
                if (aid === "")
                    aid += $(this).attr('key') + '=' + $(this).attr('valueaa') + ",";
                else
                    aid += $(this).attr('valueaa') + ",";
            }
            else
                query += $(this).attr('key') + '=' + $(this).attr('valueaa') + "&";
        });
        if (aid != "")
            aid = aid.substr(0, aid.length - 1);
        location.href = query +  "&" + aid;
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

    $('.btn-block').click(function () {
        var lis = $('.screen .list-group-item li');
        $.each(lis, function (i, e) {
            $(e).removeClass('selected');
            $(e).parent().prev('em').html($(e).parent().prev('em').attr('name'));
        });
    });
}

var page = 1;
var isnodata = false;

function loadProducts() {
    var pageSize = 10;
    var keywords = $('#searchBox').val();
    if (!isnodata) {
        page++;

        $.post('/' + areaName + '/search?' + location.href.split('?')[1],
            {
                pageNo: page,
                pageSize: pageSize,
                rnd: Math.random()
            }
            , function (products) {
                if (products.length > 0) {
                    var html = '';
                    $.each(products, function (i, product) {//图片后台处理
                        html += ' <li productid="' + product.ProductId + '" shopid="' + product.ShopId + '" price="' + product.SalePrice + '">\
						<a class="p-img" href="/' + areaName + '/product/detail/' + product.ProductId + '"><img src="' + product.ImagePath + '/1_220.png" alt=""></a>\
						<h3><a href="/' + areaName + '/product/detail/' + product.ProductId + '">' + product.ProductName + '</a></h3>\
						<p class="red">¥' + product.SalePrice + '</p>\
						<p>'+ product.Comments + '人评价</p>\
                        <p>销量：' + product.SaleCount + '</p>\
						</li>';
                    });
                    var list = $('#searchList');
                    if (page == 1)
                        list.html('');
                    list.append(html);
                    FlushPrice();
                }
                else {
                    isnodata = true;
                    $('#autoLoad').html('没有更多商品了');
                }
            }
        );
    }
}
