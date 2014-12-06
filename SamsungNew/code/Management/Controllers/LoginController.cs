using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Net;

namespace Management.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        SS_HRM_DBEntities ss = new SS_HRM_DBEntities();

        #region 登录界面+Index()
        [HttpGet]
        //[Management.Models.RequireHttps]
        public ActionResult Index()
        {
            return View();
        } 
        #endregion

        #region 校验账号是否正确+ValidateUserName()
        [HttpPost]
        public JsonResult ValidateUserName()
        {
            bool success = true;
            string message = string.Empty;
            string username = Request.Form["username"];
            var ms = (from d in ss.Managers where d.UserId == username&&(d.IsDelete!=true||d.IsDelete==null) select d).FirstOrDefault();
            if (ms == null)
            {
                success = false;
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 校验密码是否正确+ValidateUserPassword()
        [HttpPost]
        public JsonResult ValidateUserPassword()
        {
            bool success = true;
            string message = string.Empty;
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            var ms = (from d in ss.Managers where d.IsDraft==false&&d.UserId == username && d.Password == password select d).FirstOrDefault();
            if (ms == null)
            {
                success = false;
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        } 
        #endregion

        #region 提交到+Index()
        [HttpPost]
        public ActionResult Index(int i=1)
        {
            bool success = true;
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            var ms = (from d in ss.Managers where d.IsDraft == false && (d.IsDelete == false || d.IsDelete == null) && d.UserId == username && d.Password == password select d).FirstOrDefault();
            if (ms==null)
            {
                Response.Write("<script>alert('用户名或密码错误')</script>");
                return View();
            }
            Session["admin"] = ms.UserId;
            if (ms.Authority == 0)
            {
                Session["authority"] = "管理员";
            }
            else if (ms.Authority == 1)
            {
                Session["authority"] = "督导";
            }
            else if (ms.Authority == 2)
            {
                Session["authority"] = "小队长";
            }
            else { return null; }
            //得到用户ip
            
            if (HttpContext != null)
            {
                string strIp = string.Empty;
                if (Request.ServerVariables.Get("Remote_Addr") != null)
                {
                    strIp = Request.ServerVariables.Get("Remote_Addr").ToString();//获取客户端ip
                }
                else
                {
                    if (Request.ServerVariables["http_via"] != null)//网关代理(例如开wifi给别人用)
                    {
                         strIp +=Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();//网关代理后面ip
                    }
                }

                strIp += "," + Request.ServerVariables.Get("Local_Addr").ToString(); //接收请求服务器ip
                LoginLog log = new LoginLog();
                log.CreatedTime = DateTime.Now;
                log.IPAddress=strIp;
                Managers manager=(from d in ss.Managers where d.UserId==username select d ).FirstOrDefault();
                log.ManagerId = manager.Id;
                ss.LoginLog.Add(log);
                ss.SaveChanges();
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
