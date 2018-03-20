
$(function () {
    /*$("body").append("<div class=\"share-guide\"></div>");*/
    var shareguide = $(".share-guide");
    if (shareguide.length < 1)
    {
        $("body").append("<div class=\"share-guide\"></div>");
    }
    $(".share-guide").click(function () {
        var _t = $(this);
        _t.hide();
    });
    $('#databox').on('click', '.bt_share', function () {
        var $this = this;
        shareProduct($this,true);
    });
});
function shareProduct(obj,show) {
    var ShareTitle = '';
    var ShareDesc = '';
    var ShareIcon = '';
    var ProductId = 0;
    var _t = $(obj);
    var pid = _t.data("pid");
    var uid = _t.data("uid");
    var _url = ShareLink;
    _url = _url.replace("%23id%23", pid);
    _url = _url.replace("%23uid%23", uid);
    var newlink = 'http://' + window.location.host + _url;
    ProductId = pid;
    if (defaultShareTitle == '')
        ShareTitle = _t.data("title");
    else
        ShareTitle = defaultShareTitle;
    if (defaultShareDesc == '')
        ShareDesc = _t.data("title");
    else
        ShareDesc = defaultShareDesc;
    if (defaultShareIcon == '')
        ShareIcon = _t.data("img");
        //ShareIcon = 'http://' + window.location.host + _t.data("img");
    else
        ShareIcon = defaultShareIcon;
    //ShareIcon = 'http://' + window.location.host + defaultShareIcon;
    console.log("ShareIcon:" + ShareIcon);
    console.log("ShareTitle:" + ShareTitle);
    console.log("newlink:" + newlink);
    console.log("ShareIcon:" + ShareIcon);
    console.log("ProductId:" + ProductId);
    if (show)
    {
        $(".share-guide").show();//WeiXin(title,desc,link,icon,productId)
    }
    if (IsWeiXin) {
        WeiXin(ShareTitle, ShareDesc, newlink, ShareIcon, ProductId);
    }
    else
        //console.log(ShareIcon)
        console.log("只支持微信");
}
