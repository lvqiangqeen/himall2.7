var isrouletteloaded = false;
var currouletteid = 0;

$(function () {
    $("#bt_rsearch").click(function () {
        ReloadRoulette();
    });
    $("#ractname").keydown(function (e) {
        var keyCode = e.keyCode;
        if (keyCode == 13) {
            ReloadRoulette();
            return false;
        }
    });
    currouletteid = $("#RouletteId").val();
});

function ReloadRoulette() {
    var tname = $("#ractname").val();
    $("#RouletteTable").hiMallDatagrid("reload", { name: tname });
}

function SelectRoulette(id) {
    var ispost = false;
    if (id == currouletteid) {
        $.dialog.confirm('确定取消这个活动的广告展示？', function () {
            PostSelectRoulette(id);
            $("#RouletteId").val(0);
            ReloadRoulette();
        });
    } else {
        PostSelectRoulette(id);
        $("#RouletteId").val(id);
        ReloadRoulette();
    }
}

function PostSelectRoulette(id) {
    var curradtype = 2;
    var loading = showLoading();
    $.post('/admin/Gift/SelectAppAds', { id: id, adtype: curradtype }, function (result) {
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

function initRouletteTable() {
    var loading = showLoading();
    $("#RouletteTable").hiMallDatagrid({
        url: '/admin/Gift/GetRouletteList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何大转盘活动',
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
                    currouletteid = $("#RouletteId").val();
                    if (value != currouletteid) {
                        html += '<a class="good-check" onclick="SelectRoulette(' + value + ')">选择</a>';
                    } else {
                        html += '<a class="good-check checked" onclick="SelectRoulette(' + value + ')">取消</a>';
                    }
                    html += '</span>';

                    return html;
                }
            }
        ]],
        onLoadSuccess: function () {
            isrouletteloaded = true;
            loading.close();
        }
    });
}
