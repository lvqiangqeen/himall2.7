
$(function () {
    query();

    //添加管理员
    $('.add-manager').click(function () {
        LoadAddBox();
    });
})
var pwdreg = /^[^\s]{6,20}$/;
function Delete(id) {
    $.dialog.confirm('确定删除该条记录吗？', function () {
        var loading = showLoading();
        $.post("./Delete", { id: id }, function (data) {
            loading.close();
            $.dialog.tips(data.msg); query()
        });
    });
}
function BatchDelete() {
    var selectedRows = $("#list").hiMallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].Id);
    }
    if (selectedRows.length == 0) {
        $.dialog.tips("你没有选择任何选项！");
    }
    else {
        $.dialog.confirm('确定删除选择的管理员吗？', function () {
            var loading = showLoading();
            $.post("./BatchDelete", { ids: selectids.join(',') }, function (data) {
                loading.close();
                $.dialog.tips(data.msg); query()
            });
        });
    }
}

function query() {
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: {},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { checkbox: true, width: 39 },
            { field: "Id", hidden: true },
            { field: "UserName", title: '管理员' },
            { field: "CreateDate", title: '创建日期' },
            { field: "RoleName", title: '权限组' },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var roleid = row.RoleId.toString();
                var username = row.UserName;
                var realname = row.realName;
                var remark = row.reMark;
                var model = JSON.stringify({ id: id, roleid: roleid, username: username, realname: realname, remark: remark });
                var html = ["<span class=\"btn-a\">"];
                if (row.RoleId != 0) {
                    html.push("<a onclick='Change(" + model + ");'>修改</a>");
                    html.push("<a onclick=\"Delete('" + id + "');\">删除</a>");
                }
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}

function LoadRoleList(callback) {
    if ($("#RoleId option").length > 0) {
        callback();
        return;
    }
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'RoleList',
        cache: false,
        async: true,
        data: {},
        dataType: "json",
        success: function (data) {
            loading.close();
            $(data).each(function (index, item) { $("#RoleId").append("<option value=" + item.Id + ">" + item.RoleName + "</option>") });
            callback();
        },
        error: function () {
            loading.close();
        }
    });
}


function Change(model) {
    var id = model.id;
    var username = model.username;
    var realname = model.realname;
    var remark = model.remark;
    var roleid = model.roleid;
    LoadRoleList(function () {
        $("#UserName").val(username).attr("disabled", true);
        $("#PassWord").val("");
        $("#confirmPassWord").val("");
        $("#name-prefix").text("");
        $("#realName").val(realname);
        $("#reMark").val(remark);
        if (roleid != 0) {
            $("#RoleId").val(roleid);
            $("#roleGroupDiv").show();
        }
        else {
            $("#roleGroupDiv").hide();
        }
    });

    $.dialog({
        title: '修改密码',
        lock: true,
        id: 'ChangePwd',
        width: '500px',
        content: document.getElementById("addManagerform"),
        padding: '0 40px',
        okVal: '确定',
        init: function () { $("#PassWord").focus(); },
        ok: function () {
            var confirmPassWord = $("#confirmPassWord").val();
            var realName = $("#realName").val();
            var reMark = $("#reMark").val();
            var SelectedRoleId = $("#RoleId").val();
            var password = $("#PassWord").val();
            if (realName == null || realName == "") {
                $.dialog.tips("用户姓名必须填写！");
                return false;
            }
            if (password != null && password != "") {
                if (!pwdreg.test(password)) {
                    $.dialog.errorTips("密码6-20位字符，不能包含空格！");
                    return false;
                }
                if (confirmPassWord != password) {
                    $.dialog.tips("两次密码输入不一致！");
                    return false;
                }
            }
            var loading = showLoading();
            if (SelectedRoleId == null)
                SelectedRoleId = 0;
            $.post("./Change",
                { id: id, password: password, roleid: SelectedRoleId, realName: realName, reMark: reMark },
                function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.tips("修改成功", function () {
                            //if (roleid != 0 && roleid != SelectedRoleId)
                                query();
                        });
                        $("#password").val("");
                    }
                    else
                        $.dialog.tips("修改失败:" + data.msg);
                });
        }
    });
}

function LoadAddBox() {
    LoadRoleList(function () {
        $("#UserNameDiv").show();
        $("#name-prefix").text(mainUserName + ":");
        $("#UserName").val("").removeAttr("disabled");
        $("#roleGroupDiv").show();
        $("#PassWord").val("");
        $("#RoleId").val("");
        $("#realName").val("");
        $("#reMark").val("");
        $("#confirmPassWord").val("");
    })
    $.dialog({
        title: '添加管理员',
        id: 'addManager',
        width: '500px',
        padding:'0 40px',
        content: document.getElementById("addManagerform"),
        lock: true,
        okVal: '确认添加',
        init: function () { $("#UserName").focus(); },
        ok: function () {
            var roleId = $("#RoleId").val();
            var username = $("#UserName").val();
            var password = $("#PassWord").val();
            var confirmPassWord = $("#confirmPassWord").val();
            var realName = $("#realName").val();
            var reMark = $("#reMark").val();
            if (realName == null || realName == "") {
                $.dialog.tips("用户姓名必须填写！");
                return false;
            }
            if (confirmPassWord != password) {
                $.dialog.tips("两次密码输入不一致！");
                return false;
            }
            if (roleId == null) {
                $.dialog.tips("请选择权限组，如果无权限组请先添加！");
                return false;
            }
            if (!CheckAdd(username, password))
                return false;
            AddManage(username, password, roleId, realName, reMark);
        }
    });
}

function AddManage(username, password, roleid, realName, reMark) {
    var loading = showLoading();

    $.ajax({
        type: 'post',
        url: 'Add',
        cache: false,
        async: true,
        data: { UserName: username, PassWord: password, RoleId: roleid, realName: realName, reMark: reMark },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("添加成功！");
                $("#addManagerform input").val("");
                query();
            }
            else {
                $.dialog.tips("添加失败！" + data.msg);
            }
        },
        error: function () {
            loading.close();
        }
    });
}


function CheckAdd(username, password) {
    var reg = /^[\u4E00-\u9FA5\@A-Za-z0-9\_\-]{4,20}$/;

    var regpwd = /^[^\s]{6,20}$/;
    var pwdOk = regpwd.test(password);
    if (username.length < 4) {
        $.dialog.tips("用户名不能小于4个字符");
        return false;
    }
    else if (!reg.test(username)) {
        $.dialog.tips('用户名需为4-20位字符，支持中英文、数字及"-"、"_"的组合');
        return false;
    }
    else if (!pwdOk) {
        $.dialog.tips("密码6-20个字符,不包含空格");
        return false;
    }
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'IsExistsUserName',
        cache: false,
        async: false,
        data: { UserName: username },
        dataType: "json",
        success: function (data) {
            loading.close();
            result = !data.Exists;
            if (data.Exists)
                $.dialog.tips("该用户名已存在！");
        },
        error: function () {
            loading.close();
        }
    });
    return result;
}