using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Framework
{
    class JsonNetResult : JsonResult
    {
        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }
        public JsonNetResult()
        {
            SerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling= DateFormatHandling.MicrosoftDateFormat,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new HierpContractResolver()
            };
        }

        class HierpContractResolver : CamelCasePropertyNamesContractResolver
        {
            public HierpContractResolver()
            {
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, MemberSerialization.Fields);
                return properties;
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType =
                !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;
            if (Data != null)
            {
                JsonTextWriter writer = new JsonTextWriter(response.Output)
                {
                    Formatting = Formatting
                };
                JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, Data);
                writer.Flush();
            }
        }
    }
}
