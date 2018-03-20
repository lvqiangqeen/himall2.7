$(function () {
    query();
    $("#searchBtn").click(function () {
        query();
    });
})

function deleteComment(id) {
    $.dialog.confirm('确定删除该评论吗？', function () {
        var loading = showLoading();
        $.post("./Delete", { id: id }, function (data) { $.dialog.tips(data.msg); query(); loading.close(); });
    });
}

function ShowStatus(obj) {
    $(".container ul li").removeClass("active");
    $(obj).parent().attr("class", "active");
    query();
}




function query() {
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: { isReply: $("#notReply").parent().attr("class") == "active" ? "false" : "", productName: $("#txtProductName").val(), hasAppend: $("#hasAppend").prop("checked"), Rank: $("#selmark").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            {
                field: "ProductName", title: '评价商品', align: "left", width: 250,
                formatter: function (value, row, index) {
                    var spc = " ";
                    //if (row.Color != null && row.Color.length > 0) { spc += "颜色：" + row.Color; }
                    //if (row.Size != null && row.Size.length > 0) { spc += "，尺寸：" + row.Size; }
                    //if (row.Version != null && row.Version.length > 0) { spc += "，版本：" + row.Version; }
                    if (row.Color != null && row.Color.length > 0) { spc += row.ColorAlias + "：" + row.Color; }
                    if (row.Size != null && row.Size.length > 0) { spc += "，" + row.SizeAlias + "：" + row.Size; }
                    if (row.Version != null && row.Version.length > 0) { spc += "，" + row.VersionAlias + "：" + row.Version; }
                    if (spc != " ") { spc = "【" + spc + "】" }
                    var html = '<a title="' + value + spc + '" href="/product/detail/' + row.ProductId + '" target="_blank" href="/product/detail/' + row.ProductId + '"><img class="ml15" width="40" height="40" src="' + row.ImagePath + '" /><span class="single-ellipsis w150 lh40">' + value + '</a></span>';
                    return html;
                }
            },
           {
               field: "CommentContent", title: '评价内容', align: "center", width: 260,
               formatter: function (value, row, index) {
                   var html = '<p><span  title="' + value + '" class="">初次评价：' + value + '</span></p>';
                   if (row.AppendDate != null) {
                       html += '<br><p><span title="' + row.AppendContent + '" class="">追加评价：' + row.AppendContent + '</span></p>';
                   }
                   return html;
               }
           },
            { field: "CommentMark", title: '商品评分', align: "center", width: 100 },
           //{ field: "UserName", title: '评价人' },
            { field: "Date", title: '初评日期', width: 100, },
             { field: "AppendDateStr", title: '追评日期', align: "center", width: 100 },
            {
                field: "state", title: '状态', width: 100,
                formatter: function (value, row, index) {
                    var html = "";
                    if (row.Status)
                        html += '已回复';
                    else
                        html += '未回复';
                    return html;
                }
            },
        {
            field: "operation", operation: true, title: "操作", width: 100,
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var html = ["<span class=\"btn-a\">"];
                if (row.Status) {
                    html.push("<a onclick=\"detail('" + id + "');\">查看回复</a>");
                }
                if (!row.IsHidden) {
                    html.push("<a onclick=\"deleteComment('" + id + "');\">清空评价</a>");
                }
                else {
                    html.push("<a>评价已清除</a>");
                }
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}
function detail(id) {


    $.ajax({
        url: "GetComment",
        data: { id: id },
        async: false,
        success: function (data) {
            $("#reply-form").html(data);
        }
    });

    $.dialog({
        title: '查看回复',
        lock: true,
        id: 'consultReply',
        width: '500px',
        content: document.getElementById("reply-form"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
        }
    });

    //$.post("./Detail", { id: id }, function (data) {

    //});

}