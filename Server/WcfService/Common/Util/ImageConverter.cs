using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.IO.Compression;
using System.Drawing.Imaging;

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
            return HexConverter(color);
        }

        private static String HexConverter(Color c)
        {
            //RRGGBB  
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"); //"#" 
        }

        public static byte[] ByteFromBase64(string base64String)
        {
            // Convert the Base64 UUEncoded input into binary output.
            try
            {
                return Convert.FromBase64String(base64String);
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

        public static string BytesToBase64(byte[] imageBytes, int width, int height)
        {
            return BytesToBase64(imageBytes, width, height, DefaultImgFormat);
        }

        public static string BytesToBase64(byte[] imageBytes, int width, int height, ImageFormat imgFormat)
        {
            //now check if we need to change the size and format
            if (width <= 0 || height <= 0)
            {
                return Convert.ToBase64String(imageBytes); //base64String  
            }

            //must be PNG or JPEG format for silverlight so I make sure it happens here.  Imaging.ImageFormat.Png
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // resize and reformat the image
                //note. this does not allow us to get sized over the Image size
                Image resizedImage = FixedSize(ByteToImage(imageBytes), width, height, false);

                using (MemoryStream msFormatted = new MemoryStream())
                {
                    resizedImage.Save(msFormatted, imgFormat);
                    resizedImage.Dispose();
                    return Convert.ToBase64String(msFormatted.ToArray()); //base64String
                }
            }
        }

        public static Image ByteToImage(byte[] imageBytes, int width, int height, ImageFormat imgFormat)
        {
            if (width < 0 || height < 0)
            {
                return null;
            }
            //must be PNG or JPEG format for silverlight so I make sure it happens here.  Imaging.ImageFormat.Png
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                //note. this does not allow us to get sized over the Image size
                Image resizedImage = FixedSize(ByteToImage(imageBytes), width, height, false);
                using (MemoryStream msFormatted = new MemoryStream())
                {
                    resizedImage.Save(msFormatted, imgFormat);
                    //resizedImage.Dispose();
                    //image.Dispose();
                    return resizedImage;
                }
            }
        }

        public static Image ByteToImage(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap bmp = new Bitmap(image);
                bmp.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                // Convert byte[] to Base64 String
                return Convert.ToBase64String(imageBytes);//base64String
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

        public static Image ImageFromBase64(string base64String)
        {
            // Convert the Base64 UUEncoded input into binary output.
            try
            {
                byte[] binaryData = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(binaryData))
                {
                    Image image = Image.FromStream(ms);
                    return image;
                }
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

        public static Stream StreamFromBase64(string base64String)
        {
            // Convert the Base64 UUEncoded input into stream output.
            try
            {
                byte[] binaryData = Convert.FromBase64String(base64String);
                return new MemoryStream(binaryData);
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

        public static Image FixedSize(Image imgPhoto, int Width, int Height, bool needToFill)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (needToFill == false)
            {
                if (nPercentH < nPercentW)
                {
                    nPercent = nPercentH;
                }
                else
                {
                    nPercent = nPercentW;
                }
            }
            else
            {
                if (nPercentH > nPercentW)
                {
                    nPercent = nPercentH;
                    destX = (int)Math.Round((Width -
                        (sourceWidth * nPercent)) / 2);
                }
                else
                {
                    nPercent = nPercentW;
                    destY = (int)Math.Round((Height -
                        (sourceHeight * nPercent)) / 2);
                }
            }

            if (nPercent > 1)
                nPercent = 1;

            int destWidth = (int)Math.Round(sourceWidth * nPercent);
            int destHeight = (int)Math.Round(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(
                destWidth <= Width ? destWidth : Width,
                destHeight < Height ? destHeight : Height, PixelFormat.Format32bppRgb);
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

        public static void GetImageFromFile(string fullPathImageFileName, bool useInWebPages, ref string imageBase64)
        {
            try
            {
                // useInWebPages is only for web pages where <img src="" is used

                if (string.IsNullOrWhiteSpace(fullPathImageFileName))
                    fullPathImageFileName = "";

                imageBase64 = "";
                string imageStringFormat = "data:image/png;base64,"; //"data:image/png;base64,"  for the <img src="" />
                ImageFormat imgFormat = ImageFormat.Png;

                //Need to know the image format
                fullPathImageFileName = fullPathImageFileName.ToLower();
                if (fullPathImageFileName.Contains(".png"))
                {
                    imgFormat = ImageFormat.Png;
                    imageStringFormat = "data:image/png;base64,";
                }
                else if (fullPathImageFileName.Contains(".jpg") || fullPathImageFileName.Contains(".jpeg"))
                {
                    imgFormat = ImageFormat.Jpeg;
                    imageStringFormat = "data:image/jpg;base64,";
                }
                else if (fullPathImageFileName.Contains(".gif"))
                {
                    imgFormat = ImageFormat.Gif;
                    imageStringFormat = "data:image/gif;base64,";
                }

                //if file is not found, then return empty string (?)
                if (File.Exists(fullPathImageFileName))
                {
                    Image theImgage = Image.FromFile(fullPathImageFileName);
                    if (useInWebPages)
                        imageBase64 = imageStringFormat + ImageToBase64(theImgage, imgFormat);
                    else
                        imageBase64 = ImageToBase64(theImgage, imgFormat);
                }
                else
                {
                    //if not an image then return a spacer.gif
                    if (useInWebPages)
                        imageBase64 = "data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAALSURBVBhXY2AAAgAABQABqtXIUQAAAABJRU5ErkJggg==";
                    else
                    {
                        //1x1-pixel.png
                        imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAALSURBVBhXY2AAAgAABQABqtXIUQAAAABJRU5ErkJggg==";
                    }
                }

                /*  NO GIF PLEASE
                 * spacer.gif =  "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="
                string imageBase64 = @"data:image/gif;base64,R0lGODlhUAAPAKIAAAsLav///88PD9WqsYmApmZmZtZfYmdakyH5BAQUAP8ALAAAAABQAA8AAAPb
                                        WLrc/jDKSVe4OOvNu/9gqARDSRBHegyGMahqO4R0bQcjIQ8E4BMCQc930JluyGRmdAAcdiigMLVr
                                        ApTYWy5FKM1IQe+Mp+L4rphz+qIOBAUYeCY4p2tGrJZeH9y79mZsawFoaIRxF3JyiYxuHiMGb5KT
                                        kpFvZj4ZbYeCiXaOiKBwnxh4fnt9e3ktgZyHhrChinONs3cFAShFF2JhvCZlG5uchYNun5eedRxM
                                        AF15XEFRXgZWWdciuM8GCmdSQ84lLQfY5R14wDB5Lyon4ubwS7jx9NcV9/j5+g4JADs=";
                */

            }
            catch (Exception ex)
            {
                throw (ex);
            }
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

        public static ImageFormat GetImageFormat(this Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
                return ImageFormat.Jpeg;
            if (img.RawFormat.Equals(ImageFormat.Bmp))
                return ImageFormat.Bmp;
            if (img.RawFormat.Equals(ImageFormat.Png))
                return ImageFormat.Png;
            if (img.RawFormat.Equals(ImageFormat.Emf))
                return ImageFormat.Emf;
            if (img.RawFormat.Equals(ImageFormat.Exif))
                return ImageFormat.Exif;
            if (img.RawFormat.Equals(ImageFormat.Gif))
                return ImageFormat.Gif;
            if (img.RawFormat.Equals(ImageFormat.Icon))
                return ImageFormat.Icon;
            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ImageFormat.MemoryBmp;
            if (img.RawFormat.Equals(ImageFormat.Tiff))
                return ImageFormat.Tiff;
            else
                return ImageFormat.Wmf;
        }

        public static void SaveImageToFile(string fullPathImageFileName, string base64String, int width, int height, ImageFormat format)
        {
            try
            {
                byte[] imgBuffer = ByteFromBase64(base64String);
                Image imgToSave = ByteToImage(imgBuffer, width, height, format);
                imgToSave.Save(fullPathImageFileName, format);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void SaveImageToFile(string fullPathImageFileName, ref byte[] imgBuffer, int width, int height, ImageFormat format)
        {
            try
            {
                Image imgToSave = ByteToImage(imgBuffer, width, height, format);
                imgToSave.Save(fullPathImageFileName, format);
            }
            catch (Exception ex)
            {
                throw (ex);
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

        public static void DeleteFile(string fullPathImageFileName)
        {
            try
            {
                if (File.Exists(fullPathImageFileName))
                {
                    File.Delete(fullPathImageFileName);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void DeleteFilesOlderThan(string fullPath, int filesOlderThanInDays = 90)
        {
            //deletes all files older than "filesOlderThanInDays"  
            try
            {
                if (Directory.Exists(fullPath))
                {
                    DirectoryInfo source = new DirectoryInfo(fullPath);
                    // Get info of each file into the directory
                    foreach (FileInfo fi in source.GetFiles())
                    {
                        if (fi.CreationTime < (DateTime.Now - new TimeSpan(filesOlderThanInDays, 0, 0, 0)))
                        {
                            fi.Delete();
                        }
                    }
                }
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
