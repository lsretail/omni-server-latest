﻿using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using NLog;
using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.FireSharpServer;
using LSOmni.Common.Util;

namespace LSOmni.WinService
{
    public partial class LSOmniService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Task tPushNotificationProcess;
        private Task tOrderProcess;
        private Task tEmailProcess;
        private Task tDbCleanUp;

        public LSOmniService()
        {
            InitializeComponent();
        }

        //
        // This is the method we manually added.
        //
        internal void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Your start logic here.
                logger.Info("OnStart started");

                //OrderProcess
                tOrderProcess = Task.Factory.StartNew(() =>
                {
                    // Were we already canceled?
                    //ct.ThrowIfCancellationRequested();
                    OrderProcess();
                },
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //EmailProcess
                tEmailProcess = Task.Factory.StartNew(() =>
                {
                    EmailProcess();
                },
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //PushNotificationProcess
                tPushNotificationProcess = Task.Factory.StartNew(() =>
                {
                    new PushNotificationProcess().Start();
                },
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //DbCleanUp
                tDbCleanUp = Task.Factory.StartNew(() =>
                {
                    DbCleanUp();
                },
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                Thread.Sleep(1000);

                logger.Info("OnStart tOrderProcess.Id: {0}  Status; {1}", tOrderProcess.Id, tOrderProcess.Status);
                logger.Info("OnStart tEmailProcess.Id: {0}  Status: {1}", tEmailProcess.Id, tEmailProcess.Status);
                logger.Info("OnStart tPushNotificationProcess.Id: {0}  Status: {1}", tPushNotificationProcess.Id, tPushNotificationProcess.Status);
                logger.Info("OnStart tDbCleanUp.Id: {0}  Status: {1}", tDbCleanUp.Id, tDbCleanUp.Status);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "OnStart failed.");
                throw ex;
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Your stop logic here.
                logger.Info("OnStop started");

                logger.Info("OnStop tOrderProcess.Id: {0}  {1}", tOrderProcess.Id, tOrderProcess.Status);
                logger.Info("OnStop tEmailProcess.Id: {0}  {1}", tEmailProcess.Id, tEmailProcess.Status);
                logger.Info("OnStop tPushNotificationProcess.Id: {0}  {1}", tPushNotificationProcess.Id, tPushNotificationProcess.Status);
                logger.Info("OnStop tDbCleanUp.Id: {0}  {1}", tDbCleanUp.Id, tDbCleanUp.Status);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "OnStop failed.");
                throw ex;
            }
        }

        private void DbCleanUp()
        {
            try
            {
                logger.Info("DbCleanUp started");

                bool dbCleanupEnabled = false;
                DateTime timeToRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 0, 0);
                ;
                string enabledKey = "BackgroundProcessing.DbCleanup.Enabled"; //key in app.settings
                string runtime = "BackgroundProcessing.DbCleanup.RunAt"; //key in app.settings

                if (ConfigSetting.KeyExists(enabledKey))
                {
                    dbCleanupEnabled = ConfigSetting.GetBoolean(enabledKey);
                    if (ConfigSetting.KeyExists(runtime))
                    {
                        string time = ConfigSetting.GetString(runtime);
                        timeToRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                                                 Convert.ToInt32(time.Substring(0, 2)), 
                                                 Convert.ToInt32(time.Substring(3, 2)),
                                                 0);
                    }
                }

                if (!dbCleanupEnabled)
                {
                    logger.Info("BackgroundProcessing.DbCleanup.Enabled = false, stopping the DbCleanUp() ");
                    return;
                }

