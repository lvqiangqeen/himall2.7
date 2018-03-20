// JavaScript source code
var isPosting = false;

function SellerRemark(id, remark, flag) {
    $.dialog({
        title: '商家备注',
        lock: true,
        id: 'SellerRemark',
        content: document.getElementById("remark-form"),
        padding: '0 40px',
        okVal: '保存',
        init: function () {
            $("#remarkContent").focus();
            $("#remarkContentTip").hide();
            $("#remarkContent").css({ border: '1px solid #ccc' });
            if ($("#txtSellerRemark").val() != null && $("#txtSellerRemark").val() != "") {
                remark = ("#txtSellerRemark").val();
            }

            if (remark != null && remark != 'null') {
                $("#remarkContent").val(remark);
            }
            else {
                $("#remarkContent").val("最多可输入200个字");
            }


            if ($("#flag").val() != null && $("#flag").val() != "") {
                flag = $("#flag").val();
            }

            if (flag != null && flag != 'null') {
                $("input[name='radflag'][value='" + flag + "']").trigger("click");
            }
            else {
                $("input[name='radflag'][value='1']").trigger("click");
            }
        },
        ok: function () {
            var remark = $("#remarkContent").val();
            var flag = $("input[name='radflag']:checked").val();
            if (remark.trim() == "" || remark.length > 200) {
                $("#remarkContentTip").show();
                $("#remarkContentTip").text("回复内容在200个字符以内！");
                $("#remarkContent").css({ border: '1px solid #f60' });
                return false;
            }
            else {
                $("#remarkContentTip").hide();
                $("#remarkContent").css({ border: '1px solid #ccc' });
            }
            var loading = showLoading();
            $.post("../SellerRemark",
                { id: id, remark: remark, flag: flag },
                function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips("备注成功", function () {
                            $("#remarkContent").val(remark);
                            $("#txtSellerRemark").val(remark);
                        });
                    }
                    else
                        $.dialog.errorTips("备注失败:" + data.msg);
                });
        }
    });
}



