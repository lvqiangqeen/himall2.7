
var curPage = 1;       //当前页
var isMoreGet = true;  //是否还在数据
var isLoading = false;
var TmplCon = "";
var sort = 1;
var databox;
var loading;
var datas;   //数据变量  不定义的话underscore会识别不到。
//注意，<%=%>标签中%与=之间不能有空格

$(function () {

    TmplCon = $("#shoptmp").html();   //获取模板内容
    databox = $("#databox");
    h_loading.init($("#autoLoad"));

    getData();

    $(".datasort").click(function () {
        var _t = $(this);
        sort = _t.data("sort");
        initdata();
        getData();
        $(".datasort").removeClass("on");
        _t.addClass("on");
    });

    $('#databox').on('click', '.bt_agent', function () {
        var _t = $(this);
        if (!_t.hasClass("disabled")) {
            //可以代理
            var pid = _t.data("pid");
            $.post('/' + areaName + '/DistributionMarket/AgentProduct', { id: pid }, function (result) {
                if (result.success) {
                    _t.addClass("disabled").html("已代理");
                } else {
                    $.dialog.errorTips(result.msg);
                }
            });
        }
    });
});

function initdata() {
    isMoreGet = true;
    curPage = 1;
    databox.empty();
}

function getData() {
    if (isMoreGet) {
        if (!isLoading) {
            isLoading = true;
            h_loading.show();
            $.post('/' + areaName + '/DistributionMarket/ShopList', { skey: skey, page: curPage, sort: sort }, function (result) {
                isLoading = false;
                datas = result;
                if (datas.length > 0) {
                    databox.append(_.template(TmplCon, datas));
                    h_loading.hide();

                    /* 默认页面图片大小*/
                    $(".out-frame").width($('.container').width() * 93 / 100);
                    $(".out-frame").height($('.container').width() * 93 / 100);
                    $(".shop-frame").width($('.container').width() * 93 / 100);
                    $(".shop-frame").height($('.container').width() * 1 / 2);

                    $(".shop-pro>img").height($('.container').width() * 3 / 16);
                    $(".shop-pro>img").width($('.container').width() * 3 / 16);
                } else {
                    isMoreGet = false;
                    h_loading.nodata();
                }
            });
        }
    }
}
