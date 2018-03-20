/*!
 * HiShop Template Edit Config 
 * version: 1.0
 * build: Tue Aug 11 2015 11:16
 * author: CJZhao
 */

$(function () {
    HiShop.Config = HiShop.Config ? HiShop.Config : {}; //Config 命名空间
    /*!此配置路径在同一站点下，请求
     * 跨域请求就需要改动JS源码 换成dataType换成jsonp  后台返回需要加上callback()
     */
    HiShop.Config.AjaxUrl = {
        /*资源相关 start*/
        getFolderTree: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GetFolderTree",//获取图片分类菜单
        getImgList: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GetImgList",//获取图片列表
        addImg: "/common/UEditor/Handle?action=uploadtemplateimage",//上传图片
        moveImg: "/Admin/TemplateVisualizationAjax/Hi_Ajax_MoveImg",//移动图片  将图片移动到某个分类
        delImg: "/Admin/TemplateVisualizationAjax/Hi_Ajax_DelImg",//删除图片
        addFolder: "/Admin/TemplateVisualizationAjax/Hi_Ajax_AddFolder",//添加图片分类
        renameFolder: "/Admin/TemplateVisualizationAjax/Hi_Ajax_RenameFolder",//重命名图片分类
        delFolder: "/Admin/TemplateVisualizationAjax/Hi_Ajax_DelFolder",//删除图片分类
        moveCateImg: "/Admin/TemplateVisualizationAjax/Hi_Ajax_RemoveImgByFolder",//整个分类移动 
        renameImg: "/Admin/TemplateVisualizationAjax/Hi_Ajax_RenameImg",    //重命名图片名称
        /*资源相关 end*/
        /*页面操作相关  start*/
        savePage: "/Admin/TemplateVisualizationAjax/Hi_Ajax_SaveTemplate",//保存模板
        pageRecover: "/Admin/TemplateVisualizationAjax/Hi_Ajax_RenameFolder",//还原模板
        getPage: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GetTemplateByID",//获取模板（用户店铺） 
        /*页面操作相关  end*/
        Coupons: "/Admin/TemplateVisualizationAjax/Hi_Ajax_Coupons",//优惠劵
        Topic: "/Admin/TemplateVisualizationAjax/Hi_Ajax_Topic",//专题
        LimitBuy: "/Admin/TemplateVisualizationAjax/Hi_Ajax_LimitBuy",//限时购列表
        Bonus: "/Admin/TemplateVisualizationAjax/Hi_Ajax_Bonus",//现金红包
        /*商品相关 start*/
        GoodsList: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GetGoodsList",//商品列表
        goodGroup: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GoodsGourp",//商品分类
        /*商品相关 end*/
        gamesUrl: "/Admin/TemplateVisualizationAjax/Hi_Ajax_GetGames"
    }

    HiShop.Config.CodeBehind = {
        /*解析商品分组数据接口*/
        getGoodGroupUrl: "/api/Hi_Ajax_GoodsListGroup",//解析商品分组  
        /*解析商品数据接口*/
        getGoodUrl: "/api/Hi_Ajax_GoodsList"//解析商品 
      
    }
    HiShop.Config.HiTempLatePath = {
        GroupGoodTemp: "/Content/Modules/GoodGroup",
        TemplateExt:".cshtml"

    }
    



});