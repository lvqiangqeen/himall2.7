var curPage = 1;       //当前页
var isMoreGet = true;  //是否还在数据
var isLoading = false;
var TmplCon = "";
var skey = "";
var databox;
var sort = 0;
var gddatas;   //数据变量
var shopid=0;


$(document).ready(function () {

    TmplCon = $("#gdtmp").html(); //获取模板内容
    databox = $("#databox");
    h_loading.init($("#autoLoad"));

    getData();

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();

        if (scrollTop + windowHeight >= scrollHeight - 30) {
            curPage++;
            getData();
        }
    });

    $(".datasort").click(function () {
        var _t = $(this);
        sort = _t.data("sort");
        initdata();
        getData();
        $(".datasort").removeClass("on");
        _t.addClass("on");
    });


    $('.bt_shopfav').on('click', function () {
        var shopId = $(this).attr('shopId');
        var isAdd = $(this).hasClass("favorited") ? false : true;
        var returnUrl = '/' + areaName + '/vshop?addFavorite=' + shopId + '&isAdd=' + isAdd;
        checkLogin(returnUrl, function () {
            addFavorite(shopId, isAdd);
        });
    });

})

function initdata() {
    isMoreGet = true;
    curPage = 1;
    databox.empty();
}

function getData() {
    if (isMoreGet) {
        //loading = showLoading();
        if(!isLoading){
            isLoading=true;
            h_loading.show();
            var prolistUrl = '/' + areaName + '/DistributionShop/ProductList/';
            $.post(prolistUrl, { shopId: shopid, skey: skey, page: curPage, sort: sort }, function (result) {
                //loading.close();
                isLoading=false;
                gddatas = result;
                if (gddatas.length > 0) {
                    databox.append(_.template(TmplCon, gddatas));
                    showData();
                    h_loading.hide();
                } else {
                    isMoreGet = false;
                    h_loading.nodata();
                }
            });
        }
    }
}

function addFavorite(shopId, isAdd) {
    var loading = showLoading();
    var method = '';
    var title;
    if (isAdd) {
        method = 'AddFavorite';
        title = '';
    }
    else {
        method = 'DeleteFavorite';
        title = '取消';
    }
    $.post('/' + areaName + '/DistributionMarket/' + method, { shopId: shopId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips(title + '收藏成功!');
            var bt_shopfav = $('.bt_shopfav');
            bt_shopfav.html(isAdd ? '已收藏<i></i>' : '收　藏<i></i>');
            if (isAdd) {
                bt_shopfav.addClass("favorited");
            } else {
                bt_shopfav.removeClass("favorited");
            }
        }
        else
            $.dialog.errorTips(result.msg);
    });
}
