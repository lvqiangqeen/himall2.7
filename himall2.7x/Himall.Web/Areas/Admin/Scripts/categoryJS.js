function updateOrderOrName(actionName, param, actionDesc) {
    var loading = showLoading();
    ajaxRequest({
        type: 'GET',
        url: "./" + actionName,
        param: param,
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.Successful == true) {
                $.dialog.tips('更新分类的' + actionDesc + '成功.');
            }
            else {
                $.dialog.errorTips(data.Msg, function () { location.reload(); });
            }
        }, error: function () {
            loading.close();
        }
    });
}

function categoryTextEventBind() {
    var _order = 0;
    var _name = '';
    var _commis = 0;
    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });
    $('.container').on('focus', '.text-name', function () {
        _name = $(this).val();
    });
    $('.container').on('focus', '.text-commis', function () {
        _commis = $(this).val();
    });
    $('.container').on('blur', '.text-name,.text-order,.text-commis', function () {
        var id = $(this).parents('tr').find('.hidden_id').val();
        var depth = $(this).parents('tr').find('.hidden_depth').val();
        if ($(this).hasClass('text-order') && !$(this).hasClass('tac')) {
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
                updateOrderOrName("UpdateOrder", { id: id, order: parseInt($(this).val()) }, '顺序');
            }
        } else {
            if ($(this).hasClass('text-name')) {
                if ($(this).val().trim().length === 0) {
                    $.dialog.errorTips("分类名称不能为空！");
                    $(this).val(_name);
                }
                else if ($(this).val().length > 12) {
                    $.dialog.errorTips("分类名称不能多于12个字符！");
                    $(this).val(_name);
                }
                else {
                    updateOrderOrName("UpdateName", { id: id, name: $(this).val(), depth: depth }, '名称');
                }
            }
            else {
                var reg = /^((\d+(\.\d{1,2}))?|\d{1,2})$/;
                if (!reg.test($(this).val()) || parseFloat($(this).val()) < 0) {
                    $.dialog({
                        title: '更新分类信息',
                        lock: true,
                        width: '400px',
                        padding: '20px 60px',
                        content: ['<div class="dialog-form">您输入的分佣比率不合法,此项只能是大于等于零精确到小数点后两位的数字.</div>'].join(''),
                        button: [
                        {
                            name: '关闭',
                        }]
                    });
                    $(this).val(_commis);
                } else if (parseFloat($(this).val()) > 100) {
                    if (!reg.test($(this).val()) || parseFloat($(this).val()) < 0 || parseFloat($(this).val()) > 100) {
                        $.dialog({
                            title: '更新分类信息',
                            lock: true,
                            width: '400px',
                            padding: '20px 60px',
                            content: ['<div class="dialog-form">佣金比例不得大于100%.</div>'].join(''),
                            button: [
                            {
                                name: '关闭',
                            }]
                        });
                        $(this).val(_commis);
                    }
                } else {
                    if (parseInt($(this).val()) === _commis) return;
                    updateOrderOrName("UpdateCommis", { id: id, commis: $(this).val() }, '分佣比率');
                }
            }
        }
    });
}

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
        $.dialog.confirm('确定删除选中的所有分类吗？', function () {
            var loading = showLoading();
            ajaxRequest({
                type: "POST",
                url: './BatchDeleteCategory',
                param: { Ids: ids.join('|') },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.Successful) {
                        location.href = "./Management";
                    } else {
                        $.dialog.errorTips(data.msg, _this);
                    }
                },
                error: function (e) {
                    loading.close();
                    $.dialog.errorTips("删除失败,请重试！", _this);
                }
            });
        });
    });
}

function saveState() {
    var hides = [];
    $('tr[cid]').each(function () {
        if ($(this).is(':hidden')) {
            hides.push($(this).attr('cid'));
        }
    });

    if (hides.length > 0) {
        $.cookie('CategoryHideItems', hides.join(','));
    }
}