                int daysToKeepUser = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepUserData")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepUserData") : 0;
                int daysToKeepOneList = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepOneList")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepOneList") : 0;
                int daysToKeepLogs = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepLogs")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepLogs") : 30;
                int daysToKeepQueue = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepOrderQueue")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepOrderQueue") : 30;
                int daysToKeepNotify = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepNotifications")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepNotifications") : 3;

                AppSettingsBLL appBll = new AppSettingsBLL();
                while (dbCleanupEnabled)
                {
                    try
                    {
                        //ok, so it can run a few times between 10 and 10:15... but works fine
                        //does not handle cross days or only run on sundays etc 
                        if (DateTime.Now > timeToRun)
                        {
                            appBll.DbCleanUp(daysToKeepLogs, daysToKeepQueue, daysToKeepNotify, daysToKeepUser, daysToKeepOneList);
                            timeToRun = timeToRun.AddDays(1);   // run again tomorrow
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "DbCleanUp failed. Continuing... ");
                    }
                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "DbCleanUp startup failed. ");
            }
        }

        private bool IsTimeOfDayBetween(DateTime time, TimeSpan startTime, TimeSpan endTime)
        {
            //Can run between 10 and 10:30 ... but works fine
            //does not handle cross days or only run on sundays etc 

            //bool doRunNow = IsTimeOfDayBetween(DateTime.Now, new TimeSpan(22, 0, 0), new TimeSpan(22, 30, 0));
            //not perfect but works fine
            if (endTime == startTime)
            {
                return true;
            }
            else if (endTime < startTime)
            {
                return time.TimeOfDay <= endTime ||
                    time.TimeOfDay >= startTime;
            }
            else
            {
                return time.TimeOfDay >= startTime &&
                    time.TimeOfDay <= endTime;
            }
        }

        private void OrderProcess()
        {
            try
            {
                logger.Info("OrderProcess started");

                bool orderProcessEnabled = false;
                int intervalInSeconds = 20;
                string enabledKey = "BackgroundProcessing.OrderProcess.Enabled"; //key in app.settings
                string durInSeconds = "BackgroundProcessing.OrderProcess.DurationInSeconds"; //key in app.settings

                if (ConfigSetting.KeyExists(enabledKey))
                {
                    orderProcessEnabled = ConfigSetting.GetBoolean(enabledKey);
                    if (ConfigSetting.KeyExists(durInSeconds))
                        intervalInSeconds = ConfigSetting.GetInt(durInSeconds);
                }

                string qrImageFolderName = GetQrFolderName();

                if (!orderProcessEnabled)
                {
                    logger.Info("BackgroundProcessing.OrderProcess.Enabled = false, stopping the OrderProcess() ");
                    return;
                }

                while (orderProcessEnabled)
                {
                    try
                    {
                        OrderMessageBLL bll = new OrderMessageBLL();
                        logger.Info("OrderMessageProcess executing every {0} seconds ", intervalInSeconds);
                        bll.OrderMessageProcess(qrImageFolderName);
                        bll = null;

                        Thread.Sleep(1000 * intervalInSeconds);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "The OrderMessageProcess failed. Continuing... ");
                        Thread.Sleep(1000 * intervalInSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "OrderProcess startup failed.");
            }
        }

        private void EmailProcess()
        {
            try
            {
                logger.Info("EmailProcess started");

                bool emailProcessEnabled = false;
                int intervalInSeconds = 10;
                string enabledKey = "BackgroundProcessing.EmailProcess.Enabled"; //key in app.settings
                string durInSeconds = "BackgroundProcessing.EmailProcess.DurationInSeconds"; //key in app.settings

                if (ConfigSetting.KeyExists(enabledKey))
                {
                    emailProcessEnabled = ConfigSetting.GetBoolean(enabledKey);
                    if (ConfigSetting.KeyExists(durInSeconds))
                        intervalInSeconds = ConfigSetting.GetInt(durInSeconds);
                }

                string qrImageFolderName = GetQrFolderName();

                if (!emailProcessEnabled)
                {
                    logger.Info("BackgroundProcessing.EmailProcess.Enabled = false, stopping the EmailProcess() ");
                    return;
                }
                
                while (emailProcessEnabled)
                {
                    try
                    {
                        EmailBLL emailBLL = new EmailBLL();
                        logger.Info("ProcessEmails executing every {0} seconds ", intervalInSeconds);
                        emailBLL.ProcessEmails(qrImageFolderName); //sends out the emails, can take a while
                        emailBLL = null;

                        ImageConverter.DeleteFilesOlderThan(qrImageFolderName, 20);
                        Thread.Sleep(1000 * intervalInSeconds);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "The EmailProcess failed. Continuing... ");
                        Thread.Sleep(1000 * intervalInSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "EmailProcess startup failed.");
            }
        }
        
        private string GetQrFolderName()
        {
            string qrImageFolderName = string.Empty;
            string qrImageFolderNameKey = "BackgroundProcessing.QRCode.Image.FolderName"; //key in app.settings
            if (ConfigSetting.KeyExists(qrImageFolderNameKey))
            {
                if (ConfigSetting.KeyExists(qrImageFolderNameKey))
                    qrImageFolderName = ConfigSetting.GetString(qrImageFolderNameKey);

                //nothing found, default to \Images under current exe file
                if (string.IsNullOrWhiteSpace(qrImageFolderName))
                    qrImageFolderName = string.Format(@"{0}\Images", AppDomain.CurrentDomain.BaseDirectory);
            }
            return qrImageFolderName;

        }

        #region Service
        private static bool IsServiceInstalled()
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == CustomServiceInstaller.InstallServiceName);
        }

        public static void InstallService()
        {
            if (IsServiceInstalled())
            {
                UninstallService();
            }
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        public static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
        #endregion Service
    }
}
