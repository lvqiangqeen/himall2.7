using Himall.IServices;
using Himall.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;


namespace Himall.API.Helper
{
    /// <summary>
    ///  门店相关操作
    /// </summary>
    public class ShopbranchHelper
    {
        /// <summary>
        /// 通过坐标获取地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static void GetAddressByLatLng(string latLng, ref string address, ref string province, ref string city, ref string district, ref string street)
        {
            string[] latlngarr = latLng.Split(',');
            string gaodeLngLat = latlngarr[1] + "," + latlngarr[0];
            string newLatLng = Math.Round(decimal.Parse(latlngarr[1]), 4) + "," + Math.Round(decimal.Parse(latlngarr[0]), 4);
            object objlatlng = Core.Cache.Get(CacheKeyCollection.LatlngCacheKey(newLatLng));
            if (objlatlng != null)
            {
                GaodeGetAddressByLatLngResult resultobj = (GaodeGetAddressByLatLngResult)objlatlng;
                if (resultobj.status == 1 && resultobj.info == "OK")
                {
                    province = resultobj.regeocode.addressComponent.province;
                    city = string.IsNullOrEmpty(resultobj.regeocode.addressComponent.city) ? resultobj.regeocode.addressComponent.province : resultobj.regeocode.addressComponent.city;
                    district = resultobj.regeocode.addressComponent.district;
                    street = resultobj.regeocode.addressComponent.township;
                    if (string.IsNullOrEmpty(resultobj.regeocode.addressComponent.building.name))
                    {
                        address = resultobj.regeocode.addressComponent.neighborhood.name;
                    }
                    else
                    {
                        address = resultobj.regeocode.addressComponent.building.name;
                    }
                    if (string.IsNullOrEmpty(address))
                    {
                        string preAddr = province + resultobj.regeocode.addressComponent.city + district + street;
                        address = resultobj.regeocode.formatted_address.Remove(0, preAddr.Length);
                    }
                }
            }
            else
            {
                string gaoDeAPIKey = "53e4f77f686e6a2b5bf53521e178c6e7";
                string url = "http://restapi.amap.com/v3/geocode/regeo?output=json&radius=3000&location=" + gaodeLngLat + "&key=" + gaoDeAPIKey;
                string result = GetResponseResult(url);

                GaodeGetAddressByLatLngResult resultobj = ParseFormJson<GaodeGetAddressByLatLngResult>(result);
                if (resultobj.status == 1 && resultobj.info == "OK")
                {
                    var cacheTimeout = DateTime.Now.AddDays(1);
                    Core.Cache.Insert(CacheKeyCollection.LatlngCacheKey(newLatLng), resultobj, cacheTimeout); //坐标地址信息缓存一天
                    province = resultobj.regeocode.addressComponent.province;
                    city = string.IsNullOrWhiteSpace(resultobj.regeocode.addressComponent.city) ? resultobj.regeocode.addressComponent.province : resultobj.regeocode.addressComponent.city;
                    district = resultobj.regeocode.addressComponent.district;
                    street = resultobj.regeocode.addressComponent.township;
                    if (string.IsNullOrWhiteSpace(resultobj.regeocode.addressComponent.building.name))
                    {
                        address = resultobj.regeocode.addressComponent.neighborhood.name;
                    }
                    else
                    {
                        address = resultobj.regeocode.addressComponent.building.name;
                    }
                    if (string.IsNullOrWhiteSpace(address))
                    {
                        string preAddr = province + resultobj.regeocode.addressComponent.city + district + street;
                        address = resultobj.regeocode.formatted_address.Remove(0, preAddr.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 把JSON字符串还原为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="szJson">JSON字符串</param>
        /// <returns>对象实体</returns>
        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }

        /// <summary>
        /// 获取Web请求返回的字符串数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetResponseResult(string url)
        {
            string result;
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)req.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                        {
                            result = readerOfStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("根据经纬度获取地理位置异常", ex);
                result = "";
            }
            return result;
        }
    }
}
