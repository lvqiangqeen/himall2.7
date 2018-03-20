$(function () {
    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }
        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

    $("#list").hiMallDatagrid({
        url: $("#dataurl").val(),
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 20,
        pageNumber: 1,
        queryParams: {},
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "ActiveName", title: "活动名称", align: "left" },
            { field: "ShowActiveStatus", title: "活动状态", width: 100 },
            {
                field: "StartTime", title: "活动时间", width: 300, formatter: function (value, row, index) {
                    var html = time_string(value);
                    html += "至" + time_string(row.EndTime);
                    return html;
                }
            },
            {
                field: "ProductCount", title: "商品数", width: 100, formatter: function (value, row, index) {
                    var html = value;
                    if (value == -1) {
                        html = "全部";
                    }
                    return html;
                }
            },
            {
                field: "operation", title: "操作", width: 160, formatter: function (value, row, index) {
                    var id = row.Id;
                    var html = [""];

                    html.push("<span class=\"btn-a\"><a class=\"bt-edit\"  href=\"/SellerAdmin/FullDiscount/EditActive/" + id + "\">编辑</a></span>");

                    html.push("<span class=\"btn-a\"><a class=\"bt-del\" href=\"javascript:void(0);\" data-id=\"" + id + "\">删除</a></span>");
                    return html.join("");
                }
            },
        ]]
    });

    //链接
    $("#list").on("click", ".bt-del", function () {
        var _t = $(this);
        var id = _t.data("id");
        var url = $("#delurl").val();

        $.dialog({
            title: '删除活动',
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<p style="padding:0">删除后该活动相关的商品将不享受优惠，是否确认删除？</p>',
                '</div>',
            '</div>'].join(''),
            padding: '20px 60px',
            button: [
            {
                name: '确认',
                callback: function () {
                    var loading = showLoading();
                    $.post(url, { id: id }, function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("操作成功！");
                            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                        }
                        else
                            $.dialog.errorTips("操作失败");
                    });
                },
                focus: true
            }, {
                name: "取消"
            }]
        });
    });

    //下架原因
    $("#list").on("click", ".bt-saltoutdes", function () {
        var _t = $(this);
        var mdes = _t.data("mdes");
        var mtime = _t.data("mtime");
        dlgcontent = ['<div class="dialog-form">'];
        dlgcontent = dlgcontent.concat([
                '<div class="form-group">',
            '<label class="label-inline fl">平台下架原因</label>',
            '<p class="only-text">' + mdes + '</p>',
                '</div>',
                '<div class="form-group">',
            '<label class="label-inline fl">平台操作时间</label>',
            '<p class="only-text">' + mtime + '</p>',
                '</div>'
        ]);

        var dlgbt = [{
            name: '关闭',
            focus: true
        }];

        $.dialog({
            title: '下架原因查看',
            lock: true,
            id: 'saltoutdesdlg',
            width: '500px',
            content: dlgcontent.join(''),
            padding: '0 40px',
            button: dlgbt
        });

    });

    $('#searchButton').click(function (e) {
        searchClose(e);
        ReloadGrid(1);
    });

});

//重载数据
function ReloadGrid(page) {
    var pageNo = page || $("#list").hiMallDatagrid('options').pageNumber;
    var activename = $.trim($('#txtactivename').val());
    activename = stripscript(activename);
    var actstatus = $('#selactivestatus').val();
    var startTime = $('#txtStartDate').val();
    var endTime = $('#txtEndDate').val();
    $("#list").hiMallDatagrid('reload',
        {
            activeName: activename,
            status: actstatus,
            startTime: startTime,
            endTime: endTime,
            pageNumber: pageNo
        });
}
//过滤非法字符  
function stripscript(s) {
    var pattern = new RegExp("[`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]")
    var rs = "";
    for (var i = 0; i < s.length; i++) {
        rs = rs + s.substr(i, 1).replace(pattern, '');
    }
    return rs;
}