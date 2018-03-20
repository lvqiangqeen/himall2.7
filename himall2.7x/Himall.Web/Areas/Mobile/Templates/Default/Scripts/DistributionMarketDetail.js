/* 代理 */
$('.bt_agent').on('click', function() {
    var _t = $(this);
    if (!_t.hasClass("disabled")) {
        // 可以代理
        var pid = _t.data("pid");
        var agenturl = "/" + areaName + "/DistributionMarket/AgentProduct";
        $.post(agenturl, { id: pid }, function (result) {
            if (result.success) {
                _t.addClass("disabled").html("已代理");
            } else {
                alert(result.msg);
            }
        });
    } else {
        alert("您已代理此商品！");
    }
});

// 轮播图
function setScrollAttr() {
    var DocW = $('#slides').width();
    $('.detail-bd').css('marginTop', DocW);
}
setScrollAttr();
$(window).resize(setScrollAttr);

$(function() {
    function setSrc() {
        slidesImgs.each(function() {
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
                loaded: function(number) {
                    timmer = setTimeout(function() {
                        setSrc();
                    }, 5000);
                },
                complete: function(number) {
                    if ( number == 2 ) {
                        setSrc();
                        clearTimeout(timmer);
                    }     
                }
            }
        });
    } else {
        slides.css({ 'height': slides.width() })
    }
    
    // 详情延时加载
    var flag;
    $(window).scroll(function() {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        if (flag == 1) {
            return false;
        }
        if (scrollTop + windowHeight >= scrollHeight) {
            loadProductImg();
            flag = 1;
        }
    });
    
    function gotoProductImg() {
        if (flag == 1) {
            return false;
        }
        else {
            loadProductImg();
            flag = 1;
        }
    }
    
    function loadProductImg() {
        $(".goods-img").append('<h4><a name="top">商品图文详情</a></h4>' + $("#proDesc").val());
    }
    
});