$( function ()
{
    $("#subCate").click(function () {
            if (check()) {
                document.getElementById("BonusForm").submit();
                $("#subCate").attr("disabled", true);
            }
    })


    var priceType = $('#priceType').val();
    var type = $("#Type0").val();
    var style =$("#style0").val();

    $( "#IsAttention" ).change( function ()
    {
        if ( $( "#IsAttention" ).is( ":checked" ) )
        {
            $.dialog.confirm( "强制用户关注公众号、引诱用户分享等行为可能会引起微信对活动页面进行拦截，严重时甚至会封号，是否确认开启？",
                function () { },
                function ()
                {
                    $( "#IsAttention" ).removeAttr( "checked" );
                } )
        }
    } );

    $( "#IsGuideShare" ).change( function ()
    {
        if ( $( "#IsGuideShare" ).is( ":checked" ) )
        {
            $.dialog.confirm( "强制用户关注公众号、引诱用户分享等行为可能会引起微信对活动页面进行拦截，严重时甚至会封号，是否确认开启？",
                function () { },
                function ()
                {
                    $( "#IsGuideShare" ).removeAttr( "checked" );
                } )
        }
    } );

    $( "input[name=FixedAmount]" ).attr( 'disabled', "true" );
    $( "input[name=RandomAmountEnd]" ).attr( 'disabled', "true" );
    $( "input[name=RandomAmountStart]" ).attr( 'disabled', "true" );
    $( "input[name=PriceType]" ).attr( 'disabled', "true" );
    $( "input[name=Type]" ).attr( 'disabled', "true" );
    $("#TotalPrice").attr( 'disabled', "true" );
    if ( priceType == 1 )
    {
        $( "input[name=PriceType]:eq(0)" ).attr("checked",'checked');
    }
    else if(priceType == 2)
    {
        $( "input[name=PriceType]:eq(1)" ).attr("checked",'checked');
    }
    
    if(type == 1)
    {
        $( "input[name=Type]:eq(0)" ).attr("checked",'checked');
        $( "#typemsg" ).addClass( "hide" );
        $( "#Name" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#MerchantsName" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#Remark" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#Blessing" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#Description" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#upload-img" ).parents( ".form-group" ).removeClass( "hide" );
        $( "#IsAttention" ).parents( ".form-group" ).removeClass( "hide" );
        $("#IsGuideShare").parents(".form-group").removeClass("hide");
        $("#StartTime").parents(".form-group").removeClass("hide");
        $("#EndTime").parents(".form-group").removeClass("hide");
      


    }
    else if( type == 2)
    {
        $( "input[name=Type]:eq(1)" ).attr("checked",'checked');
        $( "#typemsg" ).removeClass( "hide" );
        $( "#Name" ).parents( ".form-group" ).addClass( "hide" );
        $( "#MerchantsName" ).parents( ".form-group" ).addClass( "hide" );
        $( "#Remark" ).parents( ".form-group" ).addClass( "hide" );
        $( "#Blessing" ).parents( ".form-group" ).addClass( "hide" );
        $( "#Description" ).parents( ".form-group" ).addClass( "hide" );
        $( "#upload-img" ).parents( ".form-group" ).addClass( "hide" );
        $( "#IsAttention" ).parents( ".form-group" ).addClass( "hide" );
        $("#IsGuideShare").parents(".form-group").addClass("hide");
        $("#StartTime").parents(".form-group").removeClass("hide");
        $("#EndTime").parents(".form-group").removeClass("hide");
        $(".form-group.bunus-tip").removeClass("hide");
    }
    else if (type == 3) {
        $("input[name=Type]:eq(2)").attr("checked", 'checked');
        $("#typemsg").addClass("hide");

        $("#Name").parents(".form-group").removeClass("hide");
        $(".col-sm-7").addClass("hide");
        $("#labBonusName")[0].innerHTML = "红包名称：";

        $("#MerchantsName").parents(".form-group").addClass("hide");
        $("#Remark").parents(".form-group").addClass("hide");
        $("#Blessing").parents(".form-group").addClass("hide");
        $("#Description").parents(".form-group").addClass("hide");
        $("#upload-img").parents(".form-group").addClass("hide");
        $("#IsAttention").parents(".form-group").addClass("hide");
        $("#IsGuideShare").parents(".form-group").addClass("hide");

        $("#StartTime").parents(".form-group").addClass("hide");
        $("#EndTime").parents(".form-group").addClass("hide");
        $(".form-group.bunus-tip").addClass("hide");
    }
    if(style == 1)
    {
        $( "input[name=Style]:eq(0)" ).attr("checked",'checked');
            
    }
    else if( style == 2)
    {
        $( "input[name=Style]:eq(1)" ).attr("checked",'checked');
            
    }
    else if (style == 3) {
        $("input[name=Style]:eq(2)").attr("checked", 'checked');

    }

        

    $( "#upload-img" ).himallUpload( {
        title: '',
        imageDescript: '',
        imgFieldName: "ImagePath",
        displayImgSrc: $("#MIpath").val(),
        dataWidth: 10
    } );

    $( ".start_datetime" ).datetimepicker( {
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        beforeShow:"2012-2-2",
        minView: 2
    } );
    $( ".end_datetime" ).datetimepicker( {
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        beforeShow: "2012-2-2",
        minView: 2
    } );

    $( '.start_datetime' ).on( 'changeDate', function ()
    {
        if ( $( ".end_datetime" ).val() )
        {
            if ( $( ".start_datetime" ).val() > $( ".end_datetime" ).val() )
            {
                $( '.end_datetime' ).val( $( ".start_datetime" ).val() );
            }
        }

        $( '.end_datetime' ).datetimepicker( 'setStartDate', $( ".start_datetime" ).val() );
    } );
} )

function check()
{
    var bonusType = $('input[name=Type]:checked').val();
    if (bonusType == 3) {
        if ($.trim($("#Name").val()) == '') {
            $.dialog.tips('红包名称必填！');
            return false;
        }
    }
    return true;
}