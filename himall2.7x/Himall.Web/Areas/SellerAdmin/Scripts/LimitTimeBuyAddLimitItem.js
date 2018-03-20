// JavaScript source code
$(function () {
    $("#Title").focus();
    $("#Title").val('限时折扣');
    $(".start_datetime").val($("#DNT1").val());

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
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
        //alert($(".start_datetime").val());
    });
    $('.end_datetime').on('changeDate', function () {
        $('.start_datetime').datetimepicker('setEndDate', $(".end_datetime").val());
    });


});
$(function () {
    $("#SelectProduct").click(function () {
        $.productSelector.show(null, function (selectedProducts) {
            console.log(selectedProducts);
            $("#ProductId").val(selectedProducts[0].id);
            $("#ProductName").val(selectedProducts[0].name);
            $("#ProductPrice").val(selectedProducts[0].price);
        }, 'selleradmin', false);
    });
    var a = v({
        form: 'form1',
        ajaxSubmit: true,
        beforeSubmit: function () {
            loadingobj = showLoading();
        },
        afterSubmit: function (d) {// 表单提交成功回调
            loadingobj.close();
            if (d.success) {
                $.dialog.succeedTips("保存成功！", function () {
                    window.location.href = $("#UAmag").val();
                }, 0.5);
            } else {
                $.dialog.errorTips(d.msg, null, 1);
            }
        }
    });
    a.add(
        {
            target: 'Title',
            empty: true,
            ruleType: 'required',// v.js规则验证
            error: '请您填写活动标题'
        },
        {
            target: 'ProductName',
            empty: true,
            ruleType: 'required',// v.js规则验证
            error: '请您选择商品'
        }, {
            target: 'Price',
            empty: true,
            ruleType: 'money',// v.js规则验证
            fnRule: function () {
                var a = $('#ProductPrice').val(),
                      b = $('#Price').val();
                try {
                    a = parseFloat(a);
                } catch (ex) {
                    a = 0;
                }
                try {
                    b = parseFloat(b);
                } catch (ex) {
                    b = 0;
                }
                if (b >= a || b < 0 || a < 0) {
                    return false;
                }
            },
            error: '只能为数字，  大于0且必须低于原价格'
        }, {
            target: 'StartTime',
            ruleType: 'required',// v.js规则验证
            error: '请选择活动开始时间'
        }, {
            target: 'EndTime',
            ruleType: 'required',// v.js规则验证
            error: '请选择活动结束时间'
        }, {
            target: 'MaxSaleCount',
            empty: true,
            ruleType: 'uint&&(value>0)',// v.js规则验证
            error: '最大购买数只能为数字，  且大于0'
        });
});
