// JavaScript source code
$(function () {
    $("#Title").focus();

    $("#SelectProduct").click(function () {
        $.productSelector.show(null, function (selectedProducts) {
            console.log(selectedProducts);
            $("#ProductId").val(selectedProducts[0].id);
            $("#ProductName").val(selectedProducts[0].name);
            $("#ProductPrice").val(selectedProducts[0].price);
        }, 'selleradmin', false);
    });
});
    var a = v({
    form: 'form1',
    ajaxSubmit: true,
    beforeSubmit: function () {
        loadingobj = showLoading();
    },
    afterSubmit: function (data) {// 表单提交成功回调
        loadingobj.close();
        var d = data;
        if (d.success) {
            $.dialog.succeedTips("保存成功！", function () {
                window.location.href = "../Management";
            });
        } else {
            $.dialog.errorTips(d.msg, '', 1);
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
        error: '只能为数字，  且大于0'
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