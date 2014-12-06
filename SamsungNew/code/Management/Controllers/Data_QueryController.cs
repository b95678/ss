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
    public class Data_QueryController : Controller
    {
        Regex regexNum = new Regex(@"\d");
        SS_HRM_DBEntities dal = new SS_HRM_DBEntities();
        public ActionResult BasicInfo()//基本信息
        {
            return View();
        }
        /*获取人员表信息*/
        class getMan
        {
            public string uniquenum { get; set; }
            public string name { get; set; }
            public string offices { get; set; }
            public string sex { get; set; }
            public string city { get; set; }
            public string identity { get; set; }
            public string bank { get; set; }
            public string account { get; set; }
            public string mobile { get; set; }
            public string insertuser { get; set; }
            public string updateuser { get; set; }
        }
        //修改人员等级
        class level
        {
            public string levelname { get; set; }
        }
        public ActionResult levelselect()
        {
            string levelValue = "";
            IList<level> levelRank = new List<level>();
            string humid_string = Request.Params["humid"];
            Guid humid = new Guid(humid_string);
            HumanBasicFile hum = (from h in dal.HumanBasicFile.ToList() where h.Id == humid select h).FirstOrDefault();

            levelRank.Add(new level()
            {
                levelname = "B"
            });
            levelRank.Add(new level()
            {
                levelname = "S"
            });
            levelRank.Add(new level()
            {
                levelname = "A"
            });

            foreach (var item in levelRank)
            {
                if (item.levelname == hum.HumanLevel)
                {
                    levelValue += "<option selected=\"selected\" value=\"" + item.levelname + "\">" + item.levelname + "</option>";
                }
                else
                {
                    levelValue += "<option value=\"" + item.levelname + "\">" + item.levelname + "</option>";
                }
            }
            var griddata = new
            {
                levelValue = levelValue
            };
            return Json(griddata);
        }
        public ActionResult checkBaseInfo()//显示人员信息
        {
            List<getMan> getm = new List<getMan>();
            List<HumanBasicFile> da = dal.HumanBasicFile.ToList();
            string authority = Session["authority"].ToString();
            string admin = Session["admin"].ToString();
            //获取非草稿箱的数据
            da = (from dataFilter in da where dataFilter.IsDraft == false&&(dataFilter.IsDelete==null||dataFilter.IsDelete==false) select dataFilter).OrderBy(a => a.uniNum).ToList();
            if (authority == "管理员")
            {
                #region admin页面
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.uniNum.ToUpper().Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "office")//办事处查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.City1.Office.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
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
                        city = item.City1.Name,//城市
                        identity = item.IDcardNo,//身份证号
                        bank = item.BankNum,//银行卡号
                        account = item.Bank.Name,//银行名称
                        mobile = item.Telephone,//电话号码
                        insertuser = item.CreatedManagerID,//录入人员
                        updateuser = item.EditManagerId//编辑人员
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
                #endregion
            }
            else
            {
                if (authority == "小队长")
                {
                    //返回其上级的资料
                    string Boss = (from b in dal.Managers where b.UserId == admin select b.Boss).FirstOrDefault();
                    da = (from s in da where s.Managers.UserId == Boss select s).ToList();
                }
                else if (authority == "督导")
                {
                    da = (from s in da where s.Managers.UserId == admin select s).ToList();
                }
                else
                {
                    //没有权限就返回错误页
                    return View("~/Views/Shared/AuthorityError.cshtml");
                }
                #region admin页面
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
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.uniNum.ToUpper().Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "office")//办事处查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.City1.Office.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                foreach (var item in da)
                {
                    string gender = null;
                    if (item.Sex == true)
                    { gender = "男"; }
                    else if (item.Sex == false)
                    { gender = "女"; }
                    if (getm != null)
                    {
                        getm.Add(new getMan()
                        {
                            uniquenum = item.uniNum,//唯一号
                            name = item.Name,//姓名
                            offices = item.City1.Office.Name,//办事处
                            sex = gender,//性别
                            city = item.City1.Name,//城市
                            identity = item.IDcardNo,//身份证号
                            bank = item.BankNum,//银行卡号
                            account = item.Bank.Name,//银行名称
                            mobile = item.Telephone,//电话号码
                            insertuser = item.CreatedManagerID,//录入人员
                            updateuser = item.EditManagerId//编辑人员
                        }
                        );
                    }
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
                #endregion
            }
        }
        public ActionResult editBaseInfo(string uniquenum)//编辑基本信息
        {
            var hum1 = (from s in dal.HumanBasicFile where s.uniNum == uniquenum select s).ToList();
            Guid hu;
            ViewBag.cof = 0;
            ViewBag.bank = dal.Bank.ToList();
            foreach (var item in hum1)
            {
                hu = item.Id;
                HumanBasicFile hum = dal.HumanBasicFile.Find(hu);
                return View(hum);
            }
            return View();
        }
        [HttpPost]
        public ActionResult editBaseInfo()//修改保存人员信息
        {
            ViewBag.cof = 0;
            ViewBag.bank = dal.Bank.ToList();
            var captain = Request.Form.Get("staffInfo.office");//办事处
            var city = Request.Form.Get("staffInfo.city");//城市
            var name = Request.Form.Get("staffInfo.name");//姓名
            var unique = Request.Form.Get("staffInfo.uniquenum");//唯一号
            string level = Request.Form.Get("staffInfo.level");//等级
            string jiguan = Request.Form.Get("staffInfo.birth");//籍贯
            var sex = Request.Form.Get("staffInfo.sex");//性别
            var iden = Request.Form.Get("staffInfo.identitys");//身份
            string idCardNo = Request.Form.Get("staffInfo.birth");
            var mob = Request.Form.Get("staffInfo.mobile");//号码
            var bank = Request.Form.Get("staffInfo.bank");//银行卡号
            var acc = Request.Form.Get("staffInfo.account");//银行名称
            var sch = Request.Form.Get("staffInfo.school");//学校
            var major = Request.Form.Get("staffInfo.major");//专业
            var gra = Request.Form.Get("staffInfo.graduationdate");//毕业时间
            var edu = Request.Form.Get("staffInfo.education");//学历
            var hei = Request.Form.Get("staffInfo.height");//身高
            var wei = Request.Form.Get("staffInfo.weight");//体重
            var mea = Request.Form.Get("staffInfo.meas");//三围
            Regex x = new Regex("^[1-9][0-9]{1,2}[,][1-9][0-9]{0,2}[,][1-9][0-9]{0,2}$");
            if (!string.IsNullOrEmpty(mea) && !x.IsMatch(mea))
            {
                ModelState.AddModelError("staffInfo.meas", "请规范输入三围，格式:66,44,66");
            }
            var ski = Request.Form.Get("staffInfo.skill");//技能
            #region 图片验证
            HttpPostedFileBase identityimage = Request.Files["identityimage"];//身份证图片
            HttpPostedFileBase bankimage = Request.Files["bankimage"];//银行卡图片
            if (!string.IsNullOrEmpty(bankimage.FileName))
            {
                if (!PictureValiate.PicExtend(bankimage.FileName))
                {
                    ModelState.AddModelError("bankimage", "银行卡图片有错");
                }
            }
            if (!string.IsNullOrEmpty(identityimage.FileName))
            {
                if (!PictureValiate.PicExtend(identityimage.FileName))
                {
                    ModelState.AddModelError("identityimage", "身份证图片错误");
                }
            }
            HttpPostedFileBase isPersonal = Request.Files["interviewimage"];//面试照片
            if (!string.IsNullOrEmpty(isPersonal.FileName))
            {
                if (!PictureValiate.PicExtend(isPersonal.FileName))
                {
                    ModelState.AddModelError("isPersonal", "面试照片有错");
                }
            }
            HttpPostedFileBase isStudent = Request.Files["declareimage"];//个人声明照片
            if (!string.IsNullOrEmpty(isStudent.FileName))
            {
                if (!PictureValiate.PicExtend(isStudent.FileName))
                {
                    ModelState.AddModelError("isStudent", "个人声明有错");
                }
            }
            #endregion
            #region 验证通过
            if (ModelState.IsValid)
            {
                var hum = (from s in dal.HumanBasicFile
                           where s.uniNum == unique
                           select s.Id).ToList();
                foreach (var item in hum)
                {
                    HumanBasicFile hu = dal.HumanBasicFile.Find(item);
                    hu.City1.Office.Name = captain;
                    hu.City1.Name = city;
                    hu.Name = name;
                    if (sex == "男")
                    {
                        hu.Sex = true;
                    }
                    else { hu.Sex = false; }
                    hu.IDcardNo = iden;
                    if (level != hu.HumanLevel)
                    {
                        hu.LevelEditTimes += 1;
                    }
                    hu.HumanLevel = level;
                    hu.NativePlace = jiguan;
                    hu.Telephone = mob;
                    hu.BankNum = bank;
                    hu.BankId = Convert.ToInt32(acc);
                    hu.School = sch;
                    hu.Major = major;
                    hu.GraduateTime = gra;
                    hu.Academic = edu;
                    hu.Remark = "";
                    if (hei.Length != 0)
                    {
                        hu.Height = Convert.ToInt32(hei);
                    }
                    if (wei.Length != 0)
                    {
                        hu.Weight = Convert.ToInt32(wei);
                    }
                    hu.BWH = mea;
                    hu.speciality = ski;
                    if (hu != null)
                    {
                        ViewBag.cof = 1;
                    }
                    # region //上传身份证图片
                    if (!string.IsNullOrEmpty(identityimage.FileName))//身份证图片
                    {
                        string file = PicName(identityimage.FileName);
                        var ptho = unique + "-" + name + "(身份证)";
                        string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                        ptho = PictureValiate.newName(ptho, extend);
                        identityimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/identityImg/") + ptho);
                        file = Suo.TouXiangSuoFang(identityimage, ptho, Server.MapPath("~/uploadImg/managerImg/identity/"), 114, 125);
                        hu.IDcardPhoto = file;
                    }
                    #endregion
                    # region //上传银行卡图片
                    if (!string.IsNullOrEmpty(bankimage.FileName))//银行卡图片
                    {
                        string file = PicName1(bankimage.FileName);
                        var ptho = unique + "-" + name + "(银行卡)";
                        string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                        ptho = PictureValiate.newName(ptho, extend);
                        bankimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/bankImg/") + ptho);
                        file = Suo.TouXiangSuoFang(bankimage, ptho, Server.MapPath("~/uploadImg/managerImg/bankimage/"), 114, 125);
                        hu.BankCardPhoto = file;
                    }
                    #endregion
                    # region //上传面试照片
                    if (!string.IsNullOrEmpty(isPersonal.FileName))//上传面试照片
                    {
                        string file = PicName2(isPersonal.FileName);
                        var ptho = unique + "-" + name + "面试";
                        string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                        ptho = PictureValiate.newName(ptho, extend);
                        isPersonal.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InterviewImg/") + ptho);
                        file = Suo.TouXiangSuoFang(isPersonal, ptho, Server.MapPath("~/uploadImg/managerImg/interviewimg/"), 114, 125);
                        hu.InterfacePhoto = file;
                    }
                    #endregion
                    # region //上传个人声明照片
                    if (!string.IsNullOrEmpty(isStudent.FileName))//上传个人声明照片
                    {
                        string file = PicName3(isStudent.FileName);
                        var ptho = unique + "-" + name + "个人声明";
                        string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                        ptho = PictureValiate.newName(ptho, extend);
                        isStudent.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InfoImg/") + ptho);
                        file = Suo.TouXiangSuoFang(isStudent, ptho, Server.MapPath("~/uploadImg/managerImg/personal/"), 114, 125);
                        hu.Info = file;
                    }
                    #endregion
                    dal.Entry(hu).State = EntityState.Modified;
                    dal.SaveChanges();
                    return View(hu);
                }
            }
            #endregion
            return View();
        }
        #region 删除人员信息
        /*逻辑删除*/
        public JsonResult deleteBaseInfo()
        {
            bool success = false;
            var unique = Request.Form.Get("uniquenum");
            HumanBasicFile hum = (from s in dal.HumanBasicFile where s.uniNum == unique select s).FirstOrDefault();
            if (hum != null)
            {
                hum.IsDelete = true;
                dal.Entry(hum).State = EntityState.Modified;
                dal.SaveChanges();
                success = true;
            }
            return Json(success);
        }


        /*物理删除*/
        //public JsonResult deleteBaseInfo()//删除人员信息
        //{
        //    bool success = true;
        //    // string massage = string.Empty;
        //    var unique = Request.Form.Get("uniquenum");
        //    HumanBasicFile hum = (from s in dal.HumanBasicFile where s.uniNum == unique select s).FirstOrDefault();
        //    dal.HumanBasicFile.Remove(hum);
        //    success = true;
        //    dal.SaveChanges();
        //    return Json(success);
        //}
        #endregion
        public ActionResult showBaseInfo(string uniquenum)
        {
            var hum1 = (from s in dal.HumanBasicFile where s.uniNum == uniquenum select s).ToList();
            var date2 = Request.Params.Get("date2");
            Guid hu;
            foreach (var item in hum1)
            {
                hu = item.Id;
                HumanBasicFile hum = dal.HumanBasicFile.Find(hu);
                return View(hum);
            }
            return View();
        }//选择显示人员界面
        #region 检验图片格式
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
        #endregion
        #region //检验身份证图片名称
        public string PicName(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in dal.HumanBasicFile select d).ToList();
            string newname = picName;
            foreach (var item in manPhoto)
            {
                if (picName == item.IDcardPhoto)
                {
                    newname = PictureValiate.newName(fileName, extend);
                    break;
                }
            }
            return newname;
        }
        #endregion
        #region //检验银行卡图片名称
        public string PicName1(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in dal.HumanBasicFile select d).ToList();
            string newname = picName;
            foreach (var item in manPhoto)
            {
                if (picName == item.BankCardPhoto)
                {
                    newname = PictureValiate.newName(fileName, extend);
                    break;
                }
            }
            return newname;
        }
        #endregion
        #region //检验面试照片名称
        public string PicName2(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in dal.HumanBasicFile select d).ToList();
            string newname = picName;
            //foreach (var item in manPhoto)
            //{
            //    if (picName == item.InterfacePhoto)
            //    {
            //        newname = PictureValiate.newName(fileName, extend);
            //        break;
            //    }
            //}
            return newname;
        }
        #endregion
        #region //检验个人声明照片名称
        public string PicName3(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in dal.HumanBasicFile select d).ToList();
            string newname = picName;
            //foreach (var item in manPhoto)
            //{
            //    if (picName == item.Info)
            //    {
            //        newname = PictureValiate.newName(fileName, extend);
            //        break;
            //    }
            //}
            return newname;
        }
        #endregion
        public ActionResult TrainInfo()
        {
            return View();
        }//培训信息
        public class getinfo
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
        } //获取人员信息
        public ActionResult checkTrainInfo()
        {
            List<getinfo> getm = new List<getinfo>();
            List<Train> da = dal.Train.ToList();
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();//获取权限，若为小队长则取其上级的权限
            //获取非草稿箱的数据
            da = (from dataFilter in da where dataFilter.IsDraft == false  select dataFilter).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
            if (authority == "管理员")
            {
                #region 培训信息
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    //if (content.Length == 0)
                    //{
                    //    Response.Write("<script>alert('请输入内容')</script>");
                    //}
                    //else
                    //{
                    //    
                    content = content.ToUpper();
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.ToUpper().Contains(content)&&dataFilter.HumanBasicFile.IsDelete!=true).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    //}
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content) && dataFilter.HumanBasicFile.IsDelete != true).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
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
                #endregion
            }
            else
            {

                if (authority == "小队长")
                {
                    string Boss = (from b in dal.Managers.ToList() where b.UserId == admin select b.Boss).FirstOrDefault();
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == Boss).ToList();
                }
                else if (authority == "督导")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == admin).ToList();
                }
                else
                {
                    return View("~/Views/Shared/AuthorityError.cshtml");
                }
                #region 培训信息
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    //if (content.Length == 0)
                    //{
                    //    Response.Write("<script>alert('请输入内容')</script>");
                    //}
                    //else
                    //{
                    //    
                    content = content.ToUpper();
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.ToUpper().Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    //}
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }

                foreach (var item in da)
                {
                    if (getm != null)
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
                #endregion
            }
        }//显示培训信息
        public ActionResult editTrainInfo(string uniquenum, string date)
        {
            ViewBag.cof = 0;
            var dat = date;
            Train tt = (from g in dal.Train
                        where g.HumanBasicFile.uniNum == uniquenum && g.TrainStartTime == dat
                        select g).FirstOrDefault();
            ViewBag.tra = tt;
            ViewBag.pro = (from s in dal.product select s).ToList();
            ViewBag.type = (from s in dal.ProductCategory select s).ToList();
            return View(ViewBag.tra);
        }//编辑培训人员信息
        public ActionResult showTrainInfo(string uniquenum, string date)//选择显示培训人员界面
        {
            var dat = date;
            var tt = (from g in dal.Train
                      where g.HumanBasicFile.uniNum == uniquenum && g.TrainStartTime == dat
                      select g).FirstOrDefault();
            ViewBag.tra = tt;
            return View(ViewBag.tra);

        }
        #region 删除培训人员界面
        //public ActionResult deleteTrainInfo(string uniquenum)
        //{
        //    uniquenum = Request.Form.Get("uniquenum");
        //    List<Train> hum = (from s in dal.Train
        //                       from g in dal.HumanBasicFile
        //                       where g.uniNum == uniquenum && s.HumanrId == g.Id
        //                       select s).ToList();
        //    foreach (var item in hum)
        //    {
        //        dal.Train.Remove(item);
        //        dal.SaveChanges();
        //    }
        //    return View();
        //}//删除培训人员界面
        #endregion
        [HttpPost]
        public ActionResult editTrainInfo()//修改培训人员
        {
            ViewBag.cof = 0;
            var unique = Request.Form.Get("trainInfo.uniquenum");
            var pro = Request.Form.Get("trainInfo.product");
            var type = Request.Form.Get("trainInfo.type");
            var date = Request.Form.Get("trainInfo.date");
            var enddate = Request.Form.Get("trainInfo.date1");
            var grade = Request.Form.Get("trainInfo.grade");
            var trainer = Request.Form.Get("trainInfo.trainer");
            Train hum = (from s in dal.HumanBasicFile
                         from g in dal.Train
                         where s.Id == g.HumanrId && s.uniNum == unique
                         select g).FirstOrDefault();
            hum.TrainProduction = type;
            hum.TrainScore = Convert.ToInt32(grade);
            hum.Trainlecturer = trainer;
            hum.Trainpro = pro;
            hum.TrainStartTime = date;
            hum.TrainEndTime = enddate;
            dal.SaveChanges();
            if (hum != null)
            {
                ViewBag.cof = 1;
            }
            ViewBag.tra = hum;
            ViewBag.pro = (from s in dal.product select s).ToList();
            ViewBag.type = (from s in dal.ProductCategory select s).ToList();
            return View(ViewBag.tra);
        }
        #region//验证培训开始时间
        //防止先填写结束时间再填写开始时间
        public ActionResult validateBeginTime()
        {
            bool success = true;
            string BeginTime = Request["trainInfo.date"].ToString();
            string EndTime = Request["endTime"].ToString();
            if (!string.IsNullOrEmpty(EndTime))
            {
                if (DateTime.Parse(EndTime) <= DateTime.Parse(BeginTime))
                {
                    success = false;
                }
            }
            return Json(success);
        }
        //验证培训结束时间
        public ActionResult validateEndTime()
        {
            bool success = true;
            string EndTime = Request["trainInfo.date1"].ToString();
            string BeginTime = Request["beginTime"].ToString();
            if (!string.IsNullOrEmpty(BeginTime))
            {
                if (DateTime.Parse(EndTime) <= DateTime.Parse(BeginTime))
                {
                    success = false;
                }
            }
            return Json(success);
        }
        #endregion
        public ActionResult selectType()//编辑修改产品
        {
            var product1 = Request.Form.Get("trainInfo.type");//找出产品
            List<product> producttype = (from s in dal.ProductCategory
                                         from g in dal.product
                                         where s.Name == product1
                                         && g.ProductCategoryId == s.Id
                                         select g).ToList();
            string type = "";
            foreach (var item in producttype)
            {
                type += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
            }
            var data = new
            {
                typeHtml = type
            };
            return Json(data);
        }
        public ActionResult Onduty_Info()//上班信息
        {
            return View();
        }
        public ActionResult editWorkInfo(string uniquenum, string date)//编辑上班人员信息
        {
            Dictionary<HumanBasicFile, AttendingInfo> da = new Dictionary<HumanBasicFile, AttendingInfo>();
            AttendingInfo att = (from g in dal.AttendingInfo
                                 where g.HumanBasicFile.uniNum == uniquenum && g.Date == date
                                 select g).FirstOrDefault();
            var hum = (from s in dal.HumanBasicFile where s.Id == att.HumanId select s).FirstOrDefault();
            if (hum != null)
            {
                da.Add(hum, att);
                ViewBag.cof = da;
                return View(ViewBag.cof);
            }
            ViewBag.shangban = 1;
            ViewBag.cof1 = 0;
            return View();
        }
        #region 删除上班人员信息
        //public ActionResult deleteWorkInfo()//删除上班人员信息
        //{
        //    var unique = Request.Form.Get("uniquenum");
        //    List<AttendingInfo> hum = (from s in dal.HumanBasicFile
        //                               from g in dal.AttendingInfo
        //                               where s.Id == g.HumanId
        //                               select g).ToList();
        //    foreach (var item in hum)
        //    {
        //        dal.AttendingInfo.Remove(item);
        //        dal.SaveChanges();
        //        return View();
        //    }
        //    return View();
        //}
        #endregion
        public ActionResult showWorkInfo(string uniquenum, string date)//选择显示上班人员
        {
            Dictionary<HumanBasicFile, AttendingInfo> da = new Dictionary<HumanBasicFile, AttendingInfo>();
            //var date = Request.Params.Get("date2");
            var att = (from s in dal.AttendingInfo
                       where s.HumanBasicFile.uniNum == uniquenum && s.Date == date
                       select s).FirstOrDefault();
            //foreach (var item in att)showBaseInfo
            //{
            var hum = (from s in dal.HumanBasicFile where s.Id == att.HumanId  select s).FirstOrDefault();
            //    foreach (var item1 in hum)
            //    {
            if (att != null)
            {
                da.Add(hum, att);
                ViewBag.cof = da;
                return View(ViewBag.cof);
            }
            //}
            //}
            return View();
        }
        [HttpPost]
        public ActionResult editWorkInfo()//保存修改上班人员
        {
            ViewBag.cof1 = 0;
            var unique = Request.Form.Get("workInfo.uniquenum");
            var act = Request.Form.Get("workInfo.activity");
            var wor = Request.Form.Get("workInfo.product");
            var date = Request.Form.Get("workInfo.date");
            var store = Request.Form.Get("workInfo.store");
            var job = Request.Form.Get("workInfo.job");//工作职能
            var vage = Request.Form.Get("workInfo.wage");//日标准
            var sub = Request.Form.Get("workInfo.subsidy");//补助
            var remark = Request.Form.Get("workInfo.remark");
            var dj = (from s in dal.AttendingInfo
                      from g in dal.HumanBasicFile
                      where s.HumanId == g.Id && g.uniNum == unique
                      select s).FirstOrDefault();
            //var tt = (from s in dal.AttendingInfo where s.Date == date && s.HumanBasicFile.uniNum == unique select s).Count();

            //HumanBasicFile hum1=(from s in dal.HumanBasicFile where s.uniNum==unique select s).FirstOrDefault();
            #region 保存图片
            HttpPostedFileBase student = Request.Files["workimage"];//个人声明照片
            if (!string.IsNullOrEmpty(student.FileName))//上传个人声明照片
            {
                string file = PicName4(student.FileName);
                var ptho = unique + "-" + dj.HumanBasicFile.Name + "-上班";
                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                ptho = PictureValiate.newName(ptho, extend);
                student.SaveAs(Server.MapPath("~/uploadImg/workimage/") + ptho);
                file = Suo.TouXiangSuoFang(student, ptho, Server.MapPath("~/uploadImg/workimage/suo/"), 114, 125);
                dj.WorkPhoto = file;
            }
            #endregion
            //foreach (var item in dj)
            //{
            //AttendingInfo att = dal.AttendingInfo.Find(dj.Id);
            int a = 0;
            if (!string.IsNullOrEmpty(sub))
            {
                a = Convert.ToInt32(sub);
            }
            int vage1 = Convert.ToInt32(vage) - (int)dj.StandardSalary + a;
            if (dj.BearFees != null)
            {
                vage1 = vage1 - (int)dj.BearFees;
            }
            if (dj.WorkContentId != null)
            {
                int newId = (int)dj.WorkContentId;
                WorkContent wc = (from d in dal.WorkContent where d.Id == newId select d).FirstOrDefault();
                wc.MoneyCount = wc.MoneyCount + vage1;
            }
            dj.ActionName = act;
            dj.WorkInfo = wor;
            dj.Date = date;

            var dd = (from s in dal.SShop
                      where s.Name == store
                      select s).FirstOrDefault();
            if (dd != null)
            {
                dj.Department = dd.Id;
            }
            dj.Functions = job;
            dj.StandardSalary = Convert.ToInt32(vage);
            if (!string.IsNullOrEmpty(sub))
            {
                dj.BearFees = Convert.ToInt32(sub);
            }
            else { dj.BearFees = 0; }
            dj.Remark = remark;
            dal.Entry(dj).State = EntityState.Modified;
            dal.SaveChanges();
            //}
            Dictionary<HumanBasicFile, AttendingInfo> da = new Dictionary<HumanBasicFile, AttendingInfo>();
            AttendingInfo att1 = (from s in dal.HumanBasicFile
                                  from g in dal.AttendingInfo
                                  where s.uniNum == unique && g.HumanId == s.Id
                                  select g).FirstOrDefault();
            var hum = (from s in dal.HumanBasicFile where s.Id == att1.HumanId select s).ToList();
            //ViewBag.cof1 = 0;

            foreach (var item1 in hum)
            {
                da.Add(item1, att1);
                ViewBag.cof = da;
                //if (tt >= 1)
                //{
                //    ViewBag.shangban = 0;
                //    return View();
                //}
                if (hum != null)
                {
                    ViewBag.cof1 = 1;
                }
                return View(ViewBag.cof);
            }
            return View();
        }
        #region //检验工作照片名称
        public string PicName4(string picName)
        {
            string fileName = picName.Substring(0, picName.LastIndexOf("."));
            string extend = picName.Remove(0, picName.LastIndexOf(".") + 1);
            var manPhoto = (from d in dal.HumanBasicFile select d).ToList();
            string newname = picName;
            //foreach (var item in manPhoto)
            //{
            //    if (picName == item.Info)
            //    {
            //        newname = PictureValiate.newName(fileName, extend);
            //        break;
            //    }
            //}
            return newname;
        }
        #endregion
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
            List<getinfo1> getm = new List<getinfo1>();
            List<AttendingInfo> da = dal.AttendingInfo.ToList();
            string authority = Session["authority"].ToString();
            string admin = Session["admin"].ToString();
            //获取非草稿箱的数据
            da = (from dataFilter in da where dataFilter.IsDraft == false  select dataFilter).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
            if (authority == "管理员")
            {
                #region 上班信息
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.Contains(content)&&(dataFilter.HumanBasicFile.IsDelete==false||dataFilter.HumanBasicFile.IsDelete==null)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content) && (dataFilter.HumanBasicFile.IsDelete == false || dataFilter.HumanBasicFile.IsDelete == null)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
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
                #endregion
            }
            else
            {
                if (authority == "小队长")
                {
                    string Boss = (from b in dal.Managers where b.UserId == admin select b.Boss).FirstOrDefault();
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == Boss).ToList();
                }
                else if (authority == "督导")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == admin ).ToList();
                }
                else
                {
                    return View("~/Views/Shared/AuthorityError.cshtml");
                }
                #region 上班信息
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }

                foreach (var item in da)
                {
                    if (getm != null)
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
                #endregion
            }
        }
        public ActionResult Checked_Self()//自检信息
        {
            return View();
        }
        public class getinfo2
        {
            public string uniquenum;
            public string name;
            public string office;
            public string city;
            public string identity;
            public string level;
            public int appearance;
            public int attitude;
            public int productkonwledge;
            public string date;
            public string insertuser;
            public string updateuser;
        } //获取人员信息
        public ActionResult checkCheckInfo()//显示自检信息
        {
            List<getinfo2> getm = new List<getinfo2>();
            List<DianJian> da = dal.DianJian.ToList();
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();
            //获取非草稿箱的数据
            da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).OrderBy(a => a.HumanBasicFile.uniNum).ToList();
            if (authority == "管理员")
            {
                #region 自检信息
                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.ToUpper().Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content)).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }

                foreach (var item in da)
                {
                    getm.Add(new getinfo2()
                    {
                        uniquenum = item.HumanBasicFile.uniNum,//唯一号
                        name = item.HumanBasicFile.Name,//姓名
                        office = item.HumanBasicFile.City1.Office.Name,//办事处
                        city = item.HumanBasicFile.City1.Name,
                        identity = item.HumanBasicFile.IDcardNo,//身份证号
                        level = item.HumanBasicFile.HumanLevel,//等级
                        appearance = Convert.ToInt32(item.Face),
                        attitude = Convert.ToInt32(item.WorkAttitude),//工作态度
                        productkonwledge = Convert.ToInt32(item.KOP),//产品知识
                        date = item.DJTime,//自检时间
                        insertuser = item.HumanBasicFile.CreatedManagerID,//录入人员
                        updateuser = item.HumanBasicFile.EditManagerId//编辑人员
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
                #endregion
            }
            else
            {
                if (authority == "小队长")
                {
                    string Boss = (from b in dal.Managers.ToList() where b.UserId == admin select b.Boss).FirstOrDefault();
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == Boss).ToList();
                }
                else if (authority == "督导")
                {
                    da = da.Where(dataFilter => dataFilter.HumanBasicFile.Managers.UserId == admin).ToList();
                }
                else
                {
                    return View("~/Views/Shared/AuthorityError.cshtml");
                }
                #region 自检信息

                //当前页
                int page = Convert.ToInt32(Request.Params["page"]);
                if (page <= 0) { page = 1; }//防止小于1页
                //每页显示的记录数
                int pagesize = Convert.ToInt32(Request.Params["pagesize"]);
                //选取类型
                string key = Request.Params["key"];
                //选取内容
                string content = Request.Params["content"];
                //获取非草稿箱的数据
                da = (from dataFilter in da where dataFilter.IsDraft == false select dataFilter).ToList();
                //查看全部
                if (key == "all")
                { da = (from dataFilter in da select dataFilter).OrderBy(a => a.HumanBasicFile.uniNum).ThenBy(a => a.DJTime).Skip((page - 1) * pagesize).Take(pagesize).ToList(); }
                if (key == "uniquenum")  //查看唯一号
                {
                    if (content.Length == 0)
                    {
                        Response.Write("<script>alert('请输入内容')</script>");
                    }
                    else
                    {
                        content = content.ToUpper();
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.uniNum.ToUpper().Contains(content)).OrderBy(a => a.HumanBasicFile.uniNum).ThenBy(a => a.DJTime).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }
                if (key == "name")//姓名查找
                {
                    if (content.Length == 0) { Response.Write("(script>alert('请输入内容')</script>"); }
                    else
                    {
                        da = da.Where(dataFilter => dataFilter.HumanBasicFile.Name.Contains(content)).OrderBy(a => a.HumanBasicFile.uniNum).ThenBy(a => a.DJTime).Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    }
                }

                foreach (var item in da)
                {
                    if (getm != null)
                    {
                        getm.Add(new getinfo2()
                        {
                            uniquenum = item.HumanBasicFile.uniNum,//唯一号
                            name = item.HumanBasicFile.Name,//姓名
                            office = item.HumanBasicFile.City1.Office.Name,//办事处
                            city = item.HumanBasicFile.City1.Name,
                            identity = item.HumanBasicFile.IDcardNo,//身份证号
                            level = item.HumanBasicFile.HumanLevel,//等级
                            appearance = Convert.ToInt32(item.Face),
                            attitude = Convert.ToInt32(item.WorkAttitude),//工作态度
                            productkonwledge = Convert.ToInt32(item.KOP),//产品知识
                            date = item.DJTime,//自检时间
                            insertuser = item.HumanBasicFile.CreatedManagerID,//录入人员
                            updateuser = item.HumanBasicFile.EditManagerId//编辑人员
                        }
                        );
                    }
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
                #endregion
            }
        }
        public ActionResult editCheckInfo(string uniquenum, string date)//编辑自检信息
        {
            var dat = date;
            DianJian dianjian = (from s in dal.DianJian
                                 from g in dal.HumanBasicFile
                                 where s.HumanId == g.Id && g.uniNum == uniquenum && s.DJTime == dat
                                 select s).FirstOrDefault();
            ViewBag.cof1 = 0;
            return View(dianjian);
        }
        [HttpPost]
        public ActionResult editCheckInfo()//保存自检信息
        {
            var unique = Request.Form.Get("checkInfo.uniquenum");//唯一号
            var dat = Request.Form.Get("checkInfo.date");//自检时间
            var app = Request.Form.Get("checkInfo.appearance");//仪表仪容
            var att = Request.Form.Get("checkInfo.attitude");//工作态度
            var rem = Request.Form.Get("checkInfo.remark");//备注
            var kno = Request.Form.Get("checkInfo.productkonwledge");//产品知识
            ViewBag.cof1 = 0;
            DianJian dianjian = (from s in dal.DianJian
                                 from g in dal.HumanBasicFile
                                 where s.HumanId == g.Id && g.uniNum == unique
                                 select s).FirstOrDefault();
            dianjian.DJTime = dat;
            dianjian.Face = Convert.ToInt32(app);
            dianjian.WorkAttitude = Convert.ToInt32(att);
            dianjian.DJContent = rem;
            dianjian.KOP = Convert.ToInt32(kno);
            dianjian.Score = (Convert.ToInt32(app) + Convert.ToInt32(att) + Convert.ToInt32(kno)) / 3;
            if (dianjian != null)
            {
                ViewBag.cof1 = 1;
            }
            dal.SaveChanges();
            return View(dianjian);
        }
        public ActionResult showCheckInfo(string uniquenum, string date)
        {
            var dat = date;
            DianJian dianjian = (from s in dal.DianJian
                                 from g in dal.HumanBasicFile
                                 where s.HumanId == g.Id && g.uniNum == uniquenum && s.DJTime == dat
                                 select s).FirstOrDefault();
            ViewBag.dianjian = dianjian;
            return View(dianjian);
        }//修改自检信息
        #region 删除自检信息
        //public ActionResult deleteCheckInfo(string unique)//删除自检信息
        //{
        //    var uni = (from s in dal.HumanBasicFile
        //               from g in dal.DianJian
        //               where s.Id == g.HumanId && s.uniNum == unique
        //               select g.HumanId).ToList();
        //    foreach (var item in uni)
        //    {
        //        DianJian dj = dal.DianJian.Find(item);
        //        dal.DianJian.Remove(dj);
        //        dal.SaveChanges();
        //    }
        //    return View();
        //}
        #endregion
        #region 身份证验证
        public JsonResult validateIdentity()//身份证验证
        {
            bool success = true;
            var IdList = (from id in dal.HumanBasicFile.ToList() select id.IDcardNo).ToList();
            string Id = Request["staffInfo.identitys"].ToString();
            foreach (var id in IdList)
            {
                if (id.Length != 18)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        #endregion
        #region 电话验证
        public JsonResult validateMobile()//电话验证
        {
            bool success = true;
            string Id = Request["staffInfo.mobile"].ToString();
            if (Id.Length != 11)
            {
                success = false;
            }
            return Json(success);
        }
        #endregion
        #region 银行卡验证
        public JsonResult validateBank()//银行卡验证
        {
            bool success = true;
            string Id = Request["staffInfo.bank"].ToString();
            if (!regexNum.IsMatch(Id))
            {
                success = false;
            }
            return Json(success);
        }
        #endregion


    }
}
