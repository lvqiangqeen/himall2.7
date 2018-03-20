using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Express;
using Himall.IServices;
using Himall.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Himall.Service
{
    public class ExpressService : ServiceBase, IExpressService
    {

        const string RELATEIVE_PATH = "/Plugins/Express/";

        public IEnumerable<IExpress> GetAllExpress()
        {

            IEnumerable<PluginInfo> pluginInfos = Core.PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.Express);
            IExpress[] plugins = new IExpress[pluginInfos.Count()];
            int i = 0;
            string name;
            foreach (var pluginInfo in pluginInfos)
            {
                name = pluginInfo.ClassFullName.Split(',')[1];
                IExpress plugin = Core.Instance.Get<IExpress>(pluginInfo.ClassFullName);
                plugin.Logo = RELATEIVE_PATH + name + "/" + plugin.Logo;
                plugin.BackGroundImage = RELATEIVE_PATH + name + "/" + plugin.BackGroundImage;
                plugins[i++] = plugin;
            }
            return plugins;
        }


        public IExpress GetExpress(string name)
        {
            var templates = GetAllExpress();
            return templates.FirstOrDefault(item => item.Name == name);
        }

        public void UpdatePrintElement(string name, IEnumerable<ExpressPrintElement> elements)
        {
            var express = GetIExpressByName(name);
            express.UpdatePrintElement(elements);
        }


        public IEnumerable<IExpress> GetRecentExpress(long shopId, int takeNumber)
        {
            var expressNames = Context.OrderInfo.Where(item => item.ShopId == shopId && !string.IsNullOrEmpty(item.ExpressCompanyName)).OrderByDescending(item => item.Id).Select(item => item.ExpressCompanyName).Take(takeNumber);
            var allExpress = GetAllExpress();
            var selectedExpresses = allExpress.Where(item => expressNames.Contains(item.Name)).ToList();
            if (selectedExpresses.Count < takeNumber)
            {
                Random rand = new Random();
                selectedExpresses.AddRange(allExpress.Where(item => !expressNames.Contains(item.Name)).OrderBy(item => rand.Next()).Take(takeNumber - selectedExpresses.Count));
            }
            return selectedExpresses;
        }


        IExpress GetIExpressByName(string name)
        {
            return Core.PluginsManagement.GetInstalledPlugins<IExpress>(PluginType.Express).FirstOrDefault(item => item.Name == name);
        }

        public IDictionary<int, string> GetPrintElementIndexAndOrderValue(long shopId, long orderId, IEnumerable<int> printElementIndexes)
        {
            var order = Context.OrderInfo.FirstOrDefault(item => item.Id == orderId && item.ShopId == shopId);
            if (order == null)
                throw new HimallException(string.Format("未在本店铺中找到id为{0}的订单", orderId));

            var shop = Context.ShopInfo.FirstOrDefault(item => item.Id == shopId);

            return GetPrintElementIndexAndOrderValue(shop, order, printElementIndexes);
        }

        IDictionary<int, string> GetPrintElementIndexAndOrderValue(ShopInfo shop, OrderInfo order, IEnumerable<int> printElementIndexes)
        {
            if (order == null)
                throw new NullReferenceException("订单为空");

            var dic = new Dictionary<int, string>();


            RegionService regionService = new RegionService();

            string[] regionNames = order.RegionFullName.Split(' ');

            string sellerRegionFullName = string.Empty;
            string[] sellerRegionNames = new string[] { };

            if (shop.SenderRegionId.HasValue)
            {
                sellerRegionFullName = regionService.GetFullName(shop.SenderRegionId.Value);
                sellerRegionNames = sellerRegionFullName.Split(' ');
            }

            string value;
            string fullRegionName = string.Empty;


            foreach (var index in printElementIndexes.ToList())
            {
                value = string.Empty;

                #region 获取对应值
                switch (index)
                {
                    case 1://"收货人-姓名"
                        value = order.ShipTo;
                        break;
                    case 2://"收货人-地址"
                        value = order.Address;
                        break;
                    case 3://"收货人-电话"
                        value = order.CellPhone;
                        break;
                    case 4://"收货人-邮编"
                        value = "";
                        break;
                    case 5://"收货人-地区1级"
                        value = regionNames[0];
                        break;
                    case 6://"收货人-地区2级"
                        value = regionNames.Length > 1 ? regionNames[1] : "";
                        break;
                    case 7://"收货人-地区3级"
                        value = regionNames.Length > 2 ? regionNames[2] : "";
                        break;
                    case 8://"发货人-姓名"
                        value = shop.SenderName;
                        break;
                    case 9://"发货人-地区1级"
                        value = sellerRegionNames.Length > 0 ? sellerRegionNames[0] : "";
                        break;
                    case 10:// "发货人-地区2级"
                        value = sellerRegionNames.Length > 1 ? sellerRegionNames[1] : "";
                        break;
                    case 11://"发货人-地区3级"
                        value = sellerRegionNames.Length > 2 ? sellerRegionNames[2] : "";
                        break;
                    case 12://"发货人-地址"
                        value = shop.SenderAddress;
                        break;
                    case 13://"发货人-邮编"
                        value = "";
                        break;
                    case 14://"发货人-电话"
                        value = shop.SenderPhone;
                        break;
                    case 15://"订单-订单号"
                        value = order.Id.ToString();
                        break;
                    case 16://"订单-总金额"
                        value = order.OrderTotalAmount.ToString("F2");
                        break;
                    case 17://"订单-物品总重量"
                        value = string.Empty;
                        break;
                    case 18://"订单-备注"
                        value = string.IsNullOrWhiteSpace(order.UserRemark) ? "" : order.UserRemark.ToString();
                        break;
                    case 19://"订单-详情"
                        value = string.Empty;
                        break;
                    case 21://"网店名称"
                        value = shop.ShopName;
                        break;
                    case 22://"对号-√"
                        value = "√";
                        break;
                    default:
                        value = string.Empty;
                        break;
                }
                #endregion

                dic.Add(index, value);
            }
            return dic;
        }



        public ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            var settting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var expressData = new ExpressData();//创建对象
            //使用快递100查询快递数据
            if (settting.KuaidiType.Equals(0))
            {
                string kuaidi100Code = GetExpress(expressCompanyName).Kuaidi100Code;
                expressData = GetExpressDataByKey(kuaidi100Code, shipOrderNumber);
            }
            else//快递鸟
            {
                string shipperCode = GetExpress(expressCompanyName).KuaidiNiaoCode;
                expressData = GetExpressDataByKuai(shipperCode, shipOrderNumber);
            }
            return expressData;
        }

        /// <summary>
        /// 快递100物流查看
        /// </summary>
        /// <param name="kuaidi100Code"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        private ExpressData GetExpressDataByKey(string kuaidi100Code, string shipOrderNumber)
        {
            var settting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var key = settting.Kuaidi100Key;
            var expressData = new ExpressData();//创建对象
            expressData.Success = false;
            expressData.Message = "暂无物流信息";
            if (!string.IsNullOrEmpty(key))
            {
                var model = GetKuaidi100ExpressData(kuaidi100Code, shipOrderNumber);
                if (model != null)
                {
                    var content = model.DataContent;
                    var obj = new
                    {
                        message = string.Empty,
                        lastResult = new
                        {
                            data = new[] {
                             new{
                                context=string.Empty,
                                time=string.Empty,
                                ftime=string.Empty
                            }
                        }
                        }
                    };

                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content, obj);
                    expressData.Success = true;
                    expressData.Message = m.message;
                    var dataItems = new List<ExpressDataItem>();
                    foreach (var t in m.lastResult.data)
                    {
                        dataItems.Add(new ExpressDataItem()
                        {
                            Time = DateTime.ParseExact(t.ftime, "yyyy-MM-dd HH:mm:ss", null),
                            Content = t.context.ToString()
                        });
                    }
                    expressData.ExpressDataItems = dataItems;
                }
            }
            return expressData;
        }

        /// <summary>
        /// 百度物流查看
        /// </summary>
        /// <param name="kuaidi100Code"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        private ExpressData GetExpressDataFree(string kuaidi100Code, string shipOrderNumber)
        {
            var expressData = new ExpressData();//创建对象
            string url = string.Format("https://sp0.baidu.com/9_Q4sjW91Qh3otqbppnN2DJv/pae/channel/data/asyncqury?appid=4001&com={0}&nu={1}", kuaidi100Code, shipOrderNumber);//快递查询地址
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 8000;
            request.Host = "sp0.baidu.com";
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36";
            CookieContainer cookie = new CookieContainer();
            Cookie ck = new Cookie("BAIDUID", "65C7509B002B15BE3E4EEE6B5366AFEA:FG=1", "/", ".baidu.com");
            ck.Expires = DateTime.Now.AddYears(1);
            cookie.Add(ck);
            request.CookieContainer = cookie;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));
                    StringBuilder content = new StringBuilder(streamReader.ReadToEnd());// 读取流字符串内容
                    content.Replace("&amp;", "").Replace("&nbsp;", "").Replace("&", "");//去除不需要的字符
                    // var jsonData = JObject.Parse(content.ToString());
                    var test = new
                    {
                        msg = "",
                        status = "",
                        data = new
                        {
                            info = new
                            {
                                status = "",
                                state = "",
                                context = new[]{ 
                               new{time="",desc=""}
                            }
                            }
                        }
                    };
                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content.ToString(), test);
                    expressData.Success = true;
                    expressData.Message = m.msg;
                    var dataItems = new List<ExpressDataItem>();
                    foreach (var t in m.data.info.context)
                    {
                        dataItems.Add(new ExpressDataItem()
                        {
                            Time = GetTime(t.time),
                            // Time = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm:ss", null),
                            Content = t.desc.ToString()
                        });
                    }
                    expressData.ExpressDataItems = dataItems;
                    return expressData;
                }
                else
                {
                    expressData.Message = "网络错误";
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("快递查询错误:{{kuaidi100Code:{0},shipOrderNumber:{1}}}", kuaidi100Code, shipOrderNumber), ex);
                expressData.Message = "未知错误";
            }
            return expressData;
        }

        /// <summary>
        /// 快递鸟物流查看
        /// </summary>
        /// <param name="shipperCode">物流公司编码</param>
        /// <param name="logisticsCode">物流号</param>
        /// <returns></returns>
        private ExpressData GetExpressDataByKuai(string shipperCode, string logisticsCode)
        {
            var settting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();

            //签名
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();
            data.Add("app_key", settting.KuaidiApp_key);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add("logisticsCode", logisticsCode);
            data.Add("shipperCode", shipperCode);
            data.Add("timestamp", timestamp);

            string si = string.Format("app_key{0}logisticsCode{1}shipperCode{2}timestamp{3}{4}", settting.KuaidiApp_key, logisticsCode, shipperCode, timestamp, settting.KuaidiAppSecret);
            string sign = FormsAuthentication.HashPasswordForStoringInConfigFile(si, "MD5");


            var expressData = new ExpressData();//创建对象
            string url = string.Format("http://wuliu.kuaidiantong.cn/api/logistics?app_key={0}&timestamp={1}&shipperCode={2}&logisticsCode={3}&sign={4}", settting.KuaidiApp_key, timestamp, shipperCode, logisticsCode, sign);//快递查询地址
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 8000;
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36";
            CookieContainer cookie = new CookieContainer();
            request.CookieContainer = cookie;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));
                    StringBuilder content = new StringBuilder(streamReader.ReadToEnd());// 读取流字符串内容
                    content.Replace("&amp;", "").Replace("&nbsp;", "").Replace("&", "");//去除不需要的字符

                    var test = new
                    {
                        shipperCode = "",
                        logisticsCode = "",
                        success = true,
                        state = "",
                        traces = new[]
                        {
                            new{
                                acceptTime="",
                                acceptStation="",
                                remark=""
                            }
                        }
                    };
                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content.ToString(), test);

                    var dataItems = new List<ExpressDataItem>();
                    if (m.success == true)
                    {
                        expressData.Success = true;
                        expressData.Message = "成功";
                        foreach (var t in m.traces)
                        {
                            dataItems.Add(new ExpressDataItem()
                            {
                                Time = DateTime.Parse(t.acceptTime),
                                Content = t.acceptStation
                            });
                        }
                    }
                    else
                    {
                        expressData.Success = false;
                        expressData.Message = "暂无物流信息";
                    }
                    expressData.ExpressDataItems = dataItems;
                    return expressData;
                }
                else
                {
                  //  expressData.Message = "网络错误";
                    expressData.Message = "网络错误";
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("快递查询错误:{{kuaidi100Code:{0},shipOrderNumber:{1}}}", shipperCode, logisticsCode), ex);
              //  expressData.Message = "未知错误";
                expressData.Message = "暂无物流信息";
            }
            return expressData;
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        private DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public void SubscribeExpress100(string expressCompanyName, string number, string kuaidi100Key, string city, string redirectUrl)
        { //使用快递100查询快递数据
            string kuaidi100Code = GetExpress(expressCompanyName).Kuaidi100Code;//根据快递公司名称获取对应的快递100编码
            if (string.IsNullOrEmpty(kuaidi100Key) || string.IsNullOrEmpty(expressCompanyName) || string.IsNullOrEmpty(number))
            {
                Core.Log.Info("没有设置快递100Key");
                return;
            }
            string url = "http://www.kuaidi100.com/poll";
            System.Net.WebClient WebClientObj = new System.Net.WebClient();
            NameValueCollection PostVars = new NameValueCollection();
            PostVars.Add("schema", "json");
            var model = new
            {
                company = kuaidi100Code,
                number = number,
                key = kuaidi100Key,
                to = city,
                parameters = new { callbackurl = redirectUrl }
            };
            var param = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            PostVars.Add("param", param);
            try
            {
                byte[] byRemoteInfo = WebClientObj.UploadValues(url, "POST", PostVars);
                string output = System.Text.Encoding.UTF8.GetString(byRemoteInfo);
                //注意返回的信息，只有result=true的才是成功
                var result = new { result = false, resultcode = "", message = "" };
                var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(output, result);
                Core.Log.Info(output + "," + redirectUrl + "物流公司代码：" + kuaidi100Code + ",物流单号:" + number + ",物流到达城市:" + city); //日志记录看成功否
                if (!m.result)
                {
                    Core.Log.Error("物流通知订阅失败：" + result.message + ",物流公司代码：" + kuaidi100Code + ",物流单号:" + number + ",物流到达城市:" + city + ",快递Key:" + kuaidi100Key + "回调地址：" + redirectUrl);
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("物流通知订阅失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 保存快递100回调的物流信息
        /// </summary>
        /// <param name="model"></param>
        public void SaveExpressData(OrderExpressInfo model)
        {
            var m = Context.OrderExpressInfo.Where(a => a.CompanyCode == model.CompanyCode && a.ExpressNumber == model.ExpressNumber).FirstOrDefault();
            if (m != null)
            {
                m.DataContent = model.DataContent;
            }
            else
            {
                Context.OrderExpressInfo.Add(model);
            }
            Context.SaveChanges();
        }

        private OrderExpressInfo GetKuaidi100ExpressData(string companyCode, string number)
        {
            var m = Context.OrderExpressInfo.Where(a => a.CompanyCode == companyCode && a.ExpressNumber == number).FirstOrDefault();

            return m;
        }
    }
}
