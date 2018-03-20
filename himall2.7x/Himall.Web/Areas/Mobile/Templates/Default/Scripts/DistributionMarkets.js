$(function () {
    $('.j-swipe').each(function (index, el) {
        var me = $(this);
        me.attr('id', 'mySwipe1' + index);
        var id = me.attr('id');
        // alert(id)
        var elem = document.getElementById(id);
        window.mySwipe = Swipe(elem, {
            startSlide: 0,
            auto: 3000,
            callback: function (m) {
                $(elem).find('.members_flash_time').children('span').eq(m).addClass('cur').siblings().removeClass('cur')
            },
        });
    });


    $('.detail-bottom').on('click', '.bt_agent', function () {
        var _t = $(this);
        if (!_t.hasClass("disabled")) {
            //可以代理
            var pid = _t.data("pid");
            $.post('/' + areaName + '/DistributionMarket/AgentProduct', { id: pid }, function (result) {
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
})