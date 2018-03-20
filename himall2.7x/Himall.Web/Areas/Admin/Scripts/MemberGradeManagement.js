
var a = v({
    form: 'v-form',// 表单id 必须
    beforeSubmit: function () {
        if ($("div.tip-error span").length === 0) {
            loadingobj = showLoading();
        }
    },// 表单提交之前的回调 不是必须
    afterSubmit: function (data) {
        if (data.success) {
           // a.reset();
            $.dialog.succeedTips(data.msg,function(){window.location.href = Management;});
            
        } else {
            $.dialog.errorTips(data.msg);
        }
        loadingobj.close();
    },// 表单提交之后的回调 不是必须
    ajaxSubmit: true// 是否ajax提交 如果没有这个参数那么就是默认提交方式 如果没有特殊情况建议默认提交方式
});
a.add(
{
    target: "GradeName",
    ruleType: "required",
    error: '等级名称必填填写!'
},
{
    target: "Integral",
    ruleType: "required&&uint",
    tips: '该信息为必填项，请输入所需积分数!',
    error: '所需积分数为正整数且必填填写!'
},
{
    target: "Discount",
    ruleType: "required&&mallnumber",
    tips: '必填项',
    error: '错误'
}
);
function DeleteGrade(t, id, iscan) {
    $.dialog.confirm('确认删除该会员等级吗？', function() {
        if (iscan == 1) {
            var loading = showLoading();
            $.post("./Delete/" + id, function (data) {
                $.dialog.succeedTips(data.msg);
                $(t).parents("tr").remove();
                loading.close();
            });
        } else {
            $.dialog.errorTips("有礼品兑换与等级关联，不可删除！");
        }
    })
}