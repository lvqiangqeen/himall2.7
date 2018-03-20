$( function ()
{
    initGrid();
    //bindSearchBtn();
    initCateogrySelector();
} );

function initCateogrySelector()
{
    $( '#category1,#category2,#category3' ).himallLinkage( {
        url: '../category/getCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function ( level, value, text )
        {
            $( '#categoryId' ).val( value );
        }
    } );
}

function bindSearchBtn()
{
    var vshopName = $( '#vshopName' ).val();
    var vshopState = $( '#vshopState' ).val();
    $( "#list" ).hiMallDatagrid( 'reload', { vshopName: vshopName, vshopType: vshopState } );
}


function initGrid()
{
    //商品表格
    $( "#list" ).hiMallDatagrid( {
        url: '/admin/VShop/GetVshops',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        columns:
        [[
             {
                 field: "name", title: '微店名称', align: "center"
             },
            {
                field: "creatTime", title: '创建时间', align: "center"
            },
            {
                field: "vshopTypes", title: '微店类型', align: "center"
            },
            {
                field: "StateStr", title: '状态', align: "center"
            },
            {
                field: "visiteNum", title: '进店浏览量', align: "center"
            },
            {
                field: "buyNum", title: "成交量", align: "center"
            },
            {
                field: "v", title: "预览", align: "center",
                formatter: function ( value, row, index )
                {
                    var html = '<a class="glyphicon glyphicon-eye-open view-mobile-shop" title="预览微店" data-url="/m/vshop/detail/' + row.id + '?sv=True" style="font-size:13px;text-decoration: none; cursor:pointer;"></a>';
                    return html;
                },
            },
            {
                field: "s", title: "操作", align: "center",
                formatter: function ( value, row, index )
                {
                    console.log( row );
                    var html = '<span class="btn-a">';
                    if ( row.vshopTypes != "主推微店" )
                    {
                        html += '<a class="good-setTopshop">设为主推</a>';
                    }
                    html += '<input class="thisId" type="hidden" value="' + row.id + '"/><input class="thisName" type="hidden" value="' + row.name + '"/>';
                    if ( row.vshopTypes != "热门微店" )
                    {
                        html += '<a class="good-sethot">设为热门</a>';
                    }
                    if ( row.State == 2 )
                    {
                        html += '<a class="good-down">下架</a></span>';
                    }
                    else if ( row.State == 99 )
                    {
                        html += "<a class='good-open'>开启</a></span>";
                    }
                    
                    //SetShopNormal
                    return html;
                },
            }
        ]]
    } );
}

$( '#list' ).on( 'click', '.good-setTopshop', function ()
{
    var name = $( this ).siblings( '.thisName' ).val();
    var ids = $( this ).siblings( '.thisId' ).val();
    var loading = showLoading();
    $.post( '../VShop/SetTopVshop', { vshopId: ids }, function ( result )
    {
        loading.close();
        if ( result.success )
        {
            $.dialog.tips( '已将' + name + '成功设为主推微店', function ()
            {
                location.reload();
            }, 3 );
        }
        else
            $.dialog.errorTips( '设置失败！' + result.msg );
    } );
} );



$( '#list' ).on( 'click', '.good-open', function ()
{
    var name = $( this ).siblings( '.thisName' ).val();
    var id = $( this ).siblings( '.thisId' ).val();

    $.dialog.confirm( '确定开启此微店？', function ()
    {
        var loading = showLoading();
        $.post( "../VShop/SetShopNormal", { vshopId: id }, function ( result )
        {
            loading.close();
            if ( result.success )
            {
                $.dialog.tips( '成功开启微店', function ()
                {
                    location.reload();
                } );
            }
            else
            {
                $.dialog.errorTips( '开启失败！' + result.msg );
            }
        } )
    } );

} );

$( '#list' ).on( 'click', '.good-sethot', function ()
{
    var name = $( this ).siblings( '.thisName' ).val();
    var id = $( this ).siblings( '.thisId' ).val();
    var loading = showLoading();
    $.post( '../VShop/SetHotVshop', { vshopId: id }, function ( result )
    {
        loading.close();
        if ( result.success )
        {
            $.dialog.tips( '已将' + name + '成功设为热门微店', function ()
            {
                location.reload();
            } );

        }
        else
            $.dialog.errorTips( '设置失败！' + result.msg );
    } );
} );

$( '#list' ).on( 'click', '.good-down', function ()
{
    var name = $( this ).siblings( '.thisName' ).val();
    var id = $( this ).siblings( '.thisId' ).val();
    $.dialog.confirm('下架后该商家的微店将无法访问且不再显示在商城微店列表中,是否确定下架 ' + name + '?', function ()
    {
        var loading = showLoading();
        $.post( '../VShop/DeleteVshop', { vshopId: id }, function ( result )
        {
            loading.close();
            if ( result.success )
            {
                $.dialog.succeedTips( '设置成功' );
                location.reload();
            }
            else
                $.dialog.errorTips( '设置失败！' + result.msg );
        } );
    } );
});
$(document).on('click', '.view-mobile-shop', function () {
    $("#mobileshow").attr("src", $(this).attr("data-url"));
    $('.mobile-dialog').show();
    $('.cover').fadeIn();
});
$('.cover').click(function () {
    $('.mobile-dialog').hide();
    $("#mobileshow").attr("src", "about:blank");
    $('.cover').fadeOut();
});