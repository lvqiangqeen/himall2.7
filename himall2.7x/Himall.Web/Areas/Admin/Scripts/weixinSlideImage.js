$(function () {

    $("#imgUrl").himallUpload({
        title: '轮播图片',
        imageDescript: '请上传640 * 300的图片',
        displayImgSrc: $('#SlideImageBox').val(),
        imgFieldName: "imgUrl",
		dataWidth: 8
    });
});

function ChoceTopic()
{
    $.dialog({
        title: '选择跳转专题',
        lock: true,
        width: 550,
        padding: '0 40px',
        id: 'chooseTopicDialog',
        content: $('#choceTopicUrl')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseTopic();
        }
    });

    //商品表格
    $("#topicGrid").hiMallDatagrid({
        url: '/admin/MobileTopic/list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 16,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 1 },
        columns:
        [[
            {
                field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<input type="radio" name="topic" topicId="' + value + '" />';
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
    var selectedId = $('input[name="topic"]:checked').attr('topicId');
    if (!selectedId)
        $.dialog.tips('请选择专题');
    else {
        $('#Url').val('/m-weixin/topic/detail/'+selectedId);
    }
}

function generateSlideImageInfo() {
    //专题对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id=$('#SlideImageId').val();
    slideImage.description = $('#Description').val();
    if (slideImage.description.length > 10)
    {
        $.dialog.errorTips("轮播图描述不能超过10个字符！");
        return false;
    }
    slideImage.imageUrl = $("#imgUrl").himallUpload('getImgSrc');
    slideImage.url = $("#Url").val();
    return slideImage;
}



function submitSlideImage() {
    var object;
    if ($('form').valid()) {
        try {
            object = generateSlideImageInfo();
            if (!object.imageUrl)
                $.dialog.tips("请上传轮播图片");
            else {
                var objectString = JSON.stringify(object);
                var loading = showLoading();
                $.post('/admin/WeiXin/AddSlideImage', { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.url }, function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.succeedTips('保存成功',function(){
							location.href = '/Admin/WeiXin/SlideImageSettings';
						});
                    }
                    else
                        $.dialog.errorTips('保存失败！' + result.msg);
                }, "json");
            }
        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }
    }
}


