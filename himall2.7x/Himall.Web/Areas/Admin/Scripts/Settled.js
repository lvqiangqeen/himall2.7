
//保存
$("#Save").on("click", function () {
    var BusinessType = $("input[name='BusinessType']:checked").val();

    if (isNaN($("#TrialDays").val())) {
        $.dialog.tips("试用时间必须为数字！");
        return false;
    }
    $.post("setSettled", {
        ID: $("#ID").val(),
        BusinessType: $("input[name='BusinessType']:checked").val(),
        SettlementAccountType: $("input[name='SettlementAccountType']:checked").val(),
        TrialDays: $("#TrialDays").val(),
        IsCity: $("input[name='IsCity']:checked").val(),
        IsPeopleNumber: $("input[name='IsPeopleNumber']:checked").val(),
        IsAddress: $("input[name='IsAddress']:checked").val(),
        IsBusinessLicenseCode: $("input[name='IsBusinessLicenseCode']:checked").val(),
        IsBusinessScope: $("input[name='IsBusinessScope']:checked").val(),
        IsBusinessLicense: $("input[name='IsBusinessLicense']:checked").val(),
        IsAgencyCode: $("input[name='IsAgencyCode']:checked").val(),
        IsAgencyCodeLicense: $("input[name='IsAgencyCodeLicense']:checked").val(),
        IsTaxpayerToProve: $("input[name='IsTaxpayerToProve']:checked").val(),
        CompanyVerificationType: $("input[name='CompanyVerificationType']:checked").val(),
        IsSName: $("input[name='IsSName']:checked").val(),
        IsSCity: $("input[name='IsSCity']:checked").val(),
        IsSAddress: $("input[name='IsSAddress']:checked").val(),
        IsSIDCard: $("input[name='IsSIDCard']:checked").val(),
        IsSIdCardUrl: $("input[name='IsSIdCardUrl']:checked").val(),
        SelfVerificationType: $("input[name='SelfVerificationType']:checked").val()
    }, function (data) {
        $.dialog.tips(data.msg);
    }, "json");
})