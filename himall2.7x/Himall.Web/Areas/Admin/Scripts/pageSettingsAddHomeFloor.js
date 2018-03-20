// JavaScript source code

$(function () {

    checkEditMode();
    bindSaveBtnClickEvent();

});

function checkEditMode() {
    var id = $('input[name="id"]').val();
    id = parseInt(id);
    if (id) {
        //编辑模式
        $('#mode').html('编辑');
        $('#saveBtn').html('保存');
        $('#floor-edit-next').show();
        bindSaveAndNextClickEvent();
    }

}

function bindSaveAndNextClickEvent() {
    var id = $('input[name="id"]').val();
    $('#floor-edit-next').click(function () {
        save(function () {
            location.href = 'AddHomeFloorDetail?id=' + id;
        });
    });
}

function bindSaveBtnClickEvent() {
    var id = $('input[name="id"]').val();
    id = parseInt(id);
    $('#saveBtn').click(function () {
        var func = null;
        if (id) {
            //编辑模式
            func = function () {
                $.dialog.tips('保存成功！', function () {
                    location.href = 'HomeFloor';
                })
            }
        }
        else {
            func = function (id) { location.href = 'AddHomeFloorDetail?id=' + id };
        }
        save(func);
    });
}


function save(callBack) {
    var floorName = $.trim($('#floorName').val());
    if (!floorName) {
        $.dialog.tips('请填写楼层名称');
        $('#floorName').focus();
        return;
    }

    var categoryIds = [];
    $.each($('input[name="category"]:checked'), function () {
        categoryIds.push($(this).val());
    });
    if (categoryIds.length == 0) {
        $.dialog.tips('请至少选择一个分类');
        return;
    }

    var id = $('input[name="id"]').val();
    id = parseInt(id);
    var loading = showLoading();
    $.post('SaveHomeFloorBasicInfo',
        { id: id, name: floorName, categoryIds: categoryIds.toString() },
        function (result) {
            loading.close();
            if (result.success) {
                callBack && callBack(result.id);
            }
            else {
                $.dialog.errorTips('保存失败!' + result.msg);
            }
        });

}
