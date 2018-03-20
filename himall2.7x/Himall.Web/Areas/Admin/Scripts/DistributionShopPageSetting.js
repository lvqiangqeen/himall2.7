// JavaScript source code
$(function () {


    $('.page-tab-hd li').click(function () {
        var top = $(this).position().top;
        var h = $(this).height();

        $('.page-tab-bd').css('marginTop', top).show().children().eq($(this).index()).show().siblings().hide();
        $('.arrow').css('top', top + h / 2 - 5).show();

    });

});