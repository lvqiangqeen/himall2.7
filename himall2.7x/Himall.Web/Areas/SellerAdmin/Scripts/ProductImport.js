var platFormCategoryId;
var categoryId;
var brandid;
var paraSaleStatus = -1;
var MaxFileSize = 15728640;//15M
var MaxImportCount = 20;
var RefreshInterval = 2;//Second
var _refreshProcess;//定时器
var loading;
var sellercate1 ;
var sellercate2 ;
$(function () {
    $("#btnFile").bind("change", function () {
        if ($("#btnFile").val() != '') {
            var dom_btnFile = document.getElementById('btnFile');
            if (typeof (dom_btnFile.files) == 'undefined') {
                try{
                    var fso = new ActiveXObject('Scripting.FileSystemObject');
                    var f = fso.GetFile($("#btnFile").val());
                    if (f.Size > MaxFileSize) {
                        $.dialog.tips('选择的文件太大');
                        return;
                    }
                }
                catch(e)
                {
                    errorTips(e);
                }
            }
            else {
                if (dom_btnFile.files.length > 0 && dom_btnFile.files[0].size > MaxFileSize) {
                    $.dialog.tips('选择的文件太大');
                    return;
                }
            }
            var filename = $("#btnFile").val().substring($("#btnFile").val().lastIndexOf('\\') + 1);
            $('#inputFile').val(filename);
        }
        else {
            $('#inputFile').val('请选择文件');
        }
    });
    $('#btnUpload').bind('click', function () {
        if ($('#category3').val() == null || $('#category3').val() == '') {
            $.dialog.tips('请选择一个子级类目');
            return;
        }
        sellercate1 = $('#sellercategory1').val() || '0';
        sellercate2 = $('#sellercategory2').val() || '0';

        if (sellercate1 == '' && sellercate2 == '') {
            $.dialog.tips('请选择一个商品分类');
            return;
        }
        paraSaleStatus = -1;
        $('input[name=paraSaleStatus]').each(function (idx, el) {
            if ($(el).attr('checked') == 'checked') {
                paraSaleStatus = $(el).attr('status');
            }
        });
        if (paraSaleStatus == -1)
        {
            $.dialog.tips('请选择一个商品状态');
            return;
        }
        if (parseInt($('#freightTemplate').attr('value')) <= 0) {
            $.dialog.tips('请选择一个运费模板！');
            return;
        }
        var filename = $('#inputFile').val();
        if (filename == '请选择文件')
        {
            $.dialog.tips('请选择文件');
            return;
        }
        else {
            GetImportOpCount();
        }
    });
    
    $('#category1,#category2,#category3').himallLinkage({
        url: 'GetPlatFormCategory',
        enableDefaultItem: true,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            platFormCategoryId = value;
            fnGetBrandByCategory(value);
        }
    });

    $('#sellercategory1,#sellercategory2').himallLinkage({
        url: '../Category/getCategory',
        enableDefaultItem: true,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });

    //初始导入状态
    var count = $('#inputCount').val();
    var total = $('#inputTotal').val();
    var success = $('#inputSuccess').val();
    if (count < total) {
        SetImportProcess();
        ShowImportProcess();
    }
    $('#btnAddTemplate').bind('click', function () {
        window.open('/SellerAdmin/freighttemplate/add?displayUrl=/SellerAdmin/FreightTemplate/Index');
    });
});
function fnUploadFileCallBack(filename) {
    var shopCategory = parseFloat(sellercate1);
    if (sellercate2 != '' && sellercate2!='0')
        shopCategory = parseFloat(sellercate2);
    $.ajax({
        type: 'get'
        , url: '../AsyncProductImport/ImportProduct'
        , data: { paraCategory: parseFloat($('#category3').val()), paraShopCategory: shopCategory, paraBrand: $('#selectBrand').val(), paraSaleStatus: paraSaleStatus, _shopid: $('#inputShopid').val(), _userid: $('#inputUserid').val(), freightId: $('#freightTemplate').val(), file: filename }
        , datatype: 'json'
        , success: function (data) {
            $('.ajax-loading').remove();
            $.dialog.alert(data.message, function () {
                window.history.go(0);
            }, 3);
        }
    });
}
function ShowImportProcess()
{
    _refreshProcess = setInterval(SetImportProcess, RefreshInterval * 1000);
}
function SetImportProcess()
{
    $.ajax({
        type: 'get'
       , url: 'GetImportCount'
       , datatype: 'json'
       , success: function (data) {
           if (data.Total > data.Count) {
               if ($('.ajax-loading').length == 0) {
                   loading = showLoading(data.Count + '/' + data.Total);
                   if ($('.ajax-loading').css('display') == 'none') {
                       $('.ajax-loading').show();
                   }
               }
               else {
                   if ($('.ajax-loading').css('display') == 'none')
                   {
                       $('.ajax-loading').show();
                   }
                   $('.ajax-loading p').text(data.Count + '/' + data.Total);
               }
           }
           if (data.Success == 1) {
               clearInterval(_refreshProcess);//完成后清除定时器
               $('.ajax-loading').hide();
               $.dialog.succeedTips('导入完成');
           }
       }
    });
}

function GetImportOpCount() {
    $.ajax({
        type: 'get'
       , url: 'GetImportOpCount'
       , datatype: 'json'
       , success: function (data) {
           if (data.Count >= MaxImportCount) {
               $.dialog.tips('上传人数较多，请稍等。。。');
               return;
           }
           var dom_iframe = document.getElementById('iframe');
           //非IE、IE
           dom_iframe.onload = function () {
               var filename = this.contentDocument.body.innerHTML;
               if (filename != 'NoFile' && filename != 'Error') {
                   fnUploadFileCallBack(filename);//上传文件后，继续导入商品操作
                   $('#inputFile').val('请选择文件');
               }
               else {
                   $.dialog.tips('上传文件异常');
               }
               this.onload = null;
               this.onreadystatechange = null;
           };
           //IE
           dom_iframe.onreadystatechange = function () {
               if (this.readyState == 'complete' || this.readyState == 'loaded') {
                   var filename = this.contentDocument.body.innerHTML;
                   if (filename != 'NoFile' && filename != 'Error') {
                       fnUploadFileCallBack(filename);//上传文件后，继续导入商品操作
                   }
                   else {
                       loading.close();
                       $.dialog.tips('上传文件异常');
                   }
                   this.onload = null;
                   this.onreadystatechange = null;
               }
           };
           loading = showLoading('正在上传');
           $('#formUpload').submit();
       }
    });
}

function fnGetBrandByCategory(cateid) {
    $('#selectBrand').himallLinkage({
        url: 'GetShopBrand?categoryId=' + cateid,
        enableDefaultItem: true,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            brandid = value;
        }
    });
}
function BindfreightTemplate() {
    $.post('GetFreightTemplate', {}, function (result) {
        if (result.success) {
            for (var i = 0 ; i < result.model.length; i++) {
                var opt = $('#freightTemplate').find('option[value=' + result.model[i].Value + ']');
                if (opt.length == 0) {
                    $('#freightTemplate').append('<option value="' + result.model[i].Value + '">' + result.model[i].Text + '</option>');
                }
            }
        }
    });
}
