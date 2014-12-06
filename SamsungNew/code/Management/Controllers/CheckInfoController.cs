using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Management.Controllers
{
    public class CheckInfoController : Controller
    {

        public ActionResult Uncheck()
        {
            return View();
        }

        public ActionResult Checked()
        {
            return View();
        }
    }
}
