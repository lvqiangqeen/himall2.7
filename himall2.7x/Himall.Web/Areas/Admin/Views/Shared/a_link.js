/**
 * Created by Administrator on 2016/1/6 0006.
 */

$(function(){

    $(".table").on("mouseenter","a",function() {
            if (typeof($(this).attr("target")) == 'undefined') {
                $(this).css("color", "#1b7ccd")
            } else {
                $(this).css("color", "#ff6600");
            }
        }
    )

    $(".table").on("mouseleave","a",function(){
            $(this).css("color","#1b7ccd");
        }
    )
})
