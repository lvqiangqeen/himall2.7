// JavaScript source code
$(function () {

    $("input:radio").on("change", function () {
        if ($("input:radio:checked").val() == "2")
        { $(".tag-area").show(); }
        else
        { $(".tag-area").hide(); }
    });
    $(".tag-choice").click(function () {
        $(".dialog-tag").css("display", "block");
        $(".coverage").show();
        $('input[name=check_Label]').each(function (i, checkbox) {
            $(checkbox).get(0).checked = false;
        });
        $('.tag-area span').each(function (idx, el) {
            $('#check_' + $(el).attr('LabelId')).get(0).checked = true;
        });
    });
    $(".tag-submit").click(function () {
        var labels = [];
        $('input[name=check_Label]').each(function (i, checkbox) {
            if ($(checkbox).get(0).checked) {
                labels.push('<span labelid="' + $(checkbox).attr('datavalue') + '">' + $(checkbox).val() + '</span>');
            }
        });
        $('.tag-area').html(labels.join(''));
        $(".dialog-tag").hide();
        $(".coverage").hide();
    });
    $(".dialog-tag .glyphicon-remove").click(function () {
        $(".dialog-tag").hide();
        $(".coverage").hide();
    });
    $('.tag-back').click(function () {
        $(".dialog-tag").hide();
        $(".coverage").hide();
    });
    $('#SendMsg').click(SendMsg);
    delCoupon();
});

//发送消息
function SendMsg() {
    var label = '';
    var desc = [];
    if (!$('#allLabel').get(0).checked) {
        $('.tag-area span').each(function (idx, el) {
            label += ',' + $(el).attr('labelid');
            desc.push($(el).text());
        });
        if (label.length == 0) {
            $.dialog.alert('请选择要发送的标签');
            return;
        }
        label = label.substr(1, label.length - 1);
    }
    else {
        desc.push('标签:全部');
    }

    var couponIds = "";

    $.each($(".tr_li").find("input"), function (i, e) {
        couponIds += $(e).val() + ',';
    })
    if (couponIds == "") {
        $.dialog.tips("请选择优惠券！");
        return false;
    }
    if ($('#colist tr').length > 99) {
        $.dialog.tips('发送的优惠券总数不能超过99！');
        return false;
    }
    loading = showLoading('正在发送');
    $.post('SendCouponMsg', { labelids: label, couponIds: couponIds, labelinfos: desc.join(' ') }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.alert("发送成功！", function () {
                history.go(0);
            }, 3);
        }
        else {
            $.dialog.alert(result.msg);
        }
    });
}

//删除
function delCoupon() {
    //删除
    $("a[id^='del_']").on("click", function () {
        var dx = $(this);
        $.dialog.confirm("是否确认删除此优惠券？", function () {
            dx.parent().parent().remove();
        })
    })
}

$("#ChoceCoupon").on("click", function () {
    $("#couponName").val("");
    ChoceCoupon();
})


function ChoceCoupon() {
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 650,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认选择',
        ok: function () {
            if ($('#colist tr').length + $('input[name="topic"]:checked').length > 99) {
                $.dialog.tips('发送的优惠券总数不能超过99！');
                return false;
            }
            var str = "";
            $.each($('input[name="topic"]:checked'), function (i, e) {
                if ($('.tr_li input[value="' + $(e).attr("CouponId") + '"]').length == 0) {
                    str += " <tr class=\"tr_li\">";
                    str += "<td ><a href=\"/m-wap/vshop/CouponInfo/" + $(e).attr("CouponId") + "\" class=\"fl a_hover\" target=\"_blank\" ";
                    str += "title=\"@item.CouponName\" style='text-align:center;width:100%;'>" + $(e).attr("CouponName") + "</a>";
                    str += "<input type=\"hidden\" id=\"couponId\" value=\"" + $(e).attr("CouponId") + "\" />";
                    str += "</td>";
                    str += "<td style='text-align:center'>" + $(e).attr("ShopName") + "</td>";
                    str += "<td style='text-align:center'>" + $(e).attr("Price") + "</td>";
                    str += "<td style='text-align:center'>" + $(e).attr("inventory") + "</td>";
                    str += "<td style='text-align:center'>" + $(e).attr("OrderAmount") + "</td>";
                    str += "<td style='text-align:center'>" + $(e).attr("strStartTime") + " ~ " + $(e).attr("strEndTime") + "</td>";
                    str += "<td style='text-align:center'><a href=\"javascript:;\" id=\"del_" + $(e).attr("CouponId") + "\">删除</a></td>";
                    str += "</tr>";
                }
            })
            if ($('#colist tr #couponId').length == 0) {
                $("#colist").html(str);
            } else
                $("#colist").append(str);
            delCoupon();
        }
    });

    bind();
}
function bind() {
    //优惠券表格
    $("#CouponGrid").hiMallDatagrid({
        url: '/Admin/WeiActivity/GetCouponByName',
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
        queryParams: { text: $("#couponName").val(), ReceiveType: 2, page: 1, pageSize: 5 },
        columns:
             [[
            { field: "CouponName", title: "优惠券名称", align: "center" },
            { field: "ShopName", title: "商家", align: "center" },
            { field: "Price", title: "面额", align: "center" },
            { field: "inventory", title: "剩余数量", align: "center" },
            { field: "OrderAmount", title: "使用条件", align: "center" },
            {
                field: "strEndTime", title: "有效期", align: "center", width: 180, formatter: function (value, row, index) {
                    var html = row.strStartTime + '~' + row.strEndTime;
                    return html;
                }
            },
           {
               field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {
                   
                   var checks = "";
                   if ($('.tr_li input[value="' + row.Id + '"]').length > 0) {
                       checks = ' checked="checked"';
                   }

                   var html = '<input type="checkbox" name="topic" ShopName="' + row.ShopName + '" Price="' + row.Price + '" CouponName="' + row.CouponName + '" CouponId="' + row.Id + '" inventory="' + row.inventory + '" OrderAmount="' + row.OrderAmount + '" strStartTime="' + row.strStartTime + '" strEndTime="' + row.strEndTime + '"  ' + checks + ' />';
                   return html;
               }
           }
             ]]
    });
}

$("#btncoupon").click(function () {
    bind();
});