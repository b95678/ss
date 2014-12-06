using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Text;

namespace Management.Models
{
    public class Class1
    {
        public static void Sender()
        {
            try
            {

                SmtpClient client = new SmtpClient("smtp.qq.com");
                client.Credentials = new NetworkCredential("495807556@qq.com", "81514936luster");
                client.Port = 465;
                MailMessage mm = new MailMessage();
                MailMessage message = new MailMessage(new MailAddress("495807556@qq.com"), new MailAddress("250417844@qq.com"));
                message.Body = "Hello Word!";//邮件内容
                message.Subject = "this is a test";//邮件主题

                mm.From = new MailAddress("495807556@qq.com");
                mm.To.Add(new MailAddress("250417844@qq.com"));
                mm.Subject = "Hello~!";
                mm.Body = "hahahahaa ";
                mm.IsBodyHtml = false;
                mm.Priority = MailPriority.High;
                client.Send(message);

 //MailMessage mail = new MailMessage();
 
                //mail.BodyFormat = MailFormat.Html;
                //mail.To = "250417844@qq.com";
                //mail.Subject = "Hello~!"; ;

                //mail.Body = @"Using this new feature, you can send an e-mail message from an application very easily.";
                //mail.BodyEncoding = Encoding.GetEncoding("GB2312");
                //mail.From = "495807556@qq.com";
                //mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); 
                //mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "495807556@qq.com");
                //mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "81514936luster"); 
                //SmtpMail.SmtpServer = "smtp.qq.com";//如：smtp.126.com
                //SmtpMail.Send(mail);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="strTo">收信对象邮箱</param>
        /// <param name="strSubject">邮件主题</param>
        /// <param name="strBody">邮件内容</param>
        public static void SendEmail(string strTo, string strSubject, string strBody)
        {
            string strSmtpServer = "smtp.qq.com";  //163邮件服务器
            string strFrom = "495807556@qq.com"; //用户邮箱
            string strFromPass = "81514936luster";//用户密码



            //string strSmtpServer = "smtp.qq.com"; //qq邮件服务器
            //string strSmtpServer = "202.108.3.190"; //新浪邮件服务器


            SmtpClient client = new SmtpClient(strSmtpServer);//创建邮箱服务器对象           

            client.UseDefaultCredentials = false;//获取或设置是否使用默认凭据访问 Web 代理服务器
            client.Credentials = new System.Net.NetworkCredential(strFrom, strFromPass);//创建用户对象
            client.DeliveryMethod = SmtpDeliveryMethod.Network;//投递方式
            client.Port = 465;
            client.EnableSsl = true;

            MailMessage message = new MailMessage();    //创建邮件对象
            message.From = new MailAddress(strFrom);    //发信人地址
            message.To.Add(strTo);                      //添加收信人地址
            message.Subject = strSubject;               //邮件主题
            message.Body = strBody;                     //邮件内容
            message.BodyEncoding = System.Text.Encoding.UTF8;//获取或设置用于邮件正文的编码

            message.IsBodyHtml = true;//取得或设定值，指出电子邮件的主体是否为 HTML
            if (!string.IsNullOrEmpty(strBody))//判断邮件内容是否为空
            {
                try
                {
                    client.Send(message);//发送

                    Console.Write("发送成功！");
                }
                catch (Exception ex)
                {
                    Console.Write("发送失败：" + ex.Message);
                }
            }
            else
            {
                Console.Write("不能发送空信息！");
            }       
        }
    }
}