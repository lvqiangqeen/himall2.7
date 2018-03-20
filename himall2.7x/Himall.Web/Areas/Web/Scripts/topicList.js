var page = 1;
$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight - 30) {
        loadTopics(++page);
    }
});
(function ($) {
    var li = $('#topicUl').find('li');
    if (li.length == 0) {
        $('#autoLoad').html('没有推荐专题');
    }
})($);

function loadTopics(page) {
    var url = 'List';
    $.post(url, { page: page, pageSize: 5 }, function (result) {
        var html = '';
        if (result.data.length > 0) {
            $.each(result.data, function (i, topic) {
                html += ' <li><h3>' + topic.name + '</h3><a href="./Detail/' + topic.id + '"><img alt="' + topic.name + '" src="' + topic.topimage + '" /></a></li>';
            });
            $('#topicUl').append(html);
        }
        else {
            $('#autoLoad').html('没有更多专题了');
        }
    });
}