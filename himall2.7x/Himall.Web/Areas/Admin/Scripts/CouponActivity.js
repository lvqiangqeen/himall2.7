$(function () {
    init();
    delCoupon();
});

function init() {
    clip = new ZeroClipboard.Client(); //初始化对象  
    ZeroClipboard.setMoviePath("/Scripts/ZeroClipboard.swf");// 设置flash
    clip.setHandCursor(true); //设置手型  
    clip.addEventListener('mouseDown', function (client) {  //创建监听  
        clip.setText($(".lj").text()); //设置需要复制的代码  
        $.dialog.tips("链接复制成功!");
    });
    clip.glue('fz'); //将flash覆盖至指定ID的DOM上  
}

function delCoupon() {
    //删除
    $("a[id^='del_']").on("click", function () {
        var dx = $(this);
        $.dialog.confirm("是否确认删除此优惠券？", function () {
            dx.parent().parent().remove();
        })
    })
}

//点击关闭
$("#coStatus2").on("click", function () {
    $(".tr_li").html("");
    $(".opdiv").hide();
})
//点击开启
$("#coStatus1").on("click", function () {
    $(".opdiv").show();
    init();
})

//保存
$("#btn_save").on("click", function () {
    var status = 0;
    var couponIds = "";
    if ($("#coStatus1").is(':checked')) {
        status = 1;
    }
    if (status == 1) {
        $.each($(".tr_li").find("input"), function (i, e) {
            couponIds += $(e).val() + ',';
        })
        if (couponIds == "") {
            $.dialog.tips("请选择优惠券！");
            return false;
        }
        if ($('#colist tr').length > 99) {
            $.dialog.tips('设置的优惠券总数不能超过99！');
            return false;
        }
    }
    $.post("Update", { CouponRegisterId: $("#CouponRegisterId").val(), status: status, couponIds: couponIds }, function (data) {
        $.dialog.tips(data.msg);
    }, "json")
})



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
                $.dialog.tips('设置的优惠券总数不能超过99！');
                return false;
            }
            var str = "";
            $.each($('input[name="topic"]:checked'), function (i, e) {
                if ($('.tr_li input[value="' + $(e).attr("CouponId") + '"]').length == 0) {
                    str += " <tr class=\"tr_li\">";
                    str += "<td><a href=\"/m-wap/vshop/CouponInfo/" + $(e).attr("CouponId") + "\" class=\"fl a_hover\" target=\"_blank\" ";
                    str += "title=\"@item.CouponName\">" + $(e).attr("CouponName") + "</a>";
                    str += "<input type=\"hidden\" id=\"couponId\" value=\"" + $(e).attr("CouponId") + "\" />";
                    str += "</td>";
                    str += "<td>" + $(e).attr("ShopName") + "</td>";
                    str += "<td>" + $(e).attr("Price") + "</td>";
                    str += "<td>" + $(e).attr("inventory") + "</td>";
                    str += "<td>" + $(e).attr("OrderAmount") + "</td>";
                    str += "<td>" + $(e).attr("strStartTime") + " ~ " + $(e).attr("strEndTime") + "</td>";
                    str += "<td><a href=\"javascript:;\" id=\"del_" + $(e).attr("CouponId") + "\">删除</a></td>";
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
            { field: "CouponName", title: "优惠券名称" },
            { field: "ShopName", title: "商家" },
            { field: "Price", title: "面额" },
            { field: "inventory", title: "剩余数量" },
            { field: "OrderAmount", title: "使用条件" },
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
                   var html = '<input type="checkbox" name="topic" ShopName="' + row.ShopName + '" Price="' + row.Price + '" CouponName="' + row.CouponName + '" CouponId="' + row.Id + '" inventory="' + row.inventory + '" OrderAmount="' + row.OrderAmount + '" strStartTime="' + row.strStartTime + '" strEndTime="' + row.strEndTime + '" ' + checks + '  />';
                   return html;
               }
           }
             ]]
    });
}


$("#btncoupon").click(function () {
    bind();
});