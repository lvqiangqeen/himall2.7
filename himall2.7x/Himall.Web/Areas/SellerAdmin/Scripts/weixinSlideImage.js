$(function () {
    
   

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

    $(".table.slideImage").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = parseInt($('.glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]'));
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeSequence(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable").hiMallDatagrid('reload', {});
            });
        }
    });


    $("#vshopBannerTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = $("#vshopBannerTable").hiMallDatagrid('getRowByIndex', $(this).parents('tr').prev().attr('himalldatagrid-row-index')).id;
        if (rowIndex > 0) {
            changeBannerSequence(oriRowNumber, rowIndex, function () {
                $("#vshopBannerTable").hiMallDatagrid('reload', {});
            });
        }
    });

    $(".table.vshopbanner").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = $("#vshopBannerTable").hiMallDatagrid('getRowByIndex', $(this).parents('tr').next().attr('himalldatagrid-row-index')).id;
        changeBannerSequence(oriRowNumber, rowIndex, function () {
            $("#vshopBannerTable").hiMallDatagrid('reload', {});
        });
    });
});

function saveHomePageTitle() {
    var loading = showLoading();
    $.post('./SaveVShopHomePageTitle', { homePageTitle: $('#homePageTitle').val() }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("保存成功！");

        }
        else
            $.dialog.errorTips("保存失败" + result.msg);
    });

}

function initSlideImagesTable() {
    //商品表格
    $("#slideImagesTable").hiMallDatagrid({
        url: '/selleradmin/VShop/GetSlideImages',
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
                field: "image", title: '图片', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<img width="100" height="24" src="' + value + '" />';
                    return html;
                }
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
                                    <a class="good-check" onclick="SaveSlideImage(' + value + ')">编辑</a>\
                                    <a class="good-check" onclick="DeleteSlideImage('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#slideImagesTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#slideImagesTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}

function DeleteSlideImage(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/selleradmin/vshop/DeleteSlideImage', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initSlideImagesTable();
                }, 1500);
            }
            else
                $.dialog.tips('删除失败！' + result.msg);
        });
    });
}

function SaveSlideImage(slideId) {
    $.post('./GetSlideImage', { id: slideId }, function (result) {
        if (result.success) {
            $.dialog({
                title: '轮播图',
                lock: true,
                id: 'addSlideImage',
                content: $("#addSlideImage").html(),
                padding: '0 40px',
                width:'500px',
                init: function () {
                    $("#imgUrl").himallUpload({
                        title: '轮播图片',
                        imageDescript: '640 * 320',
                        displayImgSrc: result.item.ImageUrl,
                        imgFieldName: "imgUrl",
                        dataWidth: 8

                    });
                    $("#menuUrl").val(result.item.Url);
                },
                button: [
                {
                    name: '确认',
                    callback: function () {
                        var photo = $('#imgUrl').himallUpload('getImgSrc');
                        if (photo == "") {
                            $.dialog.tips("请上传轮播图");
                            return false;
                        }
                        if ($('.table.table-bordered.slideImage').find('tr').length >= 6 && slideId == null) {
                            $.dialog.tips("最多只能上传5张轮播图");
                            return false;
                        }
                        AddSlideImage(slideId, photo, $("#menuUrl").val());
                    },
                    focus: true
                },
                {
                    name: '取消',
                }]
            });
        }
        else
            $.dialog.errorTips(result.msg);
    });
}

function AddSlideImage(slideId, imageUrl, url) {
    var loading = showLoading();
    $.post('./SaveSlideImage', { id: slideId, imageUrl: imageUrl, url: url }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("添加成功！", function () {
                initSlideImagesTable();
                //$('.page-tab-hd li').eq(1).click();
            });

        }
        else
            $.dialog.errorTips("添加失败" + result.msg);
    });
}

function changeSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/selleradmin/vshop/SlideImageChangeSequence',
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


function initVShopBannerTable() {
    //商品表格
    $("#vshopBannerTable").hiMallDatagrid({
        url: '/selleradmin/VShop/GetVShopBanners',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何导航数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "name", title: '导航名称', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<span width="100" height="24">' + value + '<span/>';
                    return html;
                }
            },
            {
                field: "url", title: '跳转地址', align: "center", width: 180, formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:180px">' + value + '</span> ';
                },
            },
            {
                field: "displaySequence", title: '排序', align: "center", formatter: function (value, row, index) {
                    return '<span class="glyphicon glyphicon-circle-arrow-up" rownumber="' + row.id + '" rowIndex="' + index + '"></span> <span class="glyphicon glyphicon-circle-arrow-down" rownumber="' + row.id + '" rowIndex="' + index + '"></span>';
                }
            },
            {
                field: "id", title: '操作', align: "center", formatter: function (value, row, index) {
                    return ' <span class="btn-a">\
                                    <a class="good-check" onclick="SaveVShopBanner(' + value + ')">编辑</a>\
                                    <a class="good-check" onclick="DeleteVShopBanner(' + value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#vshopBannerTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#vshopBannerTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}

function changeBannerSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: './BannerChangeSequence',
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

function SaveVShopBanner(id) {
    $.post('./GetVShopBanner', { id: id }, function (result) {
        if (result.success) {
            $.dialog({
                title: '导航',
                lock: true,
                id: 'addBanner',
                content: $("#addBanner").html(),
                padding: '0 40px',
                init: function () {
                    $("#ddlType1").val(result.item.UrlType);
                    if ($("#ddlType1").val() == 0)
                        $('.form-group.linkUrl').show();
                    $("#txtBannerName1").val(result.item.Name)

                    $("#bannerUrl").val(result.item.Url);
                },
                button: [
                {
                    name: '确认',
                    callback: function () {
                        var navname = $("#txtBannerName1").val();
                        var navlink = $("#bannerUrl").val();
                        if (navname.length < 1) {
                            $.dialog.errorTips("请填写导航名称");
                            return false;
                        }
                        if (navname.length > 10) {
                            $.dialog.errorTips("导航名称最多十个字符");
                            return false;
                        }
                        if ($("#ddlType1").find("option:selected").val() == 0) {
                            if (navlink.length < 1) {
                                $.dialog.errorTips("请填写导航链接");
                                return false;
                            }
                        }
                        if ($('.table.table-bordered.vshopbanner').find("tr").length >= 6 && id == null) {
                            $.dialog.errorTips("最多只能添加5张导航");
                            return false;
                        }
                        AddVShopBanner(id, navname, navlink, $("#ddlType1").val());
                    },
                    focus: true
                },
                {
                    name: '取消',
                }]
            });
        }
        else
            $.dialog.errorTips(result.msg);
    });
}

function DeleteVShopBanner(bannerId) {
    var loading = showLoading();
    $.post('./DeleteVShopBanner', { id: bannerId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips("删除成功");
            setTimeout(function () {
                initVShopBannerTable();
            }, 1500);
        }
        else
            $.dialog.tips('删除失败！' + result.msg);
    });
}

function AddVShopBanner(bannerId, name, url, urlType) {
    var loading = showLoading();
    $.post('./SaveVShopBanner', { id: bannerId, bannerName: name, url: url, urlType: urlType }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("添加成功！");
            initVShopBannerTable();
        }
        else
            $.dialog.errorTips("添加失败" + result.msg);
    });
}

function Onchange(obj) {
    if (obj.val() == "0") {
        obj.parents('.dialog-form').find(".linkUrl").show();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val("http://");
    }

    else {
        obj.parents('.dialog-form').find(".linkUrl").hide();
        obj.parents('.dialog-form').find(".linkUrl").find('input').eq(0).val("");
    }
}