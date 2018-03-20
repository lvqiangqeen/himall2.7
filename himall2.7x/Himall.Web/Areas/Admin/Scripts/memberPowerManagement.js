$(function () {
    var ulHtml = '<li>' + GetTip('StatisticsType') + '</li>'
    $('#ulTitle').append(ulHtml);
    query();
});
//获取url中的参数
function GetUrlParam(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
    var r = window.location.search.substr(1).match(reg);  //匹配目标参数
    if (r != null) return unescape(r[2]); return ''; //返回参数值
}
function GetTip(name) {
    var tip = '';
    switch (GetUrlParam(name)) {
        case 'ActiveOne':
            tip = '1个月活跃会员'; break;
        case 'ActiveThree':
            tip = '3个月活跃会员'; break;
        case 'ActiveSix':
            tip = '6个月活跃会员'; break;
        case 'SleepingThree':
            tip = '3个月沉睡会员'; break;
        case 'SleepingSix':
            tip = '6个月沉睡会员'; break;
        case 'SleepingNine':
            tip = '9个月沉睡会员'; break;
        case 'sleepingTwelve':
            tip = '12个月沉睡会员'; break;
        case 'sleepingTwentyFour':
            tip = '24个月沉睡会员'; break;
        case 'BirthdayToday':
            tip = '今天生日会员'; break;
        case 'BirthdayToMonth':
            tip = '当月生日会员'; break;
        case 'BirthdayNextMonth':
            tip = '次月生日会员'; break;
        case '0':
            tip = '1个月活跃会员'; break;
        case '1':
            tip = '3个月活跃会员'; break;
        case '2':
            tip = '6个月活跃会员'; break;
        case '10':
            tip = '3个月沉睡会员'; break;
        case '11':
            tip = '6个月沉睡会员'; break;
        case '12':
            tip = '9个月沉睡会员'; break;
        case '13':
            tip = '12个月沉睡会员'; break;
        case '14':
            tip = '24个月沉睡会员'; break;
        case '100':
            tip = '今天生日会员'; break;
        case '101':
            tip = '当月生日会员'; break;
        case '102':
            tip = '次月生日会员'; break;
        case '1000':
            tip = '注册会员'; break;

        default:
            tip = ''; break;
    }
    return tip;

}
$(function () {
    $(".StartTime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".EndTime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $("#Recently-st").on('changeDate', function () {
        if ($("#Recently-et").val()) {
            if ($("#Recently-st").val() > $("#Recently-et").val()) {
                $("#Recently-et").val($("#Recently-st").val());
            }
        }
        $("#Recently-et").datetimepicker('setStartDate', $("#Recently-st").val());
    });

    $('.StartTime,.EndTime').keydown(function (e) {
        e = e || window.event;
        var k = e.keyCode || e.which;
        if (k != 8 && k != 46) {
            return false;
        }
    });

});
$(function () {
//    var cat_h = $(".menber-factor .scategory .tab-cat").height();
//    if (cat_h > 50) {
//        $(".menber-factor .scategory .user-more").show()
//    };
//    var sla_h = $(".menber-factor .slable .tab-cat").height();
//    if (sla_h > 50) {
//        $(".menber-factor .slable .user-more").show()
//    };
//
//    $("#divSetLabel .form-group").css({ "width": "110px", "float": "left", "border": "none", "white-space": "nowrap", "overflow": "hidden", "margin": "10px" });
//
//
    $(".stime .tab-cat .user-defined i").click(function () {
		$(".stime span.defined-content").toggle()
		$(".snumber span.defined-content").css("display","none");
		$(".smoney  span.defined-content").css("display","none");
    })



    $(".snumber .tab-cat .user-defined i").click(function () {
        $(".snumber span.defined-content").toggle();
		$(".smoney  span.defined-content").css("display","none");
		$(".stime span.defined-content").css("display","none");
    });
    $(".smoney  .tab-cat .user-defined i").click(function () {
        $(".smoney  span.defined-content").toggle();
		$(".snumber span.defined-content").css("display","none");
		$(".stime span.defined-content").css("display","none");
		
    });
//
//    $(".menber-factor .scategory .user-more").toggle(function () {
//        $(".menber-factor .scategory").css("height", "auto")
//        $(".menber-factor .scategory .tab-cat").css("overflow-y", "auto")
//    }, function () {
//        $(".menber-factor .scategory").css("height", "50px")
//        $(".menber-factor .scategory .tab-cat").css("overflow-y", "hidden")
//    });
//    $(".menber-factor .slable .user-more").toggle(function () {
//        $(".menber-factor .slable").css("height", "auto")
//        $(".menber-factor .slable .tab-cat").css("overflow-y", "auto")
//    }, function () {
//        $(".menber-factor .slable").css("height", "50px")
//        $(".menber-factor .slable .tab-cat").css("overflow-y", "hidden")
//    });


    $(".sel .tab-cat a").click(function () {
        $(this).parent().parent().find("a").removeAttr("class");
        $(this).attr("class", "con");
        query();
    });

    $("#Recently-btn").click(function () {
        $(this).parent().hide();
        $(".menber-factor .stime .tab-cat").find("a").removeAttr("class");
        query();
    })

    $("#Purchases-S").on('change', function () {
        if ($("#Purchases-E").val()) {
            if ($("#Purchases-S").val() > $("#Purchases-E").val()) {
                $("#Purchases-E").val($("#Purchases-S").val());
            }
        }
    });
    $("#Purchases-btn").click(function () {
        $(this).parent().hide();
        $(".menber-factor .snumber .tab-cat").find("a").removeAttr("class");
        query();
    });
    $("#Amount-S").on('change', function () {
        if ($("#Amount-E").val()) {
            if ($("#Amount-S").val() > $("#Purchases-E").val()) {
                $("#Amount-E").val($("#Purchases-S").val());
            }
        }
    });
    $("#Amount-btn").click(function () {
        $(this).parent().hide();
        $(".menber-factor .smoney .tab-cat").find("a").removeAttr("class");
        query();
    })


});

var RecentlySpentTime, StartTime, EndTime, Purchases, StartPurchases, EndPurchases, CategoryId, AmountOfConsumption, StartAmountOfConsumption, EndAmountOfConsumption, LabelId, MemberStatisticsType, Sort, IsAsc;
var allIds = [];
function query() {
    queryParams();
    $("#list").hiMallDatagrid({
        url: './MemberPowerList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 20,
        pageNumber: 1,
        queryParams: {
            RecentlySpentTime: RecentlySpentTime, StartTime: StartTime, EndTime: EndTime, Purchases: Purchases, StartPurchases: StartPurchases, EndPurchases: EndPurchases, CategoryId: CategoryId, AmountOfConsumption: AmountOfConsumption, StartAmountOfConsumption: StartAmountOfConsumption, EndAmountOfConsumption: EndAmountOfConsumption, LabelId: LabelId, MemberStatisticsType: MemberStatisticsType
        },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { checkbox: true, width: 39 },
            { field: "id", hidden: true },
            { field: "userName", title: '会员名', width: 50 },
            //{
            //    field: "operation", title: '手机', width: 50,
            //    formatter: function (value, row, index) {
            //        var cellphone = row.cellPhone == undefined ? "" : row.cellPhone;
            //        var html = '<a onclick="showDetail(' + row.id + ')">' + cellphone + '</a>';
            //        return html;
            //    }

            //},
            {
                field: "cellPhone", title: '手机号', width: 150, formatter: function (value, row) {
                    return '<a href="/Admin/member/MemberDetail?id={0}">{1}</a>'.format(row.id, row.cellPhone);
                }
            },
            { field: "gradeName", title: '等级', width: 50 },
            {
                field: "netAmount", sort: true, title: '消费金额', width: 80, formatter: function (value, row, index) {
                    return "￥" + formatMoney(value);
                }
            },
            { field: "orderNumber", sort: true, title: '消费次数', width: 60 },
            { field: "lastConsumptionTime", sort: true, title: '最近消费时间', width: 80 },
            { field: "categoryNames", title: '购买商品类型', width: 80 }
        ]],
        onLoadSuccess: function () {
            allIds=($("#list").hiMallDatagrid('data').ids).split(',');
        }
    });
   
}
function showDetail(id) {
    location.href = "MemberDetail?id=" + id;
}
function queryParams() {
    RecentlySpentTime = null; StartTime = null; EndTime = null; Purchases = null; StartPurchases = null; EndPurchases = null; CategoryId = null; AmountOfConsumption = null; StartAmountOfConsumption = null; EndAmountOfConsumption = null; LabelId = null; MemberStatisticsType = null; Sort = null; IsAsc = null;
    //最近消费
    $(".stime a[class='con']").each(function (i, e) {
        var lid = $(e).attr("id").split('_')[1];
        $("#Recently-st").val("");
        $("#Recently-et").val("");
        if (parseInt(lid) > -1) {
            RecentlySpentTime = lid;
        }
    });

    if ($("#Recently-st").val() != "") {
        StartTime = $("#Recently-st").val();
    }
    if ($("#Recently-et").val() != "") {
        EndTime = $("#Recently-et").val();
    }


    //购买次数
    $(".snumber a[class='con']").each(function (i, e) {
        var lid = $(e).attr("id").split('_')[1];
        $("#Purchases-S").val("");
        $("#Purchases-E").val("");
        if (parseInt(lid) > -1) {
            Purchases = lid;
        }
    });


    if ($("#Purchases-S").val() != "") {
        StartPurchases = $("#Purchases-S").val();
    }
    if ($("#Purchases-E").val() != "") {
        EndPurchases = $("#Purchases-E").val();
    }

    //类目
    $(".scategory a[class='con']").each(function (i, e) {
        var lid = $(e).attr("id").split('_')[1];
        if (parseInt(lid) > -1) {
            CategoryId = lid;
        }
    });

    //消费金额
    $(".smoney a[class='con']").each(function (i, e) {
        var lid = $(e).attr("id").split('_')[1];
        $("#Amount-S").val("");
        $("#Amount-E").val("");
        if (parseInt(lid) > -1) {
            AmountOfConsumption = lid;
        }
    });

    if ($("#Amount-S").val() != "") {
        StartAmountOfConsumption = $("#Amount-S").val();
    }
    if ($("#Amount-E").val() != "") {
        EndAmountOfConsumption = $("#Amount-E").val();
    }
    //标签
    $(".slable a[class='con']").each(function (i, e) {
        var lid = $(e).attr("id").split('_')[1];
        if (parseInt(lid) > -1) {
            LabelId = lid;
        }
    });

    //购买力度
    if (statisticsType >= 0) {
        MemberStatisticsType = statisticsType;
    }
}

