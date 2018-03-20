$(function () {
    $('.page-tab-hd li').click(function () {
        var top = $(this).position().top;
        var h = $(this).height();
        var _ind = $(this).index();
        switch(_ind)
        {
            case 0:
                if (!isslideloaded) {
                    initSlideImagesTable();
                }
                break;
            case 1:
                if (!isrouletteloaded) {
                    initRouletteTable();
                }
                break;
            case 2:
                if (!isscratchcardloaded) {
                    initScratchCardTable();
                }
                break;

        }
        $('.page-tab-bd').css('marginTop', top).show().children().eq(_ind).show().siblings().hide();
        $('.arrow').css('top', top + h / 2 - 5).show();
    });
});