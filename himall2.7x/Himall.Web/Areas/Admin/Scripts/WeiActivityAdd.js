$(function () {


    rdoChange();
    $("#subCate").click(function () {
        if (check()) {
            document.getElementById("ActivityForm").submit();
        }
    });
    $("#upload-img").himallUpload({
        title: '',
        imageDescript: '',
        imgFieldName: "activityUrl",
        displayImgSrc: $("#MIpath").val(),
        dataWidth: 10
    });
    //开始时间控件绑定
    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1
    });
    //结束时间控件绑定
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1
    });
    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

    //新增专题模块
    $('#moduleContainer').on('click', 'a.choose-goods', function () {
        var moduleIndex = $(this).attr('index');
        !moduleIndex && (moduleIndex = 0);
        var ids = null;

        $.productSelector.show(ids, function () {
            //记录当前选中的商品
            //moduleProducts[moduleIndex] = selectedProducts;
            $('tr[index="' + moduleIndex + '"] td[type="selectedNumber"]').html();
        }, 'selleradmin');
    });

    //保存当前最高奖等
    $("#harward").val(3);

    ////添加change方法
    for (var i = 1; i <= 6; i++) {
        addChange(i);
    }

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }
        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
    //下一步
    $("#id_next").click(function () {
        $("ul.choose-step.dln li").eq(0).removeClass("active");
        $("ul.choose-step.dln li").eq(1).addClass("active");

        if (check1())//校验
        {
            $("#release-pro").css("display", "none");
            $("#release-detail").css("display", "block");
            $(".step2").css("display", "none");
            $(".step3").css("display", "block");
        }
    });
    //上一步
    $("#id_last").click(function () {
        $("#release-pro").css("display", "block");
        $("#release-detail").css("display", "none");
        $(".step2").css("display", "block")
        $(".step3").css("display", "none")
    });
    //新增一行 
    $("#id_addrow").click(function () {
        var nums = parseInt($("#harward").val());
        if (nums == 6) {
            $.dialog.tips("已达到奖项最高设置！");
            return;
        }
        $("#harward").val(nums + 1);
        // $("#str")[0].innerHTML += createAward(nums);//新增
        btnShow(nums + 1);


    });
    $("#btncoupon").click(function () {
        bind();
    });
    if ($('#rdi1')[0].checked) {
        $("#DayCount")[0].disabled = true;
    }
    else if ($("#rdi2")[0].checked) {
        $("#CommonCount").disabled = true;
    }
    else if ($("#rdi3")[0].checked) {
        $("#CommonCount")[0].disabled = true;
        $("#DayCount")[0].disabled = true;
    }
    if ($("#rdi4")[0].checked) {
        $("#consumePoint").disabled = true;
    }
});


