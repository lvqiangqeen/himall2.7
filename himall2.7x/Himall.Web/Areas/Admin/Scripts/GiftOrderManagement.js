var categoryId;
var expselect;
var showstatus = "";

$(function () {
    $('.nav-tabs-custom li[type="statusTab"]').click(function (e) {
        var _t = $(this);
        $(this).addClass('active').siblings().removeClass('active');
        showstatus = _t.attr("value");
        $("#skey").val("");
        reload();
    });

    showstatus = GetQueryString("status");

    showdata(showstatus);

    bindSearchBtnClick();

    expselect = $("#expselectbox").html();
});


function bindSearchBtnClick() {
    $('#search').click(function () {
        reload();
    });
}

function showdata(val) {
    $('.nav-tabs-custom li').each(function () {
        if ($(this).val() == val) {
            $(this).addClass('active').siblings().removeClass('active');
        }
    });
    initTable();
}

function initTable() {

    //文章表格
    $("#list").hiMallDatagrid({
        url: 'OrderList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { status: showstatus },
        columns:
        [[
           {
               field: "Id", title: '兑换编号', align: 'center', width: 160
           },
            {
                field: "ShowOrderStatus", title: '状态', width: 100,
                formatter: function (value, row, index) {
                    return '<span class="orderstate' + row.OrderStatus + '">' + value + '</span>';
                }
            },
            {
                field: "FirstGiftName", title: '礼品名称',
                formatter: function (value, row, index) {
                    return "<a href='/gift/Detail/" + row.FirstGiftId + "' target='_blank'>" + value + "</a>";
                }

            },
            {
                field: "FirstGiftBuyQuantity", title: '数量', width: 50

            },
            {
                field: "OrderDate", title: '下单时间', width: 160,
                formatter: function (value, row, index) {
                    return time_string(value);
                }
            },
            {
                field: "UserName", title: '会员', width: 80,
                formatter: function (value, row, index) {
                    return '<span>' + value + '</span>';
                }

            },
            {
                field: "TotalIntegral", title: '消费积分', width: 80,
                formatter: function (value, row, index) {
                    return value;
                }

            },
            {
                field: "s", title: "操作", width: 100, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a">';
                    html += "<input type=\"hidden\" name=\"rowdata\" id=\"rowdata-" + row.Id + "\" value='" + jQuery.toJSON(row) + "'>";
                    switch (row.OrderStatus) {
                        case 2:
                            html += '<a class="good-check" onclick="sendgift(' + row.Id + ')">发货</a>';
                            break;
                        case 3:
                            html += '<a class="good-check" onclick="vieworder(' + row.Id + ')">查看物流</a>';
                            break;
                        default:
                            html += '<a class="good-check" onclick="vieworder(' + row.Id + ')">查看</a>';
                            break;
                    }

                    html += '</span>';
                    return html;
                },
                styler: function () {
                    return 'td-operate';
                }
            }
        ]]
    });

}

function reload() {
    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
    $("#list").hiMallDatagrid('reload', { skey: $("#skey").val(), status: showstatus, pageNumber: pageNo });

}

function sendgift(id) {
    var dobj = $("#rowdata-" + id);
    var data = jQuery.parseJSON(dobj.val());
    $.dialog({
        title: data.Id + ' 订单发货',
        width: 400,
        lock: true,
        id: 'dlgsendgift',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">收货人</label>',
                '<p class="help-top">' + data.ShipTo.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">联系电话</label>',
                '<p class="help-top">' + data.CellPhone + '</p>',
            '</div>',
             '<div class="form-group">',
                '<label class="label-inline fl">收货地址</label>',
                '<p class="help-top">' + data.RegionFullName.replace(/>/g, '&gt;').replace(/</g, '&lt;') + " " + data.Address + '</p>',
           ' </div>',
             '<div class="form-group">',
                '<label class="label-inline fl">快递公司</label>',
                '<p class="help-top" id="expselp">' + expselect + '</p>',
           ' </div>',
             '<div class="form-group">',
                '<label class="label-inline fl">快递单号</label>',
                '<p class="help-top"><input class="input-ssm cssShipOrderNumber" id="intexpnum" name="intexpnum" type="text" value=""></p>',
           ' </div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txtRefundRemark").focus(); },
        button: [
        {
            name: '确认发货',
            callback: function () {
                var thisdlg = this;
                var expname = $("#expselp select").val();
                var expnum = $("#intexpnum").val();
                if (expname.length > 0 && expnum.length > 0) {
                    var loading = showLoading();
                    $.post('SendGift', { id: data.Id, expname: expname, expnum: expnum }, function (result) {
                        loading.close();
                        if (result.success) {
                            thisdlg.close();
                            $.dialog.tips('发货成功!');
                            reload();
                        }
                        else {
                            $.dialog.errorTips(result.msg);
                        }
                    });
                    return false;
                } else {
                    $.dialog.errorTips("请填写快递信息！");
                    return false;
                }
            },
            focus: true
        },
        {
            name: '关闭',
        }
        ]
    });
}

