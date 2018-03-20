/// <reference path="Scratch.js" />
//上传主程序
//var UPLOADID=0;
var defaultMaxSize = 2;//单位M
(function ($) {
    function uploadfile(target, opts) {
        var imgupfile = $(target).find("input:file").eq(0); //上传控件       
        var ImageHideFile = $(target).find("input:hidden").eq(0); //图片隐藏域。
        if (imgupfile.val() == "") {
            $(".Prompt").text("请选择要上传的图片！").show(300).delay(3000).hide(300);
            return false;
        } else {
            var uploadFilesCount = imgupfile.length;
            var avaliableFilesCount = 1;
            $(target).parent().find('div[imageBox]').each(function () {
                if ($(this).css('display') == 'none')
                    avaliableFilesCount++;
            });
            if (uploadFilesCount > avaliableFilesCount) {
                $(".Prompt").text('最多还能上传' + avaliableFilesCount + '张图片，请重新选择').show(300).delay(3000).hide(300);
                return;
            }

            if (!checkImgType(imgupfile.val(), opts.imgTypes)) {
                if (opts.imgTypes == "") {
                    $(".Prompt").text("请上传gif、jpeg、jpg、png、bmp格式的图片").show(300).delay(3000).hide(300);
                } else {
                    $(".Prompt").text("上传格式为" + opts.imgTypes).show(300).delay(3000).hide(300);
                }

                return false;
            }
        }

        //准备表当
        var myform = document.createElement("form");
        var url = opts.url;

        if (opts.foldName && opts.url.replace("remoteupdateimages", "").length < url.length) {
            url = url + "&foldName=" + opts.foldName;
        }
        myform.action = url;
        myform.method = "post";
        myform.enctype = "multipart/form-data";
        myform.style.display = "none";
        //将表单加当document上，
        document.body.appendChild(myform); //重点
        var form = $(myform);

        var fu = imgupfile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
        var fu1 = imgupfile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。
        var hidMaxSize = document.createElement("input");
        hidMaxSize.type = "hidden";
        hidMaxSize.name = "hidFileMaxSize";
        hidMaxSize.value = (opts.fileMaxSize == undefined || isNaN(parseFloat(opts.fileMaxSize))) ? defaultMaxSize : parseFloat(opts.fileMaxSize);
        $(hidMaxSize).appendTo(form);
        if ($("#span_error").length > 0) {
            $("#span_error").text("");
        }

        //开始模拟提交表当。
        form.ajaxSubmit({
            success: function (data) {
                if (data == "NoFile" || data == "Error" || data == "格式不正确！") {
                    $.dialog.errorTips(data);
                } else {
                    //文件上传成功，返回图片的路径。将路经赋给隐藏域
                    var files = data.split(',');
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
                            var imgUrl = files[j];
                            //if (imgUrl.replace("http://", "").length == imgUrl.length && ImageServerUrl) {
                            //    imgUrl = ImageServerUrl + imgUrl;
                            //}
                            $(images[i]).attr('src', imgUrl).show();
                            changeTarget(files[j], opts.target, opts.targetType);
                            $(hiddenImgSrces[i]).val(files[j++]);
                            $(uploadBtns[i]).hide();
                            $(imageBoxes[i]).show();
                        }
                    }
                }
                if (lastIndex + 1 < opts.imagesCount)
                    $(imageBoxes[lastIndex + 1]).show();


                $(fu1).insertAfter($(target).find("input.hiddenImgSrc", $(target)));
                form.remove();

            },
            error: function (obj) {
                var a = obj;
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

        //$(target).attr('uploadId',UPLOADID++);
        $(target).find('.glyphicon-picture').each(function () {
            var imSrc = $(this).parent().prev('input').val();
            if (imSrc != '') {
                $(this).addClass('active');
            }
            $(this).mouseenter(function () {
                var position = $(this).offset();
                var src = $(this).parent().prev('input').val();
                var imgstr = '<div class="lg-view-img"><img src="' + src + '"></div>';
                if (src != '') {
                    $('body').append(imgstr);
                    $('.lg-view-img').css({
                        'top': position.top + 20,
                        'left': position.left
                    })
                }
            });
            $(this).mouseleave(function () {
                $('.lg-view-img').remove();
            });
        });

        //鼠标移入图片显示删除按钮
        $(target).mouseenter(function () {
            if (opts.disabled)
                return;

            if ($("input.hiddenImgSrc", $(this)).val() != "") {
                $("span.remove-img", $(this)).click(function () {
                    $("img.img-upload", $(this).parent()).attr("src", "").hide();
                    $("div.glyphicon-plus", $(this).parent()).show();
                    $("input.hiddenImgSrc", $(this).parent()).val("");
                    $("input.file", $(this).parent()).val("");
                    $(this).hide();
                    changeTarget("", opts.target, opts.targetType);
                });
                $("span.remove-img", $(this)).show();
            }
        });
        $(target).mouseleave(function () {
            if ($("input.hiddenImgSrc", $(this)).val() != "") {
                $("span.remove-img", $(this)).hide();
            }
        });
    }

    $.fn.hishopUpload = function (options, param) {
        if (typeof options == "string") {
            return $.fn.hishopUpload.methods[options](this, param);
        }
        options = options || {};
        return this.each(function () {
            var opts = $.extend({}, $.fn.hishopUpload.defaults, options);
            opts.view.render.call(opts.view, opts, $(this));
            $(this).find('div[imageBox]').each(function () {
                bindEvent(this, opts);
            });
        });
    };

    $.fn.hishopUpload.methods = {
        getImgSrc: function (jQ) {
            var images = $(jQ).find('input.hiddenImgSrc');
            if (images.length == 1)
                return images.val();
            else {
                var srcArr = [];
                images.each(function () {
                    var src = $(this).val();
                    if (src)
                        srcArr.push(src);
                });
                return srcArr;
            }
        }
    };

    var renderView = {
        render: function (opts, container) {
            var id = new Date().getTime();
            var i = 0;
            var html = opts.title ? '<span class="img-upload-error" id="span_error"></span>' : '';
            html += '<div class="col-sm-12" >';
            if (!Array.isArray(opts.displayImgSrc))
                opts.displayImgSrc = [opts.displayImgSrc];
            for (var i = 0; i < opts.imagesCount; i++) {
                var display = opts.displayImgSrc[i] || i == 0 || (opts.displayImgSrc[i - 1]) ? '' : ' style="display:none"';
                var imgHideUrl = opts.displayImgSrc[i] ? opts.displayImgSrc[i] : '';
                html += ' <div class="upload-img-box" imageBox ' + display + '>\
                                 <img src="' + (opts.displayImgSrc[i] ? opts.displayImgSrc[i] : opts.defaultImg) + '" style="display:' + (opts.displayImgSrc[i] ? 'block' : 'none') + '" class="img-upload" onclick="$(\'#imgUploader_' + id + '_' + i + '\').click()" />\
                                 <span class="glyphicon remove-img animated swing" style="display:none"></span>\
                                 <div class="glyphicon glyphicon-plus img-upload-btn" onclick="$(\'#imgUploader_' + id + '_' + i + '\').click()" style="display:' + (!opts.displayImgSrc[i] ? 'block' : 'none') + '"></div>\
                                <input type="hidden" class="hiddenImgSrc"  value="' + imgHideUrl + '"   name="' + opts.imgFieldName + '" />\
                                <input class="file uploadFilebtn" style="cursor: pointer;position:absolute;left:0;top:0;width:100%;height:100%;z-index:1;opacity:0;" type="file" multiple name="_file"   id="imgUploader_' + id + '_' + i + '"/>\
                                <span class="text-muted">' + opts.pictureSize + '</span>\
                             </div>';
            }
            html += '</div>\
            <div class="Prompt alert-danger bounceInDown animated alert-tips" role="alert" id="tips" style="display: none;position:fixed;margin-left:-100px;"></div>';
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

    $.fn.hishopUpload.defaults = {
        url: '/common/publicOperation/UploadPic',
        displayImgSrc: '',
        title: '图片：',
        imageDescript: '请上传16px * 16px的图片',
        imgFieldName: 'Icon',
        view: renderView,
        headerWidth: 2,
        dataWidth: 2,
        defaultImg: '',
        imagesCount: 1,
        disabled: false,
        target: '',
        targetType: '',
        imgTypes: '',
        foldName: ''
    };
})(jQuery);
