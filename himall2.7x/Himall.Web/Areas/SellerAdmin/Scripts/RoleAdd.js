

function generateRoleInfo() {
    //角色对象
    var role = {
        RoleName: $('#RoleName').val(),
        RolePrivilegeInfo: []
    };
    var all = $("input[name='privilege']").length;
    var chkPrivileges = $("input[name='privilege']:checked");
    if (chkPrivileges.length == 0) {
        $.dialog.tips("请至少选择一个权限！");
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
            $.post('./add', { roleJson: objectString }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存成功',function(){location.href = './management';});
                }
                else
                    $.dialog.tips('保存失败！' + result.msg);
            }, "json");
        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }
    }
}