function vieworder(id) {
    var dobj = $("#rowdata-" + id);
    var data = jQuery.parseJSON(dobj.val());

    var expinfo = "";
    var ExpressCompanyName = data.ExpressCompanyName || "";
    var ShipOrderNumber = data.ShipOrderNumber || "";

    var arrcontent = ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">收货人</label>',
                '<p class="help-top">' + data.ShipTo.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">联系电话</label>',
                '<p class="help-top">' + data.CellPhone + '</p>',
            '</div>',
             '<div class="form-group">',
                '<label class="label-inline fl">收货地址</label>',
                '<p class="help-top">' + data.RegionFullName.replace(/>/g, '&gt;').replace(/</g, '&lt;') + " " + data.Address + '</p>',
           ' </div>'];

    if (ExpressCompanyName != "" & ShipOrderNumber != "") {

        arrcontent = arrcontent.concat(['<div class="form-group">',
                    '<label class="label-inline fl">快递公司</label>',
                    '<p class="help-top" id="expselp">' + (ExpressCompanyName == "-1" ? "其他" : ExpressCompanyName) + '</p>',
               ' </div>',
                 '<div class="form-group">',
                    '<label class="label-inline fl">快递单号</label>',
                    '<p class="help-top">' + ShipOrderNumber + '</p>',
               ' </div>']);

        var loading = showLoading();
        // 物流信息

        $.post('/Common/ExpressData/Search', { expressCompanyName: data.ExpressCompanyName, shipOrderNumber: ShipOrderNumber }, function (result) {
            loading.close();
            var html = "";
            html += "<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">";
            html += "<tbody id=\"tbody_track\">";
            html += "<tr><th width=\"25%\"><strong>处理时间</strong></th><th width=\"50%\"><strong>处理信息</strong></th><th width=\"15%\"><strong>操作人</strong></th></tr>"
            html += "</tbody>";
            html += "<tbody id=\"tbExpressData\">";
            var obj = result;
            if (obj.success) {
                var data = obj.data;
                for (var i = data.length - 1; i >= 0; i--) {
                    html += '<tr><td>' + data[i].time + '</td>\
                             <td>' + data[i].content + '</td>';
                    html += '<td></td></tr>';
                }
            }
            else {
                html += '<tr><td colspan="3">该单号暂无物流进展，请稍后再试，或检查公司和单号是否有误。</td></tr>';
            }

            html += '<tr><td colspan="3"><a href="http://www.kuaidi100.com" target="_blank" id="power" runat="server" style="color:Red;">此物流信息由快递100提供</a></td></tr>';
            html += "<\/tbody><\/table>";
            expinfo = html;
            $("#expshowbox").append(expinfo);
        });
    }

    arrcontent = arrcontent.concat(['<div class="form-group" id="expshowbox">',
        ' </div>',
        '</div>']);

    $.dialog({
        title: data.Id + ' 订单查看',
        width: 500,
        lock: true,
        id: 'dlgsendgift',
        content: arrcontent.join(''),
        padding: '0 40px',
        init: function () { $("#txtRefundRemark").focus(); },
        button: [
        {
            name: '关闭',
            focus: true
        }
        ]
    });
}