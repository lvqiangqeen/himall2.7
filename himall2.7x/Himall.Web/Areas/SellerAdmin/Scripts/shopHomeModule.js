$(function () {

    initModuleNameAutoSave();
    bindAddModuleBtnClickEvent();
    bindDeleteBtnClickEvent();
    bindSelectProductsBtnClickEvent();
})




function initModuleNameAutoSave() {
    $('#moduleTable').on('blur', 'input[moduleName]', function () {
        var thisObj = $(this);
        var tr = thisObj.parents('tr[moduleId]');
        var id = tr.attr('moduleId');
        var newName = thisObj.val();
        var preName = thisObj.attr('preText');
        if (!newName) {
            thisObj.val(thisObj.attr('preText'));
        }
        else if (newName != preName) {
            var loading = showLoading();
            $.post('SaveName', { id: id, name: newName }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存名称成功');
                    thisObj.attr('preText', newName);
                }
                else {
                    $.dialog.errorTips('保存失败!' + result.msg);
                    thisObj.val(thisObj.attr('preText'));
                }
            });
        }
    });
}


function bindAddModuleBtnClickEvent() {
    $('#addModule').click(function () {
        var loading = showLoading();
        $.post('AddShopHomeModule', { name: '新商品模块' }, function (result) {
            loading.close();
            if (result.success) {
                var id = result.id;
                var name = result.name;
                addNewRow(id, name, []);
            }
            else {
                $.dialog.errorTips('添加失败!' + result.msg);
            }
        })
    });
}

function bindDeleteBtnClickEvent() {
    $('#moduleTable').on('click', 'a[delete]', function () {
        var id = $(this).parents('tr').attr('moduleId');
        var name = $(this).parents('tr').find('input[modulename]').val();
        $.dialog.confirm('您确定要删除商品模块 ' + name + ' 吗？', function () {
            del(id);
        });
    })
}

function bindSelectProductsBtnClickEvent() {
    $('#moduleTable').on('click', 'a[productids]', function () {
        var thisObj = $(this);
        var id = thisObj.parents('tr').attr('moduleId');
        var ids = thisObj.attr('productids');
        if (ids)
            ids = ids.split(',');
        else
            ids = [];
        $.productSelector.show(ids, function (selectedProducts) {
            var newProductIds = [];
            $.each(selectedProducts, function () {
                newProductIds.push(this.id);
            });
            if (newProductIds.length <= 0) {
                $.dialog.errorTips('你没有选择任何产品！');
                return false;
            }
            saveShopModuleProducts(id, newProductIds, function () {
                $.dialog.tips('保存商品成功!');
                thisObj.parents('tr').find('td[count]').html(newProductIds.length);
                thisObj.attr('productids', newProductIds.toString());
            });
        },'selleradmin');
    })
}

function saveShopModuleProducts(id, productIds,callBack) {
    var loading = showLoading();
    $.post('SaveShopModuleProducts', { id: id, productIds: productIds.toString() }, function (result) {
        loading.close();
        if (result.success)
            callBack();
        else {
            $.dialog.errorTips('保存失败!' + result.msg);
        }
    })
}


function addNewRow(id, name, productIds) {
    var productsCount = (productIds ? productIds.length : 0);
    var html = '<tr moduleId="' + id + '">\
                    <td><input class="text-order" style="width:200px" type="text" value="' + name + '" modulename preText="' + name + '"></td>\
                    <td count>' + productsCount + '</td>\
                    <td class="td-operate"><span class="btn-a"><a productIds="' + productIds.toString() + '">选择商品</a><a delete>删除</a></span></td>\
                </tr>';
    $('#moduleTable').append(html);
}

function removeRow(id) {
    $('tr[moduleId="' + id + '"]').remove();
}


function del(id) {
    if (id) {
        var loading = showLoading();
        $.post('Delelte', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功');
                removeRow(id);
            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        });
    }
}