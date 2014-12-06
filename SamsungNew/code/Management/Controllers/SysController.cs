using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Data.Entity;
using System.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.GZip;

namespace Management.Controllers
{
    public class SysController : Controller
    {
        private SS_HRM_DBEntities db = new SS_HRM_DBEntities();
        //
        // GET: /Sys/

        #region 获取基本信息

        class getCondition
        {
            public string city { get; set; }
            public string attend_count { get; set; }
            public string human_count { get; set; }
            public string xc_count { get; set; }
            public string xc_monthActive { get; set; }
            public string xc_threeMonthActive { get; set; }
            public string lc_count { get; set; }
            public string lc_monthActive { get; set; }
            public string lc_threeMonthActive { get; set; }
            public string xdz_count { get; set; }
            public string xdz_monthActive { get; set; }
            public string xdz_threeMonthActive { get; set; }
            public string S { get; set; }
            public string A { get; set; }
            public string B { get; set; }
        }

        class getpeople
        {
            public string uniNum { get; set; }
            public string level { get; set; }
            public string functions { get; set; }
            public int city { get; set; }
            public string date { get; set; }
        }

        class getActionInfo
        {
            public string office { get; set; }
            public string city { get; set; }
            public string action { get; set; }
            public string lc_count { get; set; }
            public string lc_joincount { get; set; }
            public string lc_money { get; set; }
            public string xc_count { get; set; }
            public string xc_joincount { get; set; }
            public string xc_money { get; set; }
            public string xdz_count { get; set; }
            public string xdz_joincount { get; set; }
            public string xdz_money { get; set; }
            public string totalmoney { get; set; }

        }

        class getpeople2
        {
            public string human { get; set; }
            public string office { get; set; }
            public string city { get; set; }
            public string actionName { get; set; }
            public string function { get; set; }
            public string date { get; set; }
            public int salary { get; set; }
        }

        class getFeedback
        {
            public int id { get; set; }
            public string title { get; set; }
            public string createtime { get; set; }
            public string createmanager { get; set; }
            public string replymanager { get; set; }
        }

        class getLoginlog //登陆信息
        {
            public int id { get; set; }
            public string username { get; set; }
            public string ip { get; set; }
            public string date { get; set; }
        }
        #endregion

