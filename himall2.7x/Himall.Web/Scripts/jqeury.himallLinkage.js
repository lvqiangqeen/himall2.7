//多级联动

(function ($) {
    var container;

    function getData(url,level,key) {
        var data;
        $.ajax({
            url: url,
            async: false,
            data: { level: level, key: key },
            type: "post",
            dataType: "json",
            success: function (returnData) {
                data = returnData;
            }
        });
        return data;
    }

    var selectors = [];

    var selectedItems = [];

    function clear(startIndex) {
        for (var i = startIndex; i < selectors.length; i++) {
            if (selectors[i]) {
                selectors[i].remove();
                selectors[i] = null;
            }
        }
    }

    function drawSelect(level, key) {
        var newLevel = level + 1;
        var selector = selectors[newLevel];
        if (key == $.fn.himallLinkage.options.defaultItemsValue[level])
            clear(newLevel);
        if (!selector) {
            selector = $('<select class="' + $.fn.himallLinkage.options.styleClass + '"></select>');
            selectors[newLevel] = selector;
            selector.appendTo(container);
        }
        else {
            clear(newLevel + 1);
        }

        selector.empty();
        var data = getData($.fn.himallLinkage.options.url, level, key);
        if (data.length > 0) {
            if ($.fn.himallLinkage.options.enableDefaultItem) {
                var text = '<option ';
                if ($.fn.himallLinkage.options.defaultItemsValue[newLevel])
                    text += ' value="' + $.fn.himallLinkage.options.defaultItemsValue[newLevel] + '"';
                text += '>' + $.fn.himallLinkage.options.defaultItemsText[newLevel] + '</option>';
                selector.append(text);
            }
            $.each(data, function (i, item) {
                selector.append('<option value="' + (item.key ? item.key : item.Key) + '">' + (item.value ? item.value : item.Value) + '</option>');
            });

            selector.unbind('change').change(function (item) {
                selectedItems[newLevel] = $(this).val();
                if (newLevel < $.fn.himallLinkage.options.level - 1)
                    drawSelect(newLevel, $(this).val());
                if ($.fn.himallLinkage.options.onChange)
                    $.fn.himallLinkage.options.onChange(newLevel, $(this).val(), $(this).text());
            });
        }
        else
            clear(newLevel);
    }


    function setDefaultItem() {
        if ($.fn.himallLinkage.options.enableDefaultItem) {
            if (!$.isArray($.fn.himallLinkage.options.defaultItemsValue)) {
                var arr = [];
                var defaultVallue = $.fn.himallLinkage.options.defaultItemsValue;
                if (defaultVallue == null)
                    defaultVallue = '';
                var i = $.fn.himallLinkage.options.level;
                while (i--) arr.push(defaultVallue);
                $.fn.himallLinkage.options.defaultItemsValue = arr;
            }
            else if ($.fn.himallLinkage.options.defaultItemsValue.length < $.fn.himallLinkage.options.level) {
                var less = $.fn.himallLinkage.options.level - $.fn.himallLinkage.options.defaultItemsValue.length;
                while (less--)
                    $.fn.himallLinkage.options.defaultItemsValue.push('');
            }

            if (!$.isArray($.fn.himallLinkage.options.defaultItemsText)) {
                var arr = [];
                var defaultText = $.fn.himallLinkage.options.defaultItemsText;
                if (defaultText == null)
                    defaultText = '请选择';
                var i = $.fn.himallLinkage.options.level;
                while (i--) arr.push(defaultText);
                $.fn.himallLinkage.options.defaultItemsText = arr;
            }
            else {
                var itemLength = $.fn.himallLinkage.options.defaultItemsText.length;
                if (itemLength < $.fn.himallLinkage.options.level) {
                    var less = $.fn.himallLinkage.options.level - itemLength;
                    while (less--)
                        $.fn.himallLinkage.options.defaultItemsText.push('请选择');
                }
            }
        }
    }

    $.fn.himallLinkage = function (options, params) {
        /// <param name="params" type="object">$.fn.himallLinkage.options</param>

        if (typeof options == "string") {
            return $.fn.himallLinkage.methods[options](this, params);
        }

        container = $(this);
        $.fn.himallLinkage.options = $.extend({}, $.fn.himallLinkage.options, options);
        setDefaultItem();
        drawSelect(-1);
        return $;
    }

    $.fn.himallLinkage.options = {
        level: 1,//级数
        url: null,//调用地址
        selectorWidth: 120,//select框宽度
        styleClass: '',//select框样式
        enableDefaultItem: false,//是否显示默认项（即未选中时的项）
        defaultItemsText: [],//默认文本，可以是数组，也可以是统一的值
        defaultItemsValue: [],//默认值，可以是数组，也可以是统一的值
        onChange:null//select 的change事件
    };


    $.fn.himallLinkage.methods = {
        value: function (jquery,level) {
            return selectedItems[level];
        }
    }

})(jQuery);