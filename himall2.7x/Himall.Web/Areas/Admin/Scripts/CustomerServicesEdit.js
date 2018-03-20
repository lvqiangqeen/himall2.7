// JavaScript source code
$(function () {

    bindSubmitClickEvent();

    $('input:checkbox[name$=".ServerStatus"]').onoff();

	//更改错误显示
    var validate = $("form").validate();
    var elements = validate.elements();
    for (var i = 0; i < elements.length; i++) {
    	var element = elements[i];
    	var $element = $(element);
    	if ($element.data('my-required'))
    		validate.settings.rules[element.name].required = $element.data('my-required');//将required修改为自定义条件，满足条件才验证
    }
	//覆盖默认的验证成功和验证失败事件
    if (validate) {
    	validate.settings.success = function (label, element) {
    		$(element).parent().removeClass('has-error');
    		label.remove();
    	};

    	validate.settings.errorPlacement = function (error, element) {
    		element.parent().addClass('has-error');
    		var str = $(error).text();
    		if ($.notNullOrEmpty(str)) {
    			$.dialog.errorTips(str);
    			var tabPaneIndex = element.closest('.tab-pane').index();
    			$('.nav.nav-tabs li:eq({0})'.format(tabPaneIndex)).click();
    		}
    	};
    }
});

function bindSubmitClickEvent() {
    var form = $("form");
    $('#submit').click(function () {
    	if (form.valid()) {
            var loading = showLoading();
            $.post('CustomerServicesEdit', form.serialize(), function (result) {
                loading.close();
                if (result.success) {
                	for (var newId in result.newIds) {
                		$('#' + newId).val(result.newIds[newId]);
                	}
                    $.dialog.tips('保存成功');
                }
                else
                    $.dialog.errorTips('保存失败!' + result.msg);
            })
        }
    });
};