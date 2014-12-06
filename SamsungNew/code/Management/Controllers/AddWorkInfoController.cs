using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;

namespace Management.Controllers
{
    public class AddWorkInfoController : Controller
    {
        //
        // GET: /AddWorkInfo/

        SS_HRM_DBEntities ss = new SS_HRM_DBEntities();

        //根据用户输入模糊匹配门店
        public JsonResult SearchShop()
        {
            string content = Request.Form["content"];
            string storeStoreHtml="";
            var Shops = (from d in ss.SShop
                     where d.Name.StartsWith(content) ||
                     d.Name.EndsWith(content) ||
                     d.Name.IndexOf(content) >= 0
                     select d).ToList();
            foreach (var item in Shops)
            {
                storeStoreHtml += "<option value='" + item.Name + "'>" + item.Name + "</option>";
            }
            var data = new {
                storeStoreHtml = storeStoreHtml           
            };
            return Json(data);
        }

        #region 督导信息录入界面+DudaoWork()
        [HttpGet]
        [Management.filter.LoginFilter]
        public ActionResult DudaoWork()
        {
            if (Session["authority"].ToString() == "督导")
            {
                string userid = Session["admin"].ToString();
                Managers ms = (from d in ss.Managers where d.UserId == userid select d).FirstOrDefault();
                ViewBag.name = ms.Name;
                ViewBag.flag = 0;
                return View();
            }
            else
            {
                return View("~/Views/Shared/AuthorityError.cshtml");
            }
        } 
        #endregion

        #region (提交)督导信息录入+DudaoWork()
        [HttpPost]
        public ActionResult DudaoWork(int i=1)
        {
            string name = Request.Form["supervisorWorkInfo.name"];
            Managers ms = (from d in ss.Managers where d.Name == name&&d.Authority==1 select d).FirstOrDefault();
            Guid id = ms.Id;
            string activity=Request.Form["supervisorWorkInfo.activity"];
            string product=Request.Form["supervisorWorkInfo.product"];
            string sub = Request.Form["supervisorWorkInfo.subsidy"];
            string wag = Request.Form["supervisorWorkInfo.wage"];
            string date = Request.Form["supervisorWorkInfo.date"];
            string work = Request.Form["supervisorWorkInfo.work"];
            string store = Request.Form["supervisorWorkInfo.store"];
            string remark = Request.Form["supervisorWorkInfo.remark"];
            int subsidy ;
            int wage;
            if (wag==null)
            {
                ModelState.AddModelError("supervisorWorkInfo.wage", "请输入日薪标准");
            }
            if (!int.TryParse(sub, out subsidy))
            {
                ModelState.AddModelError("supervisorWorkInfo.subsidy", "请输入整数");
            }
            if (!int.TryParse(wag, out wage))
            {
                ModelState.AddModelError("supervisorWorkInfo.wage", "请输入整数");
            }
            if (name == null)
            {
                ModelState.AddModelError("supervisorWorkInfo.name", "请输入姓名");
            }
            if (activity == null)
            {
                ModelState.AddModelError("supervisorWorkInfo.activity", "请输入活动名称");
            }
            if (product == null)
            {
                ModelState.AddModelError("supervisorWorkInfo.product", "请输入产品名称");
            }
            if (work == null)
            {
                ModelState.AddModelError("supervisorWorkInfo.work", "请输入工作职能");
            }
            if (store == null)
            {
                ModelState.AddModelError("supervisorWorkInfo.store", "请输入门店");
            }
            if (!ModelState.IsValid)
            { return null; }
            AttendingInfo atf = new AttendingInfo();
            atf.Id = Guid.NewGuid();
            atf.HumanId = id;
            atf.ActionName = activity;
            atf.StandardSalary = wage;
            atf.production = product;
            atf.BearFees = subsidy;
            atf.Date = date;
            atf.Functions = work;
            atf.SShop.Name = store;
            atf.Remark = remark;
            atf.Creatorname = Session["admin"].ToString();
            ss.AttendingInfo.Add(atf);
            ss.SaveChanges();
            ViewBag.flag = 1;
            ViewBag.name = name;
            return View();
        }
        #endregion

        //绑定队长姓名,需要找到督导和队长之间的关系
        public JsonResult CaptainList()
        {
            string content = Request.Form["content"];
            string captainValue = "";
            string user = Session["admin"].ToString();
            List<Managers> mt = (from d in ss.Managers where d.CreatedManId == user && d.Authority == 2 select d).ToList();
            foreach (var item in mt)
            {
                captainValue += "<option value='" + item.Id + "'>" + item.Name + "</option>";
            }
            var data = new
            {
                captainValue = captainValue
            };
            return Json(data);
        }

        #region 队长信息录入界面+DuizhangWork()
        [HttpGet]
        [Management.filter.LoginFilter]
        public ActionResult DuizhangWork()
        {
            ViewBag.flag = 0;
            return View();
        } 
        #endregion

        #region （提交）队长信息录入+DuizhangWork()
        [HttpPost]
        public ActionResult DuizhangWork(int i = 1)
        {
            string name = Request.Form["captainWorkInfo.name"];
            Guid id = new Guid(name);
            string activity = Request.Form["captainWorkInfo.activity"];
            string product = Request.Form["captainWorkInfo.product"];
            string sub = Request.Form["captainWorkInfo.subsidy"];
            string wag = Request.Form["captainWorkInfo.wage"];
            string date = Request.Form["captainWorkInfo.date"];
            string work = Request.Form["captainWorkInfo.work"];
            string store = Request.Form["captainWorkInfo.store"];
            string remark = Request.Form["captainWorkInfo.remark"];
            int subsidy;
            int wage;
            if (wag.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.wage", "请输入日薪标准");
            }
            if (!int.TryParse(sub, out subsidy))
            {
                ModelState.AddModelError("captainWorkInfo.subsidy", "请输入整数");
            }
            if (!int.TryParse(wag, out wage))
            {
                ModelState.AddModelError("captainWorkInfo.wage", "请输入整数");
            }
            if (name.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.name", "请输入姓名");
            }
            if (activity.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.activity", "请输入活动名称");
            }
            if (product.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.product", "请输入产品名称");
            }
            if (work.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.work", "请输入工作职能");
            }
            if (store.Length == 0)
            {
                ModelState.AddModelError("captainWorkInfo.store", "请输入门店");
            }
            if (!ModelState.IsValid)
            { return null; }
            AttendingInfo atf = new AttendingInfo();
            atf.Id = Guid.NewGuid();
            atf.StandardSalary = wage;
            atf.Creatorname = Session["admin"].ToString();
            atf.HumanId = id;
            atf.ActionName = activity;
            atf.production = product;
            atf.BearFees = subsidy;
            atf.Date = date;
            atf.Functions = work;
            atf.SShop.Name = store;
            atf.Remark = remark;
            ss.AttendingInfo.Add(atf);
            ss.SaveChanges();

            ViewBag.flag = 1;
            return View();
        }
        #endregion
    }
}
