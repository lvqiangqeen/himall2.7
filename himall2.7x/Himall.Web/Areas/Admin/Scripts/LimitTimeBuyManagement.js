// JavaScript source code

$( function ()
{
    var limi_m =$("#limi_m").val();
    $( "#pass" ).click( function ()
    {
        Pass(limi_m );
    } )

    $( "#re" ).click( function ()
    {
        Refuse(limi_m );
    } )
} )


function Pass( id )
{
    $.dialog.confirm( '您确定要审核此活动吗？', function ()
    {
        var loading = showLoading();
        $.post( "../AuditItem", { id: id }, function ( data )
        {
            if ( data.success == true )
            {
                $.dialog.succeedTips( "成功审核！", function ()
                {
                    window.location.href = $("#Uama0").attr("href");
                }, 0.5 );
            }
            else
            {
                $.dialog.errorTips( data.msg );
                loading.close();
            }
        } )
    } );
}

function Refuse( id )
{
    $.dialog.confirm( '您确定要拒绝吗？', function ()
    {
        var loading = showLoading();
        $.post( "../RefuseItem", { id : id }, function ( data )
        {
            if ( data.success == true )
            {
                $.dialog.succeedTips( "成功拒绝！", function ()
                {
                    window.location.href = $("#Uama0").attr("href");
                }, 0.5 );
            }
            else
            {
                $.dialog.errorTips( data.msg );
                loading.close();
            }
        } )
    } );
}