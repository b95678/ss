using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
//using System.Web.Helpers;

namespace Management.Controllers
{
    public class BasicInfoController : Controller
    {
        SS_HRM_DBEntities ss = new SS_HRM_DBEntities();


        //验证旧密码是否存在
        [HttpPost]
        public JsonResult ValidateOldPW()
        {
            bool success = true;
            string message = string.Empty;
            string username = Request.Form["username"];
            string password = Request.Form["oldpassword"];
            var ms = (from d in ss.Managers where d.UserId == username && d.Password == password select d).FirstOrDefault();
            if (ms == null)
            {
                success = false;
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        #region 根据登录信息查询本人信息+ personinfo()
        [Management.filter.LoginFilter]
        public ActionResult personinfo()
        {
            string userid = (string)Session["admin"];
            Managers ms = (from d in ss.Managers where d.UserId == userid select d).FirstOrDefault();
            return View(ms);
        }
        #endregion

        #region 修改密码界面+AlterPassword()
        [HttpGet]
        [Management.filter.LoginFilter]
        public ActionResult AlterPassword()
        {

            return View();
        }
        #endregion

        #region (提交)修改密码+AlterPassword()
        [HttpPost]
        public ActionResult AlterPassword(int i = 1)
        {
            bool success = true;
            string userid = (string)Session["admin"];
            string password = Request.Form["newpassword"];
            Managers ms = (from d in ss.Managers where d.UserId == userid select d).FirstOrDefault();
            ms.Password = password;
            ss.SaveChanges();
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //统计人员信息和培训信息
        public JsonResult CheckDaliyMessage()
        {
            string username = Request.Form["username"];//督导用户名
            string newsId = Request.Form["id"];//日报id
            int newId = 0;
            if (!string.IsNullOrEmpty(newsId))
            {
                newId = Convert.ToInt32(newsId);
            }
            List<AttendingInfo> atolist;

            List<Train> trainlist;
            if (newId == 0)
            {
                atolist = (from d in ss.AttendingInfo where d.Creatorname == username && d.WorkContentId == null && d.IsDraft == false select d).ToList();
                trainlist = (from d in ss.Train where d.Creatorname == username && d.WorkContentId == null && d.IsDraft == false select d).ToList();
            }
            else 
            {
                atolist = (from d in ss.AttendingInfo where d.Creatorname == username && d.WorkContentId == newId && d.IsDraft == false select d).ToList();
                trainlist = (from d in ss.Train where d.Creatorname == username && d.WorkContentId == newId && d.IsDraft == false select d).ToList();
            }
            int totalWork = atolist.Count();
            int totalWage = 0;
            int totaltrain = trainlist.Count();
            foreach (var item in atolist)
            {
                if (item.BearFees.HasValue)
                {
                    totalWage += (int)item.BearFees;
                }
                totalWage += (int)item.StandardSalary;
            }
            var data = new
            {
                totalWork = totalWork,
                totalWage = totalWage,
                totalTrain = totaltrain

            };
            return Json(data);
        }

        //逻辑删除日报
        public JsonResult DeleteNews()
        {
            var success = true;
            string newsId = Request["newsId"].ToString();
            int newId = Convert.ToInt32(newsId);
            WorkContent wc = ss.WorkContent.Find(newId);
            wc.IsDelete = true;
            ss.SaveChanges();
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        #region 日报大致情况+BasicStatic()
        //日报状况
        [Management.filter.LoginFilter]
        [HttpGet]
        public ActionResult BasicStatic()
        {
            ViewBag.pass = "未审核";
            ViewBag.flag = 0;
            return View();
        }

        //(提交)日报状况
        [HttpPost]
        public ActionResult BasicStatic(int? i)
        {
            string title = Request.Form["daliyReport.title"];
            string totalWork = Request.Form["daliyReport.totalWork"];
            string totalWage = Request.Form["daliyReport.totalWage"];
            string totalTrain = Request.Form["daliyReport.totalTrain"];
            string todayWork = Request.Form["daliyReport.todayWork"];
            string tomorrowWork = Request.Form["daliyReport.tomorrowWork"];
            int totalwork = Convert.ToInt32(totalWork);
            int totalwage = Convert.ToInt32(totalWage);
            int totaltrain = Convert.ToInt32(totalTrain);
            System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
            for (int ifile = 0; ifile < files.Count; ifile++)
            {
                if (!string.IsNullOrEmpty(files[ifile].FileName))
                {
                    if (!PictureValiate.PicExtend(files[ifile].FileName))
                    {
                        if (ifile == 0)
                        {
                            ModelState.AddModelError("mail", "图片格式错误");
                        }
                        else
                        {
                            ModelState.AddModelError("mail" + ifile, "图片格式错误");
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                //Response.Write("<script type='text/javascript'>alert('不允许上传非图片格式的文件');</script>");
                ViewBag.pass = "未审核";
                ViewBag.title = title;
                ViewBag.todaywork = todayWork;
                ViewBag.tomorrowwork = tomorrowWork;
                ViewBag.flag = 0;
                return View();
            }
            string creator = Session["admin"].ToString();
            Managers ms = (from d in ss.Managers where d.UserId == creator select d).FirstOrDefault();
            WorkContent wct = new WorkContent();
            wct.Title = title;
            wct.CreatedTime = DateTime.Now;
            wct.CreatorId = ms.Id;
            wct.AttendCount = totalwork;
            wct.TrainCount = totaltrain;
            wct.MoneyCount = totalwage;
            string mailpicture = string.Empty;
            for (int ifile = 0; ifile < files.Count; ifile++)
            {
                if (!string.IsNullOrEmpty(files[ifile].FileName))
                {

                    HttpPostedFileBase mailfile = new HttpPostedFileWrapper(files[ifile]);
                    string file = MailPicName(files[ifile].FileName);
                    mailfile.SaveAs(Server.MapPath("~/uploadImg/MailImg/") + file);
                    Suo.TouXiangSuoFang(mailfile, file, Server.MapPath("~/uploadImg/MailImg/suo/"), 120, 60);
                    mailpicture += file + "?";
                }
            }
            if (!string.IsNullOrEmpty(mailpicture))
            {
                wct.MailPicture = mailpicture;
            }
            wct.IsDraft = "0";
            wct.Todaywork = todayWork;
            wct.Tomorrowplan = tomorrowWork;
            ss.WorkContent.Add(wct);
            //ViewBag.pic = wct.MailPicture;
            ss.SaveChanges();

            //将日报id为空的上班、培训信息赋值当前日报id
            List<AttendingInfo> atolist = (from d in ss.AttendingInfo where d.Creatorname == creator && d.WorkContentId == null && d.IsDraft == false select d).ToList();
            List<Train> trainlist = (from d in ss.Train where d.Creatorname == creator && d.WorkContentId == null && d.IsDraft == false select d).ToList();
            foreach(var item in atolist)
            {
                item.WorkContentId = wct.Id;
            }
            foreach (var item in trainlist)
            {
                item.WorkContentId = wct.Id;
            }
            ss.SaveChanges();

            ViewBag.flag = 1;
            ViewBag.pass = "未审核";
            return View();
        }

        //预览日报
        public ActionResult BasicStaticPre(int id)
        {
            WorkContent wc;
            wc = (from d in ss.WorkContent where d.Id == id select d).FirstOrDefault();
            ViewBag.pass = "不通过";
            if (wc != null)
            {
                if (!wc.ISPass.HasValue)
                {
                    ViewBag.pass = "未审核";
                }
                else if ((bool)wc.ISPass)
                {
                    ViewBag.pass = "通过";
                }
            }
            ViewBag.flag = 0;
            ViewBag.id = wc.Id;
            return View(wc);
        }
        //(提交)日报状况预览
        [HttpPost]
        public ActionResult BasicStaticPre()
        {
            int id = Convert.ToInt32(Request.Form["id"].ToString());
            string title = Request.Form["daliyReport.title"];
            string totalWork = Request.Form["daliyReport.totalWork"];
            string totalWage = Request.Form["daliyReport.totalWage"];
            string totalTrain = Request.Form["daliyReport.totalTrain"];
            string todayWork = Request.Form["daliyReport.todayWork"];
            string tomorrowWork = Request.Form["daliyReport.tomorrowWork"];
            int totalwork = Convert.ToInt32(totalWork);
            int totalwage = Convert.ToInt32(totalWage);
            int totaltrain = Convert.ToInt32(totalTrain);
            System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
            for (int ifile = 0; ifile < files.Count; ifile++)
            {
                if (!string.IsNullOrEmpty(files[ifile].FileName))
                {
                    if (!PictureValiate.PicExtend(files[ifile].FileName))
                    {
                        if (ifile == 0)
                        {
                            ModelState.AddModelError("mail", "图片格式错误");
                        }
                        else
                        {
                            ModelState.AddModelError("mail" + ifile, "图片格式错误");
                        }
                    }
                }
            }
            WorkContent wc = (from d in ss.WorkContent where d.Id == id select d).FirstOrDefault();
            if (!ModelState.IsValid)
            { return View(wc); }
            wc.Title = title;
            //上传图片
            string mailpicture = wc.MailPicture;
            string mailpic = string.Empty;
            string mailpic123 = string.Empty;
            string[] strs;
            if (!string.IsNullOrEmpty(mailpicture))
            {
                strs = mailpicture.Split('?');
            }
            else { strs = new string[1]; }
            for (int ifile = 0; ifile < files.Count; ifile++)
            {
                if (!string.IsNullOrEmpty(files[ifile].FileName))
                {
                    HttpPostedFileBase mailfile = new HttpPostedFileWrapper(files[ifile]);
                    string file = MailPicName(files[ifile].FileName);
                    mailfile.SaveAs(Server.MapPath("~/uploadImg/MailImg/") + file);
                    Suo.TouXiangSuoFang(mailfile, file, Server.MapPath("~/uploadImg/MailImg/suo/"), 120, 60);
                    if (ifile > strs.Length - 1)
                    {
                        mailpic += file + "?";
                    }
                    else
                    {
                        strs[ifile] = file;
                    }
                }
            }
            for (int i = 0; i < strs.Length; i++)
            {
                if (!string.IsNullOrEmpty(strs[i]))
                {
                    mailpic123 += strs[i] + "?";
                }
            }
            if (string.IsNullOrEmpty(mailpic123))
            {
                mailpicture = mailpic;
            }
            else
            {
                mailpicture = mailpic123 + mailpic;
            }
            if (!string.IsNullOrEmpty(mailpicture))
            {
                wc.MailPicture = mailpicture;
            }

            wc.AttendCount = totalwork;
            wc.TrainCount = totaltrain;
            wc.MoneyCount = totalwage;
            wc.Todaywork = todayWork;
            wc.Tomorrowplan = tomorrowWork;
            wc.IsDraft = "0";
            ViewBag.pic = wc.MailPicture;
            ss.SaveChanges();
            ViewBag.flag = 1;
            ViewBag.pass = "不通过";
            if (wc.IsDraft == "0")
            {
                if (!wc.ISPass.HasValue)
                {
                    ViewBag.pass = "未审核";
                }
                else if ((bool)wc.ISPass)
                {
                    ViewBag.pass = "通过";
                }
            }
            return View(wc);

        }

        //保存草稿
        [HttpPost]
        public ActionResult BasicStaticCao()
        {
            string todayWork = Request.Form["daliyReport.todayWork"];
            string tomorrowWork = Request.Form["daliyReport.tomorrowWork"];
            string title = Request.Form["daliyReport.title"];
            string totalWork = Request.Form["daliyReport.totalWork"];
            string totalWage = Request.Form["daliyReport.totalWage"];
            string totalTrain = Request.Form["daliyReport.totalTrain"];
            int totalwork = Convert.ToInt32(totalWork);
            int totalwage = Convert.ToInt32(totalWage);
            int totaltrain = Convert.ToInt32(totalTrain);
            HttpPostedFileBase mail = Request.Files["mail"];
            if (mail != null)
            {
                if (!string.IsNullOrEmpty(mail.FileName))
                {
                    if (!PictureValiate.PicExtend(mail.FileName))
                    {
                        ModelState.AddModelError("mail", "图片格式错误");
                    }
                }
            }
            if (!ModelState.IsValid)
            { return null; }
            string creator = Session["admin"].ToString();
            Managers ms = (from d in ss.Managers where d.UserId == creator select d).FirstOrDefault();
            DateTime yestoday = DateTime.Now.AddDays(-1);
            DateTime Tomorrow = DateTime.Now.AddDays(1);
            WorkContent wc = (from d in ss.WorkContent where d.CreatedTime > yestoday && d.CreatedTime < Tomorrow && d.CreatorId == ms.Id && d.IsDraft == "1" select d).FirstOrDefault();
            if (wc == null)
            {
                WorkContent wct = new WorkContent();
                wct.Title = title;
                wct.CreatedTime = DateTime.Now;
                wct.CreatorId = ms.Id;
                wct.AttendCount = totalwork;
                wct.TrainCount = totaltrain;
                wct.MoneyCount = totalwage;
                wct.Todaywork = todayWork;
                wct.Tomorrowplan = tomorrowWork;
                wct.IsDraft = "1";
                //上传图片
                if (mail != null)
                {
                    if (!string.IsNullOrEmpty(mail.FileName))
                    {
                        string file = MailPicName(mail.FileName);
                        mail.SaveAs(Server.MapPath("~/uploadImg/MailImg/") + file);
                        Suo.TouXiangSuoFang(mail, file, Server.MapPath("~/uploadImg/MailImg/suo/"), 120, 60);
                        wct.MailPicture = file;
                    }
                }
                ss.WorkContent.Add(wct);
            }
            else
            {
                wc.Title = title;
                //上传图片
                if (mail != null)
                {
                    if (!string.IsNullOrEmpty(mail.FileName))
                    {
                        string file = MailPicName(mail.FileName);
                        mail.SaveAs(Server.MapPath("~/uploadImg/MailImg/") + file);
                        wc.MailPicture = file;
                        Suo.TouXiangSuoFang(mail, file, Server.MapPath("~/uploadImg/MailImg/suo/"), 120, 60);
                    }
                }
                wc.AttendCount = totalwork;
                wc.TrainCount = totaltrain;
                wc.MoneyCount = totalwage;
                wc.IsDraft = "1";
                wc.Todaywork = todayWork;
                wc.Tomorrowplan = tomorrowWork;
            }
            ss.SaveChanges();
            var data = new { };
            if (mail != null)
            {
                ViewBag.pic = mail.FileName;

            }
            return Json(data);
        }
        #endregion

        #region 所有督导日报列表+CheckDuDaoNews()
        //督导日报列表
        [Management.filter.LoginFilter]
        public ActionResult CheckDuDaoNews()
        {
            return View();
        }

        //督导日报表中的列
        class Node
        {
            public int id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
            public string title { get; set; }
            public string office { get; set; }
            public string createtime { get; set; }
            public int totalWork { get; set; }
            public int totalTrain { get; set; }
            public int totalWage { get; set; }
            public string isaudit { get; set; }
            public string Remark { get; set; }

        }
        #region 获取督导日报信息+GetData()
        public JsonResult GetData()
        {
            //查询条件
            string content = Request.Params["content"];
            DateTime ct = DateTime.Now;
            if (!string.IsNullOrEmpty(content))
            {
                ct = Convert.ToDateTime(content);
            }
            DateTime yestoday = ct.AddDays(-1);
            DateTime Tomorrow = ct.AddDays(1);

            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //第一行行号
            int startRow = pagesize * (page - 1);
            IList<Node> list = new List<Node>();
            //当前页展示的记录
            List<WorkContent> list1;
            //该表符合条件的总记录
            List<WorkContent> list2;
            //判断查询条件是否为空
            if (!string.IsNullOrEmpty(content))//不为空，则根据条件查询记录
            {
                //当前页展示的记录
                list1 = ((from d in ss.WorkContent orderby d.CreatedTime descending where d.CreatedTime > yestoday && d.CreatedTime < Tomorrow && d.IsDraft == "0" && (d.IsDelete==false||d.IsDelete ==null) select d).Skip(startRow).Take(pagesize)).ToList();
                //该表符合条件的总记录
                list2 = (from d in ss.WorkContent orderby d.CreatedTime descending where d.CreatedTime > yestoday && d.CreatedTime < Tomorrow && d.IsDraft == "0" && (d.IsDelete==false||d.IsDelete ==null) select d).ToList();

            }
            else
            {
                //当前页展示的记录
                list1 = ((from d in ss.WorkContent orderby d.CreatedTime descending where d.IsDraft == "0" && (d.IsDelete==false||d.IsDelete ==null) select d).Skip(startRow).Take(pagesize)).ToList();
                //该表符合条件的总记录
                list2 = (from d in ss.WorkContent orderby d.CreatedTime descending where d.IsDraft == "0" && (d.IsDelete==false||d.IsDelete ==null) select d).ToList();

            }
            //总记录数
            int totalcout = list2.Count;
            //记录的总页数
            int totalpage = totalcout / pagesize;
            if (totalcout % pagesize != 0)
            {
                totalpage++;
            }
            //分页按钮的样式
            string pageBtnClass = "l-disabled#l-disabled";//只有一页

            if (page == 1 && page < totalpage)
            {
                pageBtnClass = "l-disabled#l-abled";
            }
            if (page > 1 && page == totalpage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            if (page > 1 && page < totalpage)
            {
                pageBtnClass = "l-abled#l-abled";
            }
            if (page > 1 && page == totalpage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            foreach (var item in list1)
            {
                string time = item.CreatedTime.Year.ToString() + "年" + item.CreatedTime.Month.ToString() + "月" + item.CreatedTime.Day.ToString() + "日";
                string pass = "不通过";
                if (!item.ISPass.HasValue)
                {
                    pass = "未审核";
                }
                else if ((bool)item.ISPass)
                {
                    pass = "通过";
                }
                list.Add(new Node()
                {
                    id = item.Id,
                    username = item.Managers.UserId,
                    name = item.Managers.Name,
                    title = item.Title,
                    office = item.Managers.City1.Office.Name,
                    createtime = time,
                    totalWork = (int)item.AttendCount,
                    totalTrain = (int)item.TrainCount,
                    totalWage = (int)item.MoneyCount,
                    isaudit = pass,
                    Remark = item.Remark
                });
            }
            var griddata = new
            {
                gdata = list,
                page = page,
                totalPage = totalpage,
                pageBtn = pageBtnClass,
                pageMsg = "当前第" + page + "页，共" + totalpage + "页，共" + totalcout + "条记录，每页显示" + pagesize + "条记录",
                pageSize = pagesize
            };
            return Json(griddata);
        }
        #endregion
        #endregion

        #region 审核督导日报+CheckDudaoBasic()
        //根据id查看督导日报详情
        [Management.filter.LoginFilter]
        public ActionResult CheckDudaoBasic(int id)
        {
            WorkContent wct = (from d in ss.WorkContent where d.Id == id && d.IsDraft == "0" select d).FirstOrDefault();
            ViewBag.flag = 0;
            return View(wct);
        }

        //审核督导日报详情
        [HttpPost]
        public ActionResult CheckDudaoBasic()
        {
            int id = Convert.ToInt32(Request.Form["id"].ToString());
            string isaudited = Request.Form["isaudited"];
            string remark = Request.Form["content"];
            bool a = false;
            WorkContent wct = ss.WorkContent.Find(id);
            MailNotifyInfo mailNotifyInfo = new MailNotifyInfo();
            mailNotifyInfo.Author = "admin";
            mailNotifyInfo.Content = remark;
            mailNotifyInfo.EmailAddress = wct.Managers.Email;
            if (isaudited == "是")
            {
                a = true;
                mailNotifyInfo.Title = wct.Title + "审核通过！！！";
            }
            else
            {
                mailNotifyInfo.Title = wct.Title + "审核不通过！！！";
            }
            NotificationHandler.Instance.AppendNotification(mailNotifyInfo);
            wct.ISPass = a;
            wct.Remark = remark;
            ss.SaveChanges();
            ViewBag.flag = 1;
            return View(wct);
        }

        #endregion

        #region 当前督导日报列表+DuDaoNewsDone()
        //督导日报列表
        [Management.filter.LoginFilter]
        public ActionResult DuDaoNewsDone()
        {
            return View();
        }

        //当前督导日报表中的列
        class NodeDone
        {
            public int id { get; set; }
            public string name { get; set; }
            public string title { get; set; }
            public string office { get; set; }
            public string createtime { get; set; }
            public int totalWork { get; set; }
            public int totalTrain { get; set; }
            public int totalWage { get; set; }
            public string isaudit { get; set; }
            public string Remark { get; set; }

        }
        #region 获取督导日报信息+GetDataDone()
        public JsonResult GetDataDone()
        {
            //查询条件
            string uer = Session["admin"].ToString();
            Guid userid = (from d in ss.Managers where d.UserId == uer select d.Id).FirstOrDefault();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //第一行行号
            int startRow = pagesize * (page - 1);
            IList<NodeDone> list = new List<NodeDone>();
            //当前页展示的记录
            List<WorkContent> list1;
            //该表符合条件的总记录
            List<WorkContent> list2;
            ////判断查询条件是否为空
            //if (!string.IsNullOrEmpty(content))//不为空，则根据条件查询记录
            //{
            //当前页展示的记录
            list1 = ((from d in ss.WorkContent orderby d.CreatedTime descending where d.CreatorId == userid && d.IsDraft == "0"&&(d.IsDelete==false||d.IsDelete ==null) select d).Skip(startRow).Take(pagesize)).ToList();
            //该表符合条件的总记录
            list2 = (from d in ss.WorkContent orderby d.CreatedTime descending where d.CreatorId == userid && d.IsDraft == "0" && (d.IsDelete==false||d.IsDelete ==null) select d).ToList();

            //总记录数
            int totalcout = list2.Count;
            //记录的总页数
            int totalpage = totalcout / pagesize;
            if (totalcout % pagesize != 0)
            {
                totalpage++;
            }
            //分页按钮的样式
            string pageBtnClass = "l-disabled#l-disabled";//只有一页

            if (page == 1 && page < totalpage)
            {
                pageBtnClass = "l-disabled#l-abled";
            }
            if (page > 1 && page == totalpage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            if (page > 1 && page < totalpage)
            {
                pageBtnClass = "l-abled#l-abled";
            }
            if (page > 1 && page == totalpage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            foreach (var item in list1)
            {
                string time = item.CreatedTime.Year.ToString() + "年" + item.CreatedTime.Month.ToString() + "月" + item.CreatedTime.Day.ToString() + "日";
                string pass = "不通过";
                if (!item.ISPass.HasValue)
                {
                    pass = "未审核";
                }
                else if ((bool)item.ISPass)
                {
                    pass = "通过";
                }
                list.Add(new NodeDone()
                {
                    id = item.Id,
                    name = item.Managers.Name,
                    title = item.Title,
                    office = item.Managers.City1.Office.Name,
                    createtime = time,
                    totalWork = (int)item.AttendCount,
                    totalTrain = (int)item.TrainCount,
                    totalWage = (int)item.MoneyCount,
                    isaudit = pass,
                    Remark = item.Remark
                });
            }
            var griddata = new
            {
                gdata = list,
                page = page,
                totalPage = totalpage,
                pageBtn = pageBtnClass,
                pageMsg = "当前第" + page + "页，共" + totalpage + "页，共" + totalcout + "条记录，每页显示" + pagesize + "条记录",
                pageSize = pagesize
            };
            return Json(griddata);
        }
        #endregion
        #endregion

        #region 培训信息+TrainInfo()
        //培训信息
        [Management.filter.LoginFilter]
        public ActionResult TrainInfo()
        {
            if (Request["id"] != null)
            {
                string id = Request["id"].ToString();
                ViewBag.id = id;
            }
            string usernames = Request["usernames"].ToString();
            ViewBag.username = usernames;
            return View();
        }
        public class getinfo//获取人员信息
        {
            public Guid id;
            public string uniquenum;
            public string name;
            public string office;
            public string city;
            public string identity;
            public string product;
            public string type;
            public int grade;
            public string trainer;
            public string date;
            public string insertuser;
            public string updateuser;
        }
        public ActionResult checkTrainInfo()
        {
            string id = Request.Params["key"].ToString();
            string usernames = Request.Params["content"].ToString();
            int newsId = 0;
            if (!string.IsNullOrEmpty(id))
            {
                newsId = Convert.ToInt32(id);
            }

            List<getinfo> getm = new List<getinfo>();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            List<Train> das;
            List<Train> da;

            if (newsId == 0)
            {
                das = (from d in ss.Train where d.IsDraft == false && d.WorkContentId == null && d.Creatorname == usernames select d).ToList();
                da = (from d in ss.Train orderby d.TrainStartTime descending where d.IsDraft == false && d.WorkContentId == null && d.Creatorname == usernames select d).Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            else
            {
                das = (from d in ss.Train where d.IsDraft == false && d.WorkContentId == newsId && d.Creatorname == usernames select d).ToList();
                da = (from d in ss.Train orderby d.TrainStartTime descending where d.IsDraft == false && d.WorkContentId == newsId && d.Creatorname == usernames select d).Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            foreach (var item in da)
            {
                getm.Add(new getinfo()
                {
                    id=item.Id,
                    uniquenum = item.HumanBasicFile.uniNum, // 唯一号
                    name = item.HumanBasicFile.Name,//姓名
                    office = item.HumanBasicFile.City1.Office.Name,//办事处
                    city = item.HumanBasicFile.City1.Name,//城市
                    identity = item.HumanBasicFile.IDcardNo,//身份证号
                    product = item.Trainpro,//产品
                    type = item.TrainProduction,//产品型号号
                    grade = Convert.ToInt32(item.TrainScore),//分数
                    trainer = item.Trainlecturer,//培训师
                    date = item.TrainStartTime,
                    insertuser = item.HumanBasicFile.CreatedManagerID,//录入人员
                    updateuser = item.HumanBasicFile.EditManagerId//编辑人员
                }
                );
            }
            int totalPage = das.Count() / pagesize;
            if (das.Count() % pagesize != 0)
            { totalPage++; }
            if (totalPage == 0)
            {
                totalPage = 1;
            }
            //分页按钮的样式
            string pageBtnClass = "l-disabled#l-disabled";//只有一页

            if (page == 1 && page < totalPage)
            {
                pageBtnClass = "l-disabled#l-abled";
            }
            if (page > 1 && page == totalPage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            if (page > 1 && page < totalPage)
            {
                pageBtnClass = "l-abled#l-abled";
            }
            if (page > 1 && page == totalPage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }

            var Data = new //对data辅值
            {
                gdata = getm,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(Data);
        }//显示培训信息
        #endregion

        //删除培训信息
        public JsonResult DeleteTrainInfo()
        {
            var success = true;
            string trainInfoId = Request["trainInfoId"].ToString();
            Guid trainId = new Guid(trainInfoId);
            Train tn = ss.Train.Find(trainId);
            ss.Train.Remove(tn);
            if (tn.WorkContentId != null)
            {
                WorkContent wc = ss.WorkContent.Find(tn.WorkContentId);
                wc.TrainCount--;
            }
            ss.SaveChanges();
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        //删除上班信息
        public JsonResult DeleteWorkInfo()
        {
            var success = true;
            string workId = Request["workId"].ToString();
            Guid wId = new Guid(workId);
            AttendingInfo attInfo = ss.AttendingInfo.Find(wId);
            ss.AttendingInfo.Remove(attInfo);
            if (attInfo.WorkContentId != null)
            {
                WorkContent wc = ss.WorkContent.Find(attInfo.WorkContentId);
                wc.MoneyCount -= attInfo.StandardSalary;
                if (attInfo.BearFees != null)
                {
                    wc.MoneyCount -= attInfo.BearFees;
                }
                wc.AttendCount--;
            }
            ss.SaveChanges();
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        #region 上班信息+checkWorkInfo()

        public ActionResult WorkInfo()
        {
            if (Request["id"] != null)
            {
                string id = Request["id"].ToString();
                ViewBag.id = id;
            }
            string usernames = Request["usernames"].ToString();
            ViewBag.username = usernames;
            return View();
        }
        public class getinfo1
        {
            public Guid id;
            public string uniquenum;
            public string name;
            public string office;
            public string city;
            public string activity;
            public string product;
            public string store;
            public string work;
            public int wage;
            public Int32 subsidy;
            public string date;
            public string insertuser;
            public string updateuser;
        } //获取人员信息
        public ActionResult checkWorkInfo()//显示上班信息
        {
            string id = Request.Params["key"].ToString();
            int newsId = 0;
            if (!string.IsNullOrEmpty(id))
            {
                newsId = Convert.ToInt32(id);
            }
            string usernames = Request.Params["content"].ToString();
            List<getinfo1> getm = new List<getinfo1>();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            List<AttendingInfo> das;

            List<AttendingInfo> da ;

            if (newsId == 0)
            {
                das = (from d in ss.AttendingInfo where d.IsDraft == false && d.Creatorname == usernames && d.WorkContentId == null select d).ToList();
                da = (from d in ss.AttendingInfo orderby d.Date descending where d.IsDraft == false && d.Creatorname == usernames && d.WorkContentId == null select d).Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            else
            {
                das = (from d in ss.AttendingInfo where d.IsDraft == false && d.Creatorname == usernames && d.WorkContentId == newsId select d).ToList();
                da = (from d in ss.AttendingInfo orderby d.Date descending where d.IsDraft == false && d.Creatorname == usernames && d.WorkContentId == newsId select d).Skip((page - 1) * pagesize).Take(pagesize).ToList();
            }
            foreach (var item in da)
            {
                getm.Add(new getinfo1()
                {
                    id=item.Id,
                    uniquenum = item.HumanBasicFile.uniNum,//唯一号
                    name = item.HumanBasicFile.Name,//姓名
                    office = item.HumanBasicFile.City1.Office.Name,//办事处
                    city = item.HumanBasicFile.City1.Name,//城市
                    activity = item.ActionName,//活动
                    product = item.production,//产品
                    store = item.SShop.Name,//上班门店
                    work = item.Functions,//工作职能
                    wage = Convert.ToInt32(item.StandardSalary),//日薪标准
                    subsidy = Convert.ToInt32(item.BearFees),//补助
                    date = item.Date,
                    insertuser = item.HumanBasicFile.CreatedManagerID,//录入人员
                    updateuser = item.HumanBasicFile.EditManagerId//编辑人员
                }
                );
            }
            int totalPage = das.Count() / pagesize;
            if (das.Count() % pagesize != 0)
            { totalPage++; }
            if (totalPage == 0)
            {
                totalPage = 1;
            }
            //分页按钮的样式
            string pageBtnClass = "l-disabled#l-disabled";//只有一页

            if (page == 1 && page < totalPage)
            {
                pageBtnClass = "l-disabled#l-abled";
            }
            if (page > 1 && page == totalPage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }
            if (page > 1 && page < totalPage)
            {
                pageBtnClass = "l-abled#l-abled";
            }
            if (page > 1 && page == totalPage)
            {
                pageBtnClass = "l-abled#l-disabled";
            }

            var Data = new //对data辅值
            {
                gdata = getm,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(Data);
        }
        #endregion

        #region 图片验证

        //检验邮件名称图片名称
        public string MailPicName(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in ss.WorkContent select d.MailPicture).ToList();
            string newname = picName;
            foreach (var item in manPhoto)
            {
                if (picName == item)
                {
                    newname = PictureValiate.newName(fileName, extend);
                    break;
                }
            }
            return newname;

        }
        #endregion
    }
}
