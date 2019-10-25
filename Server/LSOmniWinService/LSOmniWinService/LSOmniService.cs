﻿using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using NLog;
using LSOmni.BLL;
using LSOmni.FireSharpServer;
using LSOmni.Common.Util;

namespace LSOmni.WinService
{
    public partial class LSOmniService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Task tPushNotificationProcess;
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
                logger.Info("OnStop tPushNotificationProcess.Id: {0} - {1}", tPushNotificationProcess.Id, tPushNotificationProcess.Status);
                logger.Info("OnStop tDbCleanUp.Id: {0} - {1}", tDbCleanUp.Id, tDbCleanUp.Status);
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

                int daysToKeepOneList = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepOneList")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepOneList") : 0;
                int daysToKeepLogs = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepLogs")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepLogs") : 30;
                int daysToKeepNotify = (ConfigSetting.KeyExists("BackgroundProcessing.DbCleanup.DaysToKeepNotifications")) ? ConfigSetting.GetInt("BackgroundProcessing.DbCleanup.DaysToKeepNotifications") : 3;

                ConfigBLL appBll = new ConfigBLL(null);
                while (dbCleanupEnabled)
                {
                    try
                    {
                        //ok, so it can run a few times between 10 and 10:15... but works fine
                        //does not handle cross days or only run on sundays etc 
                        if (DateTime.Now > timeToRun)
                        {
                            appBll.DbCleanup(daysToKeepLogs, daysToKeepNotify, daysToKeepOneList);
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
