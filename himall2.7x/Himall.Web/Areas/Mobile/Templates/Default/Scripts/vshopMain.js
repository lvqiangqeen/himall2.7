$(function () {
    loadVshops(page++);
    initAddFavorite();
});


var page = 1;

$(window).scroll(function () {
    $('#autoLoad').removeClass('hide');
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        //alert('执行加载');
        loadVshops(page++);
    }
});


function loadVshops(page) {
    var areaname = areaName,
    	lastFn=function(){
    		$('#autoLoad').html('');
        	$('#more').html('<a class="btn btn-primary btn-sm" href="/' + areaname + '/VShop/list"> 查看更多微店 </a>');
    	},
		url = '/' + areaname + '/vshop/GetHotShops';
    $.post(url, { page: page, pageSize: 8 }, function (result) {
        var html = '';
        if(page==1 && result.length<8){
        	lastFn();
        }
        if (result.length > 0) {
            $.each(result, function (i, shop) {
                html += ' <div class="vshop-item">\
                <div class="vshop-head">\
                    <a href="/'+ areaname + '/vshop/detail/' + shop.id + '">\
                    <p class="v-logo"><img src="' + shop.logo + '" /></p>\
                    <p class="v-name">' + shop.name + '</p>\
                    <p class="v-hots">' + '宝贝数 <span>' + shop.productCount + '</span> / 关注度 <span>' + shop.FavoritesCount + '</span>' + '</p>\
                    <p class="v-tags"><span><em>' + shop.Tags + '</em></span></p>\
                    <a class="btn btn-primary btn-sm' + (shop.favorite ? ' fav-yes' : '') + '" href="javascript:;" type="addFavorite" shopId="' + shop.shopId + '"></a>\
                </a></div>\
                <ul class="v-goods clearfix">';
                $.each(shop.products, function (j, product) {
                    html += '<li>\
                        <a class="p-img" href="/' + areaname + '/product/detail/' + product.id + '"><img src="' + product.image + '" alt=""></a>\
                    </li>';
                });
                html += '</ul></div>';
            });
            $('#autoLoad').addClass('hide');
            $('#shopList').append(html);
        }
        else {
            lastFn();
        }
    });
}



$('body').on('click', 'a[type="addFavorite"]', function () {
    var shopId = $(this).attr('shopId');
    var isAdd = $(this).hasClass('fav-yes');
    var returnUrl = '/' + areaName + '/vshop?addFavorite=' + shopId + '&isAdd=' + isAdd;
    checkLogin(returnUrl, function () {
        addFavorite(shopId, isAdd);
    });
});

function initAddFavorite() {
    var shopId = QueryString('addFavorite');
    var isAdd = QueryString('isAdd');
    if (shopId) {//带有addFavorite参数，说明为登录后回调此页面添加收藏
        addFavorite(shopId, isAdd);
    }

}



function addFavorite(shopId, isAdd) {
    var loading = showLoading();
    var method = '';
    var title;
    if (!isAdd) {
        method = 'AddFavorite';
        title = '';
    }
    else {
        method = 'DeleteFavorite';
        title = '取消';
    }
    $.post('/' + areaName + '/vshop/' + method, { shopId: shopId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips(title + '收藏成功!');
            $('a[type="addFavorite"][shopId="' + shopId + '"]')[isAdd?'removeClass':'addClass']('fav-yes');
        }
        else
            $.dialog.errorTips(result.msg);
    });
}