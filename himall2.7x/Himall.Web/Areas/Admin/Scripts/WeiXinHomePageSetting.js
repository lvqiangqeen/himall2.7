// JavaScript source code
$(function () {
    $(".btn-enableTemplate").click(function () {

        $.post('./UpdateCurrentTemplate', { tName: $(this).attr("dataid") }, function (result) {
            if (result.success) {
                $.dialog.succeedTips(result.msg, function () { location.href = "./VHomepage"; });
            }
            else
                $.dialog.errorTips(result.msg);
            loading.close();
        }, "json");
    });

});