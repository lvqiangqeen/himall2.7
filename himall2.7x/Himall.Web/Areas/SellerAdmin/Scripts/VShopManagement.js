// JavaScript source code
$(function () {
    $('.view-mobile-shop').click(function () {

        $('.mobile-dialog').show();
        $('.cover').fadeIn();
        //document.frames['mobile-iframe'].location.reload();
        window.frames['mobile-iframe'].location.reload()
    });
    $('.cover').click(function () {
        $('.mobile-dialog').hide();
        $('.cover').fadeOut();
    });
})