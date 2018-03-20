/**
 * Created by Administrator on 2015/11/16 0016.
 */
var categoryId = 0;
var sort = 0;
var showType = 1;      // 显示形式 
var curPage = 1;       // 当前页
var isMoreGet = true;  // 是否还在数据
var TmplCon = "";
var skey = "";
var isLoading = false;
var loading;
var databox;
var gddatas;           // 数据变量  不定义的话underscore会识别不到。
var allgddatas=[];        //数据变量，所有商品

$(document).ready(function() {
    $("#bt_classshow").click(function(event) {
        $("#classmenubox").toggle();
        event.stopPropagation();
    });
    $("#classmenubox a").click(function() {
        var _t = $(this);
        categoryId = _t.data("cid");
        initdata();
        getData();
        $("#classmenubox a").removeClass("active");
        _t.addClass("active");
    });
    $(document).click(function() {
        $("#classmenubox").hide();
    });

    $(".datasort").click(function() {
        var _t = $(this);
        sort = _t.data("sort");
        initdata();
        getData();
        $(".nav-price i").removeClass("on");
        $(".datasort").removeClass("on");
        _t.addClass("on");
    });

    $(".nav-price").click(function() {
        var sortdom = $(".nav-price i");
        sortdom.removeClass("on");
        $(".datasort").removeClass("on");
        if (sort == 4) {
            sortdom.eq(1).addClass("on");
            sort = 5;
        } else {
            sort = 4;
            sortdom.eq(0).addClass("on");
        }
        initdata();
        getData();
    });

    $(window).scroll(function() {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        if (scrollTop + windowHeight >= scrollHeight - 30) {
            curPage++;
            getData();
        }
    });
})

function getData() {
    if (isMoreGet) {
        if (!isLoading) {
            isLoading = true;
            h_loading.show();
            var getdataurl = "/" + areaName + "/DistributionMarket/ProductList";
            $.post(getdataurl, { skey: skey, categoryid: categoryId, page: curPage, sort: sort }, function(result) {
                isLoading = false;
                gddatas = result;
                if (curPage == 1) {
                    allgddatas = gddatas;
                } else {
                    allgddatas = allgddatas.concat(gddatas);
                }
                if (gddatas.length > 0) {
                    ShowData();
                    if (gddatas.length < 5) {
                        isMoreGet = false;
                        h_loading.nodata();
                    } else {
                        h_loading.hide();
                    }
                } else {
                    isMoreGet = false;
                    h_loading.nodata();
                }
            });
        }
    }
}

function showProStyle2() {
    /* 样式二图片大小 */
    $(".top-img").width($(".container").width() * 159 / 320);
    $(".top-img").height($(".container").width() * 159 / 320);

    /* 样式二动画 */
    $(".right-show").each(function() {
        $(this).click(function() {
            var _t = $(this);
            var _p = _t.parents("li.proitem");
            var _anbox = $(".animate-info", _p);
            _anbox.animate({ 'left': 0 }, 200);
            _anbox.css("z-index", 1);
            $(".top-img", _p).click(function() {
                _anbox.animate({ 'left': '101%' }, 200);
                _anbox.css("z-index", -1);
            })
        });
    });
}

function showProStyle1() {
    /* 样式一图片大小 */
    $(".toggle01-wrap").height($(".container").width() * 23 / 64);
    $(".detail-left").width($(".container").width() * 47 / 160);
    $(".detail-left").height($(".container").width() * 47 / 160);
    $(".detail-right").width($(".container").width() * 113 / 160 - 33);
    $(".detail-right").height($(".container").width() * 23 / 64 - 22);
}

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
