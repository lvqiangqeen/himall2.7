$(function () {
    $(".categorys").hide();
    $(".nav-bar .nav-content").css({ "padding-left": "0", "width": "1190px" });
    $("#homePage>a").css("padding-left", "0");
    $(".nav").css("height", "48px");

    $("#pageJump_LMT").click(function () {
        var pageNo = parseInt($("#jumpInput").val());
        var pagecount = parseInt($("#pageCount").html());
        if (pageNo > pagecount || pageNo < 1) {
            //alert("请您输入有效的页码!");
            return;
        }
        if (isNaN(pageNo)) {
            pageNo = 1;
        }
        search_LMT(pageNo);
    });
    $("#ex-search").click(function () {
        search_LMT(1);
    });
    function search_LMT(pageNo) {
        var exp_keyWords = $("#searchVal").val();
        var orderType = getQueryString('orderType');
        var orderKey = getQueryString('orderKey');
        var isStart = getQueryString('isStart');
        var url = "./home?pageNo=" + pageNo.toString();
        if (exp_keyWords) {
            url += "&keywords=" + exp_keyWords;
        }
        if (orderType) {
            url += "&orderType=" + orderType;
        }
        if (orderKey) {
            url += "&orderKey=" + orderKey;
        }
        if (isStart) {
            url += "&isStart=" + isStart;
        }
        location.href = url;
    }
});