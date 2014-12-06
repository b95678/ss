using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace Management.Models
{
    class LogHelper
    {
        public static void WriteLog(string txt)
        {
            ILog log = LogManager.GetLogger("log4netlogger");
            log.Error(txt);
        }
    }

}