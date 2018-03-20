$.fn.extend({
    getRule: function () {
        var values = [];
        $(this).find("[level]").each(function (i, obj) {
            var level = $(this).attr("level");
            var Quota = $(obj).find("[name=Quota]").val();
            var Discount = $(obj).find("[name=Discount]").val()
            values.push({ level: level, Quota: Quota, Discount: Discount });
        });
        return values;
    },
    addRule: function (value) {
        var rule = $(this);
        var level = rule.data("level") + 1;
        if (level > 5) {
            $.dialog.tips("最多设置5级阶梯");
            return;
        }
        var template = rule.data("template").clone();
        template.attr("level", level);
        template.find(".level").text(level);

        //初始赋值
        if (value != null) {
            template.find("[name=Quota]").val(value.Quota);
            template.find("[name=Discount]").val(value.Discount);
        }

        //移除按钮
        if (level == 1) {
            template.find(".remove-rule").remove();
        } else {
            template.find(".remove-rule").click(function () {
                var level = template.attr("level");
                rule.removeRule(level);
            });
        }
        rule.css("ime-mode", "disabled");
        rule.append(template);
        rule.data("level", level);
    },
    addInputErrorClass: function (d) {
        d.addClass("rule-v-error");
    },
    removeInputErrorClass: function (d) {
        d.removeClass("rule-v-error");
    },

    removeRule: function (level) {
        $(this).find("[level=" + level + "]").remove();
        var level = 0;
        $(this).find("tr").each(function (i) {
            if (i == 0) return;
            $(this).attr("level", i);
            $(this).find(".level").text(i);
            level = i;
        })
        $(this).data("level", level);
    },
    initRule: function () {
        var rule = $(this);
        rule.data("level", 0);
        var temp = rule.find(".template");
        temp.removeClass("hidden");
        temp.removeClass("template");
        rule.data("template", temp.clone());
        temp.remove();

        //绑定事件
        rule.on("keydown", ".txt-num", function (e) {
            var keyCode = e.keyCode;
            if (keyCode >= 96 && keyCode <= 105) return true;
            if (keyCode >= 48 && keyCode <= 57) return true;
            if (keyCode == 13 || keyCode == 9 || keyCode == 8 || keyCode == 46) return true;
            if (keyCode == 37 || keyCode == 39) return true;
            if (keyCode == 110 || keyCode == 190) return true;
                return false;
            //var _t = $(this);
            //var value = _t.val();
            //rule.removeInputErrorClass(_t);
            //if (!/^\d+?(\.\d{1,2})?$/.test(value)) {
            //    rule.addInputErrorClass(_t);
            //}
        });
        rule.on("paste", ".txt-num", function () {
            var _t = $(this);
            var value = _t.val();
            rule.removeInputErrorClass(_t);
            if (!/^\d+?(\.\d{1,2})?$/.test(value)) {
                rule.addInputErrorClass(_t);
            }
        });

        var values = $("[name=RuleJSON]").val();
        if (values == "")
            $(this).addRule();
        else {
            values = JSON.parse(values);
            for (var i = 0; i < values.length; i++) {
                $(this).addRule(values[i]);
            }
        }
    },
});