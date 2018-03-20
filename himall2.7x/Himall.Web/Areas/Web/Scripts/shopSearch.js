$(function(){

    $('.search .search-form label').html("店铺");
    $('.search .search-form #searchBox').val($('#shopName').val());

    $('.shop-cate-option p').each(function() {
        if($(this).children().length<=6)
            $(this).find('.more').hide();
    });
    $('.shop-cate-option .more').click(function() {
        if($(this).parent().hasClass('min'))
            $(this).text('MORE −').parent().removeClass('min');
        else
            $(this).text('MORE +').parent().addClass('min');
    } );

    var cid = $('#searchCid').val();
    var bid = $('#searchBid').val();

    if(cid > 0)
    {
        $("#categoryDiv p em").text('MORE −').parent().removeClass('min');
    }
    if(bid > 0)
    {
        $("#brandDiv p em").text('MORE −').parent().removeClass('min');
    }
        
    // 店铺LOGO显示默认图片 //备用
    /*
    var oShopLogo = $('.J_shop_logo');
    if ( !oShopLogo.css('src') == true) {
        oShopLogo.attr('src', '');
    }
    */
        
})