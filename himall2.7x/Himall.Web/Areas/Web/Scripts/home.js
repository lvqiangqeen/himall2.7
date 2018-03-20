$(function () {

    //左侧导航
    if ($(".floors").children().length > 0) {
        $(".floor-nav li").click(function () {
            var currentE = $(".floor").eq($(this).index());
            $("html,body").stop().animate({ scrollTop: currentE.offset().top - 1 }, 600);
        });

        $(window).scroll(function () {
            if ($(document).scrollTop() > ($('.floor').first().offset().top - $(window).height() / 2 - 100) && $(document).scrollTop() + $(window).height() < $(document).height() - 200) {
                $('.floor-nav').fadeIn()
            } else {
                $('.floor-nav').fadeOut()
            }

            $(".floor").each(function () {
                if ($(this).offset().top <= $(document).scrollTop() + $(window).height() / 2) {
                    $(".floor-nav li").delay(300).eq($(this).index()).addClass("cur").siblings().removeClass();
                    
                }
            })
        });
    }
    //tab选项卡切换
    var timeoutid;
    $(".floorA .floorA-hd ul li").each(function (index) {
        $(this).mouseover(function () {
            var t = $(this);
            timeoutid = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floorA .floorA-right .tab-right").eq(index).addClass("current").siblings().removeClass("current");
            }, 300);
        }).mouseout(function () {
            clearTimeout(timeoutid);
        });
    });

    var timeout_id;
    $(".floor-six-hd ul li").each(function (index) {
        $(this).mouseover(function () {
            var t = $(this);
            timeout_id = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floor-six .floor-six-right .tab-right").eq(index).addClass("current").siblings().removeClass("current");
            }, 300);
        }).mouseout(function () {
            clearTimeout(timeout_id);
        });
    });


    var timeout_id;
    $(".floor-seven-hd ul li").each(function (index) {
        $(this).mouseover(function () {
            var t = $(this);
            timeout_id = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floor-seven .fst-mid .tab-right").eq(index).addClass("current").siblings().removeClass("current");
                $(".floor-seven .floor-seven-bottom .tab-right").eq(index).addClass("current").siblings().removeClass("current");


            }, 300);
        }).mouseout(function () {
            clearTimeout(timeout_id);
        });
    });




   
})