//模块商品,用于装载各模块已选择的商品
var moduleProducts = [];
function check1() {

    var productId = $("#productId").val();
    if ($.trim($("#activityTitle").val()) == '') {
        $.dialog.tips('活动标题必填！');
        return false;
    }
    if ($.trim($("#activityDetails").val()) == '') {
        $.dialog.tips('活动分享详情必填！');
        return false;
    }
    if ($.trim($("input[name=activityUrl]").val()) == '') {
        $.dialog.tips('请上传图片！');
        return false;
    }

    if ($.trim($("#beginTime").val()) == '') {
        $.dialog.tips('开始日期必填！');
        return false;
    }
    if ($.trim($("#endTime").val()) == '') {
        $.dialog.tips('结束日期必填！');
        return false;
    }
    var oDate1 = new Date($.trim($("#beginTime").val().replace(/-/g, "/")));
    var oDate2 = new Date($.trim(getNowFormatDate().replace(/-/g, "/")));
    var oDate3 = new Date($.trim($("#endTime").val().replace(/-/g, "/")));
    if (isNaN(parseInt(productId))) {

        if (oDate1 <= oDate2) {
            $.dialog.tips('开始日期必须大于当前日期！');
            return false;
        }
        if (oDate3 <= oDate2) {
            $.dialog.tips('结束日期必须大于当前日期！');
            return false;
        }
    }
    if (oDate3 <= oDate1) {
        $.dialog.tips('结束日期必须大于开始日期！');
        return false;
    }
    return true;
}
///验证基础信息
function check() {
    var productId = $("#productId").val();


    //参与次数
    if ($("#rdi1")[0].checked) {
        if (isNaN(parseFloat($("#CommonCount").val()))) {
            $.dialog.tips('请输入正确的参与次数！');
            return false;
        }
        if (parseInt($("#CommonCount").val()) > 99) {
            $.dialog.tips('参与次数不能大于99！');
            return false;
        }
        if (parseInt($("#CommonCount").val()) <= 0) {
            $.dialog.tips('参与次数不能小于0！');
            return false;
        }
    }
    else if ($("#rdi2")[0].checked) {
        if (isNaN(parseFloat($("#DayCount").val()))) {
            $.dialog.tips('请输入正确的参与次数！');
            return false;
        }
        if (parseInt($("#DayCount").val()) > 99) {
            $.dialog.tips('参与次数不能大于99！');
            return false;
        }
        if (parseInt($("#DayCount").val()) <= 0) {
            $.dialog.tips('参与次数不能小于0！');
            return false;
        }
    }

    //消耗积分
    if ($("#rdi5")[0].checked) {
        if (isNaN($("#consumePoint").val()) || $("#consumePoint").val() == "") {
            $.dialog.tips('请输入正确的消耗积分！');
            return false;
        }
        if (parseInt($("#consumePoint").val()) > 99999) {
            $.dialog.tips('消耗积分不能大于99999！');
            return false;
        }
        if (parseInt($("#consumePoint").val()) < 0) {
            $.dialog.tips('消耗积分不能小于0！');
            return false;
        }
    }


    var strbouns = "";
    var strcoupon = "";
    if (isNaN(parseInt(productId))) {
        var proportion = 0;
        var arawrd = "";

        var allStatus;//是否全部填写
        //当前选则奖品类型
        var arawrdId = 0;
        for (var i = 1; i <= parseInt($("#harward").val()) ; i++) {

            if ($("#lableBouns" + i)[0].style.display == "block") {
                arawrdId = 1;
            }
            else if ($("#lableCoupn" + i)[0].style.display == "block") {
                arawrdId = 2;
            }
            else { arawrdId = 0; }
            switch (i) {
                case 1: arawrd = "一等奖"; break;
                case 2: arawrd = "二等奖"; break;
                case 3: arawrd = "三等奖"; break;
                case 4: arawrd = "四等奖"; break;
                case 5: arawrd = "五等奖"; break;
                case 6: arawrd = "六等奖"; break;
            }


            var id = "#proportion" + i;//中奖概率
            var awardid = "#awardcount" + i;//奖品数量
            var integralid = "#integral" + i;//积分
            var bouns = "#brand" + i;//红包
            var coupons = "#coupon" + i;//优惠券
            var inventory = "#inventory" + i;//优惠券张数
            //当前奖项是否选择
            if ($(id).val() == "" && $(awardid).val() == "") {
                //至少设置一个奖项
                if (i == 1) {
                    $.dialog.tips('请按照奖项顺序至少设置一个奖项！');
                    return false;
                }
                else if ($(integralid).val() != "" && $(id).val() == "" && $(awardid).val() == "") {
                    $.dialog.tips('请将奖项信息填写完整！');
                    return false;
                }
                    //用户不在填写后续奖项
                else if ($(integralid).val() == "" && $(id).val() == "" && $(awardid).val() == "") {
                    allStatus = true;//用户未填写当前奖等
                }

            }
            else {
                if (allStatus) {
                    $.dialog.tips('请将奖项信息填写完整！');
                    allStatus = false;//当之前出现过奖等为空 设置全填状态为false
                    return allStatus;
                }
            }
            if (allStatus == undefined || !allStatus) {
                //判断积分
                if ((arawrdId == 0 && $(integralid).val() == "") || parseFloat($(integralid).val()) <= 0 || parseFloat($(integralid).val()) > 99999) {
                    $.dialog.tips(arawrd + '积分设置错误！');
                    return false;
                }
                //基础信息 奖品数量
                if ($(awardid).val() == "" || parseFloat($(awardid).val()) <= 0 || parseFloat($(awardid).val()) > 9999) {
                    $.dialog.tips(arawrd + '奖品数量设置错误！');
                    return false;
                }
                //判断红包
                if (arawrdId == 1) {
                    if (strbouns.lastIndexOf($(bouns).val()) >= 0) {
                        $.dialog.tips(arawrd + '红包重复选择,一个红包不能用于多个奖项！');
                        return false;
                    }
                    strbouns += $(bouns).val() + ",";
                    var bard = 0;
                    bard = $(bouns).find("option:selected").text().split('：')[1];
                    bard = bard.split('个')[0];
                    if (parseInt($(awardid).val()) > parseInt(bard)) {
                        $.dialog.tips(arawrd + '红包数量超出,请重新输入！');
                        return false;
                    }
                }

                //判断优惠券
                if (arawrdId == 2) {
                    if (strcoupon.lastIndexOf($(coupons).val()) >= 0) {
                        $.dialog.tips(arawrd + '优惠券重复选择,一个优惠券不能用于多个奖项！');
                        return false;
                    }
                    strcoupon += $(coupons).val() + ",";
                    var bard = 0;
                    bard = $(inventory).val();
                    if (parseInt($(awardid).val()) > parseInt(bard)) {
                        $.dialog.tips(arawrd + '优惠券数量超出,请重新输入！');
                        return false;
                    }
                }
                //基础信息 中奖概率
                proportion += parseFloat($(id).val());
                if (isNaN(proportion) || proportion > 100 || proportion <= 0) {
                    $.dialog.tips(arawrd + '中奖概率设置错误！');
                    return false;
                }
            }

        }
    }
    return true;
};

