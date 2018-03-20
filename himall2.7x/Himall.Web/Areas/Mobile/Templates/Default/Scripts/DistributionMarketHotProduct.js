
isProductPage = true;
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
var allgddatas;        //数据变量，所有商品


$(document).ready(function () {

    $(window).scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        if (scrollTop + windowHeight >= scrollHeight - 30) {
            curPage++;
            getData();
        }
    });
})

function initdata() {
    isMoreGet = true;
    curPage = 1;
    databox.empty();
}

function ShowData(isclear) {
    isclear = isclear || false;
    if (showType != 2) showType = 1;
    TmplCon = $("#gdtmp1").html(); //获取模板内容
    var showbox = $("#proshowbox1");
    databox = $("#databox");
    $(".ProShowBox").hide();
    if (isclear) {
        databox.empty();
    }
    databox.append(_.template(TmplCon, gddatas));
    showbox.show();
}

function getData() {
    if (isMoreGet) {
        if (!isLoading) {
            isLoading = true;
            h_loading.show();
            var getdataurl = "/" + areaName + "/DistributionMarket/ProductList";
            $.post(getdataurl, { skey: skey, categoryid: categoryId, page: curPage, sort: sort }, function (result) {
                isLoading = false;
                gddatas = result;
                if (curPage == 1) {
                    allgddatas = gddatas;
                } else {
                    allgddatas = allgddatas.concat(gddatas);
                }
                if (gddatas.length > 0) {
                    ShowData();
                    h_loading.hide();
                } else {
                    isMoreGet = false;
                    h_loading.nodata();
                }
            });
        }
    }
}

$(function () {

    getData();

    $('#databox').on('click', '.bt_agent', function () {
        var _t = $(this);

        if (!_t.hasClass("disabled")) {
            //可以代理
            var pid = _t.data("pid");
            $.post('/' + areaName + '/DistributionMarket/AgentProduct', { id: pid }, function (result) {
                if (result.success) {
                    pid = parseInt(pid);

                    for (var item in allgddatas) {
                        var curdata = allgddatas[item];
                        if (curdata.ProductId == pid) {
                            curdata.isHasAgent = true;
                        }
                    }

                    _t.addClass("disabled").html("已代理");
                } else {
                    $.dialog.errorTips(result.msg);
                }
            });
        }
    });
    $("#stylechange").click(function () {
        showType++;
        gddatas = allgddatas;
        ShowData(true);
    });

    $(".hot-tab span").click(function () {
        sort = $(this).attr("dis");
        initdata();
        curPage = 1;
        getData();
    })
});
