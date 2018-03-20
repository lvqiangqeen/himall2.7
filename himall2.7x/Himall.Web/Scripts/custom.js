/// <reference path="jquery-1.11.1.js" />
// JavaScript Document

$(document).ready(function () {

    $('.navbar-nav .dropdown').hover(function () {
        $(this).addClass('open');
    }, function () {
        $(this).removeClass('open');
    });

    $('.navbar-nav .dropdown-menu').each(function () {
        $(this).css('marginLeft', -$(this).width() / 2)
    });

    $('.dialog-simple .close,.dialog-simple #next,.cover').click(function () {
        $('.cover,.dialog-simple').fadeOut();
    })

    $('.nav.navbar-right .dropdown li').click(function () {
        var href = $(this).find('a').attr('href');
        if (typeof (href) == "undefined") {
            $(this).find('li').first().click();
        } else {
            $('.nav.navbar-left li').find('a').each(function () {
                if (href == $(this).attr('href')) {
                    $(this).parent().click();
                }
            })
        }
    });
   
    
});


function addCookie(name, value, expiresHours) {
    var cookieString = name + "=" + escape(value);
    if (expiresHours > 0) {
        var date = new Date();
        date.setTime(date.getTime() + expiresHours * 3600 * 1000);
        cookieString = cookieString + "; expires=" + date.toGMTString();
    }
    document.cookie = cookieString;
}


function getCookie(name) {
    var strCookie = document.cookie;
    var arrCookie = strCookie.split("; ");
    for (var i = 0; i < arrCookie.length; i++) {
        var arr = arrCookie[i].split("=");
        if (arr[0] == name) return arr[1];
    }
    return "";
}

function deleteCookie(name) {
    var date = new Date();
    date.setTime(date.getTime() - 10000);
    document.cookie = name + "=v; expires=" + date.toGMTString();
}

function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}