function AddWeixinMenu(divId) {

    $.dialog({
        title: '新增二级菜单',
        lock: true,
        id: 'addWeixinMenu',
        content: $("#" + divId).html(),
        padding: '0 40px',
        button: [
        {
            name: '确认',
            callback: function () {
                if ($.trim($("#txtMenuName").val()).length <= 0 || $.trim($("#txtMenuName").val()).length > 7) {
                    $.dialog.tips("菜单名称不能为空在7个字符以内");
                    return false;
                }
                if ($("#mainMenu").val() == null) {
                    $.dialog.tips("请选择一级菜单");
                    return false;
                }
                Add($("#txtMenuName").val(), $("#ddlType").val(), $("#mainMenu").val(), $("#menuUrl").val());
            },
            focus: true
        },
        {
            name: '取消',
        }]
    });
}


function AddMainMenu(divId)
{
    $.dialog({
        title: '新增一级菜单',
        lock: true,
        id: 'addWeixinMainMenu',
        content: $("#" + divId).html(),
        padding: '0 40px',
        button: [
        {
            name: '确认',
            callback: function () {
                if ($.trim($("#txtMenuName1").val()).length <= 0 || $.trim($("#txtMenuName1").val()).length > 5) {
                    $.dialog.tips("菜单名称不能为空在5个字符以内");
                    return false;
                }
                Add($("#txtMenuName1").val(), $("#ddlType1").val(), '0', $("#menuUrl1").val());
            },
            focus: true
        },
        {
            name: '取消',
        }]
    });
}


function Add(title, urlType, parentId, url) {
    var loading = showLoading();
    $.post('./AddMenu', { title: title, url: url, parentId: parentId, urlType: urlType }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("添加成功！");
            location.reload();
        }
        else
            $.dialog.errorTips("添加失败"+result.msg);
    });
}

function DeleteMainMenu(MenuId) {
    $.dialog.confirm('删除该分类将会同时删除该分类的所有下级分类，您确定要删除吗？', function () {
        var loading = showLoading();
        $.post('./DeleteMenu', { menuId: MenuId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除成功', function () {
                    location.reload();
                });
            }
            else
                $.dialog.alert('删除失败！' + result.msg);
        });
    });
}
function DeleteMenu(MenuId) {
    //var loading = showLoading();
    $.dialog.confirm('确定删除该分类吗？', function () {
        var loading = showLoading();
        $.post('./DeleteMenu', { menuId: MenuId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除成功');
                location.reload();
            }
            else
                $.dialog.alert('删除失败！' + result.msg);
        });
    });
}

function DialogAddMenu() {
    var title = $("#txtMenuName").val();
    if (title, length > 7)
        alert("二级菜单限7个字符");
    var urlType = $("#ddlType").val();
    var url = $("#menuUrl").val();
    var parentId = $("#mainMenu").val();
    AddWeixinMenu("childMenu");
}

function DialogAddMainMenu() {
    var title = $("#txtMenuName1").val();
    if (title.length > 5)
        alert("一级菜单限5个字符")
    var urlType = $("#ddlType1").val();
    var url = $("#menuUrl1").val();
    AddMainMenu("mainMenu");
}

function RequestToWeixin() {
    var loading = showLoading();
    $.post('./RequestToWeixin', {}, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips('同步成功', function () {
                location.reload();
            });

        }
        else
            $.dialog.alert('同步失败！' + result.msg);
    });
}

function Onchange(obj) {
    if (obj.val() == "6") {
        obj.parents('.dialog-form').find(".linkUrl").show();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val();
    }
    else {
        obj.parents('.dialog-form').find(".linkUrl").hide();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val();
    }
}