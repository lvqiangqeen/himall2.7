//上传主程序
//var UPLOADID=0;
(function ($) {
    function uploadfile(target, opts) {
		var imgupfile = $(target).find("input:file").eq(0); //上传控件
		var ImageHideFile = $(target).find("input:hidden").eq(0); //图片隐藏域。

		if (imgupfile.val() == "") {
			$.dialog.errorTips("请选择要上传的文件！");
			return false;
		}
		else {
			if (!checkImgType(imgupfile.val())) {
			    $.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp、doc、docx、xls、xlsx、pdf、dwg", '', 3);
				return false;
			}
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
					//$(target).find('.glyphicon-picture').addClass('active');
				}
				$(fu1).prependTo($(target));
				form.remove();

			}
		});
	}


	//检查上传的图片格式
	function checkImgType(filename) {
		var pos = filename.lastIndexOf(".");
		var str = filename.substring(pos, filename.length)
		var str1 = str.toLowerCase();
		if (!/\.(gif|jpg|jpeg|png|bmp|doc|docx|xls|xlsx|pdf|dwg)$/.test(str1)) {
			return false;
		}
		return true;
	}

	function bindEvent(target, opts) {
		$('input.uploadFilebtn', $(target)).change(function () {
			uploadfile(target, opts);
		});
		
		//$(target).attr('uploadId',UPLOADID++);
		/*$(target).find('.glyphicon-picture').each(function() {
			var imSrc= $(this).parent().prev('input').val();
			if(imSrc!=''){
				$(this).addClass('active');
			}
        });*/
		
		
	}

	$.fn.himallUpload = function (options, param) {
	    if (typeof options == "string") {
	        return $.fn.himallUpload.methods[options](this, param);
	    }
	    options = options || {};
	    return this.each(function () {
	        var opts = $.extend({}, $.fn.himallUpload.defaults, options);
	        opts.view.render.call(opts.view, opts, $(this));
	        bindEvent(this, opts);
	    });
	};

	$.fn.himallUpload.methods = {
	    getImgSrc: function (jQ) {
	        return $(jQ).find('input.hiddenImgSrc').val();
	    }
	};

	var renderView = {
		render: function (opts, container) {
			var html = [
                '<label class="up-title-' + opts.headerWidth+ '" for="">' + opts.title + '</label><input class="hiddenImgSrc" type="hidden" value="' + opts.displayImgSrc + '"  name="' + opts.imgFieldName + '">',
                '<div class="up-file-' + opts.dataWidth + '"><input class="file uploadFilebtn" type="file" name="_file"  />',
                '<span class="help-default">' + opts.imageDescript + '</span></div>'];
			//console.log(container);
			$(container).html(html.join(''));
		}
	};

	$.fn.himallUpload.defaults = {
	    url: '/common/PublicOperation/UploadPic',
        displayImgSrc:'',
		title: '文件：',
		imageDescript: '请上传正确格式的文档',
		imgFieldName: 'File',
		view: renderView,
		headerWidth: 1,
		dataWidth: 1
	};
})(jQuery);