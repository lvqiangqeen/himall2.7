// JavaScript source code
$(function () {
    $("#SiteName").focus();
});
$(function () {
    $('#Save').click(function () {
        var loading = showLoading();
        $.post('./Edit', $('form').serialize(), function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功');
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });

    function ShowRegEmailBox() {
        var regtype = $("input[name='RegisterType']:checked").val();
        if (regtype == 1) {
            $("#regemailbox").hide();
        } else {
            $("#regemailbox").show();
        }
    }

    $(function () {

        $("input[name='RegisterType']").click(function () {
            ShowRegEmailBox();
        });
        ShowRegEmailBox();

        $('#Logo').himallUpload({
            title: '网站Logo',
            imageDescript: '最佳尺寸：200*60  显示在商城头部、会员登录处等位置',
            displayImgSrc: $("#Logo1").val(),
            imgFieldName: "Logo",
            dataWidth: 8
            
        });

        $('#wxlogobox').himallUpload({
            title: 'Logo <em>(正方形)</em> 微信Logo',
            imageDescript: '建议上传100*100的图片,微信卡券使用',
            displayImgSrc: $("#WXLogo1").val(),
            imgFieldName: "WXLogo",
            dataWidth: 8
        });

        $('#MemberLogo').himallUpload({
            title: '卖家中心logo',
            imageDescript: '最佳尺寸：180*40  显示在卖家中心导航处',
            displayImgSrc: $("#MemberLogo1").val(),
            imgFieldName: "MemberLogo",
            dataWidth: 8
        });

        $('#QRCode').himallUpload({
            title: '微信二维码',
            imageDescript: '最佳尺寸：90*90  显示在商城底部',
            displayImgSrc: $("#QRCode1").val(),
            imgFieldName: "QRCode",
            dataWidth: 8
        });
        $('#PCLoginPic').himallUpload({
            title: 'PC登录区域',
            imageDescript: '最佳尺寸：460*400  显示在PC登录页左侧',
            displayImgSrc: $("#PCLoginPic1").val(),
            imgFieldName: "PCLoginPic",
            dataWidth: 8
        });
    })
    $("#btnFile").bind("change", function () {
        if ($("#btnFile").val() != '') {
            var dom_btnFile =$('#btnFile');
                        
            //准备表当
            var myform = document.createElement("form");
            myform.action = "./UploadApkFile";
            myform.method = "post";
            myform.enctype = "multipart/form-data";
            myform.style.display = "none";
            //将表单加当document上，
            document.body.appendChild(myform);  //重点
            var form = $(myform);

            var fu = dom_btnFile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
            var fu1 = dom_btnFile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。



            //开始模拟提交表当。
            form.ajaxSubmit({
                success: function (data) {
                    if (data == "NoFile" || data == "Error" || data == "上传的文件格式不正确") {
                        $.dialog.errorTips(data);
                    }
                    else {
                        //文件上传成功，返回图片的路径。将路经赋给隐藏域
                        $('#AndriodDownLoad').val(data);

                    }
                    //$(".divFile").append(fu1);
                    $(fu1).insertAfter($(".divFile"));
                    form.remove();
                }
            });
            
        }
        else {
            $('#inputFile').val('请选择文件');
        }
    });
})
function openBrowse() {
    var ie = navigator.appName == "Microsoft Internet Explorer" ? true : false;
    if (ie) {
        document.getElementById("btnFile").click();
    } else {
        var a = document.createEvent("MouseEvents");//FF的处理 
        a.initEvent("click", true, true);
        document.getElementById("btnFile").dispatchEvent(a);
    }
}