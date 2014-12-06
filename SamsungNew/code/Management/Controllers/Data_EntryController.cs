using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Entity;
using System.Data;
using System.Text.RegularExpressions;
namespace Management.Controllers
{
    public class Data_EntryController : Controller
    {
        Regex regexNum = new Regex(@"\d");
        SS_HRM_DBEntities dal = new SS_HRM_DBEntities();
        #region 新增兼职人员
        [HttpGet]
        public ActionResult Part_time_Worker()//新增兼职人员
        {
            List<Bank> bank = dal.Bank.ToList();
            ViewBag.bank = bank;
            List<Managers> hbf = (from s in dal.Managers select s).ToList();
            return View(hbf);
        }

        //检测有无兼职人员草稿
        public ActionResult Get_HasPartTimeDraft()
        {
            string admin = Session["admin"].ToString();    //保存创建人
            string hasDraft = "0";
            var DraftList = (from s in dal.HumanBasicFile where s.IsDraft == true && s.CreatedManagerID == admin select s).ToList();//只获取自己保存的draft
            if (DraftList.Count() > 0)
            {
                hasDraft = "1";
            }
            var data = new { hasDraft = hasDraft };
            return Json(data);
        }
        #endregion

        #region 返回上级和办事处的option信息
        [HttpPost]
        public ActionResult Part_time_Worker1()
        {
            string captainValue1 = "";
            string officeValue1 = "";
            string cityValue1 = "";
            //从数据库中获取数据表
            List<Managers> manager = dal.Managers.ToList();
            List<City> cities = dal.City.ToList();
            manager = manager.Where(da => da.Authority != 0 && da.IsDraft == false).ToList();
            string authority = Session["authority"].ToString();
            string admin = Session["admin"].ToString();
            if (authority != "管理员")
            {
                Managers man = new Managers();
                if (authority == "小队长")
                {
                    string boss_String = (from d in dal.Managers where d.UserId == admin select d.Boss).FirstOrDefault();
                    man = (from d in dal.Managers where d.UserId == boss_String select d).FirstOrDefault();
                }
                else if (authority == "督导")
                {
                    man = (from s in dal.Managers where s.UserId == admin && s.IsDraft == false select s).FirstOrDefault();
                }
                else
                {
                    return View("~/Views/Shared/AuthorityError.cshtml");
                }
                captainValue1 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>"; 

                officeValue1 += "<option value=\"" + man.City1.Office.Name + "\">" + man.City1.Office.Name + "</option>";
                //var city = (from s in dal.City
                //            from g in dal.Managers
                //            where s.Id == g.City
                //            select s).ToList();
                //foreach (var item in city)
                //{
                cities = cities.Where(a => a.OfficeId == man.City1.OfficeId).ToList();
                foreach (var item in cities)
                {
                    cityValue1 += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
                //}
            }
            else
            {
                foreach (var item in manager)
                {
                    captainValue1 += "<option value=\"" + item.UserId + "\">" + item.UserId + "</option>";
                    officeValue1 += "<option value=\"" + item.City1.Office.Name + "\">" + item.City1.Office.Name + "</option>";
                    var city = (from s in dal.City where s.Id == item.City select s).ToList();
                    foreach (var item1 in city)
                    {
                        cityValue1 += "<option value=\"" + item1.Name + "\">" + item1.Name + "</option>";
                    }
                }
            }
            var data = new
            {
                captainValue = captainValue1,
                officeValue = officeValue1,
                cityValue = cityValue1
            };
            return Json(data);
        }
        #endregion

        #region 添加新增兼职人员
        [HttpPost]
        public ActionResult Part_time_Worker(int i = 0)
        {
            List<Bank> bank1 = dal.Bank.ToList();
            ViewBag.bank = bank1;
            ViewBag.cof = 0;
            var captain = Request.Form.Get("staffInfo.captain");//上级
            var office = Request.Form.Get("staffInfo.office");//办事处
            var city = Request.Form.Get("staffInfo.city");//城市
            var rank = Request.Form.Get("staffInfo.rank");//等级
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();
            #region 验证
            if (string.IsNullOrEmpty(city))
            {
                ModelState.AddModelError("staffInfo.captain", "城市不能为空!");
            }
            var name = Request.Form.Get("staffInfo.name");//姓名
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("staffInfo.name", "请输入呢称!");
            }
            var uniquenum = Request.Form.Get("staffInfo.uniquenum");//唯一号
            if (string.IsNullOrEmpty(uniquenum))
            {
                ModelState.AddModelError("staffInfo.uniquenum", "请生成唯一号");
            }
            var sex = Request.Form.Get("staffInfo.sex");//性别
            var birth = Request.Form.Get("staffInfo.birth");//籍贯
            if (string.IsNullOrEmpty(birth))
            {
                ModelState.AddModelError("staffInfo.birth", "请输入籍贯!");
            }
            var identity = Request.Form.Get("staffInfo.identity");//身份证号码
            if (string.IsNullOrEmpty(identity))
            {
                ModelState.AddModelError("staffInfo.identity", "身份证号不能为空");
            }
            else if (identity.Length != 18)
            {
                ModelState.AddModelError("staffInfo.identity", "请输入18位的身份证号！");
            }
            var mobile = Request.Form.Get("staffInfo.mobile");//电话号码
            HumanBasicFile sg=(from s in dal.HumanBasicFile where s.Telephone==mobile&&s.IsDelete!=true select s).FirstOrDefault();
            if (sg != null)
            {
                ModelState.AddModelError("staffInfo.mobile", "电话号码重复，请输入电话号码!");
            }
            if (string.IsNullOrEmpty(mobile))
            {
                ModelState.AddModelError("staffInfo.mobile", "请输入电话号码!");
            }
            else if (!regexNum.IsMatch(mobile))
            {
                ModelState.AddModelError("staffInfo.mobile", "输入非数字号码!");
            }
            var bank = Request.Form.Get("staffInfo.bank");//银行卡号
            HumanBasicFile sg1 = (from s in dal.HumanBasicFile where s.BankNum== bank&&s.IsDelete!=true select s).FirstOrDefault();
            if (sg1 != null)
            {
                ModelState.AddModelError("staffInfo.bank", "银行号码重复，请输入银行号码!");
            }
            if (string.IsNullOrEmpty(bank))
            {
                ModelState.AddModelError("staffInfo.bank", "银行卡号不能为空!");
            }
            else if (!regexNum.IsMatch(bank))
            {
                ModelState.AddModelError("staffInfo.bank", "输入非数字银行卡号");
            }
            var account = Request.Form.Get("staffInfo.account");//开户号
            if (string.IsNullOrEmpty(account))
            {
                ModelState.AddModelError("staffInfo.account", "开户口不能为空!");
            }
            var school = Request.Form.Get("staffInfo.school");//学校
            if (string.IsNullOrEmpty(school))
            {
                ModelState.AddModelError("staffInfo.school", "学校不能为空!");
            }
            var major = Request.Form.Get("staffInfo.major");//专业
            if (string.IsNullOrEmpty(major))
            {
                ModelState.AddModelError("staffInfo.major", "专业不能为空");
            }
            var graduation = Request.Form.Get("staffInfo.graduationdate");//毕业时间
            if (string.IsNullOrEmpty(graduation))
            {
                ModelState.AddModelError("staffInfo.graduationdate", "毕业时间不能为空");
            }
            var education = Request.Form.Get("staffInfo.education");//学历
            if (string.IsNullOrEmpty(education))
            {
                ModelState.AddModelError("staffInfo.education", "毕业时间不能为空");
            }
            string height = Request.Form.Get("staffInfo.height");//身高
            if (height != null)
            {
                //if (!regexNum.IsMatch("height"))
                //{
                //    ModelState.AddModelError("staffInfo.height", "身高必须为整数。");
                //}
            }
            string weight = Request.Form.Get("staffInfo.weight");//体重
            if (weight.Length != 0)
            {
                //if (!regexNum.IsMatch("weight"))
                //{
                //    ModelState.AddModelError("staffInfo.weight", "体重必须为整数。");
                //}
            }
            string meas = Request.Form.Get("staffInfo.meas");//三维
            Regex x = new Regex("^[1-9][0-9]{1,2}[,][1-9][0-9]{0,2}[,][1-9][0-9]{0,2}$");
            if (!string.IsNullOrEmpty(meas) && !x.IsMatch(meas))
            {
                ModelState.AddModelError("staffInfo.meas", "请规范输入三围，格式:66,44,66");
            }
            var skill = Request.Form.Get("staffInfo.skill");//特长
            HttpPostedFileBase identityimage = Request.Files["identityimage"];//身份证图片
            if (identityimage == null)
            {
                ModelState.AddModelError("identityimage", "身份证照片不能为空！");
            }
            else if (!string.IsNullOrEmpty(identityimage.FileName))
            {
                if (!PictureValiate.PicExtend(identityimage.FileName))
                {
                    ModelState.AddModelError("identityimage", "图片格式错误");
                }
            }
            HttpPostedFileBase bankimage = Request.Files["bankimage"];//银行卡图片
            if (bankimage == null)
            {
                ModelState.AddModelError("bankimage", "银行卡图片不能为空！");
            }
            else if (!string.IsNullOrEmpty(bankimage.FileName))
            {
                if (!PictureValiate.PicExtend(bankimage.FileName))
                {
                    ModelState.AddModelError("bankimage", "银行卡图片有错");
                }
            }
            HttpPostedFileBase personal = Request.Files["personal"];//面试照片
            if (personal == null)
            {
                ModelState.AddModelError("personal", "面试照片不能为空！");
            }
            else if (!string.IsNullOrEmpty(personal.FileName))
            {
                if (!PictureValiate.PicExtend(personal.FileName))
                {
                    ModelState.AddModelError("personal", "面试照片有错");
                }
            }
            HttpPostedFileBase student = Request.Files["student"];//个人声明照片
            if (student == null)
            {
                ModelState.AddModelError("student", "个人声明照片不能为空！");
            }
            else if (!string.IsNullOrEmpty(student.FileName))
            {
                if (!PictureValiate.PicExtend(student.FileName))
                {
                    ModelState.AddModelError("student", "个人声明照片有错");
                }
            }
            #endregion
            #region 验证通过
            if (ModelState.IsValid)
            {
                #region 非管理员录入
                if (authority != "管理员")
                {
                    HumanBasicFile hum = (from s in dal.HumanBasicFile where s.IsDraft == true && s.Managers.UserId == admin select s).Distinct().FirstOrDefault();
                    if (hum == null)
                    {
                        hum = new HumanBasicFile();
                        hum.Id = Guid.NewGuid();
                        if (captain.Length != 0 && office.Length != 0)
                        {
                            var mm = (from g in dal.Managers
                                      where g.UserId == captain
                                      select g).FirstOrDefault();
                            int cityid = Convert.ToInt32(city);
                            var city1 = (from s in dal.City where s.Id == cityid select s).FirstOrDefault();
                            #region 赋值给兼职人员
                            hum.Boss = mm.Id;
                            hum.city = city1.Id;
                            hum.Name = name;
                            hum.HumanLevel = rank;
                            if (rank != "B")
                            {
                                hum.LevelEditTimes = 1;
                            }
                            if (uniquenum == null)
                            {
                                hum.uniNum = "string";
                            }
                            else { hum.uniNum = uniquenum; }
                            if (sex == "男")
                            {
                                hum.Sex = true;
                            }
                            else
                            {
                                hum.Sex = false;
                            }
                            hum.IDcardNo = identity;//身份证号码
                            hum.Telephone = mobile;//电话
                            hum.BankNum = bank;//bank                   
                            hum.BankId = Convert.ToInt32(account);//开户行
                            hum.NativePlace = birth;
                            hum.School = school;
                            hum.Major = major;
                            hum.GraduateTime = graduation;
                            hum.Academic = education;
                            hum.TrainTimes = 0;
                            hum.LevelEditTimes = 0;
                            if (weight.Length != 0)
                            {
                                hum.Weight = Convert.ToInt32(weight);
                            }
                            if (height == "")
                            {
                                hum.Height = null;
                            }
                            else
                            {
                                hum.Height = Convert.ToInt32(height);
                            }
                            hum.BWH = meas;
                            hum.speciality = skill;
                            # region //上传身份证图片
                            if (!string.IsNullOrEmpty(identityimage.FileName))//身份证图片
                            {
                                string file = PicName(identityimage.FileName);
                                var ptho = uniquenum + "-" + name + "-身份证";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                identityimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/identityImg/") + ptho);
                                ptho = Suo.TouXiangSuoFang(identityimage, ptho, Server.MapPath("~/uploadImg/managerImg/identity/"), 114, 125);
                                hum.IDcardPhoto = ptho;
                            }
                            #endregion
                            # region //上传银行卡图片
                            if (!string.IsNullOrEmpty(bankimage.FileName))//银行卡图片
                            {
                                string file = PicName1(bankimage.FileName);
                                var ptho = uniquenum + "-" + name + "-银行卡";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                bankimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/bankImg/") + ptho);
                                file = Suo.TouXiangSuoFang(bankimage, ptho, Server.MapPath("~/uploadImg/managerImg/bankimage/"), 114, 125);
                                hum.BankCardPhoto = file;
                            }
                            #endregion
                            # region //上传面试照片
                            if (!string.IsNullOrEmpty(personal.FileName))//上传面试照片
                            {
                                string file = PicName2(personal.FileName);
                                var ptho = uniquenum + "-" + name + "-面试";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                personal.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InterviewImg/") + ptho);
                                file = Suo.TouXiangSuoFang(personal, ptho, Server.MapPath("~/uploadImg/managerImg/interviewimg/"), 114, 125);
                                hum.InterfacePhoto = file;
                            }
                            #endregion
                            # region //上传个人声明照片
                            if (!string.IsNullOrEmpty(student.FileName))//上传个人声明照片
                            {
                                string file = PicName3(student.FileName);
                                var ptho = uniquenum + "-" + name + "-个人声明";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                student.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InfoImg/") + ptho);
                                file = Suo.TouXiangSuoFang(student, ptho, Server.MapPath("~/uploadImg/managerImg/personal/"), 114, 125);
                                hum.Info = file;
                            }
                            #endregion
                            hum.IsDraft = false;
                            hum.EditManagerId = Session["admin"].ToString();
                            hum.createTime = DateTime.Now;
                            if (mm.Authority != 2)
                            {
                                hum.CreatedManagerID = Session["admin"].ToString();
                            }
                            else
                            {
                                hum.CreatedManagerID = mm.Boss;
                            }
                            hum.IsDelete = false;
                            dal.HumanBasicFile.Add(hum);
                            dal.SaveChanges();
                            #endregion
                        }
                    }
                #endregion

                    #region 非草稿
                    else
                    {
                        if (captain.Length != 0 && office.Length != 0)
                        {
                            var mm = (from g in dal.Managers
                                      where g.UserId == captain
                                      select g).FirstOrDefault();
                            var city1 = (from s in dal.City
                                         where s.Name == city
                                         select s).FirstOrDefault();
                            #region 赋值给兼职人员
                            hum.createTime = DateTime.Now;//创建时间
                            hum.Boss = mm.Id;
                            hum.city = city1.Id;
                            hum.Name = name;
                            hum.HumanLevel = rank;
                            if (rank != "B")
                            {
                                hum.LevelEditTimes = 1;
                            }
                            if (uniquenum == null)
                            {
                                hum.uniNum = "string";
                            }
                            else { hum.uniNum = uniquenum; }
                            if (sex == "男")
                            {
                                hum.Sex = true;
                            }
                            else
                            {
                                hum.Sex = false;
                            }
                            hum.IDcardNo = identity;//身份证号码
                            hum.Telephone = mobile;//电话
                            hum.BankNum = bank;//bank                   
                            hum.BankId = mm.Bank1.Id;//开户行
                            hum.School = school;
                            hum.Major = major;
                            hum.GraduateTime = graduation;
                            hum.NativePlace = birth;
                            hum.TrainTimes = 0;
                            hum.LevelEditTimes = 0;
                            hum.Academic = education;
                            if (weight.Length != 0)
                            {
                                hum.Weight = Convert.ToInt32(weight);
                            }
                            if (height == "")
                            {
                                hum.Height = null;
                            }
                            else
                            {
                                hum.Height = Convert.ToInt32(height);
                            }
                            hum.BWH = meas;
                            hum.speciality = skill;
                            # region //上传身份证图片
                            if (!string.IsNullOrEmpty(identityimage.FileName))//身份证图片
                            {
                                string file = PicName(identityimage.FileName);
                                var ptho = uniquenum + "-" + name + "-身份证";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                identityimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/identityImg/") + ptho);
                                file = Suo.TouXiangSuoFang(identityimage, ptho, Server.MapPath("~/uploadImg/managerImg/identity/"), 114, 125);
                                hum.IDcardPhoto = file;
                            }
                            #endregion
                            # region //上传银行卡图片
                            if (!string.IsNullOrEmpty(bankimage.FileName))//银行卡图片
                            {
                                string file = PicName1(bankimage.FileName);
                                var ptho = uniquenum + "-" + name + "-银行卡";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                bankimage.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/bankImg/") + ptho);
                                file = Suo.TouXiangSuoFang(bankimage, ptho, Server.MapPath("~/uploadImg/managerImg/bankimage/"), 114, 125);
                                hum.BankCardPhoto = file;
                            }
                            #endregion
                            # region //上传面试照片
                            if (!string.IsNullOrEmpty(personal.FileName))//上传面试照片
                            {
                                string file = PicName2(personal.FileName);
                                var ptho = uniquenum + "-" + name + "-面试";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                personal.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InterviewImg/") + ptho);
                                file = Suo.TouXiangSuoFang(personal, ptho, Server.MapPath("~/uploadImg/managerImg/interviewimg/"), 114, 125);
                                hum.InterfacePhoto = file;
                            }
                            #endregion
                            # region //上传个人声明照片
                            if (!string.IsNullOrEmpty(student.FileName))//上传个人声明照片
                            {
                                string file = PicName3(student.FileName);
                                var ptho = uniquenum + "-" + name + "-个人声明";
                                string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                                ptho = PictureValiate.newName(ptho, extend);
                                student.SaveAs(Server.MapPath("~/uploadImg/Humanbasic/InfoImg/") + ptho);
                                file = Suo.TouXiangSuoFang(student, ptho, Server.MapPath("~/uploadImg/managerImg/personal/"), 114, 125);
                                hum.Info = file;
                            }
                            #endregion
                            hum.IsDraft = false;
                            hum.IsDelete = false;
                            hum.EditManagerId = Session["admin"].ToString();
                            hum.CreatedManagerID = Session["admin"].ToString();
                            dal.Entry(hum).State = EntityState.Modified;
                            dal.SaveChanges();
                            #endregion
                        }
                    }
                    #endregion
                    if (hum != null)
                    {
                        ViewBag.cof = 1;
                    }
                }
                List<Managers> list = (from s in dal.Managers select s).ToList();
                return View(list);
            }

            List<Managers> list1 = (from s in dal.Managers select s).ToList();
            return View(list1);
            #endregion
        }




