
$(document).ready(function() {

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
    $('.tag-back').click(function () {
        $(".dialog-tag").hide();
        $(".coverage").hide()
    });


})