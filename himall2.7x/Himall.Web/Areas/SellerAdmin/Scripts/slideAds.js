$(function () {

    bindAddSlideAdBtnClickEvent();

    bindEditBtnClickEvent();
    bindDeleteClickEvent();
    bindSortEvent();
    ReSetSwapIcon();
});




function bindAddSlideAdBtnClickEvent() {

    $('#addSlideAd').click(function () {

        showEditor();

    });

}


function bindEditBtnClickEvent() {
    $('a[btnType="edit"]').click(function () {
        var pic = $(this).attr('pic');
        var url = $(this).attr('url');
        var id = $(this).attr('slideId');
        showEditor(id, url, pic);
    })

}


function showEditor(id,url,pic) {

    $.dialog({
        title: '轮播图设置',
        lock: true,
        width: 420,
        padding:'0 40px',
        id: 'goodsArea',
        content: ['<div class="dialog-form">',
            '<div id="HandSlidePic" class="form-group upload-img clearfix">',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl" for="" style="margin-right:18px!important">跳转链接:</label>',
                '<input class="form-control input-sm" type="text" id="url">',
            '</div>',
        '</div>'].join(''),
        okVal: '保存',
        init: function () {
            $("#HandSlidePic").himallUpload(
            {
                title: '请上传图片',
                imageDescript: '1920*520',
                displayImgSrc: pic,
				dataWidth: 8
            });
            $("#url").val(url);
        },
        ok: function () {
            var valida = false;
            url = $('#url').val();
            pic = $("#HandSlidePic").himallUpload('getImgSrc');


            if (!url||url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }
            if (!pic || pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }
            var loading = showLoading();
            $.ajax({
                type: "POST",
                url: "SaveSlideAd",
                data: { url: url, pic: pic, id: id },
                dataType: "json",
                async: false,
                success: function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.tips('保存成功!', function () {

                            location.reload();
                            //if (!id)
                            //    location.reload();
                            //else {
                            //    var obj = $('a[slideId="' + id + '"]');
                            //    obj.attr('pic', data.imageUrl);
                            //    obj.attr('url', url);

                            //    $('label[slideId="' + id + '"]').html(url);
                            //    $('img[slideId="' + id + '"]').attr('src', data.imageUrl);
                            //}
                        });
                    }
                    else {
                        $.dialog.errorTips('保存失败！' + data.msg);
                        return false;
                    }
                },
                error: function (data) {
                    loading.close(); $.dialog.errorTips('操作失败,请稍候尝试.'); }
            });
        }
    });

}


function bindSortEvent() {
    //排序
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    $(".table").on("click", '.glyphicon-circle-arrow-up', function () {
        var p = $(this).parents('tr');
        var index = p.parent().find('tr').index(p);
        var thisObj = this;
        if (index == 0)
            return false;
        else {
            var oriId = parseInt(p.attr("swapid"));
            swapSequence(oriId, 1, function () {
                p.prev().before(p);
                reDrawArrow(thisObj);
            });
        }
    });

    $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
        var p = $(this).parents('tr');
        var count = p.parent().find('tr').length;
        var index = p.parent().find('tr').index(p);
        var thisObj = this;
        if (index == (count - 1))
            return false;
        else {
            var oriId = parseInt(p.attr("swapid"));
            swapSequence(oriId, 0, function () {
                p.next().after(p);
                reDrawArrow(thisObj);
            });
        }
    });
    $(".table").on("click", '.glyphicon', function () {
        $(this).parents('tbody').find('.glyphicon').removeClass('disabled');
        $(this).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
        $(this).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    });
}

function reDrawArrow(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}

function swapSequence(id, direction, callback) {
    var loading = showLoading();
    var result = false;

    ajaxRequest({
        type: 'POST',
        url: 'AdjustSlideIndex',
        cache: false,
        param: { id: id, direction: direction },
        dataType: 'json',
        success: function (data) {
            loading.close();
            if (data.success) {
                callback();
            }
        },
        error: function (data) {

            loading.close();
            $.dialog.errorTips('调整顺序失败,请稍候尝试.');
        }
    });



}

function bindDeleteClickEvent() {

    $('a[btnType="delete"]').click(function () {
        del($(this).attr('slideId'));
    })

}


function del(id) {
    $.dialog.confirm('确定要删除本条轮播图吗？', function () {
        var loading = showLoading();
        $.post('deleteSlide', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功!');
                $('tr[swapid="' + id + '"]').remove();
                ReSetSwapIcon();

            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        })


    })


}


function ReSetSwapIcon() {
    $(".table tbody").find('.glyphicon').removeClass('disabled');
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}