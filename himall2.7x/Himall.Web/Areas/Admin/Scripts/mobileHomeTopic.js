$(function () {

    $('#chooseTopicBtn').click(function () {
        chooseTopic();
    });


    //表格单选点击事件
    $('#list').on('click', 'tr', function () {
        var radio = $(this).find('input[type="radio"]');
        radio.attr('checked', true);
    });

    //绑定删除事件
    $('a[name="delete"]').click(function () {
        var id = $(this).parents('tr').attr('id');
        $.dialog.confirm('您确定要从首页删除该专题吗？', function () {
            removeChooseTopic(id);
        });
    });

    //更新顺序
    $('input[name="sequence"]').blur(function () {
        var id = $(this).parents('tr').attr('id');
        var sequence = $(this).val();
        var sequence = parseInt(sequence);
        if (isNaN(sequence)) {
            $.dialog.errorTips('数字格式不正确');
            $(this).val($(this).attr('orivalue'));
        }
        else {
            $.post('UpdateSequence', { id: id, sequence: sequence }, function (data) {
                if (data.success) {
                    $(this).attr('orivalue',sequence);
                   // $.dialog.tips('更新显示顺序成功');
                }
                else
                    $.dialog.errorTips('更新显示顺序失败！' + result.msg);
            });
        }
    });

});

function removeChooseTopic(id) {
    var loading = showLoading();
    $.post('RemoveChoseTopic', { id: id }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips('删除成功', function () { location.reload(); });
        }
        else
            $.dialog.errorTips('删除失败！' + result.msg);
    });

}


function chooseTopic() {
    $.dialog({
        title: '新增精选',
        lock: true,
        width: 550,
        padding: '0 40px',
        id: 'chooseTopicDialog',
        content: $('#choose-topic')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseTopic();
        }
    });


    $('#searchButton').unbind('click').click(function () {
        var titleKeyword = $('#titleKeyword').val();
        var tagsKeyword = $('#tagsKeyword').val();
        $("#list").hiMallDatagrid('reload', { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword });
    });

    $("#frontCoverImage").himallUpload(
    {
        title: '封面',
        imageDescript: '请上传640 * 300的图片',
        //  displayImgSrc: $('#backgroudImageBox').val(),
        imgFieldName: "frontCoverImage",
        dataWidth: 10
    });

    //商品表格
    $("#list").hiMallDatagrid({
        url: '/admin/MobileTopic/list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 1 },
        columns:
        [[
            {
                field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<input type="radio" name="topic" topicId="'+value+'" />';
                    return html;
                }
            },
            {
                field: "name", title: '专题名称', align: "center", formatter: function (value, row, index) {
                    var html = '<a href="/topic/detail/' + row.id + '" target="_blank">' + row.name + '</a>';
                    return html;
                }
            },
            {
                field: "tags", title: '标签', align: "center"
            }
        ]]
    });
}


function saveChooseTopic() {
    var img = $("#frontCoverImage").himallUpload('getImgSrc');
    var selectedId = $('#list input[type="radio"][checked="checked"]').attr('topicId');
    var result = false;
    if (!img)
        $.dialog.tips('请选择封面图片');
    else if (!selectedId)
        $.dialog.tips('请选择专题');
    else {
        var loading = showLoading();
        $.post('ChooseTopic', { frontCoverImage: img, topicId: selectedId }, function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips('选择成功', function () { location.reload(); });
                result = true;
            }
            else
                $.dialog.errorTips('选择失败！' + data.msg);
        });
        result = true;
    }
    return result;
}