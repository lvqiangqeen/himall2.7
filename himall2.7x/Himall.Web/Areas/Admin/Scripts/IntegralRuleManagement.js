// JavaScript source code
$(function () {
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
                $.dialog.succeedTips(data.msg);
            } else {
                $.dialog.errorTips(data.msg);
            }
            loadingobj.close();
        },// 表单提交之后的回调 不是必须
        ajaxSubmit: true// 是否ajax提交 如果没有这个参数那么就是默认提交方式 如果没有特殊情况建议默认提交方式
    });
    a.add(
    {
        target: "MoneyPerIntegral",
        ruleType: "required&&uint",
        tips: '必填',
        error: '错误'
    },
     {
         target: "Reg",
         ruleType: "required&&uint",
         tips: '必填',
         error: '错误'
     },
      {
          target: "BindWX",
          ruleType: "required&&uint",
          tips: '必填',
          error: '错误'
      },
       {
           target: "Login",
           ruleType: "required&&uint",
           tips: '必填',
           error: '错误'
       },
        {
            target: "Comment",
            ruleType: "required&&uint",
            tips: '必填',
            error: '错误'
        },
        {
            target: "Share",
            ruleType: "required&&uint",
            tips: '必填',
            error: '错误'
        }
    );
})