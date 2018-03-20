// JavaScript source code
$(function () {
    AutoComplete();
    var a = v({
        form: 'form1',// 表单id 必须
        ajaxSubmit: true,// 是否ajax提交 如果没有这个参数那么就是默认提交方式 如果没有特殊情况建议默认提交方式
        beforeSubmit: function () {
            var id = $("#userName").attr("real-value");
            $("#userId").val(id);
            loadingobj = showLoading();
        },// 表单提交之前的回调 不是必须
        afterSubmit: function (data) {
            loadingobj.close();
            if (data.success) {
                $.dialog.succeedTips("保存成功！", function () {
                    window.location.href = "./Search";
                }, 0.5);
            } else {
                $.dialog.errorTips(data.msg, '', 0.5);
            }
        },// 表单提交之后的回调 不是必须

    });
    a.add(
    {
        target: "userName",
        ruleType: "required",
        error: '会员名必填填写!'
    },
    {
        target: "Integral",
        ruleType: "required&&uint&&(value>0)&&(value<=10000)",
        tips: '该信息为必填项，请输入积分数!',
        error: '积分数为正整数且小于一万!'
    }
    );
})
function AutoComplete() {
    //autocomplete
    $('#userName').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#userName').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}
