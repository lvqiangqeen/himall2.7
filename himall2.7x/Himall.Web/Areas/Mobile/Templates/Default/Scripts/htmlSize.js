(function() {
    var baseFontSize = 100;
    var baseWidth = 320;
    var clientWidth = document.documentElement.clientWidth || window.innerWidth;
    var innerWidth = Math.max(Math.min(clientWidth, 480), 320);

    var rem = 100;

    if (innerWidth > 362 && innerWidth <= 375) {
        rem = Math.floor(innerWidth / baseWidth * baseFontSize);
        //rem = Math.floor(innerWidth / baseWidth * baseFontSize * 0.9);
    }
    
    if (innerWidth > 375) {
        rem = Math.floor(innerWidth / baseWidth * baseFontSize);
        //rem = Math.floor(innerWidth / baseWidth * baseFontSize * 0.84);
    }

    window.__baseREM = rem;
    document.querySelector('html').style.fontSize = rem + 'px';
}());