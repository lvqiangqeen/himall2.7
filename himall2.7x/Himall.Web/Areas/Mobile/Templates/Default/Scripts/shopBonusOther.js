function LoadWXInfos()
{
    $.post( "/m-weixin/shopbonus/GetOtherReceive", { id: $( "#grant" ).val() }, function ( data )
    {
        var html = "";
        for ( var i = 0; i < data.length ; i++ )
        {
            html += '<li class="clearfix">';
            html += '<div class="head-portrait"><img src="' + data[i].HeadImg + '"></div>';
            html += '<div class="info">';
            html += '<div class="info-c"><span class="name">' + data[i].Name + '</span><time>' + data[i].ReceiveTime + '</time></div>';
            html += '<p>' + data[i].Copywriter + '</p>';
            html += '</div>';
            html += '<div class="money">' + data[i].Price + '元</div>';
            html += '</li>';
        }

        $( ".mid" ).html( html );

    } );
}