function batchAddLabel(id) {
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $.dialog({
        title: '会员标签',
        lock: true,
        id: 'SetLabel',
        width: '630px',
        content: document.getElementById("divSetLabel"),
        padding: '0 40px',
        okVal: '确定',
        init: function () {
            //加载当前会员标签
            $.post('GetMemberLabel', { id: id }, function (result) {
                if (result.Success) {
                    $('input[name=check_Label]').each(function (i, checkbox) {
                        for (var j = 0; j < result.Data.length; j++) {
                            if ($($(checkbox).get(0)).attr("datavalue") == result.Data[j].LabelId) {
                                $(checkbox).get(0).checked = true;
                            }
                        }
                    });
                }
            });
        },
        ok: function () {
            var labelids = [];
            $('input[name=check_Label]').each(function (i, checkbox) {
                if ($(checkbox).get(0).checked) {
                    labelids.push($(checkbox).attr('datavalue'));
                }
            });
            var loading = showLoading();
            $.post('SetMemberLabel', { id: id, labelids: labelids.join(',') }, function (result) {
                if (result.Success) {
                    query();
                    $.dialog.tips('设置成功！');
                }
                loading.close();
            });
        }
    });
}


function setLabel(type) {
    switch (type) {
        case "check": batchAddLabels(); break;
        case "result": batchAddLabelsAll(); break;
    }
}

