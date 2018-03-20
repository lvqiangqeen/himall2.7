// JavaScript source code

function checkdata() {
    if ($.trim($("#txtProductName").val()) == "") {
        $.dialog.errorTips("商品不能为空 ");
        return false;
    }
    if ($.trim($("#txtEndDate").val()) == "") {
        $.dialog.errorTips("结束时间不能为空 ");
        return false;
    }
    if ($.trim($("#txtCount").val()) == "") {
        $.dialog.errorTips("限制数量不能为空 ");
        return false;
    }

    var eachResult = true;
    var eachMsg = "";
    $("#tbl input[type=text]").each(function (i, item) {
        var p = $.trim($(item).val());
        if (p == "") {
            eachMsg = "价格不能为空 ";
            eachResult = false;
            $(this).focus();
            return;
        }

        if (isNaN(parseFloat(p))) {
            eachMsg = "价格只能为数字 ";
            eachResult = false;
            $(this).focus();
            return;
        }

        if (parseFloat(p) <= 0) {
            eachMsg = "价格不能小于0 ";
            eachResult = false;
            $(this).focus();
            return;
        }

        var oldP = $(item).parent().prev().html();
        if (parseFloat(p) > parseFloat(oldP)) {
            eachMsg = "价格不能大于原始价格 ";
            eachResult = false;
            $(this).focus();
            return;
        }

    })

    if (!eachResult) {
        $.dialog.errorTips(eachMsg);
        return false;
    }

    return true;
}

var p_data = null;
$(function () {
    initDate();

    $("#SelectProduct").click(function () {
        $.productSelector.show(null, function (selectedProducts) {
            console.log(selectedProducts);

            $.post("./IsAdd", { productId: selectedProducts[0].id }, function (result) {
                if (result) {
                    $("#txtProductId").val(selectedProducts[0].id);
                    $("#txtProductName").val(selectedProducts[0].name);
                    skuShow(selectedProducts[0].id);
                }
                else {
                    $.dialog.errorTips("此商品已参与限时购或其他活动");
                }
            })

        }, 'selleradmin', false);
    });

    $("#submit").click(function () {
        if ($("#txtCategoryName option").size() == 0) {
            $.dialog.errorTips("平台还未设置限时购活动分类");
            return;
        }
        var loading = showLoading();
        //$(this).attr("disabled", "");
        if (!checkdata()) {
            //$(this).removeAttr("disabled");
            loading.close();
            return;
        }
        p_data.BeginDate = $("#txtBeginDate").val();
        p_data.EndDate = $("#txtEndDate").val();
        p_data.CategoryName = $("#txtCategoryName").val();
        p_data.LimitCountOfThePeople = $("#txtCount").val();
        p_data.ProductId = $("#txtProductId").val();
        p_data.Title = $("#txtTitle").val();

        for (var i = 0; i < p_data.Details.length; i++) {
            var price = $("#tbl input:eq(" + i + ")").val();
            p_data.Details[i].Price = parseFloat(price);
        }

        $.post("./AddFS", { fsmodel: JSON.stringify(p_data) }, function (result) {
            if (result.success) {
                loading.close();
                $.dialog.succeedTips("保存成功！", function () {
                    window.location.href = $("#UAm").val();
                }, 0.5);
            }
            else {
                $.dialog.errorTips(result.msg);
                //$(this).removeAttr("disabled");
            }
            loading.close();
            //$(this).removeAttr("disabled");
        })
        console.log(p_data);
    })

})

function skuShow(productid) {
    $.post("./GetDetailInfo", { productId: productid }, function (result) {
        p_data = result;
        var html = "";
        for (var i = 0; i < result.Details.length; i++) {
            if (i == 0 && result.Details.length > 1) {
                html += "<tr><td rowspan='" + result.Details.length + "' style='text-align:center'><img src='" + result.ProductImg + "'/></td>";
                html += "<td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td></tr>";
            }
            else if (i == 0 && result.Details.length == 1) {
                html += "<tr><td style='text-align:center'><img src='" + result.ProductImg + "'/></td>";
                html += "<td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td></tr>";
            }
            else {
                html += "<tr><td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td></tr>";
            }
        }

        $("#tbl tbody").html(html);
    })
}

function initDate() {
    $(".start_datetime").val($("#DNT0").val());

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1,
        minView: 0
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1,
        minView: 0
    });
    $('.end_datetime').datetimepicker('setEndDate', $("#VBET").val());
    $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    $('.start_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    $('.start_datetime').datetimepicker('setEndDate', $("#VBET").val());


    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
    $('.end_datetime').on('changeDate', function () {
        $('.start_datetime').datetimepicker('setEndDate', $(".end_datetime").val());
    });
}