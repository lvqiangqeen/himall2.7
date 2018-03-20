//上传主程序
//var UPLOADID=0;
(function ($) {
    function uploadfile(target, opts) {
		var imgupfile = $(target).find("input:file").eq(0); //上传控件
		var ImageHideFile = $(target).find("input:hidden").eq(0); //图片隐藏域。

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
			    //var erro = $(data).find("h2 i");
			    if (data == "NoFile" || data == "Error" || data == "格式不正确！") {
			        $.dialog.errorTips(data);
			    }
			   //else if ($(data).find("h2 i").length > 0) {
			   //    $.dialog.errorTips("上传的图片大小超过限制");
			   // }
				else {
					//文件上传成功，返回图片的路径。将路经赋给隐藏域
					ImageHideFile.val(data);
					$(target).find('.glyphicon-picture').addClass('active');
					if (opts.callback) {
					    opts.callback(data);
					}
				}
				$(fu1).insertAfter($("span.glyphicon-picture", $(target)));
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
		
		//$(target).attr('uploadId',UPLOADID++);
		$(target).find('.glyphicon-picture').each(function() {
			var imSrc= $(this).parent().prev('input').val();
			if(imSrc!=''){
				$(this).addClass('active');
			}
			$(this).mouseenter(function(){
				var position = $(this).offset();
				var src = $(this).parent().prev('input').val();
				var random = Math.random();
				var imgstr = '<div class="lg-view-img"><img src="' + src + '?version=' + random + '"></div>';
                if (src != '') {
                    $(this).append(imgstr);
                    
                    // 图片位置控制，防止最底部图片或者最顶部图片看不完全
                    var that = null;
                    var imgH = parseInt( $('.lg-view-img').outerHeight(true) );
                    console.log(imgH);
                    function picturePosition() {
                        that = $(this);
                        scroll = that.scrollTop();
                        if ( position.top - scroll >= imgH ) {
                            $('.lg-view-img').css({ 'top': 20 })
                        } else {
                            $('.lg-view-img').css({ 'top': 20 })
                        }
                    }
                    picturePosition();
                    $(window).scroll(function() {
                        picturePosition();
                    });
                    
                }
			});
			$(this).mouseleave(function(){
				$('.lg-view-img').remove();
			});
        });
		
		
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
                    '<label class="col-sm-' + opts.headerWidth+ ' control-label fl" for="">' + opts.title + '</label><input class="hiddenImgSrc" type="hidden" value="' + opts.displayImgSrc + '"  name="' + opts.imgFieldName + '">',
                    '<div class="col-sm-' + opts.dataWidth + ' upl-right"><span class="glyphicon glyphicon-picture"></span><input class="file uploadFilebtn" type="file" name="_file"  />',
                    '<span class="help-default">' + opts.imageDescript + '</span></div>'];
			//console.log(container);
			$(container).html(html.join(''));
		}
	};

	$.fn.himallUpload.defaults = {
	    url: '/common/PublicOperation/UploadPic',
        displayImgSrc:'',
		title: '图片',
		imageDescript: '建议上传50px * 50px的图片',
		imgFieldName: 'Icon',
		view: renderView,
		callback:null,
		headerWidth: 2,
		dataWidth: 4,
		maxSize: 2////M为单位

	};
})(jQuery);