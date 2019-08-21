using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.IO.Compression;
using System.Drawing.Imaging;

using LSRetail.Omni.Domain.DataModel.Base.Retail;

// http://code.google.com/p/imagelibrary/  MIT license

namespace LSOmni.Common.Util
{
    public static class ImageConverter
    {
        public static ImageFormat DefaultImgFormat = ImageFormat.Jpeg;

        public static string CalculateAverageColor(Image image)
        {
            //this gives same results as website http://www.wisegeek.com/how-can-i-find-the-average-color-in-a-photograph.htm
            Bitmap bmp = new Bitmap(image);
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y); //slow but OK

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;
                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;
            Color color = Color.FromArgb(r, g, b);

            //RRGGBB  
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"); 
        }

        public static string BytesToBase64(byte[] imageBytes, ImageSize size, ImageFormat imgFormat)
        {
            //now check if we need to change the size and format
            if (size.Width <= 0 || size.Height <= 0)
            {
                return Convert.ToBase64String(imageBytes); //base64String  
            }

            // resize and reformat the image
            //note. this does not allow us to get sized over the Image size
            Image resizedImage = FixedSize(ByteToImage(imageBytes), size);

            using (MemoryStream msFormatted = new MemoryStream())
            {
                resizedImage.Save(msFormatted, imgFormat);
                resizedImage.Dispose();
                return Convert.ToBase64String(msFormatted.ToArray()); //base64String
            }
        }

        public static Image ByteToImage(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        public static byte[] ImageToByte(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap img = new Bitmap(image);
                img.Save(ms, image.RawFormat);
                img.Dispose();
                // Convert Image to byte[]
                return ms.ToArray();
            }
        }

        public static Stream StreamFromBase64(string base64String)
        {
            // Convert the Base64 UUEncoded input into stream output.
            try
            {
                return new MemoryStream(Convert.FromBase64String(base64String));
            }
            catch (ArgumentNullException aex)
            {
                throw new ApplicationException("Base 64 string is null.", aex);
            }
            catch (FormatException fex)
            {
                throw new ApplicationException("Base 64 string length is not 4 or is not an even multiple of 4.", fex);
            }

        }

        public static Image FixedSize(Image imgPhoto, ImageSize size)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercentW = size.Width / (float)sourceWidth;
            float nPercentH = size.Height / (float)sourceHeight;
            float nPercent = (size.UseMinHorVerSize) ? (nPercentH > nPercentW) ? nPercentH : nPercentW : (nPercentH < nPercentW) ? nPercentH : nPercentW;

            if (nPercent > 1)
                nPercent = 1;

            int destWidth = (int)Math.Round(sourceWidth * nPercent);
            int destHeight = (int)Math.Round(sourceHeight * nPercent);

            Bitmap bmPhoto;
            if (size.UseMinHorVerSize)
                bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppRgb);
            else
                bmPhoto = new Bitmap(
                    destWidth <= size.Width ? destWidth : size.Width,
                    destHeight < size.Height ? destHeight : size.Height,
                    PixelFormat.Format32bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White); //need this for PNG that has no transparent
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            grPhoto.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            grPhoto.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static byte[] GetImageFromFile(string fullPathImageFileName)
        {
            try
            {
                // useInWebPages is only for web pages where <img src="" is used
                if (string.IsNullOrWhiteSpace(fullPathImageFileName))
                    fullPathImageFileName = "";

                ImageFormat imgFormat = ImageFormat.Png;

                //Need to know the image format
                fullPathImageFileName = fullPathImageFileName.ToLower();

                //if file is not found, then return empty string (?)
                if (File.Exists(fullPathImageFileName))
                {
                    Image theImgage = Image.FromFile(fullPathImageFileName);
                    return ImageToByte(theImgage);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static byte[] NAVUnCompressImage(byte[] image)
        {
            if (image.Length < 4)
                return image;

            byte[] cmp = { 0x02, 0x45, 0x7d, 0x5b };  // Compress bytes in begining of nav image
            byte[] buf = new byte[4];
            Buffer.BlockCopy(image, 0, buf, 0, 4);
            if (StructuralComparisons.StructuralEqualityComparer.Equals(cmp, buf) == false)
                return image;

            cmp = new byte[image.Length];
            Buffer.BlockCopy(image, 4, cmp, 0, image.Length - 4);   // get real image from buffer
            using (DeflateStream def = new DeflateStream(new MemoryStream(cmp), CompressionMode.Decompress))
            {
                using (MemoryStream dstr = new MemoryStream())
                {
                    def.CopyTo(dstr);
                    return dstr.ToArray();
                }
            }
        }

        public static bool FileExists(string fullPathImageFileName)
        {
            try
            {
                return File.Exists(fullPathImageFileName);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static bool DirectoryExists(string fullPath)
        {
            try
            {
                //if Directory is not found 
                return Directory.Exists(fullPath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void DirectoryCreate(string fullPath)
        {
            try
            {
                if (Directory.Exists(fullPath) == false)
                    Directory.CreateDirectory(fullPath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
