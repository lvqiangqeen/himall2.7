// JavaScript source code

$( function ()
{
    $( "#pass" ).click( function ()
    {
        Pass($("#mdid0").val());
    } )

    $( "#re" ).click( function ()
    {
        Refuse($("#mdid0").val());
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
                    window.location.href = $("#UAmag").val();
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
                    window.location.href = $("#UAmag").val();
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
