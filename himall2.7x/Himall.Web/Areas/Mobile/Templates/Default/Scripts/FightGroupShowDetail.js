
$(function () {
    function setSrc() {
        slidesImgs.each(function () {
            $(this).attr('src', $(this).data('src'));
        });
    }
    var slides = $('#slides');
    var slidesImgs = $('#slides img');
    slidesImgs.eq(0).attr('src', slidesImgs.eq(0).data('src'));
    // 焦点图滚动
    if (slides.children().length == 0) {
        slides.hide();
    }
    if (slides.children().length > 1) {
        slides.slidesjs({
            width: 640,
            height: 640,
            navigation: false,
            play: {
                active: false,
                auto: false,
                interval: 4000,
                swap: true
            },
            callback: {
                loaded: function (number) {
                    timmer = setTimeout(function () {
                        setSrc();
                    }, 5000);
                },
                complete: function (number) {
                    if (number == 2) {
                        setSrc();
                        clearTimeout(timmer);
                    }
                }
            }
        });
    } else {
        slides.css({ 'height': slides.width() })
    }
});
