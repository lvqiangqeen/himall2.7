var IsExpired = $('#IsExpired').val(),
    IsFreeze = $('#IsFreeze').val();
//焦点图渐变切换
$(function () {
    var len = $(".shop-focus ul li").length;
    var index = 0;
    var picTimer;
    var btn = '<ol>';
    for (var i = 1; i <= len; i++) {
        btn += "<li>" + i + "</li>";
    }
    btn += "</ol>";
    $('.shop-focus').append(btn);
    $(".shop-focus ul li").eq(0).show();
    $(".shop-focus ol li").mouseover(function () {
        index = $(".shop-focus ol li").index(this);
        showPics(index);
    }).eq(0).trigger("mouseover");

    $(".shop-focus ul").hover(function () {
        clearInterval(picTimer);
    }, function () {
        picTimer = setInterval(function () {
            showPics(index);
            index++;
            if (index == len) { index = 0; }
        }, 5000);
    }).trigger("mouseleave");

    function showPics(index) {
        $(".shop-focus ul li").eq(index).fadeIn().siblings().fadeOut();
        $(".shop-focus ol li").removeClass("cur").eq(index).addClass("cur");
    }

    if (IsExpired == "True") {
        $.dialog.errorTips('该店铺已过期！', '', 5000);
    }

    if (IsFreeze == "True") {
        $.dialog.errorTips('该店铺已冻结！', '', 5000);
    }

    //图片滚动切换
    $('.shop-goods-slider').each(function () {
        var len = $(this).find('li').length,
            widthLi = $(this).width();

        var btn = '<div class="slide-controls">';
        for (var i = 0; i < len; i++) {
            btn += "<span>" + i + "</span>";
        }
        btn += "</div>";
        $(this).append(btn).find('ul').width(len * widthLi);
        $(this).find('span').first().addClass("cur");
        $(this).find('span').mouseDelay().mouseenter(function () {
            $(this).addClass("cur").siblings().removeClass().parent().siblings().stop(false, true).animate({ 'left': $(this).index() * (-widthLi) }, 400);
        });

    });
});