using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Data.Entity;
using System.Data;
using System.Text.RegularExpressions;

namespace Management.Controllers
{
    public class LevelController : Controller
    {
        private SS_HRM_DBEntities db = new SS_HRM_DBEntities();

        #region 获取资料类
        class getCity
        {
            public string cityid { get; set; }
            public int officeid { get; set; }
            public string office { get; set; }
            public string city { get; set; }
            public string code { get; set; }
        }

        class getShop
        {
            public string shopid { get; set; }
            public string office { get; set; }
            public string city { get; set; }
            public int citycode { get; set; }
            public string shop { get; set; }
            public string code { get; set; }
        }

        class getA
        {
            public Guid id { get; set; }
            public string uniNum { get; set; }
            public string name { get; set; }
            public string sex { get; set; }
        }

        #region 给AddshopC页面赋值
        //给AddshopC里的officeId赋值
        public ActionResult getOfficeName()
        {
            string officename = "";
            string cityname = "";
            //从数据库中获取数据表
            List<Office> officeList = db.Office.ToList();
            List<City> cityList = db.City.ToList();
            if (Session["authority"].ToString() == "督导")
            {
                string name = Session["admin"].ToString();
                Managers man = (from m in db.Managers where m.UserId == name select m).FirstOrDefault();
                officeList = officeList.Where(a => a.Name == man.City1.Office.Name).ToList();
            }
            string chooseOffice = Request.Params["chooseOffice"];
            if (!string.IsNullOrEmpty(chooseOffice))
            {
                officeList = officeList.Where(a => a.Name.Contains(chooseOffice)).ToList();
            }
            foreach (var item in officeList)
            {
                officename += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
            }
            var firstcity = officeList.FirstOrDefault();
            foreach (var item in cityList)
            {
                if (firstcity == null)
                { break; }
                if (item.OfficeId == firstcity.Id)
                { cityname += "<option value=\"" + item.Id + "\">" + item.Name + "</option>"; }
            }

            var griddata = new
            {
                officeValue = officename,
                cityValue = cityname
            };
            return Json(griddata);

        }
        //给cityId赋值
        public ActionResult getCityName()
        {
            string cityname = "";
            //从数据库中获取数据表
            List<City> cityList = db.City.ToList();
            //若为督导则筛选督导所在办事处下的城市
            if (Session["authority"].ToString() == "督导")
            {
                var AdminOffice = (from office in db.Managers.ToList() where office.UserId == Session["admin"].ToString() select office.City1.Office.Name).FirstOrDefault().ToString();    //获取督导办事处
                cityList = (from city in db.City.ToList() where city.Office.Name == AdminOffice select city).ToList();
            }
            string chooseOffice = Request.Params["chooseOffice"]; //获取OfficeId
            if (!string.IsNullOrEmpty(chooseOffice))
            {
                cityList = cityList.Where(a => a.OfficeId == Convert.ToInt32(chooseOffice)).ToList();
            }
            string chooseCity = Request.Params["chooseCity"];
            if (!string.IsNullOrEmpty(chooseCity))
            {
                cityList = cityList.Where(a => a.Name.Contains(chooseCity)).ToList();
            }
            foreach (var item in cityList)
            {
                cityname += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
            }

            var griddata = new
            {
                cityValue = cityname,
            };
            return Json(griddata);

        }
        #endregion

        #endregion

        #region 等级控制 放在每月的最后一天 

