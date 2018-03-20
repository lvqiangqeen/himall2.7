; (function ($) {
    
    $.extend({
        alert: function (obj) {
            var obj = $.extend({// 默认参数
                width: 410,
                height: 140,
                state: 1,
                msg: '关注成功！',
                title: '提示标题',
                success: function (close) {
                },
                close: function () {
                }
            }, obj),
				close = function (fn) {
				    $('.alert_close').bind('click', function () {
				        var i = 0;
				        $('#alert_bg,.alert_box').hide(
							function () {
							    i += 1;
							    i === 2 ? !fn() : '';
							}
						);
				    });
				};
            $('body').append('<div class="alpha" id="alert_bg"></div><div class="alert_box"><div class="alert_warp"><div class="alert_title"><span>提示</span></div><div class="alert_con"><div class="alert_tips"></div></div><div class="alert_close">×</div></div></div>');
            switch (obj.state) {
                case 1: $('.alert_tips').html('<h2 class="alert_state1">' + obj.msg + '</h2>'); break;
                case 2: $('.alert_tips').html('<h2 class="alert_state2">' + obj.msg + '</h2>'); break;
                case 3: $('.alert_tips').html('<h2 class="alert_state3">' + obj.msg + '</h2>'); break;
                default: break;
            }

            obj.success.call($('.alert_tips'));
            close(obj.close);
            $('#alert_bg').show();
            $('.alert_title').html(obj.title);
            $('.alert_warp').css({
                width: obj.width,
                height: obj.height
            });
            $('.alert_box').show().css({
                marginTop: '-' + ($('.alert_box').height() / 2) + 'px',
                marginLeft: '-' + ($('.alert_box').width() / 2) + 'px',
                position: 'fixed'
            });
        }
    });
}(jQuery));