function batchAddLabels() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.id);
    });
    console.log(ids);
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $.dialog({
        title: '会员标签',
        lock: true,
        id: 'SetLabel',
        width: '630px',
        content: document.getElementById("divSetLabel"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var labelids = [];
            $('input[name=check_Label]').each(function (i, checkbox) {
                if ($(checkbox).get(0).checked) {
                    labelids.push($(checkbox).attr('datavalue'));
                }
            });
            var loading = showLoading();
            $.post('SetMembersLabel', { ids: ids.join(','), labelids: labelids.join(',') }, function (result) {
                if (result.Success) {
                    query();
                    $.dialog.tips('设置成功！');
                }
                loading.close();
            });
        }
    });
}

function batchAddLabelsAll() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });
     
    console.log(allIds);
    $.dialog({
        title: '会员标签',
        lock: true,
        id: 'SetLabel',
        width: '630px',
        content: document.getElementById("divSetLabel"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var labelids = [];
            $('input[name=check_Label]').each(function (i, checkbox) {
                if ($(checkbox).get(0).checked) {
                    labelids.push($(checkbox).attr('datavalue'));
                }
            });
            var loading = showLoading();
            $.post('SetMembersLabel', { labelids: labelids.join(','), ids: allIds.join(',') }, function (result) {
                if (result.Success) {
                    query();
                    $.dialog.tips('设置成功！');
                }
                loading.close();
            });
        }
    });
}

