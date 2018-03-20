$(function () {
    $('#addShopFavorite').on('click', function () {
        var _t = $(this);
        var shopId = _t.attr('shopId');
        var isAdd = _t.hasClass('fav-yes');
        var returnUrl = '/' + areaName + '/Product/Detail/' + $('#gid').val() + '?addFavorite=' + shopId + '&isAdd=' + isAdd;
        checkLogin(returnUrl, function () {
            addShopFavorite(shopId, isAdd);
        });
    });
});

function addShopFavorite(shopId, isAdd) {
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
            $.dialog.succeedTips(title + '关注成功!');
            var btfav = $('#addShopFavorite[shopId="' + shopId + '"]');
            if (isAdd) {
                //$('#addShopFavorite[shopId="' + shopId + '"]')[isAdd ? 'removeClass' : 'addClass']('fav-yes');
                btfav.removeClass('fav-yes').html("关注");
            } else {
                btfav.addClass('fav-yes').html("已关注");
            }
        }
        else
            $.dialog.errorTips(result.msg);
    });
}