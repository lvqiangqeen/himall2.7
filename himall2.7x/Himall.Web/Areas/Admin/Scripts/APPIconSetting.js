// JavaScript source code
function DeleteAppIcon(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/APPShop/DeleteAppIcon', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initAppIconTable();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });

}

$(function () {
    initAppIconTable();

    $("#appIconTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newRowNumber = parseInt($('#appIconTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeAppIconSequence(oriRowNumber, newRowNumber, function () {
                $("#appIconTable").hiMallDatagrid('reload', {});
            });
        }
    });


    $("#appIconTable").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('#appIconTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeAppIconSequence(oriRowNumber, newRowNumber, function () {
                $("#appIconTable").hiMallDatagrid('reload', {});
            });
        }
    });
});

function changeAppIconSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'APPIconChangeSequence',
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

//function reDrawArrow(obj) {
//    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
//    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
//    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
//}

function initAppIconTable() {
    //商品表格
    $("#appIconTable").hiMallDatagrid({
        url: '/admin/APPShop/GetAPPIcons',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何图标',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "imgUrl", title: '缩略图', align: "center", width: 120, formatter: function (value, row, index) {
                    var html = '<img width="68" height="72" src="' + value + '" />';
                    return html;
                }
            },
            {
                field: "description", title: '图标名称', align: "center", width: 120, formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:120px">' + value + '</span> ';
                },
            },
            {
                field: "url", title: '跳转链接', align: "center", width: 180, formatter: function (value, row, index) {
                    var text = "";
                    switch(value)
                    {
                        case "1":
                            text = "专题";
                            break;
                        case "2":
                            text = "拼团";
                            break;
                        case "3":
                            text = "限时购";
                            break;
                        case "4":
                            text = "积分商城";
                            break;
                        case "5":
                            text = "分类";
                            break;
                        case "6":
                            text = "周边门店";
                            break;
                    }
                    return '<span class="overflow-ellipsis" style="width:180px">' + text + '</span> ';
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
                                    <a class="good-check" onclick="editAppIcon(' + value + ',\'' + row.description + '\',\'' + row.url + '\',\'' + row.imgUrl + '\')">编辑</a>\
                                    <a class="good-check" onclick="DeleteAppIcon('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#appIconTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#appIconTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}


$('#addAppIcon').click(function () {
    editAppIcon();
});



function editAppIcon(id, description, url, imageUrl) {
    $.dialog({
        title: (id ? '编辑' : '新增') + '图标',
        lock: true,
        padding: '0 20px',
        width: 480,
        id: 'editAppIcon',
        content: $('#editAppIcon')[0],
        okVal: '保存',
        ok: function () {
            return submitAppIcon();
        }
    });

    if (id) {
        $('#appIconBox').val(imageUrl);
        $('#iconUrl').val(url);
        $('#iconDesc').val(description);
        $('#appIconId').val(id);
    }
    else {
        $('#appIconId').val('');
        $('#appIconBox').val('');
        $('#iconUrl').val('');
        $('#iconDesc').val('');
    }

    $("#iconimgUrl").himallUpload({
        title: '图标：',
        imageDescript: '请上传86*86的图片',
        displayImgSrc: $('#appIconBox').val(),
        imgFieldName: "iconimgUrl",
        dataWidth: 7
    });

}


function generateAppIcon() {
    //专题对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id = $('#appIconId').val();
    slideImage.description = $('#iconDesc').val();
    slideImage.imageUrl = $("#iconimgUrl").himallUpload('getImgSrc');
    slideImage.url = $("#iconUrl").val();

    if (!slideImage.imageUrl)
        throw new Error('请上传图片');
    if (slideImage.description.length > 10)
        throw new Error('描述信息在10个字符以内');
    if (!slideImage.url)
        throw new Error('请选择地址');

    !slideImage.id && (slideImage.id = 0);
    return slideImage;
}



function submitAppIcon() {
    var returnResult = false;
    var object;
    try {
        object = generateAppIcon();
        if (!object.imageUrl)
            $.dialog.tips("请上传轮播图片");
        else {
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: '/admin/APPShop/AddAppIcon',
                data: { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.url },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        returnResult = true;
                        $.dialog.tips('保存成功');
                        setTimeout(function () {
                            initAppIconTable();
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

