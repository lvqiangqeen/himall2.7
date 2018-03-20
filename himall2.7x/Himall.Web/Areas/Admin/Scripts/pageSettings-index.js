$(function () {
    InitUpload();
    SetLogo();
    SetKeyWords();
    SetRecommand();
    SetBottomPic();
    SetAdvertisement();
});

function InitUpload() {
    $("#uploadImg").himallUpload(
   {
       displayImgSrc: logo,
       imgFieldName: "Logo",
       title: 'LOGO',
       imageDescript: '200*60',
       dataWidth: 8
   });
    
    $("#uploadBotPic").himallUpload(
   {
       displayImgSrc: bottompic,
       imgFieldName: "bottompic",
       title: '底部服务图:',
       imageDescript: '1190*106',
       dataWidth: 7
   });
}

//设置LOGO
function SetLogo() {
    $('.logo-area').click(function () {
        $.dialog({
            title: 'LOGO设置',
            lock: true,
            width:365,
            id: 'logoArea',
            content: document.getElementById("logosetting"),
            padding: '0 40px',
            okVal: '保存',
            ok: function () {
                var logosrc = $("input[name='Logo']").val();
                if (logosrc == "") {
                    $.dialog.tips("请上传一张LOGO图片！");
                    return false;
                }
                var loading = showLoading();
                $.post(setlogoUrl, { logo: logosrc },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("LOGO修改成功！");
                            $("input[name='Logo']").val(data.logo);
                            logo = data.logo;
                        }
                        else { $.dialog.errorTips("LOGO修改失败！") }
                    });
            }
        });
    });
}

//设置广告
function SetAdvertisement() {
	$('#advertisement').click(function () {
		var $this = $(this);
		var thisPic = $this.attr('pic');
		var thisUrl = $this.attr('url');
		var imageDescript = $this.attr('dir');
		var state = $this.attr('state');

		$.dialog({
			title: '广告栏编辑',
			lock: true,
			width: 430,
			padding: '0 40px',
			id: 'advertisement',
			content: ['<div class="dialog-form">',
						'<div id="HandSlidePic" class="form-group upload-img clearfix"></div>',
						'<div class="form-group clearfix">',
							'<label class="label-inline fl" style="margin-left:-30px" for="">跳转链接</label>',
                            '&nbsp;&nbsp;&nbsp;&nbsp;',
							'<input class="form-control input-sm" type="text" id="url"/>',
						'</div>',
						'<div class="form-group clearfix">',
							'<label class="label-inline fl" style="margin-left:-30px" for="">是否显示</label>',
                            '&nbsp;&nbsp;&nbsp;&nbsp;',
							'<label class="mr10 mb0 align-middle"><input type="radio" id="advertisementState_open" name="advertisementState" value="true"/>显示</label>',
							'<label class="mb0 mb0 align-middle"><input type="radio" id="advertisementState_close" name="advertisementState" value="false"/>不显示</label>',
						'</div>',
					'</div>'].join(''),
			okVal: '保存',
			init: function () {
				$("#HandSlidePic").himallUpload(
                {
                	title: '显示图片',
                	imageDescript: imageDescript,
                	displayImgSrc: thisPic,
                	imgFieldName: "HandSlidePic",
                	dataWidth: 8
                });
				$("#url").val(thisUrl);
				$('#advertisementState_' + state).get(0).checked = true;
			},
			ok: function () {
				var valida = false;
				var url = $("#url").val();
				var pic = $("#HandSlidePic").himallUpload('getImgSrc');
				if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }

				if (url.toLowerCase().indexOf('http://') < 0 && url.toLowerCase().indexOf('https://') < 0 && url.charAt(0) != '/') {
					$("#url").focus(); $.dialog.errorTips('链接地址请以"http://"或"https://"开头'); return valida;
				}

				if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }
				var open = $('input[name=advertisementState]:radio:checked').val();
				var loading = showLoading();
				ajaxRequest({
					type: 'POST',
					url: setAdvertisementUrl,
					cache: false,
					param: { url: url, img: pic, open: open },
					dataType: 'json',
					success: function (data) {
						loading.close();
						if (data.success == true) {
							$('#advertisement').attr({ url: url, pic: pic, state: open=='true'?'open':'close' });
							$.dialog.succeedTips("广告修改成功！");
						}
					},
					error: function (data) {
						loading.close();
						$.dialog.errorTips('操作失败,请稍候尝试.');
					}
				});
			}
		});
	});
}

