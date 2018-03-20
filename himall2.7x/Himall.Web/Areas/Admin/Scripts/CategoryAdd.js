// JavaScript source code
window.onload = function () {
    $("#pCategory option").each(function () {
        $(this).html($(this).text());
    });
}
$(function () {

    $("#subCate").click(function () {

        setTimeout(function () {
            if ($("span.field-validation-error span").length === 0) {
                showLoading();
                return true;
            }
        }, 200);

    });

    $("#Name").focus();
    var depthDOM;
    if ($("#depthHidden").val() != 2) {
        $("#CommisRate").val(100);
        $("#Depth").hide();
    }
    $("#upload-img").himallUpload();

    $("#pCategory").change(function () {
        var _t = $(this);
        if (_t.val() > 0) {
            var loading = showLoading();
            $.ajax({
                type: 'GET',
                url: './GetCateDepth',
                cache: false,
                data: { 'id': $(this).val() },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.successful && data.depth == 2) {
                        $("#CommisRate").val('');
                        $("#Depth").show();
                    } else {
                        $("#CommisRate").val(100);
                        $("#Depth").hide();
                    }
                },
                error: function () {
                    loading.close();
                    $("#CommisRate").val(100);
                    $("#Depth").hide();
                }
            });
        } else {
            $("#CommisRate").val(100);
            $("#Depth").hide();
        }
    });
    
    if ($("#vb-msg").val() != undefined && $("#vb-msg").val() != "") {
        $.dialog.errorTips($("#vb-msg").val());
    }
});