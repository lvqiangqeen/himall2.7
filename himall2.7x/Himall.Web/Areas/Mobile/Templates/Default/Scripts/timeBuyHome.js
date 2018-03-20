function checkTime( time ) {
    time = time.replace(/-/g,"/");
    var endDate = new Date( time );
    var now = new Date();

    var result = endDate - now;
    result = result / 1000 / 60 / 60;
    return parseFloat(result.toFixed(2));
}

$(document).ready(function() {
    $( ".limited-nav ul" ).width( $( ".container" ).width() * 87 / 100 );
    $( ".limited-nav .glyphicon" ).width( $( ".container" ).width() * 13 / 100 - 22 );
    $( ".limited-frame" ).width( $( ".container" ).width() );
    $( ".limited-frame" ).height( $( ".container" ).width() );
    $( ".limi-hide-menu" ).width( $( ".container" ).width() );
    $( ".limited-nav>li" ).click( function() {
        $( this )
    } );
    $( "#limi-menu" ).click( function() {
        if ( $( this ).hasClass( "glyphicon-menu-down" ) ) {
            $( this ).removeClass( "glyphicon-menu-down" ).addClass( "glyphicon-menu-up" );
            $( ".limi-hide-menu" ).slideDown( "slow" );

        } else {
            $( this ).removeClass( "glyphicon-menu-up" ).addClass( "glyphicon-menu-down" );
            $( ".limi-hide-menu" ).slideUp( "slow" );
        }

    } );
} );