// JavaScript source code

    var btsubmit;
    var loading;
    function changeDisabled(state) {
        var inputs = $(".edititem input");
        if (state.toLowerCase() == "true") {
            inputs.removeAttr("disabled");
        } else {
            inputs.attr("disabled", "true");
        }
    };
  

    $(function () {
        $('input[type="checkbox"]').onoff();
        btsubmit = $("#btsubmit");
        var d_isenable = $("#IsEnable");
        changeDisabled($("#MITT").val());

        $('input#sw_IsEnable').change(function () {
            var _this = $(this),
                state = _this[0].checked;

            changeDisabled(state.toString());
            d_isenable.val(state);
            _this.onoff('state', state);

        });




        $("#copylink").zclip({
            path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中
            copy: function () {
                return $('#signlink').html();
            },
            afterCopy: function () {
                $.dialog.tips("复制成功");
            }
        });
    });
    var isposting = false;
    function beginpost() {
        if (isposting) {
            $.dialog.tips("数据提交中...");
            return false;
        }
        isposting = true;
        btsubmit.text("提交中...");
        loading = showLoading();
    }

    function successpost(data) {
        isposting = false;
        btsubmit.text("保 存");
        loading.close();
        if (data.success == true) {
            $.dialog.tips("配置签到功能成功", function () {
                window.location.href = $("#UASS").val();//数据提交成功页面跳转
            });
        } else {
            $.dialog.errorTips(data.msg);
        }
    }