        /*提交兼职人员草稿箱*/
        [HttpPost]
        public ActionResult saveDraft()
        {
            string admin = Session["admin"].ToString();    //保存创建人
            if (admin != "admin")
            {
                string uniquenum = Request.Params.Get("uniquenum");                    //唯一号
                HumanBasicFile hum = (from s in dal.HumanBasicFile where s.Managers.UserId == admin && s.IsDraft == true select s).Distinct().FirstOrDefault();
                bool success = true;
                #region 从网页中获取数据
                string name = Request.Params.Get("name");                              //真实姓名
                string birth = Request.Params.Get("birth");   //贯集
                string Mobile = Request.Params.Get("mobile");                          //电话
                if (!string.IsNullOrEmpty(Mobile) && Mobile.Length != 11)
                {
                    success = false;
                }
                string identity = Request.Params.Get("identity");                        //身份证号
                if (!string.IsNullOrEmpty(identity) && identity.Length != 18)
                {
                    success = false;             //身份证号必须是18位
                }
                string bank = Request.Params.Get("bank");                       //银行卡号
                string school = Request.Params.Get("school");                          //学校
                string major = Request.Params.Get("major");                            //专业
                string graduatedate = Request.Params.Get("graduationdate");            //毕业时间
                string academic = Request.Params.Get("academic");//学历
                string height = Request.Params.Get("height");                          //身高
                string weight = Request.Params.Get("weight");                          //体重
                string BWH = Request.Params.Get("BWH");                               //三围
                string speciality = Request.Params.Get("speciality");                       //特长
                #endregion
                #region 草稿已经存在
                if (hum != null)
                {
                    if (success == true)
                    {
                        hum.CreatedManagerID = admin;
                        hum.EditManagerId = admin;
                        hum.createTime = DateTime.Now;
                        hum.uniNum = uniquenum;
                        if (!string.IsNullOrEmpty(name))
                        {
                            hum.Name = name;
                        }
                        if (!string.IsNullOrEmpty(Mobile))
                        {
                            hum.Telephone = Mobile;
                        }
                        if (!string.IsNullOrEmpty(birth))
                        {
                            hum.NativePlace = birth;
                        }
                        if (!string.IsNullOrEmpty(identity))
                        {
                            hum.IDcardNo = identity;
                        }
                        if (!string.IsNullOrEmpty(bank))
                        {
                            hum.BankNum = bank;
                        }

                        if (!string.IsNullOrEmpty(school))
                        {
                            hum.School = school;
                        }
                        if (!string.IsNullOrEmpty(major))
                        {
                            hum.Major = major;
                        }
                        if (!string.IsNullOrEmpty(graduatedate))
                        {
                            hum.GraduateTime = graduatedate;
                        }
                        if (!string.IsNullOrEmpty(height))
                        {
                            hum.Height = Convert.ToInt32(height);
                        }
                        if (!string.IsNullOrEmpty(weight))
                        {
                            hum.Weight = Convert.ToInt32(weight);
                        }
                        if (!string.IsNullOrEmpty(BWH))
                        {
                            hum.BWH = BWH;
                        }
                        if (!string.IsNullOrEmpty(speciality))
                        {
                            hum.speciality = speciality;
                        }
                        if (!string.IsNullOrEmpty(academic))
                        {
                            hum.Academic = academic;
                        }

                        hum.IsDraft = true;
                        dal.Entry(hum).State = EntityState.Modified;
                        //dal.Configuration.ValidateOnSaveEnabled = false;
                        dal.SaveChanges();
                        dal.Configuration.ValidateOnSaveEnabled = true;
                        var data = new
                        {
                            Value = true
                        };
                        return Json(data);
                    }
                }
                #endregion
                #region 草稿不存在
                if (hum == null)
                {
                    hum = new HumanBasicFile();
                    if (success == true)
                    {
                        hum.Id = Guid.NewGuid();
                        hum.CreatedManagerID = admin;
                        hum.EditManagerId = admin;
                        hum.createTime = DateTime.Now;
                        hum.uniNum = uniquenum;
                        if (!string.IsNullOrEmpty(name))
                        {
                            hum.Name = name;
                        }
                        if (!string.IsNullOrEmpty(Mobile))
                        {
                            hum.Telephone = Mobile;
                        }
                        if (!string.IsNullOrEmpty(birth))
                        {
                            hum.NativePlace = birth;
                        }
                        if (!string.IsNullOrEmpty(identity))
                        {
                            hum.IDcardNo = identity;
                        }
                        if (!string.IsNullOrEmpty(bank))
                        {
                            hum.BankNum = bank;
                        }

                        if (!string.IsNullOrEmpty(school))
                        {
                            hum.School = school;
                        }
                        if (!string.IsNullOrEmpty(major))
                        {
                            hum.Major = major;
                        }
                        if (!string.IsNullOrEmpty(graduatedate))
                        {
                            hum.GraduateTime = graduatedate;
                        }
                        if (!string.IsNullOrEmpty(height))
                        {
                            hum.Height = Convert.ToInt32(height);
                        }
                        if (!string.IsNullOrEmpty(weight))
                        {
                            hum.Weight = Convert.ToInt32(weight);
                        }
                        if (!string.IsNullOrEmpty(BWH))
                        {
                            hum.BWH = BWH;
                        }
                        if (!string.IsNullOrEmpty(speciality))
                        {
                            hum.speciality = speciality;
                        }
                        if (!string.IsNullOrEmpty(academic))
                        {
                            hum.Academic = academic;
                        }
                        hum.IsDraft = true;
                        dal.HumanBasicFile.Add(hum);
                        dal.SaveChanges();
                        var data = new
                        {
                            Value = true
                        };
                        return Json(data);
                    }
                #endregion
                }
            }
            return Json(true);
        }
        //获取兼职人员草稿
        [HttpPost]
        public ActionResult GetDraft()
        {
            string admin = Session["admin"].ToString();    //提取创建人
            //获取所需数据表
            HumanBasicFile hum = dal.HumanBasicFile.Where(a => a.IsDraft == true &&(a.IsDelete==false||a.IsDelete==null)&& a.CreatedManagerID == admin).OrderByDescending(a => a.createTime).FirstOrDefault();
            if (hum != null)
            {
                var data = new
                {
                    name = hum.Name,
                    uniquenum = hum.uniNum,
                    birth = hum.NativePlace,
                    identity = hum.IDcardNo,
                    telephone = hum.Telephone,
                    bank = hum.BankNum,
                    school = hum.School,
                    major = hum.Major,
                    graduate = hum.GraduateTime,
                    height = hum.Height,
                    weihet = hum.Weight,
                    BWH = hum.BWH,
                    speciality = hum.speciality,
                    academic = hum.Academic,
                    //personal = manager.Photo,
                    //student = manager.StudentCardPhoto
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
            //foreach (var item in manPhoto)
            //{
            //    if (picName == item.IDcardPhoto)
            //    {
            //        newname = PictureValiate.newName(fileName, extend);
            //        break;
            //    }
            //}
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
            //foreach (var item in manPhoto)
            //{
            //    if (picName == item.BankCardPhoto)
            //    {
            //        newname = PictureValiate.newName(fileName, extend);
            //        break;
            //    }
            //}
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

        #region 办事处改变时返回 fillBase 要改
        public JsonResult fillBaseCityValue()
        {
            var office = Request.Form.Get("staffInfo.office");
            string city = "";
            List<City> city1 = (from s in dal.City
                                from g in dal.Office
                                where s.OfficeId == g.Id && g.Name == office
                                select s).ToList();
            foreach (var item in city1)
            {
                city += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
            }
            var data = new
            {
                cityValue = city
            };
            return Json(data);
        }
        #endregion
        #region 上级改变时的返回
        public JsonResult fillBaseOfficeValue()
        {
            var cap = Request.Form.Get("staffInfo.captain");
            string office = "";
            string city = "";
            List<Office> off = (from s in dal.Managers
                                from g in dal.Office
                                where s.UserId == cap && s.City1.OfficeId == g.Id
                                select g).ToList();
            List<City> cit = (from s in dal.Managers
                              from g in dal.City
                              where s.UserId == cap && s.City == g.Id
                              select g).ToList();
            foreach (var item in off)
            {
                office += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
            }
            foreach (var item in cit)
            {
                city += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
            }
            var data = new
            {
                officeValue = office,
                cityValue = city
            };
            return Json(data);
        }
        #endregion
        #region 新增培训信息
        public ActionResult Trian_info(int i = 0)//增加培训信息
        {
            ViewBag.newId = Request["id"];
            return View();
        }
        //验证培训开始时间
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
        #region 添加新培训信息
        [HttpPost]
        public ActionResult Trian_info()//增加培训信息
        {
            string newsId = Request.Form.Get("newsId");//日报Id
            var captain = Request.Form.Get("trainInfo.captain");//上级
            var unique = Request.Form.Get("trainInfo.uniquenum");//唯一号
            var name = Request.Form.Get("staffInfo.name");//姓名
            //var identity = Request.Form.Get("staffInfo.identity");//身份证号
            //var office = Request.Form.Get("staffInfo.office");//办事处
            //var city = Request.Form.Get("staffInfo.city");//城市
            var product = Request.Form.Get("trainInfo.product");//产品信息
            var type = Request.Form.Get("trainInfo.type");//产品类型
            var date = Request.Form.Get("trainInfo.date");//培训开始时间
            ViewBag.cof1 = 1;
            if (string.IsNullOrEmpty(date))
            {
                ModelState.AddModelError("trainInfo.date", "时间开始时间不能为空");
            }
            var date1 = Request.Form.Get("trainInfo.date1");//培训结束时间
            if (string.IsNullOrEmpty(date1))
            {
                ModelState.AddModelError("trainInfo.date1", "结束时间不能为空");
            }
            var grade = Request.Form.Get("trainInfo.grade");//培训分数
            if (string.IsNullOrEmpty(grade))
            {
                ModelState.AddModelError("trainInfo.grade", "培训分数不能为空");
            }
            else if (!regexNum.IsMatch(grade))
            {
                ModelState.AddModelError("trainInfo.grade", "培训分数必须为整数");
            }
            else if (Convert.ToInt32(grade) > 100)
            {
                ModelState.AddModelError("trainInfo.grade", "培训分数必须<100");
            }
            var train = Request.Form.Get("trainInfo.trainer");//培训讲师
            if (string.IsNullOrEmpty(train))
            {
                ModelState.AddModelError("trainInfo.trainer", "培训讲师不能为空!");
            }
            Train tra = (from s in dal.Train where s.IsDraft == true select s).Distinct().FirstOrDefault();
            if (tra == null)
            {
                tra = new Train();
                tra.Id = Guid.NewGuid();
                #region 赋值给培训
                var list = from g in dal.HumanBasicFile
                           where g.uniNum == unique
                         && g.Name == name
                           select g;  //人员Id
                var productid = from s in dal.product
                                from g in dal.ProductCategory
                                where g.Id == s.ProductCategoryId
                                && s.Name == product
                                && g.Name == type   //产品Id
                                select s;
                foreach (var item in list)
                {
                    tra.HumanrId = item.Id;
                    tra.TrainProduction = type;
                    tra.Trainpro = product;
                    tra.TrainStartTime = date;
                    tra.TrainEndTime = date1;
                    tra.Creatorname = captain;
                    tra.TrainScore = Convert.ToInt32(grade);
                    item.TrainTimes++;
                    tra.Trainlecturer = train;
                    tra.IsDraft = false;
                    if (!string.IsNullOrEmpty(newsId))
                    {
                        tra.WorkContentId = Convert.ToInt32(newsId);
                    }

                    dal.Train.Add(tra);
                }
                if (!string.IsNullOrEmpty(newsId))
                {
                    int newId = Convert.ToInt32(newsId);
                    WorkContent wc = (from d in dal.WorkContent where d.Id == newId select d).FirstOrDefault();

                    wc.TrainCount = wc.TrainCount + 1;
                }
                dal.SaveChanges();
                #endregion
            }
            else
            {
                #region 赋值给培训
                var list = from g in dal.HumanBasicFile
                           where g.uniNum == unique
                         && g.Name == name 
                           select g;  //人员Id
                var productid = from s in dal.product
                                from g in dal.ProductCategory
                                where g.Id == s.ProductCategoryId
                                && s.Name == product
                                && g.Name == type   //产品Id
                                select s;
                foreach (var item in list)
                {
                    tra.HumanrId = item.Id;
                    tra.TrainProduction = type;
                    tra.Trainpro = product;
                    tra.TrainStartTime = date;
                    tra.TrainEndTime = date1;
                    tra.Creatorname = captain;
                    tra.TrainScore = Convert.ToInt32(grade);
                    tra.Trainlecturer = train;
                    item.TrainTimes++;
                    if (!string.IsNullOrEmpty(newsId))
                    {
                        tra.WorkContentId = Convert.ToInt32(newsId);
                    }
                    tra.IsDraft = false;
                }
                dal.Entry(tra).State = EntityState.Modified;
                if (!string.IsNullOrEmpty(newsId))
                {
                    int newId = Convert.ToInt32(newsId);
                    WorkContent wc = (from d in dal.WorkContent where d.Id == newId select d).FirstOrDefault();

                    wc.TrainCount = wc.TrainCount + 1;
                }
                dal.SaveChanges();
                #endregion
            }
            if (tra != null)
            {
                ViewBag.cof1 = 1;
            }
            return View();
        }
        #endregion
        #region 产品信息
        public JsonResult selectProduct()
        {
            List<ProductCategory> list1 = dal.ProductCategory.ToList();
            ProductCategory lis = dal.ProductCategory.FirstOrDefault();
             List<product> list;
             var product1 = "";
             var productcategory = "";
             if (lis != null)
             {
                 list = (from s in dal.product where s.ProductCategory.Name == lis.Name select s).ToList();  
                 foreach (var item in list)
                 {
                     product1 += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
                 }
                 foreach (var item in list1)
                 {
                     productcategory += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";
                 }
             }
            var data = new
            {
                productHtml = product1, //产品型号
                typeHtml = productcategory      //培训产品
            };
            return Json(data);
        }

        #endregion
        [HttpPost]
        public JsonResult getunique()
        {
            var chooseunique = Request.Params.Get("chooseunique");
            var uniquen = Request.Params.Get("uniquen");
            string admin = Session["admin"].ToString();    //保存创建人
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.uniNum == chooseunique && (s.IsDelete == false || s.IsDelete == null)
                          && s.Boss == g.Id && g.UserId == admin
                          select s).Distinct().FirstOrDefault();
            string un = "";
            string un1 = "";
            un += "<option value=\"" + unique.uniNum + "\">" + unique.uniNum + "</option>";
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name,
                remark = unique.Remark
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new
                {
                    chooseuniqueValue = un1,
                    remark = unique.Remark
                };
                return Json(data1);
            }
            return Json(data);
        }//培训/自检唯一号查询
        public JsonResult getunique2()//自检唯一号查询
        {
            var chooseunique = Request.Params.Get("chooseunique");
            var uniquen = Request.Params.Get("uniquen");
            string admin = Session["admin"].ToString();    //保存创建人
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.uniNum == chooseunique
                          && s.Boss == g.Id && g.UserId == admin 
                          select s).Distinct().FirstOrDefault();
            string un = "";
            string un1 = "";
            un += "<option value=\"" + unique.uniNum + "\">" + unique.uniNum + "</option>";
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new { chooseuniqueValue = un1 };
                return Json(data1);
            }
            return Json(data);
        }//培训/自检唯一号查询
        public JsonResult getunique1()
        {
            var chooseunique = Request.Params.Get("chooseunique");
            var uniquen = Request.Params.Get("uniquen");
            string admin = Session["admin"].ToString();    //保存创建人
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.uniNum == chooseunique &&(s.IsDelete==false||s.IsDelete==null)
                          && s.Boss == g.Id && g.UserId == admin 
                          select s).Distinct().FirstOrDefault();
            string un = "";
            string un1 = "";
            un += "<option value=\"" + unique.uniNum + "\">" + unique.uniNum + "</option>";
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new { chooseuniqueValue = un1 };
                return Json(data1);
            }
            return Json(data);
        }//上班唯一号查询
        [HttpPost]
        public JsonResult getname()//培训姓名查询
        {
            var chooseunique = Request.Params.Get("choosename");
            string admin = Session["admin"].ToString();    //保存创建人
            var uniquen = Request.Params.Get("uniquen");
            var unique1 = (from s in dal.HumanBasicFile
                           from g in dal.Managers
                           where s.Name.Contains(chooseunique) && s.Boss == g.Id && (s.IsDelete == false || s.IsDelete == null) && g.UserId == admin
                           select s).Distinct().OrderBy(a => a.uniNum).ToList();
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.Name.Contains(chooseunique) && (s.IsDelete == false || s.IsDelete == null) && s.Boss == g.Id && g.UserId == admin
                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
            string un = "";
            string un1 = "";
            foreach (var item in unique1)
            {
                un += "<option value=\"" + item.uniNum + "\">" + item.uniNum + "</option>";
            }
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name,
                remark = unique.Remark
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new
                {
                    chooseuniqueValue = un1,
                    remark = unique.Remark
                };
                return Json(data1);
            }
            return Json(data);
        }
        public JsonResult getname1()//上班姓名查询
        {
            var chooseunique = Request.Params.Get("choosename");
            string admin = Session["admin"].ToString();    //保存创建人
            var uniquen = Request.Params.Get("uniquen");
            var unique1 = (from s in dal.HumanBasicFile
                           from g in dal.Managers
                           where s.Name.Contains(chooseunique) && s.Boss == g.Id && g.UserId == admin 
                           select s).Distinct().OrderBy(a => a.uniNum).ToList();
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.Name.Contains(chooseunique) && s.Boss == g.Id && g.UserId == admin 
                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
            string un = "";
            string un1 = "";
            foreach (var item in unique1)
            {
                un += "<option value=\"" + item.uniNum + "\">" + item.uniNum + "</option>";
            }
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new { chooseuniqueValue = un1 };
                return Json(data1);
            }
            return Json(data);
        }
        public JsonResult getname2()//点检姓名查询
        {
            var chooseunique = Request.Params.Get("choosename");
            string admin = Session["admin"].ToString();    //保存创建人
            var uniquen = Request.Params.Get("uniquen");
            var unique1 = (from s in dal.HumanBasicFile
                           from g in dal.Managers
                           where s.Name.Contains(chooseunique) && s.Boss == g.Id && g.UserId == admin 
                           select s).Distinct().OrderBy(a => a.uniNum).ToList();
            var unique = (from s in dal.HumanBasicFile
                          from g in dal.Managers
                          where s.Name.Contains(chooseunique) && s.Boss == g.Id && g.UserId == admin 
                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
            string un = "";
            string un1 = "";
            foreach (var item in unique1)
            {
                un += "<option value=\"" + item.uniNum + "\">" + item.uniNum + "</option>";
            }
            var data = new
            {
                chooseuniqueValue = un,
                name = unique.Name,
                identity = unique.IDcardNo,
                office = unique.City1.Office.Name,
                city = unique.City1.Name
            };
            if (data == null)
            {
                un1 += "<option value=\"" + uniquen + "\">" + uniquen + "</option>";
                var data1 = new { chooseuniqueValue = un1 };
                return Json(data1);
            }
            return Json(data);
        }
        [HttpPost]
        public ActionResult unionUniquenumHtmlValue()//培训/返回
        {

            List<Managers> list = dal.Managers.ToList();
            list = list.Where(da => da.Authority != 0).ToList();
            string captain2 = "";
            string uniquenum2 = "";
            string name2 = "";
            string identity2 = "";
            string office2 = "";
            string city2 = "";
            string remark = "";
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();
            string wor = Request.Params.Get("work");
            try
            {
                //HumanBasicFile hum = dal.HumanBasicFile.FirstOrDefault();
                ////city2 = hum.City1.Name;//城市  
                //name2 = hum.Name;//姓名
                //identity2 = hum.IDcardNo; //身份证号
                //office2 = hum.City1.Office.Name; //办事处
                List<HumanBasicFile> dj = (from s in dal.HumanBasicFile
                                           from g in dal.DianJian
                                           where g.HumanId == s.Id&&(s.IsDelete==null||s.IsDelete==false) && g.Score < 80 
                                           select s).Distinct().OrderBy(a => a.uniNum).ToList();
                if (authority != "管理员")
                {

                    Managers man = new Managers();
                    if (authority == "小队长")
                    {
                        string boss_String = (from d in dal.Managers where d.UserId == admin select d.Boss).FirstOrDefault();
                        man = (from d in dal.Managers where d.UserId == boss_String select d).FirstOrDefault();
                    }
                    else if (authority == "督导")
                    {
                        man = (from s in dal.Managers where s.UserId == admin && s.IsDraft == false select s).FirstOrDefault();
                    }
                    else
                    {
                        return View("~/Views/Shared/AuthorityError.cshtml");
                    }
                    captain2 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>";
                    office2 = man.City1.Office.Name;
                    city2 = man.City1.Name;
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where (man.Id == s.Boss) && (s.IsDelete == null || s.IsDelete == false)
                                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
                    if (hum != null)
                    {
                        identity2 = hum.IDcardNo;
                        name2 = hum.Name;
                        remark = hum.Remark;
                    }
                    List<HumanBasicFile> un1 = (from s in dal.HumanBasicFile
                                                where man.Id == s.Boss && (s.IsDelete == null || s.IsDelete == false)
                                                select s).Distinct().OrderBy(a => a.uniNum).ToList();
                    foreach (var item1 in un1)
                    {
                        uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                    }
                    //foreach (var iten2 in dj)
                    //{
                    //    uniquenum2 += "<option value=\"" + iten2.uniNum + "\">" + iten2.uniNum + "</option>";//唯一号
                    //}
                   
                }
                else
                {
                    Managers man = (from s in dal.Managers where s.Authority != 0 && s.IsDraft == false select s).FirstOrDefault();
                    office2 = man.City1.Office.Name; //办事处
                    city2 = man.City1.Name;
                    captain2 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>"; //上级
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where man.Id == s.Boss && (s.IsDelete == null || s.IsDelete == false)
                                          select s).Distinct().OrderBy(a => a.uniNum).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
                    if (hum != null)
                    {
                        identity2 = hum.IDcardNo;
                        name2 = hum.Name;
                    }
                    foreach (var item in list)
                    {
                        if (item.UserId != man.UserId)
                        {
                            captain2 += "<option value=\"" + item.UserId + "\">" + item.UserId + "</option>"; //上级  
                        }
                    }
                    List<HumanBasicFile> un = (from s in dal.HumanBasicFile
                                               where man.Id == s.Boss && (s.IsDelete == null || s.IsDelete == false)
                                               select s).Distinct().OrderBy(a => a.uniNum).ToList();
                    foreach (var item1 in un)
                    {

                        uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                    }
                    foreach (var item2 in dj)
                    {
                        uniquenum2 += "<option value=\"" + item2.uniNum + "\">" + item2.uniNum + "</option>";//唯一号
                    }
                }
                var data = new
                {
                    staffInfo = "yang",
                    name = name2,   //名字
                    identity = identity2,  //身份证号码
                    city = city2,
                    uniquenumValue = uniquenum2, //唯一号
                    captainValue = captain2,   //上级    
                    office = office2,       //办事处     
                    remark = remark
                };
                return Json(data);
            }
            catch
            {
                var data = new { };
                return Json(data);
            }
        }
        [HttpPost]
        public ActionResult unionUniquenumHtmlValue2()//自检/返回
        {

            List<Managers> list = dal.Managers.ToList();
            list = list.Where(da => da.Authority != 0).ToList();
            string captain2 = "";
            string uniquenum2 = "";
            string name2 = "";
            string identity2 = "";
            string office2 = "";
            string city2 = "";
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();
            string wor = Request.Params.Get("work");
            try
            {
                //HumanBasicFile hum = dal.HumanBasicFile.FirstOrDefault();
                ////city2 = hum.City1.Name;//城市  
                //name2 = hum.Name;//姓名
                //identity2 = hum.IDcardNo; //身份证号
                //office2 = hum.City1.Office.Name; //办事处
                if (authority != "管理员")
                {
                    Managers man = new Managers();
                   if (authority == "小队长")
                    {
                        string boss_String = (from d in dal.Managers where d.UserId == admin select d.Boss).FirstOrDefault();
                        man = (from d in dal.Managers where d.UserId == boss_String select d).FirstOrDefault();
                    }
                    else if (authority == "督导")
                    {
                        man = (from s in dal.Managers where s.UserId == admin && s.IsDraft == false select s).FirstOrDefault();
                    }
                    else
                    {
                        return View("~/Views/Shared/AuthorityError.cshtml");
                    }
                    captain2 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>"; 
                    office2 = man.City1.Office.Name;
                    city2 = man.City1.Name;
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where man.Id == s.Boss && s.HumanLevel != "C" && (s.IsDelete == null || s.IsDelete == false)
                                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
                    if (hum != null)
                    {
                        identity2 = hum.IDcardNo;
                        name2 = hum.Name;
                    }

                    List<HumanBasicFile> un1 = (from s in dal.HumanBasicFile where man.Id == s.Boss && (s.IsDelete == null || s.IsDelete == false) && s.HumanLevel != "C" select s).Distinct().OrderBy(a => a.uniNum).ToList();
                    foreach (var item1 in un1)
                    {
                        uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                    }
                }
                else
                {
                    Managers man = (from s in dal.Managers where s.Authority != 0 && s.IsDraft == false select s).FirstOrDefault();
                    office2 = man.City1.Office.Name; //办事处
                    city2 = man.City1.Name;
                    captain2 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>"; //上级
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where man.Id == s.Boss && s.HumanLevel != "C" && (s.IsDelete == null || s.IsDelete == false)
                                          select s).Distinct().OrderBy(a => a.uniNum).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
                    if (hum != null)
                    {
                        identity2 = hum.IDcardNo;
                        name2 = hum.Name;
                    }
                    foreach (var item in list)
                    {
                        if (item.UserId != man.UserId)
                        {
                            captain2 += "<option value=\"" + item.UserId + "\">" + item.UserId + "</option>"; //上级  
                        }
                    }
                    List<HumanBasicFile> un = (from s in dal.HumanBasicFile where man.Id == s.Boss && (s.IsDelete == null || s.IsDelete == false) && s.HumanLevel != "C" select s).Distinct().OrderBy(a => a.uniNum).ToList();
                    foreach (var item1 in un)
                    {

                        uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                    }
                }
                var data = new
                {
                    staffInfo = "yang",
                    name = name2,   //名字
                    identity = identity2,  //身份证号码
                    city = city2,
                    uniquenumValue = uniquenum2, //唯一号
                    captainValue = captain2,   //上级    
                    office = office2        //办事处          
                };
                return Json(data);
            }
            catch
            {
                var data = new { };
                return Json(data);
            }
        }
        [HttpPost]
        public JsonResult selectType() //产品改变时发生的改变
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
        [HttpPost]
        public JsonResult fillTrainFormValue() //唯一号改变时发生的改变
        {
            var unique = Request.Form.Get("trainInfo.uniquenum");//唯一号
            // var cap=Request.Form.Get("trainInfo.captain");//上级
            string name2 = "";
            string identity2 = "";
            string office2 = "";
            string city2 = "";
            HumanBasicFile un = (from s in dal.HumanBasicFile
                                 from g in dal.Managers
                                 where g.Id == s.Boss && s.uniNum == unique
                                 select s).FirstOrDefault();

            city2 = un.City1.Name;//城市
            office2 += un.City1.Office.Name;//办事处
            name2 += un.Name;//用户名
            identity2 += un.IDcardNo; //身份证号


            var data = new
            {
                name = name2,
                identity = identity2,
                office = office2,
                city = city2,
                remark = un.Remark
            };
            return Json(data);
        }
        [HttpPost]
        public JsonResult fillTrainUniquenumValue()//captain改变时发生的改变
        {
            var cap = Request.Form.Get("trainInfo.captain");
            string name2 = "";
            string identity2 = "";
            string office2 = "";
            string city2 = "";
            string unique2 = "";
            string remark = "";
            Managers list = (from s in dal.Managers where s.UserId == cap select s).FirstOrDefault();
            List<HumanBasicFile> un = (from s in dal.HumanBasicFile
                                       where list.Id == s.Boss && list.UserId == cap
                                       select s).Distinct().ToList();
            //HumanBasicFile uun = (from s in dal.HumanBasicFile 
            //                      where s.Boss==list.Id
            //                      select s).FirstOrDefault();
            foreach (var item2 in un)
            {
                office2 = item2.City1.Office.Name;//办事处
                city2 = item2.City1.Name;//城市
                name2 = item2.Name;//用户名
                identity2 = item2.IDcardNo; //身份证号
                remark = item2.Remark;
                break;
            }
            foreach (var item1 in un)
            {
                unique2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号               
            }
            var data = new
            {
                uniquenumValue = unique2,
                name = name2,
                identity = identity2,
                office = office2,
                city = city2,
                remark = remark
            };
            return Json(data);
        }
        #region 新增上班信息
        public ActionResult On_Duty()//新增上班信息
        {
            ViewBag.shangban = 1;
            ViewBag.newId = Request["id"];
            return View();
        }
        #endregion
        [HttpPost]
        public ActionResult On_Duty(int i = 1)//添加新增上班信息
        {
            string newsId = Request.Form.Get("newsId");//日报Id
            var captain = Request.Form.Get("trainInfo.captain");//上级
            var unique = Request.Form.Get("trainInfo.uniquenum");//唯一号
            var name = Request.Form.Get("workInfo.name");//姓名
            //var identity = Request.Form.Get("staffInfo.identity");//身份证号
            //var office = Request.Form.Get("staffInfo.office");//办事处
            //var city = Request.Form.Get("staffInfo.city");//城市
            var activity = Request.Form.Get("workInfo.activity");//活动名称
            ViewBag.cof = 0;
            ViewBag.shangban = 1;
            if (string.IsNullOrEmpty(activity))
            {
                ModelState.AddModelError("workInfo.activity", "活动名称不能为空");
            }
            var product = Request.Form.Get("workInfo.product");//产品名称
            if (string.IsNullOrEmpty(product))
            {
                ModelState.AddModelError("workInfo.product", "产品名称不能为空");
            }
            var subsity = Request.Form.Get("workInfo.subsidy");//补助
            var workinfo = Request.Form.Get("workInfo.wage");//日薪标准
            if (string.IsNullOrEmpty(workinfo))
            {
                ModelState.AddModelError("workInfo.wage", "日薪标准不能为空");
            }
            var date = Request.Form.Get("workInfo.date");//上班时间
            if (string.IsNullOrEmpty(date))
            {
                ModelState.AddModelError("workInfo.date", "上班时间不能为空!");
            }
            var work = Request.Form.Get("workInfo.work");//上班职能
            if (string.IsNullOrEmpty(work))
            {
                ModelState.AddModelError("workInfo.work", "上班职能不能为空");
            }
            var store1 =Request.Form.Get("workInfo.store");//门店
            var store = Convert.ToInt32(store1);
            if (string.IsNullOrEmpty(store1))
            {
                ModelState.AddModelError("workInfo.store", "门店不能为空");
            }
            HttpPostedFileBase image = Request.Files["workimage"];//工作照片
            if (image == null)
            {
                ModelState.AddModelError("workimage", "身份证照片不能为空！");
            }
            var remark = Request.Form.Get("workInfo.remark");//备注
            var list = (from g in dal.HumanBasicFile
                        where g.uniNum == unique
                        select g).FirstOrDefault();  //人员Id
            AttendingInfo dj = (from s in dal.AttendingInfo where s.IsDraft == true select s).Distinct().FirstOrDefault();
            if (dj == null)
            {
                dj = new AttendingInfo();
                dj.Id = Guid.NewGuid();
                # region //上传工作照片
                if (!string.IsNullOrEmpty(image.FileName))//上传工作照片
                {
                    string file = PicName4(image.FileName);
                    var ptho = unique + "-" + name + "-上班照片";
                    string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                    ptho = PictureValiate.newName(ptho, extend);
                    image.SaveAs(Server.MapPath("~/uploadImg/workimage/") + ptho);
                    file = Suo.TouXiangSuoFang(image, ptho, Server.MapPath("~/uploadImg/workimage/suo/"), 114, 125);
                    dj.WorkPhoto = file;
                }
                #endregion
                #region 赋值给上班人员
                dj.HumanId = list.Id;
                var tt = (from s in dal.AttendingInfo where s.HumanId == list.Id&&s.Date==date select s).Count();
                if (tt >= 1)
                {
                    ViewBag.shangban = 0;
                    return View();
                }
                dj.ActionName = activity.Trim();
                dj.production = product;
                if (!string.IsNullOrEmpty(subsity))
                {
                    dj.BearFees = Convert.ToInt32(subsity);
                }
                dj.StandardSalary = Convert.ToInt32(workinfo);
                dj.Date = date;
                dj.Functions = work;
                var dd = (from s in dal.SShop
                          where s.Id == store
                          select s).FirstOrDefault();
                if (dd != null)
                {
                    dj.Department = dd.Id;
                }
                dj.Remark = remark;
                dj.IsDraft = false;
                dj.Creatorname = captain;
                if (!string.IsNullOrEmpty(newsId))
                {
                    dj.WorkContentId = Convert.ToInt32(newsId);
                }
                dal.AttendingInfo.Add(dj);
                if (!string.IsNullOrEmpty(newsId))
                {
                    int newId = Convert.ToInt32(newsId);
                    WorkContent wc = (from d in dal.WorkContent where d.Id==newId select d).FirstOrDefault();
                    wc.MoneyCount = Convert.ToInt32(subsity) + Convert.ToInt32(workinfo);
                    wc.AttendCount = wc.AttendCount + 1;
                }
                dal.SaveChanges();
                #endregion
            }
            else
            {
                # region //上传个人声明照片
                if (!string.IsNullOrEmpty(image.FileName))//上传个人声明照片
                {
                    string file = PicName3(image.FileName);
                    var ptho = unique + "-" + name + "-上班照片";
                    string extend = file.Remove(0, file.LastIndexOf(".") + 1);
                    ptho = PictureValiate.newName(ptho, extend);
                    image.SaveAs(Server.MapPath("~/uploadImg/workimage/") + ptho);
                    file = Suo.TouXiangSuoFang(image, ptho, Server.MapPath("~/uploadImg/workimage/suo/"), 114, 125);
                    dj.WorkPhoto = file;
                }
                #endregion
                #region 赋值给上班人员
                dj.HumanId = list.Id;
                var tt = (from s in dal.AttendingInfo where s.HumanId == list.Id && s.Date == date select s).Count();
                if (tt >= 1)
                {
                    ViewBag.shangban = 0;
                }
                dj.ActionName = activity.Trim();
                dj.production = product;
                if (!string.IsNullOrEmpty(subsity))
                {
                    dj.BearFees = Convert.ToInt32(subsity);
                }
                dj.StandardSalary = Convert.ToInt32(workinfo);
                dj.Date = date;
                dj.Functions = work;
                var dd = (from s in dal.SShop
                          where s.Id == store
                          select s).FirstOrDefault();
                if (dd != null)
                {
                    dj.Department = dd.Id;
                }
                dj.Remark = remark;
                dj.IsDraft = false;
                dj.Creatorname = captain;
                if (!string.IsNullOrEmpty(newsId))
                {
                    dj.WorkContentId = Convert.ToInt32(newsId);
                }
                dal.Entry(dj).State = EntityState.Modified;
                if (!string.IsNullOrEmpty(newsId))
                {
                    int newId = Convert.ToInt32(newsId);
                    WorkContent wc = (from d in dal.WorkContent where d.Id == newId select d).FirstOrDefault();
                    wc.MoneyCount = Convert.ToInt32(subsity) + Convert.ToInt32(workinfo);
                    wc.AttendCount = wc.AttendCount + 1;
                }
                dal.SaveChanges();

                #endregion

            }
            if (dj != null)
            {
                ViewBag.cof = 1;
            }
            return View(ViewBag.shangban);
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
        [HttpPost]
        public JsonResult Caogao()//保存上班草稿箱
        {
            var activity = Request.Form.Get("activity");//活动名称
            var subsidy = Request.Form.Get("subsidy");//补助
            var wage = Request.Form.Get("wage");//日薪标准
            var date = Request.Form.Get("date");
            var uniquenum = Request.Form.Get("uniquenum");//唯一号
            var remark = Request.Form.Get("remark");//备注
            var ids = Request.Params.Get("id");
            Guid id = Guid.NewGuid();
            string admin = Session["admin"].ToString();
            if (!string.IsNullOrEmpty(ids))
            {
                id = new Guid(ids);

            }
            //HumanBasicFile hum=(from s in dal.HumanBasicFile where s.uniNum==uniquenum
            //                        &&s.IsDraft==true select s).FirstOrDefault();
            AttendingInfo hum = (from s in dal.AttendingInfo where s.HumanBasicFile.Managers.UserId== admin &&s.Id == id && s.IsDraft == true select s).FirstOrDefault();
            #region 草稿不存在
            if (hum == null)
            {
                HumanBasicFile hum1 = (from s in dal.HumanBasicFile
                                       where s.uniNum == uniquenum
                                           && s.Managers.UserId == admin
                                       select s).FirstOrDefault();
                AttendingInfo att = new AttendingInfo();
                if (activity != null | subsidy != null | wage != null | date != null | remark != null)
                {
                    att.Id = id;
                    att.IsDraft = true;
                    att.ActionName = activity;
                    att.Date = date;
                    att.HumanId = hum1.Id;
                    if (!string.IsNullOrEmpty(subsidy))
                    {
                        att.BearFees = Convert.ToInt32(subsidy);
                    }
                    if (!string.IsNullOrEmpty(wage))
                    {
                        att.StandardSalary = Convert.ToInt32(wage);
                    }
                    att.Remark = remark;
                    att.Creatorname = admin;
                    dal.AttendingInfo.Add(att);
                    dal.SaveChanges();
                    return Json(true);
                }
            }
            #endregion
            #region 草稿存在
            else if (hum != null)
            {
                if (activity != null | subsidy != null | wage != null | date != null | remark != null)
                {
                    HumanBasicFile hum1 = (from s in dal.HumanBasicFile
                                           where s.uniNum == uniquenum
                                               && s.Managers.UserId == admin
                                           select s).FirstOrDefault();
                    hum.IsDraft = true;
                    hum.ActionName = activity;
                    hum.Date = date;
                    hum.HumanId = hum1.Id;
                    if (!string.IsNullOrEmpty(subsidy))
                    {
                        hum.BearFees = Convert.ToInt32(subsidy);
                    }
                    if (!string.IsNullOrEmpty(wage))
                    {
                        hum.StandardSalary = Convert.ToInt32(wage);
                    }
                    hum.Remark = remark;
                    hum.Creatorname = admin;
                    dal.Entry(hum).State = EntityState.Modified;
                    dal.SaveChanges();
                    return Json(true);
                }
            }
            #endregion
            return Json(false);
        }
        [HttpPost]
        public JsonResult Caogao1()//保存培训草稿箱
        {
            var grade = Request.Params.Get("grade");
            var date2 = Request.Params.Get("date2");
            var date1 = Request.Params.Get("date1");
            var trainer = Request.Params.Get("trainer");
            var uniquenum1 = Request.Params.Get("uniquenum1");
            string admin = Session["admin"].ToString();
            var ids = Request.Form.Get("id");
            Guid id = Guid.NewGuid();
            if (!string.IsNullOrEmpty(ids))
            {
                id = new Guid(ids);

            }
            //HumanBasicFile hum = (from s in dal.HumanBasicFile where s.uniNum == uniquenum1&&s.IsDraft==false select s).Distinct().FirstOrDefault();
            Train hum = (from s in dal.Train where s.Id == id && s.IsDraft == true && s.HumanBasicFile.Managers.UserId == admin select s).FirstOrDefault();
            #region 草稿不存在
            if (hum == null)
            {
                HumanBasicFile hum1 = (from s in dal.HumanBasicFile
                                       where s.uniNum == uniquenum1
                                           && s.Managers.UserId == admin
                                       select s).FirstOrDefault();
                if (grade != null | date1 != null | date2 != null | trainer != null)
                {
                    Train tra = new Train();
                    tra.Id = id;
                    tra.IsDraft = true;
                    if (grade != "")
                    {
                        tra.TrainScore = Convert.ToInt32(grade);
                    }
                    tra.HumanrId = hum1.Id;
                    tra.Creatorname = admin;
                    tra.TrainStartTime = date2;
                    tra.TrainEndTime = date1;
                    tra.Trainlecturer = trainer;
                    dal.Train.Add(tra);
                    dal.SaveChanges();
                    return Json(true);
                }
            }
            #endregion
            #region 草稿存在
            if (hum != null)
            {
                HumanBasicFile hum1 = (from s in dal.HumanBasicFile
                                       where s.uniNum == uniquenum1
                                           && s.Managers.UserId == admin
                                       select s).FirstOrDefault();
                if (grade != null | date1 != null | date2 != null | trainer != null)
                {
                    hum.IsDraft = true;
                    if (grade != "")
                    {
                        hum.TrainScore = Convert.ToInt32(grade);
                    }
                    hum.HumanrId = hum1.Id;
                    hum.TrainStartTime = date2;
                    hum.Creatorname = admin;
                    hum.TrainEndTime = date1;
                    hum.Trainlecturer = trainer;
                    dal.Entry(hum).State = EntityState.Modified;
                    dal.SaveChanges();
                    return Json(true);
                }
            }
            #endregion
            return Json(false);
        }
        public JsonResult Getcao()//获取上班草稿
        {
            //var uni = Request.Params.Get("trainInfo.uniquenum");
            string admin = Session["admin"].ToString();
            AttendingInfo att = (from s in dal.AttendingInfo where s.IsDraft == true where s.Creatorname== admin&&s.HumanBasicFile.Managers.UserId == admin select s).Distinct().FirstOrDefault();
            if (att != null)
            {
                var data = new
                {
                    activity = att.ActionName,
                    subsidy = att.BearFees,
                    wage = att.StandardSalary,
                    date = att.Date,
                    id = att.Id,
                    //uniquenum = Request.Form.Get("uniquenum");//唯一号
                    remark = att.Remark//备注
                };
                return Json(data);
            }
            else
            {
                var data = new { };
                return Json(data);
            }
        }
        public JsonResult Getcao1()//获取培训草稿
        {
            //var uni = Request.Form.Get("trainInfo.uniquenum");
            string admin = Session["admin"].ToString();
            Train tra = (from s in dal.Train where s.IsDraft == true &&s.Creatorname==admin&&s.HumanBasicFile.Managers.UserId == admin select s).Distinct().FirstOrDefault();
            if (tra != null)
            {
                var data = new
                {
                    grade = tra.TrainScore,
                    date2 = tra.TrainStartTime,
                    date1 = tra.TrainEndTime,
                    trainer = tra.Trainlecturer,
                    id = tra.Id
                };
                return Json(data);
            }
            else
            {
                var data = new { };
                return Json(data);
            }
        }
        [HttpPost]
        public ActionResult unionUniquenumHtmlValue1()//上班信息页面返回
        {

            List<Managers> list = dal.Managers.ToList();
            list = list.Where(da => da.Authority != 0 && da.IsDraft == false).ToList();
            string captain2 = "";
            string uniquenum2 = "";
            string name2 = "";
            string identity2 = "";
            string office2 = "";
            string city2 = "";
            string wor = Request.Params.Get("work");
            string pro = "";
            string admin = Session["admin"].ToString();
            string authority = Session["authority"].ToString();
            try
            {
                List<ProductCategory> p = dal.ProductCategory.ToList();
                //city2 = hum.City1.Name;//城市  
                //name2 = hum.Name;//姓名
                //identity2 = hum.IDcardNo; //身份证号
                foreach (var item in p)
                {
                    pro += "<option value=\"" + item.Name + "\">" + item.Name + "</option>";//培训产品
                }
                if (authority != "管理员")
                {
                   
                    Managers man = new Managers();
                    if (authority == "小队长")
                    {
                        string boss_String = (from d in dal.Managers where d.UserId == admin select d.Boss).FirstOrDefault();
                        man = (from d in dal.Managers where d.UserId == boss_String select d).FirstOrDefault();
                    }
                    else if (authority == "督导")
                    {
                        man = (from s in dal.Managers where s.UserId == admin && s.IsDraft == false select s).FirstOrDefault();
                    }
                    else
                    {
                        return View("~/Views/Shared/AuthorityError.cshtml");
                    }
                    captain2 += "<option value=\"" + man.UserId + "\">" + man.UserId + "</option>";
                    office2 = man.City1.Office.Name;
                    city2 = man.City1.Name;
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where man.Id == s.Boss && s.IsDraft == false &&(s.IsDelete==null||s.IsDelete==false)
                                          select s).Distinct().OrderBy(a => a.uniNum).FirstOrDefault();
                    if (hum != null)
                    {
                        name2 = hum.Name;
                        identity2 = hum.IDcardNo;
                        city2 = hum.City1.Name;
                        office2 = hum.City1.Office.Name;
                    }
                    List<HumanBasicFile> un1 = (from s in dal.HumanBasicFile where man.Id == s.Boss && s.IsDraft == false &&(s.IsDelete==null||s.IsDelete==false) select s).Distinct().OrderBy(a => a.uniNum).ToList();
                    foreach (var item1 in un1)
                    {
                        uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                    }
                }
                else
                {
                    Managers man = (from s in dal.Managers where s.Authority != 0 && s.IsDraft == false select s).FirstOrDefault();
                    office2 = man.City1.Office.Name; //办事处
                    city2 = man.City1.Name;
                    HumanBasicFile hum = (from s in dal.HumanBasicFile
                                          where man.Id == s.Boss && s.IsDraft == false && (s.IsDelete == null || s.IsDelete == false)
                                          select s).Distinct().FirstOrDefault();
                    if (hum != null)
                    {
                        identity2 = hum.IDcardNo;
                        name2 = hum.Name;
                        city2 = hum.City1.Name;
                        office2 = hum.City1.Office.Name;
                    }
                    foreach (var item in list)
                    {
                        captain2 += "<option value=\"" + item.UserId + "\">" + item.UserId + "</option>"; //上级  
                        List<HumanBasicFile> un = (from s in dal.HumanBasicFile where item.Id == s.Boss && s.IsDraft == false && (s.IsDelete == null || s.IsDelete == false) select s).Distinct().OrderBy(a => a.uniNum).ToList();
                        foreach (var item1 in un)
                        {

                            uniquenum2 += "<option value=\"" + item1.uniNum + "\">" + item1.uniNum + "</option>";//唯一号
                        }
                    }
                }
                var data = new
                {
                    staffInfo = "yang",
                    name = name2,   //名字
                    identity = identity2,  //身份证号码
                    city = city2,
                    uniquenumValue = uniquenum2, //唯一号
                    captainValue = captain2,   //上级    
                    office = office2,       //办事处       
                    product2 = pro
                };
                return Json(data);
            }
            catch
            {
                var data = new { };
                return Json(data);
            }
        }
        public ActionResult selectCityForStore()//搜索门店
        {
            var con = Request.Params.Get("content");
            var city = Request.Params.Get("city");
            var store = Request.Params.Get("store");
            string admin = Session["admin"].ToString();
            List<SShop> da = dal.SShop.ToList();
            if (con.Length != 0)
            {
                da = da.Where(dataFilter => dataFilter.Name.Contains(con)).ToList();
            }
                if (admin != "admin")
            {
                Office off = (from s in dal.Office
                              from g in dal.Managers
                              where g.UserId == admin && g.City1.Office.Id == s.Id
                              select s).FirstOrDefault();
                da = da.Where(dataFilter => dataFilter.City.Office.Name == off.Name && dataFilter.City.Name == city).ToList();
            }
            string mdian = "";
            foreach (var item in da)
            {
                if (item.Name != store)
                {
                    mdian += "<option value=\"" + item.Id + "\">" + item.Name + "</option>";
                }
            }
            var data = new
            {
                storeStoreHtml = mdian
            };
            return Json(data);
        }
        #region 自检信息
        public ActionResult Self_Checked(int i = 1)//自检信息
        {

            return View();
        }
        #endregion
        [HttpPost]
        public ActionResult Self_Checked()
        {
            ViewBag.cof = 0;
            var captain = Request.Form.Get("trainInfo.captain");//上级
            var unique = Request.Form.Get("trainInfo.uniquenum");//唯一号
            var name = Request.Form.Get("staffInfo.name");//姓名
            //var identity = Request.Form.Get("staffInfo.identity");//身份证号
            //var office = Request.Form.Get("staffInfo.office");//办事处
            //var city = Request.Form.Get("staffInfo.city");//城市
            var level = Request.Form.Get("checkInfo.level");//等级
            var date = Request.Form.Get("checkInfo.date");//自检时间
            var appearance = Request.Form.Get("checkInfo.appearance");//仪表仪容
            var attitude = Request.Form.Get("checkInfo.attitude");//工作态度
            var che = Request.Form.Get("checkInfo.remark");//点检内容
            var knowledge = Request.Form.Get("checkInfo.productkonwledge");//知识
            var list = from g in dal.HumanBasicFile
                       where g.uniNum == unique
                       select g.Id;  //人员Id
            DianJian dj = (from s in dal.DianJian where s.IsDraft == true select s).Distinct().FirstOrDefault();
            if (dj == null)
            {
                dj = new DianJian();
                dj.Id = Guid.NewGuid();
                #region 保存自检信息
                foreach (var item in list)
                {
                    dj.HumanId = item;
                    dj.DJTime = date;
                    dj.Face = Convert.ToInt32(appearance);
                    dj.WorkAttitude = Convert.ToInt32(attitude);
                    //dj.Remark = che;
                    dj.DJContent = che;
                    dj.KOP = Convert.ToInt32(knowledge);
                    dj.Score = (dj.Face + dj.WorkAttitude + dj.KOP) / 3;
                    dj.IsDraft = false;
                    dj.CreatedManager = captain;
                    dj.EditManagerID = captain;
                    dal.DianJian.Add(dj);
                }
                dal.SaveChanges();
                dal.Configuration.ValidateOnSaveEnabled = true;
                #endregion
            }
            else
            {
                #region 保存自检信息
                foreach (var item in list)
                {
                    dj.HumanId = item;
                    dj.DJTime = date;
                    dj.Face = Convert.ToInt32(appearance);
                    dj.WorkAttitude = Convert.ToInt32(attitude);
                    dj.DJContent = che;
                    dj.KOP = Convert.ToInt32(knowledge);
                    dj.Score = (dj.Face + dj.WorkAttitude + dj.KOP) / 3;
                    dj.IsDraft = false;
                    dj.CreatedManager = captain;
                    dj.EditManagerID = captain;
                }
                dal.Entry(dj).State = EntityState.Modified;
                dal.SaveChanges();
                dal.Configuration.ValidateOnSaveEnabled = true;
                #endregion
            }
            if (dj != null)
            {
                ViewBag.cof = "1";
            }
            return View();
        }//添加自检信息
        //获取是否有自检草稿
        public ActionResult Get_HasDraft()
        {
            string hasDraft = "0";
            var DraftList = (from s in dal.DianJian where s.IsDraft == true select s).ToList();
            if (DraftList.Count() > 0)
            {
                hasDraft = "1";
            }
            var data = new { hasDraft = hasDraft };
            return Json(data);
        }
        [HttpPost]
        public JsonResult Get_Checked_Draft()//获取自检信息草稿
        {
            string admin = Session["admin"].ToString();
            DianJian dj = (from s in dal.DianJian where s.IsDraft == true&&s.CreatedManager==admin&&s.HumanBasicFile.Managers.UserId==admin select s).OrderByDescending(a => a.DraftCreateTime).FirstOrDefault();
            if (dj != null)
            {
                var data = new
                {
                    date = dj.DJTime,
                    appearance = dj.Face,
                    attitude = dj.WorkAttitude,
                    remark = dj.Remark,
                    productkonwledge = dj.KOP
                };
                return Json(data);
            }
            else
            {
                var data = new
                {
                    value = false
                };
                return Json(data);
            }
        }
        public JsonResult save_GetChecked_Draft()//保存自检信息草稿
        {
            var date = Request.Form.Get("date");
            var appearance = Request.Form.Get("appearance");
            var attitude = Request.Form.Get("attitude");
            var remark = Request.Form.Get("remark");
            var productkonwledge = Request.Form.Get("productkonwledge");
            var uniquenum = Request.Form.Get("uniquenum");
            var captain = Request.Form.Get("captain");
            DianJian dj = new DianJian();
            DianJian hum = (from s in dal.DianJian where s.IsDraft == true && s.CreatedManager == captain select s).Distinct().FirstOrDefault();
            if (hum == null)
            {
                #region 草稿不存在
                dj.Id = Guid.NewGuid();
                if (date.Length != 0)
                {
                    dj.DJTime = date;
                }
                if (appearance.Length != 0)
                {
                    dj.Face = Convert.ToInt32(appearance);
                }
                if (attitude != "")
                {
                    dj.WorkAttitude = Convert.ToInt32(attitude);
                }
                dj.Remark = remark;
                if (productkonwledge != "")
                {
                    dj.KOP = Convert.ToInt32(productkonwledge);
                }
                dj.CreatedManager = captain;
                dj.EditManagerID = captain;
                dj.DraftCreateTime = DateTime.Now;
                dj.IsDraft = true;
                if (dj != null)
                {
                    dal.DianJian.Add(dj);
                    dal.SaveChanges();
                    dal.Configuration.ValidateOnSaveEnabled = true;
                    var data = new
                    {
                        Value = true
                    };
                    return Json(data);
                }
                #endregion
            }
            else if (hum != null)
            {
                #region  草稿存在
                dj = (from s in dal.DianJian where s.IsDraft == true select s).Distinct().FirstOrDefault();
                //dj = (from s in dal.DianJian where s.HumanId == hum.Id select s).Distinct().FirstOrDefault();
                if (date.Length != 0)
                {
                    dj.DJTime = date;
                }
                dj.CreatedManager = captain;
                dj.EditManagerID = captain;
                if (appearance.Length != 0)
                {
                    dj.Face = Convert.ToInt32(appearance);
                }
                if (attitude != "")
                {
                    dj.WorkAttitude = Convert.ToInt32(attitude);
                }
                dj.Remark = remark;
                if (productkonwledge != "")
                {
                    dj.KOP = Convert.ToInt32(productkonwledge);
                }
                dj.DraftCreateTime = DateTime.Now;
                dj.IsDraft = true;
                if (dj != null)
                {
                    dal.Entry(dj).State = EntityState.Modified;
                    dal.SaveChanges();
                    dal.Configuration.ValidateOnSaveEnabled = true;
                    var data = new
                    {
                        Value = true
                    };
                    return Json(data);
                }
                #endregion
            }
            //var data = new
            //{
            //    Value = true
            //};
            return Json(true);

        }
        public JsonResult checkLevel()//等级页面返回
        {
            var le = Request.Form.Get("uniquenum");
            var lev = (from g in dal.HumanBasicFile
                       where g.uniNum == le
                       select g).ToList();
            string leve = "";
            foreach (var item in lev)
            {
                leve += item.HumanLevel;
            }
            var data = new
            {
                level = leve
            };
            return Json(data);
        }
        #region 生成唯一码
        [HttpPost]
        public ActionResult unique()
        {
            var captain = Request.Form.Get("staffInfo.captain");
            var office = Request.Form.Get("staffInfo.office");//获得办事处
            string cap = null;
            Office code1 = (from s in dal.Office
                            where s.Name == office
                            select s).FirstOrDefault();
            string code = code1.mask;
            if (office.Length != 0)
            {
                int yi = (from s in dal.HumanBasicFile
                          from g in dal.Office
                          where s.City1.OfficeId == g.Id && s.IsDraft == false
                          && g.Name == office
                          select s.uniNum).Distinct().Count();
                HumanBasicFile hum = (from s in dal.HumanBasicFile
                                      from g in dal.Office
                                      where s.City1.OfficeId == g.Id 
                                      && g.Name == office && s.IsDraft == false
                                      select s).OrderByDescending(a => a.uniNum).FirstOrDefault();
                List<HumanBasicFile> hum1 = (from s in dal.HumanBasicFile
                                             from g in dal.Office
                                             where s.City1.OfficeId == g.Id
                                             && g.Name == office && s.IsDraft == false
                                             select s).OrderBy(a => a.uniNum).ToList();
                int last = 0;
                if (hum != null)
                {
                    var last1 = hum.uniNum.Remove(0, hum.uniNum.LastIndexOf("0") + 1);
                    if (last1.Length != 0)
                    {
                        last = Convert.ToInt32(last1);
                    }
                }
                if (last < 10)
                {
                    cap = code + "-000" + Convert.ToString(yi);
                    if (last == 9)
                    {
                        cap = code + "-00" + Convert.ToString(10);
                    }
                    for (int i = 0; i <= last; i++)
                    {
                        string cap1 = code + "-000" + Convert.ToString(i);
                        if (i == 9)
                        {
                            cap1 = code + "-00" + Convert.ToString(10);
                        }
                        var ca = from s in dal.HumanBasicFile where s.uniNum == cap1 select s.uniNum;
                        if (ca.Count() == 0)
                        {
                            cap = cap1;
                            break;
                        }
                    }
                }
                else if (last < 100)
                {
                    cap = code + "-00" + Convert.ToString(yi);
                    if (last == 99)
                    {
                        cap = code + "-0" + Convert.ToString(100);
                    }
                    for (int i = 10; i < last; i++)
                    {
                        string cap1 = "";
                        if (i < 100)
                        {
                            cap1 = code + "-00" + Convert.ToString(i);
                            if (i == 99)
                            {
                                cap1 = code + "-0" + Convert.ToString(100);
                            }
                        }
                        var ca = from s in dal.HumanBasicFile where s.uniNum == cap1 select s.uniNum;
                        if (ca.Count() == 0)
                        {
                            cap = cap1;
                        }
                    }
                }
                else if (last < 1000)
                {
                    cap = code + "-0" + Convert.ToString(yi);
                    if (last == 999)
                    {
                        cap = code + Convert.ToString(1000);
                    }
                    for (int i = 100; i < last; i++)
                    {
                        string cap1 = "";
                        if (i < 1000)
                        {
                            cap1 = code + "-0" + Convert.ToString(i);
                            if (i == 999)
                            {
                                cap1 = code + Convert.ToString(1000);
                            }
                        }
                        var ca = from s in dal.HumanBasicFile where s.uniNum == cap1 select s.uniNum;
                        if (ca.Count() == 0)
                        {
                            cap = cap1;
                        }
                    }
                }
                else
                {
                    cap = code + "-" + Convert.ToString(yi);
                    for (int i = 1000; i < last; i++)
                    {
                        string cap1 = "";
                        cap1 = code + Convert.ToString(i);
                        var ca = from s in dal.HumanBasicFile where s.uniNum == cap1 select s.uniNum;
                        if (ca.Count() == 0)
                        {
                            cap = cap1;
                        }
                    }
                }
            }

            var data = new
            {
                staffInfo = cap  //返回json

            };

            return Json(data);
        }
        #endregion
        #region 身份证号验证
        public JsonResult validateIdentity()
        {
            bool success = true;
            var IdList = (from id in dal.HumanBasicFile.ToList() where id.IsDraft==false&&id.IsDelete!=true select id.IDcardNo).ToList();
            string Id = Request["staffInfo.identity"].ToString();
            foreach (var id in IdList)
            {
                if (id == Id)
                {
                    success = false;
                    break;
                }
            }
            return Json(success);
        }
        #endregion
        #region 电话验证
        public JsonResult validateMobile()
        {
            bool success = true;
            //var IdList = (from id in dal.Managers.ToList() select id.Telephone).ToList();
            string Id = Request["staffInfo.mobile"].ToString();
            //foreach (var id in IdList)
            //{
            if (Id.Length != 11)
            {
                success = false;
            }
            //}
            return Json(success);
        }
        #endregion
        #region 银行卡验证
        public JsonResult validateBank()
        {
            bool success = true;
            //var IdList = (from id in dal.Managers.ToList() select id.IDcard).ToList();
            string Id = Request["staffInfo.bank"].ToString();
            //foreach (var id in IdList)
            //{
            if (!regexNum.IsMatch(Id))
            {
                success = false;
            }
            //}
            return Json(success);
        }
        #endregion
        public ActionResult captainHtmlValue()
        {
            var captain = Request.Form.Get("type");
            var office = Request.Form.Get("base");
            var data = new
            {
                captainValue = captain, //返回json
                officeValue = office
            };
            return Json(data);
        }


        //获取唯一码中的数字部分，主要用于排序
        public string getUniNum(string uni)
        {
            string uniNum_string = uni.Substring(uni.IndexOf("-") + 1, uni.Length - uni.IndexOf("-") - 1);
            return uniNum_string;
        }

        //验证是否已经上班
        public ActionResult validateIsAttend()
        {
            bool success = true;
            string date = Request["date"].ToString();
            string uniquenum = Request["uniquenum"].ToString();
            if ((!string.IsNullOrEmpty(date)) && (!string.IsNullOrEmpty(uniquenum)))
            {
                Guid humanId=(from d in dal.HumanBasicFile where d.IsDelete!=true&&d.IsDraft!=true&&d.uniNum==uniquenum select d.Id).FirstOrDefault();
                AttendingInfo at = (from d in dal.AttendingInfo where d.IsDraft != true && d.Date == date && d.HumanId == humanId select d).FirstOrDefault();
                if (at != null)
                {
                    success = false;
                }
            }
            return Json(success);
        }
    }
}
