using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Management.Models
{
    public class Suo
    {
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

    }
}