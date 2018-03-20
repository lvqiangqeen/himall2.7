var cate;
var curpagesize = 10, curpageindex = 1, total = -1, lodeEnd = false, shopCategoryId = 0;
$(function () {
    //滚动
    $(".categoryLeft").niceScroll({ cursorwidth: 0, cursorborder: 0 });

    //图片延迟加载
    //   $(".lazyload").scrollLoading({ container: $(".category2") });

    $('.index-category').height($(window).height() - ($('.index-topimg').height() + $('.index-address').height()));

    //点击切换2 3级分类
    var array = new Array();
    $('.categoryLeft li').each(function () {
        array.push($(this).position().top - 56);
    });

    $('.categoryLeft li').click(function () {
        if ($(this).hasClass('cur')) {
            return;
        }
        page = 1;
        var index = $(this).index();
        $('.categoryLeft').delay(200).animate({ scrollTop: array[index] }, 300);
        $(this).addClass('cur').siblings().removeClass();
    });
    $('.bottom-btn').click(function () {
        $('.index-mask').removeClass('hide');
    });
    $('#cancel').click(function () {
        $('.index-mask').addClass('hide');
    });
    $('#call').click(function () {
        $('.index-mask').addClass('hide');
        location.href = "tel:\\" + $.trim($('.content').html());
    });
    $("#categoryLeft li").click(function () {
        $(this).addClass("cur").siblings().removeClass("cur");
        shopCategoryId = Number($(this).attr("id"));
        lodeEnd = false;
        curpageindex = 1;
        total = -1;
        //loadData();
        storeObj.LoadView(shopCategoryId);
    })
    $("#categoryRight").scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(this)[0].scrollHeight;
        var windowHeight = $(this).height();
        if (scrollTop + windowHeight >= scrollHeight) {
            setTimeout(loadData(), 200);
        }
    });
    shopCategoryId = Number($("#categoryLeft li:first").attr("id"));
    loadData();
});
//加载右边数据
function loadData() {
    if (lodeEnd)
        return;
    var queryData = {
        pageNo: curpageindex, pageSize: curpagesize, shopCategoryId: shopCategoryId, shopId: $("#shopId").val(), shopBranchId: $("#shopBranchId").val(), url: "../ProductList"
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.Success == false) {
                $.dialog.tips(data.Message);
            } else {
                var databox = $("#productlist");
                if (curpageindex == 1)
                    databox.empty();
                if (data) {
                    total = data.Total;
                    if (data.Models && data.Models.length > 0) {
                        $.each(data.Models, function (i, model) {
                            var userhtm = getProductHtml(model);
                            databox.append(userhtm);
                        });
                        curpageindex += 1;
                        if (total == data.Models.length)
                            lodeEnd = true;
                    } else {
                        lodeEnd = true;
                    }
                    loadEndProcess();
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function loadEndProcess() {
    if (lodeEnd) {
        $("#autoLoad").show();
        $("#autoLoad").html("没有更多商品了");
    } else {
        $("#autoLoad").hide();
    }
}
//组合商品数据
function getProductHtml(obj) {
    var arr = new Array();
    var showUnit = obj.MeasureUnit|| "";
    arr.push('<li>');
    arr.push('<a href="/m-wap/product/detail/' + obj.Id + '"><img src="' + obj.RelativePath + '"/></a>');
    arr.push('<div class="content">');
    arr.push('<a href="/m-wap/product/detail/' + obj.Id + '"><h3>' + obj.ProductName + '</h3>');
    arr.push('<p>销量 ' + obj.SaleCounts + '' + showUnit + '</p>');
    arr.push('<span class="money"><span>¥</span> ' + obj.MinSalePrice + '</span>');
    arr.push('</a></div>');
    arr.push('</li>');
    return arr.join("");
}
//查看地图
function onMapClick(latitude, longitude, shopbranchAddress) {
    window.location.href = 'http://apis.map.qq.com/tools/routeplan/eword=' + shopbranchAddress + '&epointx=' + longitude + '&epointy=' + latitude + '?referer=myapp&key=OB4BZ-D4W3U-B7VVO-4PJWW-6TKDJ-WPB77';
}
var storeObj = {
    curView: 0,
    LoadView: function (suff) {
        if (storeObj.curView == suff) {
            return;
        }
        loadData();
        storeObj.curView = shopCategoryId;
    }
}