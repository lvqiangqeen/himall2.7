    $(function () {

        $("#NewWeekSettlement").keyup(function () {
            value = value.replace(/[^\d\.]/g, '')
        });
    })


function checkValue() {
    var text = $("#NewWeekSettlement").val();
    var value = parseInt($("#NewWeekSettlement").val());
    var type = /^[1-9]\d*$/;
    var re = new RegExp(type);
    if (text.match(re) == null) {
        $.dialog.tips("请输入大于零的整数!")
        return false;
    }
    if (value > 0) {
        if (value > 365) {
            $.dialog.tips("请输入小于等于365的整数！");
            return false;
        }
        else {
            return true;
        }
    }
    else {
        $.dialog.tips("请输入大于0的整数！");
        return false;
    }
}
