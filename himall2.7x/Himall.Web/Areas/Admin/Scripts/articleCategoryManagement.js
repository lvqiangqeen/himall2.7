function updateOrderOrName(actionName, param, obj) {
    var loading = showLoading();
    $.post(actionName, param, function (data) {
        loading.close();
        if (data.success) {
            obj.attr('oriValue', actionName == 'UpdateOrder' ? param.displaySequence : param.name);
            $.dialog.tips('更新文章分类的' + (actionName == 'UpdateOrder' ? '显示顺序' : '名称') + '成功.');
        } else {
            $.dialog.errorTips(data.msg);
            window.location.reload();
        }
    });
}

function categoryTextEventBind() {
    var _order = 0;

    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });

    $('.container').on('blur', '.text-name,.text-order', function () {
        var id = $(this).parent('td').find('.hidden_id').val();
        var isdefault = $(this).attr("isdefault");
        if (isdefault == "True") return;
        var value = $.trim($(this).val());
        if (!value) {
            $(this).val($(this).attr('oriValue'));
        }
        else {
            if ($(this).hasClass('text-order')) {
                if (isNaN($(this).val()) || parseInt($(this).val()) <= 0) {
                    $.dialog({
                        title: '更新分类信息',
                        lock: true,
                        width: '400px',
                        padding: '20px 60px',
                        content: ['<div class="dialog-form">您输入的序号不合法,此项只能是大于零的整数.</div>'].join(''),
                        button: [
                        {
                            name: '关闭',
                        }]
                    });
                    $(this).val(_order);
                } else {
                    if (parseInt($(this).val()) === _order) return;
                    updateOrderOrName("UpdateOrder", { id: id, displaySequence: parseInt(value) }, $(this));
                }
            }
            else {
                updateOrderOrName("UpdateName", { id: id, name: value }, $(this));
            }
        }
    });
}


function bindGlyphicon() {
    $('.level-1 .glyphicon').click(function () {
        var p = $(this).parents('.level-1');
        if ($(this).hasClass('glyphicon-plus-sign')) {
            var category = $(this).next('input').val();
            var url = "GetArticleCategories";
            ajaxRequestForCategoryTree(this, category, url, 1);
        } else {
            $(this).removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
            p.nextUntil('.level-1').remove();
        }
    });
}



$(function () {

    categoryTextEventBind();

    bindGlyphicon();

    initialBatchDelete();
});


function initialBatchDelete() {
    $("#deleteBatch").click(function () {
        var ids = [];

        $('table.category_table tbody tr.level-1').each(function () {
            var curRow = $(this);
            if (curRow.find('input[type=checkbox]').attr('checked') == 'checked') {
                ids.push(curRow.find('input.hidden_id').val());
            }
        });
        if (ids.length == 0) { $.dialog.tips('不能批量删除,因为您没有选中任何分类.'); return; }
        $.dialog.confirm('确定删除选中的' + ids.length + '分类吗？', function () {
            var loading = showLoading();
            ajaxRequest({
                type: "POST",
                url: 'BatchDelete',
                param: { ids: ids.toString() },
                dataType: "json",
                success: function (data) {
                    if (data.success) {
                        $.dialog.tips('删除成功！');
                        setTimeout(function () { location.href = "Management"; }, 1500);
                    } else {
                        $.dialog.errorTips("删除失败,请重试！", _this);
                    }
                    loading.close();
                },
                error: function (e) {
                    $.dialog.errorTips("删除失败,请重试！", _this);
                    loading.close();
                }
            });
        });
    });
}

