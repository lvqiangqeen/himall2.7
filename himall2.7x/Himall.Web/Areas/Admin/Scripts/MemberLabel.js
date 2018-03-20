
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
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});
    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
    $('#searchButton').click(GetData);
    GetData();
});

function GetData()
{
    $(function () {
        var labelname = $('#txtLabelName').val();
        $("#list").hiMallDatagrid({
            url: './list',
            singleSelect: true,
            pagination: true,
            NoDataMsg: '没有找到符合条件的数据',
            idField: "Id",
            pageSize: 15,
            pageNumber: 1,
            queryParams: { keywords: labelname },
            columns:
            [[
                { field: "LabelName", title: '标签名称', width: 300, align: "left" },
                { field: "MemberNum", title: "会员数", width: 200, align: "center" },
                {
                    field: "operation", operation: true, title: "操作", width: 200,
                    formatter: function (value, row, index) {
                        var html = [];
                        html = ["<span class=\"btn-a\">"];
                        html.push("<a onclick=\"editLabel('" + row.Id + "');\">编辑</a>");
                        html.push("</span>");
                        if (row.MemberNum == 0) {
                            html.push("<span class=\"btn-a\">");
                            html.push("<a onclick=\"deleteLabel('" + row.Id + "');\">删除</a>");
                            html.push("</span>");
                        }
                        return html.join("");
                    }
                }
            ]]
        });
    });
}
function deleteLabel(id)
{
    $.ajax({
        type: 'post',
        url: 'deleteLabel',
        data: { Id: id },
        dataType: 'json',

        success: function (data) {
            if (data.Success)
            {
                $.dialog.alert('删除成功！', function () {

                    GetData();
                });
            }
            else {
                $.dialog.alert(data.Msg, function () {
                    
                });
            }
        }
    });
}
function editLabel(id) {
    location.href = 'Label?Id=' + id;
}

//$(function(){
    //$(". aui_main .aui_content").css("padding","30px 60px!important");
//})