//设置底部服务图片
function SetBottomPic() {
    $('.bottompic-area').click(function () {
        $.dialog({
            title: '底部服务图片设置',
            lock: true,
            width:420,
            id: 'bottompicArea',
            content: document.getElementById("bottompicbox"),
            padding: '0 40px',
            okVal: '保存',
            ok: function () {
                var picsrc = $("input[name='bottompic']").val();
                if (picsrc == "") {
                    $.dialog.tips("请上传一张底部服务图片！");
                    return false;
                }
                var loading = showLoading();
                $.post(setbottompicUrl, { pic: picsrc },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("底部服务图片修改成功！");
                            $("input[name='bottompic']").val(data.logo);
                            bottompic = data.pic;
                        }
                        else { $.dialog.errorTips("底部服务图片修改失败！") }
                    });
            }
        });
    });
}


//热门关键字设置
function SetKeyWords() {
    $('.search-area').click(function () {
        $("#txtkeyword").val(keyword);
        $("#txthotkeywords").val(hotkeywords);
        $.dialog({
            title: '热门关键字设置',
            lock: true,
            id: 'searchArea',
            content: document.getElementById("keywordsSettting"),
            padding: '0 40px',
            okVal: '保存',
            ok: function () {
                var word = $("#txtkeyword").val().replace(/(^\s*)|(\s*$)/g, "");;
                var words = $("#txthotkeywords").val().replace(/(^\s*)|(\s*$)/g, "");;
                if (word == "" || words == "") {
                    $.dialog.tips("请填写关键字！");
                    return false;
                }
                var loading = showLoading();
                $.post(setkeyWords, { keyword: word, hotkeywords: words },
                     function (data) {
                         loading.close();
                         if (data.success) {
                             $.dialog.succeedTips("关键字设置成功！");
                             logo = data.logo;
                             keyword = word;
                             hotkeywords = words;
                         }
                         else { $.dialog.succeedTips(data.msg); }
                     });
            }
        });
    });
}

//LOGO设置

function SetRecommand() {

    //橱窗1编辑
    $('.imageAdRecommend').click(function () {
        var that = this;
        var thisPic = $(this).attr('pic');
        var thisUrl = $(this).attr('url');
        var value = $(this).attr('value');
        var imageDescript = $(this).attr('dir');
        //var imageDescript = "170*76";

        //switch (parseInt(value)) {
        //    case 1:
        //    case 8:
        //        imageDescript = "464*288";
        //        break;
        //    case 2:
        //    case 3:
        //    case 4:
        //    case 5:
        //    case 6:
        //    case 7:
        //        imageDescript = "226*288";
        //        break;
        //    case 13:
        //        imageDescript = "464*288";
        //        break;
        //    case 15:
        //        imageDescript = "1000*75";
        //        break;

        //}
        $.dialog({
            title: '推荐商品编辑',
            lock: true,
            width:430,
            padding: '0 40px',
            id: 'goodsArea',
            content: ['<div class="dialog-form">',
                '<div id="HandSlidePic" class="form-group upload-img clearfix">',
                '</div>',
                '<div class="form-group clearfix">',
                    '<label class="label-inline fl" style="margin-left:-30px" for="">跳转链接</label>',
                    '&nbsp;&nbsp;&nbsp;&nbsp;',
                    '<input class="form-control input-sm" type="text" id="url">',
                '</div>',
            '</div>'].join(''),
            okVal: '保存',
            init: function () {
                $("#HandSlidePic").himallUpload(
                {
                    title: '显示图片',
                    imageDescript: imageDescript,
                    displayImgSrc: thisPic,
                    imgFieldName: "HandSlidePic",
                    dataWidth: 8
                });
                $("#url").val(thisUrl);
            },
            ok: function () {
                var valida = false;
                var id = parseInt($(that).attr('value'));
                var url = $("#url").val();
                var pic = $("#HandSlidePic").himallUpload('getImgSrc');
                if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }

                if (url.toLowerCase().indexOf('http://') < 0 && url.toLowerCase().indexOf('https://') < 0 && url.charAt(0) != '/') {
                    $("#url").focus(); $.dialog.errorTips('链接地址请以"http://"或"https://"开头'); return valida;
                }

                if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }
                var loading = showLoading();
                ajaxRequest({
                    type: 'POST',
                    url: './SlideAd/UpdateImageAd',
                    cache: false,
                    param: { url: url, pic: pic, id: id },
                    dataType: 'json',
                    success: function (data) {
                        loading.close();
                        if (data.successful == true) {
                            $.dialog.succeedTips("推荐商品修改成功！", function () { location.reload(); });
                        }
                    },
                    error: function (data) {
                        loading.close();
                        $.dialog.errorTips('操作失败,请稍候尝试.');
                    }
                });
            }
        });
    });
}

$(function(){
    $(".dialog-form .upl-right").css("margin", "17px 0!important");
})

