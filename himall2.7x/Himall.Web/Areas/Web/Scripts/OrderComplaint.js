function AddComplain(shopId, orderId) {
    $.dialog({
        title: '我要投诉',
        lock: true,
        id: 'ChangePwd',
        width: '500px',
        content: document.getElementById("addform"),
        padding: '20px 10px',
        okVal: '确认投诉',
        init: function () { $("#content").focus(); },
        ok: function () {
            var complaint = $("#content").val();
            complaint = complaint.replace(/>/g, '&gt;').replace(/</g, '&lt;').replace(/\n/g, "&lt;br&gt;").replace("'", "‘").replace('"', "“");
            var phone = $("#UserPhone").val();
            if (complaint.length < 5) {
                $.dialog.errorTips("投诉原因不能小于五个字符！", '', 1);
                return false;
            }
            else if (phone == "") {
                $.dialog.errorTips("投诉电话不能为空！", '', 1);
                return false;
            }
            var reg = /^0?1[3|4|5|8][0-9]\d{8}$/;
            if (!reg.test(phone)) {
                $.dialog.errorTips("电话格式错误！", '', 1);
                return false;
            }
            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: '/OrderComplaint/AddOrderComplaint',
                cache: false,
                data: { "ComplaintReason": complaint, "UserPhone": phone, "ShopId": shopId, "OrderId": orderId },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips("提交成功！", function () {
                            window.location.href = window.location.href;
                        }, 1);

                    }
                    else {
                        $.dialog.errorTips("提交失败！" + data.msg, '', 2);
                    }
                },
            });
        }
    });
}