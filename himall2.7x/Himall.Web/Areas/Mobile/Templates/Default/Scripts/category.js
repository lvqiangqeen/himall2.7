var cate;
$(function () {
    //滚动
    $(".category1").niceScroll({ cursorwidth: 0, cursorborder: 0 });

    //图片延迟加载
    //   $(".lazyload").scrollLoading({ container: $(".category2") });

    $('.category-box').height($(window).height());

    //点击切换2 3级分类
    var array = new Array();
    $('.category1 li').each(function () {
        array.push($(this).position().top - 56);
    });

    $('.category1 li').click(function () {
        var index = $(this).index();
        $('.category1').delay(200).animate({ scrollTop: array[index] }, 300);
        $(this).addClass('cur').siblings().removeClass();
        $('.category2 dl').eq(index).show().siblings().hide();
        $('.category2').scrollTop(0);
        //设置选中目录 
        window.sessionStorage.setItem("selectCategory", $(this).text());
    });
    var category = window.sessionStorage.getItem("selectCategory");
    if (category != null) {
        $(".category1 li").each(function () {
            if ($(this).text() == category) {
                $(this).click();
                return false;
            }
        });
    }
});


$('#searchDistribution').click(function () {
    var keywords = $('#searchBox').val();
    var vshopid = "@(ViewBag.VshopId)";
    if (vshopid.length < 1) {
        vshopid = QueryString('vshopId');
    }
    var jumpurl = '/' + areaName + '/DistributionMarket/SearchProduct?skey=' + encodeURIComponent(keywords);
    location.href = jumpurl;
});