function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function sendCoupon(type) {
    switch (type) {
        case "check": ChoceCoupon(); break;
        case "result": ChoceCouponAll(); break;
    }
}

function ChoceCoupon() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');

    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.id);
    });

    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    $("#couponName").val("");
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 650,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认赠送',
        ok: function () {
            if ($('#colist tr').length + $('input[name="topic"]:checked').length > 99) {
                $.dialog.tips('发送的优惠券总数不能超过99！');
                return false;
            }

            var selecteds = $("#list").hiMallDatagrid('getSelections');
            var ids = [];
            $.each(selecteds, function () {
                ids.push(this.id);
            });

            selecteds = $("#CouponGrid").hiMallDatagrid('getSelections');
            var couponIds = [];
            $.each(selecteds, function () {
                couponIds.push(this.id);
            });
            if (couponIds.length == 0) {
                $.dialog.tips('请选择优惠券！');
                return;
            }


            var loading = showLoading();
            $.post("SendCoupon", { ids: ids.join(','), couponIds: couponIds.join(',') }, function (data) {
                if (data.success) {
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            }, "json");
        }
    });

    bind();
}

function ChoceCouponAll() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');

    $("#couponName").val("");
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 650,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认赠送',
        ok: function () {
            if ($('#colist tr').length + $('input[name="topic"]:checked').length > 99) {
                $.dialog.tips('发送的优惠券总数不能超过99！');
                return false;
            }

            var selecteds = $("#list").hiMallDatagrid('getSelections');
            var ids = [];
            $.each(selecteds, function () {
                ids.push(this.id);
            });

            selecteds = $("#CouponGrid").hiMallDatagrid('getSelections');
            var couponIds = [];
            $.each(selecteds, function () {
                couponIds.push(this.id);
            });
            if (couponIds.length == 0) {
                $.dialog.tips('请选择优惠券！');
                return;
            }


            var loading = showLoading();
            $.post('SendAllCoupon', { couponIds: couponIds.join(','), RecentlySpentTime: RecentlySpentTime, StartTime: StartTime, EndTime: EndTime, Purchases: Purchases, StartPurchases: StartPurchases, EndPurchases: EndPurchases, CategoryId: CategoryId, AmountOfConsumption: AmountOfConsumption, StartAmountOfConsumption: StartAmountOfConsumption, EndAmountOfConsumption: EndAmountOfConsumption, LabelId: LabelId, MemberStatisticsType: MemberStatisticsType }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });

    bind();
}
function bind() {
    //优惠券表格
    $("#CouponGrid").hiMallDatagrid({
        url: 'CouponList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 5,
        pageNumber: 1,
        queryParams: { Name: $("#couponName").val(), PageNo: 1, pageSize: 5 },
        columns:
             [[
            { field: "couponName", title: "优惠券名称", align: "center" },
            { field: "shopName", title: "商家", align: "center" },
            { field: "price", title: "面额", align: "center" },
            {
                field: "inventory", title: "剩余数量", align: "center",
                formatter: function (value, row) {
                    return row.permax == 0 ? '不限张' : value;
                }
            },
            { field: "orderAmount", title: "使用条件", align: "center" },
            {
                field: "strEndTime", title: "有效期", width: 100, align: 'center',
                formatter: function (value, row) {
                    return row.strStartTime + '至' + row.strEndTime;
                }
            },
            { checkbox: true, width: 50 },
            { field: "id", hidden: true }
             ]]
    });
}

$("#btncoupon").click(function () {
    bind();
});



