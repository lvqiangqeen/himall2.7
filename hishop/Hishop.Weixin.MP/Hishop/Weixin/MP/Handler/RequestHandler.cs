namespace Hishop.Weixin.MP.Handler
{
    using Hishop.Weixin.MP;
    using Hishop.Weixin.MP.Request;
    using Hishop.Weixin.MP.Request.Event;
    using Hishop.Weixin.MP.Util;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Linq;

    public abstract class RequestHandler
    {
        public RequestHandler(Stream inputStream)
        {
            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                this.RequestDocument = XDocument.Load(reader);
                this.Init(this.RequestDocument);
            }
        }

        public RequestHandler(string xml)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                this.RequestDocument = XDocument.Load(reader);
                this.Init(this.RequestDocument);
            }
        }

        public abstract AbstractResponse DefaultResponse(AbstractRequest requestMessage);
        public void Execute()
        {
            if (this.RequestMessage != null)
            {
                switch (this.RequestMessage.MsgType)
                {
                    case RequestMsgType.Text:
                        this.ResponseMessage = this.OnTextRequest(this.RequestMessage as TextRequest);
                        return;

                    case RequestMsgType.Image:
                        this.ResponseMessage = this.OnImageRequest(this.RequestMessage as ImageRequest);
                        return;

                    case RequestMsgType.Voice:
                        this.ResponseMessage = this.OnVoiceRequest(this.RequestMessage as VoiceRequest);
                        return;

                    case RequestMsgType.Video:
                        this.ResponseMessage = this.OnVideoRequest(this.RequestMessage as VideoRequest);
                        return;

                    case RequestMsgType.Location:
                        this.ResponseMessage = this.OnLocationRequest(this.RequestMessage as LocationRequest);
                        return;

                    case RequestMsgType.Link:
                        this.ResponseMessage = this.OnLinkRequest(this.RequestMessage as LinkRequest);
                        return;

                    case RequestMsgType.Event:
                        this.ResponseMessage = this.OnEventRequest(this.RequestMessage as EventRequest);
                        return;
                }
                throw new WeixinException("未知的MsgType请求类型");
            }
        }

        private void Init(XDocument requestDocument)
        {
            this.RequestDocument = requestDocument;
            this.RequestMessage = RequestMessageFactory.GetRequestEntity(this.RequestDocument);
        }

        public virtual AbstractResponse OnEvent_ClickRequest(ClickEventRequest clickEventRequest)
        {
            return this.DefaultResponse(clickEventRequest);
        }

        public virtual AbstractResponse OnEvent_LocationRequest(LocationEventRequest locationEventRequest)
        {
            return this.DefaultResponse(locationEventRequest);
        }

        public virtual AbstractResponse OnEvent_ScanRequest(ScanEventRequest scanEventRequest)
        {
            return this.DefaultResponse(scanEventRequest);
        }

        public virtual AbstractResponse OnEvent_SubscribeRequest(SubscribeEventRequest subscribeEventRequest)
        {
            return this.DefaultResponse(subscribeEventRequest);
        }

        public virtual AbstractResponse OnEvent_UnSubscribeRequest(UnSubscribeEventRequest unSubscribeEventRequest)
        {
            return this.DefaultResponse(unSubscribeEventRequest);
        }

        public AbstractResponse OnEventRequest(EventRequest eventRequest)
        {
            AbstractResponse response = null;
            switch (eventRequest.Event)
            {
                case RequestEventType.Subscribe:
                    return this.OnEvent_SubscribeRequest(eventRequest as SubscribeEventRequest);

                case RequestEventType.UnSubscribe:
                    return this.OnEvent_UnSubscribeRequest(eventRequest as UnSubscribeEventRequest);

                case RequestEventType.Scan:
                    this.ResponseMessage = this.OnEvent_ScanRequest(eventRequest as ScanEventRequest);
                    return response;

                case RequestEventType.Location:
                    return this.OnEvent_LocationRequest(eventRequest as LocationEventRequest);

                case RequestEventType.Click:
                    return this.OnEvent_ClickRequest(eventRequest as ClickEventRequest);
            }
            throw new WeixinException("未知的Event下属请求信息");
        }

        public virtual AbstractResponse OnImageRequest(ImageRequest imageRequest)
        {
            return this.DefaultResponse(imageRequest);
        }

        public AbstractResponse OnLinkRequest(LinkRequest linkRequest)
        {
            return this.DefaultResponse(linkRequest);
        }

        public virtual AbstractResponse OnLocationRequest(LocationRequest locationRequest)
        {
            return this.DefaultResponse(locationRequest);
        }

        public virtual AbstractResponse OnTextRequest(TextRequest textRequest)
        {
            return this.DefaultResponse(textRequest);
        }

        public virtual AbstractResponse OnVideoRequest(VideoRequest videoRequest)
        {
            return this.DefaultResponse(videoRequest);
        }

        public virtual AbstractResponse OnVoiceRequest(VoiceRequest voiceRequest)
        {
            return this.DefaultResponse(voiceRequest);
        }

        public XDocument RequestDocument { get; set; }

        public AbstractRequest RequestMessage { get; set; }

        public string ResponseDocument
        {
            get
            {
                if (this.ResponseMessage == null)
                {
                    return string.Empty;
                }
                return EntityHelper.ConvertEntityToXml<AbstractResponse>(this.ResponseMessage).ToString();
            }
        }

        public AbstractResponse ResponseMessage { get; set; }
    }
}

