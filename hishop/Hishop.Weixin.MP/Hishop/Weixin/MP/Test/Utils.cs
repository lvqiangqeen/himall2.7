namespace Hishop.Weixin.MP.Test
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Request;
    using Hishop.Weixin.MP.Response;
    using Hishop.Weixin.MP.Util;
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    internal class Utils
    {
        private const string xml = "<xml><ToUserName><![CDATA[gh_ef4e2090afe3]]></ToUserName><FromUserName><![CDATA[opUMDj9jbOmTtbZuE2hM6wnv27B0]]></FromUserName><CreateTime>1385887183</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[s]]></Content><MsgId>5952340126940580233</MsgId></xml>";

        public AbstractRequest ConvertRequest<T>(Stream inputStream) where T: AbstractRequest
        {
            XmlDocument document = new XmlDocument();
            document.Load(inputStream);
            if (document.SelectSingleNode("xml/MsgType").InnerText.ToLower() != "text")
            {
                return default(T);
            }
            return new TextRequest { Content = document.SelectSingleNode("xml/Content").InnerText, FromUserName = document.SelectSingleNode("xml/FromUserName").InnerText, MsgId = Convert.ToInt32(document.SelectSingleNode("xml/FromUserName").InnerText) };
        }

        public string MethodName()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<xml><ToUserName><![CDATA[gh_ef4e2090afe3]]></ToUserName><FromUserName><![CDATA[opUMDj9jbOmTtbZuE2hM6wnv27B0]]></FromUserName><CreateTime>1385887183</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[s]]></Content><MsgId>5952340126940580233</MsgId></xml>");
            return document.SelectSingleNode("xml/ToUserName").InnerText;
        }

        public void Test02()
        {
            XDocument doc = XDocument.Parse("<xml><ToUserName><![CDATA[gh_ef4e2090afe3]]></ToUserName><FromUserName><![CDATA[opUMDj9jbOmTtbZuE2hM6wnv27B0]]></FromUserName><CreateTime>1385887183</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[s]]></Content><MsgId>5952340126940580233</MsgId></xml>");
            TextRequest entity = new TextRequest();
            EntityHelper.FillEntityWithXml<TextRequest>(entity, doc);
        }

        public string Test03()
        {
            TextResponse entity = new TextResponse {
                Content = "hah",
                FromUserName = "123",
                ToUserName = "456"
            };
            return EntityHelper.ConvertEntityToXml<TextResponse>(entity).ToString();
        }

        public void Test04()
        {
            A a = new A("<xml><ToUserName><![CDATA[gh_ef4e2090afe3]]></ToUserName><FromUserName><![CDATA[opUMDj9jbOmTtbZuE2hM6wnv27B0]]></FromUserName><CreateTime>1385887183</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[s]]></Content><MsgId>5952340126940580233</MsgId></xml>");
            object requestDocument = a.RequestDocument;
        }
    }
}

