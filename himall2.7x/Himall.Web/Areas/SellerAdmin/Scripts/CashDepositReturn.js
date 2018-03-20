// JavaScript source code
$(function () {
    $('.progress-').hide();
})

var secs = 5; //倒计时的秒数
var URL;
function Load(url) {
    URL = url;
    for (var i = secs; i >= 0; i--) {
        window.setTimeout('doUpdate(' + i + ')', (secs - i) * 1000);
    }
}

function doUpdate(num) {
    document.getElementById("ShowDiv").innerHTML = '将在<strong><font color=red> ' + num + ' </font></strong>'+$("#HRhp").val()+'，请稍候...';
    if (num == 0) { window.location = URL; }
}
$(function () {
    Load("/selleradmin?url=/selleradmin/CashDeposit/Management&tar=CashDeposit");
})
