using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;

namespace Management.Controllers
{
    public class CheckWorkController : Controller
    {
        //
        // GET: /CheckWork/

        SS_HRM_DBEntities ss = new SS_HRM_DBEntities();

        //表中的列
        class Node
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
            public string activity { get; set; }
            public string product { get; set; }
            public string store { get; set; }
            public string work { get; set; }
            public int wage { get; set; }
            public int subsidy { get; set; }
            public string remark { get; set; }
            public string date { get; set; }
        }

        #region 督导上班信息界面+DudaoWork()
        [Management.filter.LoginFilter]
        public ActionResult DudaoWork()
        {

            return View();
        } 
        #endregion

        #region 获取督导上班信息+GetData()
        public JsonResult GetData()
        {
            string user = Session["admin"].ToString();
            Managers mt=(from d in ss.Managers where  d.UserId==user select d  ).FirstOrDefault();
            Guid userid=mt.Id;
            string usersname = mt.Name;
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //第一行行号
            int startRow = pagesize*(page-1);
            IList<Node> list = new List<Node>();
            //当前页展示的记录
            List<AttendingInfo> list1 =((from d in ss.AttendingInfo orderby d.Date descending where d.HumanId==userid select d).Skip(startRow).Take(pagesize)).ToList();
            //该表符合条件的总记录
            List<AttendingInfo> list2 = (from d in ss.AttendingInfo orderby d.Date descending where d.HumanId == userid select d).ToList();
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
                list.Add(new Node()
                {
                    id = item.Id,
                    username = user ,
                    name = usersname,
                    activity = item.ActionName,
                    product = item.production,
                    store = item.SShop.Name,
                    work = item.Functions,
                    wage = (int)item.StandardSalary,
                    subsidy = (int)item.BearFees,
                    remark = item.Remark,
                    date = item.Date

                });
            }
            var griddata = new
            {
                gdata = list,
                page = page,
                totalPage = totalpage,
                pageBtn = pageBtnClass,
                pageMsg = "当前第" + page + "页，共" + totalpage + "页，共" + totalcout + "条记录，每页显示"+pagesize+"条记录",
                pageSize = pagesize
            };
            return Json(griddata);
        } 
        #endregion

        #region 根据id删除督导上班信息+DeletebyDdId()
        public JsonResult DeletebyDdId()
        {
            bool success = true;
            string message = string.Empty;
            try
            {
                Guid id = new Guid(Request.Params["supervisorWorkInfo.id"]);
                AttendingInfo ato = ss.AttendingInfo.Find(id);
                ss.AttendingInfo.Remove(ato);
                ss.SaveChanges();
                
            }
            catch
            {
                success = false;
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        } 
        #endregion

        #region 队长上班信息界面+DuizhangWork()
        [Management.filter.LoginFilter]
        public ActionResult DuizhangWork()
        {
            return View();
        } 
        #endregion

        #region 获取队长上班信息+GetData()
        public JsonResult GetData1()
        {
            string user = Session["admin"].ToString();
            List<AttendingInfo> lists=new List<AttendingInfo>();
            List<Managers> mt = (from d in ss.Managers where d.CreatedManId == user && d.Authority == 2 select d).ToList();
            foreach(var item in mt)//遍历队长集合
            {
                Guid userid = item.Id;//上班人员id
                List<AttendingInfo> list2 = (from d in ss.AttendingInfo orderby d.Date descending where d.HumanId == userid select d).ToList();
                lists.AddRange(list2);

            }

            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //第一行行号
            int startRow = pagesize * (page - 1);
            IList<Node> list = new List<Node>();
            //当前页展示的记录
            List<AttendingInfo> list1 = (lists.OrderByDescending(d=>d.Date).Skip(startRow).Take(pagesize)).ToList();
            //总记录数
            int totalcout = lists.Count;
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
                Managers mti = (from d in ss.Managers where d.Id==item.HumanId select d).FirstOrDefault();
                string usernames = mti.UserId;
                string usersname = mti.Name;//上班人员name
                list.Add(new Node()
                {
                    id = item.Id,
                    username =usernames,
                    name = usersname,
                    activity = item.ActionName,
                    product = item.production,
                    store = item.SShop.Name,
                    work = item.Functions,
                    wage = (int)item.StandardSalary,
                    subsidy = (int)item.BearFees,
                    remark = item.Remark,
                    date = item.Date

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

        #region 根据id删除队长上班信息+DeletebyDzId()
        public JsonResult DeletebyDzId()
        {
            bool success = true;
            string message = string.Empty;
            try
            {
                Guid id = new Guid(Request.Params["captainWorkInfo.id"]);
                AttendingInfo ato = ss.AttendingInfo.Find(id);
                ss.AttendingInfo.Remove(ato);
                ss.SaveChanges();

            }
            catch
            {
                success = false;
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
}
