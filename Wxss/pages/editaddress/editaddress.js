// pages/raiseaddress/raiseaddress.js
var config = require("../../utils/config.js");
//var area = require('../../area/area.js')
var procitydata=null;
var cityRegionIds = new Array();//已加载的城市ID
var areadata = new Array();
var areaRegionIds = new Array();//已加载的地区ID
var streetdata = new Array();
var p = 0, c = 0, d = 0 , s = 0
var app = getApp();
var currAreaData = [];
var currStreetData = [];

Page({
  data: {
    navigateTitle: "",
    addressData: {},
    shipTo: "",
    cellPhone: "",
    fullAddress: "",
    address: "",
    regionId:"",

    provinceName: [],
    provinceCode: [],
    provinceSelIndex: '',
    cityName: [],
    cityCode: [],
    citySelIndex: '',
    districtName: [],
    districtCode: [],
    districtSelIndex: '',
    streetName: [],
    streetCode: [],
    streetSelIndex: '',
    showMessage: false,
    messageContent: '',
    showDistpicker: false,
    Source:"",
    ProductSku: '',
    BuyAmount: 0,
    FromPage: '',
    CountdownId: '',
    ShipAddressId: '',
    FullRegionName:'请填写所在地区',
    isCss:true
  },
  onLoad: function (options) {
    this.setAreaData();
    // 页面初始化 options为页面跳转所带来的参数
    var navigateTitle = options.title;
    this.data.navigateTitle = navigateTitle;
    
    wx.setNavigationBarTitle({
      title: this.data.navigateTitle
    })
    var regionId = 0;
    if (navigateTitle == '编辑收货地址') {
      var addressdata = JSON.parse(options.extra);
      var regionName = addressdata.FullAddress.replace(" "+addressdata.Address,"");
      if (addressdata.FullRegionPath != undefined && addressdata.FullRegionPath != null) {
        var regionArr = addressdata.FullRegionPath.split(',');
        regionId = regionArr[regionArr.length - 1];
      }
      this.setData({
        addressData: addressdata,
        shipTo: addressdata.ShipTo,
        cellPhone: addressdata.CellPhone,
        fullAddress: addressdata.FullAddress,
        FullRegionName: addressdata.FullRegionName,
        address: addressdata.Address,
        ProductSku : options.productsku,
        cartItemIds: options.cartItemIds,
        BuyAmount : options.buyamount,
        FromPage : options.frompage,
        CountdownId : options.countdownid,
        ShipAddressId : options.shipaddressid,
        Source:options.Source,
        isCss:false
      })
    }
    
   
    this.setData({
      regionId: regionId,
      ProductSku: options.productsku,
      cartItemIds: options.cartItemIds,
      BuyAmount: options.buyamount,
      FromPage: options.frompage,
      CountdownId: options.countdownid,
      ShipAddressId: options.shipaddressid,
      Source: options.Source,
    })
  },

  bindShipToTap: function (e) {
    var shipTo = e.detail.value;
    var that = this;
    that.data.shipTo = shipTo;
  },

  bindCellPhoneTap: function (e) {
    var cellPhone = e.detail.value;
    var that = this;
    that.data.cellPhone = cellPhone;
  },

  bindFullAddressTap: function (e) {
    p = 0, c = 0, d = 0;
    this.setAreaData();
    this.setData({
      showDistpicker: true
    });

  },

  bindAddressTap: function (e) {
    var address = e.detail.value;
    var that = this;
    that.data.address = address;
  },

  bindSaveTapTap: function (e) {
    var that = this;
    if (that.data.shipTo.length == 0) {
      wx.showToast({
        title: '请输入收货人',
        icon: 'fail',
        duration: 2000
      })
      return;
    } else if (that.data.cellPhone.length == 0) {
      wx.showToast({
        title: '请输入联系电话',
        icon: 'fail',
        duration: 2000
      })
      return;
    } else if (that.data.fullAddress.length == 0) {
      wx.showToast({
        title: '请输入所在地区',
        icon: 'fail',
        duration: 2000
      })
      return;
    } else if (that.data.address.length == 0) {
      wx.showToast({
        title: '请输入详细地址',
        icon: 'fail',
        duration: 2000
      })
      return;
    }
    app.getOpenId(function (openId) {
      wx.showNavigationBarLoading();
      if (that.data.navigateTitle == '新增收货地址') {
        var parameters = {
          openId: openId,
          shipTo: that.data.shipTo,
          address: that.data.address,
          cellphone: that.data.cellPhone,
          regionId: that.data.regionId
        }
        config.httpPost(app.getUrl(app.globalData.addShippingAddress), parameters, that.getEditAddressData);
      } else {
        var parameters = {
          openId: openId,
          shippingId: that.data.addressData.ShippingId,
          isDefault: that.data.addressData.IsDefault,
          shipTo: that.data.shipTo,
          address: that.data.address,
          cellphone: that.data.cellPhone,
          regionId: that.data.regionId
        }
        config.httpPost(app.getUrl(app.globalData.updateShippingAddress), parameters, that.getEditAddressData);
      }
    })
  },

  getEditAddressData: function (res) {
    wx.hideNavigationBarLoading();
    if (res.Message == "NOUser") {
      wx.navigateTo({
        url: '../login/login'
      })
    } else if (res.Status == "OK") {
      var source = this.data.Source;
      var toUrl="";
      if (source == undefined || source==""){
          toUrl="../address/address";
        }
        else{
          //如果是从选择地址页面过来的则返回选择地址页面
        if (source ="choiceaddress"){
            toUrl = '../choiceaddress/choiceaddress?productsku=' + this.data.ProductSku + '&cartItemIds=' + this.data.cartItemIds + '&buyamount=' + this.data.BuyAmount + '&frompage=' + this.data.FromPage +'&countdownid=' + this.data.CountdownId;
          }
          else if(source="submmitorder"){
            //如果是从提交订单页面过来的则返回提交订单页面
            toUrl = '../submitorder/submitorder?productsku=' + this.data.ProductSku + '&cartItemIds=' + this.data.cartItemIds + '&buyamount=' + this.data.BuyAmount + '&frompage=' + this.data.FromPage + '&countdownid=' + this.data.CountdownId + '&shipaddressid=' + res.Message;
          }
        }
        if(toUrl!=undefined&&toUrl!=""){
          wx.redirectTo({
            url: toUrl
          });
        }
    }
    else{
      wx.showToast({
        title: res.Message,
        icon: 'loading',
        duration: 10000
      })

      setTimeout(function () {
        wx.hideToast()
      }, 2000)

    }
  },

  changeArea: function (e) {
    const that=this;
    p = e.detail.value[0];
    c = e.detail.value[1];
    if(e.detail.value.length>2)
      d = e.detail.value[2];
    else
      d = 0;
    s = 0;
    console.log('省:'+p+'市:'+c+'区:'+d);
  //  if (e.detail.value.length > 3)
   //   s = e.detail.value[3]
  //  else
  //     s = "";
    that.setAreaData(p, c, d, s)
  },
  showDistpicker: function () {
    this.setData({
      showDistpicker: true
    })
  },
  distpickerCancel: function () {
    this.setData({
      showDistpicker: false
    })
  },
  distpickerSure: function () {
    console.log('确定省：'+p+'确定市：'+c+'确定区：'+d);
    var fullAddress = this.data.provinceName[p] +" "+ this.data.cityName[c] +" "+ this.data.districtName[d]; //+ this.data.streetName[s];
    var regionId;
    if (this.data.streetCode.length>0){
      regionId = this.data.streetCode[s]
    } else if (this.data.districtCode.length > 0){
      regionId = this.data.districtCode[d]
    }
    else if (this.data.cityCode.length > 0){
      regionId = this.data.cityCode[c]
    }
    var cssval = this.data.isCss;
    if (this.data.FullRegionName =='请填写所在地区'){
      cssval=false;
    }
    this.setData({
      fullAddress: fullAddress,
      FullRegionName:fullAddress,
      regionId: regionId,
      isCss: cssval
    })
    this.distpickerCancel()
  },
  ArrayContains:function(arr, obj) {
    var i = arr.length;
    while (i--) {
      if (arr[i] === obj) {
        return true;
      }
    }
    return false;
  },
  getRegions: function (regionId, depth, a, s){
    const that = this;
    var hasData=true;
    if(depth==3){
      //如果未包含则获取
      if (!that.ArrayContains(cityRegionIds,regionId)){
        hasData=false;
      }
    }
    else if(hasData==4){
      //如果未包含则获取
      if (!that.ArrayContains(areaRegionIds, regionId)) {
        hasData = false;
      }
    }
    wx.request({
      url: app.getUrl("Region/GetSub"),
      async: false,
      data: {
        parentId:regionId
      },
      success: function (result) {
        console.log(result);
        if (result.data.Status == "OK") {
          if (result.data.Depth==3){
            
            that.setAreaDataShow(result.data.Regions, regionId, a, s);
          }
          else if(result.Depth==4){
            that.setStreetData(result.data.Regions, regionId, a, s);
          }
        }
        else {

        }
      }
    });
  },
  setProvinceCityData:function(data,p,c,a,s){
    const that = this;
      if(data!=null){
        procitydata = data;
      }
      var province = procitydata;
      var provinceName = [];
      var provinceCode = [];
      for (var item in province) {
        var name = province[item]["name"];
        var code = province[item]["id"];
        provinceName.push(name)
        provinceCode.push(code)
      }
      that.setData({
        provinceName: provinceName,
        provinceCode: provinceCode
      })
      // 设置市的数据
      var city = procitydata[p]["city"];
      var cityName = [];
      var cityCode = [];
      var cIndex=0;
      for (var item in city) {
        var name = city[item]["name"];
        var code = city[item]["id"];
       
        cIndex+=1;
        cityName.push(name)
        cityCode.push(code)
      }
      
      that.setData({
        cityName: cityName,
        cityCode: cityCode
      });
      var district = city[c]["area"];
      var districtName = [];
      var districtCode = [];
      var dIndex = 0;
      if(district!=null&&district.length>0){
        for (var item in district) {
          var name = district[item]["name"];
          var code = district[item]["id"];
          districtName.push(name)
          districtCode.push(code)
        }
        that.setData({
          districtName: districtName,
          districtCode: districtCode
        })
      }
      else{
        that.setData({
        districtName: [],
          districtCode: [],
     //   streetName: [],
    //    streetCode: []
        })
      }
      
      //var hasdata = this.ArrayContains(cityRegionIds, c);
      //if (!hasdata) {
      //  that.getRegions(c, 3, a, s);
     // }
     // else {
     //   that.setAreaDataShow(null, c, a, s);
      //}
  },
  getItemIndex:function(arr,item){
    var i = arr.length;
    var index=-1;
    while (i--) {
      if (arr[i] === item) {
       return i;
      }
    }
    return index;
  },
  setAreaDataShow: function (data, parentRegionId, a, s){
    const that = this;
    if(data!=null){
      currAreaData=data;
      cityRegionIds.push(parentRegionId);
      areadata.push(data);
    }
    else{
      var itemIndex=that.getItemIndex(cityRegionIds,parentRegionId);
      if(itemIndex>=0){
        currAreaData=areadata[itemIndex];
      }
      else{
        currAreaData=[];
      }
    }
    var districtName = [];
    var districtCode = [];
    if (currAreaData && currAreaData.length > 0) {
      for (var item in currAreaData) {
        var name = item.id;
        var code = item.name;
        districtName.push(name)
        districtCode.push(code)
      }
      that.setData({
        districtName: districtName,
        districtCode: districtCode
      })
    } else {
      that.setData({
        districtName: [],
        districtCode: [],
        //   streetName: [],
        //    streetCode: []
      })
    }

      var hasdata = this.ArrayContains(areaRegionIds, a);
      if (!hasdata) {
        that.getRegions(c, 4,a,s);
      }
      else {
        that.setStreetData(null, c,a,s);
      }
  },
  setStreetData(data,parentRegionId,a,s){
    const that = this;
    if(data!=null){
      areaRegionIds.push(regionId);
      streetdata.push(data);
      currStreetData=data;
    }
    else{
      var itemIndex = that.getItemIndex(areaRegionIds, parentRegionId);
      if (itemIndex >= 0) {
        currStreetData = streetdata[itemIndex];
      }
      else {
        currStreetData = [];
      }
    }
  },
  setAreaData: function (p, c, d, s) {
    const that = this;
    var p = p || 0 // provinceSelIndex
    var c = c || 0 // citySelIndex
    var d = d || 0 // districtSelIndex
    var s = d || 0 // streetSelIndex
    console.log('procitydata'+procitydata);
    // 设置省的数据
    if(procitydata==undefined||procitydata==null){
      wx.request({
        url: app.getUrl("Region/GetAll"),
        async: false,
        success: function (result) {
          if(result.data.Status=="OK"){
            console.log("aaa"+"-"+p + "-" + c + "-" + d + "-" + s);
            that.setProvinceCityData(result.data.province,p,c,d,s);
          }
        },error:function(e){
          console.log(e);
        }
      });
    }
    else{
      that.setProvinceCityData(null,p,c,d,s);
    }
   
    

    
    // 设置街道的数据
   // var street = area['province'][p]["city"][c]["county"][d]["street"];
   // var streetName = [];
   // var streetCode = [];
  // if (district && district.length > 0) {
  //    if (street && street.length > 0) {
  //      for (var item in street) {
  //        var name = street[item]["name"];
  //        var code = street[item]["id"];
   //       streetName.push(name)
   //       streetCode.push(code)
  //      }
   //     this.setData({
  //        streetName: streetName,
    //      streetCode: streetCode
      //  })
      //} else {
       // this.setData({
        //  streetName: [],
         // streetCode: []
        //})
      //}
    }// else {
      //this.setData({
       // districtName: [],
       // districtCode: []
      //})
    //}
  //},
})