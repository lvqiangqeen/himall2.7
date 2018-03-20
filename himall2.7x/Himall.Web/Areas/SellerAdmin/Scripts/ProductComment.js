$(function () {
    Init();
    $("#searchBtn").click(function () {
        Init();
    });
    $("#replyContent").blur(function () {
        var content = $("#replyContent").val();
        if (content.length > 300 || !content) {
            $("#commentCotentTip").text("回复内容在200个字符以内！");
            $("#replyContent").css({ border: '1px solid #f60' });
            return false;
        }
        else {
            $("#commentCotentTip").text("");
            $("#replyContent").css({ border: '1px solid #ccc' });
        }
    })
});

function Init() {
    var tips = $(window.parent.document).find('#UnReplyComments').text().replace('(', '').replace(')', '');
    if (tips && tips > 0) {
        query('false')
    }
    var status = GetQueryString('status');
    if (status && status > 0) {
        query('false');
    }
    else {
        query('');
    }
}


function ReplyComment(id) {

    $.ajax({
        url: "GetComment",
        data: { id: id },
        async: false,
        cache: false,
        success: function (data) {
            $("#reply-form").html(data);
        }
    });
    var replyContent = $("#replyContent");
    var appendContent = $("#appendContent");
    var title = "";
    if (replyContent.length>0|| appendContent.length>0) {
        title = "回复评论";
    }
    else {
        title = "查看评论";
    }
    $.dialog({
        title: title,
        lock: true,
        id: 'ReplyComment',
        content: document.getElementById("reply-form"),
        padding: '0 40px',
        okVal: '确定',
        width:'600px',
        init: function () {},//$("#replyContent").focus(); },
        ok: function () {
            //var loading = showLoading();
            if (replyContent.length > 0 && appendContent.length==0) {
                if (replyContent.val() == "" || replyContent.val().length > 200) {
                    $("#commentCotentTip").text("回复内容在200个字符以内！");
                    $("#replyContent").css({ border: '1px solid #f60' });
                    return false;
                }
            }
            if (appendContent.length>0) {
                if (appendContent.val() == "" || appendContent.val().length > 200) {
                    $("#AppendcommentCotentTip").text("回复内容在200个字符以内！");
                    $("#appendContent").css({ border: '1px solid #f60' });
                    return false;
                }
            }
            if (replyContent.length==0 && appendContent.length == 0)
            {
                return true;
            }
            var loading = showLoading();
            $.post("./ReplyComment",
                { id: id, replycontent: replyContent.val(), appendContent: appendContent.val() },
                function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips("回复成功", function () {
                            $("#replyContent").val("");
                            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                        });
                    }
                    else
                        $.dialog.errorTips("回复失败:" + data.msg);
                });
        }
    });
}

function query(val) {
    $('.nav-tabs-custom li').each(function () {
        if ($(this).attr('flag') == val) {
            $(this).addClass('active').siblings().removeClass('active');
        }
    });
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
        queryParams: { isReply: val, productName: $("#txtProductName").val(), hasAppend: $("#hasAppend").prop("checked"), Rank: $("#selmark").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            {
                field: "OrderId", title: '订单号', align: "center", width: 100,
                formatter: function (value, row, index) {
                    var html = '<a href="/SellerAdmin/order/Detail/' + value + '" >' + value + '</a>';
                    return html;
                }
            },
            {
                field: "ProductName", title: '评价商品', align: "left", width: 180,
                formatter: function (value, row, index) {
                    var spc = " ";
                    if (row.Color != null && row.Color.length > 0) { spc += row.ColorAlias + "：" + row.Color; }
                    if (row.Size != null && row.Size.length > 0) { spc += "，" + row.SizeAlias + "：" + row.Size; }
                    if (row.Version != null && row.Version.length > 0) { spc += "，" + row.VersionAlias + "：" + row.Version; }
                    var html = '<a title="' + value + "【" + spc + '】" href="/product/detail/' + row.ProductId + '" target="_blank" href="/product/detail/' + row.ProductId + '"><img class="ml15" width="40" height="40" src="' + row.ImagePath + '" /><span class="single-ellipsis w90 lh40">' + value + '</a></span>';
                    return html;
                }
            },
            { field: "UserName", title: '会员账号', width: 80 },
            { field: "UserPhone", title: '会员手机号', width: 80 },
            {
                field: "CommentContent", title: '评价内容', align: "center", width: 150,
                formatter: function (value, row, index) {
                    var html = '<p><span  title="' + value + '" class="">初次评价：' + value + '</span></p>';
                    if (row.AppendDate != null) {
                        html += '<br><p><span  title="' + row.AppendContent + '" class="">追加评价：' + row.AppendContent + '</span></p>';
                    }
                    return html;
                }
            },
            { field: "CommentMark", title: '商品评分',align: "center", width:70 },
            { field: "CommentDateStr", title: '初评日期', width: 80 },
             { field: "AppendDateStr", title: '追评日期', width: 80 },

            {
                field: "state", title: '状态', align: "center", width:60,
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
            field: "operation", operation: true, title: "操作", align: "center", width:70,
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var html = ["<span class=\"btn-a\">"];
                if (row.Status) {
                    html.push("<a onclick=\"ReplyComment('" + id + "');\">查看回复</a>");
                }
                else
                    html.push("<a onclick=\"ReplyComment('" + id + "');\">回复</a>");
                html.push("</span>");
                return html.join("");
            }
        },
       { field: "ReplyContent", hidden: true },

        ]]
    });
}
function detail(id) {
    $.post("./Detail", { id: id }, function (data) {
        var content = data.ConsulationContent == "" ? "无" : data.ConsulationContent;
        $.dialog({
            title: '查看回复',
            lock: true,
            id: 'consultReply',
            width: '500px',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<label class="label-inline fl">评论</label>',
                    '<p class="only-text">' + content + '</p>',
                '</div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">评论回复</label>',
                    '<p class="only-text">' + data.ReplyContent + '</p>',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            okVal: '确定',
            ok: function () {
            }
        });
    });

};

$(function () {

    $(document).on("click", ".after-service-img img", function () {
        $(".preview-img").show();
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(".cover").show();
    });
    $(".preview-img").click(function () {
        $(this).hide()
        $(".cover").hide();
    });
    $(".cover").click(function () {
        $(".preview-img").hide();
        $(".cover").hide();
    })
});
