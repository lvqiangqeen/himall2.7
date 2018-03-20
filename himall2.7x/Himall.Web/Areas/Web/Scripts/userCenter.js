$(function () {
    //下拉菜单
    $('.top .dropdown').hover(function () {
        $(this).toggleClass('hover');
    });

    //搜索切换
    $('.search-form label').click(function () {
        $(this).siblings('ul').show();
    });
    $('.search-form ul li').click(function () {
        $(this).parent().hide().siblings('label').text($(this).text());
    });


    $('#searchBtn').click(function () {
        var keyWords = $.trim($('#searchBox').val());

        var selected = $(".search .search-form label").html();
        if (selected == "店铺") {
            //if (keyWords == '') {
            //    $.dialog.errorTips('请输入店铺搜索关键字！');
            //    return;
            //}
            location.href = "/shopsearch?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
        }
        else {
            location.href = "/search/searchAd?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
        }

    });

    $("#searchBox").keydown(function (e) {
        if (e.keyCode == 13) {
            var keyWords = $('#searchBox').val();
            location.href = "/search/searchAd?keywords=" + encodeURIComponent(keyWords ? keyWords : $('#searchBox').attr('placeholder'))
        }
    });

    $(".aside-list .aside-clk").click(function () {
        $(this).addClass("aside-active").siblings().removeClass("aside-active");
    })
});