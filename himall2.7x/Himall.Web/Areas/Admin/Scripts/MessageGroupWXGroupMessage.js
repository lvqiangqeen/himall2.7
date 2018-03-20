// JavaScript source code
$(document).ready(function () {

    $("input:radio").bind("change", function () {
        if ($("input:radio:checked").val() == "2")
        { $(".tag-area").show(); }
        else
        { $(".tag-area").hide(); }
    });
    $(".tag-choice").click(function () {

        $(".dialog-tag").css("display", "block");
        $(".coverage").show();
        $('input[name=check_Label]').each(function (i, checkbox) {
            $(checkbox).get(0).checked = false;
        });
        $('.tag-area span').each(function (idx, el) {
            $('#check_' + $(el).attr('LabelId')).get(0).checked = true;
        });
    });
    $(".tag-submit").click(function () {
        var labels = [];
        $('input[name=check_Label]').each(function (i, checkbox) {
            if ($(checkbox).get(0).checked) {
                labels.push('<span labelid="' + $(checkbox).attr('datavalue') + '">' + $(checkbox).val() + '</span>');
            }
        });
        $('.tag-area').html(labels.join(''));
        $(".dialog-tag").hide();
        $(".coverage").hide()
    });
    $(".dialog-tag .glyphicon-remove").click(function () {
        $(".dialog-tag").hide();
        $(".coverage").hide();
    });
    $('.tag-back').click(function () {
        $(".dialog-tag").hide();
        $(".coverage").hide()
    });

    $(".tab-content .library").click(function () {
        $(".sucai-library").show();
        $(".coverage").show();
        GetMaterialData();
    });
    $(".sucai-library .glyphicon-remove").click(function () {
        $(".sucai-library").hide();
        $(".coverage").hide();
    });
    $('#btnOk').click(function () {
        var $li = $('li[curMedia=1]');
        if ($li.length > 0) {
            curMedia = $li.attr('id');
            $(".sucai-library").hide();
            $(".coverage").hide();
            $(".create_access").hide();

            var curMediaObj = mediaList[$li.attr('idx')];
            var $detail = $('#mediaDetail');
            $('#mediaTime').val(curMediaObj.update_time);
            $('img[name=wrapper]').attr('src', 'GetMedia?mediaid=' + curMediaObj.items[0].thumb_media_id);
            $('span[name=wrapperTitle]').text(curMediaObj.items[0].title);

            var html = [];
            $(curMediaObj.items).each(function (idx, el) {
                if (idx > 0) {
                    html.push(' <div class="item" >');
                    html.push(' <div class="WX-edted">');
                    html.push(' <i><img src=GetMedia?mediaid=' + $(el).attr('thumb_media_id') + ' /> </i>');
                    html.push(' <span name="title">' + $(el).attr('title') + '</span>');
                    html.push('</div></div>');
                }
            });
            $('#divChild').html(html.join(''));
            $detail.show();
        }
    });
    $('#btnCancel').click(function () {
        $(".sucai-library").hide();
        $(".coverage").hide();
    });
    $('#msgtype_news').click(function () {
        $('#mediaSelect').show();
        $('#txtInput').hide();
        $('#msgtype_text').removeClass('active');
        $(this).addClass('active');
    });
    $('#msgtype_text').click(function () {
        $('#txtInput').show();
        $('#mediaSelect').hide();
        $('#msgtype_news').removeClass('active');
        $(this).addClass('active');
    });

    $('#btnSendWX').click(SendWXMsg);

});
var pageTotal = 0;
var pageIdx = 1;
var pageSize = 8;
var curMedia = '';
var mediaList = {};
function GetMaterialData() {
    $.post('GetWXMaterialList', { pageIdx: pageIdx, pageSize: pageSize }, function (data) {
        var returnCode = data.errCode || '0';
        if (data.msg) {
            $('#list').append('<li class="con-frame"><div class="source-l">' + data.msg + '</div></li>');
        }
        else {
            if (data.errMsg) {
                $('#list').append('<li class="con-frame">' + data.errMsg + '</li>');
            }
            else {
                var html = [], lihtml = [], mediaid = '';
                $('#list').html('');
                mediaList = data.content;
                $(data.content).each(function (idx, el) {
                    lihtml = [];
                    mediaid = '';
                    $(el.items).each(function (i, item) {
                        if (mediaid == '')
                            mediaid = item.thumb_media_id;
                        lihtml.push('<li>' + item.title + '</li>');
                    });
                    html.push("<li idx=" + idx + " id=\"" + el.media_id + "\" class=\"con-frame\"  onclick=\"selectMaterial('" + el.media_id + "')\">");
                    html.push('<div class="source-l">');
                    html.push('<span><img src="GetMedia?mediaid=' + mediaid + '"></span>');
                    html.push('<ol>');
                    html.push(lihtml.join(''));
                    html.push('</ol>');
                    html.push('</div>');
                    html.push('<div class="source-M"><time>' + el.update_time + '</time></div>');
                    html.push('<span class="SCover"></i></span>');
                    html.push('<i class="glyphicon glyphicon-ok">');
                    html.push('</li>');
                    $('#list').append(html.join(''));
                    html = [];
                });
                $(".con-frame").hover(function () {
                    $(".SCover", this).show();
                }, function () {
                    if ($('.glyphicon-ok', this).css('display') != 'block') {
                        $(".SCover", this).hide();
                    }
                });
                $(".con-frame").click(function () {
                    $(".SCover").hide();
                    $(".glyphicon-ok").hide();
                    $(".SCover", this).show();
                    $(".glyphicon-ok", this).show();
                });
            }
        }
    });
}
function selectMaterial(mediaid) {
    $('#' + mediaid).siblings().attr('curMedia', 0);
    $('#' + mediaid).attr('curMedia', 1);
}
function SendWXMsg() {
    var label = '';
    var desc = [];
    if (!$('#allLabel').get(0).checked) {
        $('.tag-area span').each(function (idx, el) {
            label += ',' + $(el).attr('labelid');
            desc.push($(el).text());
        });
        if (label.length == 0) {
            $.dialog.alert('请选择要发送的标签');
            return;
        }
        label = label.substr(1, label.length - 1);
    }
    else {
        desc.push('标签:全部');
    }

    var msgtype = $('#msgtype').children('.active').attr('value');
    var sex = $('#sexType').val() == -1 ? '' : $('#sexType').val();
    var region = $('#region').val() == -1 ? '' : $('#region').val();
    var msgcontent = '';
    var sexSelect = $('#sexType').get(0);
    desc.push("性别：" + sexSelect.options[sexSelect.selectedIndex].text);
    var regSelect = $('#region').get(0);
    desc.push("地区：" + regSelect.options[regSelect.selectedIndex].text);
    if (msgtype == 1) {
        msgcontent = $('#txtInput textarea').val();
        if (msgcontent == '') {
            $.dialog.alert('发送内容不能为空');
            return;
        }
        if (msgcontent.length > 600) {
            $.dialog.alert('发送内容长度不能超过600');
            return;
        }
    }
    else {
        if (curMedia == 'p') {
            $.dialog.alert('请选择素材模板');
            return;
        }
    }
    loading = showLoading('正在发送');
    $.post('SendWXGroupMessage', { msgtype: msgtype, userdesc: desc.join(' '), mediaid: curMedia, labelids: label, sex: sex, region: region, msgcontent: msgcontent }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.alert('发送成功');
        }
        else {
            $.dialog.alert(result.msg);
        }
    });
};
