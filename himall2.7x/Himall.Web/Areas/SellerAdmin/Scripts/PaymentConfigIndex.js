// JavaScript source code
$(function () {
    init();
})

function init() {
    $("#btnSave").click(function () {
        var loading = showLoading();
        var str = "";
        var cityids = "";
        $(".show-region-real").each(function () {
            if ($(this).prop("checked")) {
                str += "'" + $(this)[0].id + "',";
            }
        })
        str = str.substr(0, str.length - 1);

        $(".show-region-ckb").each(function () {
            if ($(this).prop("checked")) {
                cityids += "'" + $(this)[0].id + "',";
            }
        })
        cityids = cityids.substr(0, cityids.length - 1);
        $.post("Save", { addressIds: str, addressIds_city: cityids }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips(result.msg);
            }
        })
    })

    $(".sc1").click(function () {
        $(".hidereginDiv").hide();
        $(this).parent().parent().find(" ul").hide();
        $(this).parent().parent().find(".hidereginDiv").show();
    })

    $(".show-region").click(function () {
        $(this).parent().parent().find(".hidereginDiv ul").hide();
        $(this).parent().find("ul").show();
    })

    $(".show-region-ckb").change(function () {
        if ($(this).prop("checked")) {
            $(this).parent().find("li input[type=checkbox]").attr("checked", 'true');
        }
        else {
            $(this).parent().find("li input[type=checkbox]").removeAttr("checked");
        }
    })

    $(".show-region-real").change(function () {
        if ($(this).prop("checked")) {
            var selectedLen = 0;

            $(this).parent().parent().find("li input[type=checkbox]").each(function () {
                if ($(this).prop("checked")) {
                    selectedLen += 1;
                }
            })

            if ($(this).parent().parent().find("li").length == selectedLen) {
                $(this).parent().parent().parent().find(".show-region-ckb").attr("checked", 'true');
            }
        }
        else {
            $(this).parent().parent().parent().find(".show-region-ckb").removeAttr("checked");
        }
    })

    $(".region-select span").click(function () {
        $(this).parent().hide();
    })
}