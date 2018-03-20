using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.IServices;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class MessageController : BaseAdminController
    {
        ISiteSettingService _iSiteSettingService;
        private IWXMsgTemplateService _iWXMsgTemplateService;
        private string wxtmppluginsid = "Himall.Plugin.Message.WXTMMSG";
        public MessageController(ISiteSettingService iSiteSettingService, IWXMsgTemplateService iWXMsgTemplateService)
        {
            _iSiteSettingService = iSiteSettingService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }
        // GET: Admin/Message
        public ActionResult Management()
        {
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item =>
            {
                dynamic model = new ExpandoObject();
                model.name = item.PluginInfo.DisplayName;
                model.pluginId = item.PluginInfo.PluginId;
                model.enable = item.PluginInfo.Enable;
                model.status = item.Biz.GetAllStatus();
                return model;
            });
            #region 微信模板消息
            List<WeiXinMsgTemplateInfo> wxtempllist = new List<WeiXinMsgTemplateInfo>();
            wxtempllist = _iWXMsgTemplateService.GetWeiXinMsgTemplateList();
            var statelist = new Dictionary<MessageTypeEnum, StatusEnum>();
            foreach (var item in Enum.GetValues(typeof(Himall.Core.Plugins.Message.MessageTypeEnum)))
            {
                var _tmpv = (int)item;
                var _tmpenum = (Himall.Core.Plugins.Message.MessageTypeEnum)item;
                var _tmpdata = wxtempllist.FirstOrDefault(d => d.MessageType == _tmpv);
                if (_tmpdata != null)
                {
                    statelist.Add(_tmpenum, (_tmpdata.IsOpen ? StatusEnum.Open : StatusEnum.Close));
                }
                else
                {
                    statelist.Add(_tmpenum, StatusEnum.Disable);
                }
            }
            dynamic _wxdata = new ExpandoObject();
            _wxdata.name = "微信模板消息";
            _wxdata.pluginId = wxtmppluginsid;
            _wxdata.enable = true;
            _wxdata.status = statelist;
            List<dynamic> data2 = new List<dynamic>();
            data2.Add(_wxdata);
            data = data.Concat(data2);
            #endregion

            return View(data);
        }

        public ActionResult Edit(string pluginId)
        {
            if (pluginId == wxtmppluginsid)
            {
                return RedirectToAction("editwxtm");
            }
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item =>
            {
                dynamic model = new ExpandoObject();
                model.name = item.PluginInfo.DisplayName;
                model.pluginId = item.PluginInfo.PluginId;
                model.enable = item.PluginInfo.Enable;
                model.status = item.Biz.GetAllStatus();
                return model;
            }
                );

            ViewBag.messagePlugins = data;
            ViewBag.Id = pluginId;
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.Name = messagePlugin.PluginInfo.DisplayName;
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            var formData = messagePlugin.Biz.GetFormData();
            var sms = PluginsManagement.GetPlugins<ISMSPlugin>().FirstOrDefault();
            ViewBag.ShowSMS = false;
            ViewBag.ShowBuy = false;
            if (sms != null && pluginId == sms.PluginInfo.PluginId)
            {
                ViewBag.ShowSMS = true;
                ViewBag.LoginLink = sms.Biz.GetLoginLink();
                ViewBag.BuyLink = sms.Biz.GetBuyLink();
                if (sms.Biz.IsSettingsValid)
                {
                    ViewBag.Amount = sms.Biz.GetSMSAmount();
                    ViewBag.ShowBuy = true;
                }
            }
            return View(formData);
        }

        #region 微信模板消息
        public ActionResult editwxtm()
        {
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item =>
            {
                dynamic model = new ExpandoObject();
                model.name = item.PluginInfo.DisplayName;
                model.pluginId = item.PluginInfo.PluginId;
                model.enable = item.PluginInfo.Enable;
                model.status = item.Biz.GetAllStatus();
                return model;
            }
                );

            ViewBag.messagePlugins = data;

            List<WeiXinMsgTemplateInfo> wxtempllist = new List<WeiXinMsgTemplateInfo>();
            wxtempllist = _iWXMsgTemplateService.GetWeiXinMsgTemplateList();
            return View(wxtempllist);
        }
        /// <summary>
        /// 重置行业
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetWXIndustry()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSiteSetting.WeixinAppId) && !string.IsNullOrWhiteSpace(CurrentSiteSetting.WeixinAppSecret))
            {
                var accessToken = AccessTokenContainer.TryGetToken(CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);

                var rdata = ApiHandlerWapper.TryCommonApi(actoken =>
                {
                    const string urlFormat = "https://api.weixin.qq.com/cgi-bin/template/api_set_industry?access_token={0}";
                    var msgData = new
                    {
                        industry_id1 = "1",
                        industry_id2 = "4"
                    };
                    return CommonJsonSend.Send<Senparc.Weixin.Entities.WxJsonResult>(accessToken, urlFormat, msgData, timeOut: 10000);

                }, accessToken);
                if (rdata.errcode == Senparc.Weixin.ReturnCode.请求成功)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, msg = rdata.errcode.ToString() + ":" + rdata.errmsg });
                }
            }
            else
            {
                return Json(new { success = false, msg = "未配置微信公众信息！" });
            }
        }
        /// <summary>
        /// 重置模板
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetWXTmplate(Himall.Core.Plugins.Message.MessageTypeEnum? type = null)
        {
            _iWXMsgTemplateService.AddMessageTemplate(type);
            return Json(new { success = true });
        }
        #endregion

        [HttpPost]
        [UnAuthorize]
        [ValidateInput(false)]
        public JsonResult Save(string pluginId, string values)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(values);
            messagePlugin.Biz.SetFormValues(items);
            return Json(new { success = true });
        }

        [HttpPost]
        [UnAuthorize]
        [ValidateInput(false)]
        public JsonResult Send(string pluginId, string destination)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);

            if (string.IsNullOrEmpty(destination))
            {
                return Json(new Result { success = false, msg = "你填写的" + messagePlugin.Biz.ShortName + "不能为空！" });
            }

            if (!messagePlugin.Biz.CheckDestination(destination))
            {
                return Json(new Result { success = false, msg = "你填写的" + messagePlugin.Biz.ShortName + "不正确" });
            }
            var siteName = _iSiteSettingService.GetSiteSettings().SiteName;
            var result = messagePlugin.Biz.SendTestMessage(destination, "该条信息，请勿回复!【" + siteName + "】", "这是一封测试邮件");
            if (result == "发送成功")
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new Result { success = false, msg = result });
            }
        }

        [HttpPost]
        [UnAuthorize]
        [ValidateInput(false)]
        public JsonResult Enable(string pluginId, MessageTypeEnum messageType, bool enable)
        {
            if (pluginId == wxtmppluginsid)
            {
                _iWXMsgTemplateService.UpdateWeiXinMsgOpenState(messageType, enable);
                return Json(new { success = true });
            }
            else
            {
                var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);

                if (enable)
                    messagePlugin.Biz.Enable(messageType);
                else
                    messagePlugin.Biz.Disable(messageType);
                return Json(new { success = true });
            }
        }
    }
}