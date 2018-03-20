// JavaScript source code
//function refreshCartProducts() {
//	$.post('/cart/GetCartProducts', {}, function (cart) {
//		var products = cart.products;
//		var count = cart.totalCount;
//		$('#shopping-amount,#cartProductsCount').html(count);
//	});
//}
$(function () {
	var isSellerAdmin = $("#VIseller").val().toLowerCase();
	if (isSellerAdmin == "true") {
		//隐藏 分类管理 页面设置
		window.onload = function () {
			$("a:contains('分类管理'),a:contains('页面设置')").parent().hide();
		};
	}

	//下拉菜单
	$('.top .dropdown').hover(function () {
		$(this).toggleClass('hover');
	});
	//refreshCartProducts();
})

var _getElementById = function (tagName) {
	return window.frames[0].document.getElementById(tagName);
};
function change() {
	var stae1, stae2, stae3;
	window.frames[0].$.dialog({
		title: '修改密码',
		lock: true,
		id: 'ChangePwd',
		width: '400px',
		content: '<div id="changePassword" style="">\
        <div class="dialog-form">\
                <div class="form-group">\
                    <label for="" class="label-inline">旧密码：</label>\
                    <input type="password" value="" id="old"  name="userVo.realName" maxlength="20" class="form-control input-sm">\
                    <p id="old_msg" class="help-block"></p>\
                </div>\
                <div class="form-group">\
                    <label for="" class="label-inline">新密码：</label>\
                    <input type="password" value="" id="new" name="userVo.realName" maxlength="20" class="form-control input-sm">\
                    <p id="new_msg" class="help-block"></p>\
                </div>\
                <div class="form-group">\
                    <label for="" class="label-inline">重复密码：</label>\
                    <input type="password" value="" id="confirm" name="userVo.realName" maxlength="20" class="form-control input-sm">\
                    <p class="help-block" id="confirm_msg" ></p>\
                </div>\
        </div>\
    </div>',
		padding: '0 40px',
		okVal: '确定',
		init: function () {


			//console.log(_getElementById);
			$(_getElementById("old")).focus();

			fn = function (dom, d, callback, msg) {
				$.ajax({
					type: 'post',
					url: $("#COP").val(),
					data: 'password=' + d,
					dataType: "json",
					success: function (data) {
						if (!data.success) {
							callback(dom, data.success, msg, d);
						} else {
							$(_getElementById('old_msg')).html('');
							$(_getElementById("old")).css({ borderColor: '#ccc' });
							stae1 = $(_getElementById(dom)).val();
						}
					}
				});
			},
            handle = function (dom, data, msg, d) {
            	if (data) {
            		stae1 = $(_getElementById(dom)).val();
            	} else {
            		$(_getElementById(dom)).css({ borderColor: '#f60' });
            		$(_getElementById(dom + '_msg')).css({ color: '#f60' }).html(msg);
            		stae1 = '';
            	}
            };

			$(_getElementById("old")).blur(function () {
				//alert();
				var d = $(this).val();
				fn('old', d, handle, '密码错误!');
			});
			$(_getElementById('new')).blur(function () {
				var d = $(this).val();
				if (d.length < 5) {
					$(_getElementById('new')).css({ borderColor: '#f60' });
					$(_getElementById('new_msg')).css({ color: '#f60' }).html('密码不能少于5位!');
					stae2 = '';
				} else {
					$(_getElementById('new')).css({ borderColor: '#ccc' });
					$(_getElementById('new_msg')).css({ color: '#ccc' }).html('');
					stae2 = d;
				}
			});
			$(_getElementById('confirm')).blur(function () {
				var d = $(this).val();
				if (d == $(_getElementById('new')).val()) {
					$(_getElementById('confirm')).css({ borderColor: '#ccc' });
					$(_getElementById('confirm_msg')).css({ color: '#ccc' }).html('');
					stae3 = d;
				} else {
					$(_getElementById('confirm')).css({ borderColor: '#f60' });
					$(_getElementById('confirm_msg')).css({ color: '#f60' }).html('密码不一致!');
					stae3 = '';
				}
			});
		},
		button: [
            {
            	name: '确认',
            	callback: function () {
            		$.ajax({
            			type: 'post',
            			url: $("#UACP").val(),
            			data: { "oldpassword": stae1, "password": stae3 },
            			dataType: "json",
            			success: function (data) {
            				if (data.success) {
            					window.frames[0].$.dialog.succeedTips("密码修改成功！", function () {
            						$(_getElementById("old")).val('');
            						$(_getElementById("new")).val('');
            						$(_getElementById("confirm")).val('');
            						$(_getElementById("ChangePwd")).hide();
            						$(_getElementById("changePassword")).hide();
            					}, 2);
            				} else {
            					window.frames[0].$.dialog.errorTips("保存失败！", '', 3);
            				}
            			}
            		});
            	},
            	focus: true
            },
            {
            	name: '取消',
            }]
	});
}

function logout() {
	$.removeCookie('Himall-User', { path: '/' });
	$.removeCookie('Himall-SellerManager', { path: "/" });
	location.reload();
}

$('.navbar-right .dropdown li').click(function () {
	var flag = $(this).attr('flag');
	$(this).siblings('#flag').val(flag);
});

$(function () {
	var arr = $("#VBr0").val().split(','),
        obj = {};
	if (arr[0] == 0) {
		return;
	}
	; (function (a, o) {
		$.each(a, function (i, e) {
			o[e] = true;
		});
	}(arr, obj));
	$('.id_menu').each(function (i, e) {
		var num = $(e).children().length,
            uid = 0;
		$(e).children().each(function (n, o) {
			var a = $(o).find('a').attr('id');
			if (!obj[a]) {
				$(o).remove();
				$('li[right="_' + a + '"]').remove();
				uid += 1;
			}
			if (uid == num) {
				$(e).parent().remove();
			}
		});
	});

	//refreshCartProducts();
});

//function refreshCartProducts() {
//	$.post('/cart/GetCartProducts', {}, function (cart) {
//		var products = cart.products;
//		var count = cart.totalCount;
//		$('#shopping-amount').html(count);
//	});
//}