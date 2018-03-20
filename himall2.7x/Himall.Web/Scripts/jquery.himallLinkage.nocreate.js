(function ($) {

    function getData(url, level, key) {
        var data;
        if (level == -1 || key) {
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
        }
        else
            data = [];
        return data;
    }

    var selectors = {};
    var uid = 0;

    var selectedItems = {};

    var himallLinkageOptions = {};

    function setDefaultItem(index) {
        if (himallLinkageOptions[index].enableDefaultItem) {
            if (!$.isArray(himallLinkageOptions[index].defaultItemsValue)) {
                var arr = [];
                var defaultVallue = himallLinkageOptions[index].defaultItemsValue;
                if (defaultVallue == null)
                    defaultVallue = '';
                var i = himallLinkageOptions[index].level;
                while (i--) arr.push(defaultVallue);
                himallLinkageOptions[index].defaultItemsValue = arr;
            }
            else if (himallLinkageOptions[index].defaultItemsValue.length < himallLinkageOptions[index].level) {
                var less = himallLinkageOptions[index].level - himallLinkageOptions[index].defaultItemsValue.length;
                while (less--)
                    himallLinkageOptions[index].defaultItemsValue.push('');
            }

            if (!$.isArray(himallLinkageOptions[index].defaultItemsText)) {
                var arr = [];
                var defaultText = himallLinkageOptions[index].defaultItemsText;
                if (defaultText == null)
                    defaultText = '请选择';
                var i = himallLinkageOptions[index].level;
                while (i--) arr.push(defaultText);
                himallLinkageOptions[index].defaultItemsText = arr;
            }
            else {
                var itemLength = himallLinkageOptions[index].defaultItemsText.length;
                if (itemLength < himallLinkageOptions[index].level) {
                    var less = himallLinkageOptions[index].level - itemLength;
                    while (less--)
                        himallLinkageOptions[index].defaultItemsText.push('请选择');
                }
            }
        }
    }


    function clear(startIndex, index) {
        for (var i = startIndex; i < selectors[index].length; i++) {
            if (selectors[index][i]) {
                if (himallLinkageOptions[index].displayWhenNull)
                    selectors[index][i].empty().attr('disabled', 'disabled');
                else
                    selectors[index][i].empty().hide();
            }
        }
    }


    function drawSelect(level, key, index) {
        var newLevel = level + 1;
        var selector = selectors[index][newLevel];
        if (key == himallLinkageOptions[index].defaultItemsValue[level])
            clear(newLevel,index);

        if (himallLinkageOptions[index].displayWhenNull)
            selector.empty().removeAttr('disabled');
        else
            selector.empty().show();
        var data = getData(himallLinkageOptions[index].url, level, key);
        if (data != null && data.length > 0) {
            if (himallLinkageOptions[index].enableDefaultItem) {
                var text = '<option ';
                if (himallLinkageOptions[index].defaultItemsValue[newLevel])
                    text += ' value="' + himallLinkageOptions[index].defaultItemsValue[newLevel] + '"';
                else
                    text += ' value=""';
                text += '>' + himallLinkageOptions[index].defaultItemsText[newLevel] + '</option>';
                selector.append(text);
            }
            $.each(data, function (i, item) {
                selector.append('<option value="' + (item.key ? item.key : item.Key) + '">' + (item.value ? item.value : item.Value) + '</option>');
            });

            selector.unbind('change').change(function (item) {
                var currentIndex = parseInt($(this).attr('linkageId'));
                selectedItems[currentIndex][newLevel] = $(this).val();
                if (newLevel < himallLinkageOptions[currentIndex].level - 1)
                    drawSelect(newLevel, $(this).val(), currentIndex);
                if (himallLinkageOptions[currentIndex].onChange)
                    himallLinkageOptions[currentIndex].onChange(newLevel, $(this).val(), $(this).find('option:selected').text());
            });
        }
        else
            clear(newLevel, index);
        var currentIndex = parseInt(selector.attr('linkageId'));
        if (newLevel < himallLinkageOptions[currentIndex].level -1 )
            drawSelect(newLevel,selector.val(), currentIndex);
    }

    function selectValue(level, key, index) {
        var selector = selectors[index][level];
        selector.val(key);
        if (level + 1 < himallLinkageOptions[index].level)
            drawSelect(level, key, index);
    }

    function initDefaultSelectedValues(index) {
        var selectedValues = himallLinkageOptions[index].defaultSelectedValues;
        if (selectedValues && selectedValues.length > 0 && parseInt(selectedValues[0])) {
            selectedValues = [0].concat(selectedValues);
            himallLinkageOptions[index].defaultSelectedValues = selectedValues;
        }

        for (var i = 1; i < selectedValues.length; i++)
            selectValue(i - 1, selectedValues[i], index);
    }


    $.fn.himallLinkage = function (options, params) {
        /// <param name="params" type="object">$.fn.himallLinkage.options</param>
        if (typeof options == "string") {
            return $.fn.himallLinkage.methods[options](this, params);
        }
        resetData(uid);
        selectors[uid] = [];
        $.each($(this), function (i, item) {
            $(item).attr('linkageId', uid);
            selectors[uid].push($(item));
        })
        $.fn.himallLinkage.options = $.extend({}, $.fn.himallLinkage.options, options);
        $.fn.himallLinkage.options.level = selectors[uid].length;
        himallLinkageOptions[uid] = $.fn.himallLinkage.options;
        setDefaultItem(uid);
        drawSelect(-1, null, uid);
        initDefaultSelectedValues(uid);
        uid++;
        return $;
    }

    $.fn.himallLinkage.options = {
        url: null,//调用地址
        enableDefaultItem: false,//是否显示默认项（即未选中时的项）
        defaultItemsText: [],//默认文本，可以是数组，也可以是统一的值
        defaultItemsValue: [],//默认值，可以是数组，也可以是统一的值
        onChange: null,//select 的change事件
        displayWhenNull: true,//
        defaultSelectedValues:[]
    };


    $.fn.himallLinkage.methods = {
        value: function (jquery, level) {
            var index = parseInt($(jquery).attr('linkageId'));
            return selectedItems[index][level];
        },
        reset: function (jquery) {
            var index =  parseInt($(jquery).attr('linkageId'));
            drawSelect(-1, null, index);
        }
    }

    function resetData(index) {
        selectors[index] = [];
        selectedItems[index] = [];
    }

})(jQuery);