function sendSms(type) {
    switch (type) {
        case "check": ChoceSms(); break;
        case "result": ChoceSmsAll(); break;
    }
}

function ChoceSms() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.id);
    });
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $("#contentDesc1").val("");
    $.dialog({
        title: '发送短信',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendSms"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {

            if ($("#contentDesc1").val() == "") {
                $.dialog.tips('请输入发送短信内容！');
                return false;
            }

            var loading = showLoading();
            $.post('SendSms', { ids: ids.join(','), sendCon: $("#contentDesc1").val() }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.errorTips('发送失败！');
                }
                loading.close();
            });
        }
    });
}

function ChoceSmsAll() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });
    $("#contentDesc1").val("");
    $.dialog({
        title: '发送短信',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendSms"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            if ($("#contentDesc1").val() == "") {
                $.dialog.tips('请输入发送短信内容！');
                return false;
            }

            var loading = showLoading();
            $.post('SendAllSms', { sendCon: $("#contentDesc1").val(), RecentlySpentTime: RecentlySpentTime, StartTime: StartTime, EndTime: EndTime, Purchases: Purchases, StartPurchases: StartPurchases, EndPurchases: EndPurchases, CategoryId: CategoryId, AmountOfConsumption: AmountOfConsumption, StartAmountOfConsumption: StartAmountOfConsumption, EndAmountOfConsumption: EndAmountOfConsumption, LabelId: LabelId, MemberStatisticsType: MemberStatisticsType }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });
}



function sendWei(type) {
    switch (type) {
        case "check": ChoceWei(); break;
        case "result": ChoceWeiAll(); break;
    }
}

var pageTotal = 0;
var pageIdx = 1;
var pageSize = 8;
var curMedia = '';
var mediaList = {};

function ChoceWei() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.id);
    });
    if (ids.length == 0) {
        $.dialog.tips('请选择会员！');
        return;
    }
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });

    $.dialog({
        title: '选择图文素材',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendWei"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {

            var msgtype = $('#msgtype').children('.active').attr('value');
            var msgcontent = '';
            if (msgtype == 1) {
                msgcontent = $('#txtInput textarea').val();
                if (msgcontent == '') {
                    $.dialog.alert('发送内容不能为空');
                    return;
                }
                if (msgcontent.length > 600) {
                    $.dialog.alert('发送内容长度不能超过600');
                    return;
                }
            }
            else {
                if (curMedia == 'p') {
                    $.dialog.alert('请选择素材模板');
                    return;
                }
            }
            loading = showLoading('正在发送');
            $.post('SendWXGroupMessage', { ids: ids.join(','), msgtype: msgtype, mediaid: curMedia, msgcontent: msgcontent }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.alert('发送成功');
                }
                else {
                    $.dialog.alert(result.msg);
                }
            });
        }
    });
}

function ChoceWeiAll() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    //debugger
    $('input[name=check_Label]').each(function (i, checkbox) {
        $(checkbox).get(0).checked = false;
    });
    $.dialog({
        title: '选择图文素材',
        lock: true,
        id: 'SendSms',
        width: '630px',
        content: document.getElementById("divSendWei"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var msgtype = $('#msgtype').children('.active').attr('value');
            var msgcontent = '';
            if (msgtype == 1) {
                msgcontent = $('#txtInput textarea').val();
                if (msgcontent == '') {
                    $.dialog.alert('发送内容不能为空');
                    return;
                }
                if (msgcontent.length > 600) {
                    $.dialog.alert('发送内容长度不能超过600');
                    return;
                }
            }
            else {
                if (curMedia == 'p') {
                    $.dialog.alert('请选择素材模板');
                    return;
                }
            }

            var loading = showLoading();
            $.post('SendAllWXGroupMessage', { msgtype: msgtype, mediaid: curMedia, msgcontent: msgcontent, RecentlySpentTime: RecentlySpentTime, StartTime: StartTime, EndTime: EndTime, Purchases: Purchases, StartPurchases: StartPurchases, EndPurchases: EndPurchases, CategoryId: CategoryId, AmountOfConsumption: AmountOfConsumption, StartAmountOfConsumption: StartAmountOfConsumption, EndAmountOfConsumption: EndAmountOfConsumption, LabelId: LabelId, MemberStatisticsType: MemberStatisticsType }, function (result) {
                if (result.success) {
                    query();
                    $.dialog.tips('发送成功！');
                } else {
                    $.dialog.tips(result.msg);
                }
                loading.close();
            });
        }
    });
}


