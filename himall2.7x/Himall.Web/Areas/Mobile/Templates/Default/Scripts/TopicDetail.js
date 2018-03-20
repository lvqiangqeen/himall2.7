function autoSizeImage() {
    // 高度控制
    var w = $('.container').width();
    $('.recom-topic li').height(w * 15 / 32);
    $('.goods-list li').height(w / 16 * 11);
    $('.goods-list .p-img').height($('.p-img').width() * 5 / 8);

    square($('.p-img'));
}
var page = 0;
var topicId = $('#topicId').val();

function loadProducts(page) {
    $('#autoLoad').html('').show();
    var url = getAreaPath() + '/Topic/LoadProducts';
    var moduleId = $('div[name="module"].cur').attr('module');
    $.post(url, { topicId: topicId, moduleId: moduleId, page: page, pageSize: 8 }, function(result) {
        $('#autoLoad').hide();
        if (result.length > 0) {
            var html = '';
            $.each(result, function(i, product) {
                html += ' <li>\
                <a class="p-img" href="/'+ areaName + '/product/detail/' + product.id + '"><img src="' + product.image + '" alt=""></a>\
                <i>' + (((product.price / product.marketPrice) * 10).toFixed("1")) + '折</i>\
                <h3><a>' + product.name + '</a></h3>\
                <p><span>￥' + product.price.toFixed("2") + '</span><s>￥' + product.marketPrice.toFixed("2") + '</s></p>\
                </li>';
            });
            $('#productList').append(html);
            autoSizeImage();
        }
        else
            $('#autoLoad').html('没有更多商品了');
    });
}

function selectTab(moduleId) {
    $('div[module]').removeClass('cur');
    $('div[module="' + moduleId + '"]').addClass('cur');
    page = 1;
    $('#productList').html('');
    loadProducts(page);
}

$(function() {
    $('.fixed-box').height($('.fixed-box').height());
    $('.tab-hd').width($('.tab-hd').width());

    $(window).scroll(function() {
        var _scroll = $(window).scrollTop();
        var top = $('.fixed-box').offset().top;
        if (_scroll >= top) {
            $('.tab-hd').addClass('fixed')
        } else {
            $('.tab-hd').removeClass('fixed')
        }

        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();

        if (scrollTop + windowHeight >= scrollHeight) {
            loadProducts(++page);
        }

    });

    //$('a[name="moduleTab"]').click(function() {
    //    selectTab($(this).attr('moduleId'));
    //});

    //$('a[name="moduleTab"]').first().click();
    $('a[name="moduleTab"]').click(function () {
        location.href = $('#topicUrl').val() + "?module=" + $(this).attr('moduleId');
    });
    var curModule = QueryString('module');
    if (curModule == '') {
        curModule = $('a[name="moduleTab"]').first().attr('moduleid');
    }
    selectTab(curModule);
});