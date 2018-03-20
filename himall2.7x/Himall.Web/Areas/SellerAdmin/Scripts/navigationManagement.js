
$(function () {
    bindSortEvent();
    //添加导航
    LoadAddBox();
})

function Delete(id) {
    $.dialog.confirm('确定删除该条记录吗？', function () {
        $.post("./Delete", { id: id }, function (data) { if (data.success) { $.dialog.tips('删除成功'); $("tr[swapid='" + id + "']").remove(); ReSetSwapIcon(); } else { $.dialog.errorTips('删除失败！' + data.msg); } });
    });
}


function bindSortEvent() {
    //排序
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    $(".table").on("click", '.glyphicon-circle-arrow-up', function () {
        var p = $(this).parents('tr');
        var index = p.parent().find('tr').index(p);
        var thisObj = this;
        if (index == 0)
            return false;
        else {
            var oriId = parseInt(p.attr("swapid"));
            var newId = parseInt(p.prev().attr("swapid"));
            swapSequence(oriId, newId, function () {
                p.prev().before(p);
                reDrawArrow(thisObj);
            });
        }
    });

    $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
        var p = $(this).parents('tr');
        var count = p.parent().find('tr').length;
        var index = p.parent().find('tr').index(p);
        var thisObj = this;
        if (index == (count - 1))
            return false;
        else {
            var oriId = parseInt(p.attr("swapid"));
            var newId = parseInt(p.next().attr("swapid"));
            swapSequence(oriId, newId, function () {
                p.next().after(p);
                reDrawArrow(thisObj);
            });
        }
    });
    $(".table").on("click", '.glyphicon', function () {
        $(this).parents('tbody').find('.glyphicon').removeClass('disabled');
        $(this).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
        $(this).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    });
}

function ReSetSwapIcon() {
    $(".table tbody").find('.glyphicon').removeClass('disabled');
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}

function swapSequence(id, id2, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'SwapDisplaySequence',
        cache: false,
        async: true,
        data: { id: id, id2: id2 },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (!data.success)
                $.dialog.errorTips('调整顺序出错!' + data.msg);
            else
                callback();
        },
        error: function () {
            loading.close();
        }
    });
}
function reDrawArrow(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}


//添加导航
function LoadAddBox() {

    $('.add-nav-link').click(function () {
        $("#txtNavName").val("");
        $("#txtNavUrl").val("");
        $.dialog({
            title: '新增导航',
            lock: true,
            padding:'0 40px',
            id: 'addNavLink',
            content: document.getElementById("dialog-form"),
            okVal: '保存',
            ok: function () {
                var navname = $("#txtNavName").val();
                var navurl = $("#txtNavUrl").val();
                if (navname == "" || navurl == "") {
                    $.dialog.tips("导航名和跳转地址不能为空");
                    return false;
                }
                else {
                    var loading = showLoading();
                    $.post("./Add", { Name: navname, Url: navurl },
                        function (data) {
                            loading.close();
                            if (data.success) {
                                $.dialog.tips("添加导航成功！",function () { window.location.href = window.location.href;});
                            }
                            else
                                $.dialog.tips(data.msg);
                        });
                }

            }
        });
    });
}
//编辑导航
function Edit(obj) {
    var id = $(obj).parents('tr').attr("swapid");
    $("#txtNavName").val($(obj).parents('tr').find("td").eq(0).text().trim());
    $("#txtNavUrl").val($(obj).parents('tr').find("td").eq(1).text().trim());
    $.dialog({
        title: '编辑导航',
        lock: true,
        padding:'0 40px',
        id: 'EditNavLink',
        content: document.getElementById("dialog-form"),
        okVal: '保存',
        ok: function () {
            var navname = $("#txtNavName").val();
            var navurl = $("#txtNavUrl").val();
            if (navname == "" || navurl == "") {
                $.dialog.tips("导航名和跳转地址不能为空");
                return false;
            }
            else {
                var loading = showLoading();
                $.post("./Edit", { ID: id, Name: navname, Url: navurl },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.tips("编辑导航成功！");
                            $(obj).parents('tr').find("td").eq(0).text(navname);
                            $(obj).parents('tr').find("td").eq(1).text(navurl);
                        }
                        else
                            $.dialog.tips(data.msg);
                    });
            }
        }
    });
}