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
            var uploadFilesCount = 1;
            var flag = true;
            if (imgupfile[0].files != null)
            {
                uploadFilesCount = imgupfile[0].files.length;
                $(imgupfile[0].files).each(function (index, item) {
                    if (item.size/1024 > opts.maxSize*1024) {
                        flag = false;
                        return;
                    }
                });
            }
         
            var avaliableFilesCount = 1;
            $(target).parent().find('div[imageBox]').each(function () {
                if ($(this).css('display') == 'none')
                    avaliableFilesCount++;
            });



            if (uploadFilesCount > avaliableFilesCount) {
                $.dialog.errorTips('最多还能上传' + avaliableFilesCount + '张图片，请重新选择');
                return;
            }

            if (!checkImgType(imgupfile.val())) {
                $.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp", '', 3);
                return false;
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
                //    $.dialog.errorTips("上传的图片大小超过限制！");
                //}
                else {
                    //文件上传成功，返回图片的路径。将路经赋给隐藏域
                    var files = data.split(',');
                    var container = $(target).parent();
                    var imageBoxes = container.find('div[imageBox]');

                    var images = container.find('img');
                    //  var uploadBtns = container.find('.img-upload-btn');
                    var hiddenImgSrces = container.find('input.hiddenImgSrc');
                    var started = false;
                    var lastIndex = 0;
                    for (var i = 0, j = 0; i < opts.imagesCount; i++) {
                        if (imageBoxes[i] == target)
                            started = true;
                        if (started && files[j]) {
                            lastIndex = i;
                            $(images[i]).attr('src', files[j]).show();
                            $(imageBoxes[i]).find('.glyphicon-picture').addClass('active');
                            $(hiddenImgSrces[i]).val(files[j++]);
                            //  $(uploadBtns[i]).hide();
                            $(imageBoxes[i]).show();
                        }
                    }
                }
                if (lastIndex + 1 < opts.imagesCount)
                    $(imageBoxes[lastIndex + 1]).show();


                //  $(fu1).insertAfter($(target).find("input.hiddenImgSrc", $(target)));
                $(fu1).insertAfter($(target).find(".glyphicon-picture", $(target)));
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
        $(target).find('.glyphicon-picture').each(function () {
            var imSrc = $(this).parent().find('input').val();
            if (imSrc != '') {
                $(this).addClass('active');
            }
            $(this).mouseenter(function () {
                var position = $(this).offset();
                var random = Math.random();
                var src = $(this).parent().find('input').val();
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
                            $('.lg-view-img').css({ 'bottom': 20 })
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
            $(this).mouseleave(function () {
                $('.lg-view-img').remove();
            });
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
            var html = opts.title ? '<label class="control-label col-sm-' + opts.headerWidth + '" for="' + opts.title + '">' + opts.title + '</label>' : '';
            html += '<div class="col-sm-' + opts.dataWidth + '" >';

            if (!$.isArray(opts.displayImgSrc))
                opts.displayImgSrc = opts.displayImgSrc.split(',');

            for (var i = 0; i < opts.imagesCount; i++) {
                var display = opts.displayImgSrc[i] || i == 0 ? '' : ' style="display:none"';
                html += ' <div imageBox ' + display + ' class="fl">\
                                <input type="hidden" class="hiddenImgSrc"  value="' + (opts.displayImgSrc[i] ? opts.displayImgSrc[i] : '') + '"   name="' + opts.imgFieldName + '" />\
                                <span class="glyphicon glyphicon-picture"></span>\
                                <input class="file uploadFilebtn" type="file" multiple name="_file"   id="imgUploader_' + id + '_' + i + '"/>\
              			     </div>';
            }
            html += '<p class="text-muted">' + opts.imageDescript + '</p>\</div>';

            $(container).html(html);
        }
    };

    $.fn.hishopUpload.defaults = {
        url: '/common/PublicOperation/UploadPic',
        displayImgSrc: '',
        title: '图片：',
        imageDescript: '建议上传16px * 16px的图片',
        imgFieldName: 'Icon',
        view: renderView,
        headerWidth: 2,
        dataWidth: 2,
        defaultImg: '',
        imagesCount: 1,
        pictureSize: '',
        maxSize:2  //M为单位
    };
})(jQuery);