        #region 人员数据汇总
        //获取办事处人员数据库总览
        public ActionResult checkConditionInfo()
        {
            //新建一个基本信息对象。
            IList<getCondition> conInfo = new List<getCondition>();
            IList<getpeople> peo = new List<getpeople>();
            //从数据库中获取表
            List<HumanBasicFile> humanData = db.HumanBasicFile.ToList();
            List<AttendingInfo> attendingInfo = db.AttendingInfo.ToList();
            //获取非草稿箱状态的数据表
            humanData = humanData.Where(a => a.IsDraft == false).ToList();
            attendingInfo = attendingInfo.Where(a => a.IsDraft == false).ToList();

            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //选取类型
            string Citycontent = Request.Params["content"];   //城市
            string Officecontent = Request.Params["office"]; //办事处
            List<City> cityList = new List<City>();

            if (Session["authority"].ToString() == "督导")
            {
                Managers man = (from m in db.Managers.ToList() where m.UserId == Session["admin"].ToString() select m).FirstOrDefault();
                var AdminOffice = (from office in db.Managers.ToList() where office.Authority1.Name == Session["authority"].ToString() && office.City1.OfficeId == man.City1.OfficeId select office.City1.Office.Name).FirstOrDefault().ToString();    //获取督导办事处
                cityList = (from city in db.City.ToList() where city.Office.Name == AdminOffice select city).ToList();
            }
            else if (Session["authority"].ToString() == "小队长")
            {
                //获取小队长的上级
                string Boss = (from b in db.Managers.ToList() where b.UserId == Session["admin"].ToString() select b.Boss).FirstOrDefault();
                Managers man = (from m in db.Managers.ToList() where m.UserId == Boss select m).FirstOrDefault();
                var AdminOffice = (from office in db.Managers.ToList() where office.Authority1.Name == man.Authority1.Name && office.City1.OfficeId == man.City1.OfficeId select office.City1.Office.Name).FirstOrDefault().ToString();    //获取小队长办事处
                cityList = (from city in db.City.ToList() where city.Office.Name == AdminOffice select city).ToList();
            }
            else if (Session["authority"].ToString() == "管理员")
            {
                cityList = (from city in db.City.ToList() select city).ToList();
            }
            if(!(string.IsNullOrEmpty(Officecontent)))
            {
                cityList=cityList.Where(a=>a.Office.Name==Officecontent).ToList();
            }
            //过滤开始
            if (string.IsNullOrEmpty(Citycontent))  //显示所有城市
            {


                foreach (var cityid in cityList)   //遍历城市
                {
                    var shopNamelist = (from shop in db.SShop.ToList() where shop.CityId == cityid.Id select shop.Id).ToList();
                    foreach (var shopname in shopNamelist)//遍历城市中商店
                    {
                        var humanAttend = (from i in attendingInfo where i.Department == shopname select i).ToList();
                        foreach (var people in humanAttend)  //遍历商店中人员
                        {
                            
                                //获取等级
                                var rank = (from h in humanData where h.IsDraft == false && h.Id == people.HumanId select h.HumanLevel).FirstOrDefault().ToString();
                                //增加记录
                                peo.Add(new getpeople()
                                {
                                    uniNum = people.HumanBasicFile.uniNum,
                                    level = rank,
                                    functions = people.Functions,
                                    city = cityid.Id,
                                    date = people.Date
                                });
                            
                        }
                    }
                }

                foreach (var item in cityList)
                {
                    var cityname = (from city in db.City.ToList() where city.Id == item.Id select city.Name).FirstOrDefault().ToString();//获取城市名
                    conInfo.Add(new getCondition()
                    {
                        city = cityname,
                        human_count = humanData.Where(c => c.City1.Id == item.Id).Select(b => b.uniNum).Distinct().Count().ToString(),
                        attend_count = attendingInfo.Where(a => a.SShop.City.Id == item.Id).Select(b => b.HumanBasicFile.uniNum).Distinct().Count().ToString(),
                        xc_count = peo.Where(a => a.city == item.Id && a.functions == "形促").Select(a => a.uniNum).Distinct().Count().ToString(),
                        xc_monthActive = peo.Where(a => a.functions == "形促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xc_threeMonthActive = peo.Where(a => a.functions == "形促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_count = peo.Where(a => a.city == item.Id && a.functions == "临促").Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_monthActive = peo.Where(a => a.functions == "临促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_threeMonthActive = peo.Where(a => a.functions == "临促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_count = peo.Where(a => a.city == item.Id && a.functions == "小队长").Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_monthActive = peo.Where(a => a.functions == "小队长" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_threeMonthActive = peo.Where(a => a.functions == "小队长" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        S = humanData.Where(a => a.HumanLevel == "S" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                        A = humanData.Where(a => a.HumanLevel == "A" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                        B = humanData.Where(a => a.HumanLevel == "B" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                    });
                }
                #region 汇总
                string humtotal = conInfo.Sum(a => Convert.ToInt32(a.human_count)).ToString();
                string attendtotal = conInfo.Sum(a => Convert.ToInt32(a.attend_count)).ToString();
                string xctotal = conInfo.Sum(a => Convert.ToInt32(a.xc_count)).ToString();
                string xconetotal = conInfo.Sum(a => Convert.ToInt32(a.xc_monthActive)).ToString();
                string xcthreetotal = conInfo.Sum(a => Convert.ToInt32(a.xc_threeMonthActive)).ToString();
                string lctotal = conInfo.Sum(a => Convert.ToInt32(a.lc_count)).ToString();
                string lconetotal = conInfo.Sum(a => Convert.ToInt32(a.lc_monthActive)).ToString();
                string lcthreetotal = conInfo.Sum(a => Convert.ToInt32(a.lc_threeMonthActive)).ToString();
                string xdztotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_count)).ToString();
                string xdzonetotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_monthActive)).ToString();
                string xdzthreetotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_threeMonthActive)).ToString();
                string Stotal = conInfo.Sum(a => Convert.ToInt32(a.S)).ToString();
                string Atotal = conInfo.Sum(a => Convert.ToInt32(a.A)).ToString();
                string Btotal = conInfo.Sum(a => Convert.ToInt32(a.B)).ToString();

                #endregion

                //总页数
                int totalPage = cityList.Count() / pagesize;
                if (cityList.Count() % pagesize != 0)
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
                conInfo = conInfo.Skip(pagesize * (page - 1)).Take(pagesize).ToList();
                conInfo.Add(new getCondition()
                {
                    city = "总人数",
                    human_count = humtotal,
                    attend_count = attendtotal,
                    xc_count = xctotal,
                    xc_monthActive = xconetotal,
                    xc_threeMonthActive = xcthreetotal,
                    lc_count = lctotal,
                    lc_monthActive = lconetotal,
                    lc_threeMonthActive = lcthreetotal,
                    xdz_count = xdztotal,
                    xdz_monthActive = xdzonetotal,
                    xdz_threeMonthActive = xdzthreetotal,
                    S = Stotal,
                    A = Atotal,
                    B = Btotal

                });

                //给前台所需数据赋值
                var gridData = new
                {
                    gdata = conInfo,
                    humtotal = humtotal,
                    attendtotal = attendtotal,
                    xctotal = xctotal,
                    xconetotal = xconetotal,
                    xcthreetotal = xcthreetotal,
                    lctotal = lctotal,
                    lconetotal = lconetotal,
                    lcthreetotal = lcthreetotal,
                    xdztotal = xdztotal,
                    xdzonetotal = xdzonetotal,
                    xdzthreetotal = xdzthreetotal,
                    Stotal = Stotal,
                    Atotal = Atotal,
                    Btotal = Btotal,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + cityList.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }

            else
            {
                var city1 = cityList.Where(a => a.Name.Contains(Citycontent)).ToList();//获取城市名

                foreach (var cityid in city1)   //遍历城市
                {
                    var shopNamelist = (from shop in db.SShop.ToList() where shop.CityId == cityid.Id select shop.Id).ToList();
                    foreach (var shopname in shopNamelist)//遍历城市中商店
                    {

                        var humanAttend = (from i in attendingInfo where i.Department == shopname select i).ToList();
                        foreach (var people in humanAttend)  //遍历商店中人员
                        {
                            
                                //获取等级
                                var rank = (from h in humanData where h.IsDraft == false && h.Id == people.HumanId select h.HumanLevel).FirstOrDefault().ToString();
                                //增加记录
                                peo.Add(new getpeople()
                                {
                                    uniNum = people.HumanBasicFile.uniNum,
                                    level = rank,
                                    functions = people.Functions,
                                    city = cityid.Id,
                                    date = people.Date
                                });
                            
                        }
                    }

                }

                foreach (var item in city1)
                {
                    var cityname = (from city in db.City.ToList() where city.Id == item.Id select city.Name).FirstOrDefault().ToString();//获取城市名
                    conInfo.Add(new getCondition()
                    {
                        city = cityname,
                        human_count = humanData.Where(c => c.City1.Id == item.Id).Select(b => b.uniNum).Distinct().Count().ToString(),
                        attend_count = attendingInfo.Where(a => a.SShop.City.Id == item.Id).Select(b => b.HumanBasicFile.uniNum).Distinct().Count().ToString(),
                        xc_count = peo.Where(a => a.city == item.Id && a.functions == "形促").Select(a => a.uniNum).Distinct().Count().ToString(),
                        xc_monthActive = peo.Where(a => a.functions == "形促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xc_threeMonthActive = peo.Where(a => a.functions == "形促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_count = peo.Where(a => a.city == item.Id && a.functions == "临促").Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_monthActive = peo.Where(a => a.functions == "临促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        lc_threeMonthActive = peo.Where(a => a.functions == "临促" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_count = peo.Where(a => a.city == item.Id && a.functions == "小队长").Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_monthActive = peo.Where(a => a.functions == "小队长" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        xdz_threeMonthActive = peo.Where(a => a.functions == "小队长" && a.city == item.Id && DateTime.Parse(a.date) >= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-3).ToShortDateString())).Select(a => a.uniNum).Distinct().Count().ToString(),
                        S = peo.Where(a => a.level == "S" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                        A = peo.Where(a => a.level == "A" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                        B = peo.Where(a => a.level == "B" && a.city == item.Id).Select(a => a.uniNum).Distinct().Count().ToString(),
                    });
                }

                //总页数
                int totalPage = city1.Count() / pagesize;
                if (city1.Count() % pagesize != 0)
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

                #region 汇总
                string humtotal = conInfo.Sum(a => Convert.ToInt32(a.human_count)).ToString();
                string attendtotal = conInfo.Sum(a => Convert.ToInt32(a.attend_count)).ToString();
                string xctotal = conInfo.Sum(a => Convert.ToInt32(a.xc_count)).ToString();
                string xconetotal = conInfo.Sum(a => Convert.ToInt32(a.xc_monthActive)).ToString();
                string xcthreetotal = conInfo.Sum(a => Convert.ToInt32(a.xc_threeMonthActive)).ToString();
                string lctotal = conInfo.Sum(a => Convert.ToInt32(a.lc_count)).ToString();
                string lconetotal = conInfo.Sum(a => Convert.ToInt32(a.lc_monthActive)).ToString();
                string lcthreetotal = conInfo.Sum(a => Convert.ToInt32(a.lc_threeMonthActive)).ToString();
                string xdztotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_count)).ToString();
                string xdzonetotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_monthActive)).ToString();
                string xdzthreetotal = conInfo.Sum(a => Convert.ToInt32(a.xdz_threeMonthActive)).ToString();
                string Stotal = conInfo.Sum(a => Convert.ToInt32(a.S)).ToString();
                string Atotal = conInfo.Sum(a => Convert.ToInt32(a.A)).ToString();
                string Btotal = conInfo.Sum(a => Convert.ToInt32(a.B)).ToString();

                #endregion


                conInfo = conInfo.Skip(pagesize * (page - 1)).Take(pagesize).ToList();
                conInfo.Add(new getCondition()
                {
                    city = "总人数",
                    human_count = humtotal,
                    attend_count = attendtotal,
                    xc_count = xctotal,
                    xc_monthActive = xconetotal,
                    xc_threeMonthActive = xcthreetotal,
                    lc_count = lctotal,
                    lc_monthActive = lconetotal,
                    lc_threeMonthActive = lcthreetotal,
                    xdz_count = xdztotal,
                    xdz_monthActive = xdzonetotal,
                    xdz_threeMonthActive = xdzthreetotal,
                    S = Stotal,
                    A = Atotal,
                    B = Btotal

                });
                var gridData = new
                {
                    gdata = conInfo,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + city1.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }

        }

        //自动搜索办事处(数据汇总)
        public IEnumerable<Office> OfficeList()
        {
            List<Office> officeList = (from s in db.Office select s).ToList();
            return officeList.AsEnumerable();
        }

        public ActionResult autoComplete_Office(string OfficeInput = "")
        {
            var formateData = OfficeList().Where(x => x.Name.Contains(OfficeInput));


            //把办事处以字符串形式串起来
            string s = "";
            int num = 0;
            foreach (var i in formateData)
            {
                if (formateData.Count() <= 0)
                    return Json(s);
                else if (num < formateData.Count() - 1)
                {
                    if (num == 0)
                    {
                        s += "[{OfficeName:\"" + i.Name + "\",OfficeMask:\"" + i.mask + "\"},";
                        num++;
                    }
                    else
                    {
                        s += "{OfficeName:\"" + i.Name + "\",OfficeMask:\"" + i.mask + "\"},";
                        num++;
                    }
                }
                else { s += "{OfficeName:\"" + i.Name + "\",OfficeMask:\"" + i.mask + "\"}]"; }
            }

            var data = new { s = s, count = formateData.Count() };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //自动搜索城市(数据汇总)
        public IEnumerable<City> CityList()
        {
           
            
            List<City> cityList = (from s in db.City select s).ToList();
            return cityList.AsEnumerable();
        }

        public ActionResult autoComplete_City(string CityInput = "")
        {
            string OfficeName = Request.Params["officename"].ToString();
            
            var formateData = CityList().Where(x => x.Name.Contains(CityInput));
            if (!String.IsNullOrEmpty(OfficeName))
            {
                formateData = formateData.Where(a => a.Office.Name == OfficeName);
            }


            //把办事处以字符串形式串起来
            string s = "";
            int num = 0;
            foreach (var i in formateData)
            {
                if (formateData.Count() <= 0)
                {
                    
                    return Json(s);
                }
                    
                else if (num < formateData.Count() - 1)
                {
                    if (num == 0)
                    {
                        s += "[{CityName:\"" + i.Name + "\",CityId:\"" + i.Id + "\"},";
                        num++;
                    }
                    else
                    {
                        s += "{CityName:\"" + i.Name + "\",CityId:\"" + i.Id + "\"},";
                        num++;
                    }
                }
                else { s += "{CityName:\"" + i.Name + "\",CityId:\"" + i.Id + "\"}]"; }
            }
            if (string.IsNullOrEmpty(s))
            {
                s += "[{CityName:\" \",CityId:\" \"}]";
            }
            var data = new { s = s, count = formateData.Count() };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //获取办事处活动人次总览
        public ActionResult checkActionInfo()
        {
            //新建一个基本信息对象。
            IList<getActionInfo> ActionInfo = new List<getActionInfo>();
            IList<getpeople2> people = new List<getpeople2>();
            //从数据库中获取表
            List<HumanBasicFile> humanData = db.HumanBasicFile.ToList();
            List<AttendingInfo> attendingInfo = db.AttendingInfo.ToList();
            //获取非草稿箱状态的数据表
            humanData = humanData.Where(a => a.IsDraft == false).ToList();
            attendingInfo = attendingInfo.Where(a => a.IsDraft == false).ToList();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //选取类型
            string actioncontent = Request.Params["content"];   //活动名称
            string actiontime = Request.Params["actionTime"];   //时间
            string timebegin = "";                           //月头
            string timeEnd = "";                             //月尾
            if (string.IsNullOrEmpty(actiontime))
            {
                timebegin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).ToShortDateString();
                timeEnd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString();
            }
            else
            {
                timebegin = DateTime.Parse(actiontime).ToShortDateString();
                timeEnd = DateTime.Parse(actiontime).AddMonths(1).ToShortDateString();
            }

            List<City> citylist = new List<City>();

            if (Session["authority"].ToString() == "督导")
            {
                Managers man = (from m in db.Managers.ToList() where m.UserId == Session["admin"].ToString() select m).FirstOrDefault();
                var AdminOffice = (from office in db.Managers.ToList() where office.Authority1.Name == Session["authority"].ToString() && office.City1.OfficeId == man.City1.OfficeId select office.City1.Office.Name).FirstOrDefault().ToString();    //获取督导办事处              
                attendingInfo = attendingInfo.Where(a => a.SShop.City.Office.Name == AdminOffice).ToList();
                citylist = (from city in db.City.ToList() where city.Office.Name == AdminOffice select city).ToList();
            }
            else if (Session["authority"].ToString() == "小队长")
            {
                //获取小队长的上级
                string Boss = (from b in db.Managers.ToList() where b.UserId == Session["admin"].ToString() select b.Boss).FirstOrDefault();
                Managers man = (from m in db.Managers.ToList() where m.UserId == Boss select m).FirstOrDefault();
                var AdminOffice = (from office in db.Managers.ToList() where office.Authority1.Name == man.Authority1.Name && office.City1.OfficeId == man.City1.OfficeId select office.City1.Office.Name).FirstOrDefault().ToString();    //获取小队长上级办事处              
                attendingInfo = attendingInfo.Where(a => a.SShop.City.Office.Name == AdminOffice).ToList();
            }
            else if (Session["authority"].ToString() == "管理员")
            {
                citylist = (from city in db.City.ToList() select city).ToList();
            }
            else
            {
                return View("~/Views/Shared/AuthorityError.cshtml");
            }
            var actionname = (from e in attendingInfo select e.ActionName).ToList().Distinct().ToList();
            if (!string.IsNullOrEmpty(actioncontent))
            {
                actionname = actionname.Where(a => a.Contains(actioncontent)).ToList();
            }
            foreach (var item in attendingInfo)
            {
                int bearFees = item.BearFees != null ?(int) item.BearFees : 0;
                people.Add(new getpeople2()
                {
                    actionName = item.ActionName,
                    city = item.SShop.City.Name,
                    office = item.SShop.City.Office.Name,
                    function = item.Functions,
                    human = item.HumanBasicFile.uniNum,
                    date = item.Date,
                    salary = (int)item.StandardSalary + bearFees
                });
            }
            foreach (var an in actionname)
            {
                foreach (var item in citylist)
                {

                    if (people.Where(a => a.actionName == an && a.city == item.Name && DateTime.Parse(a.date) >= DateTime.Parse(timebegin) && DateTime.Parse(a.date) <= DateTime.Parse(timeEnd)).FirstOrDefault() != null)
                    {
                        ActionInfo.Add(new getActionInfo()
                            {
                                action = an,
                                city = item.Name,
                                office = item.Office.Name,
                                lc_count = attendingInfo.Where(a => a.Functions == "临促" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Distinct().Count().ToString(),
                                lc_joincount = attendingInfo.Where(a => a.Functions == "临促" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Count().ToString(),
                                lc_money = ((int)attendingInfo.Where(a => a.Functions == "临促" && a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.StandardSalary) + (int)attendingInfo.Where(a => a.Functions == "临促" && a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.BearFees)).ToString(),
                                xc_count = attendingInfo.Where(a => a.Functions == "形促" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Distinct().Count().ToString(),
                                xc_joincount = attendingInfo.Where(a => a.Functions == "形促" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Count().ToString(),
                                xc_money = ((int)attendingInfo.Where(a => a.Functions == "形促" && a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.StandardSalary) + (int)attendingInfo.Where(a => a.Functions == "形促" && a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.BearFees)).ToString(),
                                xdz_count = attendingInfo.Where(a => a.Functions == "小队长" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Distinct().Count().ToString(),
                                xdz_joincount = attendingInfo.Where(a => a.Functions == "小队长" && a.ActionName == an && a.SShop.City.Name == item.Name).Select(a => a.HumanBasicFile.uniNum).Count().ToString(),
                                xdz_money = attendingInfo.Where(a => a.Functions == "小队长" && a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.StandardSalary).ToString(),
                                totalmoney = ((int)attendingInfo.Where(a => a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.StandardSalary) + (int)attendingInfo.Where(a => a.ActionName == an && a.SShop.City.Name == item.Name).Sum(a => a.BearFees)).ToString()

                            });
                    }
                }
            }



            //总页数
            int totalPage = ActionInfo.Count() / pagesize;
            int totalContent = ActionInfo.Count();
            if (attendingInfo.Count() % pagesize != 0)
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

            ActionInfo = ActionInfo.Skip(pagesize * (page - 1)).Take(pagesize).ToList();
            //给前台所需数据赋值
            var gridData = new
            {
                gdata = ActionInfo,
                page = page,
                showTime = DateTime.Parse(timebegin).ToString("yyyy年MM月"),
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + totalContent.ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);
        }



        [Management.filter.LoginFilter]
        public ActionResult Condition()
        {

            List<Office> office = db.Office.ToList();
            ViewBag.Office = office;

            return View();
        }

        #endregion

        #region 导出
        [Management.filter.LoginFilter]
        public ActionResult ExportInfo()
        {
            return View();
        }

        public void ZipFilenew(string strFileFolder, string strZip)
        {
            if (strFileFolder[strFileFolder.Length - 1] != Path.DirectorySeparatorChar)
                strFileFolder += Path.DirectorySeparatorChar;
            ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(strZip));
            s.SetLevel(9); // 0 - store only to 9 - means best compression  
            zip(strFileFolder, strZip, s);
            s.Finish();
            s.Close();
        }  //压缩图片

        private void zip(string strFile, string StoreFile, ZipOutputStream s)  //压缩图片
        {
            // 出现乱码就是因为CodePage不对

            Encoding gbk = Encoding.GetEncoding("gbk");

            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = gbk.CodePage; 

            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar)
                strFile += Path.DirectorySeparatorChar;
            Crc32 crc = new Crc32();
            string[] filenames = Directory.GetFileSystemEntries(strFile);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    zip(file, StoreFile, s);

                }
                else
                {
                    //打开压缩文件,开始压缩  
                    FileStream fs = System.IO.File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    string tempfile = file.Substring(StoreFile.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(tempfile);
                    
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void DeleteFolder(string dir)
        {

            //如果存在这个文件夹删除之 

            if (Directory.Exists(dir))
            {
                string[] picList = Directory.GetFiles(dir, "*.jpg");
                string[] zipList = Directory.GetFiles(Server.MapPath("~/uploadImg/"), "*.zip");
                foreach (string f in picList)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(f);
                    fi.Delete();
                }
                foreach (string f in zipList)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(f);
                    fi.Delete();
                }
                //catch (System.IO.IOException e)
                //{
                //    Console.WriteLine(e.Message);
                //}
                //foreach (string d in Directory.GetFileSystemEntries(dir))
                //{

                //    //if (File.Exists(d))

                //    //    File.Delete(d);//直接删除其中的文件 

                //     DeleteFolder(d);//递归删除子文件夹  

                //}

                //Directory.Delete(dir);

                //删除已空文件夹 

                //Response.Write(dir + "文件夹删除成功");

            }

            //else //如果文件夹不存在则提示 

            //    Response.Write(dir + "该文件夹不存在");

        }//删除文件


        static void CopyFolderTo(string directorySource, string directoryTarget)
        {
            //检查是否存在目的目录  
            if (!Directory.Exists(directoryTarget))
            {
                Directory.CreateDirectory(directoryTarget);
            }
            FileInfo file = new FileInfo(directorySource);
            file.CopyTo(Path.Combine(directoryTarget, file.Name),true);
            #region 整个文件夹 //先来复制文件
            //DirectoryInfo directoryInfo = new DirectoryInfo(directorySource);
            //FileInfo[] files = directoryInfo.GetFiles();
            ////Directory.GetFiles(dir, "*.jpg");
            ////复制所有文件  
            //foreach (FileInfo file in files)
            //{
            //    file.CopyTo(Path.Combine(directoryTarget, file.Name));
            //}
            #endregion
            //最后复制目录  
            //DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories();
            //foreach (DirectoryInfo dir in directoryInfoArray)
            //{
            //    CopyFolderTo(Path.Combine(directorySource, dir.Name), Path.Combine(directoryTarget, dir.Name));
            //}

        }

        [HttpPost]
        public ActionResult dctupian1()//导出图片
        {
            var office = Request.Params.Get("officeValue");//办事处
            var type = Request.Params.Get("typeValue");//导出类型
            DateTime begin = Convert.ToDateTime(Request.Params.Get("beginValue"));//起始时间
            DateTime end = Convert.ToDateTime(Request.Params.Get("endValue")).AddDays(1);//结束时间
            #region 基本信息图片
            if (type == "基本信息")
            {
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/identityImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/bankImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/InterviewImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/InfoImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/work/"));
                List<HumanBasicFile> hum=db.HumanBasicFile.Where(a=>a.createTime<end&&a.createTime>begin).ToList();
                if (office != "全部")
                {
                    hum = (from s in db.HumanBasicFile where s.City1.Office.Name == office select s).Where(a => a.createTime < end && a.createTime > begin).ToList();
                }
                if (hum != null)
                {
                    foreach (var item in hum)
                    {
                        FileInfo file = new FileInfo(Server.MapPath("~/uploadImg/Humanbasic/identityImg/" + item.IDcardPhoto));
                        if (file.Exists == true)
                        {
                            CopyFolderTo(Server.MapPath("~/uploadImg/Humanbasic/identityImg/" + item.IDcardPhoto), Server.MapPath("~/uploadImg/linshi/identityImg/"));
                        }
                        FileInfo file1 = new FileInfo(Server.MapPath("~/uploadImg/Humanbasic/bankImg/" + item.BankCardPhoto));
                        if (file1.Exists == true)
                        {
                            CopyFolderTo(Server.MapPath("~/uploadImg/Humanbasic/bankImg/" + item.BankCardPhoto), Server.MapPath("~/uploadImg/linshi/bankImg/"));
                        }
                        if (item.InterfacePhoto != null)
                        {
                            FileInfo file11 = new FileInfo(Server.MapPath("~/uploadImg/Humanbasic/InterviewImg/" + item.InterfacePhoto));
                            if (file11.Exists == true)
                            {
                                CopyFolderTo(Server.MapPath("~/uploadImg/Humanbasic/InterviewImg/" + item.InterfacePhoto), Server.MapPath("~/uploadImg/linshi/InterviewImg/"));
                            }
                        }
                        FileInfo file2 = new FileInfo(Server.MapPath("~/uploadImg/Humanbasic/InfoImg/" + item.Info));
                        if (file2.Exists == true)
                        {
                            CopyFolderTo(Server.MapPath("~/uploadImg/Humanbasic/InfoImg/" + item.Info), Server.MapPath("~/uploadImg/linshi/InfoImg/"));
                        }
                    }
                }
                string ss = "压缩成功";
                ZipFilenew(Server.MapPath("~/uploadImg/linshi/"), Server.MapPath("~/uploadImg/Humanbasic.zip"));
                var path = Server.MapPath("~/uploadImg/Humanbasic.zip");//下载图片
                //d.Delete();
                return File(path, "application/x-zip-compressed", "Humanbasic.Zip");
            }
            #endregion
            #region 上班信息图片
            if (type == "上班信息")
            {
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/identityImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/bankImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/InterviewImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/InfoImg/"));
                DeleteFolder(Server.MapPath("~/uploadImg/linshi/work/"));
                List<AttendingInfo> hum = db.AttendingInfo.ToList();
                if(office != "全部")
                {
                  hum = (from s in db.HumanBasicFile
                               from g in db.AttendingInfo
                               where s.City1.Office.Name == office && g.HumanId == s.Id
                               select g).ToList();
                }
                hum = hum.Where(a => (Convert.ToDateTime(a.Date) > begin) && (Convert.ToDateTime(a.Date) < end)).ToList();
                if (hum != null)
                {
                    foreach (var item in hum)
                    {
                        FileInfo file = new FileInfo("~/uploadImg/workimage/work/" + item.WorkPhoto);
                        if (file.Exists == true)
                        {
                            CopyFolderTo(Server.MapPath("~/uploadImg/workimage/work/" + item.WorkPhoto), Server.MapPath("~/uploadImg/linshi/work/"));
                        }
                    }
                }
                string ss = "压缩成功";
                ZipFilenew(Server.MapPath("~/uploadImg/linshi/work/"), Server.MapPath("~/uploadImg/work.zip"));
                var path = Server.MapPath("~/uploadImg/work.zip");//下载图片
                //d.Delete();
                return File(path, "application/x-zip-compressed", "work.Zip");
            }
            #endregion
            return File(Server.MapPath("~/uploadImg/linshi/Humanbasic.zip"), "application/x-zip-compressed", "Humanbasic.Zip");
        }
        //返office信息
        public JsonResult supervisorHtmlValue()
        {
            string office = "<option value=\"" + "全部" + "\">" + "全部" + "</option>"; ;
            List<Office> off = db.Office.ToList();
            foreach (var item in off)
            {
                office += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
            }
            var data = new
            {
                officeValue = office
            };
            return Json(data);
        }

        class getMan//预览基本界面信息
        {
            public string uniquenum { get; set; }//唯一号
            public string name { get; set; }//姓名
            #region 基本信息
            public string offices { get; set; }//办事处
            public string sex { get; set; }//性别
            public string identity { get; set; }//身份证号
            public string city { get; set; }//城市
            public string bank { get; set; }//银行卡号
            public string account { get; set; }//开户行
            public string mobile { get; set; }//电话号码
            #endregion
        }

        class getMan1//预览培训界面信息
        {
            public string uniquenum { get; set; }//唯一号
            public string name { get; set; }//姓名
            #region 培训信息
            public string offices { get; set; }//办事处
            public string sex { get; set; }//性别
            public string identity { get; set; }//身份证号
            public string city { get; set; }//城市
            public string bank { get; set; }//银行卡号
            public string product { get; set; }//培训产品
            public int grade { get; set; }//培训分数
            public string trainer { get; set; }//培训人员    
            public string date { get; set; }//培训时间
            #endregion
        }
        class getMan2//预览自检界面信息
        {
            #region 自检信息
            public string uniquenum { get; set; }//唯一号
            public string name { get; set; }//姓名
            public string offices { get; set; }//办事处
            public string sex { get; set; }//性别
            public string identity { get; set; }//身份证号
            public string city { get; set; }//城市
            public string bank { get; set; }//银行卡号
            public string level { set; get; }//等级
            public int appearance { set; get; }//仪表仪容
            public int attitude { get; set; }//工作态度
            public int productkonwledge { get; set; }//产品知识
            public string date { get; set; }//培训时间
            #endregion
        }
        class getMan3//预览上班界面信息
        {
            #region 上班信息
            public string uniquenum { get; set; }//唯一号
            public string name { get; set; }//姓名
            public string offices { get; set; }//办事处
            public string sex { get; set; }//性别
            public string identity { get; set; }//身份证号
            public string city { get; set; }//城市
            public string bank { get; set; }//银行卡号
            public string activity { set; get; }//活动名称
            public string product { get; set; }//培训产品
            public string store { get; set; }//上班门店
            public string work { get; set; }//上班职能
            public int wage { get; set; }//日薪标准
            public int subsidy { get; set; }//补助
            public string date { get; set; }//培训时间
            #endregion
        }
        //预览界面
        public JsonResult displayInfo()
        {
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //选取信息类型
            string type = Request.Params["type"];
            //选取办事处
            string office = Request.Params["office"];
            //获取开始时间
            string begin = Request.Params["begin"];
            if (begin.Length == 0)
            {
                ModelState.AddModelError("begin", "时间不能为空！");
            }
            //获取结束时间
            string end = Request.Params["end"];
            if (end.Length == 0)
            {
                ModelState.AddModelError("end", "时间不能为空！");
            }
            #region 基本信息
            if (type == "基本信息")
            {
                List<getMan> getm = new List<getMan>();
                List<HumanBasicFile> da = db.HumanBasicFile.OrderBy(a=>a.uniNum).ToList();
                da = da.Where(a => a.createTime > Convert.ToDateTime(begin)).ToList();
                da = da.Where(a => a.createTime < Convert.ToDateTime(end).AddDays(1) && a.IsDelete != true).ToList();
                if (office != "全部")
                {
                    da = da.Where(dataFilter => dataFilter.City1.Office.Name == office).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                }
                else
                {
                    da = (da.Skip((page - 1) * pagesize).Take(pagesize)).ToList();
                }
                foreach (var item in da)
                {
                    string gender = null;
                    if (item.Sex == true)
                    { gender = "男"; }
                    else if (item.Sex == false)
                    { gender = "女"; }
                    getm.Add(new getMan()
                    {
                        uniquenum = item.uniNum,//唯一号
                        name = item.Name,//姓名
                        offices = item.City1.Office.Name,//办事处
                        sex = gender,//性别
                        identity = item.IDcardNo,//身份证号码
                        city = item.City1.Name,//城市
                        bank = item.BankNum,//银行卡号
                        account = item.Bank.Name,//银行名称
                        mobile = item.Telephone,//电话号码
                    }
                    );
                }
                int totalPage = da.Count() / pagesize;
                if (da.Count() % pagesize != 0)
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

                var gridData = new //对data辅值
                {
                    gdata = getm,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }
            #endregion
            #region 培训信息
            else if (type == "培训信息")
            {
                List<Train> da = db.Train.OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                da = da.Where(dataFilter => Convert.ToDateTime(dataFilter.TrainStartTime) > Convert.ToDateTime(begin)).ToList();
                da = da.Where(dataFilter => Convert.ToDateTime(dataFilter.TrainEndTime) < Convert.ToDateTime(end).AddDays(1)).ToList();
                if (office != "全部")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.City1.Office.Name == office).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                }
                else
                {
                    da = (da.Skip((page - 1) * pagesize).Take(pagesize)).ToList();
                }
                List<getMan1> getm = new List<getMan1>();
                foreach (var item in da)
                {
                    string gender = null;
                    if (item.HumanBasicFile.Sex == true)
                    { gender = "男"; }
                    else if (item.HumanBasicFile.Sex == false)
                    { gender = "女"; }
                    getm.Add(new getMan1()
                    {
                        uniquenum = item.HumanBasicFile.uniNum,//唯一号
                        name = item.HumanBasicFile.Name,//姓名
                        offices = item.HumanBasicFile.City1.Name,
                        sex = gender,
                        identity = item.HumanBasicFile.IDcardNo,
                        city = item.HumanBasicFile.City1.Name,
                        bank = item.HumanBasicFile.BankNum,
                        product = item.Trainpro,
                        grade = Convert.ToInt32(item.TrainScore),
                        trainer = item.Trainlecturer,
                        date = item.TrainStartTime
                    }
                    );
                }
                int totalPage = da.Count() / pagesize;
                if (da.Count() % pagesize != 0)
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

                var gridData = new //对data辅值
                {
                    gdata = getm,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }
            #endregion
            #region 自检信息
            else if (type == "自检信息")
            {
                List<DianJian> da = db.DianJian.OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                da = da.Where(dataFilter => Convert.ToDateTime(dataFilter.DJTime) > Convert.ToDateTime(begin)).ToList();
                da = da.Where(a => Convert.ToDateTime(a.DJTime) < Convert.ToDateTime(end).AddDays(1)).ToList();
                if (office != "全部")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.City1.Office.Name == office).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                }
                else
                {
                    da = (da.Skip((page - 1) * pagesize).Take(pagesize)).ToList();
                }
                List<getMan2> getm = new List<getMan2>();
                foreach (var item in da)
                {
                    string gender = null;
                    if (item.HumanBasicFile.Sex == true)
                    { gender = "男"; }
                    else if (item.HumanBasicFile.Sex == false)
                    {
                        gender = "女";
                    }
                    getm.Add(new getMan2()
                    {
                        uniquenum = item.HumanBasicFile.uniNum,//唯一号
                        name = item.HumanBasicFile.Name,//姓名
                        offices = item.HumanBasicFile.City1.Name,
                        sex = gender,
                        identity = item.HumanBasicFile.IDcardNo,
                        city = item.HumanBasicFile.City1.Name,
                        bank = item.HumanBasicFile.BankNum,
                        level = item.HumanBasicFile.HumanLevel,//等级
                        appearance = Convert.ToInt32(item.Face),//仪表仪容
                        attitude = Convert.ToInt32(item.WorkAttitude),//工作态度
                        productkonwledge = Convert.ToInt32(item.KOP),//产品知识
                        date = item.DJTime//工作时间
                    }
                    );
                }
                int totalPage = da.Count() / pagesize;
                if (da.Count() % pagesize != 0)
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

                var gridData = new //对data辅值
                {
                    gdata = getm,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }
            #endregion
            #region 上班信息
            else if (type == "上班信息")
            {
                List<AttendingInfo> da = db.AttendingInfo.ToList();
                da = da.Where(dataFilter => Convert.ToDateTime(dataFilter.Date) > Convert.ToDateTime(begin)).ToList();
                  da= da.Where(a=>Convert.ToDateTime(a.Date) < Convert.ToDateTime(end).AddDays(1)).ToList();
                if (office != "全部")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.City1.Office.Name == office).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                }
                else
                {
                    da = (da.Skip((page - 1) * pagesize).Take(pagesize)).ToList();
                }
                List<getMan3> getm = new List<getMan3>();
                foreach (var item in da)
                {
                    string gender = null;
                    if (item.HumanBasicFile.Sex == true)
                    { gender = "男"; }
                    else if (item.HumanBasicFile.Sex == false)
                    {
                        gender = "女";
                    }
                    getm.Add(new getMan3()
                    {
                        uniquenum = item.HumanBasicFile.uniNum,//唯一号
                        name = item.HumanBasicFile.Name,//姓名
                        offices = item.HumanBasicFile.City1.Name,
                        sex = gender,
                        identity = item.HumanBasicFile.IDcardNo,
                        city = item.HumanBasicFile.City1.Name,
                        bank = item.HumanBasicFile.BankNum,
                        activity = item.ActionName,//活动名称
                        product = item.production,//工作产品
                        store = item.SShop.Name,//上班门店
                        work = item.Functions,//工作职能
                        wage = Convert.ToInt32(item.StandardSalary),//日新标准
                        subsidy = Convert.ToInt32(item.BearFees),//补助
                        date = item.Date
                    }
                    );
                }
                int totalPage = da.Count() / pagesize;
                if (da.Count() % pagesize != 0)
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

                var gridData = new //对data辅值
                {
                    gdata = getm,
                    page = page,
                    totalPage = totalPage,
                    pageBtn = pageBtnClass,
                    pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + da.Count().ToString() + "条记录",
                    pageSize = pagesize
                };
                return Json(gridData);
            }
            #endregion
            return Json("null");
        }//预览界面

        //导出为一个excel表
        public ActionResult exportInformation()
        {
            var office = Request.Form.Get("office");//办事处
            var type = Request.Form.Get("type");//信息类型
            var bengintime = Request.Form.Get("begin");//开始时间
            string admin = Session["admin"].ToString();
            if (bengintime.Length == 0)
            {
                ModelState.AddModelError("begin", "时间不能为空！");
            }
            var ben = Convert.ToDateTime(bengintime).ToOADate();
            var endtime = Request.Form.Get("end");//结束时间
            if (endtime.Length == 0)
            {
                ModelState.AddModelError("end", "时间不能为空！");
            }
            var end = Convert.ToDateTime(endtime).ToOADate();
            var sex = "";
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0' Charset='utf-8'>");
            sbHtml.Append("<tr>");
            List<string> lstTitle = null;
            if (admin == "admin")
            {
                #region 基本信息导出
                if (type == "基本信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "开户行", "电话号码", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<HumanBasicFile> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.HumanBasicFile
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && g.Name == office
                               select s).OrderBy(a => a.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.HumanBasicFile
                               from g in db.Office
                               where s.City1.OfficeId == g.Id
                               select s).OrderBy(a => a.uniNum).ToList();
                    }
                    hum = hum.Where(a => a.createTime > Convert.ToDateTime(bengintime)).ToList();
                    hum = hum.Where(a => a.createTime < Convert.ToDateTime(endtime).AddDays(1) && a.IsDelete != true).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.City1.Office.Name);
                        if (item1.Sex == true)
                        {
                            sex = "男";
                        }
                        else
                        { sex = "女"; }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", sex);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Bank.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Telephone);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 培训信息导出
                if (type == "培训信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "培训产品", "培训分数", "培训人员", "培训开始时间", "培训结束时间", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<Train> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.Train
                               from g in db.Office
                               where s.HumanBasicFile.City1.OfficeId == g.Id && g.Name == office
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.Train
                               from g in db.Office
                               where s.HumanBasicFile.City1.OfficeId == g.Id
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    hum = hum.Where(a => Convert.ToDateTime(a.TrainStartTime) >= Convert.ToDateTime(bengintime)).ToList();
                    hum = hum.Where(a => Convert.ToDateTime(a.TrainEndTime) <= Convert.ToDateTime(endtime).AddDays(1)).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainProduction);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainScore);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Creatorname);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainStartTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainEndTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 上班信息导出
                if (type == "上班信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "上班门店", "活动名称", "产品名称", "工作职能", "日薪标准", "补助","上班时间","手机号码","起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<AttendingInfo> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.AttendingInfo
                               from g in db.Office
                               from ss in db.HumanBasicFile
                               where s.HumanId == ss.Id && g.Name == office && ss.City1.OfficeId == g.Id
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.AttendingInfo
                               from g in db.Office
                               from ss in db.HumanBasicFile
                               where s.HumanId == ss.Id && ss.City1.OfficeId == g.Id
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    hum = hum.Where(a =>Convert.ToDateTime(a.Date)>Convert.ToDateTime(bengintime)).ToList();
                    hum = hum.Where(a=>Convert.ToDateTime(a.Date)<Convert.ToDateTime(endtime).AddDays(1)).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.Append("<tr>");
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.SShop.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.ActionName);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.production);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Functions);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.StandardSalary);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.BearFees);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>",item1.Date);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Telephone);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 自检信息导出
                if (type == "自检信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "等级", "仪容仪表", "工作态度", "产品知识", "自检时间", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<DianJian> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.HumanBasicFile
                               from ss in db.DianJian
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && g.Name == office && s.Id == ss.HumanId
                               select ss).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.HumanBasicFile
                               from ss in db.DianJian
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && s.Id == ss.HumanId
                               select ss).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    hum = hum.Where(a => Convert.ToDateTime(a.DJTime) > Convert.ToDateTime(bengintime)).ToList();
                    hum = hum.Where(a => Convert.ToDateTime(a.DJTime) < Convert.ToDateTime(endtime).AddDays(1)).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.HumanLevel);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Face);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.WorkAttitude);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.KOP);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.DJTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
            }
            else
            {

                #region 基本信息导出
                if (type == "基本信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "开户行", "电话号码", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<HumanBasicFile> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.HumanBasicFile
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && g.Name == office && s.Managers.UserId == admin
                               select s).OrderBy(a => a.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.HumanBasicFile
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && s.Managers.UserId == admin
                               select s).OrderBy(a => a.uniNum).ToList();
                    }
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", office);
                        if (item1.Sex == true)
                        {
                            sex = "男";
                        }
                        else
                        { sex = "女"; }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", sex);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "\'" + item1.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "\'" + item1.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Bank.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Telephone);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 培训信息导出
                if (type == "培训信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "培训产品", "培训分数", "培训人员", "培训开始时间", "培训结束时间", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<Train> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.Train
                               from g in db.Office
                               where s.HumanBasicFile.City1.OfficeId == g.Id && g.Name == office && s.HumanBasicFile.Managers.UserId == admin
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.Train
                               from g in db.Office
                               where s.HumanBasicFile.City1.OfficeId == g.Id && s.HumanBasicFile.Managers.UserId == admin
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    hum = hum.Where(a => Convert.ToDateTime(a.TrainStartTime) >= Convert.ToDateTime(bengintime)).ToList();
                    hum = hum.Where(a => Convert.ToDateTime(a.TrainEndTime) <= Convert.ToDateTime(endtime).AddDays(1)).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainProduction);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainScore);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Creatorname);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainStartTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.TrainEndTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 上班信息导出
                if (type == "上班信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "上班门店", "活动名称", "产品名称", "工作职能", "日薪标准", "补助", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<AttendingInfo> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.AttendingInfo
                               from g in db.Office
                               from ss in db.HumanBasicFile
                               where s.HumanId == ss.Id && g.Name == office && ss.City1.OfficeId == g.Id && ss.Managers.UserId == admin
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.AttendingInfo
                               from g in db.Office
                               from ss in db.HumanBasicFile
                               where s.HumanId == ss.Id && ss.City1.OfficeId == g.Id && ss.Managers.UserId == admin
                               select s).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    //hum = hum.Where(a => Convert.ToDateTime(a.) >= Convert.ToDateTime(beginTime)).ToList();
                    foreach (var item1 in hum)
                    {
                        sbHtml.Append("<tr>");
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.SShop.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.ActionName);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.production);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Functions);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.StandardSalary);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.BearFees);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);

                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
                #region 自检信息导出
                if (type == "自检信息")
                {
                    lstTitle = new List<string> { "唯一号", "姓名", "办事处", "性别", "身份证号", "城市", "银行卡号", "等级", "仪容仪表", "工作态度", "产品知识", "自检时间", "起始时间", "截止时间" }.ToList();
                    foreach (var item in lstTitle)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item);
                    }
                    sbHtml.Append("</tr>");
                    List<DianJian> hum;
                    if (office != "全部")
                    {
                        hum = (from s in db.HumanBasicFile
                               from ss in db.DianJian
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && g.Name == office && s.Id == ss.HumanId && s.Managers.UserId == admin
                               select ss).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    else
                    {
                        hum = (from s in db.HumanBasicFile
                               from ss in db.DianJian
                               from g in db.Office
                               where s.City1.OfficeId == g.Id && s.Id == ss.HumanId && s.Managers.UserId == admin
                               select ss).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
                    }
                    foreach (var item1 in hum)
                    {
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.uniNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Office.Name);
                        if (item1.HumanBasicFile.Sex == true)
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "男");
                        }
                        else
                        {
                            sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", "女");
                        }
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.IDcardNo);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.City1.Name);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.BankNum);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.HumanBasicFile.HumanLevel);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.Face);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.WorkAttitude);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.KOP);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", item1.DJTime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", bengintime);
                        sbHtml.AppendFormat("<td style='vnd.ms-excel.numberformat:@;font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25' ;Charset='utf-8'>{0}</td>", endtime);
                        sbHtml.Append("</tr>");
                    }
                }
                #endregion
            }
            sbHtml.Append("</table>");
            Response.Charset = "utf-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.HeaderEncoding = System.Text.Encoding.UTF8;
            byte[] fileContents = Encoding.Default.GetBytes(sbHtml.ToString());
            return File(fileContents, "application/ms-excel", office+".xls");
        }

        //导出文件夹

        #endregion

        #region 反馈表 createManagerId和Editmanager要改
        //反馈表主页面
        [Management.filter.LoginFilter]
        public ActionResult Feedback()
        {
            return View();
        }

        //获取反馈表的信息
        public ActionResult getFeedbackInfo()
        {
            //新建一个反馈信息对象
            IList<getFeedback> feedbackData1 = new List<getFeedback>();
            //从数据库张获取表
            List<Feedback> fblist = db.Feedback.OrderByDescending(a => a.CreatedTime).ToList();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);



            //总页数
            int totalPage = fblist.Count() / pagesize;
            if (totalPage == 0) { totalPage = 1; }
            if (feedbackData1.Count() % pagesize != 0 && totalPage != 1)
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
            var list1 = fblist.Skip(pagesize * (page - 1)).Take(pagesize).ToList();
            foreach (var item in list1)
            {
                feedbackData1.Add(new getFeedback()
                {
                    id = item.Id,
                    title = item.Title,
                    createtime = item.CreatedTime.ToString(),
                    createmanager = item.CreatedManagerId,
                    replymanager = item.LastReplyManager
                });
            }

            //给前台所需数据赋值
            var gridData = new
            {
                gdata = feedbackData1,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + fblist.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);



        }

        //新增反馈页面
        [Management.filter.LoginFilter]
        public ActionResult newFeedback()
        {
            return View();
        }

        //增加反馈操作，createmanager和replymanager要改回session值!!!
        [HttpPost]
        public ActionResult addFeedback()
        {
            //获取问题反馈的标题
            string title = Request.Form.Get("feedbackInfo.title");
            //获取问题反馈的内容
            string content = Request.Form.Get("feedbackInfo.contents");
            Feedback fb = new Feedback();

            //标题验证
            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("feedbackInfo.title", "请输入标题");
            }
            else
            {
                fb.Title = title;
                ViewBag.Title = title;
            }

            //内容验证
            if (string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("feedbackInfo.contents", "请输入反馈内容");
            }
            else
            {
                fb.FBContext = content;
                ViewBag.content = content;
            }
            //如果验证没有问题
            if (ModelState.IsValid)
            {
                fb.CreatedTime = DateTime.Now;
                fb.CreatedManagerId = Session["admin"].ToString();//创建人，要改回session值
                fb.LastReplyManager = Session["admin"].ToString();//回复人，要改回session值
                db.Feedback.Add(fb);
                db.SaveChanges();
                return RedirectToAction("feedback");
            }

            return View("newFeedback");

        }

        //显示反馈的具体内容
        public ActionResult showFeedback(int id)
        {
            Feedback fb = db.Feedback.Find(id);
            Session["FeedbackId"] = fb.Id;
            List<ReplyFeedback> rfb = db.ReplyFeedback.ToList();
            ViewBag.reply = rfb;
            return View(fb);
        }

        //提交反馈表
        [HttpPost]
        public ActionResult replyFeedback()
        {
            int fb_id = (int)Session["FeedbackId"];
            ReplyFeedback rfb = new ReplyFeedback();
            string content = Request.Form.Get("feedbackInfo.contents");
            if (string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("feedbackInfo.contents", "请输入内容");
            }
            else
            { rfb.ReplyContent = content; }

            if (ModelState.IsValid)
            {
                rfb.CreateManId = "aa";
                rfb.CreateTime = DateTime.Now;
                rfb.TitleId = fb_id;
                db.ReplyFeedback.Add(rfb);
                db.SaveChanges();
                Feedback fb = db.Feedback.Find(fb_id);
                fb.LastReplyManager = Session["admin"].ToString();// Session["admin"];
                db.Entry(fb).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("showFeedback", new { id = fb_id });
            }
            Feedback fb1 = db.Feedback.Find(fb_id);
            List<ReplyFeedback> a = db.ReplyFeedback.ToList();
            ViewBag.reply = a;

            return View("showFeedback", fb1);
        }

        #endregion

        #region 添加公告
        [Management.filter.LoginFilter]
        public ActionResult News()
        {
            return View();
        }
        [HttpPost]
        public ActionResult addNews()
        {
            Notice notice = new Notice();
            string newsInfoTitle = Request.Form.Get("newsInfo.title");
            if (string.IsNullOrEmpty(newsInfoTitle))
            {
                ModelState.AddModelError("newsInfo.title", "请输入标题");
            }
            else
            {
                notice.NoticeTitle = newsInfoTitle;
                ViewBag.title = newsInfoTitle;
            }
            string newsInfoContent = Request.Form.Get("newsInfo.content");
            if (string.IsNullOrEmpty(newsInfoContent))
            {
                ModelState.AddModelError("newsInfo.content", "请输入内容");
            }
            else
            {
                notice.NoticeContent = newsInfoContent;
                ViewBag.content = newsInfoContent;
            }
            if (ModelState.IsValid)
            {
                notice.ManagerId = "admin";
                notice.CreatedTime = DateTime.Now;
                db.Notice.Add(notice);
                db.SaveChanges();
                //添加成功后则删除viewbag内容
                ViewBag.title = "";
                ViewBag.content = "";
                return View("News");
            }
            return View("News");
        }

        #endregion

        #region 登陆信息
        //登陆信息视图
        [Management.filter.LoginFilter]
        public ActionResult CheckAdmin()
        {
            return View();
        }
        //登陆信息获取
        public ActionResult LoginInfo()
        {
            //新建一个基本现象
            IList<getLoginlog> logindata1 = new List<getLoginlog>();
            //从数据库中获取表
            List<LoginLog> logindata = db.LoginLog.OrderByDescending(a => a.Id).ToList();

            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);


            //总页数
            int totalPage = logindata.Count() / pagesize;
            if (totalPage == 0) { totalPage = 1; }
            if (logindata.Count() % pagesize != 0 && totalPage != 1)
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

            var i = logindata.Skip(pagesize * (page - 1)).Take(pagesize).ToList();
            foreach (var item in i)
            {
                logindata1.Add(new getLoginlog()
                {
                    id = item.Id,
                    username = item.Managers.UserId,
                    ip = item.IPAddress,
                    date = item.CreatedTime.ToString()
                });
            }
            //给前台所需数据赋值
            var gridData = new
            {
                gdata = logindata1,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + logindata.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);


        }


        #endregion

        #region 产品信息
        //产品信息页面
        [Management.filter.LoginFilter]
        public ActionResult Product()
        {
            try
            {
                List<ProductCategory> PCList = db.ProductCategory.ToList();
                ViewBag.PCList = PCList;
                ViewBag.ProductId = PCList.Select(a => a.Id).First();
                if (PCList.Count() != 0)
                {
                    var PCId = (from p in PCList select p.Id).First();
                    List<product> pro = (from p in db.product.ToList() where p.ProductCategoryId == PCId select p).ToList();
                    ViewBag.PList = pro;
                }
                return View();
            }
            catch
            {
                return View();
            }

        }

        //获取培训产品的型号
        [Management.filter.LoginFilter]
        public ActionResult getProduct(string id)
        {
            int num = Convert.ToInt32(id);
            List<ProductCategory> PCList = db.ProductCategory.ToList();
            ViewBag.PCList = PCList;
            ViewBag.ProductId = PCList.Where(a => a.Id == num).Select(a => a.Id).First();
            List<product> pro = (from p in db.product.ToList() where p.ProductCategoryId == num select p).ToList();
            ViewBag.PList = pro;
            return View();
        }

        //增加培训产品
        [HttpPost]
        public ActionResult addProduct()
        {
            string proname = Request.Form.Get("productInfo.trainProduct");
            if (string.IsNullOrEmpty(proname))
            {
                ModelState.AddModelError("productInfo.trainProduct", "请输入产品名称");
            }
            ProductCategory pro = new ProductCategory();
            if (ModelState.IsValid)
            {
                pro.Name = proname;
                pro.CreatedTime = DateTime.Now;
                db.ProductCategory.Add(pro);
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            return RedirectToAction("Product");
        }

        //增加产品型号
        [HttpPost]
        public ActionResult addType()
        {
            string typename = Request.Form.Get("typeInfo.trainType");
            string proId = Request.Form.Get("typeInfo.productId");
            if (string.IsNullOrEmpty(typename))
            {
                ModelState.AddModelError("typeInfo.trainType", "请输入型号名称");
            }
            product pro = new product();
            if (ModelState.IsValid)
            {
                pro.Name = typename;
                pro.CreatedTime = DateTime.Now;
                pro.ProductCategoryId = Convert.ToInt32(proId);
                db.product.Add(pro);
                db.SaveChanges();
                return RedirectToAction("getProduct", new { id = proId });
            }
            return RedirectToAction("getProduct", new { id = proId });
        }

        //删除培训产品
        public ActionResult deleteProductCategory(int id)
        {
            int productId = Convert.ToInt32(id);//获取id
            ProductCategory PC = db.ProductCategory.Find(productId);
            var Plist = db.product.Where(a => a.ProductCategoryId == productId).ToList();
            //先删除该产品下的型号
            foreach (var item in Plist)
            {
                product p = db.product.Find(item.Id);
                db.product.Remove(p);
                db.SaveChanges();
            }
            //再删除该产品
            db.ProductCategory.Remove(PC);
            db.SaveChanges();

            return RedirectToAction("Product");
        }

        public ActionResult deleteType(int id)
        {
            product p = db.product.Find(id);
            db.product.Remove(p);
            db.SaveChanges();
            return RedirectToAction("Product");
        }
        #endregion
    }
}