//获取当前日期
function getNowFormatDate() {
    var date = new Date();
    var seperator1 = "-";
    var seperator2 = ":";
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var strDate = date.getDate();
    if (month >= 1 && month <= 9) {
        month = "0" + month;
    }
    if (strDate >= 0 && strDate <= 9) {
        strDate = "0" + strDate;
    }
    var currentdate = year + seperator1 + month + seperator1 + strDate
            + " " + date.getHours() + seperator2 + date.getMinutes()
            + seperator2 + date.getSeconds();
    return currentdate;
}
//选择改变类型
function addChange(nums) {
    var typeName = "input[name=ReceiveType" + nums + "]";
    $(typeName).change(function () {
        var self = $(this).val();
        var id = $(this)[0].name.substring($(this)[0].name.length, $(this)[0].name.length - 1);
        if (self == 0) {
            $("#lableBouns" + id)[0].style.display = "none"
            $("#lable" + id)[0].style.display = "block"
            $("#lableCoupn" + id)[0].style.display = "none"

        }
        else if (self == 1) {
            $("#lableBouns" + id)[0].style.display = "block"
            $("#lable" + id)[0].style.display = "none"
            $("#lableCoupn" + id)[0].style.display = "none"

        }
        else {
            $("#lableBouns" + id)[0].style.display = "none"
            $("#lable" + id)[0].style.display = "none"
            $("#lableCoupn" + id)[0].style.display = "block"
        }

    });
}

//单选按钮事件
function rdoChange() {

    var typeName = "input[name=participationType]";
    $(typeName).change(function () {
        var self = $(this).val();
        if (self == "CommonCount") {
            $("#DayCount")[0].disabled = true;
            $("#DayCount").val("");
            $("#CommonCount")[0].disabled = false;

        }
        else if (self == "DayCount") {
            $("#CommonCount")[0].disabled = true;
            $("#CommonCount").val("");
            $("#DayCount")[0].disabled = false;
        }
        else {
            $("#CommonCount")[0].disabled = true;
            $("#DayCount")[0].disabled = true;
            $("#CommonCount").val("");
            $("#DayCount").val("");
        }
    });

    var pointName = "input[name=isPoint]";
    $(pointName).change(function () {
        var self = $(this).val();
        if (self == 0) {
            $("#consumePoint")[0].disabled = true;
            $("#consumePoint").val("");

        }
        else {
            $("#consumePoint")[0].disabled = false;
            $("#consumePoint").val("");
        }

    });
}

//控制显示删除按钮
function btnShow(num) {
    while (num > 3) {
        if ((num) == parseInt($("#harward").val())) {
            var btnId = "#id_deleterow" + num;

            $(btnId).show();
            $("#" + num).show();
        }
        else {
            var btnId = "#id_deleterow" + num;
            $(btnId).hide();

        }
        num--;
    }
}
//删除
function deleteRow(num) {
    //删除当前表格
    var idTable = "#" + num;
    $(idTable).hide();
    $("#harward").val(num - 1);
    btnShow(num - 1);
}

//选取优惠券
function ChoceCoupon(id) {
    $.dialog({
        title: '选取优惠券',
        lock: true,
        width: 550,
        padding: '0 5px',
        id: 'chooseCouponDialog',
        content: $('#choceCouponUrl')[0],
        okVal: '确认选择',
        ok: function () {
            return saveChooseTopicToSlideImg(id);
            // $.dialog.close();
        }
    });

    bind(id);
}
function bind(couponId) {
    //优惠券表格
    $("#CouponGrid").hiMallDatagrid({
        url: './GetCouponByName',
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
        queryParams: { text: $("#couponName").val(), endtime: $("#endTime").val(), page: 1, pageSize: 5 },
        columns:
             [[
            { field: "CouponName", title: "优惠券名称" },
            { field: "Price", title: "面额" },
            { field: "ShopName", title: "商家" },
            { field: "OrderAmount", title: "使用条件" },
            { field: "strEndTime", title: "有效期" },
            {
                filed: "N", title: "库存", formatter: function (value, row, index) {
                    var html = row.inventory;
                    return html;
                }
            },
           {
               field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {


                   if (row.Id == $("#coupon" + couponId).val()) {
                       return '<input type="radio" name="topic" topicId="' + row.Id + '"  inventory="' + row.inventory + '" data-name="' + row.CouponName + '"  checked="checked"/>';
                   }
                   var html = '<input type="radio" name="topic" topicId="' + row.Id + '"  inventory="' + row.inventory + '" data-name="' + row.CouponName + '" />';

                   return html;
               }
           }
             ]]
    });
}

function saveChooseTopicToSlideImg(id) {
    var check = $('input[name="topic"]:checked')
    $("#coupon" + id).val(check.attr('topicId'));
    $("#inventory" + id).val(check.attr('inventory'));
    $("#coupon" + id).siblings('.btn').text('修改').siblings('.choose-tips').html('优惠券 | ' + check.data('name')).css('display', 'inline-block');
    $.dialog.close();
}





