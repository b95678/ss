using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Management.Models
{
    public class EncryptbyMD5
    {
        //public static string MD5(string Sourcein)
        //{
        //    MD5CryptoServiceProvider MD5CSP = new MD5CryptoServiceProvider();
        //    byte[] MD5Source = System.Text.Encoding.UTF8.GetBytes(Sourcein);
        //    byte[] MD5Out = MD5CSP.ComputeHash(MD5Source);
        //    return Convert.ToBase64String(MD5Out);

        //}
        public static  string GetMD5(string strSource)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strSource, "MD5").ToLower();


        }
    }
}