var star = '',
    uid = '862',
    cont = '';
$('.commstar').children().each(function (i, e) {
    var obj = $(e);
    obj.bind('click', function () {
        obj.siblings().removeClass('active');
        obj.addClass('active');
        var a = obj.attr('data');
        if (a != uid) {
            cont = '';
            star = '';
            uid = a;
        }
        star = obj.attr('data-t');
        obj.parent().attr('data', star);
    });
});
$('.id_cont_txt').each(function (i, e) {
    $(e).focus(function () {
        $(e).val('');
        $(e).removeClass('area01');
    }).blur(function () {
        var a = $(e).attr('data');
        if (a != uid) {
            cont = '';
            star = '';
            uid = a;
        }
        cont = $(e).val();
        if ($(e).val()) {
            //
        } else {
            $(e).addClass('area01');
            $(e).val('商品是否给力？快分享你的购买心得吧~');
        }
    });
});

$('.pj').each(function (i, e) {
    $(e).bind('click', function () {
        var id = $(e).attr('catethird');
        uid = id;
        if ($('#' + id).css('display') == 'block') {
            $('#' + id).slideUp(100);
        } else {
            $('#' + id).slideDown(100);
        }
    });
});