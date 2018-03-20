// JavaScript source code
function Onchange(obj) {
    if (obj.val() == "5") {
        obj.parents('.dialog-form').find(".linkUrl").show();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val();
    }
    else {
        obj.parents('.dialog-form').find(".linkUrl").hide();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val();
    }
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
        $.post('./DeleteMenu', { menuId: MenuId }, function (result) {
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

/*保存二级菜单*/
function SaveMenu(menuId) {
    $.post('./GetMenu', { menuId: menuId }, function (result) {
        if (result.success) {
            $.dialog({
                title: '保存二级菜单',
                lock: true,
                id: 'addWeixinMenu',
                content: $("#childMenu").html(),
                padding: '0 40px',
                init: function () {
                    $("#txtMenuName").val(result.title);
                    $("#ddlType").val(result.urlType);
                    $("#menuUrl").val(result.url);
                    $("#mainMenu").val(result.parentId);
                    if (result.urlType == 5) {
                        $('.linkUrl').show();
                    }
                },
                button: [
                {
                    name: '确认',
                    callback: function () {
                        if ($("#txtMenuName").val().length <= 0 || $("#txtMenuName").val().length > 7) {
                            $.dialog.alert("菜单名称不能为空在7个字符以内");
                            return false;
                        }
                        if ($("#mainMenu").val() == null) {
                            $.dialog.alert("请选择一级菜单");
                            return false;
                        }
                        Add($("#txtMenuName").val(), $("#ddlType").val(), $("#mainMenu").val(), $("#menuUrl").val(), menuId);
                    },
                    focus: true
                },
                {
                    name: '取消',
                }]
            });
        }
        else
            $.dialog.alert("保存失败" + result.msg)
    });
}

/*保存一级菜单*/
function SaveMainMenu(menuId) {
    $.post('./GetMenu', { menuId: menuId }, function (result) {
        if (result.success) {
            $.dialog({
                title: '保存一级菜单',
                lock: true,
                id: 'addWeixinMainMenu',
                content: $("#mainMenu").html(),
                padding: '0 40px',
                init: function () {
                    $("#txtMenuName1").val(result.title);
                    $("#ddlType1").val(result.urlType);
                    $("#menuUrl1").val(result.url);
                    if (result.urlType == 5) {
                        $('.linkUrl').show();
                    }
                },
                button: [
                {
                    name: '确认',
                    callback: function () {
                        if ($("#txtMenuName1").val().length <= 0 || $("#txtMenuName1").val().length > 5) {
                            $.dialog.alert("菜单名称不能为空在5个字符以内");
                            return false;
                        }
                        Add($("#txtMenuName1").val(), $("#ddlType1").val(), '0', $("#menuUrl1").val(), menuId);
                    },
                    focus: true
                },
                {
                    name: '取消',
                }]
            });
        }
        else
            $dialog.alert("保存失败" + result.msg);
    });
}

function AddMainMenu(menuId) {

}


function Add(title, urlType, parentId, url, menuId) {
    var loading = showLoading();
    $.post('./AddMenu', { title: title, url: url, parentId: parentId, urlType: urlType, menuId: menuId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("保存成功！");
            location.reload();
        }
        else
            $.dialog.alert("保存失败" + result.msg);
    });
}

