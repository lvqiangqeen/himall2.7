/**
 * Created by Administrator on 2015/11/16 0016.
 */
var isProductPage=false, isShopPage=false;

$(document).ready(function () {
    //模拟下拉
    $(".tab-nav .litxt").click(function (e) {
        var aim_txt = $(e.target).text();
        var toggle_txt = $('.toggle-target').text();
        var aim_id = $(e.target).attr('id');
        $('.toggle-target').text(aim_txt).attr("id", aim_id);
        $('.tab-nav .litxt').text(toggle_txt);
        $('.tab-nav .sptxt').text(aim_txt);
        if($('.tab-nav .litxt').text()=="店铺"){
            $('.tab-nav .litxt').removeClass("icon2").addClass("icon1")
        }else{
            $('.tab-nav .litxt').removeClass("icon1").addClass("icon2")
        }
        if($('.tab-nav .sptxt').text()=="店铺"){
            $('.tab-nav .sptxt').removeClass("icon2").addClass("icon1")
        }else{
            $('.tab-nav .sptxt').removeClass("icon1").addClass("icon2")
        }
        return false;
    });

    $("body").click(function (event) {
        $(".tab-nav").hide();
    });

    /*历史记录弹框大小*/
    $(".Mar-history").width($('.container').width()*93/100);

   /* 搜索框焦点*/
    $(".Mar-search").focus(function(){
        $(this).val("");
        $(".Mar-sbtn").html("取消");
        $(".Mar-default").hide();
        $(".Mar-history h5").show();
        if($(".Mar-history p").length<1){
            $(".Mar-history h5").hide();
        }
        $(".tab-nav").hide();
        $(".Mar-history").show();
        $(".shop-result").hide();
        $(".ProResult-style").hide();
        $(".style-toggle").hide();
    });

    /*商品、店铺切换*/
    $(".tab-text").unbind("click").click(function(){
        $(".tab-nav").toggle();
        return false;
    });

    $(".Mar-history").on("click", "p", function () {
        var _t = $(this);
        var tmpkey = _t.html();
        if(tmpkey.length>0)
        {
            $("#skey").val(tmpkey);
            $(".Mar-sbtn").click();
        }
    });

    $(".Mar-sbtn").click(function () {
        var _t = $(this);
        if (_t.html() == "取消") {
            $(".Mar-history").hide();
            $(".Mar-default").show();
            $(".shop-result").show();
            $(".ProResult-style").show();
            $(".style-toggle").show();
            _t.html("搜索");
            search();
        } else {
            search();
        };
        $(".tab-nav").hide();

    });

    function search()
    {
        $(".tab-nav").css("display", "none");
        var skey = $("#skey").val();
        if (skey.length > 0) {
            skey = encodeURIComponent(skey);
        }
        var isjump = true;
        if ($(".toggle-target").text() == "商品") {
            SearchUrl = "/" + areaName + "/DistributionMarket/SearchProduct/";
            SearchUrl += "?skey=" + skey;
        } else {
            SearchUrl = "/" + areaName + "/DistributionMarket/SearchShops/";
            SearchUrl += "?skey=" + skey;
        }
        window.location.href = SearchUrl;
    }

    /*执行搜索*/
    $("#skey").keypress(function (event) {
        if (event.keyCode == 13) {
            search();
        }
    });

})