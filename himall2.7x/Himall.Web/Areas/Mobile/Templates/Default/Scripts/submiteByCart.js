    var paymentShown = false;
    var loading;

    function integralSubmit( ids )
    {
        ajaxRequest( {
            type: 'POST',
            url: '/' + areaName + '/Order/PayOrderByIntegral',
            param: { orderIds: ids },
            dataType: 'json',
            success: function ( data )
            {
                if ( data.success == true )
                {
                    $.dialog.succeedTips( '支付成功！', function ()
                    {
                        location.href = '/' + areaName + '/Member/Orders';
                    }, 0.5 );

                }
            },
            error: function ( data ) { $.dialog.tips( '支付失败,请稍候尝试.', null, 0.5 ); }
        } );
    }

$('#submit-order').click(function () {
    var cartItemIds = QueryString( 'cartItemIds' );

    //发票相关
    var invoiceType = 0
    var invoiceTitle = "";
    var invoiceContext = "";
    if ( !( $( ".bill a" ).text() == "不需要发票" ) )
    {
        invoiceType = 2;
        invoiceTitle = $( ".bill a" ).text();
        invoiceContext = $( ".bill-Cart .content-bill .active " ).parent().text();
    }

    //是否货到付款
    var isCashOnDelivery = false; 
    if ( $( "#icod" ).val() == "True" )
    {
        isCashOnDelivery = $( ".way-01 .offline" ).hasClass( "active" ); 
    }
    var recieveAddressId = $('#shippingAddressId').val();
    //var orderRemarks = $("#orderRemarks").val();
    var flag = true;
    var orderRemarks = [];
    $(".orderRemarks").each(function () {
        orderRemarks.push($(this).val());
        if ($(this).val().length > 200) {
            flag = false;
        } 
    });
    if (!flag) {
        $.dialog.tips('留言信息至多200个汉字！');
        return false;
    }
    recieveAddressId = parseInt(recieveAddressId);
    recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;

    var integral = 0;
    if ( isintegral )
    {
        integral = $("#userIntegrals").val();
        integral = isNaN(integral) ? 0 : integral;
    }


    var couponIds = "";
    $( 'input[name="couponIds"]' ).each( function ( i, e )
    {
        var type = $( this ).attr( "data-type" );
        couponIds = couponIds + $(e).val() +'_'+type+ ',';
    })
    couponIds = couponIds.substring(0, couponIds.length - 1);

    if ( !recieveAddressId )
    {
        $.dialog.alert( '请选择或新建收货地址' );
    }
    else
    {
        var model = {};
        model.cartItemIds = cartItemIds;
        model.recieveAddressId = recieveAddressId;
        model.couponIds = couponIds;
        model.integral = integral;
        model.isCashOnDelivery = isCashOnDelivery;
        model.invoiceType = invoiceType;
        model.invoiceTitle = invoiceTitle;
        model.invoiceContext = invoiceContext;
        model.orderRemarks = orderRemarks.toString();

        loading = showLoading();
        var total = parseFloat( $( "#total" ).val() );
        $.post( '/' + areaName + '/Order/IsAllDeductible', { integral: model.integral, total: total }, function ( result )
        {
            loading.close();
            if ( result )
            {
                $.dialog.confirm( "您确定用积分抵扣全部金额吗?", function ()
                {
                    submit( model );
                } )
            }
            else
            {
                submit( model );
            }
        } )
        
    }
});


function submit( model )
{
    loading = showLoading();
    $.post('/' + areaName + '/Order/SubmitOrderByCart', { cartItemIds: model.cartItemIds, recieveAddressId: model.recieveAddressId, couponIds: model.couponIds, integral: model.integral, isCashOnDelivery: model.isCashOnDelivery, invoiceType: model.invoiceType, invoiceTitle: model.invoiceTitle, invoiceContext: model.invoiceContext, orderRemarks: model.orderRemarks }, function (result)
    {
        if ( result.success )
        {
            if ( model.isCashOnDelivery && $( "#onlyshop1" ).val() == "True" )
            {
                loading.close();
                if ( result.realTotalIsZero )
                {
                    integralSubmit( result.orderIds.toString() );
                }
                else
                {
                    $.dialog.succeedTips( '提交成功！', function ()
                    {
                        location.href = '/' + areaName + '/Member/Orders';
                    }, 0.5 );
                }

            }
            else if ( result.realTotalIsZero )
            {
                loading && loading.close();
                integralSubmit( result.orderIds.toString() );
            }
            else
            {
                loading && loading.close();
                GetPayment(result.orderIds.toString());
            }
        }
        else
        {
            loading && loading.close();
            $.dialog.alert( result.msg );
        }
    } );
}

$("#choiceAddr").click(function () {
    location.href = '/' + areaName + '/Order/ChooseShippingAddress?returnURL=' + encodeURIComponent(location.href);

});

$(document).on('click','.cover, #paymentsChooser .close',function () {
    $('.cover,.custom-dialog').fadeOut();
    if (paymentShown) {//如果已经显示支付方式，则跳转到订单列表页面
        location.href = '/' + areaName + '/Member/Orders';
    }
});

$("#addaddr").click(function () {
    location.href = '/' + areaName + '/Order/EditShippingAddress?addressId=0&returnURL=' + encodeURIComponent(location.href);
})