
var editor,editorMobile;

$(function () {
    initRichTextEditor();
    bindSubmitBtnClickEvent();
});



function initRichTextEditor() {

    editor = UE.getEditor('des', { maximumWords: 4000 });
    editorMobile = UE.getEditor('mdes', { maximumWords: 4000 });
    //eidtor.addListener('contentChange', function () {
    //    $('#contentError').hide();
    //});

}

function bindSubmitBtnClickEvent() {
    $('#submit').click(function () {
        submit();
    });
}


function submit() {
    var name = $.trim($('input[name="name"]').val());
    var content = editor.getContent();
    var mobcontent = editorMobile.getContent();
    var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;

    if (!name || name.length > 30 || reg.test(name) == false) {
        $('input[name="name"]').css({ border: '1px solid #f60' });
        $('input[name="name"]').focus();
        return false;
    } else {
        $('input[name="name"]').css({ border: '1px solid #ccc' });
    }
    if (!content) {
        $(".descon .nav li").eq(0).click();
        $('.descon').css({ border: '1px solid #f60' });
        editor.focus();
        return false;
    }
    if (!mobcontent) {
        $(".descon .nav li").eq(1).click();
        $('.descon').css({ border: '1px solid #f60' });
        editorMobile.focus();
        return false;
    }
    if ($('input[name="position"]:checked').length==0)
    {
        $.dialog.errorTips('请选择版式位置');
        return;
    }

    if (!name || name.length > 30 || reg.test(name) == false || !content) return false;

    var data = $('form').serialize();
    var loading = showLoading();
    $.post('add', data, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips('保存成功');
            setTimeout(function () { location.href = 'management'; }, 1300);
        }
        else
            $.dialog.errorTips('保存失败' + result.msg);
    });

}

$(function () {
    $('input[name="name"]').blur(function () {
        var name = $.trim($('input[name="name"]').val());
        var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;
        if (!name) {
            $("#nameTip").text("请输入关联板式的名称！");
            $('input[name="name"]').css({ border: '1px solid #f60' });
            $('input[name="name"]').focus();
            return false;
        }
        if (reg.test(name) == false) {
            $("#nameTip").text("名称必须是中文，字母，数字！");
            $('input[name="name"]').css({ border: '1px solid #f60' });
            $('input[name="name"]').focus();
            return false;
        }

        if (name.length > 30) {
            $("#nameTip").text("名称最多只能输入30个字符！");
            $('input[name="name"]').css({ border: '1px solid #f60' });
            $('input[name="name"]').focus();
            return false;
        }
        else {
            $("#nameTip").text("");
            $('input[name="name"]').css({ border: '1px solid #ccc' });
        }
    });
    //切换
    $('.nav-tabs li').click(function () {
        $(this).addClass('active').siblings().removeClass('active');
        $('.tab-content .tab-pane').eq($(this).index()).addClass('active').siblings().removeClass('active');
    });
})