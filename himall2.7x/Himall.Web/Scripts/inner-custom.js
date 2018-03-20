// JavaScript Document

$(document).ready(function () {

    // radio获取选中项的value值
    $('input[name="testradio"]:checked').val();

    //处理只能输入小数点、数字input
    $('.input-number').keypress(function () {
        if (!this.value.match(/^[\+\-]?\d*?\.?\d*?$/)) {
            this.value = this.t_value;
        } else {
            this.t_value = this.value;
        }
        if (this.value.match(/^(?:[\+\-]?\d+(?:\.\d+)?)?$/)) {
            this.o_value = this.value;
        }
    }).keyup(function () {
        if (!this.value.match(/^[\+\-]?\d*?\.?\d*?$/)) {
            this.value = this.t_value;
        } else {
            this.t_value = this.value;
        }
        if (this.value.match(/^(?:[\+\-]?\d+(?:\.\d+)?)?$/)) {
            this.o_value = this.value;
        }
    }).blur(function () {
        if (!this.value.match(/^(?:[\+\-]?\d+(?:\.\d+)?|\.\d*?)?$/)) {
            this.value = this.o_value;
        } else {
            if (this.value.match(/^\.\d+$/)) {
                this.value = 0 + this.value;
            }
            if (this.value.match(/^\.$/)) {
                this.value = 0;
            }
            this.o_value = this.value
        }
    });

    //只能输入整数
    $('.input-int-num').keydown(function (e) {
        var keyCode = e.keyCode || e.which || e.charCode;
        var ctrlKey = e.ctrlKey || e.metaKey;
        if (ctrlKey && keyCode == 86) return true;
        if (ctrlKey && keyCode == 67) return true;
        if (ctrlKey && keyCode == 88) return true;
        if (keyCode == 116) return true;
        if (keyCode >= 96 && keyCode <= 105) return true;
        if (keyCode >= 48 && keyCode <= 57) return true;
        if (keyCode == 13 || keyCode == 9 || keyCode == 8 || keyCode == 46) return true;
        if (keyCode >= 35 && keyCode <= 39) return true;
        //if (keyCode == 110 || keyCode == 190) return true;
        return false;
    }).on("blur", function () {
        var _t = $(this);
        var _v = _t.val();
        _v = _v.replace(/[^0-9-]+/, '');
        _t.val(_v);
    });

    //不能输入特殊字符
    $('.input-no-sp').keyup(function () {
        (this.v = function () {
            this.value = this.value.replace(/[^\a-\z\A-\Z0-9\u4E00-\u9FA5\，\,\.\\b]+/, '');
        }).call(this);
    }).blur(function () {
        this.v();
    });

    //不能输入特殊字符
    $('.input-no-at').keyup(function () {
        (this.v = function () {
            this.value = this.value.replace(/[\@]/g, '');
        }).call(this);
    }).blur(function () {
        this.v();
    });

    //操作提示
    $('.primary-btn').hover(function () {
        $('.primary').toggle();
    });

    //文本框聚焦自动选中文字
    $("input[type=text]").click(function () {
        this.select();
    });
    //添加权限组
    $('.checkbox-list li').each(function () {
        var _this = $(this);
        _this.find('.parent-check>input').click(function () {
            _this.children('p').find('input').prop('checked', this.checked);
        })
    });
    $('.power-check-all input').click(function () {
        $('.checkbox-list').find('input[type=checkbox]').prop('checked', this.checked);
    });

});


function searchClose(e) {
    e ? e.stopPropagation() : event.cancelBubble = true;
    $('.custom-inline').removeClass('deploy');
    //$('.start_datetime,.end_datetime').val('');
}


function solvedOpen(elem) {
    var tips = $(window.parent.document).find(elem).text().replace('(', '').replace(')', '');
    if (tips > 0) {
        $('.nav-tabs-custom li.solved').click();
    }
}
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}