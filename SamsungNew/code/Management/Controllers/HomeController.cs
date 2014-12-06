using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Data.Entity;
using System.Data;

namespace Management.Controllers
{
    public class HomeController : Controller
    {
        private SS_HRM_DBEntities db = new SS_HRM_DBEntities();


        #region 首页+Index()
        [Management.filter.LoginFilter]
        public ActionResult Index()
        {
            return View();
        } 
        #endregion

        #region 公告操作+CheckNews()
        //查看公告
        [Management.filter.LoginFilter]
        public ActionResult CheckNews()
        {

            List<Notice> notice = db.Notice.ToList();
            return View(notice);
        } 

        //编辑公告
        [Management.filter.LoginFilter]
        public ActionResult EditNews(int id)
        {
            Session["NewsId"] = id;
            Notice notice = db.Notice.Find(id);
            ViewBag.title = notice.NoticeTitle;
            ViewBag.content = notice.NoticeContent;
            return View(notice);
        }

        //提交编辑
        [HttpPost]
        [Management.filter.LoginFilter]
        public ActionResult ModifyNews()
        {
            int NewsId=(int)Session["NewsId"];
            Notice notice = db.Notice.Find(NewsId);
            string title = Request.Form.Get("newsInfo.title");
            string content = Request.Form.Get("newsInfo.contents");

            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("newsInfo.title", "请输入内容");
            }
            else
            {
                notice.NoticeTitle = title;
                ViewBag.title = title;

            }

            if (string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("newsInfo.content", "请输入内容");
            }
            else
            {
                notice.NoticeContent = content;
                ViewBag.content = notice.NoticeContent;
            }

            if (ModelState.IsValid)
            {
                db.Entry(notice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("CheckNews");
            }
            return View("editNews");
        }
        //展示公告
        [Management.filter.LoginFilter]
        public ActionResult showNewsInfo(int id)
        {
            Notice notice = db.Notice.Find(id);
            
            return View(notice);
        }

        //删除公告
        public ActionResult deleteNews(int id)
        {
            Notice notice = db.Notice.Find(id);
            db.Notice.Remove(notice);
            db.SaveChanges();
            return RedirectToAction("CheckNews");

        }
        #endregion



    }
}
