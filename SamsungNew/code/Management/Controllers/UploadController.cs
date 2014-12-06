using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Management.Models;
using System.Data.Entity;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Transactions;
using System.Data.OleDb;

namespace Management.Controllers
{
    public class UploadController : Controller
    {
        SS_HRM_DBEntities db = new SS_HRM_DBEntities();
        //
        // GET: /Upload/

        #region 上传功能

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        //开始上传
        [HttpPost]
        public ActionResult Index(int abc = 1)
        {
            string username = Session["admin"].ToString();
            Managers ms = (from d in db.Managers where d.UserId == username select d).FirstOrDefault();
            string creator = ms.UserId;//录入人员
            HttpPostedFileBase file = Request.Files["wenjian"];
            string FileName;
            string savePath;
            string errormes = "";
            if (file == null || file.ContentLength <= 0)
            {
                errormes += "文件不能为空" + "</Br></Br>";
                ViewBag.error = errormes;
                return View();
            }

            string filename = Path.GetFileName(file.FileName);
            int filesize = file.ContentLength;//获取上传文件的大小单位为字节byte
            string fileEx = System.IO.Path.GetExtension(filename);//获取上传文件的扩展名
            string NoFileName = System.IO.Path.GetFileNameWithoutExtension(filename);//获取无扩展名的文件名
            string FileType = ".xls,.xlsx";//定义上传文件的类型字符串

            FileName = NoFileName + DateTime.Now.ToString("yyyyMMdd") + fileEx;
            if (!FileType.Contains(fileEx))
            {
                errormes += "文件类型不对，只能导入xls和xlsx格式的文件" + "</Br></Br>";
                ViewBag.error = errormes;
                return View();
            }
            string path = Server.MapPath("~/UploadExcel/");
            savePath = Path.Combine(path, FileName);
            file.SaveAs(savePath);
            string filename1 = "错误文本" + ".txt";
            if (System.IO.File.Exists(path + filename1))
            {
                System.IO.File.Delete(path + filename1);
            }
            string strConn;
            strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + savePath + ";" + "Extended Properties='Excel 8.0;;HDR=YES;IMEX=1;'";
            if (fileEx == ".xlsx")
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + savePath + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;'";
            }
            OleDbConnection conn = new OleDbConnection(strConn);
            DataSet myDataSet = new DataSet();

            try
            {
                conn.Open();
                DataTable sheetNames = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string tablename = sheetNames.Rows[0][2].ToString();//获取表名
                string cmd = "select * from [" + tablename + "]";
                OleDbDataAdapter myCommand = new OleDbDataAdapter(cmd, strConn);
                myCommand.Fill(myDataSet, "ExcelInfo");
            }
            catch (Exception ex)
            {
                errormes += ex.Message;
                System.IO.File.Delete(savePath);//删掉导入的excel
                ViewBag.error = errormes;
                return View();
            }
            finally
            {
                conn.Close();
            }
            DataTable table = myDataSet.Tables["ExcelInfo"].DefaultView.ToTable();
            if (table.Columns.Count != 17)
            {
                errormes += "请按照模板格式进行上传数据" + "</Br></Br>";
                ViewBag.error = errormes;
                System.IO.File.Delete(savePath);//删掉导入的excel
                return View();
            }

