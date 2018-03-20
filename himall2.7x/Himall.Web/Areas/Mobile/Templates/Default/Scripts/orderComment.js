$(function () {
    checkOrderIsUncomment();
    initStarsReaction();

    $('#submit').click(function () {
        submitComment();
    });

});


function initStarsReaction() {
    $('.star-score i').click(function () {
        var i = $(this).index() + 1;
        
        $(this).addClass('l').removeClass('b').nextAll('i').addClass('b').removeClass('l');
        $(this).prevAll('i').addClass('l').removeClass('b');
        $(this).siblings('input').val(i);
    });
}


function checkOrderIsUncomment() {
    var isValid = $('#isValid').val();
    if (!isValid) {
        $.dialog.tips('已经评论过该订单或订单无效!', function () {
            history.go(-1);//返回
        });
    }
}

function submitComment() {
    var commentsDiv = $('div[name="productComment"]');
    var comments = [];
    $.each(commentsDiv, function (i, comment) {

        var images = [];
        var WXmediaId = [];
        if ($('#ProductImages' + i).length > 0)
        {
            images = $('#ProductImages' + i).himallUpload('getImgSrc');
        }
        else if ($('#UploadPlace' + i).length > 0)
        {
          //  alert($('#WXimages' + i).html());
            $('#UploadPlace' + i).find("img").each(function (index, item) {
               // alert($(item).data("serverid"));
                WXmediaId.push($(item).data("serverid"));
            });
        }
        comments.push({
            mark: $(comment).find('input[name="mark"]').val(),
            content: $(comment).find('[name="content"]').val(),
            orderItemId: $(comment).attr('orderItemId'),
            Images: images,
            WXmediaId:WXmediaId,
        });
    });

    var orderId = QueryString('orderId');
    var comment = {
        orderId: orderId,
        serviceMark: $('#serviceMark').val(),
        deliveryMark: $('#deliveryMark').val(),
        packMark: $('#packMark').val(),
        productComments: comments
    };

    try {
        checkComments(comment);
        var loading = showLoading();
        var json = JSON.stringify(comment);
        $.post('/' + areaName + '/Comment/AddComment',
           { comment: json },
           function (result) {
               loading.close();
               if (result.success)
                   $.dialog.succeedTips('评论成功！', function () { window.location.href ='/' + areaName + '/member/orders/' });
               else
                   $.dialog.alert('评论失败！' + result.msg);
           });
    }
    catch (e) {
        $.dialog.errorTips(e.message);
    }


}

function checkComments(comment) {
    $.each(comment.productComments, function (i, productComment) {
        if (!productComment.mark)
            throw new Error('请给商品打分');
        if (!productComment.content)
            throw new Error('请填写商品评价内容');
    });

    if (!comment.packMark)
        throw new Error('请给商品包装打分');
    if (!comment.deliveryMark)
        throw new Error('请给送货速度打分');
    if (!comment.serviceMark)
        throw new Error('请给配送服务打分');
}