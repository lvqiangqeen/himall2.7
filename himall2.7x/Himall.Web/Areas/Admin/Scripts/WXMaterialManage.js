$(function () {
    GetMaterialData();
    $('#btnPre').click(function () {
        if (pageIdx > 1) {
            pageIdx = pageIdx - 1;
            GetMaterialData();
        }
    });
    $('#btnNext').click(function () {
        if (pageIdx < pageTotal) {
            pageIdx = pageIdx + 1;
            GetMaterialData();
        }
    });
});
var pageTotal = 0;
var pageIdx = 1;
var pageSize = 8;
function GetMaterialData()
{
    $('#pageNav').hide();
    $.post('GetWXMaterialList', { pageIdx: pageIdx, pageSize: pageSize }, function (data) {
        var returnCode=data.errCode||'0';
        if (data.msg)
        {
            $('#list').append('<li class="con-frame"><div class="source-l"><span>' + data.msg + '</span></div></li>');
        }
        else {
            if (data.errMsg || data.count == 0) {
                if (data.count == 0)
                    $('#list').append('<li class="con-frame text-center"><h2 class="mt0 mb0" style="font-size: 18px;line-height:104px;color:#8e8f92;">没有找到符合条件的数据</h2></li>');
                else {
                    $('#list').append('<li class="con-frame"><span>' + data.errMsg + '</span></li>');
                }
            }
            else {
                $('#pageNav').show();
                var html = [], lihtml = [], mediaid = '';
                $('#list').html('');
                pageTotal = Math.ceil(data.total_count / pageSize);
                $('#totalCnt').text(data.total_count);
                $(data.content).each(function (idx, el) {
                    
                    lihtml = [];
                    mediaid = '';
                    $(el.items).each(function (i, item) {
                        if (mediaid == '')
                            mediaid = item.thumb_media_id;
                        lihtml.push('<li>' + item.title + '</li>');
                    });
                    html.push('<li class="con-frame">');
                    html.push('<div class="source-l">');
                    html.push('<span><img src="GetMedia?mediaid=' + mediaid + '"></span>');
                    html.push('<ol>');
                    html.push(lihtml.join(''));
                    html.push('</ol>');
                    html.push('</div>');
                    html.push('<div class="source-M"><time>' + el.update_time + '</time></div>');
                    html.push('<div class="source-R"><a href="WXMsgTemplate?mediaid=' + el.media_id + '">编辑</a><a onclick="DeleteMaterial(\'' + el.media_id + '\')">删除</a></div>');
                    html.push('</li>');
                    $('#list').append(html.join(''));
                    html = [];
                });
            }
        }
    });
}
function DeleteMaterial(mediaid)
{
    $.dialog.confirm('确定删除选择的素材吗？', function () {
        var loading = showLoading();
        $.post("./DeleteMedia", { mediaid: mediaid }, function (data) {
            if (data.success) {
                $.dialog.tips("删除成功");
                GetMaterialData();
                loading.close();
            }
            else {
                $.dialog.tips(data.msg);
            }
        });
    });
}