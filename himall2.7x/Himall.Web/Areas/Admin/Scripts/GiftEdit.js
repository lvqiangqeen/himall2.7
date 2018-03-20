var eidtor;
var dtips;

$(function () {
    initRichTextEditor();
    $("#GiftName").focus();
    dtips = $("#destips");

    //提交前检测
    $("#gifteditform").bind("submit", function (e) {
        var isdataok = true;
        eidtor.sync();
        var des = $("[name='Description']").val();
        if (des.length < 1) {
            dtips.show();
            isdataok= false;
        }
        var pic1 = $("#PicUrl1").val();
        if (pic1.length < 1) {
            $('#up_pic1').css({ border: '1px solid #f60' });
            $('#PicUrl1').focus();
            isdataok= false;
        }
        return isdataok;
    });
});
function initRichTextEditor() {
    eidtor = UE.getEditor('Description');
    eidtor.addListener('contentChange', function () {
        //    editor.sync();
        dtips.hide();
    });
    //同步内容
    eidtor.addListener("blur", function () {
        eidtor.sync();
        var content = eidtor.getContent();
        if (content.length < 1) {
            dtips.show();
        } else {
            dtips.hide();
        }
    })

};
var MaxFileSize = 1048576;//1M

//AJAX上传文件
function ajaxuploadfile(target, opts) {
    var imgupfile = $(target).find("input:file").eq(0); //上传控件

    if (imgupfile.val() == "") {
        $.dialog.errorTips("请选择要上传的文件！");
        return false;
    }
    else {
        if (!checkImgType(imgupfile.val())) {
            $.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp ", '', 3);
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
            $(fu1).prependTo($(target));
            form.remove();
            if (data == "NoFile" || data == "Error" || data == "格式不正确！") {
                $.dialog.errorTips(data);
            }
            else {
                //回调
                opts.success(data);
            }

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

$(function () {
    $('.uploadFile').bind('change', function (e) {
        var _t = $(this);
        var d_parent = _t.parent();
        var string = d_parent.attr('data-del'), isHide = string >> 0;
        var curnum = _t.data("num");

        //图片大小限制
        var dom_btnFile = _t[0];
        if (typeof (dom_btnFile.files) == 'undefined') {
            try {
                var fso = new ActiveXObject('Scripting.FileSystemObject');
                var f = fso.GetFile($("#btnFile").val());
                if (f.Size > MaxFileSize) {
                    $.dialog.tips('选择的图片不能大于' + MaxFileSize / 1048576 + 'M');
                    $('#from' + curnum).trigger('reset');
                    return;
                }
            }
            catch (e) {
                //$.dialog.tips(e);
                //$('#from' + i).trigger('reset');
            }
        }
        else {
            if (dom_btnFile.files.length > 0 && dom_btnFile.files[0].size > MaxFileSize) {
                $.dialog.tips('选择的图片不能大于' + MaxFileSize / 1048576 + 'M');
                $('#from' + curnum).trigger('reset');
                return;
            }
        }//end

        ajaxuploadfile(d_parent, {
            url: "/common/PublicOperation/UploadPic",
            success: function (data) {
                var uppicbox = $('#up_pic' + curnum);
                uppicbox.html('<img src="' + data + '" width="99" height="99">');
                uppicbox.css({border:"none"});
                $("#fileBox" + curnum).removeClass('glyphicon glyphicon-open').addClass('glyphicon glyphicon-remove').attr('data-del', '1');
                $('#up_pic' + curnum).attr('data-url', data);
                $('#PicUrl' + curnum).val(data);
                _t.hide();
            }
        });
    });

    $('#id_upload').bind('click', function (e) {
        var str = $(e.target).attr('data-del'),
            del = str >> 0;
        if (del) {
            var dnum = $(e.target).find('input.uploadFile').data("num");
            $(e.target).next().attr('data-url', '').html('');
            $(e.target).find('input.uploadFile').show();
            $(e.target).removeClass('glyphicon glyphicon-remove').addClass('glyphicon glyphicon-open');
            $("#PicUrl" + dnum).val("");
            return;
        }
    });
});
var btsubmit;
var loading;
$(function () {
    $("#EndDate").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1,
        minView: 0
    });
    $('#EndDate').datetimepicker('setStartDate', $("#DNT").val());

    btsubmit = $("#btsubmit");
});

var isposting = false;
function beginpost() {
    if (isposting) {
        $.dialog.tips("数据提交中...");
        return false;
    }
    isposting = true;
    btsubmit.text("提交中...");
    loading = showLoading();
}

function successpost(data) {
    isposting = false;
    btsubmit.text("保 存");
    loading.close();
    if (data.success == true) {
        $.dialog.tips("礼品信息操作成功", function () {
            window.location.href = $("#UMG").val();//数据提交成功页面跳转
        });
    } else {
        $.dialog.errorTips(data.msg);
    }
}