using System;
using System.Linq;
using System.Text;
using System.Reflection;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;
using System.Drawing.Imaging;

namespace LSOmni.BLL
{
    public abstract class BaseBLL
    {
        private static Assembly dalAssembly = null;
        private static Assembly boAssembly = null;
        protected BOConfiguration config = null;

        protected static LSLogger logger = new LSLogger();

        public virtual string DeviceId { get; set; }    //DeviceId is used everywhere...

        public BaseBLL(BOConfiguration config)
        {
            this.config = config;
        }

        #region protected

        protected T GetDbRepository<T>(BOConfiguration config)
        {
            try
            {
                //TODO read from app.config
                if (dalAssembly == null)
                {
                    string asm = GetDalAssemblyName();

                    string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    appPath = appPath.Replace("file:\\", "");
                    asm = appPath + "\\" + asm;
                    dalAssembly = Assembly.LoadFrom(asm);
                }

                //OK to use type to create instance when only one of that type is in the assembly
                Type myType = dalAssembly.GetTypes().Where(typeof(T).IsAssignableFrom).FirstOrDefault();
                T instance = (T)Activator.CreateInstance(myType, config);

                string cls = myType.FullName;
                if (instance == null)
                    throw new ApplicationException("dalAssembly.CreateInstance() failed for settings: " + cls);

                return instance;
            }
            catch (ReflectionTypeLoadException ex)
            {
                //catch those failed to load some dll..
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (string.IsNullOrEmpty(exFileNotFound.FusionLog) == false)
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                throw new ApplicationException(sb.ToString(), ex);
            }
        }

        private string GetDalAssemblyName()
        {
            string key = "Infrastructure.Dal.AssemblyName"; //key in app.settings
            if (ConfigSetting.KeyExists(key))
                return ConfigSetting.GetString(key);
            else
                return "LSOmni.DataAccess.Dal.dll"; //just in case the key is missing in app.settings file
        }

        protected T GetBORepository<T>(string key, bool json)
        {
            try
            {
                //TODO read from app.config
                if (boAssembly == null)
                {
                    string asm = GetBOAssemblyName();

                    string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    appPath = appPath.Replace("file:\\", "");
                    asm = appPath + "\\" + asm;
                    boAssembly = Assembly.LoadFrom(asm);
                }
                if (config == null)
                {
                    ConfigBLL bll = new ConfigBLL();
                    if (!bll.ConfigKeyExists(ConfigKey.LSKey, key))
                    {
                        string msg = string.Format("SecurityToken:{0} is invalid.", key);
                        throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, msg);
                    }
                    config = bll.ConfigGet(key);
                    config.IsJson = json;
                }
                //OK to use type to create instance when only one of that type is in the assembly
                Type myType = boAssembly.GetTypes().Where(typeof(T).IsAssignableFrom).FirstOrDefault();
                T instance = (T)Activator.CreateInstance(myType, config);
                //T instance = (T)boAssembly.CreateInstance(myType.FullName, true);

                string cls = myType.FullName;

                if (instance == null)
                    throw new ApplicationException("boAssembly.CreateInstance() failed for settings: " + cls);

                return instance;
            }
            catch (ReflectionTypeLoadException ex)
            {
                //catch those failed to load some dll..
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (string.IsNullOrEmpty(exFileNotFound.FusionLog) == false)
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                throw new ApplicationException(sb.ToString(), ex);
            }
        }

        private string GetBOAssemblyName()
        {
            string key = "BOConnection.AssemblyName"; //key in app.settings
            if (ConfigSetting.KeyExists(key))
                return ConfigSetting.GetString(key);
            else
                return "LSOmni.DataAccess.BOConnection.CentalPre.dll"; //just in case the key is missing in app.settings file
        }

        protected string Base64GetFromByte(byte[] image, ImageSize imageSize, ImageFormat imgFormat)
        {
            try
            {
                return ImageConverter.BytesToBase64(image, imageSize, imgFormat);
            }
            catch (Exception ex)
            {
                //HandleExceptions(ex, "Failed to GetImageFromByte() ");  //don't throw an error but return something
                string msg = "Base64GetFromByte() failed but returning empty base64 string";
                if (image == null)
                    msg += " image bytes are null";
                else
                    msg += " image bytes length: " + image.Length;

                logger.Warn(config.LSKey.Key, ex, msg);
                return "";// GetImageFile("na.jpg");
            }
        }

        #endregion protected
    }
}
