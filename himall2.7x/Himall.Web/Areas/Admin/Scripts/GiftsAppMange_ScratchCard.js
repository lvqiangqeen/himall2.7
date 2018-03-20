var isscratchcardloaded = false;
var curscratchcardid = 0;

$(function () {
    $("#bt_csearch").click(function () {
        ReloadScratchCardTable();
    });
    $("#cactname").keydown(function (e) {
        var keyCode = e.keyCode;
        if (keyCode == 13) {
            ReloadScratchCardTable();
            return false;
        }
    });
    curscratchcardid = $("#ScratchCardId").val();
});

function ReloadScratchCardTable()
{
    var tname = $("#cactname").val();
    $("#ScratchCardTable").hiMallDatagrid("reload", { name: tname });
}
function SelectScratchCard(id) {
    if (id == curscratchcardid) {
        $.dialog.confirm('确定取消这个活动的广告展示？', function () {
            PostSelectScratchCard(id);
            $("#ScratchCardId").val(0);
            ReloadScratchCardTable();
        });
    } else {
        PostSelectScratchCard(id);
        $("#ScratchCardId").val(id);
        ReloadScratchCardTable();
    }
}

function PostSelectScratchCard(id) {
    var curcadtype = 1;
    var loading = showLoading();
    $.post('/admin/Gift/SelectAppAds', { id: id, adtype: curcadtype }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功");
            setTimeout(function () {
                initSlideImagesTable();
            }, 1500);
        }
        else
            $.dialog.errorTips('操作失败！' + result.msg);
    });
}

function initScratchCardTable() {
    var loading = showLoading();
    $("#ScratchCardTable").hiMallDatagrid({
        url: '/admin/Gift/GetScratchCardList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何刮刮卡活动',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "activityTitle", title: '活动名称', align: "left", formatter: function (value, row, index) {
                    var html = '<img width="50" src="' + row.activityUrl + '" />';
                    html += value;
                    return html;
                },
            },
            {
                field: "consumePoint", title: '积分', align: "center", width: 120
            },
            {
                field: "Id", title: '操作', width: 200, align: "center", formatter: function (value, row, index) {
                    html = ' <span class="btn-a">';
                    curscratchcardid = $("#ScratchCardId").val();
                    if (value == curscratchcardid) {
                        html += '<a class="good-check checked" onclick="SelectScratchCard(' + value + ')">取消</a>';
                    } else {
                        html += '<a class="good-check" onclick="SelectScratchCard(' + value + ')">选择</a>';
                    }
                    html += '</span>';
                    return html;
                }
            }
        ]],
        onLoadSuccess: function () {
            isscratchcardloaded = true;
            loading.close();
        }
    });
}
