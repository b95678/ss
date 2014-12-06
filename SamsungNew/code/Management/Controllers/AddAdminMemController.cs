using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Data.Entity;
using System.Data;
using Management.filter;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;

namespace Management.Controllers
{
    public class AddAdminMemController : Controller
    {
        SS_HRM_DBEntities db = new SS_HRM_DBEntities();
        //
        // GET: /AddAdminMem/
        /*查看管理人员视图*/
        [Management.filter.LoginFilter]
        public ActionResult CheckMan()
        {
            if (Session["authority"].ToString() == "管理员" || Session["authority"].ToString() == "督导")
            {
                return View();
            }
            else
            {
                return View("~/Views/Shared/AuthorityError.cshtml");
            }
        }

        #region 获取信息
        /*获取人员表信息*/
        class getMan
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
            public string roles { get; set; }
            public string supervisor { get; set; }
            public string offices { get; set; }
            public string sex { get; set; }
            public string city { get; set; }
            public string mobile { get; set; }
            public string insertuser { get; set; }
            public string updateuser { get; set; }
        }

        /*获取人员信息*/
        public ActionResult GetData()
        {
            //获取查询人员的类型和用户名
            string authority = Session["authority"].ToString();
            string user=Session["admin"].ToString();
            //新建一个表
            IList<getMan> data1 = new List<getMan>();
            /*获取manager表的数据，并列成表*/
            List<Managers> data = db.Managers.ToList();
            //获取非草稿箱的数据
            data = (from dataFilter in data where dataFilter.IsDraft == false select dataFilter).ToList();
            if (authority == "督导")
            {
                data = data.Where(a => a.UserId == user || a.CreatedManId == user).ToList();
            }
            //当前页
            int page = Convert.ToInt32(Request.Params["page"]);
            if (page <= 0) { page = 1; }//防止小于1页
            //每页显示的记录数
            int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
            //选取类型
            string key = Request.Params["key"];
            //选取内容
            string content = Request.Params["content"];

            //查看全部
            if (key == "all")
            { data = (from dataFilter in data select dataFilter).ToList(); }
            //查看用户名
            if (key == "username")
            {
                if (content.Length == 0) { Response.Write("<script>alert('请输入内容')</script>"); }
                else
                {
                    content = content.ToLower();
                    data = data.Where(dataFilter => dataFilter.UserId.ToLower().Contains(content)).ToList();
                }
            }
            //查看姓名
            if (key == "name")
            {
                if (content.Length == 0) { Response.Write("<script>alert('请输入内容')</script>"); }
                else
                {
                    data = data.Where(dataFilter => dataFilter.Name.Contains(content)).ToList();

                }
            }
            //查看性别
            if (key == "sex")
            {
                if (content.Length == 0) { Response.Write("<script>alert('请输入内容')</script>"); }
                if (content == "男")
                {
                    data = data.Where(dataFilter => dataFilter.Sex == true).ToList();
                }
                if (content == "女")
                {
                    data = data.Where(dataFilter => dataFilter.Sex == false).ToList();
                }
            }
            //查看类型
            if (key == "role")
            {
                if (content.Length == 0) { Response.Write("<script>alert('请输入内容')</script>"); }
                else
                {
                    data = data.Where(dataFilter => dataFilter.Authority1.Name.Contains(content)).ToList();
                }

            }
            //查看办事处
            if (key == "office")
            {
                if (content.Length == 0) { Response.Write("<script>alert('请输入内容')</script>"); }
                else
                {
                    data = data.Where(dataFilter => dataFilter.City1.Office.Name.Contains(content)).ToList();
                }

            }



          

            var data2 = data.OrderBy(a=>a.Authority).Skip(pagesize * (page - 1)).Take(pagesize).ToList();

            //对data1赋值
            foreach (var item in data2)
            {
                string gender = null;
                if (item.Sex == true)
                { gender = "男"; }
                else if (item.Sex == false)
                { gender = "女"; }
                if (item.City1!=null)
                {
                    if (item.IsDelete == false||item.IsDelete==null)
                    {
                        getMan gm=(new getMan()
                        {
                            id = item.Id,
                            username = item.UserId,
                            name = item.Name,
                            roles = item.Authority1.Name,
                            supervisor = item.Boss,
                            mobile = item.Telephone,
                            insertuser = item.CreatedManId,
                            updateuser = item.EditManId,
                            city = item.City1.Name,
                            offices = item.City1.Office.Name
                        });
                        data1.Add(gm);
                    }
                }
            }

            //总页数
            int totalPage = data1.Count() / pagesize;
            if (data1.Count() % pagesize != 0)
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
                gdata = data1,
                page = page,
                totalPage = totalPage,
                pageBtn = pageBtnClass,
                pageMsg = "第" + page.ToString() + "页，共" + totalPage.ToString() + "页，共" + data1.Count().ToString() + "条记录",
                pageSize = pagesize
            };
            return Json(gridData);
        }

        /*增加页面 获取类型和办事处*/
        public ActionResult GetRoleAndOffice()
        {
            string role = "";
            string office = "";
            string city = "";

            string authority = Session["authority"].ToString();
            string user=Session["admin"].ToString();
            //从数据库中获取数据表
            List<Authority> roleList = db.Authority.ToList();
            //roleList = roleList.Where(a => a.Name != "管理员").ToList();    //不可创建管理员身份
            List<Office> officeList = db.Office.ToList();
            List<City> cityList = db.City.ToList();

            List<Office> officeList1 = db.Office.Take(1).ToList();
            Managers man = (from s in db.Managers where s.UserId == user select s).FirstOrDefault(); 

            //给对象赋值
            //类型
            if (authority == "管理员")
            {
                foreach (var item in roleList)
                {
                    if (item.Id == 1)
                    {
                        role += "<option value=\"" + item.Id + "\" selected='true'>" + item.Name + "</option>";
                    }
                    else
                    {
                        role += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                    }

                }

                //办事处
                foreach (var item in officeList)
                {
                    office += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }

                foreach (var item in officeList1)
                {
                    cityList = cityList.Where(cityFilter => cityFilter.OfficeId == item.Id).ToList();
                    foreach (var a in cityList)
                    {
                        city += "<option value=\"" + a.Id + "\">" + a.Name + "</option>";
                    }
                    break;
                }
            }
            else if (authority == "督导")
            {
                role = "<option value=\"2\" selected='true'>小队长</option>";
                office = "<option value=\"" + man.City1.OfficeId + "\">" + man.City1.Office.Name + "</option>";
                cityList = cityList.Where(cityFilter => cityFilter.OfficeId == man.City1.OfficeId).ToList();
                foreach (var a in cityList)
                {
                    city += "<option value=\"" + a.Id + "\">" + a.Name + "</option>";
                }
            }
            //给前台所需数据赋值
            var data = new
            {
                roleValue = role,
                officeValue = office,
                cityValue = city
            };
            return Json(data);
        }

        /*查看某人信息 获取类型和办事处*/
        public ActionResult GetManRoleAndOffice()
        {
            string role = "";
            string office = "";
            string city = "";
            string boss = "";
            //从数据库中获取数据表
            string id_string = Request["id"].ToString();
            Guid id = new Guid(id_string);
            Managers man = db.Managers.Find(id);

            List<Authority> roleList = db.Authority.ToList();
            List<Office> officeList = db.Office.ToList();
            List<City> cityList = db.City.ToList();
            var bossList = db.Managers.Where(a => a.Authority != 0 && a.Authority != 2).ToList();


            List<Office> officeList1 = (from office1 in db.Office.ToList() where office1.Id == man.City1.OfficeId select office1).ToList();


            //给对象赋值
            foreach (var item in roleList)
            {
                if (man.Authority == item.Id)
                {
                    if (Session["authority"].ToString() =="管理员"&&man.Authority!=0)
                    {
                        role += "<option selected=\"selected\" value=\"0\">管理员</option>";
                        role += "<option selected=\"selected\" value=\"" + item.Id + "\">" + man.Authority1.Name + "</option>";
                    }
                    else
                    {
                        role += "<option selected=\"selected\" value=\"" + item.Id + "\">" + man.Authority1.Name + "</option>";
                    }

                    foreach (var a in bossList)
                    {
                        if (man.Boss == item.Name)
                        { boss += "<option selected=\"selected\" value=\"" + a.UserId + "\">" + a.UserId + "</option>"; }
                        else
                        {
                            boss += "<option value=\"" + a.UserId + "\">" + a.UserId + "</option>";
                        }
                    }
                }
                else
                {
                   
                    if (item.Id != 0)
                    { role += "<option value=\"" + item.Id + "\">" + item.Name + "</option>"; }
                }
            }
            foreach (var item in officeList)
            {
                if (man.City1.Office.Id == item.Id)
                {
                    office += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>";

                }
                else
                {
                    office += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
            }
            foreach (var item in officeList1)
            {
                cityList = cityList.Where(cityFilter => cityFilter.OfficeId == item.Id).ToList();
                foreach (var a in cityList)
                {
                    if (man.City == a.Id)
                    {
                        city += "<option selected=\"selected\" value=\"" + a.Id + "\">" + a.Name + "</option>";
                    }
                    else
                    {
                        city += "<option value=\"" + a.Id + "\">" + a.Name + "</option>";
                    }
                }
                break;
            }

            //给前台所需数据赋值
            var data = new
            {
                roleValue = role,
                officeValue = office,
                cityValue = city,
                role = man.Authority,
                boss = boss,
            };
            return Json(data);

        }

        /*获取城市*/
        public ActionResult getCity(string officeId)
        {
            int oId = Convert.ToInt32(officeId);
            string city = "";
            List<City> cityList = db.City.ToList();
            cityList = cityList.Where(dataFilter => dataFilter.OfficeId == oId).ToList();
            foreach (var item in cityList)
            {
                city += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
            }
            var data = new
            {
                cityValue = city
            };
            return Json(data);
        }

        /*获取上级*/
        public ActionResult getBoss()
        {
            //获取权限
            string authority = Session["authority"].ToString();
            string user = Session["admin"].ToString();
            string boss = "";
            if (authority == "管理员")
            {
                
                var bossList = db.Managers.Where(a => a.Authority ==1).ToList();
                string BossValue = Request.Params["bossValue"];
                foreach (var item in bossList)
                {
                    if (!string.IsNullOrEmpty(BossValue))
                    {
                        if (BossValue == item.Name)
                        { 
                            boss += "<option selected=\"selected\" value=\"" + item.UserId + "\">" + item.UserId + "</option>";
                        }
                        else
                        {
                            boss += "<option value=\"" + item.UserId + "\">" + item.UserId + "</option>";
                        }
                    }
                    else
                    {
                        boss += "<option selected=\"selected\" value=\"" + item.UserId + "\">" + item.UserId + "</option>";
                    }
                }

            }
            else if (authority == "督导")
            {
                boss = "<option selected=\"selected\" value=\"" + user + "\">" + user + "</option>";

            } 
            var data = new
                {
                    boss = boss
                };
                return Json(data);
        }
        #endregion

        #region 增加人员操作
        /*增加人员信息视图*/
        [Management.filter.LoginFilter]
        public ActionResult AddManInfo()
        {
            if (Session["authority"].ToString() == "管理员" || Session["authority"].ToString() == "督导")
            {
                List<Bank> bankList = db.Bank.ToList();
                ViewBag.Bank = bankList;
                ViewBag.flag = 0;
                return View();
            }
            else { return View("~/Views/Shared/AuthorityError.cshtml"); }
        }

        /*提交人员信息*/
        [HttpPost]
        public ActionResult AddManInfo(Managers man)
        {
            List<Bank> bankList = db.Bank.ToList();
            ViewBag.Bank = bankList;
            string admin = Session["admin"].ToString();

            //正则表达式验证
            Regex r = new Regex("[0-9]");
            Regex idc = new Regex("^[1-9][0-9]{17}$|^[1-9][0-9]{16}[X]$|^[1-9][0-9]{16}[x]$");//身份证格式
            Regex mail = new Regex("[1-9a-zA-Z]{1,}[@][1-9a-zA-Z]{1,}[.][a-zA-Z]");           //邮箱格式

            #region 从form中获取数据

            string username = Request.Form.Get("captainInfo.username");                      //用户名
            ViewBag.username = username;
            if (string.IsNullOrEmpty(username))                                              //用户名为空的验证
            {
                ModelState.AddModelError("captainInfo.username", "请输入用户名");
            }
            if (!validateUsernameBack(username))                                             //用户名重名验证
            {
                ModelState.AddModelError("captainInfo.username", "用户名已存在");
            }
            string password = Request.Form.Get("password1");                      //密码
            ViewBag.password = password;
            if (string.IsNullOrEmpty(password))                                              //密码不能为空
            {
                ModelState.AddModelError("captainInfo.password", "请输入密码");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("captainInfo.password", "密码不能少于6位");         //密码不能少于6位
            }
            string office = Request.Form.Get("captainInfo.office");                          //办事处
            ViewBag.office = office;
            if (string.IsNullOrEmpty(office))                                                //办事处不能为空
            {
                ModelState.AddModelError("captainInfo.office", "请选择办事处");
            }
            string city = Request.Form.Get("captainInfo.city");                              //城市
            ViewBag.city = city;
            if (string.IsNullOrEmpty(city))                                                  //城市不能为空
            {
                ModelState.AddModelError("captainInfo.city", "请选择城市");
            }
            string role = Request.Form.Get("captainInfo.role");                              //类型
            ViewBag.role = role;
            if (string.IsNullOrEmpty(role))                                                  //类型不能为空
            {
                ModelState.AddModelError("captainInfo.role", "请选择类型");
            }
            string name = Request.Form.Get("captainInfo.name");                              //真实姓名
            ViewBag.name = name;
            if (string.IsNullOrEmpty(name))                                                  //真实姓名不能为空
            {
                ModelState.AddModelError("captainInfo.name", "请输入真实姓名");
            }
            string gender = Request.Form.Get("captainInfo.sex");                             //性别
            string sex;
            if (gender == "true") sex = "男";
            else sex = "女";
            ViewBag.sex = "<option selected=\"selected\" value=" + gender + ">" + sex + "</option>";
            if (string.IsNullOrEmpty(gender))                                                //性别不能为空
            {
                ModelState.AddModelError("captainInfo.sex", "请选择性别");
            }
            string Mobile = Request.Form.Get("captainInfo.mobile");                          //电话
            ViewBag.telephone = Mobile;
            if (string.IsNullOrEmpty(Mobile))                                                //电话不能为空
            {
                ModelState.AddModelError("captainInfo.mobile", "请输入电话");
            }
            else if (Mobile.Length != 11)
            {
                ModelState.AddModelError("captainInfo.mobile", "电话必须是11位");
            }
            string boss = Request.Form.Get("captainInfo.supervisor");                        //上级
            string email = Request.Form.Get("captainInfo.mail");                             //邮箱
            ViewBag.email = email;
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("captainInfo.mail", "请输入邮箱地址");             //邮箱不能为空
            }
            else if (!mail.IsMatch(email))
            {
                ModelState.AddModelError("captainInfo.mail", "请输入正确的邮箱地址");       //邮箱规范要正确
            }
            string idCard = Request.Form.Get("captainInfo.identity");                        //身份证号
            ViewBag.idCard = idCard;
            if (string.IsNullOrEmpty(idCard))                                                //身份证号不能为空
            {
                ModelState.AddModelError("captainInfo.identity", "请输入身份证号");
            }
            else if (idCard.Length != 18)
            {
                ModelState.AddModelError("captainInfo.identity", "身份证号必须是18位");      //身份证号必须是18位
            }
            else if (!validateIdentityBack(idCard))
            {
                ModelState.AddModelError("captainInfo.identity", "身份证号已存在");
            }
            else if (!idc.IsMatch(idCard))
            {
                ModelState.AddModelError("captainInfo.identity", "请输入正确的身份证格式");  //验证身份证格式
            }
            string bankaccount = Request.Form.Get("captainInfo.bank");                       //银行卡号
            ViewBag.bankAccount = bankaccount;
            if (string.IsNullOrEmpty(bankaccount))                                           //银行卡号不能为空
            {
                ModelState.AddModelError("captainInfo.bank", "请输入银行卡号");
            }
            else if (!validateBankBack(bankaccount))
            {
                ModelState.AddModelError("captainInfo.bank", "银行账号已存在");
            }
            string bank = Request.Form.Get("captainInfo.account");                           //银行名称(开户行)
            if (string.IsNullOrEmpty(bank))                                                  //银行名称不能为空
            {
                ModelState.AddModelError("captainInfo.account", "请选择银行");
            }
            string school = Request.Form.Get("captainInfo.school");                          //学校
            ViewBag.school = school;
            if (string.IsNullOrEmpty(school))                                                //学校不能为空
            {
                ModelState.AddModelError("captainInfo.account", "请输入学校");
            }
            string major = Request.Form.Get("captainInfo.major");                            //专业
            ViewBag.major = major;
            if (string.IsNullOrEmpty(major))                                                 //专业不能为空
            {
                ModelState.AddModelError("captainInfo.major", "请输入专业");
            }
            string graduatedate = Request.Form.Get("captainInfo.graduationdate");            //毕业时间
            ViewBag.graduate = graduatedate;
            if (string.IsNullOrEmpty(graduatedate))                                                 //毕业时间不能为空
            {
                ModelState.AddModelError("captainInfo.graduationdate", "请输入毕业时间");
            }
            string academic = Request.Form.Get("captainInfo.education");                     //学历
            ViewBag.academic = academic;
            if (string.IsNullOrEmpty(academic))                                              //学历不能为空
            {
                ModelState.AddModelError("captainInfo.education", "请输入学历");
            }
            string height = Request.Form.Get("captainInfo.height");                          //身高
            ViewBag.height = height;
            if (!string.IsNullOrEmpty(height) && !r.IsMatch(height))
            {
                ModelState.AddModelError("captainInfo.height", "身高请输入整数");
            }
            string weight = Request.Form.Get("captainInfo.weight");                          //体重
            ViewBag.weight = weight;
            if (!string.IsNullOrEmpty(weight) && !r.IsMatch(weight))
            {
                ModelState.AddModelError("captainInfo.weight", "体重请输入整数");
            }
            Regex x = new Regex("^[1-9][0-9]{1,2}[,][1-9][0-9]{0,2}[,][1-9][0-9]{0,2}$");
            string BWH = Request.Form.Get("captainInfo.meas");                               //三围
            ViewBag.BWH = BWH;
            if (!string.IsNullOrEmpty(BWH) && !x.IsMatch(BWH))
            {
                ModelState.AddModelError("captainInfo.meas", "请规范输入三围，格式:66,44,66");
            }
            string speciality = Request.Form.Get("captainInfo.skill");                       //特长
            ViewBag.speciality = speciality;
            HttpPostedFileBase personalPhoto = Request.Files["personalImage"];               //个人照
            if (!string.IsNullOrEmpty(personalPhoto.FileName))
            {
                if (!PicExtend(personalPhoto.FileName))
                {
                    ModelState.AddModelError("personalImage", "图片格式只能是jpeg、jpg、png、bmp、gif");
                }
            }
            HttpPostedFileBase studentPhoto = Request.Files["studentImage"];                 //学生证照片
            if (!string.IsNullOrEmpty(studentPhoto.FileName))
            {
                if (!PicExtend(studentPhoto.FileName))
                {
                    ModelState.AddModelError("studentImage", "图片格式只能是jpeg、jpg、png、bmp、gif");
                }
            }

            #endregion


            if (ModelState.IsValid)
            {
                var kman = (from a in db.Managers where a.IsDraft == true && a.UserId == username select a).FirstOrDefault();
                if (kman == null)
                {
                    //保存至数据库
                    man.Id = Guid.NewGuid();
                    man.UserId = username;
                    man.Password = password;
                    man.City = Convert.ToInt32(city);
                    man.Authority = Convert.ToInt32(role);
                    man.Name = name;
                    man.Sex = Convert.ToBoolean(gender);
                    man.Telephone = Mobile;
                    man.IDcard = idCard;
                    if (!string.IsNullOrEmpty(boss))
                    { man.Boss = boss; }
                    man.Email = email;
                    man.BankCard = bankaccount;
                    man.Bank = Convert.ToInt32(bank);
                    man.School = school;
                    man.Major = major;
                    man.Graduate = Convert.ToDateTime(graduatedate);
                    man.Academic = academic;
                    man.CreateTime = DateTime.Now;
                    man.CreatedManId = admin;
                    man.EditManId = admin;
                    man.IsDelete = false;
                    if (!string.IsNullOrEmpty(height))
                    {
                        man.height = Convert.ToInt32(height);
                    }
                    if (!string.IsNullOrEmpty(weight))
                    {
                        man.weight = Convert.ToInt32(weight);
                    }
                    if (!string.IsNullOrEmpty(BWH))
                    {
                        man.BWH = BWH;
                    }
                    if (!string.IsNullOrEmpty(speciality))
                    {
                        man.Speciality = speciality;
                    }
                    //上传图片
                    if (!string.IsNullOrEmpty(personalPhoto.FileName))
                    {
                        string file = PicName(personalPhoto.FileName);
                        personalPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/personalImg/yuan/") + file);
                        file = TouXiangSuoFang(personalPhoto, file, Server.MapPath("~/uploadImg/managerImg/personalImg/suo/"), 114, 125);
                        man.Photo = file;
                    }
                    if (!string.IsNullOrEmpty(studentPhoto.FileName))
                    {
                        string file = studentPicName(studentPhoto.FileName);                          //检查格式防止重名
                        studentPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/studentImg/yuan/") + file);
                        file = TouXiangSuoFang(studentPhoto, file, Server.MapPath("~/uploadImg/managerImg/studentImg/suo/"), 114, 125);
                        man.StudentCardPhoto = file;
                    }
                    man.CreateTime = DateTime.Now;
                    man.IsDraft = false;
                    db.Managers.Add(man);
                    db.SaveChanges();
                    ViewBag.flag = 1;
                }
                else
                {
                    //提交草稿箱的内容。
                    Managers sman = db.Managers.Find(kman.Id);
                    sman.UserId = username;
                    sman.Password = password;
                    sman.City = Convert.ToInt32(city);
                    sman.Authority = Convert.ToInt32(role);
                    sman.Name = name;
                    sman.Sex = Convert.ToBoolean(gender);
                    sman.Telephone = Mobile;
                    if (!string.IsNullOrEmpty(boss))
                    { sman.Boss = boss; }
                    sman.Email = email;
                    sman.IDcard = idCard;
                    sman.BankCard = bankaccount;
                    sman.Bank = Convert.ToInt32(bank);
                    sman.School = school;
                    sman.Major = major;
                    sman.Graduate = Convert.ToDateTime(graduatedate);
                    sman.Academic = academic;
                    sman.CreateTime = DateTime.Now;
                    sman.CreatedManId = admin;
                    sman.EditManId = admin;
                    if (!string.IsNullOrEmpty(height))
                    {
                        sman.height = Convert.ToInt32(height);
                    }
                    if (!string.IsNullOrEmpty(weight))
                    {
                        sman.weight = Convert.ToInt32(weight);
                    }
                    if (!string.IsNullOrEmpty(BWH))
                    {
                        sman.BWH = BWH;
                    }
                    if (!string.IsNullOrEmpty(speciality))
                    {
                        man.Speciality = speciality;
                    }
                    //上传图片
                    if (!string.IsNullOrEmpty(personalPhoto.FileName))
                    {
                        string file = PicName(personalPhoto.FileName);
                        personalPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/personalImg/yuan/") + file);
                        file = TouXiangSuoFang(personalPhoto, file, Server.MapPath("~/uploadImg/managerImg/personalImg/suo/"), 114, 125);
                        sman.Photo = file;
                    }
                    if (!string.IsNullOrEmpty(studentPhoto.FileName))
                    {
                        string file = studentPicName(studentPhoto.FileName);                          //检查格式防止重名
                        studentPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/studentImg/yuan/") + file);
                        file = TouXiangSuoFang(studentPhoto, file, Server.MapPath("~/uploadImg/managerImg/studentImg/suo/"), 114, 125);
                        sman.StudentCardPhoto = file;
                    }
                    sman.IsDraft = false;
                    db.Entry(sman).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.flag = 1;
                }
                ViewBag.username = "";
                ViewBag.password = "";
                ViewBag.office = "";
                ViewBag.city = "";
                ViewBag.role = "";
                ViewBag.name = "";
                ViewBag.sex = "";
                ViewBag.telephone = "";
                ViewBag.idCard = "";
                ViewBag.email = "";
                ViewBag.bankAccount = "";
                ViewBag.school = "";
                ViewBag.graduate = "";
                ViewBag.academic = "";
                ViewBag.height = "";
                ViewBag.weight = "";
                ViewBag.BWH = "";
                ViewBag.speciality = "";
                return View();

            }

            return View(man);



        }

        /*判断有无草稿*/
        public ActionResult hasDraft()
        {
            int hasDraft = 0;
            string name = Session["admin"].ToString();
            Managers man = (from s in db.Managers.ToList() where s.IsDraft == true && s.CreatedManId == name select s).FirstOrDefault();
            if (man != null)
            {
                hasDraft = 1;
            }
            var data = new
            {
                hasDraft = hasDraft.ToString()
            };
            return Json(data);
        }

        #endregion

        #region 草稿箱
        /*提交草稿箱*/
        public ActionResult saveDraft()
        {
            Managers man = new Managers();
            string errormessage = "";
            string admin = Session["admin"].ToString();                                     //保存创建人
            bool success = true;
            #region 从网页中获取数据

            string username = Request.Params["username"];                                     //用户名
            if (string.IsNullOrEmpty(username))
            {
                success = false;
                errormessage += "用户名不能为空！<br>";

            }
            //用户名重名验证
            else if (!validateUsernameBack(username))
            {
                success = false;
                errormessage += "用户名已存在！<br>";
            }
            string password = Request.Params.Get("password");                                //密码
            if (!string.IsNullOrEmpty(password) && password.Length < 6)
            {
                success = false;                                                             //密码不能少于6位
                errormessage += "密码不能少于6位！<br>";
            }
            string office = Request.Params.Get("office");                          //办事处
            string city = Request.Params.Get("city");                              //城市
            string role = Request.Params.Get("role");                              //类型
            string name = Request.Params.Get("name");                              //真实姓名
            string gender = Request.Params.Get("sex");                             //性别
            string Mobile = Request.Params.Get("telephone");                          //电话
            if (!string.IsNullOrEmpty(Mobile) && Mobile.Length != 11)
            {
                success = false;
                errormessage += "密码不能少于6位！<br>";
            }
            string boss = Request.Params.Get("boss");                        //上级
            string email = Request.Params.Get("mail");                             //邮箱
            string idCard = Request.Params.Get("identity");                        //身份证号
            if (!string.IsNullOrEmpty(idCard) && idCard.Length != 18)
            {
                success = false;                                                             //身份证号必须是18位
                errormessage += "身份证号必须是18位！<br>";
            }
            else if (!validateIdentityBack(idCard))
            {
                success = false;
                errormessage += "此身份证已存在！<br>";//此身份证已存在。
            }
            string bankaccount = Request.Params.Get("bank");                       //银行卡号
            string bank = Request.Params.Get("account");                           //银行名称(开户行)
            string school = Request.Params.Get("school");                          //学校
            string major = Request.Params.Get("major");                            //专业
            string graduatedate = Request.Params.Get("graduationdate");            //毕业时间
            string academic = Request.Params.Get("academic");                     //学历
            string height = Request.Params.Get("height");                          //身高
            string weight = Request.Params.Get("weight");                          //体重
            string BWH = Request.Params.Get("BWH");                               //三围
            string speciality = Request.Params.Get("speciality");                       //特长

            #endregion
            if (success == true)
            {
                var mana = (from b in db.Managers where b.UserId == username select b).FirstOrDefault();
                if (mana != null)
                {
                    man = mana;
                    man.UserId = username;
                    if (!string.IsNullOrEmpty(password))
                    {
                        man.Password = password;
                    }
                    if (!string.IsNullOrEmpty(city))
                    {
                        man.City = Convert.ToInt32(city);
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        man.Authority = Convert.ToInt32(role);
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        man.Name = name;
                    }
                    if (!string.IsNullOrEmpty(gender))
                    {
                        man.Sex = Convert.ToBoolean(gender);
                    }
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        man.Telephone = Mobile;
                    }
                    if (!string.IsNullOrEmpty(boss))
                    {
                        man.Boss = boss;
                    }
                    if (!string.IsNullOrEmpty(email))
                    {
                        man.Email = email;
                    }
                    if (!string.IsNullOrEmpty(idCard))
                    {
                        man.IDcard = idCard;
                    }
                    if (!string.IsNullOrEmpty(bankaccount))
                    {
                        man.BankCard = bankaccount;
                    }
                    if (!string.IsNullOrEmpty(bank))
                    {
                        man.Bank = Convert.ToInt32(bank);
                    }
                    if (!string.IsNullOrEmpty(school))
                    {
                        man.School = school;
                    }
                    if (!string.IsNullOrEmpty(major))
                    {
                        man.Major = major;
                    }
                    if (!string.IsNullOrEmpty(graduatedate))
                    {
                        man.Graduate = Convert.ToDateTime(graduatedate);
                    }
                    if (!string.IsNullOrEmpty(academic))
                    {
                        man.Academic = academic;
                    }
                    if (!string.IsNullOrEmpty(height))
                    {
                        man.height = Convert.ToInt32(height);
                    }
                    if (!string.IsNullOrEmpty(weight))
                    {
                        man.weight = Convert.ToInt32(weight);
                    }
                    if (!string.IsNullOrEmpty(BWH))
                    {
                        man.BWH = BWH;
                    }
                    if (!string.IsNullOrEmpty(speciality))
                    {
                        man.Speciality = speciality;
                    }
                    man.CreateTime = DateTime.Now;
                    man.IsDraft = true;
                    db.Entry(man).State = EntityState.Modified;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;

                    var data = new
                    {
                        Value = success,
                        error = errormessage
                    };
                    return Json(data);
                }
                else
                {
                    man.Id = Guid.NewGuid();
                    man.UserId = username;
                    if (!string.IsNullOrEmpty(password))
                    {
                        man.Password = password;
                    }
                    if (!string.IsNullOrEmpty(city))
                    {
                        man.City = Convert.ToInt32(city);
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        man.Authority = Convert.ToInt32(role);
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        man.Name = name;
                    }
                    if (!string.IsNullOrEmpty(gender))
                    {
                        man.Sex = Convert.ToBoolean(gender);
                    }
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        man.Telephone = Mobile;
                    }
                    if (!string.IsNullOrEmpty(boss))
                    {
                        man.Boss = boss;
                    }
                    if (!string.IsNullOrEmpty(idCard))
                    {
                        man.IDcard = idCard;
                    }
                    if (!string.IsNullOrEmpty(bankaccount))
                    {
                        man.BankCard = bankaccount;
                    }
                    if (!string.IsNullOrEmpty(bank))
                    {
                        man.Bank = Convert.ToInt32(bank);
                    }
                    if (!string.IsNullOrEmpty(school))
                    {
                        man.School = school;
                    }
                    if (!string.IsNullOrEmpty(major))
                    {
                        man.Major = major;
                    }
                    if (!string.IsNullOrEmpty(graduatedate))
                    {
                        man.Graduate = Convert.ToDateTime(graduatedate);
                    }
                    if (!string.IsNullOrEmpty(academic))
                    {
                        man.Academic = academic;
                    }
                    if (!string.IsNullOrEmpty(height))
                    {
                        man.height = Convert.ToInt32(height);
                    }
                    if (!string.IsNullOrEmpty(weight))
                    {
                        man.weight = Convert.ToInt32(weight);
                    }
                    if (!string.IsNullOrEmpty(BWH))
                    {
                        man.BWH = BWH;
                    }
                    if (!string.IsNullOrEmpty(speciality))
                    {
                        man.Speciality = speciality;
                    }
                    man.CreateTime = DateTime.Now;
                    man.IsDraft = true;
                    db.Managers.Add(man);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;

                    var data = new
                    {
                        Value = success,
                        error = errormessage
                    };
                    return Json(data);
                }
            }
            var data1 = new
            {
                Value = success,
                error = errormessage
            };
            return Json(data1);

        }
        //获取草稿
        public ActionResult getDraft()
        {
            string role = "";
            string office = "";
            string city = "";
            string boss = "";
            string UserId_string = Session["admin"].ToString();
            //获取所需数据表
            Managers manager = db.Managers.Where(a => a.IsDraft == true && a.CreatedManId == UserId_string).OrderByDescending(a => a.CreateTime).FirstOrDefault();
            List<Authority> roleList = db.Authority.ToList();
            List<Office> officeList = db.Office.ToList();
            List<City> cityList = db.City.ToList();

            if (manager != null)
            {
                #region 获取城镇和类型
                if (manager.City != null)
                {
                    List<Office> officeList1 = (from office1 in db.Office.ToList() where office1.Id == manager.City1.OfficeId select office1).ToList();
                    foreach (var item in officeList1)
                    {
                        cityList = cityList.Where(cityFilter => cityFilter.OfficeId == item.Id).ToList();
                        foreach (var a in cityList)
                        {
                            if (manager.City == a.Id)
                            {
                                city += "<option selected=\"selected\" value=\"" + a.Id + "\">" + a.Name + "</option>";
                            }
                            else
                            {
                                city += "<option value=\"" + a.Id + "\">" + a.Name + "</option>";
                            }
                        }
                        break;
                    }

                }
                foreach (var item in roleList)
                {
                    if (manager.Authority == item.Id)
                    { role += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>"; }
                    else
                    {
                        role += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                    }
                }
                foreach (var item in officeList)
                {
                    if (manager.City1.Office.Id == item.Id)
                    {
                        office += "<option selected=\"selected\" value=\"" + item.Id + "\">" + item.Name + "</option>";

                    }
                    else
                    {
                        office += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                    }
                }
                string date = "";
                if (manager.Graduate != null)
                {
                    date = DateTime.Parse(manager.Graduate.ToString()).ToString("yyyy年MM月");

                }
                #endregion
                var bossList = db.Managers.Where(a => a.Authority != 0 && a.Authority != 2).ToList();
                foreach (var item in bossList)
                {
                    if (item.UserId == manager.Boss)
                    {
                        boss += "<option selected=\"selected\" value=\"" + manager.Boss + "\">" + manager.Boss + "</option>";  //如果有预先选择草稿箱
                    }
                    else
                    {
                        boss += "<option  value=\"" + item.UserId + "\">" + item.UserId + "</option>";
                    }
                }
                var data = new
                {
                    userid = manager.UserId,
                    password = manager.Password,
                    officeValue = office,
                    roleValue = role,
                    cityValue = city,
                    name = manager.Name,
                    sex = manager.Sex,
                    telephone = manager.Telephone,
                    IDcard = manager.IDcard,
                    boss = boss,
                    email = manager.Email,
                    bankaccount = manager.BankCard,
                    bank = manager.Bank,
                    school = manager.School,
                    major = manager.Major,
                    graduate = date,
                    academic = manager.Academic,
                    height = manager.height,
                    weight = manager.weight,
                    BWH = manager.BWH,
                    speciality = manager.Speciality,
                    personal = manager.Photo,
                    student = manager.StudentCardPhoto
                };
                return Json(data);
            }
            else
            {
                var data = new
                {
                    Value = false
                };
                return Json(data);
            }

        }
        #endregion

        #region 删除功能 逻辑删除，删除后无法登陆，数据库可恢复。
        
        //此处为逻辑删除
        public ActionResult deleteMan()
        {
            string id_string = Request["captainInfo.Id"].ToString();
            Guid id = new Guid(id_string);
            Managers man = db.Managers.Find(id);
            try
            {
                man.IsDelete = true;
               db.Entry(man).State = EntityState.Modified;
               db.SaveChanges();
            }
            catch
            {
                Response.Write("<script>alert(\'删除失败\')</script>");
                return View("Index");
            }
            return Json(true);
        }

        //public ActionResult deleteMan()
        //{
        //    string id_string = Request["captainInfo.Id"].ToString();
        //    Guid id = new Guid(id_string);
        //    Managers man = db.Managers.Find(id);
        //    try
        //    {
        //        //要删除好多东西

        //        //删除反馈表中该人员填写的反馈
        //        var fbList = (from fb in db.Feedback.ToList() where fb.CreatedManagerId == man.Id select fb.Id).ToList();
        //        foreach (var fb_id in fbList)
        //        {
        //            Feedback fb=db.Feedback.Find(fb_id);
        //            db.Feedback.Remove(fb);
        //            db.SaveChanges();
        //        }

        //        //删除登录表中的信息
        //        var loginList = (from lgList in db.LoginLog.ToList() where lgList.ManagerId == man.Id select lgList.Id).ToList();
        //        foreach (var login_id in loginList)
        //        {
        //            LoginLog ll = db.LoginLog.Find(login_id);
        //            db.LoginLog.Remove(ll);
        //            db.SaveChanges();
        //        }

        //        //删除公告
        //        var noticeList = (from nList in db.Notice.ToList() where nList.ManagerId == man.Id select nList.Id).ToList();
        //        foreach (var notice_id in noticeList)
        //        {
        //            Notice notice = db.Notice.Find(notice_id);
        //            db.Notice.Remove(notice);
        //            db.SaveChanges();
        //        }

        //        //督导今日工作内容

        //        db.Managers.Remove(man);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    catch
        //    {
        //        Response.Write("<script>alert(\'该管理人员还有很多东西没有删除\')</script>");
        //        return View("Index");
        //    }
        //}
        #endregion

        #region 查看人员信息
        [Management.filter.LoginFilter]
        public ActionResult ShowManInfo(Guid id)
        {
            if (Session["authority"].ToString() == "管理员" || Session["authority"].ToString() == "督导")
            {
                Managers man = db.Managers.Find(id);
                return View(man);
            }
            else
            {
                return View("~/Views/Shared/AuthorityError.cshtml");
            }

        }
        #endregion

        #region 编辑人员
        //编辑管理人员信息
        [Management.filter.LoginFilter]
        public ActionResult EditManInfo(Guid id)
        {
            string user = Session["admin"].ToString();
            Managers man = db.Managers.Find(id);
            if (Session["authority"].ToString() == "管理员")
            {
                string admin = Session["admin"].ToString();
                List<Bank> bankList = db.Bank.ToList();
                ViewBag.Bank = bankList;
                ViewBag.flag = 0;
                if (man.Sex == true)
                {
                    ViewBag.sex = "男";
                }
                else
                {
                    ViewBag.sex = "女";
                }
                return View(man);
            }
            else if (user == man.UserId)
            {
                string admin = Session["admin"].ToString();
                List<Bank> bankList = db.Bank.ToList();
                ViewBag.Bank = bankList;
                ViewBag.flag = 0;
                if (man.Sex == true)
                {
                    ViewBag.sex = "男";
                }
                else
                {
                    ViewBag.sex = "女";
                }
                return View(man);
            }

            else { return View("~/Views/Shared/AuthorityError.cshtml"); }
        }

        [HttpPost]
        public ActionResult EditManInfo()
        {
            Regex r = new Regex("[0-9]");
            Regex mail = new Regex("[1-9a-zA-Z]{1,}[@][1-9a-zA-Z]{1,}[.][a-zA-Z]");           //邮箱格式
            string admin_string = Session["admin"].ToString();                //获取登录人的用户名，作为编辑人员
            string id_string = Request["id"].ToString();
            Guid id = new Guid(id_string);
            Managers mans = db.Managers.Find(id);

            #region 从form中获取数据

            string username = Request.Form.Get("captainInfo.username");                      //用户名
            if (string.IsNullOrEmpty(username))                                              //用户名为空的验证
            {
                ModelState.AddModelError("captainInfo.username", "请输入用户名");
            }
            if (!validateUsernameBack(username))                                             //用户名重名验证
            {
                ModelState.AddModelError("captainInfo.username", "用户名已存在");
            }
            string password = Request.Form.Get("captainInfo.password");                      //密码
            if (string.IsNullOrEmpty(password))                                              //密码不能为空
            {
                ModelState.AddModelError("captainInfo.password", "请输入密码");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("captainInfo.password", "密码不能少于6位");         //密码不能少于6位
            }
            string office = Request.Form.Get("captainInfo.office");                          //办事处
            if (string.IsNullOrEmpty(office))                                                //办事处不能为空
            {
                ModelState.AddModelError("captainInfo.office", "请选择办事处");
            }
            string city = Request.Form.Get("captainInfo.city");                              //城市
            if (string.IsNullOrEmpty(city))                                                  //城市不能为空
            {
                ModelState.AddModelError("captainInfo.city", "请选择城市");
            }
            string role = Request.Form.Get("captainInfo.role");                              //类型
            if (string.IsNullOrEmpty(role))                                                  //类型不能为空
            {
                ModelState.AddModelError("captainInfo.role", "请选择类型");
            }
            string name = Request.Form.Get("captainInfo.name");                              //真实姓名
            if (string.IsNullOrEmpty(name))                                                  //真实姓名不能为空
            {
                ModelState.AddModelError("captainInfo.name", "请输入真实姓名");
            }
            string gender = Request.Form.Get("captainInfo.sex");                             //性别
            if (string.IsNullOrEmpty(gender))                                                //性别不能为空
            {
                ModelState.AddModelError("captainInfo.sex", "请选择性别");
            }
            string Mobile = Request.Form.Get("captainInfo.mobile");                          //电话
            if (string.IsNullOrEmpty(Mobile))                                                //电话不能为空
            {
                ModelState.AddModelError("captainInfo.mobile", "请输入电话");
            }
            else if (Mobile.Length != 11)
            {
                ModelState.AddModelError("captainInfo.mobile", "电话必须是11位");
            }
            string boss = Request.Form.Get("captainInfo.supervisor");                        //上级
            string email = Request.Form.Get("captainInfo.mail");                             //邮箱
            ViewBag.email = email;
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("captainInfo.mail", "请输入邮箱地址");             //邮箱不能为空
            }
            else if (!mail.IsMatch(email))
            {
                ModelState.AddModelError("captainInfo.mail", "请输入正确的邮箱地址");       //邮箱规范要正确
            }
            string idCard = Request.Form.Get("captainInfo.identitys");                        //身份证号
            if (string.IsNullOrEmpty(idCard))                                                //身份证号不能为空
            {
                ModelState.AddModelError("captainInfo.identitys", "请输入身份证号");
            }
            else if (idCard.Length != 18)
            {
                ModelState.AddModelError("captainInfo.identitys", "身份证号必须是18位");      //身份证号必须是18位
            }
            else if (!validateIdentityBack(idCard))
            {
                ModelState.AddModelError("captainInfo.identitys", "身份证号已存在");          //身份证已存在
            }
            string bankaccount = Request.Form.Get("captainInfo.bank");                       //银行卡号
            if (string.IsNullOrEmpty(bankaccount))                                           //银行卡号不能为空
            {
                ModelState.AddModelError("captainInfo.bank", "请输入银行卡号");
            }
            string bank = Request.Form.Get("captainInfo.account");                           //银行名称(开户行)
            if (string.IsNullOrEmpty(bank))                                                  //银行名称不能为空
            {
                ModelState.AddModelError("captainInfo.account", "请选择银行");
            }
            string school = Request.Form.Get("captainInfo.school");                          //学校
            if (string.IsNullOrEmpty(school))                                                //学校不能为空
            {
                ModelState.AddModelError("captainInfo.account", "请输入学校");
            }
            string major = Request.Form.Get("captainInfo.major");                            //专业
            if (string.IsNullOrEmpty(major))                                                 //专业不能为空
            {
                ModelState.AddModelError("captainInfo.major", "请输入专业");
            }
            string graduatedate = Request.Form.Get("captainInfo.graduationdate");            //毕业时间
            if (string.IsNullOrEmpty(graduatedate))                                                 //毕业时间不能为空
            {
                ModelState.AddModelError("captainInfo.graduationdate", "请输入毕业时间");
            }
            string academic = Request.Form.Get("captainInfo.education");                     //学历
            if (string.IsNullOrEmpty(academic))                                              //学历不能为空
            {
                ModelState.AddModelError("captainInfo.education", "请输入专业");
            }
            string height = Request.Form.Get("captainInfo.height");
            if (!string.IsNullOrEmpty(height) && !r.IsMatch(height))
            {
                ModelState.AddModelError("captainInfo.height", "身高请输入整数");
            }
            string weight = Request.Form.Get("captainInfo.weight");                          //体重
            if (!string.IsNullOrEmpty(weight) && !r.IsMatch(weight))
            {
                ModelState.AddModelError("captainInfo.weight", "体重请输入整数");
            }
            Regex x = new Regex("^[1-9][0-9]{1,2}[,][1-9][0-9]{0,2}[,][1-9][0-9]{0,2}$");
            string BWH = Request.Form.Get("captainInfo.meas");                               //三围
            if (!string.IsNullOrEmpty(BWH) && !x.IsMatch(BWH))
            {
                ModelState.AddModelError("captainInfo.meas", "请规范输入三围，格式:66,44,66");
            }
            string speciality = Request.Form.Get("captainInfo.skill");                       //特长
            HttpPostedFileBase personalPhoto = Request.Files["personalimage"];               //个人照
            if (personalPhoto != null)
            {
                if (!string.IsNullOrEmpty(personalPhoto.FileName))
                {
                    if (!PicExtend(personalPhoto.FileName))
                    {
                        ModelState.AddModelError("personalImage", "图片格式只能是jpeg、jpg、png、bmp、gif");
                    }
                }
            }
            HttpPostedFileBase studentPhoto = Request.Files["studentImage"];                 //学生证照片
            if (studentPhoto != null)
            {
                if (!string.IsNullOrEmpty(studentPhoto.FileName))
                {
                    if (!PicExtend(studentPhoto.FileName))
                    {
                        ModelState.AddModelError("studentImage", "图片格式只能是jpeg、jpg、png、bmp、gif");
                    }
                }
            }
            #endregion
            #region 提交
            if (ModelState.IsValid)
            {

                mans.UserId = username;
                mans.Password = password;
                mans.City = Convert.ToInt32(city);
                mans.Authority = Convert.ToInt32(role);
                mans.Name = name;
                mans.Sex = Convert.ToBoolean(gender);
                mans.Telephone = Mobile;
                mans.Boss = boss;
                mans.Email = email;
                mans.IDcard = idCard;
                mans.BankCard = bankaccount;
                mans.Bank = Convert.ToInt32(bank);
                mans.School = school;
                mans.Major = major;
                mans.Graduate = DateTime.Now;
                mans.Academic = academic;
                mans.EditManId = admin_string;
                mans.CreateTime = mans.CreateTime;
                if (!string.IsNullOrEmpty(height))
                {
                    mans.height = Convert.ToInt32(height);
                }
                else { mans.height = null; }
                if (!string.IsNullOrEmpty(weight))
                {
                    mans.weight = Convert.ToInt32(weight);
                }
                else { mans.weight = null; }

                if (!string.IsNullOrEmpty(BWH))
                {
                    mans.BWH = BWH;
                }
                else { mans.BWH = null; }

                if (!string.IsNullOrEmpty(speciality))
                {
                    mans.Speciality = speciality;
                }
                if (personalPhoto != null)
                {
                    if (!string.IsNullOrEmpty(personalPhoto.FileName))
                    {
                        if (string.IsNullOrEmpty(mans.Photo))
                        {
                            string file = PicName(personalPhoto.FileName);                          //检查格式防止重名
                            personalPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/personalImg/yuan/") + file);
                            file = TouXiangSuoFang(personalPhoto, file, Server.MapPath("~/uploadImg/managerImg/personalImg/suo/"), 114, 125);
                            mans.Photo = file;
                        }
                        else
                        {
                            string file = mans.Photo;
                            personalPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/personalImg/yuan/") + mans.Photo);
                            file = TouXiangSuoFang(personalPhoto, file, Server.MapPath("~/uploadImg/managerImg/personalImg/suo/"), 114, 125);
                        }
                    }
                }
                if (studentPhoto != null)
                {
                    if (!string.IsNullOrEmpty(studentPhoto.FileName))
                    {
                        if (string.IsNullOrEmpty(mans.StudentCardPhoto))
                        {
                            string file = studentPicName(studentPhoto.FileName);                          //检查格式防止重名
                            studentPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/studentImg/yuan/") + file);
                            file = TouXiangSuoFang(studentPhoto, file, Server.MapPath("~/uploadImg/managerImg/studentImg/suo/"), 114, 125);
                            mans.StudentCardPhoto = file;
                        }
                        else
                        {
                            string file = mans.StudentCardPhoto;
                            studentPhoto.SaveAs(Server.MapPath("~/uploadImg/managerImg/studentImg/") + mans.StudentCardPhoto);
                            file = TouXiangSuoFang(studentPhoto, file, Server.MapPath("~/uploadImg/managerImg/studentImg/suo/"), 114, 125);
                        }
                    }
                }
                if (mans.Sex == true)
                {
                    ViewBag.sex = "男";
                }
                else
                {
                    ViewBag.sex = "女";
                }
                db.Entry(mans).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.flag = 1;
                List<Bank> bankList = db.Bank.ToList();
                ViewBag.Bank = bankList;
                return View(mans);
            }
            #endregion
            List<Bank> a = db.Bank.ToList();
            ViewBag.Bank = a;
            if (mans.Sex == true)
            {
                ViewBag.sex = "男";
            }
            else
            {
                ViewBag.sex = "女";
            }
            return View(mans);
        }
        #endregion

        #region 用户名验证
        //前段用户名验证
        public JsonResult validateCaptain()
        {
            bool success = true;
            var usernameList = (from UserName in db.Managers.ToList() where UserName.IsDraft == false select UserName.UserId).ToList();
            string username = Request["captainInfo.username"].ToString();
            foreach (var name in usernameList)
            {
                if (name.ToLower() == username.ToLower())
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //编辑页面用户名验证
        public JsonResult EditValidateCaptain()
        {
            bool success = true;
            string username = Request["captainInfo.username"].ToString();
            string Id_string = Request["id"].ToString();
            Guid Id = new Guid(Id_string);
            var usernameList = (from UserName in db.Managers.ToList() where UserName.IsDraft == false && UserName.Id != Id select UserName.UserId).ToList();
            foreach (var name in usernameList)
            {
                if (name.ToLower() == username.ToLower())
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //后台用户名验证
        public bool validateUsernameBack(string username)
        {
            bool success = true;
            Guid Id = (from id in db.Managers where id.UserId == username select id.Id).FirstOrDefault();
            var usernameList = (from UserName in db.Managers.ToList() where UserName.IsDraft == false && UserName.Id != Id select UserName.UserId).ToList();
            foreach (var name in usernameList)
            {
                if (name.ToLower() == username.ToLower())
                {
                    success = false;
                    break;
                }
            }
            return success;
        }
        #endregion

        #region 身份证号验证
        //前端身份证号验证
        public JsonResult validateIdentity()
        {
            bool success = true;
            var IdList = (from id in db.Managers.ToList() where id.IsDraft == false select id.IDcard).ToList();
            string Identity = Request["captainInfo.identity"].ToString();
            foreach (var id in IdList)
            {
                if (id == Identity)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //编辑页面身份证号验证
        public JsonResult EditValidateIdentity()
        {
            bool success = true;
            string Identity = Request["identitys"].ToString();
            string Id_string = Request["id"].ToString();
            Guid Id = new Guid(Id_string);
            var IdList = (from id in db.Managers.ToList() where id.IsDraft == false && id.Id != Id select id.IDcard).ToList();

            foreach (var id in IdList)
            {
                if (id == Identity)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //后台身份证号验证.
        public bool validateIdentityBack(string IdCard)
        {
            bool success = true;
            Guid id_new = (from idcards in db.Managers where idcards.IDcard == IdCard select idcards.Id).FirstOrDefault();
            var IdList = (from id in db.Managers.ToList() where id.IsDraft == false && id.Id != id_new select id.IDcard).ToList();
            foreach (var id in IdList)
            {
                if (id == IdCard)
                {
                    success = false;
                    break;
                }
            }
            return success;

        }
        #endregion

        #region 电话重复验证
        //电话重复验证
        public JsonResult validateMobile()
        {
            bool success = true;
            var mobileList = (from id in db.Managers.ToList() where id.IsDraft == false select id.Telephone).ToList();
            string Mobile = Request["captainInfo.mobile"].ToString();
            foreach (var num in mobileList)
            {
                if (num == Mobile)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //编辑页面电话重复验证
        public JsonResult EditValidateMobile()
        {
            bool success = true;
            string Mobile = Request["mobiles"].ToString();
            string Id_string = Request["id"].ToString();
            Guid Id = new Guid(Id_string);
            var mobileList = (from id in db.Managers.ToList() where id.Id != Id && id.IsDraft == false select id.Telephone).ToList();
            foreach (var num in mobileList)
            {
                if (num == Mobile)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        //后台电话重复验证
        public bool validateMobileBack(string mobiles)
        {
            bool success = true;
            Guid id_new = (from idcards in db.Managers where idcards.Telephone == mobiles select idcards.Id).FirstOrDefault();
            var moblieList = (from mobile in db.Managers.ToList() where mobile.IsDraft == false && mobile.Id != id_new select mobile.IDcard).ToList();
            foreach (var mobile in moblieList)
            {
                if (mobile == mobiles)
                {
                    success = false;
                    break;
                }
            }
            return success;

        }
        #endregion

        #region 银行卡号验证
        //银行卡号验证
        public JsonResult validateBank()
        {
            bool success = true;
            string Bank = Request["captainInfo.bank"].ToString();
            var bankList = (from id in db.Managers.ToList() where id.IsDraft == false select id.BankCard).ToList();
            foreach (var num in bankList)
            {
                if (num == Bank)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }

        //编辑页面银行卡号验证
        public JsonResult EditValidateBank()
        {
            bool success = true;
            string Bank = Request["banks"].ToString();
            string Id_string = Request["id"].ToString();
            Guid Id = new Guid(Id_string);
            var bankList = (from id in db.Managers.ToList() where id.IsDraft == false && id.Id != Id select id.BankCard).ToList();
            foreach (var num in bankList)
            {
                if (num == Bank)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);

        }
        //后台银行卡号重复验证
        public bool validateBankBack(string cardid)
        {
            bool success = true;
            Guid id_new = (from idcards in db.Managers where idcards.BankCard == cardid select idcards.Id).FirstOrDefault();
            var bankList = (from id in db.Managers.ToList() where id.IsDraft == false && id.Id != id_new select id.BankCard).ToList();
            foreach (var num in bankList)
            {
                if (num == cardid)
                {
                    success = false;
                    break;
                }
            }
            return success;
        }
        #endregion

        #region 图片验证
        //检验图片格式
        public bool PicExtend(string picName)
        {
            bool success = false;
            if (picName.LastIndexOf(".") <= 0)
            {
                return success;
            }
            string extend = picName.Remove(0, picName.IndexOf(".") + 1);
            string[] Extendlock = { "jpeg", "jpg", "png", "bmp", "gif" };
            foreach (string item in Extendlock)
            {
                if (extend.ToLower() == item)
                {
                    success = true;
                    break;
                }
            }
            return success;
        }

        //检验个人图片名称
        public string PicName(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from photo in db.Managers.ToList() select photo.Photo).ToList();
            string newname = picName;
            foreach (var item in manPhoto)
            {
                if (picName == item)
                {
                    newname = newName(fileName, extend);
                    break;
                }
            }
            return newname;
        }

        //检验学生证图片名称
        public string studentPicName(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from photo in db.Managers.ToList() select photo.StudentCardPhoto).ToList();
            string newname = picName;
            foreach (var item in manPhoto)
            {
                if (picName == item)
                {
                    newname = newName(fileName, extend);
                    break;
                }
            }
            return newname;

        }

        public static String newName(String name, String type)
        {
            int begin = name.LastIndexOf("(");  //获得name中最后一个"("字符串的位置
            int end = name.LastIndexOf(")");  //查看name中最后一个")"字符串的位置

            Regex r = new Regex("[a-zA-Z\u4E00-\u9FA5]");
            if (name.LastIndexOf("(") <= 0 || name.LastIndexOf(")") <= 0)
            {
                return name + "(" + 1 + ")" + "." + type;
            }
            string something = name.Substring(begin + 1, end - (begin + 1));
            if (r.IsMatch(something))
            {
                return name + "(" + 1 + ")" + "." + type;
            }

            if ((end - begin) <= 1)
            {
                return name + "(" + 1 + ")" + "." + type;
            }
            String oldNum = name.Substring(begin + 1, end - (begin + 1));
            int newNum = Convert.ToInt32(oldNum) + 1; //将获得的字符串转为整型并且加1
            String nname = name.Substring(0, begin);
            return nname + "(" + newNum + ")" + "." + type;  //递归查询一边并生成新的name字符串
        }
        #endregion

        #region 图片缩略图
        public static string TouXiangSuoFang(HttpPostedFileBase UploadFile, string imageName, string imagemath, int width, int height)
        {
            //生成原图 
            Byte[] oFileByte = new byte[UploadFile.ContentLength];
            System.IO.Stream oStream = UploadFile.InputStream;
            System.Drawing.Image oImage = System.Drawing.Image.FromStream(oStream);

            int oWidth = oImage.Width; //原图宽度 
            int oHeight = oImage.Height; //原图高度 
            int tWidth = width; //设置缩略图初始宽度 
            int tHeight = height; //设置缩略图初始高度 

            //按比例计算出缩略图的宽度和高度 
            if (oWidth >= oHeight)
            {
                tHeight = (int)Math.Floor(Convert.ToDouble(oHeight) * (Convert.ToDouble(tWidth) / Convert.ToDouble(oWidth)));
            }
            else
            {
                tWidth = (int)Math.Floor(Convert.ToDouble(oWidth) * (Convert.ToDouble(tHeight) / Convert.ToDouble(oHeight)));
            }
            //生成缩略原图 
            Bitmap tImage = new Bitmap(tWidth, tHeight);
            Graphics g = Graphics.FromImage(tImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High; //设置高质量插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//设置高质量,低速度呈现平滑程度 
            //g.Clear(Color.Transparent); //清空画布并以透明背景色填充 
            g.DrawImage(oImage, new Rectangle(0, 0, tWidth, tHeight), new Rectangle(0, 0, oWidth, oHeight), GraphicsUnit.Pixel);

            try
            {
                //以JPG格式保存图片 
                tImage.Save(imagemath + imageName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //释放资源 
                oImage.Dispose();
                g.Dispose();
                tImage.Dispose();
            }
            return imageName;
        }
        #endregion

    }
}