$('#msgtype_news').click(function () {
    $('#mediaSelect').show();
    $('#txtInput').hide();
    $('#msgtype_text').removeClass('active');
    $(this).addClass('active');
});
$('#msgtype_text').click(function () {
    $('#txtInput').show();
    $('#mediaSelect').hide();
    $('#msgtype_news').removeClass('active');
    $(this).addClass('active');
});


$(".tab-content .library").click(function () {
    $(".sucai-library").show();
    $(".coverage").show();
    GetMaterialData();
});

$(".sucai-library .glyphicon-remove").click(function () {
    $(".sucai-library").hide();
    $(".coverage").hide();
});
$('#btnCancel').click(function () {
    $(".sucai-library").hide();
    $(".coverage").hide();
});

function GetMaterialData() {
    $.post('GetWXMaterialList', { pageIdx: pageIdx, pageSize: pageSize }, function (data) {
        var returnCode = data.errCode || '0';
        if (data.msg) {
            $('#list').append('<li class="con-frame"><div class="source-l">' + data.msg + '</div></li>');
        }
        else {
            if (data.errMsg) {
                $('#list').append('<li class="con-frame">' + data.errMsg + '</li>');
            }
            else {
                var html = [], lihtml = [], mediaid = '';
                $('#list').html('');
                mediaList = data.content;
                $(data.content).each(function (idx, el) {
                    lihtml = [];
                    mediaid = '';
                    $(el.items).each(function (i, item) {
                        if (mediaid == '')
                            mediaid = item.thumb_media_id;
                        lihtml.push('<li>' + item.title + '</li>');
                    });
                    html.push("<li idx=" + idx + " id=\"" + el.media_id + "\" class=\"con-frame\"  onclick=\"selectMaterial('" + el.media_id + "')\">");
                    html.push('<div class="source-l">');
                    html.push('<span><img src="GetMedia?mediaid=' + mediaid + '"></span>');
                    html.push('<ol>');
                    html.push(lihtml.join(''));
                    html.push('</ol>');
                    html.push('</div>');
                    html.push('<div class="source-M"><time>' + el.update_time + '</time></div>');
                    html.push('<span class="SCover"></i></span>');
                    html.push('<i class="glyphicon glyphicon-ok">');
                    html.push('</li>');
                    $('#list').append(html.join(''));
                    html = [];
                });
                $(".con-frame").hover(function () {
                    $(".SCover", this).show();
                }, function () {
                    if ($('.glyphicon-ok', this).css('display') != 'block') {
                        $(".SCover", this).hide();
                    }
                });
                $(".con-frame").click(function () {
                    $(".SCover").hide();
                    $(".glyphicon-ok").hide();
                    $(".SCover", this).show();
                    $(".glyphicon-ok", this).show();
                });
            }
        }
    });
}

function selectMaterial(mediaid) {
    $('#' + mediaid).siblings().attr('curMedia', 0);
    $('#' + mediaid).attr('curMedia', 1);
}

$('#btnOk').click(function () {
    var $li = $('li[curMedia=1]');
    if ($li.length > 0) {
        curMedia = $li.attr('id');
        $(".sucai-library").hide();
        $(".coverage").hide();
        $(".create_access").hide();

        var curMediaObj = mediaList[$li.attr('idx')];
        var $detail = $('#mediaDetail');
        $('#mediaTime').val(curMediaObj.update_time);
        $('img[name=wrapper]').attr('src', 'GetMedia?mediaid=' + curMediaObj.items[0].thumb_media_id);
        $('span[name=wrapperTitle]').text(curMediaObj.items[0].title);

        var html = [];
        $(curMediaObj.items).each(function (idx, el) {
            if (idx > 0) {
                html.push(' <div class="item" >');
                html.push(' <div class="WX-edted">');
                html.push(' <i><img src=GetMedia?mediaid=' + $(el).attr('thumb_media_id') + ' /> </i>');
                html.push(' <span name="title">' + $(el).attr('title') + '</span>');
                html.push('</div></div>');
            }
        });
        $('#divChild').html(html.join(''));
        $detail.show();
    }
});