function ajaxRequestForCategoryTree(target, category, url, layer) {
    var maxDepth = 2;
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: url,
        cache: false,
        data: { parentId: category },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data) {
                var p = $(target).parents('.level-' + layer);
                if (data.length === 0) { $.dialog.tips('该分类下目前还没有子分类.'); return; }
                for (var i = 0; i < data.length; i++) {
                    $(target).addClass('glyphicon-minus-sign').removeClass('glyphicon-plus-sign');
                    var left = layer == 1 ? 5 : (layer - 1) * 50;
                    var pix = data[i].Depth === maxDepth ? '├───' : '└───';
                    var className = "invisible";
                    var sub = ['<tr class="level-' + (layer + 1) + '">',
                        '<td><s class="line" style="margin-left:' + left + 'px">' + pix + '</s>'];
                    if (data[i].Depth !== maxDepth) {
                        sub.push('<span class="glyphicon glyphicon-plus-sign"></span>');
                    }
                    sub.push('<input class="hidden_id" type="hidden" value="' + data[i].Id + '">');
                    sub.push('<input class="text-name" maxlength="5" type="text" value="' + data[i].Name + '"/>');
                    //sub.push('<input class="text-order" type="text" value="' + data[i].DisplaySequence + '"/></td>');
                    sub.push('<td class="td-operate">');
                    sub.push('<span class="btn-a">');

                    if (data[i].Depth !== maxDepth) {
                        className = "add-classify";
                    }
                    sub.push('<a onclick="edit(' + data[i].Id + ')" href="javascript:;">编辑</a><a class="delete-classify"  onclick="del(' + data[i].Id + ')">删除</a></span>');
                    sub.push('</td>');
                    sub.push('</tr>');
                    p.after(sub.join(''));
                }

                $('.level-' + (layer + 1) + ' .glyphicon').unbind('click').bind('click', function () {
                    var p = $(this).parents('.level-' + (layer + 1));
                    if ($(this).hasClass('glyphicon-plus-sign')) {
                        var category = $(this).next('input').val();
                        var url = "ajaxRequestForCategoryTree";
                        ajaxRequestForCategoryTree(this, category, url, layer + 1);
                    } else {
                        $(this).removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
                        p.nextUntil('.level-2,.level-1').remove();
                    }

                });
            }

        },
        error: function () {
            loading.close();
        }
    });
};


function del(id) {
    if (id > 0) {
        $.dialog.confirm('您确定要删除吗？', function () {
            var loading = showLoading();
            $.post('delete', { id: id }, function (result) {
                if (result.success) {
                    $.dialog.tips('删除成功！');
                    setTimeout(function () { location.reload(); }, 1500);
                }
                else
                    $.dialog.errorTips('删除失败：' + result.msg);
                loading.close();
            });
        });
    }

}


function edit(id, defaultParentId) {
    var oriArticleCateogry = { name: '', parentId: 0 };
    if (id) {
        var loading = showLoading();

        $.ajax({
            type: "post",
            url: "GetArticleCategory",
            data: { id: id },
            dataType: "json",
            async: true,
            success: function (data) {
                oriArticleCateogry = data;
            },
            error: function () {
            }
        });
    }

    $.post('GetArticleCategories', { parentId: 0 }, function (data) {
        var selector = '<p><select id="parentArticleCategory" class="form-control input-sm"><option value="0" >无</option>';
        $.each(data, function (i, articleCategory) {
            selector += '<option value="' + articleCategory.Id + '" ' + ((articleCategory.Id == oriArticleCateogry.parentId || articleCategory.Id == defaultParentId) ? 'selected="selected"' : '') + ' >' + articleCategory.Name + '</option>';
        });
        selector += '</select></p>';
        $.dialog({
            title: '新增分类',
            lock: true,
            id: 'addArticleSort',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<label class="label-inline fl" for="">名称</label><input class="form-control input-sm" maxlength="5" type="text" id="articleName" value="' + oriArticleCateogry.name + '"><p class="help-block">不能多于5个字</span></p>',

                '</div>',
                '<div class="form-group">',
                    '<label class="label-inline fl" for="">上级分类</label>',
                    selector,
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#articleName").focus(); },
            okVal: '保存',
            ok: function () {
                var articleName = $.trim($('#articleName').val());
                if (!articleName) {
                    $.dialog.tips('请输入文章分类名称');
                    return false;
                }
                else {
                    var loading = showLoading();
                    var parentId = $('#parentArticleCategory').val();
                    var params = {};
                    if (id)
                        params.id = id;
                    params.parentId = parentId;
                    params.name = articleName;

                    $.post('add', params, function (result) {
                        loading.close();
                        if (result.success) {
                            $.dialog.succeedTips((id ? '添加' : '保存') + '成功', function () {
                                //if (parentId == 0) {
                                    location.reload();
                                //}
                            });
                            if (!id) {
                                $('span[articleCategoryId="' + parentId + '"]').removeClass('disabled');
                            }
                        }
                        else
                            $.dialog.errorTips((id ? '添加' : '保存') + '失败！' + result.msg);

                    });
                }
            }
        });

        loading.close();
    });


}


$(function () {
    $('#addArticleCategory').click(function () {
        edit();
    })
})



