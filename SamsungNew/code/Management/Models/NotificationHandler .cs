using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace Management.Models
{

    ///然后是全局对象类的定义，我使用了单件模式来实现其全局性。

    /// <summary>

    /// 处理邮件发送功能的类。

    /// </summary>
    public class NotificationHandler
    {

        /// <summary>

        /// 该类的静态实例。

        /// </summary>

        private static readonly NotificationHandler g_instance = new NotificationHandler();

        /// <summary>

        /// 获取该类的唯一实例。

        /// </summary>

        public static NotificationHandler Instance
        {

            get
            {

                return g_instance;

            }

        }

        /// <summary>

        /// 默认构造方法。

        /// </summary>

        private NotificationHandler()
        {

            this.m_lockObject = new object();

            this.m_mailNotifyInfos = new LinkedList<MailNotifyInfo>();

            this.m_threadEvent = new ManualResetEvent(false);

            this.m_workThread = new Thread(this.ThreadStart);

            this.m_workThread.Start();

        }

        private readonly LinkedList<MailNotifyInfo> m_mailNotifyInfos;

        private readonly Thread m_workThread;

        private readonly ManualResetEvent m_threadEvent;

        private readonly Object m_lockObject;

        /// <summary>

        /// 添加待发送邮件的相关信息。

        /// </summary>

        public void AppendNotification(MailNotifyInfo mailNotifyInfo)
        {

            lock (this.m_lockObject)
            {

                this.m_mailNotifyInfos.AddLast(mailNotifyInfo);

                if (this.m_mailNotifyInfos.Count != 0)
                {

                    this.m_threadEvent.Set();

                }

            }

        }

        /// <summary>

        /// 发送邮件线程的执行方法。

        /// </summary>

        private void ThreadStart()
        {

            while (true)
            {

                this.m_threadEvent.WaitOne();

                MailNotifyInfo mailNotifyInfo = this.m_mailNotifyInfos.First.Value;

                string host = ConfigurationManager.AppSettings["SmtpServer"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                string user = ConfigurationManager.AppSettings["SmtpUser"];
                string pwd=ConfigurationManager.AppSettings["SmtpPass"];

                EmailNotificationService mailNotificationService = new EmailNotificationService(host, true, port, user, pwd);

                mailNotificationService.SendTo(mailNotifyInfo.Author,

                mailNotifyInfo.EmailAddress,

                mailNotifyInfo.Title,

                mailNotifyInfo.Content);

                lock (this.m_lockObject)
                {

                    this.m_mailNotifyInfos.Remove(mailNotifyInfo);

                    if (this.m_mailNotifyInfos.Count == 0)
                    {

                        this.m_threadEvent.Reset();

                    }

                }

            }
        }
    }
}