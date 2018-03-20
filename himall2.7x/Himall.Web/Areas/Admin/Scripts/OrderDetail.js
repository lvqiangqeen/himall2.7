// JavaScript source code
$(function () {
    var ExpressCompanyName = $("#MECo").val();
    var ShipOrderNumber = $("#MSOn").val();
    if (ExpressCompanyName != "" & ShipOrderNumber != "") {
        // 物流信息
        $.post('/Common/ExpressData/Search', { expressCompanyName: ExpressCompanyName, shipOrderNumber: ShipOrderNumber }, function (result) {
            var html = '';
            var obj = result;
            if (obj.success) {
                var data = obj.data;
                for (var i = data.length - 1; i >= 0; i--) {
                    html += '<div class="form-group clearfix"><label class="col-sm-4">' + data[i].time + '</label>\
                             <div class="col-sm-7 form-control-static">' + data[i].content + '</div>';
                    html += '</div>';
                }
            }
            else {
                html += '<div class="form-group clearfix">该单号暂无物流进展，请稍后再试，或检查公司和单号是否有误。</div>';
            }

            html += '<div class="form-group deli-link clearfix" style="top: -20px;"><a href="www.kuaidi100.com" target="_blank" id="power" runat="server" style="color:#F97616;display:none;">此物流信息由快递100提供</a></div>';
            $("#tbExpressData").append(html);
        });
    }
});

function showExpress() {
    $("#tbExpressData").show();
    location.href = "#delivery-detail";

}

$(function () {
    $(".detail-open").click(function () {
        var t_display = $("#tbExpressData").css("display");
        $("#tbExpressData").toggle();
        if (t_display == 'none') {
            $(".delivery-detail p span").css("background-position", "-223px 5px");
        } else {
            $(".delivery-detail p span").css("background-position", "-247px 5px");
        }

    });
    $(".list-open").click(function () {
        var l_display = $(".order-log .table").css("display");
        $(".order-log .table").toggle();
        if (l_display == 'none') {
            $(".order-log p span").css("background-position", "-223px 5px");
        } else {
            $(".order-log p span").css("background-position", "-247px 5px");
        }

    });
    if ($(".order-info .caption").height() > $(".delivery-info .caption").height()) {
        $(".delivery-info .caption").height($(".order-info .caption").height());
    } else {
        $(".order-info .caption").height($(".delivery-info .caption").height());
    }
})