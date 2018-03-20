$(function () {
    //左侧导航
    var S_width = $(window).width();
    $(".topic-nav li").click(function () {
        var currentE = $(".topic .topic-goods-Z").eq($(this).index());
        $("html,body").stop().animate({ scrollTop: currentE.offset().top + 20 }, 500);
    });
    if ($('.topic-goods-Z').length > 0) {
        $(window).scroll(function () {
            if ($(document).scrollTop() > ($('.topic-goods-Z').first().offset().top - 60) && S_width >= 1340) {
                $('.topic-nav').show()
            } else {
                $('.topic-nav').hide()
            }

            $(".topic .w .topic-goods-Z").each(function () {
                if ($(this).offset().top <= $(document).scrollTop() + 100) {
                    $(".topic-nav li").eq($(this).index()).addClass("cur").siblings().removeClass();
                }
            })
        });
    }

});

$(function () {
    $(".topic .topic-goods-Z li").hover(function () {
        $(this).children(".p-name").css("visibility", "visible");

    }, function () {
        $(this).children(".p-name").css("visibility", "hidden");

    })
})