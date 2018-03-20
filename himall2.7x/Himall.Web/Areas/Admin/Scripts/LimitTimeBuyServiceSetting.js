// JavaScript source code
$(function () {
    $("#Price").focus();

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
                window.location.reload();
            }, 0.5);
        } else {
            $.dialog.errorTips(d.msg, '', 0.5);
        }
    }
});
a.add(
    //{
    //    target: 'ReviceDays',
    //    empty: true,
    //    ruleType: 'uint&&(value>0)&&(value<=5)',// v.js规则验证
    //    error: '只能为数字，且只能是0到5之间的整数!'
    //},
    {
        target: 'Price',
        empty: true,
        ruleType: 'money&&(value>=0)',// v.js规则验证
        error: '只能为数字，  且大于等于0!'
    });

});
