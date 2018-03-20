$('#searchBtn').click(function () {
    var keywords = $('#searchBox').val();
    var vshopid = QueryString("vshopid");
    if ($.trim(keywords) == "")
        $.dialog.tips("请输入搜索关键字");
    var jumpurl = '/' + areaName + '/search?keywords=' + encodeURIComponent(keywords);
    if (vshopid) {
        jumpurl += "&vshopid=" + vshopid;
    }
    location.href = jumpurl;
});