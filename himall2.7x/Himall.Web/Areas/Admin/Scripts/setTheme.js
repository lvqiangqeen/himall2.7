
//主题类别切换
$("#themeType1").on("change", function () {
    $(".wh100").attr("readonly", "readonly");
    $(".wh100").css("background", "#EBEBE4");
})

$("#themeType2").on("change", function () {
    $(".wh100").removeAttr("readonly");
    $(".wh100").css("background", "#fff");
})

$(".wh100").on("change", function () {
    $(this).parent().parent().find(".divBox").css("background", $(this).val());
})

//保存
$("#btn_save").on("click", function () {
    var typeId = 0;
    if ($("#themeType2").is(':checked')) {
        typeId = 1;
    }
    if (typeId == 1) {
        if ($("#MainColor").val() == "" || $("#MainColor").val().length > 40) {
            $.dialog.tips("请填写正确的商城主色！");
            $("#MainColor").focus();
            return false;
        }
        if ($("#SecondaryColor").val() == "" || $("#SecondaryColor").val().length > 40) {
            $.dialog.tips("请填写正确的商城辅色！");
            $("#SecondaryColor").focus();
            return false;
        }
        if ($("#WritingColor").val() == "" || $("#WritingColor").val().length > 40) {
            $.dialog.tips("请填写正确的字体效果！");
            $("#WritingColor").focus();
            return false;
        }
        if ($("#FrameColor").val() == "" || $("#FrameColor").val().length > 40) {
            $.dialog.tips("请填写正确的侧边栏颜色！");
            $("#FrameColor").focus();
            return false;
        }
        if ($("#ClassifiedsColor").val() == "" || $("#ClassifiedsColor").val().length > 40) {
            $.dialog.tips("请填写正确的商品分类栏颜色！");
            $("#ClassifiedsColor").focus();
            return false;
        }
    }
    $.post("updateTheme", { id: $("#themeid").val(), typeId: typeId, MainColor: $("#MainColor").val(), SecondaryColor: $("#SecondaryColor").val(), WritingColor: $("#WritingColor").val(), FrameColor: $("#FrameColor").val(), ClassifiedsColor: $("#ClassifiedsColor").val() }, function (data) {
        if (data.status > 0) {
            $.dialog.tips("保存成功！");
        } else {
            $.dialog.tips("保存失败！");
        }
    }, "json");
})