            //引用事务机制，出错时，事物回滚
            using (TransactionScope transaction = new TransactionScope())
            {

                for (int i = 0; i < table.Rows.Count; i++)
                {

                    //获取地区名称
                    string _areaName = "";
                    if (table.Rows[i][0] != null)
                    {
                        _areaName = table.Rows[i][0].ToString();
                    }
                    //获取姓名信息
                    string name = "";
                    if (table.Rows[i][1] != null)
                    {
                        name = table.Rows[i][1].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,姓名为空，请填写"  +Environment.NewLine;
                    }
                    //获取籍贯信息
                    string nativeplace = "";
                    if (table.Rows[i][2] != null)
                    {
                        nativeplace = table.Rows[i][2].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,籍贯为空，请填写" + Environment.NewLine;
                    }
                    //获取性别
                    string sex = "";
                    if (table.Rows[i][3] != null)
                    {
                        sex = table.Rows[i][3].ToString();
                        if (sex != "男" && sex != "女")
                        {
                            errormes += "第" + (i + 2) + "行,性别只能为男或女" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,性别为空，请填写" + Environment.NewLine;
                    }
                    //获取身份证信息
                    string idcard = "";
                    if (table.Rows[i][4] != null)
                    {
                        idcard = table.Rows[i][4].ToString();
                        if (validateIdCart(idcard))
                        {
                            errormes += "第" + (i + 2) + "行,身份证已存在于数据库，不能重复录入" + Environment.NewLine;
                        }
                        if (idcard.Length != 18)
                        {
                            errormes += "第" + (i + 2) + "行,身份证必须18位" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,身份证为空，请填写" + Environment.NewLine;
                    }
                    //获取电话信息
                    string telephone = "";
                    if (table.Rows[i][5] != null)
                    {
                        telephone = table.Rows[i][5].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,电话为空，请填写" + Environment.NewLine;
                    }
                    //获取银行信息
                    int bankid = 0;
                    if (table.Rows[i][6] != null)
                    {
                        string bankname = table.Rows[i][6].ToString();
                        bankid = validateBankName(bankname);
                        if (bankid == 0)
                        {
                            errormes += "第" + (i + 2) + "行," + bankname + "银行不存在，请先添加该银行" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,开户行为空，请填写" + Environment.NewLine;
                    }
                    //获取银行卡号信息
                    string banknum = "";
                    if (table.Rows[i][7] != null)
                    {
                        banknum = table.Rows[i][7].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,银行卡号为空，请填写" + Environment.NewLine;
                    }

                    //获取学校信息
                    string school = "";
                    if (table.Rows[i][8] != null)
                    {
                        school = table.Rows[i][8].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,学校为空，请填写" + Environment.NewLine;
                    }
                    //获取专业信息
                    string major = "";
                    if (table.Rows[i][9] != null)
                    {
                        major = table.Rows[i][9].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,专业为空，请填写" + Environment.NewLine;
                    }

                    //获取毕业时间
                    string gratetime = "";
                    if (table.Rows[i][10] != null)
                    {
                        gratetime = table.Rows[i][10].ToString();
                        try
                        {
                            DateTime dt = DateTime.Parse(gratetime);
                            gratetime = dt.Year.ToString() + "年" + dt.Month.ToString() + "月";
                            if (dt.Month.ToString().Length < 2)
                            {
                                gratetime = dt.Year.ToString() + "年" + "0" + dt.Month.ToString() + "月";
                            }
                        }
                        catch (Exception ex)
                        {
                            errormes += "第" + (i + 2) + "行,毕业时间格式错误，请按（格式：XXXX年XX月）填写" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,毕业时间为空，请填写" + Environment.NewLine;
                    }
                    //获取学历
                    string academic = "";
                    if (table.Rows[i][11] != null)
                    {
                        academic = table.Rows[i][11].ToString();
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,学历为空，请填写" + Environment.NewLine;
                    }
                    //获取身高
                    int height = 0;
                    if (table.Rows[i][12] != null)
                    {
                        string heights = table.Rows[i][12].ToString();
                        if (heights != "")
                        {
                            if (!int.TryParse(heights, out height))
                            {
                                errormes += "第" + (i + 2) + "行," + heights + "身高格式不对，请填写整数（单位：厘米）可不填" + Environment.NewLine;
                            }
                        }
                    }
                    //获取体重
                    int weight = 0;
                    if (table.Rows[i][13] != null)
                    {
                        string weights = table.Rows[i][13].ToString();
                        if (weights != "")
                        {
                            if (!int.TryParse(weights, out weight))
                            {

                                errormes += "第" + (i + 2) + "行," + weights + "体重格式不对，请填写整数（单位：kg）可不填" + Environment.NewLine;

                            }
                        }
                    }
                    //获取特长
                    string specicallity = "";
                    if (table.Rows[i][14] != null)
                    {
                        specicallity = table.Rows[i][14].ToString();
                    }

                    //获取三围
                    string BWH = "";
                    if (table.Rows[i][15] != null)
                    {
                        BWH = table.Rows[i][15].ToString();
                        //Regex x = new Regex("^[1-9][0-9]{1,2}[,][1-9][0-9]{0,2}[,][1-9][0-9]{0,2}$");
                        //if (!x.IsMatch(BWH))
                        //{
                        //    errormes += "第" + (i + 2) + "行," + BWH + "三围格式不对，请规范输入三围，格式:66,44,66 可不填" + Environment.NewLine;
                        //}
                    }
                    //获取上级
                    string bossstr = "";
                    Guid boss=new Guid();
                    if (table.Rows[i][16] != null)
                    {
                        bossstr = table.Rows[i][16].ToString();
                        Managers ms1 = validateBoss(bossstr);
                        if (ms1 == null)
                        {
                            errormes += "第" + (i + 2) + "行," + bossstr + "上级用户名不存在，请核对" + Environment.NewLine;
                        }
                        else
                        {
                            boss = ms1.Id;
                            if (ms1.Authority == 2)
                            {
                                Managers mss = (from d in db.Managers where d.UserId == ms.Boss select d).FirstOrDefault();
                                boss = mss.Id;
                                bossstr = mss.UserId;
                            }

                        }
                    }
                    else
                    {
                        errormes += "第" + (i + 2) + "行,上级为空，请填写" + Environment.NewLine;
                    }
                    //判断地区是否存在
                    int cityid = validateCityName(_areaName, bossstr);
                    if (cityid == 0 && bossstr!=null)
                    {
                        errormes += "第" + (i + 2) + "行," + _areaName + "城市不存在，请先添加该地区" + Environment.NewLine;
                    }
                    if (errormes == "")
                    {
                        HumanBasicFile station = new HumanBasicFile();
                        station.Id = Guid.NewGuid();
                        station.city = cityid;
                        station.Name = name;
                        station.NativePlace = nativeplace;
                        if (sex == "男")
                        {
                            station.Sex = true;
                        }
                        else { station.Sex = false; }
                        station.IDcardNo = idcard;
                        station.BankId = bankid;
                        station.BankNum = banknum;
                        station.Telephone = telephone;
                        station.School = school;
                        station.Major = major;
                        station.GraduateTime = gratetime;
                        station.Academic = academic;
                        if (height != 0)
                        {
                            station.Height = height;
                        }
                        if (weight != 0)
                        {
                            station.Weight = weight;
                        }
                        station.speciality = specicallity;
                        station.BWH = BWH;
                        station.HumanLevel = "B";
                        station.IsDraft = false;
                        station.LevelEditTimes = 0;
                        station.TrainTimes = 0;
                        station.Boss = boss;
                        station.CreatedManagerID = creator;
                        station.EditManagerId = creator;
                        station.uniNum = Getunique(boss);
                        station.Remark = "资料不全";
                        station.createTime = DateTime.Now;
                        db.HumanBasicFile.Add(station);
                        db.SaveChanges();

                    }
                    if ((i == table.Rows.Count - 1) && errormes != "")
                    {
                        System.IO.File.AppendAllText(path+filename1,errormes);
                        System.IO.File.Delete(savePath);//删掉导入的excel
                        return File(new FileStream(path + filename1, FileMode.Open), "text/plain", filename1);
                    }
                }
                transaction.Complete();
            }
            System.Threading.Thread.Sleep(2000);
            ViewBag.flag = 1;
            System.IO.File.Delete(savePath);//删掉导入的excel
            return View();
        }

        //验证上传文件后缀名 只能够是xls和xlsx
        public ActionResult validateExt()
        {
            bool success = false;
            string[] extend = { "xls", "xlsx" };
            string filename = Request["cityid"];
            string extendName = filename.Remove(0, filename.LastIndexOf(".") + 1);
            foreach (var item in extend)
            {
                if (extendName.ToLower() == item)
                {
                    success = true;
                    break;
                }

            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        //验证城市名是否存在
        public int validateCityName(string cityname, string bossstr)
        {
            int cityid = 0;
            string username = bossstr;
            Managers ms = (from d in db.Managers where d.UserId == username select d).FirstOrDefault();
            if (ms != null)
            {
                string officename = ms.City1.Office.Name;
                var office = (from o in db.Office.ToList() where o.Name == officename select o).FirstOrDefault();
                var cityList = (from city in db.City.ToList() select city).ToList();
                cityList = cityList.Where(a => a.OfficeId == office.Id).ToList();
                foreach (var name in cityList)
                {
                    if (name.Name == cityname)
                    {
                        cityid = name.Id;
                        break;
                    }
                }
            }
            return cityid;
        }

        //验证银行是否存在
        public int validateBankName(string bankyname)
        {
            int bankid = 0;
            Bank bk = (from d in db.Bank where d.Name == bankyname select d).FirstOrDefault();
            if (bk != null)
            {
                bankid = bk.Id;
            }
            return bankid;
        }

        //验证上级是否存在
        public Managers validateBoss(string boss)
        {
            Managers ms1 = (from d in db.Managers where d.UserId == boss select d).FirstOrDefault();
            return ms1 == null ? null : ms1;
        }

        //验证身份证是否已经存在
        public bool validateIdCart(string id)
        {
            HumanBasicFile human = (from d in db.HumanBasicFile where d.IDcardNo == id&&(d.IsDelete==false||d.IsDelete==null)&&d.IsDraft==false select d).FirstOrDefault();
            if (human != null)
            {
                return true;
            }
            return false;
        }

        //产生唯一号
        public string Getunique(Guid boss)
        {
            Managers ms = (from d in db.Managers where d.Id == boss select d).FirstOrDefault();
            string lastunique = (from d in db.HumanBasicFile where d.Boss == boss orderby d.uniNum descending select d.uniNum).FirstOrDefault();
            if (!string.IsNullOrEmpty(lastunique))
            {
                string lastuni = lastunique.Substring(3);
                int newlastuni = Convert.ToInt32(lastuni) + 1;
                string newquniquestr = "000" + newlastuni.ToString();
                if (newlastuni >= 10 && newlastuni < 100)
                {
                    newquniquestr = "00" + newlastuni.ToString();
                }
                else if (newlastuni >= 100 && newlastuni < 1000)
                {
                    newquniquestr = "0" + newlastuni.ToString();
                }
                else if (newlastuni >= 1000)
                {
                    newquniquestr = newlastuni.ToString();
                }

                string newunique = lastunique.Substring(0, 3) + newquniquestr;
                return newunique;

            }
            string office = ms.City1.Office.mask;
            return office + "-0000";

        }

        #endregion

        #region 下载功能
        //支持大文件下载
        public FileStreamResult StreamFileFromDisk()
        {
            string path = Server.MapPath("~/UploadExcel/");
            string filename = "Parttime_template.xls";
            return File(new FileStream(path + filename, FileMode.Open), "text/plain", filename);
        }

        #endregion
    }
}