$(function () {
    var ExpressCompanyName = $("#MECN").val();
    var ShipOrderNumber = $("#MSON").val();
    if (ExpressCompanyName != "" & ShipOrderNumber != "") {
        // 物流信息
        $.post('/Common/ExpressData/Search', { expressCompanyName: ExpressCompanyName, shipOrderNumber: ShipOrderNumber }, function (result) {
            var html = '';
            var obj = result;
            if (obj.success) {
                var data = obj.data;
                for (var i = data.length - 1; i >= 0; i--) {
                    html += '<div class="form-group clearfix"><label class="col-sm-4">' + data[i].time + '</label>\
                             <div class="col-sm-7">' + data[i].content + '</div>';
                    html += '</div>';
                }
            }
            else {
                html += '<div class="form-group clearfix" style="text-align:center;font-size:16px;">该单号暂无物流进展，请稍后再试，或检查公司和单号是否有误。</div>';
            }

            html += '<div class="form-group clearfix deli-link" style="top: -20px;"><a href="http://www.kuaidi100.com" target="_blank" id="power" runat="server" style="color:#F97616;display:none;">此物流信息由快递100提供</a></div>';
            $("#tbExpressData").append(html);
        });
    }

    // 物流信息

    $('.cg-b').click(function () {
        //$(this).fadeOut(200);
        var itemid = $(this).attr("itemid");
        var discountAmount = $("#inputDiscount" + itemid).val();
        if ($(this).parent("td").find("select").val() == 2)
            discountAmount = -discountAmount;
        if (isNaN(discountAmount) || discountAmount.length == 0) {
            $.dialog.errorTips("请输入正确金额！");
            return false;
        }
        if ($("#updateWay").val() == 1 && isNaN($(this).parent().next().html() < discountAmount))
            $.dialog.errorTips("减价金额不能大于实付金额！");
        var url = $("#UDA").val();
        var loading = showLoading();
        if (isPosting) return;
        isPosting = true;
        $.post(url, { orderItemId: itemid, discountAmount: discountAmount }, function (result) {
            loading.close();
            isPosting = false;
            if (result.success) {
                $.dialog.succeedTips("改价成功！", function () { location.href = location.href; });
                // location.href = "./@Model.Id/?updatePrice="+true;

            }
            else
                $.dialog.errorTips("操作失败" + result.msg);
        });
    });

    $("#updateFreight").click(function () {
        var frieght = $("#freight").val();
        if (frieght == "" || frieght == null || isNaN(frieght) || frieght < 0) {
            $.dialog.errorTips("请输入正确的运费金额！");
            return false;
        }
        var loading = showLoading();
        $.post($("#UOF").val(), { orderId: $("#MDID").val(), frieght: frieght }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("修改运费成功！", function () { location.href = location.href; });

            }
            else
                $.dialog.errorTips("操作失败" + result.msg);
        });
    });
    $.post($("#GRIP").val(), { regionId: $("#MRid").val() }, function (result) {
        $("#hidRegionIdPath").val(result);
    });

    // 修改收货地址
    $('.order-info-resive').click(function () {
        $.dialog({
            title: '修改收货地址',
            lock: true,
            id: 'orderInfoResive',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<label class="label-inline" for="">收货人</label>',
                    '<input class="form-control input-sm" type="text" id="txtShopTo" value="' + $("#spShipTo").html() + '">',
                '</div>',
                '<div class="form-group" id="regionAddr">',
                    '<label class="label-inline" for="">地址</label>',
                    '<span id="RegionSelector"></span>',
                    '<br />',
                    '<input class="form-control input-sm input-only-line" style="margin: 10px 0 0 150px;" type="text" id="txtAddress" value="' + $("#spAddress").html() + '">',
                '</div>',
                '<div class="form-group">',
                    '<label class="label-inline" for="">手机号码</label>',
                    '<input class="form-control input-sm" type="text" id="txtCellPhone" value="' + $("#spCellPhone").html() + '">',
                '</div>',

            '</div>'].join(''),
            padding: '0 40px',
            button: [
           {
               name: '确认修改',
               callback: function () {
                   // if (isSelectAddr($('#region1'), $('#region2'), $('#region3'))) {
                   UpdateAddress();
                   // }
                   // else {
                   //    $('#regionAddr').css({ border: '1px solid #f60' });
                   //   return false
                   // }
               },
               focus: true
           }]
        });

        $("#RegionSelector").RegionSelector({
            selectClass: "form-control input-sm select-sort",
            valueHidden: "#MRid",
        });
        //  setProvince($('#region1'), $('#region2'), $('#region3'));
        // InitRegion($('#region1'), $('#region2'), $('#region3'), $('#hidRegionIdPath').val());
        /*
        $('#region1,#region2,#region3').himallLinkage({
            url: '../getRegion',
            enableDefaultItem: true,
            defaultItemsText: '全部',
            defaultSelectedValues: $("#hidRegionIdPath").val().split(','),
            onChange: function (level, value, text) {
                if (level == 0)
                {$("#hidTopRegionId").val(value);
                    $('#hidCityId').val('0');
                    $("#hidRegionId").val('0');
                }
                if (level==1)
                {
                    $('#hidCityId').val(value);
                    $("#hidRegionId").val('0');
                }
                if (level == 2)
                    $("#hidRegionId").val(value);
            }
        });*/
    });
});

function showExpress() {
    $("#tbExpressData").show();
    location.href = "#delivery-detail";

}

function UpdateAddress() {
    var loading = showLoading();
    $.post('../UpdateAddress', {
        orderId: $("#MDID").val(), shipTo: $("#txtShopTo").val(), cellPhone: $("#txtCellPhone").val(),
        topRegionId: $("#RegionSelector select:first").find("option:selected").val(), regionId: $("#MRid").val(), address: $("#txtAddress").val()
    }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！", function () { location.href = '../Detail/' + $("#MDID").val(); });

        }
        else
            $.dialog.errorTips("操作失败");
    });
};
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
});