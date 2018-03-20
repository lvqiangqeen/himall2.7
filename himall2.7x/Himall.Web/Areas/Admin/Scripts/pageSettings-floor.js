function changeSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'FloorChangeSequence',
        cache: false,
        async: true,
        data: { oriRowNumber: oriRowNumber, newRowNumber: newRowNumber },
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


function changSwitch(id,enable,callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'FloorEnableDisplay',
        cache: false,
        async: true,
        data: { id: id, enable: enable },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success)
                callback();
        },
        error: function () {
            loading.close();
        }
    });
}


function deleteFloor(id,name) {
    $.dialog.confirm('确定要删除楼层 ' + name + ' 吗？', function () {
        var loading = showLoading();
        $.post('DeleteFloor', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功!', function () {
                    $('tr[floorId="' + id + '"]').fadeOut(1000, function () { $(this).remove(); });
                });
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);

        });
    });

}



$(function () {

    $(".bootstrapSwitch").bootstrapSwitch({
        onSwitchChange: function (event, state) {
            var id = $(this).parents('tr').attr('floorId');
            var switcher = $(this);
            changSwitch(id, state, function () {
                if (result!=undefined&&result.success) {
                    switcher.bootstrapSwitch('state', state);
                }
                else {
                    switcher.bootstrapSwitch('state', !state);
                    $.dialog.errorTips('操作失败!失败原因：' + result.msg);
                }
            });
        }
    });

    //开关
    $(".bootstrapSwitch").bootstrapSwitch();

    //排序
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    $(".table").on("click", '.glyphicon-circle-arrow-up', function () {
        var thisObj = this;
        var p = $(this).parents('tr');
        var index = p.parent().find('tr').index(p);
        if (index == 0)
            return false;
        else {
            var oriRowNumber = parseInt(p.attr('rowNumber'));
            var newRowNumber = p.prev().attr('rowNumber');
            changeSequence(oriRowNumber, newRowNumber, function () {
                p.attr('rowNumber', newRowNumber);
                p.prev().attr('rowNumber', oriRowNumber);
                p.prev().before(p);
                reDrawArrow(thisObj);
            });
        }
    } );

    $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
        var thisObj = this;
        var p = $(this).parents('tr');
        var count = p.parent().find('tr').length;
        var index = p.parent().find('tr').index(p);
        if (index == (count - 1))
            return false;
        else {
            var oriRowNumber = parseInt(p.attr('rowNumber'));
            var newRowNumber = p.next().attr('rowNumber');

            changeSequence(oriRowNumber, newRowNumber, function () {
                p.attr('rowNumber', newRowNumber);
                p.next().attr('rowNumber', oriRowNumber);
                p.next().after(p);
                reDrawArrow(thisObj);
            });
        }
    });

  

});


function reDrawArrow(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}