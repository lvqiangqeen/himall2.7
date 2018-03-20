
$(function () {
    RestorePrivileges();
    RestoreCheck();
    $('#RoleName').focus();
})
function RestorePrivileges() {

    var privileges = $("#hdPrivileges").val();
    var Jsonprivileges = $.parseJSON(privileges);
    if (Jsonprivileges.length>0)
    {
        if (Jsonprivileges[0].Privilege == 0) {
            $("input[type='checkbox']").attr("checked", true);
        }
        else {

            $(Jsonprivileges).each(function (index, item) {
                $("input[value='" + item.Privilege + "']").attr("checked", true)
            });
        }
    }
   
}

function RestoreCheck() {
    $(".parent-check").each(
function (index, item) {
    var p = $(item).next("p");
    if ($(p).find("input:checked").length == $(p).find("input").length) {
        $(item).find("input").attr("checked", true);
    };
});
}


function generateRoleInfo() {
    //角色对象
    var role = {
        RoleName: $('#RoleName').val(),
        RolePrivilegeInfo: []
    };
    var all = $("input[name='privilege']").length;
    var chkPrivileges = $("input[name='privilege']:checked");
    if (chkPrivileges.length == 0) {
        $.dialog.errorTips("请至少选择一个权限！");
        return;
    }
    else {
        if (chkPrivileges.length == all) {
            var PrivilegeInfo = {
                Privilege: null
            };
            PrivilegeInfo.Privilege = 0;
            role.RolePrivilegeInfo.push(PrivilegeInfo);
        }
        else {
            $(chkPrivileges).each(function (index, item) {
                var PrivilegeInfo = {
                    Privilege: null
                };
                PrivilegeInfo.Privilege = ($(item).val());
                role.RolePrivilegeInfo.push(PrivilegeInfo);
            });
        }
    }
    return role;
}

function submitRole() {
    var object;
    if ($('form').valid()) {
        try {
            object = generateRoleInfo();
            if (object == null) return;
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.post('./'+$("#ID").val(), { roleJson: objectString }, function (result) {
                if (result.success) {
                    $.dialog.tips('保存成功',function(){location.href = './../management';});
                }
                else
                    $.dialog.errorTips('保存失败！' + result.msg);
                loading.close();
            }, "json");
        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }
    }
}

