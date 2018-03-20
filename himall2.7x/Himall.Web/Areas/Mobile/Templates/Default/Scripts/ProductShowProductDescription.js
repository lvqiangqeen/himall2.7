var hasDescLoaded = false;
var isDescLoading = false;
var gid_desc = 0;
//事件绑定
$(function () {
    h_loading.init($("#autoLoad"));

    gid_desc = $("#gid_desc").val();
    $("#showmoreprodesc").click(function () {
        LoadProductDesc();
    });
});
//自动加载
$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();
    if (hasDescLoaded) {
        return false;
    }
    if (scrollTop + windowHeight >= scrollHeight) {
        LoadProductDesc();
    }
});

function LoadProductDesc() {
    if (hasDescLoaded) {
        return;
    }
    else {
        GetProductDesc();
        hasDescLoaded = true;
    }
}

function GetProductDesc() {
    if (!isDescLoading) {
        isDescLoading = true;
        h_loading.show();
        var prodesc = "";
        var descurl = "/" + areaName + "/Product/GetProductDescription";
        var prodesccon = $("#prodesccon");
        $.ajax({
            type: 'get',
            url: descurl,
            data: { pid: gid_desc },
            dataType: 'json',
            cache: true,// 开启ajax缓存
            success: function (data) {
                if (data) {
                    //console.log(data);
                    if (data.prodesPrefix.length > 0) {
                        prodesc += data.prodesPrefix;
                    }
                    if (data.prodes.length > 0) {
                        prodesc += data.prodes;
                    }
                    if (data.prodesSuffix != null && data.prodesSuffix.length > 0) {
                        prodesc += data.prodesSuffix;
                    }
                    var d_prodes = $(prodesc);
                    var imgs = $("img", d_prodes);
                    imgs.each(function () {
                        var _t = $(this);
                        _t.attr("data-url", _t.attr("src"));  //图片延时加载
                        _t.addClass("lazyload");
                        _t.attr("src", "/Areas/Web/images/blank.gif");  //图片延时加载
                    });

                    if (prodesccon) {
                        prodesccon.append('<h4><a name="top">商品图文详情</a></h4>');
                        prodesccon.append(d_prodes);

                        //图片延迟加载
                        $(".lazyload").scrollLoading();
                    }
                }
                isDescLoading = false;
            },
            error: function (e) {
                //alert(e);
                isDescLoading = false;
            }
        });
        h_loading.hide();
    }
}