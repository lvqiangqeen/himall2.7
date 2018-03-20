//上传主程序
//var UPLOADID=0;
(function ($) {
    function uploadfile(target, opts) {
        var imgupfile = $(target).find("input:file").eq(0); //上传控件
        var ImageHideFile = $(target).find("input:hidden").eq(0); //图片隐藏域。
        if (imgupfile.val() == "") {
            $.dialog.errorTips("请选择要上传的图片！");
            return false;
        } else {
            var uploadFilesCount = imgupfile.length;
            var avaliableFilesCount = 1;
            $(target).parent().find('div[imageBox]').each(function () {
                if ($(this).css('display') == 'none')
                    avaliableFilesCount++;
            });
            if (uploadFilesCount > avaliableFilesCount) {
                $.dialog.errorTips('最多还能上传' + avaliableFilesCount + '张图片，请重新选择');
                return;
            }

            if (!checkImgType(imgupfile.val(), opts.imgTypes)) {
                if (opts.imgTypes == "") {
                    $.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp");
                } else {
                    $.dialog.errorTips("上传格式为" + opts.imgTypes);
                }

                return false;
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
        }

        //准备表当
        var myform = document.createElement("form");
        myform.action = opts.url;
        myform.method = "post";
        myform.enctype = "multipart/form-data";
        myform.style.display = "none";
        //将表单加当document上，
        document.body.appendChild(myform); //重点
        var form = $(myform);

        var fu = imgupfile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
        var fu1 = imgupfile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。



        //开始模拟提交表当。
        form.ajaxSubmit({
            success: function (data) {
                //文件上传成功，返回图片的路径。将路经赋给隐藏域
                var filesData = data;
                var files = filesData.split(',');
                var container = $(target).parent();
                var imageBoxes = container.find('div[imageBox]');
                var images = container.find('img');
                var uploadBtns = container.find('.img-upload-btn');
                var hiddenImgSrces = container.find('input.hiddenImgSrc');
                var started = false;
                var lastIndex = 0;
                for (var i = 0, j = 0; i < opts.imagesCount; i++) {
                    if (imageBoxes[i] == target)
                        started = true;
                    if (started && files[j]) {
                        lastIndex = i;
                        $(images[i]).attr('src', files[j]).show();
                        changeTarget(files[j], opts.target, opts.targetType);
                        $(hiddenImgSrces[i]).val(files[j++]);
                        $(uploadBtns[i]).hide();
                        $(imageBoxes[i]).show();
                    }
                }
                if (lastIndex + 1 < opts.imagesCount)
                    $(imageBoxes[lastIndex + 1]).show();


                $(fu1).insertAfter($(target).find("input.hiddenImgSrc", $(target)));
                form.remove();
				$("span.remove-img", $(target)).show();
				$("span.remove-img", $(target)).click(function () {
					$("img.img-upload", $(this).parent()).attr("src", "").hide();
					$("div.img-upload-btn", $(this).parent()).show();
					$("input.hiddenImgSrc", $(this).parent()).val("");
					$("input.file", $(this).parent()).val("");
					$(this).hide();
					changeTarget("", opts.target, opts.targetType);
				});
            }
        });

    }


    function changeTarget(fileURL, target, targetType) {
        var url = "";
        switch (targetType) {
            case "background-image":
                url = "url(" + fileURL + ")";
                $(target).css(targetType, url)
                break;
            case "src":
                url = fileURL;
                $(target).attr(targetType, url);
                break;
        }
    }

    //检查上传的图片格式
    function checkImgType(filename, imgTypes) {
        var pos = filename.lastIndexOf(".");
        var str = filename.substring(pos, filename.length)
        var str1 = str.toLowerCase();
        if (imgTypes == "") {
            if (!/\.(gif|jpg|jpeg|png|bmp)$/.test(str1)) {
                return false;
            }
        } else {
            var regExp = new RegExp("^.(" + imgTypes + ")$");
            if (!regExp.test(str1)) {
                return false;
            }
        }

        return true;
    }

    function bindEvent(target, opts) {
        $('input.uploadFilebtn', $(target)).change(function () {
            uploadfile(target, opts);
        });

        //鼠标移入图片显示删除按钮

        if (!opts.isMobile) {
            $(target).mouseenter(function () {
                if (opts.disabled)
                    return;
                if ($("input.hiddenImgSrc", $(target)).val() != "")
                    $("span.remove-img", $(this)).show();
                else
                    $("span.remove-img", $(this)).hide();
            });
            
            $(target).mouseleave(function () {
                if ($("input.hiddenImgSrc", $(this)).val() != "") {
                    $("span.remove-img", $(this)).hide();
                }
            });
        }
        else {
            if (opts.disabled)
                return;
			
        }
		if ($("input.hiddenImgSrc", $(target)).val() != "") {
			$("span.remove-img", $(target)).click(function () {
				$("img.img-upload", $(this).parent()).attr("src", "").hide();
				$("div.img-upload-btn", $(this).parent()).show();
				$("div.glyphicon-plus", $(this).parent()).show();
				$("input.hiddenImgSrc", $(this).parent()).val("");
				$("input.file", $(this).parent()).val("");
				$(this).hide();
				changeTarget("", opts.target, opts.targetType);
			});
			
		}
    }

    $.fn.uploadPictures = function (options, param) {
        if (typeof options == "string") {
            return $.fn.uploadPictures.methods[options](this, param);
        }
        options = options || {};
        return this.each(function () {
            var opts = $.extend({}, $.fn.uploadPictures.defaults, options);
            opts.view.render.call(opts.view, opts, $(this));
            $(this).find('div[imagebox]').each(function () {
                bindEvent(this, opts);
            });
        });
    };

    $.fn.uploadPictures.methods = {
        getImgSrc: function (jQ) {
            var images = $(jQ).find('input.hiddenImgSrc');
            var srcArr = [];
            images.each(function () {
                var src = $(this).val();
                if (src)
                    srcArr.push(src);
            });
            return srcArr;
        }
    };

    var renderView = {
        render: function (opts, container) {
            var id = new Date().getTime();
            var i = 0;
            var html = '<div class="clearfix">'; //opts.title ? '<span class="img-upload-error" id="span_error"></span>' : '';
            if (!Array.isArray(opts.displayImgSrc)) {
                opts.displayImgSrc = opts.displayImgSrc.split(",");
            }
            for (var i = 0; i < opts.imagesCount; i++) {
                var display = opts.displayImgSrc[i] || i == 0 || (opts.displayImgSrc[i - 1]) ? '' : ' style="display:none"';
                html += ' <div class="upload-img-box" imagebox ' + display + '>\
                                 <img src="' + (opts.displayImgSrc[i] ? opts.displayImgSrc[i] : opts.defaultImg) + '" style="display:' + (opts.displayImgSrc[i] ? 'block' : 'none') + '" class="img-upload" onclick="$(\'#imgUploader_' + id + '_' + i + '\').click()" />\
                                 <span class="remove-img" style="display:none;">删除</span>\
                                 <div class="img-upload-btn" onclick="$(\'#imgUploader_' + id + '_' + i + '\').click()" style="display:' + (!opts.displayImgSrc[i] ? 'block' : 'none') + '">';
								 if(!opts.isMobile)
								 	html += '+';
								 else 
								 	html += '<i class="glyphicon glyphicon-camera"></i>';
								 html += '</div>\
                                <input type="hidden" class="hiddenImgSrc"  value="' + (opts.displayImgSrc[i] ? opts.displayImgSrc[i] : '') + '"   name="' + opts.imgFieldName + '" />\
                                <input class="file uploadFilebtn" type="file" multiple name="_file"   id="imgUploader_' + id + '_' + i + '"/>\
                                <span class="text-muted">' + opts.pictureSize + '</span>\
                             </div>';
            }
            html += '</div>';
            
            $(container).html(html);
            if (opts.disabled) {
                $("div.upload-img-box input:hidden.hiddenImgSrc").each(function () {
                    if ($(this).val() == "") {
                        $(this).parent().remove();
                    }
                    else {
                        $(this).next().remove();
                    }

                });
            }
        }
    };

    $.fn.uploadPictures.defaults = {
        url: '/common/PublicOperation/UploadPic',
        displayImgSrc: '',
        title: '图片：',
        imageDescript: '请上传16px * 16px的图片',
        imgFieldName: 'Icon',
        view: renderView,
        defaultImg: '',
        imagesCount: 1,
        disabled: false,
        target: '',
        targetType: '',
        imgTypes: '',
		maxSize: 6,
        isMobile: false
    };
})(jQuery);
