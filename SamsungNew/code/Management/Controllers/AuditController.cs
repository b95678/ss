using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;

namespace Management.Controllers
{
    public class AuditController : Controller
    {
        private SS_HRM_DBEntities db = new SS_HRM_DBEntities();

        //存放已审核信息。
        class getAuditedInfo
        {
            public string uniquenum { get; set; }
            public string name { get; set; }
            public string city { get; set; }
            public string identitys { get; set; }
            public string sumwage { get; set; }
            public string sumwork { get; set; }
            public string begindate { get; set; }
            public string enddate { get; set; }
        }

        class getAttendingInfo
        {
            public string id { get; set; }
            public string uniquenum { get; set; }
            public string name { get; set; }
            public string shop { get; set; }
            public string wage { get; set; }
            public string begindate { get; set; }
        }


        //
        // GET: /Audit/

        //今日审核视图
        [Management.filter.LoginFilter]
        public ActionResult Audit()
        {
            return View();
        }

        //获取今日审核的上班人员
        public ActionResult getAttending()
        {
            //获取基本信息
            IList<getAttendingInfo> atInfo = new List<getAttendingInfo>();
            //从数据库中获取表
            //获取非草稿状态
            List<AttendingInfo> Attending = db.AttendingInfo.Where(a => a.IsDraft == false && a.IsPass != true).ToList();
            //获取当前督导可控制的记录
            string id = Session["admin"].ToString();
            Attending = Attending.Where(a => a.Creatorname == Session["admin"].ToString()).ToList();
            //获取当天需要审核的记录
            Attending = Attending.Where(a => Convert.ToDateTime(a.Date).ToShortDateString() == DateTime.Today.ToShortDateString()).ToList();

            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);

            //总页数
            int totalPage = Attending.Count() / pagesize;
            if (Attending.Count() % pagesize != 0)
            { totalPage++; }
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

            foreach (var item in Attending)
            {
                atInfo.Add(new getAttendingInfo
                {
                    id = item.Id.ToString(),
                    uniquenum = item.HumanBasicFile.uniNum,
                    name = item.HumanBasicFile.Name,
                    shop = item.SShop.Name,
                    begindate = item.Date
                });
            }

            //给前台所需数据赋值
            var gridData = new
            {
                gdata = atInfo,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + Attending.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);

        }

        //审核通过
        public ActionResult pass()
        {
            string idString = Request["HumanInfo.Id"];
            Guid Id = new Guid(idString);
            AttendingInfo atInfo = db.AttendingInfo.Find(Id);
            atInfo.IsPass = true;
            db.Entry(atInfo).State = EntityState.Modified;
            db.SaveChanges();
            return Json(true);
        }
        //审核不通过
        public ActionResult notPass()
        {
            string idString = Request.Params["HumanInfo.Id"];
            Guid Id = new Guid(idString);
            AttendingInfo atInfo = db.AttendingInfo.Find(Id);
            atInfo.IsPass = false;
            db.Entry(atInfo).State = EntityState.Modified;
            db.SaveChanges();
            return Json(true);
        }

        #region 已审核
        //已审核页面
        [Management.filter.LoginFilter]
        public ActionResult Audited()
        {
            return View();
        }
        //已审核页面资料获取
        public ActionResult managerAuditedWorkInfo()
        {
            try
            {
                IList<getAuditedInfo> auditedList = new List<getAuditedInfo>();
                //从数据库张获取表
                List<HumanBasicFile> hbList = db.HumanBasicFile.ToList();
                List<AttendingInfo> adInfoList = db.AttendingInfo.ToList();
                //获取非草稿箱状态
                hbList = hbList.Where(a => a.IsDraft == false).ToList();
                adInfoList = adInfoList.Where(a => a.IsDraft == false).ToList();

                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);

                string auditedDate = Request.Params["content"];
                string year = auditedDate.Substring(0, 4);//获取年份数值
                string month = auditedDate.Substring(5, 2);//获取月份数值
                string day = auditedDate.Substring(8, 2);//获取日的数值

                //过滤条件
                adInfoList = adInfoList.Where(a => a.Date.Substring(0, 4) == year).ToList();//获取相同的年
                adInfoList = adInfoList.Where(a => a.Date.Substring(5, 2) == month).ToList();//获取相同月份

                var adhumidList = (from pp in adInfoList select pp.HumanId).Distinct().ToList();
                foreach (var item in adInfoList)
                {
                    foreach (var hid in adhumidList)
                    {
                        if (hid != item.HumanId)
                        {
                            continue;
                        }
                        else
                        {
                            int workDays = adInfoList.Where(a => a.HumanId == hid).Count();
                            int money = 0;
                            var salary = (from sa in adInfoList where sa.HumanId == hid select sa.StandardSalary).ToList();
                            foreach (var m in salary)
                            {
                                if (m != null)
                                {
                                    money += (int)m;//m不为空时算工资
                                }
                            }
                            var fee = (from sa in adInfoList where sa.HumanId == hid select sa.BearFees).ToList();
                            foreach (var m in fee)
                            {
                                if (m != null)
                                {
                                    money += (int)m;//m不为空时算补助
                                }
                            }

                            string beginDate = (from bgd in adInfoList where bgd.HumanId == hid select bgd.Date).First().ToString();
                            string lastDate = (from bgd in adInfoList where bgd.HumanId == hid select bgd.Date).Last().ToString();
                            var hb = hbList.Where(a => a.Id == hid);
                            foreach (var id in hb)
                            {
                                auditedList.Add(new getAuditedInfo()
                                {
                                    begindate = beginDate,
                                    enddate = lastDate,
                                    sumwork = workDays.ToString(),
                                    city = id.City1.Name,
                                    identitys = id.IDcardNo,
                                    name = id.Name,
                                    uniquenum = id.uniNum,
                                    sumwage = money.ToString()
                                });
                            }

                        }

                    }
                }

               

                var list = (from al in auditedList
                            select new
                            {
                                al.uniquenum,
                                al.name,
                                al.city,
                                al.sumwage,
                                al.sumwork,
                                al.identitys,
                                al.begindate,
                                al.enddate
                            }).Distinct().ToList();

                //总页数
                int totalPage = list.Count() / pagesize;
                if (list.Count() % pagesize != 0)
                { totalPage++; }
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

                //给前台所需数据赋值
                var gridData = new
                {
                    gdata = list,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + list.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }
            catch
            {
                return View("Audited");
            }
        }


        #endregion

        #region 显示人员相关信息
        //显示人员相关详细信息
        [Management.filter.LoginFilter]
        public ActionResult showMan(string uniquenum, string begin, string end)
        {
            string id_string = (from humid in db.HumanBasicFile.ToList() where humid.uniNum == uniquenum select humid.Id.ToString()).First().ToString();
            Guid id = new Guid(id_string);
            List<AttendingInfo> humanAttendInfo = (from hum in db.AttendingInfo.ToList() where hum.HumanId == id select hum).ToList();
            List<AttendingInfo> human_time = humanAttendInfo.Where(a => Convert.ToDateTime(a.Date) >= Convert.ToDateTime(begin)).ToList();
            human_time = human_time.Where(a => Convert.ToDateTime(a.Date) <= Convert.ToDateTime(end)).ToList();
            ViewBag.humInfo = human_time;
            return View();

        }

        #endregion
    }
}