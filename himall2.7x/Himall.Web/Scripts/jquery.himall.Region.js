/// <div id="Selector"></div>
/// <input type="hidden" name="RegionId" id="RegionId">
/// $("#Selector").RegionSelector({
///        selectClass:"itemSelect",
///        valueHidden:"#RegionId",
/// });
///
///
///
(function ($) {
    $.fn.RegionSelector = function (options) {
        var my = $(this);
        var funs = {
            GetValue: function (obj) {
                return obj.data("value");
            },
            IsFinal: function () {
                return obj.data("isfinal");
            },
        }
        if (typeof options == "string") {
            return funs[options](my);
        }
        var defautls = {
            url: "/common/RegionAPI/GetSubRegion",//获取数据API
            urlTree: "/common/RegionAPI/GetRegionTree",
            maxLevel:4,
            selectClass: "",
            value: null,
            valueHidden:null,
            change: function (val, text, level) { }  //选择变动 回调方法
        };
        var params = $.extend({}, defautls, options || {});
        //生成选择框
        my.empty();
        var selectClassStr = (params.selectClass == "" ? "" : "class='" + params.selectClass + "'");
        for (var i = 1; i <= params.maxLevel; i++) {
            $(this).append("<select level='" + i + "' " + selectClassStr + "></select>");
        }
        var select_list = my.find("select");

        var hidden = null;
        var forid = my.attr("for");
        if (forid) hidden = $("#" + forid);//根据for
        if (params.valueHidden != null) hidden = $(params.valueHidden);

        //定义选择框级别
        select_list.each(function (i) {
            $(this).attr("level", i + 1);
            maxLevel = i + 1;
        });

        //绑定选择方法
        select_list.change(function () {
            var select = $(this);
            var isFinal = true;
            var level = parseInt(select.attr("level"));
            var val = select.find("option:selected").val();
            var text = select.find("option:selected").text();
            if (isNaN(val)) {//选择无效值
                select.nextAll("select").html('<option>请选择</option>');//清空下级所有选择框数据
                if (level == 1) {
                    val = 0;
                    text = "请选择";
                } else {
                    val = select.prev("select").find("option:selected").val();
                    text = select.prev("select").find("option:selected").text();
                    level = parseInt(select.prev("select").attr("level"));
                }
                SetFinal(false);
            } else {
                loadOption(level + 1, val);
            }
            OnChange(val, text, level);
        });

        //数据加载方法
        function loadOption(level, parent) {
            var select = select_list.eq(level - 1);
            select.html('<option>请选择</option>');//清空原有数据
            select.nextAll("select").html('<option>请选择</option>');//清空下级所有选择框数据
            $.post(params.url, { parent: parent }, function (data) {
                if (data.length == 0)
                    SetFinal(true);
                else
                    SetFinal(false);
                for (var i = 0; i < data.length; i++) {
                    select.append('<option value="' + data[i].Id + '">' + data[i].Name + '</option>');
                }
            });
        }

        //初始化方法
        function InitSelector(value) {
            if (value == null)
            {
                value = 0;
                if (params.value != null && params.value > 0) value = params.value;
                if (hidden != null && hidden.val() > 0) value = hidden.val();
            }
            if (value > 0) {//初始化选定值
                $.post(params.urlTree, { id: value }, function (data) {
                    for (var i = maxLevel; i > 0; i--) {
                        var select = select_list.eq(i - 1);
                        select.html('<option>请选择</option>');
                        if (data[i] == null || data[i].length == 0)
                            continue;
                        var items = data[i];
                        for (var x = 0; x < items.length; x++) {
                            select.append('<option value="' + items[x].Id + '" ' + (items[x].option ? "selected" : "") + '>' + items[x].Name + '</option>');
                        }
                    }
                });
                my.data("value", value);
                SetFinal(true);
            } else {
                loadOption(1, 0);//初始化默认设置
            }
        }

        function OnChange(val, text, level) {
            my.data("value", val);
            if (hidden != null) {
                hidden.val(val);
                
                hidden.attr("text", text);
                hidden.attr("level", level);
            }
            if (params.change) {
                params.change(val, text, level);
            }
        }
        function SetFinal(val) {
            my.data("isfinal", val);
            if (hidden != null) hidden.attr("isFinal", val);
        }

        
        InitSelector();
    }
    $.fn.RegionSelectorFuns = {
        GetValue: function () {

        }
    };
})(jQuery);
