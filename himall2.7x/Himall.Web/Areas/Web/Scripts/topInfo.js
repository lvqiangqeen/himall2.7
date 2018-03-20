$(function () {

    refreshCartProducts();
});
function refreshCartProducts() {
    $.post('/cart/GetCartProducts', {}, function (cart) {
        var products = cart.products;
        var count = cart.totalCount;
        $('#shopping-amount,#right_cart em').html(count);
    });
}

function logout() {
    $.removeCookie('Himall-User', { path: '/' });
    $.removeCookie('Himall-SellerManager', { path: "/" });
    window.location.href = "/Login";
}