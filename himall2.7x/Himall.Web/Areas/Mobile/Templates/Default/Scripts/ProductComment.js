$(function() {
    loadProductComments(1);
})

var page = 1;
var isend = false;
var pid = QueryString("pId");
var commentType = QueryString("commentType");
$(".comment-tab a").eq(commentType).addClass("active");

function returnComment(type) {
    // location.href = '/' + areaName + '/Product/ProductComment?pId=' + pid + '&commentType=' + commentType;
    commentType = type;
    $(".comment-tab a").removeClass("active");
    $(".comment-tab a").eq(commentType).addClass("active");
    $('#productComment').html("");
    loadProductComments(1);
}

$(window).scroll(function() {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();
    if (scrollTop + windowHeight >= scrollHeight) {
        if (!isend) {
            $('#autoLoad').removeClass('hide');
            loadProductComments(++page);
        }
    }
});

function loadProductComments(page) {
    var areaname = '@ViewBag.AreaName';
    var url = '/' + areaName + '/Product/GetProductComment';
    $.post(url, { pId: pid, pageNo: page, commentType: commentType, pageSize: 8 }, function(result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        var liW = $('#div1').width();
        if (result.length > 0) {
            $.each(result, function(i, items) {
                // console.log(items);
                html += '<li><div class="gray"><span class="name">' + items.UserName[0] + "***" + items.UserName[items.UserName.length-1] + '</span>';
                html += '<span>' + items.ReviewDate + '</span>';
                if (items.Sku != null && items.Sku.length > 1) {
                    html += '<p>购买了 ' + items.Sku + '</span>';
                }
                html += '</div>';
                var dataImg = showImg = '';
                for (var j = 0; j < items.Images.length; j++) {
                    dataImg += '<li style="width:' + liW + 'px"><img src="' + items.Images[j].CommentImage + '"/></li>';
                    showImg += '<dd><span><div class="son"><img src="' + items.Images[j].CommentImage + '"/></div></span></dd>'
                }
                html += '<p class="c-2a2a2a">' + items.ReviewContent + '</p><dl class="comment-img" data-img=\''+dataImg+'\'>'+showImg+'</dl>';

                if (items.ReplyContent != null && items.ReplyContent != '' && items.ReplyContent != "暂无回复") {
                    html += '<dl class="shop-reply">商家回复：' + items.ReplyContent + '</><div class="date-answer">' + items.ReplyDate + '</div></dl>'
                }
                if (items.AppendDate != null && items.AppendDate != "") {
                    dataImg = showImg = '';
                    for (var k = 0; k < items.AppendImages.length; k++) {
                        dataImg += '<li style="width:' + liW + 'px"><img src="' + items.AppendImages[k].CommentImage + '"/></li>';
                        showImg += '<dd><span><div class="son"><img src="' + items.AppendImages[k].CommentImage + '"/></div></span></dd>';
                    }
                    html += '<dl class="comment-ago"><dt>收货' + GetDateDiff(items.FinshDate, items.AppendDate) + '天后追加</dt><dd>' + items.AppendContent + '</dd></dl><dl class="comment-img" data-img=\''+dataImg+'\'>'+showImg+'</dl>';
                }
                if (items.ReplyDate != null && items.ReplyAppendContent != null && items.ReplyAppendContent != "" && items.ReplyAppendContent != "暂无回复") {
                    html += '<dl class="shop-reply">商家回复：' + items.ReplyAppendContent + '</dl>';//e.ReplyDate
                }

                html += '</li>';

            });
            $('#productComment').append(html);
           
            // 放大评论图
            $('#productComment').on('click', '.comment-img dd', function() {
                var len = $(this).parent().find('dd').length;
                var index = $(this).index();
                iNow = num = index;
                $('#title').html( num + 1 + ' / ' + len );
                $('#ul1').css({'left':-index * liW,'width':len*liW}).html( $(this).parent().data('img') );
                $('.comment-popup').addClass('is-show');
            });
            $('.comment-popup').on('click', function(ev) {
                if ( $(ev.target).is('.comment-popup') || $(ev.target).is('.comment-popup-header') ) {
                    $(this).removeClass('is-show');
                }
            });
            
        }
        else {
            isend = true;
            $('#autoLoad').html('没有更多评论了').removeClass('hide');
        }
    });
}

function GetDateDiff(startDate, endDate) {
    var startTime = new Date(Date.parse(startDate.replace(/-/g, "/"))).getTime();
    var endTime = new Date(Date.parse(endDate.replace(/-/g, "/"))).getTime();
    var dates = Math.abs((startTime - endTime)) / (1000 * 60 * 60 * 24);
    return parseInt(dates);
}

// 评论图滑动
var iNow = 0;
var num = 0;
window.onload = function() {
    var oTitle = document.getElementById('title');
    var oDiv = document.getElementById('div1');
    var oUl = document.getElementById('ul1');
    var aLi = oUl.getElementsByTagName('li');
    var w = oDiv.offsetWidth;
    for ( var i = 0; i < aLi.length; i++ ) {
        aLi[i].style.width = w + 'px';
    }
    document.querySelector('.comment-popup').ontouchmove = function(ev) {
        ev.preventDefault();
    };
    
    var downLeft = 0;
    var downX = 0;
    var downTime = 0;
    oUl.ontouchstart = function(ev) {
        var touchs = ev.changedTouches[0];
        var bBtn = true;
        downLeft = this.offsetLeft;
        downX = touchs.pageX;
        downTime = Date.now();
        oUl.ontouchmove = function(ev) {
            var touchs = ev.changedTouches[0];
            if( this.offsetLeft >= 0 ) {
                if(bBtn) {
                    bBtn = false;
                    downX = touchs.pageX;       
                }
                this.style.left = (touchs.pageX - downX)/3 + 'px';
            }
            else if(this.offsetLeft <= oDiv.offsetWidth - this.offsetWidth) {
                if(bBtn) {
                    bBtn = false;
                    downX = touchs.pageX;       
                }
                this.style.left = (touchs.pageX - downX)/3 + (oDiv.offsetWidth - this.offsetWidth) + 'px';
            }
            else{
                this.style.left = touchs.pageX - downX + downLeft + 'px';
            }
        };
        oUl.ontouchend = function(ev) {
            var touchs = ev.changedTouches[0];
            this.ontouchmove = null;
            this.ontouchend = null;
            if( downX < touchs.pageX ) {  //→
                if( iNow != 0) {
                    if(touchs.pageX - downX > oDiv.offsetWidth/2 || Date.now() - downTime < 300 &&  touchs.pageX - downX > 30) {
                        iNow--;
                        num--;
                        oTitle.innerHTML = num + 1 + ' / ' + aLi.length;
                    }
                }
                startMove(oUl,{left : -iNow*w},400,'easeOut');
            }
            else{  //←
                if( iNow != aLi.length-1) {
                    if(downX - touchs.pageX > oDiv.offsetWidth/2 || Date.now() - downTime < 300 && downX - touchs.pageX > 30) {
                        iNow++;
                        num++;
                        oTitle.innerHTML = num + 1 + ' / ' + aLi.length;
                    }
                }
                startMove(oUl,{left : -iNow*w},400,'easeOut');
            }
        };
    };
};