$(function () {
    //新增分类
    categoryTextEventBind();
    //dialogInitial();
    initialBatchDelete();
    $('#btnlevel1').click(function () {
        $('table.category_table tbody span.glyphicon-minus-sign').removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
        $('table.category_table tbody tr.level-2,table.category_table tbody tr.level-3').hide();
        saveState();
    });
    $('#btnlevelAll').click(function () {
        $('table.category_table tbody span.glyphicon-plus-sign').removeClass('glyphicon-plus-sign').addClass('glyphicon-minus-sign');
        $('table.category_table tbody tr.level-2,table.category_table tbody tr.level-3').show();
        $.removeCookie('CategoryHideItems');
    });

    $('.check-all').click(function () {
        var checkbox = $('.table').find('input[type=checkbox]');
        if (this.checked) {
            checkbox.each(function () {
                this.checked = true
            })
        } else {
            checkbox.each(function () {
                this.checked = false
            })
        }
    });

    $('tr .glyphicon').click(function () {
        var category = $(this).next('input').val();
        if ($(this).hasClass('glyphicon-plus-sign')) {
            if ($('tr[parentid=' + category + ']').length == 0) {
                $.dialog.tips('该分类下目前还没有子分类.');
                return;
            }
            $(this).removeClass('glyphicon-plus-sign').addClass('glyphicon-minus-sign');
            $('tr[parentid=' + category + ']').show();
            $('tr[parentid=' + category + ']').each(function (idx, el) {
                var pid = $(el).find('.hidden_id').val();
                if ($('tr[parentid=' + pid + ']').length > 0) {
                    $(el).find('.glyphicon').removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign')
                }
                else {
                    $(el).find('.glyphicon').removeClass('glyphicon-plus-sign').addClass('glyphicon-minus-sign');
                }
            });

            //var url = "./GetCategoryByParentId";
            //ajaxRequestForCategoryTree(this, category, url, 1);
        } else {
            $('tr[parentid=' + category + ']').hide();
            $('tr[parentid=' + category + ']').each(function (idx, el) {
                var pid = $(el).find('.hidden_id').val();
                $('tr[parentid=' + pid + ']').hide();
            });
            $(this).removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
            //p.nextUntil('.level-1').remove();
        }

        saveState();
    });
    //TODO:已修改成默认全部加载，取消了异步加载方法
    function ajaxRequestForCategoryTree(target, category, url, layer) {
        var loading = showLoading();
        $.ajax({
            type: 'GET',
            url: url,
            cache: false,
            data: {
                id: category
            },
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.Successfly === true) {
                    var p = $(target).parents('.level-' + layer);
                    if (data.Category.length === 0) {
                        $.dialog.tips('该分类下目前还没有子分类.'); return;
                    }
                    for (var i = 0; i < data.Category.length; i++) {
                        $(target).addClass('glyphicon-minus-sign').removeClass('glyphicon-plus-sign');
                        var left = layer == 1 ? 5 : (layer - 1) * 50;
                        var pix = data.Category[i].Depth === 3 ? '├───' : '└───';
                        var className = "invisible";
                        var sub = ['<tr class="level-' + (layer + 1) + '">',

                            '<td><s class="line" style="margin-left:' + left + 'px">' + pix + '</s>'];
                        if (data.Category[i].Depth !== 3) {
                            sub.push('<span class="glyphicon glyphicon-plus-sign"></span>');
                        }
                        sub.push('<input class="hidden_id" type="hidden" value="' + data.Category[i].Id + '">');
                        sub.push('<input class="text-name" type="text" value="' + data.Category[i].Name + '"/>');
                        sub.push('<input class="text-order" type="text" value="' + data.Category[i].DisplaySequence + '"/></td>');

                        if (data.Category[i].Depth === 3) {
                            sub.push('<td>');
                            sub.push('<input class="text-commis" type="text" value="' + data.Category[i].CommisRate + '"/>');
                            sub.push('<span >%</span>');
                            sub.push('</td>');
                        }
                        sub.push('<td ></td>');
                        sub.push('<td class="td-operate">');
                        sub.push('<span class="btn-a">');

                        if (data.Category[i].Depth !== 3) {
                            className = "add-classify";
                            sub.push('<a href="./AddByParent?Id=' + data.Category[i].Id + '" class="add">新增下级</a>');
                        }
                        sub.push('<a href="./Edit?Id=' + data.Category[i].Id + '" class="edit">编辑</a><a class="delete-classify">删除</a></span>');
                        sub.push('</td>');
                        sub.push('</tr>');
                        p.after(sub.join(''));
                    }

                    $('.level-' + (layer + 1) + ' .glyphicon').unbind('click').bind('click', function () {
                        var p = $(this).parents('.level-' + (layer + 1));
                        if ($(this).hasClass('glyphicon-plus-sign')) {
                            var category = $(this).next('input').val();
                            var url = "./GetCategoryByParentId";
                            ajaxRequestForCategoryTree(this, category, url, layer + 1);
                        } else {
                            $(this).removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
                            p.nextUntil('.level-2,.level-1').remove();
                        }

                    });
                }

            },
            error: function () {

            }
        });
    };

    $('.container').on('click', 'td.td-operate .delete-classify', function () {
        var msg = "";
        var tr = $(this).closest('tr');
        if (tr.attr("class") == "level-3") {
            msg = "您确定要删除吗？";
        }
        else {
            msg = "删除该分类将会同时删除该分类的所有下级分类，您确定要删除吗？";
        }

        var id = $(this).parents('td.td-operate').prev('td').prev('td').find('.hidden_id').val();
        $.dialog.confirm(msg, function () {
            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: "./DeleteCategoryById",
                param: { id: id },
                dataType: "json",
                success: function (data) {
                    if (data.Successful == true) {
                        //tr.remove();
                        removeChild(tr);
                    } else {
                        $.dialog.errorTips(data.msg);
                    }
                    loading.close();
                }, error: function () { }
            });
        });
    });

    //$.removeCookie('CategoryHideItems');
});
function removeChild(obj) {
    var cid = $(obj).attr("cid");
    $(obj).remove();
    if ($("tr[parentid='" + cid + "']").length > 0) {
        $("tr[parentid='" + cid + "']").each(function () {
            removeChild($(this));
        });
    }
}