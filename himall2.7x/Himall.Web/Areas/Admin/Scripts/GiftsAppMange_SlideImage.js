var isslideloaded = false, isrouletteloaded = false, isscratchcardloaded = false;
$(function () {
    InitSlideEventBind();
});

function InitSlideEventBind() {

    $("#slideImagesTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newRowNumber = parseInt($('.glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeSequence(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable").hiMallDatagrid('reload', {});
            });
        }
    });


    $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('.glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeSequence(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable").hiMallDatagrid('reload', {});
            });
        }
    });

    $('#topicSearchButton').unbind('click').click(function () {
        var titleKeyword = $('#titleKeyword').val();
        var tagsKeyword = $('#tagsKeyword').val();
        $("#topicGrid").hiMallDatagrid('reload', { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword });
    });
}

function DeleteSlideImage(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/Gift/AppDeleteSlideImage', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initSlideImagesTable();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
}

function changeSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/Admin/Gift/AppSlideImageChangeSequence',
        cache: false,
        async: true,
        data: { oriRowNumber: oriRowNumber, newRowNumber: newRowNumber },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (!data.success)
                $.dialog.tips('调整顺序出错!' + data.msg);
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

function initSlideImagesTable() {
    var loading = showLoading();
    //商品表格
    $("#slideImagesTable").hiMallDatagrid({
        url: '/admin/Gift/AppGetSlideImages',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何轮播图',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "imgUrl", title: '缩略图', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<img width="100" height="24" src="' + value + '" />';
                    return html;
                }
            },
            {
                field: "description", title: '描述', align: "center", width: 120, formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:120px">' + value + '</span> ';
                },
            },
            {
                field: "url", title: '跳转链接', align: "center", width: 180, formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:180px">' + value + '</span> ';
                },
            },
            {
                field: "displaySequence", title: '排序', align: "center", formatter: function (value, row, index) {
                    return '<span class="glyphicon glyphicon-circle-arrow-up" rownumber="' + value + '" rowIndex="' + index + '"></span> <span class="glyphicon glyphicon-circle-arrow-down" rownumber="' + value + '" rowIndex="' + index + '"></span>';
                }
            },
            {
                field: "id", title: '操作', align: "center", formatter: function (value, row, index) {
                    return ' <span class="btn-a">\
                                    <a class="good-check" onclick="editSlideImage(' + value + ',\'' + row.description + '\',\'' + row.url + '\',\'' + row.imgUrl + '\')">编辑</a>\
                                    <a class="good-check" onclick="DeleteSlideImage('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            isslideloaded = true;
            loading.close();
            $("#slideImagesTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#slideImagesTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}

$('#addSlideImage').click(function () {
    editSlideImage();
});

function editSlideImage(id, description, url, imageUrl) {
    $.dialog({
        title: (id ? '编辑' : '新增') + '轮播图',
        lock: true,
        padding: '0 40px',
        width: 480,
        id: 'editSlideImage',
        content: $('#editSlideImage')[0],
        okVal: '保存',
        ok: function () {
            return submitSlideImage();
        }
    });

    if (id) {
        $('#SlideImageBox').val(imageUrl);
        $('#url').val(url);
        $('#description').val(description);
        $('#SlideImageId').val(id);
    }
    else {
        $('#SlideImageId').val('');
        $('#SlideImageBox').val('');
        $('#url').val('');
        $('#description').val('');
    }

    $("#imgUrl").himallUpload({
        title: '轮播图片：',
        imageDescript: '请上传640 * 300的图片',
        displayImgSrc: $('#SlideImageBox').val(),
        imgFieldName: "imgUrl",
        dataWidth: 8
    });

}

function generateSlideImageInfo() {
    //轮播对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id = $('#SlideImageId').val();
    slideImage.description = $('#description').val();
    slideImage.imageUrl = $("#imgUrl").himallUpload('getImgSrc');
    slideImage.url = $("#url").val();

    if (!slideImage.imageUrl)
        throw new Error('请上传图片');
    if (slideImage.description.length > 10)
        throw new Error('描述信息在10个字符以内');
    if (!slideImage.url)
        throw new Error('请输入链接地址');
    if (slideImage.url.toLowerCase().indexOf('http://') < 0 && slideImage.url.toLowerCase().indexOf('https://') < 0 && slideImage.url.charAt(0) != '/')
        throw new Error('链接地址请以"http://"或"https://"开头');

    !slideImage.id && (slideImage.id = 0);
    return slideImage;
}

function submitSlideImage() {
    var returnResult = false;
    var object;
    try {
        object = generateSlideImageInfo();
        if (!object.imageUrl)
            $.dialog.tips("请上传轮播图片");
        else {
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: '/admin/Gift/AppAddSlideImage',
                data: { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.url },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        returnResult = true;
                        $.dialog.tips('保存成功');
                        setTimeout(function () {
                            initSlideImagesTable();
                        }, 1500);
                    }
                    else
                        $.dialog.tips('保存失败！' + result.msg);
                }
            });
        }
    }
    catch (e) {
        $.dialog.errorTips('保存失败!' + e.message);
    }
    return returnResult;
}

function editImageAd(id) {
    getSlideImage(id);
    $.dialog({
        title: ('编辑轮播图'),
        lock: true,
        padding: '0 40px',
        width: 450,
        id: 'editImageAds',
        content: $('#editImageAds')[0],
        okVal: '保存',
        ok: function () {
            return submitImageAd(id);
        }
    });
    var Descript = "请上传212*234的图片";
    if (id == 16 || id == 17) {
        Descript = "请上传320*158的图片"
    }
    $("#uploadImgUrl").himallUpload({
        title: '专题图片：',
        imageDescript: Descript,
        displayImgSrc: $('#SlideImageBox').val(),
        imgFieldName: "uploadImgUrl",
        dataWidth: 8
    });
}
function submitImageAd(id) {
    var imageUrl = $('#uploadImgUrl').himallUpload('getImgSrc');
    var url = $("#gotoUrl").val();
    $.ajax({
        type: "post",
        url: '/admin/Gift/AppUpdateImageAd',
        data: { id: id, pic: imageUrl, url: url },
        dataType: "json",
        async: false,
        success: function (result) {
            if (result.success) {
                returnResult = true;
                $.dialog.tips('保存成功');
                setTimeout(function () {
                    initSlideImagesTable();
                }, 1500);
            }
            else
                $.dialog.tips('保存失败！' + result.msg);
        }
    });
}

function getSlideImage(id) {
    $.ajax({
        type: "GET",
        url: '/admin/Gift/AppGetImageAd/' + id,
        dataType: "json",
        async: false,
        success: function (result) {
            if (result.success) {
                $("#gotoUrl").val(result.url);
                $('#SlideImageBox').val(result.imageUrl);
            }
            else
                $.dialog.tips("获取图片失败" + result.msg);
        }
    });
}