        //降C级       
        public ActionResult Clower()
        {
            #region C级淘汰
            List<DianJian> dj = (from a in db.DianJian.ToList() select a).ToList();

            var Chum = (from b in db.HumanBasicFile select b).ToList();
            foreach (var item in Chum)
            {
                var c = dj.Where(a => a.Score < 80 && a.HumanBasicFile.uniNum == item.uniNum && a.HumanBasicFile.LevelEditTimes < 1).ToList();
                if (c.Count() >= 2)
                {
                    HumanBasicFile hum = db.HumanBasicFile.Find(item.Id);
                    hum.HumanLevel = "C";
                    db.Entry(hum).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            #endregion

            return Json(true);
        }
        
        //各种升级降级
        public ActionResult levelCon()
        {
            List<DianJian> dj = (from a in db.DianJian.ToList() select a).ToList();
            #region 升级
            //系统默认
            #region B级升A级

            var BList = (from b in db.HumanBasicFile where b.HumanLevel == "B" select b).ToList();
            //选择等级B的人员
            foreach (var item in BList)
            {
                //当月的兼职人员
                var a = dj.Where(b => b.HumanBasicFile.uniNum == item.uniNum && b.HumanBasicFile.LevelEditTimes < 1 && DateTime.Parse(b.DJTime) >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) && DateTime.Parse(b.DJTime) <= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString())).ToList();

                //次数多于4次则执行
                if (a.Count() >= 4)
                {                //平均分数高于90
                    double ave = (double)a.Select(d => d.Score).Average();
                    if (ave > 90)
                    {
                        HumanBasicFile hum = db.HumanBasicFile.Find(item.Id);
                        hum.HumanLevel = "A";
                        hum.LevelEditTimes += 1;
                        db.Entry(hum).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            #endregion

            #endregion

            #region 降级
            if (DateTime.Now.ToShortDateString()==DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToShortDateString())
            {
                #region AorS级降B级

                var SorAList = (from SA in db.HumanBasicFile.ToList() where SA.HumanLevel == "S" || SA.HumanLevel == "A" select SA).ToList();
                foreach (var item in SorAList)
                {
                    //当月自检平均分在90分以下降为B级。
                    var toBList = dj.Where(b => b.HumanBasicFile.uniNum == item.uniNum && b.HumanBasicFile.LevelEditTimes < 1 && DateTime.Parse(b.DJTime) >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) && DateTime.Parse(b.DJTime) <= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString())).ToList();
                    toBList = toBList.Where(a => a.Score < 90).ToList();
                    if (toBList.Count() >= 1)
                    {
                        double ave = (double)toBList.Select(d => d.Score).Average();
                        if (ave < 90 && ave > 80)
                        {
                            HumanBasicFile hum = db.HumanBasicFile.Find(item.Id);
                            hum.HumanLevel = "B";
                            hum.LevelEditTimes += 1;
                            db.Entry(hum).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }

                #endregion

                #region S级 降 A级
                var SList = (from S in db.HumanBasicFile.ToList() where S.HumanLevel == "S" select S).ToList();
                foreach (var item in SList)
                {
                    //当月自检平均分在90分以下降为B级。
                    var toBList = dj.Where(b => b.HumanBasicFile.uniNum == item.uniNum && b.HumanBasicFile.LevelEditTimes < 1 && DateTime.Parse(b.DJTime) >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) && DateTime.Parse(b.DJTime) <= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString())).ToList();
                    if (toBList.Count() >= 1)
                    {
                        double ave = (double)toBList.Select(d => d.Score).Average();
                        if (ave > 90 && ave < 95)
                        {
                            HumanBasicFile hum = db.HumanBasicFile.Find(item.Id);
                            hum.HumanLevel = "A";
                            hum.LevelEditTimes += 1;
                            db.Entry(hum).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                #endregion
            }
            #region A级 降 B级 情况2
            var AList2 = (from A in db.HumanBasicFile.ToList() where A.HumanLevel == "A" select A).ToList();
            foreach (var item in AList2)
            {
                //2个月没有上班降为B级。
                var att = (from a in db.AttendingInfo.ToList() where a.HumanId == item.Id select a).ToList();
                var toBList = att.Where(b => b.HumanBasicFile.uniNum == item.uniNum && b.HumanBasicFile.LevelEditTimes == 0 && DateTime.Parse(b.Date) > DateTime.Parse(DateTime.Now.ToShortDateString()).AddMonths(-2)).ToList();
                if (toBList.Count() <= 0)
                {
                    if (item.LevelEditTimes == 0)
                    {
                        HumanBasicFile hum = db.HumanBasicFile.Find(item.Id);
                        hum.HumanLevel = "B";
                        hum.LevelEditTimes += 1;
                        db.Entry(hum).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            #endregion

            #endregion
            return Json(true);
        }

        public ActionResult levelConfirm()
        {
            if (Session["authority"].ToString() == "督导"||Session["authority"].ToString()=="小队长")
            {
                return View();
            }
            else
            { return View("~/Views/Shared/AuthorityError.cshtml"); }
        }

        public ActionResult levelConfirm1()
        {
            string humid_string = Request.Params["humid"];
            Guid humid = new Guid(humid_string);
            HumanBasicFile hum = db.HumanBasicFile.Find(humid);
            hum.HumanLevel = "S";
            hum.LevelEditTimes += 1;
            db.Entry(hum).State = EntityState.Modified;
            db.SaveChanges();
            return Json(true);
        }

        public ActionResult getAtoS()
        {
            IList<getA> getA = new List<getA>();
            
            //权限
            string authority = Session["authority"].ToString();
            //用户名
            string userId = Session["admin"].ToString();
            Managers man=new Managers();
            if (authority == "小队长")
            {
                string boss_string = (from m in db.Managers where m.UserId == userId select m.Boss).FirstOrDefault();
                man = (from m in db.Managers where m.UserId == boss_string select m).FirstOrDefault();
            }
            else if (authority == "督导")
            {
                man = (from m in db.Managers where m.UserId == userId select m).FirstOrDefault();
            }
            else
            {
                //返回错误页面
                return View("~/Views/Shared/AuthorityError.cshtml");
            }
            //获取数据库
            List<HumanBasicFile> AhumList = (from a in db.HumanBasicFile.ToList() where a.HumanLevel == "A"&&a.City1.Office.Id==man.City1.OfficeId select a).ToList();
            
            var dj = db.DianJian.ToList();
            DateTime monthBegin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            DateTime nextmonthBegin = DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString());
            foreach (var item in AhumList)
            {
                var b = dj.Where(d => d.HumanId == item.Id && DateTime.Parse(d.DJTime) >= monthBegin && DateTime.Parse(d.DJTime) <= nextmonthBegin).ToList();
                if (b.Count() >= 4)
                {
                    double ave = (double)b.Select(d => d.Score).Average();
                    if (ave > 95)
                    {
                        string gender = "";
                        if (item.Sex == true)
                        {
                            gender = "男";
                        }
                        else
                        {
                            gender = "女";
                        }
                        getA.Add(new getA()
                        {
                            id = item.Id,
                            name = item.Name,
                            sex = gender,
                            uniNum = item.uniNum
                        });
                    }
                }
            }
            //总记录数
            int totaldata = getA.Count();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //总页数
            int totalPage = getA.Count() / pagesize;
            if (getA.Count() % pagesize != 0)
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
            getA = getA.Skip(pagesize * (page - 1)).Take(pagesize).ToList();

            //给前台所需数据赋值
            var gridData = new
            {
                gdata = getA,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + totaldata.ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);
        }

        //获取该人每月每次自检的情况
        public ActionResult getPeopleData(Guid id)
        {
            HumanBasicFile hum = db.HumanBasicFile.Find(id);
            List<DianJian> dj = (from a in db.DianJian.ToList() where a.HumanId == hum.Id && DateTime.Parse(a.DJTime) >= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) && DateTime.Parse(a.DJTime) <= DateTime.Parse(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString()) select a).ToList();
            ViewBag.dj = dj;
            return View(hum);
        }

        //每月修改次数清0
        public ActionResult retoZero()
        {
            if (DateTime.Now.Day.ToString() == "1")
            {
                List<HumanBasicFile> hum = (from a in db.HumanBasicFile.ToList() where a.HumanLevel != "C" select a).ToList();
                foreach (var item in hum)
                {
                    HumanBasicFile h = db.HumanBasicFile.Find(item.Id);
                    h.LevelEditTimes = 0;
                    db.Entry(h).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(true);
            }
            else
            {
                return null;
            }

        }
        #endregion

        #region 查看店铺和城市
        //查看店铺
        public ActionResult AddShop()
        {
            return View();
        }
        #endregion

        #region 店铺增删查改管理控制


        //获取城市
        public ActionResult checkCity()
        {
            IList<getCity> Cities = new List<getCity>();
            //从数据库中获取表
            List<City> cityList = db.City.ToList();
            //若为督导则筛选督导所在办事处下的城市
            if (Session["authority"].ToString() == "督导" || Session["authority"].ToString() == "小队长")
            {
                var AdminOffice = (from office in db.Managers.ToList() where office.UserId == Session["admin"].ToString() select office.City1.Office.Name).FirstOrDefault().ToString();    //获取督导办事处
                cityList = (from city in db.City.ToList() where city.Office.Name == AdminOffice select city).ToList();
            }
            //有关键字的筛选
            string officeContent = Request.Params["office"];
            string cityContent = Request.Params["content"];
            string codeContent = Request.Params["codeContent"];
            if (!string.IsNullOrEmpty(officeContent))
            {
                cityList = cityList.Where(a => a.Office.Name.Contains(officeContent)).ToList();
            }
            if (!string.IsNullOrEmpty(cityContent))
            {
                cityList = cityList.Where(a => a.Name.Contains(cityContent)).ToList();
            }
            if (!string.IsNullOrEmpty(codeContent))
            {
                int b = Convert.ToInt32(codeContent);
                cityList = cityList.Where(a => a.Code == b).ToList();
            }
            //获取记录
            foreach (var item in cityList)
            {
                Cities.Add(new getCity()
                {
                    cityid = item.Id.ToString(),
                    officeid = (int)item.OfficeId,
                    office = item.Office.Name,
                    city = item.Name,
                    code = item.Code.ToString()
                });
            }
            //总记录数
            int totaldata = Cities.Count();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
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
            Cities = Cities.OrderBy(a => a.officeid).ThenBy(a => Convert.ToInt32(a.code)).ToList();
            Cities = Cities.Skip(pagesize * (page - 1)).Take(pagesize).ToList();

            //给前台所需数据赋值
            var gridData = new
            {
                gdata = Cities,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + totaldata.ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);
        }

        //获取商铺
        public ActionResult checkShop()
        {
            IList<getShop> shops = new List<getShop>();
            //从数据库中获取表
            List<SShop> shopList = db.SShop.ToList();

            //若为督导则筛选督导所在办事处下的商店
            if (Session["authority"].ToString() == "督导" || Session["authority"].ToString() == "小队长")
            {
                var AdminOffice = (from office in db.Managers.ToList() where office.UserId == Session["admin"].ToString() select office.City1.Office.Name).FirstOrDefault().ToString();    //获取督导或小队长办事处
                shopList = (from shop in db.SShop.ToList() where shop.City.Office.Name == AdminOffice select shop).ToList();
            }

            //筛选信息
            string cityContent = Request.Params["content"];
            string shopContent = Request.Params["shopcontent"];
            string codeContent = Request.Params["codecontent"];

            if (!string.IsNullOrEmpty(cityContent))
            {
                shopList = shopList.Where(a => a.City.Name.Contains(cityContent)).ToList();
            }
            if (!string.IsNullOrEmpty(shopContent))
            {
                shopList = shopList.Where(a => a.Name.Contains(shopContent)).ToList();
            }
            if (!string.IsNullOrEmpty(codeContent))
            {
                shopList = shopList.Where(a => a.Code == Convert.ToInt32(codeContent)).ToList();
            }

            //获取记录
            foreach (var item in shopList)
            {
                shops.Add(new getShop()
                {
                    shopid = item.Id.ToString(),
                    office = item.City.Office.Name,
                    city = item.City.Name,
                    citycode=item.City.Code,
                    shop = item.Name,
                    code = item.Code.ToString()
                });

            }
            //总记录数
            int totaldata = shops.Count();
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //总页数
            int totalPage = shopList.Count() / pagesize;
            if (shopList.Count() % pagesize != 0)
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
            shops = shops.OrderBy(a => a.office).ThenBy(a => a.citycode).ThenBy(a => Convert.ToInt32(a.shopid)).ToList();
            shops = shops.Skip(pagesize * (page - 1)).Take(pagesize).ToList();

            //给前台所需数据赋值
            var gridData = new
            {
                gdata = shops,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + totaldata.ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);
        }

        //新增店铺页面
        public ActionResult addShopC()
        {
            ViewBag.flag = 0;
            return View();
        }

        //增加店铺操作
        public ActionResult AddshopCon()
        {
            //新增对象
            SShop shop = new SShop();
            //从页面获取参数
            string cityId = Request.Form.Get("cityname");
            string shopName = Request.Form.Get("captainInfo.shopname");
            string shopCode = Request.Form.Get("captainInfo.shopcode");

            if (string.IsNullOrEmpty(cityId))
            {
                ModelState.AddModelError("cityname", "请选择城市");
            }
            if (string.IsNullOrEmpty(shopName))
            {
                ModelState.AddModelError("captainInfo.shopname", "请输入店铺名称");
            }
            if (string.IsNullOrEmpty(shopCode))
            {
                ModelState.AddModelError("captainInfo.shopcode", "请输入店铺编号");
            }

            if (ModelState.IsValid)
            {
                shop.CityId = Convert.ToInt32(cityId);
                shop.Name = shopName;
                shop.Code = Convert.ToInt32(shopCode);
                db.SShop.Add(shop);
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("addShopC");

        }

        //删除店铺操作
        public ActionResult DeleteShop()
        {
            string shopid_string = Request.Params["ShopId"];
            int shopid = Convert.ToInt32(shopid_string);
            //删除该商店下的所有上班信息
            var Attending = (from at in db.AttendingInfo.ToList() select at).ToList();
            foreach (var item in Attending)
            {
                if (item.Department == shopid)
                {
                    AttendingInfo ai = db.AttendingInfo.Find(item.Id);
                    db.AttendingInfo.Remove(ai);
                    db.SaveChanges();
                }
            }
            //删除该商店
            SShop shop = db.SShop.Find(shopid);
            db.SShop.Remove(shop);
            db.SaveChanges();
            return Json(true);
        }

        //编辑商店页面
        public ActionResult EditShop(int id)
        {
            ViewBag.flag = 0;
            SShop shop = db.SShop.Find(id);
            return View(shop);
        }
        //编辑商店操作
        public ActionResult EditShopCon()
        {
            string shopid_string = Request.Form.Get("shopid");
            int shopid = Convert.ToInt32(shopid_string);
            //获取对象
            SShop shop = db.SShop.Find(shopid);
            //从页面获取参数
            string cityId = Request.Form.Get("cityname");
            string shopName = Request.Form.Get("captainInfo.shopname");
            string shopCode = Request.Form.Get("captainInfo.shopcode");

            if (string.IsNullOrEmpty(cityId))
            {
                ModelState.AddModelError("cityname", "请选择城市");
            }
            if (string.IsNullOrEmpty(shopName))
            {
                ModelState.AddModelError("captainInfo.shopname", "请输入店铺名称");
            }
            if (string.IsNullOrEmpty(shopCode))
            {
                ModelState.AddModelError("captainInfo.shopcode", "请输入店铺编号");
            }

            if (ModelState.IsValid)
            {
                shop.CityId = Convert.ToInt32(cityId);
                shop.Name = shopName;
                shop.Code = Convert.ToInt32(shopCode);
                db.Entry(shop).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("EditShop", shop);

        }
        //编辑商店获取该商店的办事处和城市
        public ActionResult getEditOfficeName()
        {
            string officename = "";
            string cityname = "";
            //从数据库中获取数据表
            List<Office> officeList = db.Office.ToList();
            List<City> cityList = db.City.ToList();

            string shopid_string = Request.Params["shopid"];
            int shopid = Convert.ToInt32(shopid_string);
            SShop shop = db.SShop.Find(shopid);
            foreach (var item in officeList)
            {
                if (item.Id == shop.City.OfficeId)
                {
                    officename += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
                else
                {
                    officename += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
            }
            var firstcity = officeList.Where(a => a.Id == shop.City.OfficeId).FirstOrDefault();
            foreach (var item in cityList)
            {
                if (item.OfficeId == firstcity.Id)
                {
                    if (item.Id == shop.CityId)
                    {
                        cityname += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>";
                    }
                    else
                    {
                        cityname += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                    }
                }
            }

            var griddata = new
            {
                officeValue = officename,
                cityValue = cityname
            };
            return Json(griddata);
        }

        #region 商铺验证
        //前台商铺名验证
        public ActionResult validateShopName()
        {
            bool success = true;
            var shopList = (from shop in db.SShop.ToList() select shop).ToList();

            string shopName = Request["captainInfo.shopname"].ToString();
            string shopId_string = Request["shopid"];
            if (!string.IsNullOrEmpty(shopId_string))
            {
                int shopid = Convert.ToInt32(shopId_string);
                //排除编辑商店自身的店名
                shopList = shopList.Where(a => a.Id != shopid).ToList();
            }
            string cityId = Request["cityid"].ToString();
            shopList = shopList.Where(a => a.CityId == Convert.ToInt32(cityId)).ToList();
            foreach (var name in shopList)
            {
                if (name.Name == shopName)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //前台商铺编号验证
        public ActionResult validateShopCode()
        {
            bool success = true;
            var shopList = (from shop in db.SShop.ToList() select shop).ToList();

            string shopCode_string = Request["captainInfo.shopcode"].ToString();
            int shopCode = Convert.ToInt32(shopCode_string);
            string shopId_string = Request["shopid"];
            if (!string.IsNullOrEmpty(shopId_string))
            {
                int shopid = Convert.ToInt32(shopId_string);
                //排除编辑商店自身的店名
                shopList = shopList.Where(a => a.Id != shopid).ToList();
            }
            foreach (var name in shopList)
            {
                if (name.Code == shopCode)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }

        #endregion

        #endregion

        #region 城市增删查改管理控制
        //新增城市页面
        public ActionResult AddCity()
        {
            ViewBag.flag = 0;
            return View();
        }

        //新增城市操作
        public ActionResult AddCityCon()
        {
            //新增对象
            City city = new City();
            //从页面获取参数
            string officeId = Request.Form.Get("officename");
            string cityName = Request.Form.Get("captainInfo.cityname");
            string cityCode = Request.Form.Get("captainInfo.citycode");

            if (string.IsNullOrEmpty(officeId))
            {
                ModelState.AddModelError("officename", "请选择办事处");
            }
            if (string.IsNullOrEmpty(cityName))
            {
                ModelState.AddModelError("captainInfo.cityname", "请输入城市名称");
            }
            if (string.IsNullOrEmpty(cityCode))
            {
                ModelState.AddModelError("captainInfo.citycode", "请输入城市编号");
            }

            if (ModelState.IsValid)
            {
                city.OfficeId = Convert.ToInt32(officeId);
                city.Name = cityName;
                city.Code = Convert.ToInt32(cityCode);
                db.City.Add(city);
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("AddCity");
        }

        //城市名验证
        public ActionResult ValidateCityName()
        {
            bool success = true;
            var cityList = (from city in db.City.ToList() select city).ToList();

            string cityName = Request["captainInfo.cityname"].ToString();
            string officeId = Request["officeid"];
            string cityid = Request["cityid"];
            //若有cityid则排除自身，此为编辑验证
            if (!string.IsNullOrEmpty(cityid))
            {
                cityList = cityList.Where(a => a.Id != Convert.ToInt32(cityid)).ToList();
            }
            cityList = cityList.Where(a => a.OfficeId == Convert.ToInt32(officeId)).ToList();
            foreach (var name in cityList)
            {
                if (name.Name == cityName)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }

        //城市编号验证
        public ActionResult ValidateCityCode()
        {
            bool success = true;
            var cityList = (from city in db.City.ToList() select city).ToList();
            string officeId = Request["officeid"];
            cityList = cityList.Where(a => a.OfficeId == Convert.ToInt32(officeId)).ToList();
            string cityCode_string = Request["captainInfo.citycode"];
            if (!string.IsNullOrEmpty(cityCode_string))
            {
                int cityCode = Convert.ToInt32(cityCode_string);
                string cityid = Request["cityid"];
                //若有cityid则排除自身，此为编辑验证
                if (!string.IsNullOrEmpty(cityid))
                {
                    cityList = cityList.Where(a => a.Id != Convert.ToInt32(cityid)).ToList();
                }
                foreach (var name in cityList)
                {
                    if (name.Code == cityCode)
                    {
                        success = false;
                        break;
                    }
                }
            }
            return Json(success);
        }

        //城市删除操作
        public ActionResult DeleteCity()
        {
            string cityid_string = Request.Params["CityId"];
            int cityid = Convert.ToInt32(cityid_string);
            List<SShop> shopList = (from s in db.SShop.ToList() where s.CityId == cityid select s).ToList();
            List<AttendingInfo> at = new List<AttendingInfo>();

            //删除商店下的上班信息
            foreach (var item in shopList)
            {
                at = (from a in db.AttendingInfo.ToList() where a.Department == item.Id select a).ToList();
                foreach (var item1 in at)
                {
                    AttendingInfo attend = db.AttendingInfo.Find(item1.Id);
                    db.AttendingInfo.Remove(attend);
                    db.SaveChanges();
                }
                SShop shop = db.SShop.Find(item.Id);
                db.SShop.Remove(shop);
                db.SaveChanges();
            }

            City city = db.City.Find(cityid);
            db.City.Remove(city);
            db.SaveChanges();
            return Json(true);
        }

        //城市编辑页面
        public ActionResult EditCity(int id)
        {
            City city = db.City.Find(id);
            ViewBag.flag = 0;
            return View(city);
        }

        //城市编辑操作
        public ActionResult EditCityCon()
        {
            string cityid_string = Request.Form.Get("cityid");
            int cityid = Convert.ToInt32(cityid_string);
            //修改对象
            City city = db.City.Find(cityid);
            //从页面获取参数
            string officeId = Request.Form.Get("officename");
            string cityName = Request.Form.Get("captainInfo.cityname");
            string cityCode = Request.Form.Get("captainInfo.citycode");

            if (string.IsNullOrEmpty(officeId))
            {
                ModelState.AddModelError("officename", "请选择办事处");
            }
            if (string.IsNullOrEmpty(cityName))
            {
                ModelState.AddModelError("captainInfo.cityname", "请输入城市名称");
            }
            if (string.IsNullOrEmpty(cityCode))
            {
                ModelState.AddModelError("captainInfo.citycode", "请输入城市编号");
            }

            if (ModelState.IsValid)
            {
                city.OfficeId = Convert.ToInt32(officeId);
                city.Name = cityName;
                city.Code = Convert.ToInt32(cityCode);
                db.Entry(city).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("EditCity", city);
        }
        //城市编辑获取办事处
        public ActionResult getEditofficeName1()
        {
            string officename = "";
            //从数据库中获取数据表
            List<Office> officeList = db.Office.ToList();

            string cityid_string = Request.Params["cityid"];
            int cityid = Convert.ToInt32(cityid_string);
            City city = db.City.Find(cityid);
            foreach (var item in officeList)
            {
                if (item.Id == city.OfficeId)
                {
                    officename += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
                else
                {
                    officename += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
            }

            var griddata = new
            {
                officeValue = officename
            };
            return Json(griddata);
        }
        #endregion

        #region 办事处新增管理控制
        //办事处新增页面
        public ActionResult AddOffice()
        {
            ViewBag.flag = 0;
            return View();
        }

        //办事处新增操作
        public ActionResult AddOfficeCon()
        {
            //过滤办事处过滤条件
            Regex Letter = new Regex("[a-zA-Z0-9]");
            //新增对象
            Office office = new Office();
            //从页面获取参数
            string officeName = Request.Form.Get("captainInfo.officename");
            string officeCode = Request.Form.Get("captainInfo.officecode");
            string officeMask = Request.Form.Get("captainInfo.officemask");

            if (string.IsNullOrEmpty(officeName))
            {
                ModelState.AddModelError("captainInfo.officename", "请输入办事处名称");
            }
            if (string.IsNullOrEmpty(officeCode))
            {
                ModelState.AddModelError("captainInfo.officecode", "请输入办事处编号");
            }
            if (string.IsNullOrEmpty(officeMask))
            {
                ModelState.AddModelError("captainInfo.officemask", "请输入办事处代号");
            }
            else if (officeMask.Length > 5)
            {
                ModelState.AddModelError("captainInfo.officemask", "至多输入5个字符");
            }
            else if (!Letter.IsMatch(officeMask))
            {
                ModelState.AddModelError("captainInfo.officemask", "请输入大小写字母、数字");
            }

            if (ModelState.IsValid)
            {
                office.Name = officeName;
                office.Code = Convert.ToInt32(officeCode);
                office.mask = officeMask;
                db.Office.Add(office);
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("AddOffice");
        }

        //办事处名验证
        public ActionResult ValidateOfficeName()
        {
            bool success = true;
            var officeList = (from office in db.Office.ToList() select office).ToList();
            string officeName = Request["captainInfo.officename"];
            if (!string.IsNullOrEmpty(officeName))
            {
                foreach (var name in officeList)
                {
                    if (name.Name == officeName)
                    {
                        success = false;
                        break;
                    }
                }
            }
            return Json(success);
        }

        //办事处编号验证
        public ActionResult ValidateOfficeCode()
        {
            bool success = true;
            var officeList = (from city in db.Office.ToList() select city).ToList();
            string officeCode_string = Request["captainInfo.officecode"];
            if (!string.IsNullOrEmpty(officeCode_string))
            {
                int officeCode = Convert.ToInt32(officeCode_string);
                foreach (var name in officeList)
                {
                    if (name.Code == officeCode)
                    {
                        success = false;
                        break;
                    }
                }
            }
            return Json(success);
        }

        //办事处缩写验证
        public ActionResult ValidateOfficeMask()
        {
            Regex Letter = new Regex("[a-zA-Z0-9]");
            bool success = true;
            var officeList = (from city in db.Office.ToList() select city).ToList();
            string officeMask_string = Request["captainInfo.officemask"];
            if (!Letter.IsMatch(officeMask_string))
            {
                success = false;
            }
            return Json(success);
        }
        #endregion

        #region 办事处修改管理控制

        //办事处修改页面
        public ActionResult EditOffice(int officeId)
        {
            Office Office = db.Office.Find(officeId);
            ViewBag.flag = 0;
            return View(Office);
        }

        //办事处修改操作
        public ActionResult EditOfficeCon()
        {
            string officeId_string = Request.Form.Get("officeid");
            int officeid = Convert.ToInt32(officeId_string);
            //修改对象
            Office office = db.Office.Find(officeid);
            //从页面获取参数
            string officeName = Request.Form.Get("captainInfo.officename");
            string officeCode = Request.Form.Get("captainInfo.officecode");
            string officeMask = Request.Form.Get("captainInfo.officemask");

            if (string.IsNullOrEmpty(officeName))
            {
                ModelState.AddModelError("captainInfo.officename", "请输入办事处名称");
            }
            if (string.IsNullOrEmpty(officeCode))
            {
                ModelState.AddModelError("captainInfo.officecode", "请输入办事处编号");
            }
            if (string.IsNullOrEmpty(officeMask))
            {
                ModelState.AddModelError("captainInfo.officemask", "请输入办事处代号");
            }

            if (ModelState.IsValid)
            {
                office.Name = officeName;
                office.Code = Convert.ToInt32(officeCode);
                office.mask = officeMask;
                db.Entry(office).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.flag = 1;
            }
            return View("EditOffice", office);
        }
        //编辑办事处名字验证
        public ActionResult ValidateOfficeName1()
        {
            bool success = true;
            string officeName = Request["captainInfo.officename"].ToString();
            string officeId = Request["officeid"];
            var officeList = (from office1 in db.Office.ToList() select office1).ToList();
            officeList = officeList.Where(a => a.Id != Convert.ToInt32(officeId)).ToList();
            var office = (from s in officeList where s.Name == officeName select s).FirstOrDefault();
            if (office != null)
            {
                success = false;
            }


            return Json(success);          
        }

        //编辑办事处编号验证
        public ActionResult ValidateOfficeCode1()
        {
            bool success = true;
            string officeCode_string = Request["captainInfo.officecode"].ToString();
            int officeCode = Convert.ToInt32(officeCode_string);
            string officeId = Request["officeid"];
            var officeList = (from office1 in db.Office.ToList() select office1).ToList();
            officeList = officeList.Where(a => a.Id != Convert.ToInt32(officeId)).ToList();
            var office = (from s in officeList where s.Code == officeCode select s).FirstOrDefault();
            if (office != null)
            {
                success = false;
            }
            return Json(success);
        }

        //编辑办事处代号验证
        public ActionResult ValidateOfficeMask1()
        {
            bool success = true;
            string officeMask = Request["captainInfo.officemask"].ToString();
            string officeId = Request["officeid"];
            var officeList = (from office1 in db.Office.ToList() select office1).ToList();
            officeList = officeList.Where(a => a.Id != Convert.ToInt32(officeId)).ToList();
            var office = (from s in officeList where s.mask == officeMask select s).FirstOrDefault(); 
            if (office != null)
            {
                success = false;
            }
            return Json(success);
        }
        #endregion
    }
}
