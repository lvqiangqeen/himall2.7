var vshopId = $("#vshopId").val();
var shopid = $("#shopId").val();

$(function () {
    returnFavoriteHref = "/" + areaName + "/vshop/introduce/" + vshopId;
    returnFavoriteHref = encodeURIComponent(returnFavoriteHref);

    $("#favorite").click(function () {
        checkLogin(returnFavoriteHref, function () {
            addFavorite();
        });
    });
});

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
        value = "收藏";
    }
    $.post(url, { shopId: shopid }, function (result) {
        loading.close();
        $("#favorite").text(value);
        $.dialog.succeedTips(result.msg, null, 0.5);
    });
}