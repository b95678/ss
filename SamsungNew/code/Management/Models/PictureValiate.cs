using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Management.Models
{
    public class PictureValiate
    {

        #region 图片验证
        //检验图片格式
        public static bool PicExtend(string picName)
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
        public  static String newName(String name, String type)
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
    }
}