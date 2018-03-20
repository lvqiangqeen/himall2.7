function emailCheck(emailStr) {
    var pattern = /^([\.a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(\.[a-zA-Z0-9_-])+/;
    if (!pattern.test(emailStr)) {
        return false;
    } else {
        return true;
    }
}