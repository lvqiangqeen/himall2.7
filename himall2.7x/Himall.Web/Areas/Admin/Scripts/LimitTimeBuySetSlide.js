// JavaScript source code


function editSlideEvent(url, title, id, pic, action) {
    Loading(id, title, pic, action, url);
}

function Loading(id, title, pic, action, url) {
    $.dialog({
        title: title,
        lock: true,
        id: 'addHandImg',
        padding: '0 40px',
        content: ['<div class="dialog-form">',
            '<div id="slidePic" class="form-group upload-img clearfix">',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl" for="">跳转链接</label>',
                '<input class="form-control input-sm" type="text" id="url">',
            '</div>',
        '</div>'].join(''),
        okVal: '保存',
        init: function () {
            $("#slidePic").himallUpload(
            {
                title: '请上传图片',
                imageDescript: '1920*450',
                displayImgSrc: pic.replace(/\|/g, '/'),
                imgFieldName: "slidePic",
                dataWidth: 8
            });
            if (action == 'EditSlideAd') {
                $("#url").val(url);
            }
            $("#url").focus();
        },
        ok: function () {

            var url = $("#url").val();
            var pic = $("#slidePic").himallUpload('getImgSrc');
            if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }
            if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }

            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: './' + action,
                cache: false,
                param: { url: url, pic: pic, id: id },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.successful == true) {
                        location.href = './SetSlide';
                    }
                },
                error: function (data) {
                    loading.close(); $.dialog.errorTips('操作失败,请稍候尝试.');
                }
            });

        }
    });
}

function deleteSlideEvent(obj, id) {
    $.dialog.confirm('您确定要删除吗？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: './DeleteSlide',
            cache: false,
            param: { Id: id },
            dataType: 'json',
            success: function (data) {
                loading.close();
                if (data.successful == true) {
                    $(obj).parents('tr').remove();
                    $.dialog.succeedTips('删除成功.', null, 0.5);
                }
            },
            error: function (data) {
                loading.close();
                $.dialog.tips('删除失败,请稍候尝试.', null, 0.5);
            }
        });
    });
}

$(function () {
    var addSlideContentHtml = '';



    function LoadTable() {


        $("#slideDatagrid").hiMallDatagrid({
            url: "./GetSlideJson",
            singleSelect: true,
            pagination: false,
            NoDataMsg: '没有找到符合条件的数据',
            idField: "Id",
            pageSize: 15,
            pageNumber: 1,
            queryParams: {},
            columns:
            [[

                { field: "Id", title: "Id", hidden: true },
                {
                    field: "Pic", title: "图片", width: 200,
                    formatter: function (value, row, index) {
                        var html = '<img style="margin-left:15px;" width="150" height="50" src="' + row.Pic + '" />';
                        return html;
                    }
                },
                { field: "URL", title: "跳转地址", width: 400 },
                {
                    field: "DisplaySequence", title: "排序", width: 120,
                    formatter: function (value, row, index) {
                        return '<span class="glyphicon glyphicon-circle-arrow-up"></span> <span class="glyphicon glyphicon-circle-arrow-down"></span>';
                    }
                },
                {
                    field: "operation", operation: true, title: "操作",
                    formatter: function (value, row, index) {
                        var id = row.Id.toString();
                        var html = ['<span class="btn-a">'];
                        html.push('<a onclick="editSlideEvent(\'' + row.URL + '\',\'编辑轮播图片\',' + id + ',\'' + row.Pic.replace(/\//g, '|') + '\',\'EditSlideAd\');">编辑</a>');
                        html.push('<a class="a-del" onclick="deleteSlideEvent(this,' + id + ');">删除</a>');
                        html.push("</span>");
                        return html.join("");
                    }
                }
            ]],
            onLoadSuccess: function () {
                InitialArrowBtn();
            }
        });

    };

    function InitialArrowBtn() {
        $(".table tbody tr").find('.glyphicon').removeClass('disabled');
        $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
        $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    }

    $(function () {

        LoadTable();

        $('#AddSlide').click(function () {
            if ($("#slideDatagrid tbody tr").length >= 6) { $.dialog.errorTips("只能添加6张轮播图片."); return; }
            Loading(0, '新增轮播图', '', 'AddSlideAd', '');
        });


        //排序


        $(".table").on("click", '.glyphicon-circle-arrow-up', function () {
            var that = this;
            var id = $("#slideDatagrid").hiMallDatagrid('getRowByIndex', $(this).parents('tr').attr('himalldatagrid-row-index')).Id;
            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: './AdjustSlideIndex',
                cache: false,
                param: { id: id, direction: 1 },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.successful == true) {
                        var p = $(that).parents('tr');
                        var index = p.parent().find('tr').index(p);
                        if (index == 0)
                            return false;
                        else {
                            p.prev().before(p);
                            $.dialog.succeedTips('调整顺序成功.', null, 0.3);
                            InitialArrowBtn();
                        }
                    }
                },
                error: function (data) {
                    loading.close(); $.dialog.errorTips('调整顺序失败,请稍候尝试.');
                }
            });

        });

        $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
            var that = this;
            var id = $("#slideDatagrid").hiMallDatagrid('getRowByIndex', $(this).parents('tr').attr('himalldatagrid-row-index')).Id;
            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: './AdjustSlideIndex',
                cache: false,
                param: { id: id, direction: 0 },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.successful == true) {
                        var p = $(that).parents('tr');
                        var count = p.parent().find('tr').length;
                        var index = p.parent().find('tr').index(p);
                        if (index == (count - 1))
                            return false;
                        else {
                            p.next().after(p);
                            $.dialog.succeedTips('调整顺序成功.', null, 0.3);
                            InitialArrowBtn();
                        }
                    }
                },
                error: function (data) {
                    loading.close(); $.dialog.errorTips('调整顺序失败,请稍候尝试.');
                }
            });
        });

        $(".table").on("click", '.glyphicon', function () {
            $(this).parents('tbody').find('.glyphicon').removeClass('disabled');
            $(this).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $(this).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        });


    });

})