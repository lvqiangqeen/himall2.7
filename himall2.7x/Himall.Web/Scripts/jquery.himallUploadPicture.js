
(function ($) {
    function uploadfile(target, opts) {
		var imgupfile = $(target).find("input:file").eq(0); //上传控件
		var ImageHideFile = $(target).find("input:hidden").eq(0); //图片隐藏域。
		var imgFile = $(target).find("img").eq(0); 

		if (imgupfile.val() == "") {
			$.dialog.errorTips("请选择要上传的图片！");
			return false;
		}
		else {
			if (!checkImgType(imgupfile.val())) {
				$.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp",'',3);
				return false;
			}
		}
		
		var flag = true;
		if (imgupfile[0].files != null) {
		    uploadFilesCount = imgupfile[0].files.length;
		    $(imgupfile[0].files).each(function (index, item) {
		        if (item.size / 1024 > opts.maxSize * 1024) {
		            flag = false;
		            return;
		        }
		    });
		}
		if (!flag) {
		    $.dialog.errorTips("上传的图片不能超过" + opts.maxSize + "M");
		    return;
		}
		//准备表当
		var myform = document.createElement("form");
		myform.action = opts.url;
		myform.method = "post";
		myform.enctype = "multipart/form-data";
		myform.style.display = "none";
		//将表单加当document上，
		document.body.appendChild(myform);  //重点
		var form = $(myform);

		var fu = imgupfile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
		var fu1 = imgupfile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。

		//开始模拟提交表当。
		form.ajaxSubmit({
		    success: function (data) {
			    if (data == "NoFile" || data == "Error" || data == "格式不正确！") {
			        $.dialog.errorTips(data);
			    }
				else {
					//文件上传成功，返回图片的路径。将路经赋给隐藏域
					ImageHideFile.val(data);
					imgFile[0].src=data;
					
					$(target).find('a').show();
					
				}
				$(fu1).insertAfter($(target).find('a'));
				form.remove();

			}
		});
	}


	//检查上传的图片格式
	function checkImgType(filename) {
		var pos = filename.lastIndexOf(".");
		var str = filename.substring(pos, filename.length)
		var str1 = str.toLowerCase();
		if (!/\.(gif|jpg|jpeg|png|bmp)$/.test(str1)) {
			return false;
		}
		return true;
	}

	function bindEvent(target, opts) {
		$('input.uploadFilebtn', $(target)).change(function () {
			uploadfile(target, opts);
		});
		
		$('a', $(target)).click(function() {
			$(this).hide().siblings('img').attr('src','').siblings('input').val('');
		});
		
	}

	$.fn.himallUploadPic = function (options, param) {
	    if (typeof options == "string") {
	        return $.fn.himallUploadPic.methods[options](this, param);
	    }
	    options = options || {};
	    return this.each(function () {
	        var opts = $.extend({}, $.fn.himallUploadPic.defaults, options);
	        opts.view.render.call(opts.view, opts, $(this));
	        bindEvent(this, opts);
	    });
	};

	$.fn.himallUploadPic.methods = {
	    getImgSrc: function (jQ) {
	        return $(jQ).find('input.hiddenImgSrc').val();
	    }
	};

	var renderView = {
		render: function (opts, container) {
			var html ='<img width="40" height="40" src="' + opts.displayImgSrc + '" />'+
				'<a style="cursor: pointer;'+(opts.displayImgSrc?'display:inline-block;':'')+'">删除</a>'+
				'<input class="file uploadFilebtn" type="file" name="_file"  />'+
				'<input class="hiddenImgSrc" type="hidden" value="' + opts.displayImgSrc + '"  name="' + opts.imgFieldName + '">';
			$(container).html(html);
		}
	};

	$.fn.himallUploadPic.defaults = {
	    url: '/common/PublicOperation/UploadPic',
		displayImgSrc:'',
		imgFieldName: 'Icon',
		view: renderView,
		maxSize: 2////M为单位

	};
})(jQuery);