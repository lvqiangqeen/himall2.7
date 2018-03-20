$(function () {
    loadProducts(1)
});

var page = 1;
var siteName=$("#siteName").val();

$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        $('#autoLoad').removeClass('hide');
        loadProducts(++page);
    }
});


function loadProducts(page) {
    var url = '/' + areaName + '/Member/GetUserCollectionShop';
    $.post(url, { pageNo: page, pageSize: 8 }, function (result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        if (result.length > 0) {
            $.each(result, function (i, item) {

                html+='<li><a class="p-img" href="/'+areaName+'/vshop/detail/'+item.Id+'"><img class="lazyload" src="'+item.Logo+'" width="100%" alt=""></a>';
                html+=' <h3><a>'+item.Name+'</a></h3>';
                html+=' <p class="red">'+siteName+'</p></li>';
            })
            $('.search-list').append(html);
            $('.search-list li').click(function () {
                window.location.href = $(this).find("a.p-img").attr("href");
            });
        }
        else {
            $('#autoLoad').html('没有更多店铺了').removeClass('hide');
        }
    });
}