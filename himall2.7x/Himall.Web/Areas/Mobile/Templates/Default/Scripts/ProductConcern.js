$(function () {
    loadProducts(1)
});

var page = 1;

$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        $('#autoLoad').removeClass('hide');
        loadProducts(++page);
    }
});


function loadProducts(page) { //后台图片已处理
    var url = '/' + areaName + '/Member/GetUserCollectionProduct';
    $.post(url, { pageNo: page, pageSize: 8 }, function (result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        if (result.length > 0) {
            $.each(result, function (i, item) {

                html += '<li><a class="p-img" href="/'+areaName+'/product/detail/'+item.Id+'"><img class="lazyload" src="' + item.Image + '" width="100%" alt=""></a>';
                html += ' <h3><a>' + item.ProductName + '</a></h3>';
                html += ' <p class="red">￥' + item.SalePrice + '</p><p>' + item.Evaluation + '人评价</p></li>';
            })
            $('.search-list').append(html);
        }
        else {
            $('#autoLoad').html('没有更多商品了').removeClass('hide');
        }
    });
}