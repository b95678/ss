using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Management.Models
{
    public class MailNotifyInfo
    {
        /// <summary>

        /// 获取或设置稿件的标题。

        /// </summary>

        public string Title
        {

            get;
            set;

        }

        /// <summary>

        /// 获取或设置稿件的作者名称。

        /// </summary>

        public string Author
        {

            get;
            set;

        }

        /// <summary>

        /// 获取或设置作者的电子邮件地址。

        /// </summary>

        public string EmailAddress
        {

            get;
            set;

        }

        /// <summary>

        /// 获取或设置稿件的内容。

        /// </summary>

        public string Content
        {

            get;
            set;

        }
    }

}