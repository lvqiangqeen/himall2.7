using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.ComponentModel;
using System.Configuration;
using System.Net.Configuration;

namespace Himall.CoreTest
{
    public class SendMail
    {
        SmtpClient SmtpClient = null;   //设置SMTP协议
        MailAddress MailAddress_from = null; //设置发信人地址  当然还需要密码
        MailAddress MailAddress_to = null;  //设置收信人地址  不需要密码
        MailMessage MailMessage_Mai = null;

        public SendMail(string serverHost, int Port, string mailFrom, string Password, string DisPlayText)
        {
            setSmtpClient(serverHost, Port);
            setAddressform(mailFrom, Password, DisPlayText);
        }

        public SendMail()
        {

            setSmtpClient("smtp.qq.com", 25);
            setAddressform("1412053424@qq.com", "hishopefx2013", "zhiyong");
           // Core.Log.Debug(config.SmtpServer + ":" + config.SmtpPort + ": " + config.SendAddress + ":" + config.DisplayName);
        }

        /// <summary>
        /// 设置smtp服务器信息
        /// </summary>
        /// <param name="ServerName">SMTP服务名</param>
        /// <param name="Port">端口号</param>
        private void setSmtpClient(string ServerHost, int Port)
        {
            SmtpClient = new SmtpClient();
            SmtpClient.Host = ServerHost;//指定SMTP服务名  例如QQ邮箱为 smtp.qq.com 新浪cn邮箱为 smtp.sina.cn等
            SmtpClient.Port = Port; //指定端口号
            SmtpClient.Timeout = 0;  //超时时间
        }

        /// <summary>
        /// 发件人信息设置
        /// </summary>
        /// <param name="MailAddress">发件邮箱地址</param>
        /// <param name="MailPwd">邮箱密码</param>
        private void setAddressform(string MailAddress, string MailPwd, string DisplayName)
        {
            //创建服务器认证
            NetworkCredential NetworkCredential_my = new NetworkCredential(MailAddress, MailPwd);
            //实例化发件人地址
            MailAddress_from = new System.Net.Mail.MailAddress(MailAddress, DisplayName);
            //指定发件人信息  包括邮箱地址和邮箱密码
            SmtpClient.Credentials = NetworkCredential_my;
        }



        public void ASynSendMail(string title, string[] mailto, string content)
        {
            Core.Log.Debug(title+string.Join(",",mailto)+content);
            MailMessage_Mai = new MailMessage();
            foreach (var s in mailto)
            {
                MailAddress_to = new MailAddress(s);
                //设置收件人 
                MailMessage_Mai.To.Add(MailAddress_to);
            }
            //发件人邮箱
            MailMessage_Mai.From = MailAddress_from;
            //邮件主题
            MailMessage_Mai.Subject = title;
            MailMessage_Mai.IsBodyHtml = true; ;
            MailMessage_Mai.SubjectEncoding = System.Text.Encoding.UTF8;
            //邮件正文
            MailMessage_Mai.Body = content;
            MailMessage_Mai.BodyEncoding = System.Text.Encoding.UTF8;
            //清空历史附件  以防附件重复发送
            MailMessage_Mai.Attachments.Clear();
            //添加附件
            // mm.Attachments.Add( new Attachment( @"d:a.doc", System.Net.Mime.MediaTypeNames.Application.Rtf ) 
            //注册邮件发送完毕后的处理事件
            SmtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            //开始发送邮件
            SmtpClient.SendAsync(MailMessage_Mai, "");
           // SmtpClient.Send(MailMessage_Mai);
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            Core.Log.Debug(e.Error);
        }
    }
}
