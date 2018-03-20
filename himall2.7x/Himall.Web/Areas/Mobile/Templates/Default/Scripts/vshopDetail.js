var page = 2;
var vCouponScroll;
$(window).scroll(function () {
    $('#autoLoad').removeClass('hide');
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        //alert('执行加载');
        loadProducts(page++);
    }
});

$(function () { 

    vshopId = $("#vshopId").val();
    shopid = $("#shopId").val();

    //var isF = QueryString('addFavorite');
    //if (isF) {//带有addFavorite参数，说明为登录后回调此页面添加收藏
    //    addFavorite();
    //}

    returnFavoriteHref = "/" + areaName + "/vshop/Detail/" + vshopId;
    returnFavoriteHref = encodeURIComponent(returnFavoriteHref);

    $("#favorite").click(function () {
        checkLogin(returnFavoriteHref, function () {
            addFavorite();
        });
    });
    initSearchBox();
    initCoupon();
	var wshop = $('.vshop-coupon').width();
    //var len = $('.vshop-coupon ul li').length;
    $('.vshop-coupon li').width(wshop * 0.4);
	if($('#vshopCoupon').length>0)
		vCouponScroll = new iScroll("vshopCoupon",{vScroll:false,hScroll:true,hScrollbar:false});
});

function initCoupon() {

    $('.v-coupon-btn').parent().click(function () {
        var $thisCoupon = $(this).find('a').eq(0);
        var cpid = $thisCoupon.attr('cpid') || 0;
        if (parseInt(cpid) > 0) {
            var returnUrl = '/' + areaName + '/vshop/detail/' + vshopId + '?couponid=' + cpid;
            checkLogin(returnUrl, function () {
                var loading = showLoading();
                $.post('../AcceptCoupon', { vshopid: vshopid, couponid: parseInt(cpid) }, function (result) {
                    loading.close();
                    if (result.status == 1) {
                        //alert('未登录');
                        var returnUrl = '/' + areaName + '/vshop/detail/' + vshopId + '?couponid=' + cpid;
                        var loginUrl = '/' + areaName + '/Login/Entrance?returnUrl=' + encodeURIComponent(returnUrl);
                        window.location.href = loginUrl;
                        return;
                    }
                    else {
                        if (result.status == 0) {
                            window.location.href = '/' + areaName + '/VShop/GetCouponSuccess/' + result.crid;
                        } else {
                            $.dialog.tips(result.msg, function () {
                                if (window.location.href.toLowerCase().indexOf('couponid=') > 0) {
                                    //window.location.href = '/' + areaName + '/vshop/detail/' + vshopId;
                                    window.location.href = '/' + areaName + '/VShop/GetCouponSuccess/' + result.crid;
                                }
                            })

                        }
                        return;
                    }
                });
            }, MAppType);
        }
    });
    /*
    acceptId = parseInt(acceptId || 0);
    if (acceptId > 0) {
        var loading = showLoading();
        $.post('../AcceptCoupon', { vshopid: vshopid, couponid: acceptId }, function (result) {
            loading.close();
            if (result.status == 1) {
                //alert('未登录');
                var returnUrl = '/' + areaName + '/vshop/detail/' + vshopId + '?couponid=' + acceptId;
                var loginUrl = '/' + areaName + '/Login/Entrance?returnUrl=' + encodeURIComponent(returnUrl);
                window.location.href = loginUrl;
                return;
            }
            else {
                $.dialog.tips(result.msg, function () {
                    if (window.location.href.toLowerCase().indexOf('couponid=') > 0) {
                        window.location.href = '/' + areaName + '/vshop/detail/' + vshopId;
                    }
                });
                return;
            }
        });
    }*/
}

function initSearchBox() {
    $('#searchBtn').click(function () {
        var keywords = $('#searchBox').val();
        location.href = '/' + areaName + '/Vshop/Search?vshopId=' + vshopId + '&keywords=' + encodeURIComponent(keywords);
    });

}

function loadProducts(page) {
    var areaname = areaName;

    var url = '/' + areaname + '/vshop/productlist';
    $.post(url, { shopId: $("#shopId").val(), pageNo: page, pageSize: 8 }, function (result) {
        var html = '';
        if (result.length > 0) {
            $.each(result, function (i, product) {
                var url = '/' + areaname + '/product/detail/' + product.Id;
                html += '<li><a class="p-img" href="' + url + '"><img src="' + product.ImageUrl + '" alt=""></a>';
                html+='<h3><a>'+product.Name+'</a></h3>';
                html+= '<p><span>'+product.SalePrice+'</span></p></li>';
            });
            $('#autoLoad').addClass('hide');
            $('#products').append(html);
        }
        else {
            $('#autoLoad').html('没有更多商品了');
        }
    });
}


function initAddFavorite() {
   
}

function addFavorite() {
    var loading = showLoading();
    var url;
    var value;
    if ($("#favorite").text() == "收藏") {
        url = "../AddFavorite";
        value = "已收藏";
    }
    else {
        url = "../DeleteFavorite";
        value="收藏";
    }
    $.post(url, { shopId: shopid }, function (result) {
        loading.close();
        $("#favorite").text(value);
        $.dialog.succeedTips(result.msg,null,0.5);
    });
}


/* 微店详情 */
var isClose = $("#vshopState").val();
var acceptId = $("#vshopAcceptId").val();
var vshopid = $("#vshopBagId").val();
var IsPlatform = $("#vshopPlatform").val();
$(function () {
    if (isClose == "Close") {
        $.dialog.errorTips("该微店已下架", function () {
            history.go(-1);
        }, 5);
    }
    if (IsPlatform == 1)
    {
        $('#homeLink').hide();
        $('.btn_more').hide();
    }
    
    var slides=$('#slides');
    if(slides.children().length==0){
        slides.hide();
    }
    if(slides.children().length>1){
        slides.slidesjs({
            width: 640,
            height: 320,
            navigation: false,
            play: {
                active: false,
                auto: true,
                interval: 4000,
                swap: true
            }
        });
    }else{
        slides.css({'height':$(document).width()/2})
    }
});
