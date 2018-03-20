var MaxFileSize = 2097152;//2M
var navCnt = 1;
var curCnt = 1;
var flag = true;
$(document).ready(function () {
    //新增图文
    $(".item-wrapper03").click(function () {
        var add_item = $(".tmpl-item-wrapper02").clone();
        add_item.removeClass("tmpl-item-wrapper02");
        add_item.hover(function () {
            var _t = $(this);
            _t.find(".edit-cover").css("display", "block");
        }, function () {
            var _t = $(this);
            _t.find(".edit-cover").css("display", "none");
        });
        add_item.click(function () {
            $(this).addClass("wrap-border").siblings(".item").removeClass("wrap-border");
            showMaterial($(this).attr('cnt'));
        });
        $(this).before(add_item);
        add_item.addClass("wrap-border").siblings(".item").removeClass("wrap-border");
        add_item.show();
        $(".glyphicon-trash").click(function () {
            var $nav = $(this).parent().parent();
            $('#content' + $nav.attr('cnt')).remove();
            $nav.remove();
        });

        
        navCnt = navCnt + 1;
        add_item.attr('cnt', navCnt);
        addMaterial();//增加输入内容项
    });
	/*上下排序*/
	
	$(document).on('click','.glyphicon-arrow-down',function () {
		$(this).parents(".item ").next(".item ").after($(this).parents(".item "))
	});
	$(document).on('click','.glyphicon-arrow-up',function () {
		$(this).parents(".item ").prev(".item ").before($(this).parents(".item "))
	});
    $("#navWrapper").click(function () {
        $(this).addClass("wrap-border").siblings(".item").removeClass("wrap-border");
        showMaterial($(this).attr('cnt'));
    });
    //操作图层显示切换
    $(".cover-handle").hover(function () {
        $(this).children(".edit-cover").css("display", "block");
    }, function () {
        $(this).children(".edit-cover").css("display", "none");
    });
    //点击外框
    $(".WX-edit .item").click(function () {
        $(this).addClass("wrap-border").siblings(".item").removeClass("wrap-border");
    });

    //
    getEditor('contentDesc1');
    //上传图片
    $("#btnFile1").bind("change", function () {
        selectImage();
    });
    $('button[name=btnSave]').click(SaveMaterial);
    $('.WXinput-area input[name=title]').bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/64');
    });
    $('.WXinput-area input[name=author]').bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/8');
    });
    $('.WXinput-area textarea[name=digest]').bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/120');
    });
    if ($('#mediaid').val() != '') {
        BindData($('#mediaid').val());
    }
});
function getEditor(name)
{
    var eidtor = UE.getEditor(name, {
        toolbars: [['bold', //加粗
            'indent', //首行缩进
            'italic', //斜体
            'underline', //下划线
            'strikethrough', //删除线
            'subscript', //下标
            'fontborder', //字符边框
            'superscript', //上标
            'pasteplain', //纯文本粘贴模式
            'horizontal', //分隔线
            'removeformat', //清除格式
            'fontfamily', //字体
            'fontsize', //字号
            'justifyleft', //居左对齐
            'justifyright', //居右对齐
            'justifycenter', //居中对齐
            'justifyjustify', //两端对齐
            'forecolor', //字体颜色
            'backcolor', //背景色
            'lineheight', //行间距
            'touppercase', //字母大写
            'tolowercase', //字母小写
        ]]
    });
    eidtor.addListener('contentChange', function () {
        //    editor.sync();
    });
    //同步内容
    eidtor.addListener("blur", function () {
        eidtor.sync();
        var content = eidtor.getContent();
    });
    return eidtor;
}
function BindData(mediaid)
{
    $.post('GetMediaInfo', { mediaid: mediaid }, function (result) {
    console.log(result)
        if (result.success)
        {
            $(result.data).each(function (idx, el) {
                var $content = $('#content' + (idx + 1));
                var $el = el;
                if ($content.length == 0) {
                    $(".item-wrapper03").click();
                }
                var EditorContent = UE.getEditor('contentDesc' + (idx + 1));
                EditorContent.addListener("ready", function () {
                    setTimeout(1000);
                    // editor准备好之后才可以使用
                    EditorContent.setContent($el.content);

                });
                $('input[name=title]', $content).val($el.title);
                $('input[name=thumb_media_id]', $content).val($el.thumb_media_id);
                $('input[name=author]', $content).val($el.author);
                $('textarea[name=digest]', $content).val($el.digest);
                $('input[name=show_cover_pic]', $content).get(0).checked = $el.show_cover_pic == '0' ? false : true;
                $('input[name=content_source_url]', $content).val($el.content_source_url);
                $('div[cnt=' + (idx + 1) + '] img').attr('src', 'GetMedia?mediaid=' + $el.thumb_media_id);
                if (result.data.length > (idx + 1)) {
                    $(".item-wrapper03").click();
                }
            });
        }
    });
}
function selectImage()
{
    var dom_iframe = document.getElementById('iframe' + curCnt);
    //非IE、IE
    dom_iframe.onload = function () {
        var filename = this.contentDocument.body.innerHTML;
        if (filename != 'NoFile' && filename != 'Error') {
            $.post('AddWXImageMsg', { name: filename }, function (data) {
                console.log(data)
                if (data.success) {
                    loading.close();
                    fnUploadFileCallBack(filename, data.media);
                    $.dialog.tips('上传成功');
                }
                else {
                    loading.close();
                    $.dialog.tips(data.msg);
                }
            });
        }
        else {
            loading.close();
            $.dialog.tips('上传文件异常');
        }
        this.onload = null;
        this.onreadystatechange = null;
    };
    //IE
    dom_iframe.onreadystatechange = function () {
        if (this.readyState == 'complete' || this.readyState == 'loaded') {
            var filename = this.contentDocument.body.innerHTML;
            if (filename != 'NoFile' && filename != 'Error') {
                $.post('AddWXImageMsg', { name: filename }, function (data) {
                    if (data.success) {
                        loading.close();
                        fnUploadFileCallBack(filename, data.media);
                        $.dialog.tips('上传成功');
                    }
                });
            }
            else {
                loading.close();
                $.dialog.tips('上传文件异常');
            }
            this.onload = null;
            this.onreadystatechange = null;
        }
    };
    if ($("#btnFile" + curCnt).val() != '') {
        var dom_btnFile = document.getElementById('btnFile' + curCnt);
        if (typeof (dom_btnFile.files) == 'undefined') {
            try {
                var fso = new ActiveXObject('Scripting.FileSystemObject');
                var f = fso.GetFile($("#btnFile").val());
                if (f.Size > MaxFileSize) {
                    $.dialog.tips('选择的文件太大');
                    return;
                }
            }
            catch (e) {
                errorTips(e);
            }
        }
        else {
            if (dom_btnFile.files.length > 0 && dom_btnFile.files[0].size > MaxFileSize) {
                $.dialog.tips('选择的文件太大');
                return;
            }
        }
        loading = showLoading('正在上传');
        $('#formUpload' + curCnt).submit();
    }
}
function fnUploadFileCallBack(filename,mediaid) {
    var cnt = curCnt;
    var $content = $('#content' + cnt);
    var $nav=$('')
    $('input[name=thumb_media_id]', $content).val(mediaid);
    $('div[cnt=' + cnt + '] img').attr('src', filename);
}
function AddImageMedia(filename)
{
    $.post('AddWXImageMsg', { name: filename }, function (data) {
        if (data.success)
        {
            fnUploadFileCallBack(data.media);
        }
    });
}
function showMaterial(idx)
{
    curCnt = idx;
    $('.WXinput-area').hide();
    $('#content' + idx).show();
}
function addMaterial()
{//clone右则素材输入框
    $('.WXinput-area').hide();
    var content = $('#content1').clone();
    content.attr('id', 'content' + navCnt);
    $('#contentDesc1', content).attr('id', 'contentDesc' + navCnt);
    $('#formUpload1', content).attr('id', 'formUpload' + navCnt).attr('target', 'iframeUpload' + navCnt);
    $('#iframe1', content).attr('id', 'iframe' + navCnt).attr('name', 'iframeUpload' + navCnt);
    $('#btnFile1', content).attr('id', 'btnFile' + navCnt).bind('change', selectImage);
    $('input[name=title]', content).bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/64');
    });
    $('input[name=author]', content).bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/8');
    });
    $('textarea[name=digest]', content).bind('keyup', function () {
        $(this).next('i').text($(this).val().length + '/120');
    });
    $('button[name=btnSave]', content).unbind('click').click(SaveMaterial);
    curCnt = navCnt;//新添加的为当前素材
    $('div[name=contentDesc]', content).html('<textarea id="contentDesc' + navCnt + '"></textarea>');
    $('.edit-wrap').append(content);
    var editor = getEditor('contentDesc' + navCnt);
    content.show();
}
function SaveMaterial()
{
    var material = [];
    var materials = [];
    var isCheck = true;
    var tipMsg = [];
    $('.item',$('.WX-edit')).each(function (idx, el) {
        var $nav = $(el);
        var $content = $('#content' + $nav.attr('cnt'));
        if ($content.length>0)
        {
            material = [];
            if ($('input[name=title]', $content).val() == '')
            {
                isCheck = false;
            }
            if ($('input[name=author]', $content).val() == '')
            {
                isCheck = false;
            }
            var EditorContent = getEditor('contentDesc' + $nav.attr('cnt')).getContent();
            if (EditorContent == '') {
                isCheck = false;
            }
            if ($('input[name=thumb_media_id]', $content).val() == '')
            {
                isCheck = false;
            }
            EditorContent = EditorContent.replace(/\"/g, "'");
            material.push('title:"' + $('input[name=title]', $content).val()+'"');
            material.push('thumb_media_id:"' + $('input[name=thumb_media_id]', $content).val() + '"');
            material.push('author:"' + $('input[name=author]', $content).val() + '"');
            material.push('digest:"' + $('textarea[name=digest]', $content).val() + '"');
            material.push('show_cover_pic:"' + ($('input[name=show_cover_pic]', $content).get(0).checked ? '1' : '0') + '"');
            material.push('content:"' + EditorContent + '"');
            if ($('input[name=setUrl]', $content).get(0).checked) {
                material.push('content_source_url:"' + $('input[name=content_source_url]', $content).val().replace(/\"/g,"'") + '"');
            } else {
                material.push('content_source_url:""');
            }
            materials.push('{' + material.join(',') + '}');
        }
    });
    if (!isCheck)
    {
        $.dialog.alert('标题、作者、正文、封面不能为空！');
        return;
    }
    if (materials.length == 0)
    {
        $.dialog.alert('未找到需保存的素材信息');
        return;
    }
    var data = '[' + materials.join(',') + ']';
    loading = showLoading('正在提交');
    var mediaid = '';
    if ($('#mediaid').val() != '') {
        mediaid = $('#mediaid').val();
    }
    if (flag) {
        $.post('AddWXMsgTemplate', { mediaid: mediaid, data: data }, function (result) {
            loading.close();
            if (result.success) {
                if ($('#mediaid').val() == '')//新增时，改变flag值,不允许重复提交
                    flag = false;
                $.dialog.alert('保存成功');
            }
            else {
                $.dialog.errorTips(result.msg);
            }
        });
    }
    else {
        loading.close();
        $.dialog.alert('不能重复提交！');
    }
}
