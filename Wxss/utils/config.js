function json2Form(json) {
  var str = [];
  for (var p in json) {
    str.push(encodeURIComponent(p) + "=" + encodeURIComponent(json[p]));
  }
  return str.join("&");
}
module.exports = {
  json2Form: json2Form,
}

function httpGet(url,data,callBack) {
    wx.request({
        url: url,
        data: data,
        method: "GET",
        success: function (res) {
            callBack(res.data);
        },
        fail: function (res) {
            wx.showToast({
                title: res.errMsg,
                icon: 'fail',
                duration: 2000
            })
        },
        error:function(res)
        {
console.log(res);
        },
        complete: function (res) {
            // complete
        }
    })
}

function httpPost(url,data,callBack) {
    wx.request({
        url: url,
        header: {
          "Content-Type": "application/x-www-form-urlencoded"
        },
        data: json2Form(data),
        method: "POST",       
        success: function (res) {
            callBack(res.data);
        },
        fail: function (res) {
            wx.showToast({
                title: res.errMsg,
                icon: 'fail',
                duration: 2000
            })
        },
        complete: function (res) {
            // complete
        }
    })
}

function ArrayContains(arr, obj) {
  var i = arr.length;
  while (i--) {
    if (arr[i] === obj) {
      return true;
    }
  }
  return false;
}

module.exports = {
    httpGet:httpGet,
    httpPost:httpPost
}
